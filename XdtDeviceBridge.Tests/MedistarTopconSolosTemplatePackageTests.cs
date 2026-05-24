using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarTopconSolosTemplatePackageTests
{
    private const string DefaultAisProfileId = "ais-medistar-default";
    private const string DefaultDeviceProfileId = "device-topcon-solos-default";
    private const string DefaultExportProfileId = "export-medistar-topcon-solos-default";
    private const string DefaultInterfaceProfileId = "interface-medistar-topcon-solos-default";

    private static readonly DateTimeOffset Timestamp = new(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Fact]
    public void Export_ShouldCreateTemplateCandidateWithExpectedZipStructure()
    {
        var zipPath = ExportTemplateCandidate();

        using var archive = ZipFile.OpenRead(zipPath);
        var entries = archive.Entries
            .Select(entry => entry.FullName)
            .OrderBy(entry => entry, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[]
            {
                $"ais/{DefaultAisProfileId}.json",
                $"devices/{DefaultDeviceProfileId}.json",
                $"exports/{DefaultExportProfileId}.json",
                $"interfaces/{DefaultInterfaceProfileId}.json",
                "package.json"
            },
            entries);
    }

    [Fact]
    public void Export_ShouldContainOnlySolosAndDependencies()
    {
        var importResult = _importer.Import(ExportTemplateCandidate());

        Assert.Equal("package-medistar-topcon-solos-v1", importResult.Package.Metadata.Id);
        Assert.Equal("MEDISTAR + TOPCON Solos Templatepaket V1", importResult.Package.Metadata.Name);
        Assert.Equal(DefaultAisProfileId, Assert.Single(importResult.AisProfiles).Metadata.Id);
        Assert.Equal(DefaultDeviceProfileId, Assert.Single(importResult.DeviceProfiles).Metadata.Id);
        Assert.Equal(DefaultExportProfileId, Assert.Single(importResult.ExportProfiles).Metadata.Id);

        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.Equal(DefaultInterfaceProfileId, interfaceProfile.Metadata.Id);
        Assert.Equal(DefaultAisProfileId, interfaceProfile.AisProfileId);
        Assert.Equal(DefaultDeviceProfileId, interfaceProfile.DeviceProfileId);
        Assert.Equal(DefaultExportProfileId, interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.False(interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Null(interfaceProfile.DeviceOutput);

        var includedIds = importResult.Package.IncludedProfiles.Select(profile => profile.Id).ToArray();
        Assert.Contains(DefaultAisProfileId, includedIds);
        Assert.Contains(DefaultDeviceProfileId, includedIds);
        Assert.Contains(DefaultExportProfileId, includedIds);
        Assert.Contains(DefaultInterfaceProfileId, includedIds);
        Assert.DoesNotContain(includedIds, id => id.Contains("ark1s", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("ar360", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("lm7", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("nt530", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("cl300", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("cv5000", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(includedIds, id => id.Contains("document", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Export_ShouldNotContainLivePathsCustomerDataOrPatientData()
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate());
        var forbiddenMarkers = new[]
        {
            @"C:\",
            @"C:\\",
            @"\\",
            "MarcK",
            "M.Kurze",
            "Kunde",
            "Praxis Dr.",
            "Mustermann",
            "11253",
            "4711"
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    private string ExportTemplateCandidate()
    {
        var zipPath = CreateTempZipPath();
        var result = _selectionExportService.CreateForInterfaceProfile(CreateDefaultCatalog(), DefaultInterfaceProfileId, Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal("medistar-topcon-solos-v1.templatepackage.zip", result.SuggestedFileName);
        _exporter.Export(zipPath, result.Request!);
        return zipPath;
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
                DefaultDeviceProfileDefinitions.CreateTopconSolosDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
                DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
                DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault(),
                DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCt1PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default(),
                DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
                DefaultExportProfileDefinitions.CreateMedistarManualDocumentTransferDefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530PDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconSolosDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk2PDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt1PDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault()
            });
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

    private static string CreateTempZipPath()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "medistar-topcon-solos-v1.templatepackage.zip");
    }
}
