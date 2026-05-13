using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ExportRuleRemovalService
{
    private readonly ProfileCatalogService _profileCatalogService;

    public ExportRuleRemovalService()
        : this(new ProfileCatalogService())
    {
    }

    public ExportRuleRemovalService(ProfileCatalogService profileCatalogService)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
    }

    public ExportRuleRemovalResult Evaluate(ProfileCatalog catalog, string exportProfileId, string exportRuleId, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (string.IsNullOrWhiteSpace(exportProfileId))
        {
            return ExportRuleRemovalResult.Blocked("Kein Exportprofil ausgewählt.");
        }

        if (string.IsNullOrWhiteSpace(exportRuleId))
        {
            return ExportRuleRemovalResult.Blocked("Keine Exportregel ausgewählt.");
        }

        var profile = catalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, exportProfileId, StringComparison.OrdinalIgnoreCase));
        if (profile is null)
        {
            return ExportRuleRemovalResult.Blocked($"Exportprofil nicht gefunden: {exportProfileId}.");
        }

        if (profile.Metadata.IsBuiltIn)
        {
            return ExportRuleRemovalResult.Blocked("Exportregeln in BuiltIn-Exportprofilen können nicht entfernt werden.", profile);
        }

        if (!profile.Metadata.IsUserDefined)
        {
            return ExportRuleRemovalResult.Blocked("Exportregeln können nur aus UserDefined-Exportprofilen entfernt werden.", profile);
        }

        var removedRule = profile.Rules.FirstOrDefault(rule =>
            string.Equals(rule.Id, exportRuleId, StringComparison.OrdinalIgnoreCase));
        if (removedRule is null)
        {
            return ExportRuleRemovalResult.Blocked($"Exportregel nicht gefunden: {exportRuleId}.", profile);
        }

        var updatedProfile = profile with
        {
            Metadata = profile.Metadata with { UpdatedAt = updatedAt },
            Rules = profile.Rules
                .Where(rule => !string.Equals(rule.Id, exportRuleId, StringComparison.OrdinalIgnoreCase))
                .ToList()
        };
        var issues = updatedProfile.Validate();
        if (issues.Count > 0)
        {
            return ExportRuleRemovalResult.Blocked(
                "Exportregel kann nicht entfernt werden, weil das Exportprofil danach ungültig wäre.",
                updatedProfile,
                removedRule,
                issues);
        }

        return ExportRuleRemovalResult.Removed(updatedProfile, removedRule);
    }

    public ExportRuleRemovalResult Remove(ProfileCatalog catalog, AppDataPaths paths, string exportProfileId, string exportRuleId, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var evaluation = Evaluate(catalog, exportProfileId, exportRuleId, updatedAt);
        if (!evaluation.Success || evaluation.UpdatedProfile is null)
        {
            return evaluation;
        }

        _profileCatalogService.SaveExportProfileDefinition(paths, evaluation.UpdatedProfile, overwriteExisting: true);
        return evaluation;
    }
}
