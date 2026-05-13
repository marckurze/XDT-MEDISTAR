using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ExportProfileDeletionService
{
    private readonly ProfileCatalogService _profileCatalogService;

    public ExportProfileDeletionService()
        : this(new ProfileCatalogService())
    {
    }

    public ExportProfileDeletionService(ProfileCatalogService profileCatalogService)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
    }

    public ExportProfileDeletionResult Evaluate(ProfileCatalog catalog, string exportProfileId)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (string.IsNullOrWhiteSpace(exportProfileId))
        {
            return ExportProfileDeletionResult.Blocked("Kein Exportprofil ausgewählt.");
        }

        var profile = catalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, exportProfileId, StringComparison.OrdinalIgnoreCase));
        if (profile is null)
        {
            return ExportProfileDeletionResult.Blocked($"Exportprofil nicht gefunden: {exportProfileId}.");
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return ExportProfileDeletionResult.Blocked("BuiltIn-Exportprofile können nicht gelöscht werden.", profile);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return ExportProfileDeletionResult.Blocked("Nur UserDefined-Exportprofile können gelöscht werden.", profile);
        }

        var references = catalog.InterfaceProfiles
            .Where(interfaceProfile => string.Equals(interfaceProfile.ExportProfileId, exportProfileId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(interfaceProfile => interfaceProfile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        if (references.Count > 0)
        {
            var referenceNames = string.Join(", ", references.Select(interfaceProfile => interfaceProfile.Metadata.Name));
            return ExportProfileDeletionResult.Blocked(
                $"Dieses Exportprofil wird noch von Schnittstellenprofilen verwendet und kann nicht gelöscht werden: {referenceNames}.",
                profile,
                references);
        }

        return new ExportProfileDeletionResult(
            Success: true,
            Message: "Exportprofil kann gelöscht werden.",
            Profile: profile,
            ReferencingInterfaceProfiles: Array.Empty<InterfaceProfileDefinition>());
    }

    public ExportProfileDeletionResult Delete(ProfileCatalog catalog, AppDataPaths paths, string exportProfileId)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var evaluation = Evaluate(catalog, exportProfileId);
        if (!evaluation.Success || evaluation.Profile is null)
        {
            return evaluation;
        }

        var deleted = _profileCatalogService.DeleteExportProfile(paths, exportProfileId);
        return deleted
            ? ExportProfileDeletionResult.Deleted(evaluation.Profile)
            : ExportProfileDeletionResult.Blocked($"Exportprofil nicht gefunden: {exportProfileId}.", evaluation.Profile);
    }
}
