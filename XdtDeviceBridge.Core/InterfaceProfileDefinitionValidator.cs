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

        var isManualDocumentSelection = profile.FolderOptions.IsAttachmentOnlyMode
            && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var usesSerialDevice = profile.SerialSettings is not null;

        if (profile.IsActive)
        {
            if (string.IsNullOrWhiteSpace(profile.FolderOptions.AisImportFolder))
            {
                issues.Add("AisImportFolder must be set when profile is active.");
            }

            if (!isManualDocumentSelection
                && !usesSerialDevice
                && string.IsNullOrWhiteSpace(profile.FolderOptions.DeviceImportFolder))
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

        if (!usesSerialDevice
            && profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing
            && string.IsNullOrWhiteSpace(profile.FolderOptions.DeviceImportFolder))
        {
            issues.Add("DeviceImportFolder must be set when ClearDeviceImportFolderBeforeProcessing is true.");
        }

        if (usesSerialDevice && profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing)
        {
            issues.Add("ClearDeviceImportFolderBeforeProcessing must be false for serial RS232 profiles.");
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

        if (!Enum.IsDefined(profile.FolderOptions.ArchiveProcessedFileMode))
        {
            issues.Add("ArchiveProcessedFileMode must be a valid value.");
        }

        if (!Enum.IsDefined(profile.FolderOptions.AttachmentTransferMode))
        {
            issues.Add("AttachmentTransferMode must be a valid value.");
        }

        if (!Enum.IsDefined(profile.FolderOptions.AttachmentRequirementMode))
        {
            issues.Add("AttachmentRequirementMode must be a valid value.");
        }

        if (!Enum.IsDefined(profile.FolderOptions.AttachmentOnlySourceMode))
        {
            issues.Add("AttachmentOnlySourceMode must be a valid value.");
        }

        if (profile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move
            && !profile.FolderOptions.ArchiveProcessedFiles)
        {
            issues.Add("ArchiveProcessedFiles must be true when ArchiveProcessedFileMode is Move.");
        }

        if (profile.FolderOptions.ArchiveRetentionDays < 0)
        {
            issues.Add("ArchiveRetentionDays must not be negative.");
        }

        if (profile.FolderOptions.AttachmentWaitTimeoutSeconds < 0)
        {
            issues.Add("AttachmentWaitTimeoutSeconds must not be negative.");
        }

        if (profile.FolderOptions.AttachmentFileStabilityWaitSeconds < 0)
        {
            issues.Add("AttachmentFileStabilityWaitSeconds must not be negative.");
        }

        if (profile.FolderOptions.AutoImportScanIntervalSeconds < 1)
        {
            issues.Add("AutoImportScanIntervalSeconds must be at least 1.");
        }

        if (profile.FolderOptions.DeviceFileWaitTimeoutMinutes < 0)
        {
            issues.Add("DeviceFileWaitTimeoutMinutes must not be negative.");
        }

        if (profile.SerialSettings is not null)
        {
            if (profile.IsActive && string.IsNullOrWhiteSpace(profile.SerialSettings.PortName))
            {
                issues.Add("Serial PortName must be set when serial profile is active.");
            }

            if (profile.SerialSettings.BaudRate <= 0)
            {
                issues.Add("Serial BaudRate must be greater than 0.");
            }

            if (profile.SerialSettings.DataBits is < 5 or > 8)
            {
                issues.Add("Serial DataBits must be between 5 and 8.");
            }

            if (!Enum.IsDefined(profile.SerialSettings.StopBits))
            {
                issues.Add("Serial StopBits must be a valid value.");
            }

            if (!Enum.IsDefined(profile.SerialSettings.Parity))
            {
                issues.Add("Serial Parity must be a valid value.");
            }

            if (!Enum.IsDefined(profile.SerialSettings.Handshake))
            {
                issues.Add("Serial Handshake must be a valid value.");
            }

            if (profile.SerialSettings.ReadTimeoutMilliseconds < 0)
            {
                issues.Add("Serial ReadTimeoutMilliseconds must not be negative.");
            }

            if (profile.SerialSettings.WriteTimeoutMilliseconds < 0)
            {
                issues.Add("Serial WriteTimeoutMilliseconds must not be negative.");
            }
        }

        if (profile.NidekRtSerialSendMode is { } sendMode
            && !Enum.IsDefined(sendMode))
        {
            issues.Add("NidekRtSerialSendMode must be a valid value.");
        }

        if (profile.FolderOptions.ArchiveProcessedFiles
            && profile.FolderOptions.ArchiveRetentionDays > 0
            && string.IsNullOrWhiteSpace(profile.FolderOptions.ArchiveFolder))
        {
            issues.Add("ArchiveFolder must be set when ArchiveRetentionDays is configured for archived files.");
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
