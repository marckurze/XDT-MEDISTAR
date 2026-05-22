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

    public static InterfaceProfileDefinition CreateMedistarTopconCl300Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-cl300-default",
                Name: "MEDISTAR + TOPCON CL300",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON CL-300.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CL-300",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-cl300-default",
            ExportProfileId: "export-medistar-topcon-cl300-default",
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
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON CL-300 lensmeter profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconKr800Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-kr800-default",
                Name: "MEDISTAR + TOPCON KR800S",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON KR-800S.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON KR-800S",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-kr800-default",
            ExportProfileId: "export-medistar-topcon-kr800-default",
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
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON KR-800S REF, KM and SBJ profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconTrk2PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-trk2p-default",
                Name: "MEDISTAR + TOPCON TRK2P",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON TRK-2P.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON TRK-2P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-trk2p-default",
            ExportProfileId: "export-medistar-topcon-trk2p-default",
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
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON TRK-2P REF, KM, TM, CCT and optional SBJ profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconCt1PDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-ct1p-default",
                Name: "MEDISTAR + TOPCON CT1P",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON CT-1P.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CT-1P",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-ct1p-default",
            ExportProfileId: "export-medistar-topcon-ct1p-default",
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
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON CT-1P TM and CCT profile candidate.");
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

    public static InterfaceProfileDefinition CreateMedistarManualDocumentTransferDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-manual-document-transfer-default",
                Name: "MEDISTAR + Manuelle Dokumentübergabe",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile for manual document handoff to MEDISTAR with per-file attachment links.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/Manuelle Dokumentübergabe",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-manual-document-selection-default",
            ExportProfileId: "export-medistar-manual-document-transfer-default",
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
                AttachmentTransferMode: AttachmentTransferMode.Copy,
                IsAttachmentProcessingEnabled: true,
                AttachmentRequirementMode: AttachmentRequirementMode.Required,
                IsAttachmentOnlyMode: true,
                ShowAttachmentDocumentationDialog: true,
                AttachmentCompletionMode: AttachmentCompletionMode.ManualConfirmation,
                AttachmentOnlySourceMode: AttachmentOnlySourceMode.ManualUserSelection),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive V1 candidate for workflows where users manually select documents and pass them to MEDISTAR as attachments.");
    }
}
