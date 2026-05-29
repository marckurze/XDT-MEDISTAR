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
                ClearAisImportFolderBeforeProcessing: false,
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
                ClearAisImportFolderBeforeProcessing: false,
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
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: true),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Built-in inactive default interface definition for the MEDISTAR/TOPCON KR-1 REF profile candidate. KM/KRT remains pending until a real fixture is available.");
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
                ClearAisImportFolderBeforeProcessing: false,
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
                ClearAisImportFolderBeforeProcessing: false,
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
                ClearAisImportFolderBeforeProcessing: false,
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
            description: "Built-in inactive candidate for NIDEK RT-2100 serial RS232 phoropter captures. PC-to-RT LM/AR output is prepared for Baukasten preview only until live validation.",
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
            description: "Built-in inactive candidate for NIDEK RT-3100 serial RS232 phoropter captures with Type1/Type2 communication presets prepared.",
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
            description: "Built-in inactive candidate for NIDEK RT-5100 serial RS232 phoropter captures with extended data sources kept diagnostic until practice captures are available.",
            timestamp: new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    }

    private static InterfaceProfileDefinition CreateMedistarNidekRtSerialDefault(
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
                ClearAisImportFolderBeforeProcessing: false,
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
            NidekRtSerialOutputFrameVariant: NidekRtSerialOutputFrameVariant.FullSelectedData);
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
