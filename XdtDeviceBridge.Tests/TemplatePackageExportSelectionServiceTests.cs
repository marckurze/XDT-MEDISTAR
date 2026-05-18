using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageExportSelectionServiceTests
{
    private readonly TemplatePackageExportSelectionService _service = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateForInterfaceProfile_ShouldIncludeOnlySelectedInterfaceAndDependencies()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-ark1s-default",
            Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.Request);
        var request = result.Request!;
        Assert.Equal("medistar-nidek-ark1s-v1.templatepackage.zip", result.SuggestedFileName);
        Assert.Equal("ais-medistar-default", Assert.Single(request.AisProfiles).Metadata.Id);
        Assert.Equal("device-nidek-ark1s-default", Assert.Single(request.DeviceProfiles).Metadata.Id);
        Assert.Equal("export-medistar-nidek-ark1s-default", Assert.Single(request.ExportProfiles).Metadata.Id);
        Assert.Equal("interface-medistar-nidek-ark1s-default", Assert.Single(request.InterfaceProfiles).Metadata.Id);

        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("lm7", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("nt530", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("ar360", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateForAr360InterfaceProfile_ShouldIncludeOnlyAr360AndDependencies()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-ar360-default",
            Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.Request);
        var request = result.Request!;
        Assert.Equal("medistar-nidek-ar360-v1.templatepackage.zip", result.SuggestedFileName);
        Assert.Equal("ais-medistar-default", Assert.Single(request.AisProfiles).Metadata.Id);
        Assert.Equal("device-nidek-ar360-default", Assert.Single(request.DeviceProfiles).Metadata.Id);
        Assert.Equal("export-medistar-nidek-ar360-default", Assert.Single(request.ExportProfiles).Metadata.Id);
        Assert.Equal("interface-medistar-nidek-ar360-default", Assert.Single(request.InterfaceProfiles).Metadata.Id);

        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("lm7", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("nt530", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateForLm7InterfaceProfile_ShouldIncludeOnlyLm7AndDependencies()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-lm7-default",
            Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.Request);
        var request = result.Request!;
        Assert.Equal("medistar-nidek-lm7-v1.templatepackage.zip", result.SuggestedFileName);
        Assert.Equal("ais-medistar-default", Assert.Single(request.AisProfiles).Metadata.Id);
        Assert.Equal("device-nidek-lm7-default", Assert.Single(request.DeviceProfiles).Metadata.Id);
        Assert.Equal("export-medistar-nidek-lm7-default", Assert.Single(request.ExportProfiles).Metadata.Id);
        Assert.Equal("interface-medistar-nidek-lm7-default", Assert.Single(request.InterfaceProfiles).Metadata.Id);

        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("ar360", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("nt530", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.DeviceProfiles, profile => profile.Metadata.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("ar360", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(request.Package.IncludedProfiles, profile => profile.Id.Contains("topcon", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExportedSelectedPackage_ShouldContainOnlyRequiredZipEntries()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-ark1s-default",
            Timestamp);
        var zipPath = CreateTempZipPath();

        _exporter.Export(zipPath, result.Request!);

        using var archive = ZipFile.OpenRead(zipPath);
        var entries = archive.Entries.Select(entry => entry.FullName).OrderBy(entry => entry, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.Equal(
            new[]
            {
                "ais/ais-medistar-default.json",
                "devices/device-nidek-ark1s-default.json",
                "exports/export-medistar-nidek-ark1s-default.json",
                "interfaces/interface-medistar-nidek-ark1s-default.json",
                "package.json"
            },
            entries);
    }

    [Fact]
    public void ExportedSelectedPackage_ShouldBeImportable()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-ark1s-default",
            Timestamp);
        var zipPath = CreateTempZipPath();
        _exporter.Export(zipPath, result.Request!);

        var importResult = _importer.Import(zipPath);

        Assert.Equal("package-medistar-nidek-ark1s-v1", importResult.Package.Metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK ARK1S Templatepaket V1", importResult.Package.Metadata.Name);
        Assert.Single(importResult.AisProfiles);
        Assert.Single(importResult.DeviceProfiles);
        Assert.Single(importResult.ExportProfiles);
        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.False(interfaceProfile.IsActive);
    }

    [Fact]
    public void CreateForInterfaceProfile_ShouldBlockMissingAisDependency()
    {
        var catalog = CreateDefaultCatalog() with
        {
            AisProfiles = Array.Empty<AisProfile>()
        };

        var result = _service.CreateForInterfaceProfile(catalog, "interface-medistar-nidek-ark1s-default", Timestamp);

        Assert.False(result.Success);
        Assert.Null(result.Request);
        Assert.Contains("referenzierte AIS-Profil nicht gefunden", result.ErrorMessage);
    }

    [Fact]
    public void CreateForInterfaceProfile_ShouldBlockMissingDeviceDependency()
    {
        var catalog = CreateDefaultCatalog() with
        {
            DeviceProfiles = Array.Empty<DeviceProfileDefinition>()
        };

        var result = _service.CreateForInterfaceProfile(catalog, "interface-medistar-nidek-ark1s-default", Timestamp);

        Assert.False(result.Success);
        Assert.Null(result.Request);
        Assert.Contains("referenzierte Geräteprofil nicht gefunden", result.ErrorMessage);
    }

    [Fact]
    public void CreateForInterfaceProfile_ShouldBlockMissingExportDependency()
    {
        var catalog = CreateDefaultCatalog() with
        {
            ExportProfiles = Array.Empty<ExportProfileDefinition>()
        };

        var result = _service.CreateForInterfaceProfile(catalog, "interface-medistar-nidek-ark1s-default", Timestamp);

        Assert.False(result.Success);
        Assert.Null(result.Request);
        Assert.Contains("referenzierte Exportprofil nicht gefunden", result.ErrorMessage);
    }

    [Fact]
    public void CreateForInterfaceProfile_ShouldCreateSafeFileName()
    {
        var fileName = TemplatePackageExportSelectionService.CreateSafeTemplatePackageFileName("MEDISTAR + NIDEK ARK1S V1!");

        Assert.Equal("medistar-nidek-ark1s-v1.templatepackage.zip", fileName);
        Assert.DoesNotContain(" ", fileName);
        Assert.DoesNotContain("+", fileName);
    }

    [Fact]
    public void CreateForInterfaceProfile_ShouldBlockObviousLivePaths()
    {
        var unsafeInterface = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                AisImportFolder = @"C:\MEDISTAR\Import"
            }
        };
        var catalog = CreateDefaultCatalog() with
        {
            InterfaceProfiles = new[] { unsafeInterface }
        };

        var result = _service.CreateForInterfaceProfile(catalog, unsafeInterface.Metadata.Id, Timestamp);

        Assert.False(result.Success);
        Assert.Null(result.Request);
        Assert.Contains(result.Messages, message => message.Contains("absoluten", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExportedSelectedPackage_ShouldNotContainLivePathsCustomerDataOrPatientData()
    {
        var result = _service.CreateForInterfaceProfile(
            CreateDefaultCatalog(),
            "interface-medistar-nidek-ark1s-default",
            Timestamp);
        var zipPath = CreateTempZipPath();
        _exporter.Export(zipPath, result.Request!);
        var archiveText = ReadAllZipText(zipPath);

        var forbiddenMarkers = new[]
        {
            @"C:\",
            @"C:\\",
            @"\\",
            "Praxis Dr.",
            "Kunde",
            "Mustermann",
            "MarcK",
            "M.Kurze",
            "11253",
            "4711"
        };
        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static ProfileCatalog CreateDefaultCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[]
            {
                DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
                DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
                DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCl300Default(),
                DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
                DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default()
            });
    }

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "templatepackage.zip");
    }

    private static string ReadAllZipText(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        return string.Join(
            Environment.NewLine,
            archive.Entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
                .OrderBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(ReadEntry));
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
