using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarNidekReferenceBatch6TemplatePackageTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Theory]
    [MemberData(nameof(NidekBatch6TemplateCandidates))]
    public void Export_ShouldCreateNidekBatch6TemplateCandidate(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId)
    {
        var zipPath = ExportTemplateCandidate(interfaceProfileId);

        using var archive = ZipFile.OpenRead(zipPath);
        var entries = archive.Entries
            .Select(entry => entry.FullName)
            .OrderBy(entry => entry, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "ais/ais-medistar-default.json",
                $"devices/{deviceProfileId}.json",
                $"exports/{exportProfileId}.json",
                $"interfaces/{interfaceProfileId}.json",
                "package.json"
            },
            entries);

        var importResult = _importer.Import(zipPath);

        Assert.Equal("ais-medistar-default", Assert.Single(importResult.AisProfiles).Metadata.Id);
        Assert.Equal(deviceProfileId, Assert.Single(importResult.DeviceProfiles).Metadata.Id);
        Assert.Equal(exportProfileId, Assert.Single(importResult.ExportProfiles).Metadata.Id);

        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.Equal(interfaceProfileId, interfaceProfile.Metadata.Id);
        Assert.Equal(deviceProfileId, interfaceProfile.DeviceProfileId);
        Assert.Equal(exportProfileId, interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.Null(interfaceProfile.SerialSettings);
        Assert.Null(interfaceProfile.DeviceOutput);
        Assert.False(interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Theory]
    [MemberData(nameof(NidekBatch6TemplateCandidates))]
    public void Export_ShouldNotContainLivePathsCustomerDataOrReferenceArtifacts(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId)
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate(interfaceProfileId));

        Assert.Contains(deviceProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(exportProfileId, archiveText, StringComparison.OrdinalIgnoreCase);

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
            "4711",
            "4701-1",
            "Testfrau",
            "Anna",
            ".exe",
            ".pdf"
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> NidekBatch6TemplateCandidates()
    {
        yield return new object[]
        {
            "interface-medistar-nidek-ar1-default",
            "device-nidek-ar1-default",
            "export-medistar-nidek-ar1-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-ar1s-default",
            "device-nidek-ar1s-default",
            "export-medistar-nidek-ar1s-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-ar310a-default",
            "device-nidek-ar310a-default",
            "export-medistar-nidek-ar310a-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-lm1800pd-default",
            "device-nidek-lm1800pd-default",
            "export-medistar-nidek-lm1800pd-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-nt1-default",
            "device-nidek-nt1-default",
            "export-medistar-nidek-nt1-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-nt1e-default",
            "device-nidek-nt1e-default",
            "export-medistar-nidek-nt1e-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-nt1p-default",
            "device-nidek-nt1p-default",
            "export-medistar-nidek-nt1p-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-nt510-default",
            "device-nidek-nt510-default",
            "export-medistar-nidek-nt510-default"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-nt530-default",
            "device-nidek-nt530-default",
            "export-medistar-nidek-nt530-default"
        };
    }

    private string ExportTemplateCandidate(string interfaceProfileId)
    {
        var zipPath = CreateTempZipPath();
        var result = _selectionExportService.CreateForInterfaceProfile(CreateCatalog(), interfaceProfileId, Timestamp);

        Assert.True(result.Success, result.ErrorMessage);
        _exporter.Export(zipPath, result.Request!);
        return zipPath;
    }

    private static ProfileCatalog CreateCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[]
            {
                DefaultDeviceProfileDefinitions.CreateNidekAr1Default(),
                DefaultDeviceProfileDefinitions.CreateNidekAr1SDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekAr310ADefault(),
                DefaultDeviceProfileDefinitions.CreateNidekLm1800PdDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekNt1Default(),
                DefaultDeviceProfileDefinitions.CreateNidekNt1EDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekNt1PDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekNt510Default(),
                DefaultDeviceProfileDefinitions.CreateNidekNt530Default()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarNidekAr1Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekAr1SDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekAr310ADefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekLm1800PdDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt1Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt1EDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt1PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt510Default(),
                DefaultExportProfileDefinitions.CreateMedistarNidekNt530Default()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr1Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr1SDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr310ADefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm1800PdDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt1Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt1EDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt1PDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt510Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530Default()
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
        return Path.Combine(folder, "candidate.templatepackage.zip");
    }
}
