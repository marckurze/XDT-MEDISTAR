using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageExporterTests
{
    private readonly TemplatePackageExporter _exporter = new();
    private readonly ProfileJsonSerializer _serializer = new();

    [Fact]
    public void Export_ShouldCreateZipFile()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        Assert.True(File.Exists(zipPath));
    }

    [Fact]
    public void Export_ShouldContainPackageJson()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "package.json");
    }

    [Fact]
    public void Export_ShouldContainAisProfile()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "ais/ais-medistar-default.json");
    }

    [Fact]
    public void Export_ShouldContainDeviceProfile()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "devices/device-nidek-ark1s-default.json");
    }

    [Fact]
    public void Export_ShouldContainExportProfile()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "exports/export-medistar-nidek-ark1s-default.json");
    }

    [Fact]
    public void Export_ShouldContainInterfaceProfile()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "interfaces/interface-medistar-nidek-ark1s-default.json");
    }

    [Fact]
    public void Export_ShouldContainAttachmentExternalLinkSettingsInInterfaceProfile()
    {
        var zipPath = CreateTempZipPath();
        var interfaceProfile = CreateInterfaceProfileWithAttachmentSettings();

        _exporter.Export(zipPath, CreateRequest(interfaceProfile));

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = Assert.Single(archive.Entries, entry => entry.FullName == "interfaces/interface-medistar-nidek-ark1s-default.json");
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        Assert.Contains("\"AttachmentImportFolder\":", json);
        Assert.Contains("\"AttachmentExportFolder\":", json);
        Assert.Contains("\"AttachmentFileNameTemplate\":", json);
        Assert.Contains("\"AttachmentTransferMode\": \"Move\"", json);
        Assert.Contains("\"AttachmentExternalLinkDocumentName\": \"PDF-Befund\"", json);
        Assert.Contains("\"AttachmentExternalLinkFileFormat\": \"{ExtensionUpperWithoutDot}\"", json);
        Assert.Contains("\"AttachmentExternalLinkDescription\": \"Messprotokoll Autorefraktor\"", json);
        Assert.Contains("\"AttachmentExternalLinkPathTemplate\": \"{Attachment.TargetFullPath}\"", json);
        Assert.Contains("\"IsAttachmentProcessingEnabled\": true", json);
        Assert.Contains("\"AttachmentRequirementMode\": \"Required\"", json);
        Assert.Contains("\"AttachmentWaitTimeoutSeconds\": 45", json);
        Assert.Contains("\"DeviceFileWaitTimeoutMinutes\": 12", json);
    }

    [Fact]
    public void Export_ShouldWriteDeserializablePackageJson()
    {
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, CreateRequest());

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = Assert.Single(archive.Entries, entry => entry.FullName == "package.json");
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        var package = _serializer.DeserializeTemplatePackage(reader.ReadToEnd());

        Assert.Equal("package-medistar-nidek-ark1s", package.Metadata.Id);
        Assert.Equal("1.0", package.PackageFormatVersion);
    }

    [Fact]
    public void Export_ShouldCreateTargetFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "nested");
        var zipPath = Path.Combine(folder, "profiles.zip");

        _exporter.Export(zipPath, CreateRequest());

        Assert.True(Directory.Exists(folder));
        Assert.True(File.Exists(zipPath));
    }

    [Fact]
    public void Export_ShouldThrowArgumentExceptionForEmptyZipFilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() => _exporter.Export("", CreateRequest()));

        Assert.Contains("ZIP file path must not be empty.", exception.Message);
    }

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, "profiles.zip");
    }

    private static TemplatePackageExportRequest CreateRequest(InterfaceProfileDefinition? interfaceProfileOverride = null)
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var interfaceProfile = interfaceProfileOverride ?? DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        var package = new TemplatePackage(
            Metadata: CreateMetadata("package-medistar-nidek-ark1s", "MEDISTAR + NIDEK ARK1S Package", ProfileKind.TemplatePackage, timestamp),
            IncludedProfiles: new[]
            {
                aisProfile.Metadata,
                deviceProfile.Metadata,
                exportProfile.Metadata,
                interfaceProfile.Metadata
            },
            PackageFormatVersion: "1.0",
            CreatedAt: new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc),
            CreatedBy: "XdtDeviceBridge",
            Description: "Template package export test fixture.");

        return new TemplatePackageExportRequest(
            Package: package,
            AisProfiles: new[] { aisProfile },
            DeviceProfiles: new[] { deviceProfile },
            ExportProfiles: new[] { exportProfile },
            InterfaceProfiles: new[] { interfaceProfile });
    }

    private static InterfaceProfileDefinition CreateInterfaceProfileWithAttachmentSettings()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
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
                DeviceFileWaitTimeoutMinutes = 12
            }
        };
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
