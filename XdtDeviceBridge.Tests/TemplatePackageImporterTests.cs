using System.IO.Compression;
using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImporterTests
{
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();
    private readonly ProfileJsonSerializer _serializer = new();

    [Fact]
    public void Import_ShouldReadPreviouslyExportedPackage()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        Assert.Equal("package-medistar-nidek-ark1s", result.Package.Metadata.Id);
        Assert.Single(result.AisProfiles);
        Assert.Single(result.DeviceProfiles);
        Assert.Single(result.ExportProfiles);
        Assert.Single(result.InterfaceProfiles);
    }

    [Fact]
    public void Import_ShouldReadPackageJson()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        Assert.Equal("MEDISTAR + NIDEK ARK1S Package", result.Package.Metadata.Name);
        Assert.Equal("1.0", result.Package.PackageFormatVersion);
    }

    [Fact]
    public void Import_ShouldReadAisProfile()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        var profile = Assert.Single(result.AisProfiles);
        Assert.Equal("MEDISTAR", profile.Name);
        Assert.Equal("6310", profile.RequiredStaticFields["8000"]);
    }

    [Fact]
    public void Import_ShouldReadDeviceProfile()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        var profile = Assert.Single(result.DeviceProfiles);
        Assert.Equal("NIDEK", profile.Manufacturer);
        Assert.Equal("ARK1S", profile.Model);
    }

    [Fact]
    public void Import_ShouldReadExportProfile()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        var profile = Assert.Single(result.ExportProfiles);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(2, profile.Rules.Count(rule => rule.TargetFieldCode == "6228"));
    }

    [Fact]
    public void Import_ShouldReadInterfaceProfile()
    {
        var zipPath = ExportPackage();

        var result = _importer.Import(zipPath);

        var profile = Assert.Single(result.InterfaceProfiles);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
    }

    [Fact]
    public void Import_ShouldReadAttachmentExternalLinkSettingsFromInterfaceProfile()
    {
        var zipPath = ExportPackage(CreateInterfaceProfileWithAttachmentSettings());

        var result = _importer.Import(zipPath);

        var profile = Assert.Single(result.InterfaceProfiles);
        Assert.Equal(@"C:\XdtDeviceBridge\GAImport", profile.FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"C:\XdtDeviceBridge\GAExport", profile.FolderOptions.AttachmentExportFolder);
        Assert.Equal("GA_{Ais.PatientNumber}{ExtensionUpper}", profile.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, profile.FolderOptions.AttachmentTransferMode);
        Assert.Equal("PDF-Befund", profile.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", profile.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal("Messprotokoll Autorefraktor", profile.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", profile.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.True(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Required, profile.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(45, profile.FolderOptions.AttachmentWaitTimeoutSeconds);
    }

    [Fact]
    public void Import_ShouldIgnoreUnknownAdditionalFile()
    {
        var zipPath = ExportPackage();
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update, Encoding.UTF8))
        {
            var entry = archive.CreateEntry("notes/readme.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("ignored");
        }

        var result = _importer.Import(zipPath);

        Assert.Equal("package-medistar-nidek-ark1s", result.Package.Metadata.Id);
        Assert.Single(result.AisProfiles);
    }

    [Fact]
    public void Import_ShouldAllowMissingOptionalProfileFolders()
    {
        var zipPath = CreateTempZipPath();
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create, Encoding.UTF8))
        {
            WriteEntry(archive, "package.json", _serializer.SerializeTemplatePackage(CreateTemplatePackage(Array.Empty<ProfileMetadata>())));
        }

        var result = _importer.Import(zipPath);

        Assert.Equal("package-medistar-nidek-ark1s", result.Package.Metadata.Id);
        Assert.Empty(result.AisProfiles);
        Assert.Empty(result.DeviceProfiles);
        Assert.Empty(result.ExportProfiles);
        Assert.Empty(result.InterfaceProfiles);
    }

    [Fact]
    public void Import_ShouldThrowArgumentExceptionForEmptyZipFilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() => _importer.Import(""));

        Assert.Contains("ZIP file path must not be empty.", exception.Message);
    }

    [Fact]
    public void Import_ShouldThrowFileNotFoundExceptionForMissingZipFile()
    {
        var zipPath = CreateTempZipPath();

        var exception = Assert.Throws<FileNotFoundException>(() => _importer.Import(zipPath));

        Assert.Contains("Template package ZIP file not found:", exception.Message);
        Assert.Equal(zipPath, exception.FileName);
    }

    [Fact]
    public void Import_ShouldThrowInvalidOperationExceptionWhenPackageJsonIsMissing()
    {
        var zipPath = CreateTempZipPath();
        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create, Encoding.UTF8))
        {
            WriteEntry(archive, "ais/ais-medistar-default.json", _serializer.SerializeAisProfile(DefaultAisProfiles.CreateMedistarDefault()));
        }

        var exception = Assert.Throws<InvalidOperationException>(() => _importer.Import(zipPath));

        Assert.Contains("Template package ZIP must contain package.json.", exception.Message);
    }

    private string ExportPackage(InterfaceProfileDefinition? interfaceProfileOverride = null)
    {
        var zipPath = CreateTempZipPath();
        _exporter.Export(zipPath, CreateRequest(interfaceProfileOverride));
        return zipPath;
    }

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "profiles.zip");
    }

    private static TemplatePackageExportRequest CreateRequest(InterfaceProfileDefinition? interfaceProfileOverride = null)
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var interfaceProfile = interfaceProfileOverride ?? DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var package = CreateTemplatePackage(new[]
        {
            aisProfile.Metadata,
            deviceProfile.Metadata,
            exportProfile.Metadata,
            interfaceProfile.Metadata
        });

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
                AttachmentWaitTimeoutSeconds = 45
            }
        };
    }

    private static TemplatePackage CreateTemplatePackage(IReadOnlyList<ProfileMetadata> includedProfiles)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new TemplatePackage(
            Metadata: CreateMetadata("package-medistar-nidek-ark1s", "MEDISTAR + NIDEK ARK1S Package", ProfileKind.TemplatePackage, timestamp),
            IncludedProfiles: includedProfiles,
            PackageFormatVersion: "1.0",
            CreatedAt: new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc),
            CreatedBy: "XdtDeviceBridge",
            Description: "Template package import test fixture.");
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

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }
}
