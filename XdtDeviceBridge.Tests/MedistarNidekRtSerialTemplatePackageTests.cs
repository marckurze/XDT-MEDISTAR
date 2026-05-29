using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarNidekRtSerialTemplatePackageTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Theory]
    [MemberData(nameof(RtSerialTemplateCandidates))]
    public void Export_ShouldCreateRtSerialTemplateCandidate(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        string expectedSlugPart)
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

        Assert.Contains(expectedSlugPart, importResult.Package.Metadata.Id, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("ais-medistar-default", Assert.Single(importResult.AisProfiles).Metadata.Id);
        Assert.Equal(deviceProfileId, Assert.Single(importResult.DeviceProfiles).Metadata.Id);
        Assert.Equal(exportProfileId, Assert.Single(importResult.ExportProfiles).Metadata.Id);

        var interfaceProfile = Assert.Single(importResult.InterfaceProfiles);
        Assert.Equal(interfaceProfileId, interfaceProfile.Metadata.Id);
        Assert.Equal(deviceProfileId, interfaceProfile.DeviceProfileId);
        Assert.Equal(exportProfileId, interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.NotNull(interfaceProfile.DeviceOutput);
        Assert.False(interfaceProfile.DeviceOutput!.IsEnabled);
        Assert.Equal(NidekRtSerialPhoropterOutputWriter.DeviceOutputFormat, interfaceProfile.DeviceOutput.Format);
        Assert.NotNull(interfaceProfile.SerialSettings);
        Assert.True(interfaceProfile.SerialSettings!.DtrEnable);
        Assert.True(interfaceProfile.SerialSettings.RtsEnable);
    }

    [Theory]
    [MemberData(nameof(RtSerialTemplateCandidates))]
    public void Export_ShouldNotContainLivePathsCustomerDataOrPatientData(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        string expectedSlugPart)
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate(interfaceProfileId));
        Assert.Contains(deviceProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(exportProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedSlugPart, archiveText, StringComparison.OrdinalIgnoreCase);
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
            "Anna"
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> RtSerialTemplateCandidates()
    {
        yield return new object[]
        {
            "interface-medistar-nidek-rt2100-serial-default",
            "device-nidek-rt2100-serial-default",
            "export-medistar-nidek-rt2100-serial-default",
            "rt-2100"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-rt3100-serial-default",
            "device-nidek-rt3100-serial-default",
            "export-medistar-nidek-rt3100-serial-default",
            "rt-3100"
        };
        yield return new object[]
        {
            "interface-medistar-nidek-rt5100-serial-default",
            "device-nidek-rt5100-serial-default",
            "export-medistar-nidek-rt5100-serial-default",
            "rt-5100"
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
                DefaultDeviceProfileDefinitions.CreateNidekRt2100SerialDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekRt5100SerialDefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarNidekRt2100SerialDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekRt5100SerialDefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt2100SerialDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt5100SerialDefault()
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
        return Path.Combine(folder, "medistar-nidek-rt-serial-v1.templatepackage.zip");
    }
}
