using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileJsonSerializerTests
{
    private readonly ProfileJsonSerializer _serializer = new();

    [Fact]
    public void AisProfile_ShouldRoundTrip()
    {
        var profile = DefaultAisProfiles.CreateMedistarDefault();

        var json = _serializer.SerializeAisProfile(profile);
        var deserialized = _serializer.DeserializeAisProfile(json);

        Assert.Contains("\"ProfileKind\": \"AisProfile\"", json);
        Assert.Equal("MEDISTAR", deserialized.Name);
        Assert.Equal("Windows-1252", deserialized.DefaultEncoding);
        Assert.Equal("6310", deserialized.RequiredStaticFields["8000"]);
    }

    [Fact]
    public void DeviceProfileDefinition_ShouldRoundTrip()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            DeviceImagePath = @"C:\Praxis\Bilder\ark1s.png"
        };

        var json = _serializer.SerializeDeviceProfileDefinition(profile);
        var deserialized = _serializer.DeserializeDeviceProfileDefinition(json);

        Assert.Equal("NIDEK", deserialized.Manufacturer);
        Assert.Equal("ARK1S", deserialized.Model);
        Assert.Equal(@"C:\Praxis\Bilder\ark1s.png", deserialized.DeviceImagePath);
        Assert.Equal(DeviceConnectionKind.NetworkLan, deserialized.ConnectionKind);
        Assert.Null(deserialized.SerialSettings);
        Assert.Equal(profile.Measurements.Count, deserialized.Measurements.Count);
        Assert.Contains(deserialized.Measurements, measurement =>
            measurement.Id == "far-pd"
            && measurement.SourcePath == "PD/PDList[@No='1']/FarPD");
    }

    [Fact]
    public void DeviceProfileDefinition_ShouldRoundTripSerialRs232Settings()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            ConnectionKind = DeviceConnectionKind.SerialRs232,
            SerialSettings = SerialCommunicationSettings.Default with
            {
                PortName = "COM4",
                BaudRate = 19200,
                DataBits = 7,
                StopBits = SerialStopBitsSetting.Two,
                Parity = SerialParitySetting.Even,
                Handshake = SerialHandshakeSetting.RequestToSend,
                IsBidirectional = true,
                LineTerminator = SerialLineTerminatorSetting.CR,
                ReadTimeoutMilliseconds = 1500,
                WriteTimeoutMilliseconds = 2000
            }
        };

        var json = _serializer.SerializeDeviceProfileDefinition(profile);
        var deserialized = _serializer.DeserializeDeviceProfileDefinition(json);

        Assert.Contains("\"ConnectionKind\": \"SerialRs232\"", json);
        Assert.Equal(DeviceConnectionKind.SerialRs232, deserialized.ConnectionKind);
        Assert.NotNull(deserialized.SerialSettings);
        Assert.Equal("COM4", deserialized.SerialSettings!.PortName);
        Assert.Equal(19200, deserialized.SerialSettings.BaudRate);
        Assert.Equal(7, deserialized.SerialSettings.DataBits);
        Assert.Equal(SerialStopBitsSetting.Two, deserialized.SerialSettings.StopBits);
        Assert.Equal(SerialParitySetting.Even, deserialized.SerialSettings.Parity);
        Assert.Equal(SerialHandshakeSetting.RequestToSend, deserialized.SerialSettings.Handshake);
        Assert.True(deserialized.SerialSettings.IsBidirectional);
        Assert.Equal(SerialLineTerminatorSetting.CR, deserialized.SerialSettings.LineTerminator);
        Assert.Equal(1500, deserialized.SerialSettings.ReadTimeoutMilliseconds);
        Assert.Equal(2000, deserialized.SerialSettings.WriteTimeoutMilliseconds);
    }

    [Fact]
    public void DeviceProfileDefinition_ShouldDeserializeOlderJsonWithoutConnectionKindAsNetworkLan()
    {
        var json = """
        {
          "Metadata": {
            "Id": "device-old",
            "Name": "Old Device",
            "ProfileKind": "DeviceProfile",
            "Description": null,
            "Vendor": null,
            "Product": null,
            "Version": "1.0",
            "CreatedAt": "2026-05-03T12:00:00+00:00",
            "UpdatedAt": "2026-05-03T12:00:00+00:00",
            "CreatedBy": "Test",
            "IsBuiltIn": false,
            "IsUserDefined": true
          },
          "Manufacturer": "NIDEK",
          "Model": "ALT",
          "DeviceType": "Autorefractor",
          "ParserMode": "Xml",
          "Measurements": [],
          "SupportedExaminationTypes": [],
          "CanContainMultipleExaminationTypes": false,
          "IsBidirectional": false,
          "DeviceImagePath": ""
        }
        """;

        var deserialized = _serializer.DeserializeDeviceProfileDefinition(json);

        Assert.Equal(DeviceConnectionKind.NetworkLan, deserialized.ConnectionKind);
        Assert.Null(deserialized.SerialSettings);
    }

    [Fact]
    public void ExportProfileDefinition_ShouldRoundTrip()
    {
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var json = _serializer.SerializeExportProfileDefinition(profile);
        var deserialized = _serializer.DeserializeExportProfileDefinition(json);

        Assert.Contains("\"RuleType\": \"StaticValue\"", json);
        Assert.Equal("Windows-1252", deserialized.OutputEncoding);
        Assert.Equal(profile.Rules.Count, deserialized.Rules.Count);
        Assert.Equal(2, deserialized.Rules.Count(rule => rule.TargetFieldCode == "6228"));
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldRoundTrip()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                AttachmentImportFolder = @"C:\XdtDeviceBridge\GAImport",
                AttachmentExportFolder = @"C:\XdtDeviceBridge\GAExport",
                AttachmentFileNameTemplate = "GA_{Ais.PatientNumber}{ExtensionUpper}",
                AttachmentTransferMode = AttachmentTransferMode.Move,
                AttachmentExternalLinkDocumentName = "PDF-Befund",
                AttachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
                AttachmentExternalLinkDescription = "Messprotokoll Autorefraktor",
                AttachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}",
                IsAttachmentProcessingEnabled = true,
                AttachmentRequirementMode = AttachmentRequirementMode.Required,
                AttachmentWaitTimeoutSeconds = 45,
                AttachmentFileStabilityWaitSeconds = 3,
                AutoImportScanIntervalSeconds = 7,
                DeviceFileWaitTimeoutMinutes = 12,
                IsAttachmentOnlyMode = true,
                ShowAttachmentDocumentationDialog = true,
                AttachmentCompletionMode = AttachmentCompletionMode.ManualConfirmation,
                AttachmentQuietPeriodSeconds = 25
            }
        };

        var json = _serializer.SerializeInterfaceProfileDefinition(profile);
        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.False(deserialized.IsActive);
        Assert.True(deserialized.IsLicenseRequired);
        Assert.Equal(profile.FolderOptions, deserialized.FolderOptions);
        Assert.Null(deserialized.SerialSettings);
        Assert.Contains("\"AttachmentImportFolder\":", json);
        Assert.Contains("\"AttachmentExportFolder\":", json);
        Assert.Contains("\"AttachmentFileNameTemplate\":", json);
        Assert.Contains("\"AttachmentTransferMode\": \"Move\"", json);
        Assert.Contains("\"AttachmentExternalLinkDocumentName\":", json);
        Assert.Contains("\"AttachmentExternalLinkFileFormat\":", json);
        Assert.Contains("\"AttachmentExternalLinkDescription\":", json);
        Assert.Contains("\"AttachmentExternalLinkPathTemplate\":", json);
        Assert.Contains("\"IsAttachmentProcessingEnabled\": true", json);
        Assert.Contains("\"AttachmentRequirementMode\": \"Required\"", json);
        Assert.Contains("\"AttachmentWaitTimeoutSeconds\": 45", json);
        Assert.Contains("\"AttachmentFileStabilityWaitSeconds\": 3", json);
        Assert.Contains("\"AutoImportScanIntervalSeconds\": 7", json);
        Assert.Contains("\"DeviceFileWaitTimeoutMinutes\": 12", json);
        Assert.Contains("\"IsAttachmentOnlyMode\": true", json);
        Assert.Contains("\"ShowAttachmentDocumentationDialog\": true", json);
        Assert.Contains("\"AttachmentCompletionMode\": \"ManualConfirmation\"", json);
        Assert.Contains("\"AttachmentQuietPeriodSeconds\": 25", json);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldRoundTripSerialSettings()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            SerialSettings = SerialCommunicationSettings.Default with
            {
                PortName = "COM8",
                BaudRate = 38400,
                Handshake = SerialHandshakeSetting.XOnXOff,
                IsBidirectional = true
            }
        };

        var json = _serializer.SerializeInterfaceProfileDefinition(profile);
        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.Contains("\"SerialSettings\":", json);
        Assert.NotNull(deserialized.SerialSettings);
        Assert.Equal("COM8", deserialized.SerialSettings!.PortName);
        Assert.Equal(38400, deserialized.SerialSettings.BaudRate);
        Assert.Equal(SerialHandshakeSetting.XOnXOff, deserialized.SerialSettings.Handshake);
        Assert.True(deserialized.SerialSettings.IsBidirectional);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldRoundTripNidekRtSerialSendMode()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault() with
        {
            NidekRtSerialSendMode = NidekRtSerialSendMode.RsSdHandshake,
            NidekRtSerialOutputFrameVariant = NidekRtSerialOutputFrameVariant.LmOnlyWithoutAdd
        };

        var json = _serializer.SerializeInterfaceProfileDefinition(profile);
        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.Contains("\"NidekRtSerialSendMode\": \"RsSdHandshake\"", json);
        Assert.Contains("\"NidekRtSerialOutputFrameVariant\": \"LmOnlyWithoutAdd\"", json);
        Assert.Equal(NidekRtSerialSendMode.RsSdHandshake, deserialized.NidekRtSerialSendMode);
        Assert.Equal(NidekRtSerialOutputFrameVariant.LmOnlyWithoutAdd, deserialized.NidekRtSerialOutputFrameVariant);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldDeserializeOlderFolderOptionsWithoutArchiveMode()
    {
        var json = """
        {
          "Metadata": {
            "Id": "interface-old",
            "Name": "Old Interface",
            "ProfileKind": "InterfaceProfile",
            "Description": null,
            "Vendor": null,
            "Product": null,
            "Version": "1.0",
            "CreatedAt": "2026-05-03T12:00:00+00:00",
            "UpdatedAt": "2026-05-03T12:00:00+00:00",
            "CreatedBy": "Test",
            "IsBuiltIn": false,
            "IsUserDefined": true
          },
          "AisProfileId": "ais",
          "DeviceProfileId": "device",
          "ExportProfileId": "export",
          "FolderOptions": {
            "AisImportFolder": "",
            "DeviceImportFolder": "",
            "ExportFolder": "",
            "ArchiveFolder": "",
            "ErrorFolder": "",
            "ClearAisImportFolderBeforeProcessing": false,
            "ClearDeviceImportFolderBeforeProcessing": false,
            "ClearExportFolderAfterSuccessfulTransfer": false,
            "ArchiveProcessedFiles": false,
            "MoveFailedFilesToErrorFolder": true
          },
          "IsActive": false,
          "IsLicenseRequired": true,
          "Description": null
        }
        """;

        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.Equal(ArchiveProcessedFileMode.Copy, deserialized.FolderOptions.ArchiveProcessedFileMode);
        Assert.Null(deserialized.FolderOptions.ArchiveRetentionDays);
        Assert.Equal(string.Empty, deserialized.FolderOptions.AttachmentImportFolder);
        Assert.Equal(string.Empty, deserialized.FolderOptions.AttachmentExportFolder);
        Assert.Equal(AttachmentFileNameBuilder.DefaultTemplate, deserialized.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, deserialized.FolderOptions.AttachmentTransferMode);
        Assert.Equal("Datei", deserialized.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", deserialized.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal(string.Empty, deserialized.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", deserialized.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.False(deserialized.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Optional, deserialized.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(30, deserialized.FolderOptions.AttachmentWaitTimeoutSeconds);
        Assert.Equal(2, deserialized.FolderOptions.AttachmentFileStabilityWaitSeconds);
        Assert.Equal(5, deserialized.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(10, deserialized.FolderOptions.DeviceFileWaitTimeoutMinutes);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldDeserializeFolderOptionsWithAttachmentFolders()
    {
        var json = """
        {
          "Metadata": {
            "Id": "interface-attachments",
            "Name": "Interface Attachments",
            "ProfileKind": "InterfaceProfile",
            "Description": null,
            "Vendor": null,
            "Product": null,
            "Version": "1.0",
            "CreatedAt": "2026-05-03T12:00:00+00:00",
            "UpdatedAt": "2026-05-03T12:00:00+00:00",
            "CreatedBy": "Test",
            "IsBuiltIn": false,
            "IsUserDefined": true
          },
          "AisProfileId": "ais",
          "DeviceProfileId": "device",
          "ExportProfileId": "export",
          "FolderOptions": {
            "AisImportFolder": "",
            "DeviceImportFolder": "",
            "ExportFolder": "",
            "ArchiveFolder": "",
            "ErrorFolder": "",
            "ClearAisImportFolderBeforeProcessing": false,
            "ClearDeviceImportFolderBeforeProcessing": false,
            "ClearExportFolderAfterSuccessfulTransfer": false,
            "ArchiveProcessedFiles": false,
            "MoveFailedFilesToErrorFolder": true,
            "ArchiveProcessedFileMode": "Copy",
            "ArchiveRetentionDays": null,
            "AttachmentImportFolder": "C:\\XdtDeviceBridge\\GAImport",
            "AttachmentExportFolder": "C:\\XdtDeviceBridge\\GAExport",
            "AttachmentFileNameTemplate": "GA_{Ais.PatientNumber}{ExtensionUpper}",
            "AttachmentTransferMode": "Move",
            "AttachmentExternalLinkDocumentName": "PDF-Befund",
            "AttachmentExternalLinkFileFormat": "{ExtensionUpperWithoutDot}",
            "AttachmentExternalLinkDescription": "Messprotokoll Autorefraktor",
            "AttachmentExternalLinkPathTemplate": "{Attachment.TargetFullPath}",
            "IsAttachmentProcessingEnabled": true,
            "AttachmentRequirementMode": "Required",
            "AttachmentWaitTimeoutSeconds": 45,
            "AttachmentFileStabilityWaitSeconds": 3,
            "AutoImportScanIntervalSeconds": 7,
            "DeviceFileWaitTimeoutMinutes": 12
          },
          "IsActive": false,
          "IsLicenseRequired": true,
          "Description": null
        }
        """;

        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.Equal(@"C:\XdtDeviceBridge\GAImport", deserialized.FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"C:\XdtDeviceBridge\GAExport", deserialized.FolderOptions.AttachmentExportFolder);
        Assert.Equal("GA_{Ais.PatientNumber}{ExtensionUpper}", deserialized.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, deserialized.FolderOptions.AttachmentTransferMode);
        Assert.Equal("PDF-Befund", deserialized.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", deserialized.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal("Messprotokoll Autorefraktor", deserialized.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", deserialized.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.True(deserialized.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Required, deserialized.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(45, deserialized.FolderOptions.AttachmentWaitTimeoutSeconds);
        Assert.Equal(3, deserialized.FolderOptions.AttachmentFileStabilityWaitSeconds);
        Assert.Equal(7, deserialized.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(12, deserialized.FolderOptions.DeviceFileWaitTimeoutMinutes);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldKeepExplicitAttachmentProcessingDisabled()
    {
        var json = """
        {
          "Metadata": {
            "Id": "interface-attachments-disabled",
            "Name": "Interface Attachments Disabled",
            "ProfileKind": "InterfaceProfile",
            "Description": null,
            "Vendor": null,
            "Product": null,
            "Version": "1.0",
            "CreatedAt": "2026-05-03T12:00:00+00:00",
            "UpdatedAt": "2026-05-03T12:00:00+00:00",
            "CreatedBy": "Test",
            "IsBuiltIn": false,
            "IsUserDefined": true
          },
          "AisProfileId": "ais",
          "DeviceProfileId": "device",
          "ExportProfileId": "export",
          "FolderOptions": {
            "AisImportFolder": "",
            "DeviceImportFolder": "",
            "ExportFolder": "",
            "ArchiveFolder": "",
            "ErrorFolder": "",
            "ClearAisImportFolderBeforeProcessing": false,
            "ClearDeviceImportFolderBeforeProcessing": false,
            "ClearExportFolderAfterSuccessfulTransfer": false,
            "ArchiveProcessedFiles": false,
            "MoveFailedFilesToErrorFolder": true,
            "IsAttachmentProcessingEnabled": false
          },
          "IsActive": false,
          "IsLicenseRequired": true,
          "Description": null
        }
        """;

        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.False(deserialized.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void InterfaceProfileDefinition_ShouldKeepExplicitAttachmentTransferModeCopy()
    {
        var json = """
        {
          "Metadata": {
            "Id": "interface-attachments-copy",
            "Name": "Interface Attachments Copy",
            "ProfileKind": "InterfaceProfile",
            "Description": null,
            "Vendor": null,
            "Product": null,
            "Version": "1.0",
            "CreatedAt": "2026-05-03T12:00:00+00:00",
            "UpdatedAt": "2026-05-03T12:00:00+00:00",
            "CreatedBy": "Test",
            "IsBuiltIn": false,
            "IsUserDefined": true
          },
          "AisProfileId": "ais",
          "DeviceProfileId": "device",
          "ExportProfileId": "export",
          "FolderOptions": {
            "AisImportFolder": "",
            "DeviceImportFolder": "",
            "ExportFolder": "",
            "ArchiveFolder": "",
            "ErrorFolder": "",
            "ClearAisImportFolderBeforeProcessing": false,
            "ClearDeviceImportFolderBeforeProcessing": false,
            "ClearExportFolderAfterSuccessfulTransfer": false,
            "ArchiveProcessedFiles": false,
            "MoveFailedFilesToErrorFolder": true,
            "AttachmentTransferMode": "Copy"
          },
          "IsActive": false,
          "IsLicenseRequired": true,
          "Description": null
        }
        """;

        var deserialized = _serializer.DeserializeInterfaceProfileDefinition(json);

        Assert.Equal(AttachmentTransferMode.Copy, deserialized.FolderOptions.AttachmentTransferMode);
    }

    [Fact]
    public void TemplatePackage_ShouldRoundTrip()
    {
        var package = CreateTemplatePackage();

        var json = _serializer.SerializeTemplatePackage(package);
        var deserialized = _serializer.DeserializeTemplatePackage(json);

        Assert.Equal(package.Metadata.Id, deserialized.Metadata.Id);
        Assert.Equal(package.Metadata.Name, deserialized.Metadata.Name);
        Assert.Equal("1.0", deserialized.PackageFormatVersion);
        Assert.Equal(package.IncludedProfiles.Count, deserialized.IncludedProfiles.Count);
    }

    private static TemplatePackage CreateTemplatePackage()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);
        var createdAt = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

        return new TemplatePackage(
            Metadata: CreateMetadata("package-medistar-nidek-ark1s", "MEDISTAR + NIDEK ARK1S Package", ProfileKind.TemplatePackage, timestamp),
            IncludedProfiles: new[]
            {
                CreateMetadata("ais-medistar-default", "MEDISTAR", ProfileKind.AisProfile, timestamp),
                CreateMetadata("device-nidek-ark1s-default", "NIDEK ARK1S", ProfileKind.DeviceProfile, timestamp),
                CreateMetadata("export-medistar-nidek-ark1s-default", "MEDISTAR + NIDEK ARK1S Export", ProfileKind.ExportProfile, timestamp)
            },
            PackageFormatVersion: "1.0",
            CreatedAt: createdAt,
            CreatedBy: "XdtDeviceBridge",
            Description: "Serializer roundtrip test package.");
    }

    private static ProfileMetadata CreateMetadata(
        string id,
        string name,
        ProfileKind profileKind,
        DateTimeOffset timestamp)
    {
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: true,
            IsUserDefined: false);
    }
}
