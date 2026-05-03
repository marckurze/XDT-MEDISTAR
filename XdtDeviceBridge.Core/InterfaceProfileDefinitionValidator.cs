namespace XdtDeviceBridge.Core;

public static class InterfaceProfileDefinitionValidator
{
    public static IReadOnlyList<string> Validate(InterfaceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var issues = new List<string>();

        if (profile.Metadata is null)
        {
            issues.Add("Metadata must not be null.");
        }
        else if (profile.Metadata.ProfileKind != ProfileKind.InterfaceProfile)
        {
            issues.Add("Metadata.ProfileKind must be InterfaceProfile.");
        }

        if (string.IsNullOrWhiteSpace(profile.AisProfileId))
        {
            issues.Add("AisProfileId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.DeviceProfileId))
        {
            issues.Add("DeviceProfileId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.ExportProfileId))
        {
            issues.Add("ExportProfileId must not be empty.");
        }

        if (profile.FolderOptions is null)
        {
            issues.Add("FolderOptions must not be null.");
            return issues;
        }

        if (profile.IsActive)
        {
            if (string.IsNullOrWhiteSpace(profile.FolderOptions.AisImportFolder))
            {
                issues.Add("AisImportFolder must be set when profile is active.");
            }

            if (string.IsNullOrWhiteSpace(profile.FolderOptions.DeviceImportFolder))
            {
                issues.Add("DeviceImportFolder must be set when profile is active.");
            }

            if (string.IsNullOrWhiteSpace(profile.FolderOptions.ExportFolder))
            {
                issues.Add("ExportFolder must be set when profile is active.");
            }
        }

        if (profile.FolderOptions.ClearAisImportFolderBeforeProcessing
            && string.IsNullOrWhiteSpace(profile.FolderOptions.AisImportFolder))
        {
            issues.Add("AisImportFolder must be set when ClearAisImportFolderBeforeProcessing is true.");
        }

        if (profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing
            && string.IsNullOrWhiteSpace(profile.FolderOptions.DeviceImportFolder))
        {
            issues.Add("DeviceImportFolder must be set when ClearDeviceImportFolderBeforeProcessing is true.");
        }

        if (profile.FolderOptions.ClearExportFolderAfterSuccessfulTransfer
            && string.IsNullOrWhiteSpace(profile.FolderOptions.ExportFolder))
        {
            issues.Add("ExportFolder must be set when ClearExportFolderAfterSuccessfulTransfer is true.");
        }

        if (profile.FolderOptions.ArchiveProcessedFiles
            && string.IsNullOrWhiteSpace(profile.FolderOptions.ArchiveFolder))
        {
            issues.Add("ArchiveFolder must be set when ArchiveProcessedFiles is true.");
        }

        if (profile.IsActive
            && profile.FolderOptions.MoveFailedFilesToErrorFolder
            && string.IsNullOrWhiteSpace(profile.FolderOptions.ErrorFolder))
        {
            issues.Add("ErrorFolder must be set when MoveFailedFilesToErrorFolder is true.");
        }

        return issues;
    }
}
