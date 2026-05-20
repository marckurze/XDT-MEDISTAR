namespace XdtDeviceBridge.Core;

public static class DefaultInterfaceProfileDefinitions
{
    public static InterfaceProfileDefinition CreateMedistarNidekArk1sDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-nidek-ark1s-default",
                Name: "MEDISTAR + NIDEK ARK1S",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and NIDEK ARK1S.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK ARK1S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-ark1s-default",
            ExportProfileId: "export-medistar-nidek-ark1s-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the validated MEDISTAR/NIDEK ARK1S profile.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekAr360Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-nidek-ar360-default",
                Name: "MEDISTAR + NIDEK AR360",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and NIDEK AR360 / AR-360A.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK AR360",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-ar360-default",
            ExportProfileId: "export-medistar-nidek-ar360-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/NIDEK AR360 profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekLm7Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-nidek-lm7-default",
                Name: "MEDISTAR + NIDEK LM7",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and NIDEK LM7 / LM-7P.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK LM7",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-lm7-default",
            ExportProfileId: "export-medistar-nidek-lm7-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/NIDEK LM7 lensmeter profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt530PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-nidek-nt530p-default",
                Name: "MEDISTAR + NIDEK NT530P",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and NIDEK NT-530P.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK NT-530P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-nt530p-default",
            ExportProfileId: "export-medistar-nidek-nt530p-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/NIDEK NT-530P tonometry and pachymetry profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarDocumentAttachmentDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 20, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-document-attachment-default",
                Name: "MEDISTAR + Dokumentanhang",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile for document-only device workflows with XDT attachment links.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/Dokumentanhang",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-document-attachment-default",
            ExportProfileId: "export-medistar-document-attachment-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true,
                IsAttachmentProcessingEnabled: true,
                AttachmentRequirementMode: AttachmentRequirementMode.Required,
                IsAttachmentOnlyMode: true,
                ShowAttachmentDocumentationDialog: true,
                AttachmentCompletionMode: AttachmentCompletionMode.WaitForQuietPeriod,
                AttachmentQuietPeriodSeconds: 10),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive V1 candidate for devices that pass documents/files to MEDISTAR as attachments without measurement parsing.");
    }
}
