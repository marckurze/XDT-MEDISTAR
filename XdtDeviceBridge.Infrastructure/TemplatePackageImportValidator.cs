using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportValidator
{
    public TemplatePackageImportValidationResult Validate(TemplatePackageImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var issues = new List<TemplatePackageImportValidationIssue>();

        if (result.Package is null)
        {
            AddError(issues, "Package must not be null.", null, ProfileKind.TemplatePackage);
        }
        else if (result.Package.Metadata is null || result.Package.Metadata.ProfileKind != ProfileKind.TemplatePackage)
        {
            AddError(
                issues,
                "Package Metadata.ProfileKind must be TemplatePackage.",
                result.Package.Metadata?.Id,
                result.Package.Metadata?.ProfileKind);
        }

        var aisProfiles = result.AisProfiles ?? Array.Empty<AisProfile>();
        var deviceProfiles = result.DeviceProfiles ?? Array.Empty<DeviceProfileDefinition>();
        var exportProfiles = result.ExportProfiles ?? Array.Empty<ExportProfileDefinition>();
        var interfaceProfiles = result.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>();

        AddDuplicateIdErrors(issues, aisProfiles.Select(profile => profile.Metadata), ProfileKind.AisProfile);
        AddDuplicateIdErrors(issues, deviceProfiles.Select(profile => profile.Metadata), ProfileKind.DeviceProfile);
        AddDuplicateIdErrors(issues, exportProfiles.Select(profile => profile.Metadata), ProfileKind.ExportProfile);
        AddDuplicateIdErrors(issues, interfaceProfiles.Select(profile => profile.Metadata), ProfileKind.InterfaceProfile);

        var aisIds = aisProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var deviceIds = deviceProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var exportIds = exportProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var exportProfile in exportProfiles)
        {
            if (!aisIds.Contains(exportProfile.TargetAisProfileId))
            {
                AddError(
                    issues,
                    $"Export profile references missing AIS profile: {exportProfile.TargetAisProfileId}",
                    exportProfile.Metadata.Id,
                    ProfileKind.ExportProfile);
            }

            if (!deviceIds.Contains(exportProfile.SourceDeviceProfileId))
            {
                AddError(
                    issues,
                    $"Export profile references missing Device profile: {exportProfile.SourceDeviceProfileId}",
                    exportProfile.Metadata.Id,
                    ProfileKind.ExportProfile);
            }
        }

        if (interfaceProfiles.Count == 0)
        {
            AddWarning(issues, "Template package does not contain any interface profiles.", null, ProfileKind.InterfaceProfile);
        }

        foreach (var interfaceProfile in interfaceProfiles)
        {
            if (!aisIds.Contains(interfaceProfile.AisProfileId))
            {
                AddError(
                    issues,
                    $"Interface profile references missing AIS profile: {interfaceProfile.AisProfileId}",
                    interfaceProfile.Metadata.Id,
                    ProfileKind.InterfaceProfile);
            }

            if (!deviceIds.Contains(interfaceProfile.DeviceProfileId))
            {
                AddError(
                    issues,
                    $"Interface profile references missing Device profile: {interfaceProfile.DeviceProfileId}",
                    interfaceProfile.Metadata.Id,
                    ProfileKind.InterfaceProfile);
            }

            if (!exportIds.Contains(interfaceProfile.ExportProfileId))
            {
                AddError(
                    issues,
                    $"Interface profile references missing Export profile: {interfaceProfile.ExportProfileId}",
                    interfaceProfile.Metadata.Id,
                    ProfileKind.InterfaceProfile);
            }

            if (interfaceProfile.IsActive)
            {
                AddMissingActiveFolderErrors(issues, interfaceProfile);
            }

            if (HasDeleteOptionEnabled(interfaceProfile.FolderOptions))
            {
                AddWarning(
                    issues,
                    "Imported delete options must be reviewed before productive activation.",
                    interfaceProfile.Metadata.Id,
                    ProfileKind.InterfaceProfile);
            }
        }

        return new TemplatePackageImportValidationResult(issues);
    }

    private static void AddDuplicateIdErrors(
        List<TemplatePackageImportValidationIssue> issues,
        IEnumerable<ProfileMetadata> metadata,
        ProfileKind profileKind)
    {
        var duplicates = metadata
            .GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateId in duplicates)
        {
            AddError(issues, $"Duplicate {profileKind} profile Id: {duplicateId}", duplicateId, profileKind);
        }
    }

    private static void AddMissingActiveFolderErrors(
        List<TemplatePackageImportValidationIssue> issues,
        InterfaceProfileDefinition interfaceProfile)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AisImportFolder))
        {
            AddError(issues, "Active interface profile requires AisImportFolder.", interfaceProfile.Metadata.Id, ProfileKind.InterfaceProfile);
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.DeviceImportFolder))
        {
            AddError(issues, "Active interface profile requires DeviceImportFolder.", interfaceProfile.Metadata.Id, ProfileKind.InterfaceProfile);
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ExportFolder))
        {
            AddError(issues, "Active interface profile requires ExportFolder.", interfaceProfile.Metadata.Id, ProfileKind.InterfaceProfile);
        }
    }

    private static bool HasDeleteOptionEnabled(InterfaceFolderOptions folderOptions)
    {
        return folderOptions.ClearAisImportFolderBeforeProcessing
            || folderOptions.ClearDeviceImportFolderBeforeProcessing
            || folderOptions.ClearExportFolderAfterSuccessfulTransfer;
    }

    private static void AddError(
        List<TemplatePackageImportValidationIssue> issues,
        string message,
        string? profileId,
        ProfileKind? profileKind)
    {
        issues.Add(new TemplatePackageImportValidationIssue(
            TemplatePackageImportValidationIssueSeverity.Error,
            message,
            profileId,
            profileKind));
    }

    private static void AddWarning(
        List<TemplatePackageImportValidationIssue> issues,
        string message,
        string? profileId,
        ProfileKind? profileKind)
    {
        issues.Add(new TemplatePackageImportValidationIssue(
            TemplatePackageImportValidationIssueSeverity.Warning,
            message,
            profileId,
            profileKind));
    }
}
