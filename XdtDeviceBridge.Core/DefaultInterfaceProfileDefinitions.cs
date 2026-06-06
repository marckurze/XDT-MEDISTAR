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
                ClearAisImportFolderBeforeProcessing: true,
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/NIDEK AR360 profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekAr1Default()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-ar1-default",
            name: "MEDISTAR + NIDEK AR-1",
            product: "MEDISTAR/NIDEK AR-1",
            deviceProfileId: "device-nidek-ar1-default",
            exportProfileId: "export-medistar-nidek-ar1-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK AR-1 XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekAr1SDefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-ar1s-default",
            name: "MEDISTAR + NIDEK AR-1S",
            product: "MEDISTAR/NIDEK AR-1S",
            deviceProfileId: "device-nidek-ar1s-default",
            exportProfileId: "export-medistar-nidek-ar1s-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK AR-1S XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekAr310ADefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-ar310a-default",
            name: "MEDISTAR + NIDEK AR-310A",
            product: "MEDISTAR/NIDEK AR-310A",
            deviceProfileId: "device-nidek-ar310a-default",
            exportProfileId: "export-medistar-nidek-ar310a-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK AR-310A XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekArk510ADefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-ark510a-default",
            name: "MEDISTAR + NIDEK ARK-510A",
            product: "MEDISTAR/NIDEK ARK-510A",
            deviceProfileId: "device-nidek-ark510a-default",
            exportProfileId: "export-medistar-nidek-ark510a-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK ARK-510A XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekArk560ADefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-ark560a-default",
            name: "MEDISTAR + NIDEK ARK-560A",
            product: "MEDISTAR/NIDEK ARK-560A",
            deviceProfileId: "device-nidek-ark560a-default",
            exportProfileId: "export-medistar-nidek-ark560a-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK ARK-560A XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekLm1800PDefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-lm1800p-default",
            name: "MEDISTAR + NIDEK LM-1800P",
            product: "MEDISTAR/NIDEK LM-1800P",
            deviceProfileId: "device-nidek-lm1800p-default",
            exportProfileId: "export-medistar-nidek-lm1800p-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK LM-1800P XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekLm1800PdDefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-lm1800pd-default",
            name: "MEDISTAR + NIDEK LM-1800PD",
            product: "MEDISTAR/NIDEK LM-1800PD",
            deviceProfileId: "device-nidek-lm1800pd-default",
            exportProfileId: "export-medistar-nidek-lm1800pd-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK LM-1800PD XML files.");
    }

    private static InterfaceProfileDefinition CreateMedistarNidekXmlDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description)
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description);
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
                ClearAisImportFolderBeforeProcessing: true,
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/NIDEK NT-530P tonometry and pachymetry profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt1Default()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-nt1-default",
            name: "MEDISTAR + NIDEK NT-1",
            product: "MEDISTAR/NIDEK NT-1",
            deviceProfileId: "device-nidek-nt1-default",
            exportProfileId: "export-medistar-nidek-nt1-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK NT-1 XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt1EDefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-nt1e-default",
            name: "MEDISTAR + NIDEK NT-1E",
            product: "MEDISTAR/NIDEK NT-1E",
            deviceProfileId: "device-nidek-nt1e-default",
            exportProfileId: "export-medistar-nidek-nt1e-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK NT-1E XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt1PDefault()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-nt1p-default",
            name: "MEDISTAR + NIDEK NT-1P",
            product: "MEDISTAR/NIDEK NT-1P",
            deviceProfileId: "device-nidek-nt1p-default",
            exportProfileId: "export-medistar-nidek-nt1p-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK NT-1P XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt510Default()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-nt510-default",
            name: "MEDISTAR + NIDEK NT-510",
            product: "MEDISTAR/NIDEK NT-510",
            deviceProfileId: "device-nidek-nt510-default",
            exportProfileId: "export-medistar-nidek-nt510-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK NT-510 XML files.");
    }

    public static InterfaceProfileDefinition CreateMedistarNidekNt530Default()
    {
        return CreateMedistarNidekXmlDefault(
            id: "interface-medistar-nidek-nt530-default",
            name: "MEDISTAR + NIDEK NT-530",
            product: "MEDISTAR/NIDEK NT-530",
            deviceProfileId: "device-nidek-nt530-default",
            exportProfileId: "export-medistar-nidek-nt530-default",
            description: "Built-in inactive default interface definition for MEDISTAR and NIDEK NT-530 XML files.");
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON CL-300 lensmeter profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconCl300PdlDefault()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateMedistarTopconCl300Default();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "interface-medistar-topcon-cl300pdl-default",
                Name: "MEDISTAR + TOPCON CL-300PDL",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON CL-300PDL.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CL-300PDL",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            DeviceProfileId = "device-topcon-cl300pdl-default",
            ExportProfileId = "export-medistar-topcon-cl300pdl-default",
            Description = "Built-in inactive default interface definition for the MEDISTAR/TOPCON CL-300PDL lensmeter profile candidate."
        };
    }

    public static InterfaceProfileDefinition CreateMedistarTopconSolosDefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-solos-default",
                Name: "MEDISTAR + TOPCON Solos",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON SOLOS.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON SOLOS",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-solos-default",
            ExportProfileId: "export-medistar-topcon-solos-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON SOLOS lensmeter profile candidate.");
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON KR-800S REF, KM and SBJ profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconKr1Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-kr1-default",
                Name: "MEDISTAR + TOPCON KR-1",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON KR-1.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON KR-1",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-kr1-default",
            ExportProfileId: "export-medistar-topcon-kr1-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON KR-1 REF profile candidate. KM/KRT remains pending until a real fixture is available.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconRm800Default()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateMedistarTopconKr1Default();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "interface-medistar-topcon-rm800-default",
                Name: "MEDISTAR + TOPCON RM-800",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON RM-800.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON RM-800",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            DeviceProfileId = "device-topcon-rm800-default",
            ExportProfileId = "export-medistar-topcon-rm800-default",
            Description = "Built-in inactive default interface definition for the MEDISTAR/TOPCON RM-800 REF-only profile candidate."
        };
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON TRK-2P REF, KM, TM, CCT and optional SBJ profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconTrk3OmniaDefault()
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
        var profile = CreateMedistarTopconTrk2PDefault();

        return profile with
        {
            Metadata = new ProfileMetadata(
                Id: "interface-medistar-topcon-trk3-omnia-default",
                Name: "MEDISTAR + TOPCON TRK-3 Omnia",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON TRK-3 Omnia.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON TRK-3 Omnia",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            DeviceProfileId = "device-topcon-trk3-omnia-default",
            ExportProfileId = "export-medistar-topcon-trk3-omnia-default",
            Description = "Built-in inactive default interface definition for the MEDISTAR/TOPCON TRK-3 Omnia REF, KM, TM, CCT and optional SBJ profile candidate."
        };
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
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON CT-1P TM and CCT profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconCt800ADefault()
    {
        var timestamp = new DateTimeOffset(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-ct800a-default",
                Name: "MEDISTAR + TOPCON CT-800A",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for MEDISTAR and TOPCON CT-800A.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CT-800A",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-ct800a-default",
            ExportProfileId: "export-medistar-topcon-ct800a-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON CT-800A TM profile candidate.");
    }

    public static InterfaceProfileDefinition CreateMedistarTopconCv5000Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 23, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-topcon-cv5000-default",
                Name: "MEDISTAR + TOPCON CV-5000",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for the bidirectional MEDISTAR and TOPCON CV-5000 / CV-5000S phoropter workflow.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/TOPCON CV-5000",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-topcon-cv5000-default",
            ExportProfileId: "export-medistar-topcon-cv5000-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive candidate for TOPCON CV-5000/CV-5000S: AIS history can be prepared for phoropter XML import; phoropter SBJ XML returns export Prescription entirely as 6228 and Full Correction entirely as 6227.",
            DeviceOutput: new DeviceOutputConfiguration(
                IsEnabled: false,
                OutputFolder: string.Empty,
                FileNameTemplate: "CVImport.xml",
                Format: "TOPCON CV-5000 XML"));
    }

    public static InterfaceProfileDefinition CreateMedistarNidekRt6100Default()
    {
        var timestamp = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: "interface-medistar-nidek-rt6100-default",
                Name: "MEDISTAR + NIDEK RT-6100",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile definition for the bidirectional MEDISTAR and NIDEK RT-6100 LAN/MEM-200 phoropter workflow.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK RT-6100",
                Version: "1.0.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: "device-nidek-rt6100-default",
            ExportProfileId: "export-medistar-nidek-rt6100-default",
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive candidate for NIDEK RT-6100: AIS LM/AR history can be prepared for MEM-200 RT import; RT return XML exports Best entirely as 6228 and Full entirely as 6227.",
            DeviceOutput: new DeviceOutputConfiguration(
                IsEnabled: false,
                OutputFolder: string.Empty,
                FileNameTemplate: NidekRt6100InputXmlWriter.DefaultFileNameTemplate,
                Format: NidekRt6100InputXmlWriter.DeviceOutputFormat));
    }

    public static InterfaceProfileDefinition CreateMedistarNidekRt2100SerialDefault()
    {
        return CreateMedistarNidekRtSerialDefault(
            id: "interface-medistar-nidek-rt2100-serial-default",
            name: "MEDISTAR + NIDEK RT-2100 RS232",
            product: "MEDISTAR/NIDEK RT-2100 RS232",
            deviceProfileId: "device-nidek-rt2100-serial-default",
            exportProfileId: "export-medistar-nidek-rt2100-serial-default",
            description: "Built-in inactive candidate for NIDEK RT-2100 serial RS232 phoropter captures. PC-to-RT LM/AR output uses the RT reference frame shape without ID block by default.",
            frameVariant: NidekRtSerialOutputFrameVariant.ReferenceWithoutIdArAl,
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarNidekRt3100SerialDefault()
    {
        return CreateMedistarNidekRtSerialDefault(
            id: "interface-medistar-nidek-rt3100-serial-default",
            name: "MEDISTAR + NIDEK RT-3100 RS232",
            product: "MEDISTAR/NIDEK RT-3100 RS232",
            deviceProfileId: "device-nidek-rt3100-serial-default",
            exportProfileId: "export-medistar-nidek-rt3100-serial-default",
            description: "Built-in inactive candidate for NIDEK RT-3100 serial RS232 phoropter captures with Type1/Type2 communication presets prepared and RT reference frame shape as default.",
            frameVariant: NidekRtSerialOutputFrameVariant.ReferenceWithoutIdArAl,
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarNidekRt5100SerialDefault()
    {
        return CreateMedistarNidekRtSerialDefault(
            id: "interface-medistar-nidek-rt5100-serial-default",
            name: "MEDISTAR + NIDEK RT-5100 RS232",
            product: "MEDISTAR/NIDEK RT-5100 RS232",
            deviceProfileId: "device-nidek-rt5100-serial-default",
            exportProfileId: "export-medistar-nidek-rt5100-serial-default",
            description: "Built-in inactive candidate for NIDEK RT-5100 serial RS232 phoropter captures with extended data sources kept diagnostic and RT reference frame shape as default.",
            frameVariant: NidekRtSerialOutputFrameVariant.ReferenceWithoutIdArAl,
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    private static InterfaceProfileDefinition CreateMedistarNidekRtSerialDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        NidekRtSerialOutputFrameVariant frameVariant,
        DateTimeOffset timestamp)
    {
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description,
            DeviceOutput: new DeviceOutputConfiguration(
                IsEnabled: false,
                OutputFolder: string.Empty,
                FileNameTemplate: NidekRtSerialPhoropterOutputWriter.DefaultFileNameTemplate,
                Format: NidekRtSerialPhoropterOutputWriter.DeviceOutputFormat),
            SerialSettings: CreateNidekRtSerialDefaultSettings(),
            NidekRtSerialSendMode: NidekRtSerialSendMode.DirectWriterFrame,
            NidekRtSerialOutputFrameVariant: frameVariant);
    }

    private static SerialCommunicationSettings CreateNidekRtSerialDefaultSettings()
    {
        return new SerialCommunicationSettings(
            BaudRate: 2400,
            DataBits: 7,
            StopBits: SerialStopBitsSetting.Two,
            Parity: SerialParitySetting.Even,
            Handshake: SerialHandshakeSetting.None,
            DtrEnable: true,
            RtsEnable: true,
            IsBidirectional: true,
            LineTerminator: SerialLineTerminatorSetting.CR);
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponAccurefR800Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-accuref-r800-default",
            name: "MEDISTAR + Shin-Nippon Accuref R-800",
            product: "MEDISTAR/Shin-Nippon Accuref R-800",
            deviceProfileId: "device-shin-nippon-accuref-r800-default",
            exportProfileId: "export-medistar-shin-nippon-accuref-r800-default",
            description: "Built-in inactive serial profile for Shin-Nippon Accuref R-800 autorefractor text imports. COM defaults use 115200 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 115200,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponAccurefK900Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-accuref-k900-default",
            name: "MEDISTAR + Shin-Nippon Accuref K-900",
            product: "MEDISTAR/Shin-Nippon Accuref K-900",
            deviceProfileId: "device-shin-nippon-accuref-k900-default",
            exportProfileId: "export-medistar-shin-nippon-accuref-k900-default",
            description: "Built-in inactive serial profile for Shin-Nippon Accuref K-900 REF/KM text imports. COM defaults use 115200 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 115200,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponDl1000Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-dl1000-default",
            name: "MEDISTAR + Shin-Nippon DL-1000",
            product: "MEDISTAR/Shin-Nippon DL-1000",
            deviceProfileId: "device-shin-nippon-dl1000-default",
            exportProfileId: "export-medistar-shin-nippon-dl1000-default",
            description: "Built-in inactive serial profile for Shin-Nippon DL-1000 lensmeter text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponDl800Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-dl800-default",
            name: "MEDISTAR + Shin-Nippon DL-800",
            product: "MEDISTAR/Shin-Nippon DL-800",
            deviceProfileId: "device-shin-nippon-dl800-default",
            exportProfileId: "export-medistar-shin-nippon-dl800-default",
            description: "Built-in inactive serial profile for Shin-Nippon DL-800 lensmeter text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponDl900Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-dl900-default",
            name: "MEDISTAR + Shin-Nippon DL-900",
            product: "MEDISTAR/Shin-Nippon DL-900",
            deviceProfileId: "device-shin-nippon-dl900-default",
            exportProfileId: "export-medistar-shin-nippon-dl900-default",
            description: "Built-in inactive serial profile for Shin-Nippon DL-900 lensmeter text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponNct200Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-nct200-default",
            name: "MEDISTAR + Shin-Nippon NCT-200",
            product: "MEDISTAR/Shin-Nippon NCT-200",
            deviceProfileId: "device-shin-nippon-nct200-default",
            exportProfileId: "export-medistar-shin-nippon-nct200-default",
            description: "Built-in inactive serial profile for Shin-Nippon NCT-200 tonometry text imports. COM defaults use 19200 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 19200,
            readTimeoutMilliseconds: 30000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarShinNipponSlm4000Default()
    {
        return CreateMedistarShinNipponTextSerialDefault(
            id: "interface-medistar-shin-nippon-slm4000-default",
            name: "MEDISTAR + Shin-Nippon SLM-4000",
            product: "MEDISTAR/Shin-Nippon SLM-4000",
            deviceProfileId: "device-shin-nippon-slm4000-default",
            exportProfileId: "export-medistar-shin-nippon-slm4000-default",
            description: "Built-in inactive serial profile for Shin-Nippon SLM-4000 lensmeter text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            readTimeoutMilliseconds: 5000,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static InterfaceProfileDefinition CreateMedistarShinNipponTextSerialDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        int baudRate,
        int readTimeoutMilliseconds,
        DateTimeOffset timestamp)
    {
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description,
            SerialSettings: CreateShinNipponTextSerialSettings(baudRate, readTimeoutMilliseconds));
    }

    private static SerialCommunicationSettings CreateShinNipponTextSerialSettings(
        int baudRate,
        int readTimeoutMilliseconds)
    {
        return new SerialCommunicationSettings(
            BaudRate: baudRate,
            DataBits: 8,
            StopBits: SerialStopBitsSetting.One,
            Parity: SerialParitySetting.None,
            Handshake: SerialHandshakeSetting.None,
            DtrEnable: false,
            RtsEnable: false,
            IsBidirectional: false,
            LineTerminator: SerialLineTerminatorSetting.CRLF,
            ReadTimeoutMilliseconds: readTimeoutMilliseconds,
            WriteTimeoutMilliseconds: 1000);
    }

    public static InterfaceProfileDefinition CreateMedistarReichert7CrNctDefault()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-reichert-7cr-nct-default",
            name: "MEDISTAR + Reichert 7CR NCT",
            product: "MEDISTAR/Reichert 7CR NCT",
            deviceProfileId: "device-reichert-7cr-nct-default",
            exportProfileId: "export-medistar-reichert-7cr-nct-default",
            description: "Built-in inactive serial profile for Reichert 7CR NCT tonometry text imports. COM defaults use 19200 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 19200,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds: 30000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarReichertLensChekPlusDefault()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-reichert-lenschek-plus-default",
            name: "MEDISTAR + Reichert LensChek Plus",
            product: "MEDISTAR/Reichert LensChek Plus",
            deviceProfileId: "device-reichert-lenschek-plus-default",
            exportProfileId: "export-medistar-reichert-lenschek-plus-default",
            description: "Built-in inactive serial profile for Reichert LensChek Plus lensmeter text/XML imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarRodenstockCx800Default()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-rodenstock-cx800-default",
            name: "MEDISTAR + Rodenstock CX 800",
            product: "MEDISTAR/Rodenstock CX 800",
            deviceProfileId: "device-rodenstock-cx800-default",
            exportProfileId: "export-medistar-rodenstock-cx800-default",
            description: "Built-in inactive serial profile for Rodenstock CX 800 REF/KM text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.None,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static InterfaceProfileDefinition CreateMedistarReferenceTextSerialDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        SerialCommunicationSettings serialSettings,
        DateTimeOffset timestamp)
    {
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description,
            SerialSettings: serialSettings);
    }

    public static InterfaceProfileDefinition CreateMedistarCanonRkF2Default()
    {
        return CreateMedistarCanonZeissVisionixFileDefault("interface-medistar-canon-rkf2-default", "MEDISTAR + Canon RK-F2", "MEDISTAR/Canon RK-F2", "device-canon-rkf2-default", "export-medistar-canon-rkf2-default", "Built-in inactive file profile for Canon RK-F2 XML REF imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarCanonTx20PDefault()
    {
        return CreateMedistarCanonZeissVisionixFileDefault("interface-medistar-canon-tx20p-default", "MEDISTAR + Canon TX-20P", "MEDISTAR/Canon TX-20P", "device-canon-tx20p-default", "export-medistar-canon-tx20p-default", "Built-in inactive file profile for Canon TX-20P XML IOP/CCT imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarZeissVisulens550Default()
    {
        return CreateMedistarCanonZeissVisionixFileDefault("interface-medistar-zeiss-visulens550-default", "MEDISTAR + ZEISS VISULENS 550", "MEDISTAR/ZEISS VISULENS 550", "device-zeiss-visulens550-default", "export-medistar-zeiss-visulens550-default", "Built-in inactive file profile for ZEISS VISULENS 550 XML lensmeter imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarZeissVisuplan500Default()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-zeiss-visuplan500-default",
            name: "MEDISTAR + ZEISS VISUPLAN 500",
            product: "MEDISTAR/ZEISS VISUPLAN 500",
            deviceProfileId: "device-zeiss-visuplan500-default",
            exportProfileId: "export-medistar-zeiss-visuplan500-default",
            description: "Built-in inactive serial profile for ZEISS VISUPLAN 500 tonometry text imports. COM defaults use 19200 8N1 without DTR/RTS according to the reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 19200,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarZeissVisuref100Default()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-zeiss-visuref100-default",
            name: "MEDISTAR + ZEISS VISUREF 100",
            product: "MEDISTAR/ZEISS VISUREF 100",
            deviceProfileId: "device-zeiss-visuref100-default",
            exportProfileId: "export-medistar-zeiss-visuref100-default",
            description: "Built-in inactive serial profile for ZEISS VISUREF 100 REF/KM text imports. COM defaults use 9600 8N1 without DTR/RTS according to the reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarZeissIolMaster700Default()
    {
        return CreateMedistarCanonZeissVisionixFileDefault(
            "interface-medistar-zeiss-iolmaster700-default",
            "MEDISTAR + ZEISS IOLMaster 700",
            "MEDISTAR/ZEISS IOLMaster 700",
            "device-zeiss-iolmaster700-default",
            "export-medistar-zeiss-iolmaster700-default",
            "Built-in inactive file profile for ZEISS IOLMaster 700 XML biometry imports. No device output is sent.",
            attachmentExternalLinkPathTemplate: string.Empty);
    }

    public static InterfaceProfileDefinition CreateMedistarVisionixRetinomax5Default()
    {
        return CreateMedistarReferenceTextSerialDefault(
            id: "interface-medistar-visionix-retinomax5-default",
            name: "MEDISTAR + Visionix Retinomax 5",
            product: "MEDISTAR/Visionix Retinomax 5",
            deviceProfileId: "device-visionix-retinomax5-default",
            exportProfileId: "export-medistar-visionix-retinomax5-default",
            description: "Built-in inactive serial profile for Visionix Retinomax 5 REF/KM text imports. COM defaults use 115200 8N1 without DTR/RTS according to the reference settings; practical raw-data validation remains open.",
            serialSettings: new SerialCommunicationSettings(
                BaudRate: 115200,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.None,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CRLF,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000),
            timestamp: new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarVisionixVx120Default()
    {
        return CreateMedistarCanonZeissVisionixFileDefault("interface-medistar-visionix-vx120-default", "MEDISTAR + Visionix VX 120", "MEDISTAR/Visionix VX 120", "device-visionix-vx120-default", "export-medistar-visionix-vx120-default", "Built-in inactive file profile for Visionix VX 120 XML REF imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarVisionixVx650Default()
    {
        return CreateMedistarCanonZeissVisionixFileDefault("interface-medistar-visionix-vx650-default", "MEDISTAR + Visionix VX 650", "MEDISTAR/Visionix VX 650", "device-visionix-vx650-default", "export-medistar-visionix-vx650-default", "Built-in inactive file profile for Visionix VX 650 XML REF/TM imports.");
    }

    private static InterfaceProfileDefinition CreateMedistarCanonZeissVisionixFileDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        string attachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}")
    {
        var timestamp = new DateTimeOffset(2026, 6, 6, 12, 0, 0, TimeSpan.Zero);
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true,
                AttachmentExternalLinkPathTemplate: attachmentExternalLinkPathTemplate),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description);
    }

    public static InterfaceProfileDefinition CreateMedistarHuvitzHrk8000ADefault()
    {
        return CreateMedistarHuvitzTextSerialDefault(
            id: "interface-medistar-huvitz-hrk8000a-default",
            name: "MEDISTAR + Huvitz HRK-8000A",
            product: "MEDISTAR/Huvitz HRK-8000A",
            deviceProfileId: "device-huvitz-hrk8000a-default",
            exportProfileId: "export-medistar-huvitz-hrk8000a-default",
            description: "Built-in inactive serial profile for Huvitz HRK-8000A REF/KM text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarHuvitzHrk9000ADefault()
    {
        return CreateMedistarHuvitzTextSerialDefault(
            id: "interface-medistar-huvitz-hrk9000a-default",
            name: "MEDISTAR + Huvitz HRK-9000A",
            product: "MEDISTAR/Huvitz HRK-9000A",
            deviceProfileId: "device-huvitz-hrk9000a-default",
            exportProfileId: "export-medistar-huvitz-hrk9000a-default",
            description: "Built-in inactive serial profile for Huvitz HRK-9000A REF/KM text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarHuvitzHnt1PDefault()
    {
        return CreateMedistarHuvitzTextSerialDefault(
            id: "interface-medistar-huvitz-hnt1p-default",
            name: "MEDISTAR + Huvitz HNT-1P",
            product: "MEDISTAR/Huvitz HNT-1P",
            deviceProfileId: "device-huvitz-hnt1p-default",
            exportProfileId: "export-medistar-huvitz-hnt1p-default",
            description: "Built-in inactive serial profile for Huvitz HNT-1P tonometry/pachymetry text imports. COM defaults use 115200 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 115200,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarHuvitzHtr1ADefault()
    {
        return CreateMedistarHuvitzTextSerialDefault(
            id: "interface-medistar-huvitz-htr1a-default",
            name: "MEDISTAR + Huvitz HTR-1A",
            product: "MEDISTAR/Huvitz HTR-1A",
            deviceProfileId: "device-huvitz-htr1a-default",
            exportProfileId: "export-medistar-huvitz-htr1a-default",
            description: "Built-in inactive serial profile for Huvitz HTR-1A combined REF/KM/IOP/CCT text imports. COM defaults use 9600 8N1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            baudRate: 9600,
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    private static InterfaceProfileDefinition CreateMedistarHuvitzTextSerialDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        int baudRate,
        DateTimeOffset timestamp)
    {
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description,
            SerialSettings: CreateHuvitzTextSerialSettings(baudRate));
    }

    private static SerialCommunicationSettings CreateHuvitzTextSerialSettings(int baudRate)
    {
        return new SerialCommunicationSettings(
            BaudRate: baudRate,
            DataBits: 8,
            StopBits: SerialStopBitsSetting.One,
            Parity: SerialParitySetting.None,
            Handshake: SerialHandshakeSetting.None,
            DtrEnable: false,
            RtsEnable: false,
            IsBidirectional: false,
            LineTerminator: SerialLineTerminatorSetting.CRLF,
            ReadTimeoutMilliseconds: 5000,
            WriteTimeoutMilliseconds: 1000);
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyCf2000Default()
    {
        return CreateMedistarTomeySerialDefault(
            id: "interface-medistar-tomey-cf2000-default",
            name: "MEDISTAR + TOMEY CF-2000",
            product: "MEDISTAR/TOMEY CF-2000",
            deviceProfileId: "device-tomey-cf2000-default",
            exportProfileId: "export-medistar-tomey-cf2000-default",
            description: "Built-in inactive serial profile for TOMEY CF-2000 lensmeter text imports. COM defaults use 9600 8O1 without DTR/RTS according to the neutral reference settings; practical raw-data validation remains open.",
            timestamp: new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyTl2000CDefault()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-tl2000c-default", "MEDISTAR + TOMEY TL-2000C", "MEDISTAR/TOMEY TL-2000C", "device-tomey-tl2000c-default", "export-medistar-tomey-tl2000c-default", "Built-in inactive file profile for TOMEY TL-2000C lensmeter CSV imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyTl6000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-tl6000-default", "MEDISTAR + TOMEY TL-6000", "MEDISTAR/TOMEY TL-6000", "device-tomey-tl6000-default", "export-medistar-tomey-tl6000-default", "Built-in inactive file profile for TOMEY TL-6000 lensmeter CSV imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyTl7000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-tl7000-default", "MEDISTAR + TOMEY TL-7000", "MEDISTAR/TOMEY TL-7000", "device-tomey-tl7000-default", "export-medistar-tomey-tl7000-default", "Built-in inactive file profile for TOMEY TL-7000 lensmeter CSV imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyMr6000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-mr6000-default", "MEDISTAR + TOMEY MR-6000", "MEDISTAR/TOMEY MR-6000", "device-tomey-mr6000-default", "export-medistar-tomey-mr6000-default", "Built-in inactive file profile for TOMEY MR-6000 XML imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyTop1000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-top1000-default", "MEDISTAR + TOMEY TOP-1000", "MEDISTAR/TOMEY TOP-1000", "device-tomey-top1000-default", "export-medistar-tomey-top1000-default", "Built-in inactive file profile for TOMEY TOP-1000 XML imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyEm3000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-em3000-default", "MEDISTAR + TOMEY EM-3000", "MEDISTAR/TOMEY EM-3000", "device-tomey-em3000-default", "export-medistar-tomey-em3000-default", "Built-in inactive file profile for TOMEY EM-3000 endothelial CSV imports.");
    }

    public static InterfaceProfileDefinition CreateMedistarTomeyEm4000Default()
    {
        return CreateMedistarTomeyFileDefault("interface-medistar-tomey-em4000-default", "MEDISTAR + TOMEY EM-4000", "MEDISTAR/TOMEY EM-4000", "device-tomey-em4000-default", "export-medistar-tomey-em4000-default", "Built-in inactive file profile for TOMEY EM-4000 endothelial CSV imports.");
    }

    private static InterfaceProfileDefinition CreateMedistarTomeyFileDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description)
    {
        var timestamp = new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: $"{description} Practical raw-data validation remains open.",
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: $"{description} Practical raw-data validation remains open.");
    }

    private static InterfaceProfileDefinition CreateMedistarTomeySerialDefault(
        string id,
        string name,
        string product,
        string deviceProfileId,
        string exportProfileId,
        string description,
        DateTimeOffset timestamp)
    {
        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: name,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: description,
                Vendor: "XdtDeviceBridge",
                Product: product,
                Version: "0.1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: true,
                IsUserDefined: false),
            AisProfileId: "ais-medistar-default",
            DeviceProfileId: deviceProfileId,
            ExportProfileId: exportProfileId,
            FolderOptions: new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: true,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: description,
            SerialSettings: new SerialCommunicationSettings(
                BaudRate: 9600,
                DataBits: 8,
                StopBits: SerialStopBitsSetting.One,
                Parity: SerialParitySetting.Odd,
                Handshake: SerialHandshakeSetting.None,
                DtrEnable: false,
                RtsEnable: false,
                IsBidirectional: false,
                LineTerminator: SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds: 5000,
                WriteTimeoutMilliseconds: 1000));
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
                ClearAisImportFolderBeforeProcessing: true,
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
                Name: "MEDISTAR + Manuelle DokumentÃ¼bergabe",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: "Default interface profile for manual document handoff to MEDISTAR with per-file attachment links.",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/Manuelle DokumentÃ¼bergabe",
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
                ClearAisImportFolderBeforeProcessing: true,
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

