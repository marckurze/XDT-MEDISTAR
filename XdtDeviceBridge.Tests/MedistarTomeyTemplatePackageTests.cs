using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarTomeyTemplatePackageTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Theory]
    [MemberData(nameof(TomeyTemplateCandidates))]
    public void Export_ShouldCreateTomeyTemplateCandidate(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        bool expectedSerialProfile)
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
        Assert.Null(interfaceProfile.DeviceOutput);

        if (expectedSerialProfile)
        {
            Assert.NotNull(interfaceProfile.SerialSettings);
            Assert.Equal(9600, interfaceProfile.SerialSettings!.BaudRate);
            Assert.Equal(8, interfaceProfile.SerialSettings.DataBits);
            Assert.Equal(SerialStopBitsSetting.One, interfaceProfile.SerialSettings.StopBits);
            Assert.Equal(SerialParitySetting.Odd, interfaceProfile.SerialSettings.Parity);
            Assert.False(interfaceProfile.SerialSettings.DtrEnable);
            Assert.False(interfaceProfile.SerialSettings.RtsEnable);
        }
        else
        {
            Assert.Null(interfaceProfile.SerialSettings);
        }
    }

    [Theory]
    [MemberData(nameof(TomeyTemplateCandidates))]
    public void Export_ShouldNotContainLivePathsCustomerDataOrForeignProductNames(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        bool expectedSerialProfile)
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate(interfaceProfileId));

        Assert.Contains(deviceProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(exportProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        if (expectedSerialProfile)
        {
            Assert.Contains("9600", archiveText, StringComparison.OrdinalIgnoreCase);
        }

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
            string.Concat("Middle", "ware"),
            string.Concat("Device", " ", "Connect"),
            string.Concat("team", "2", "work"),
            string.Concat("Compu", "Group")
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> TomeyTemplateCandidates()
    {
        yield return new object[] { "interface-medistar-tomey-cf2000-default", "device-tomey-cf2000-default", "export-medistar-tomey-cf2000-default", true };
        yield return new object[] { "interface-medistar-tomey-tl2000c-default", "device-tomey-tl2000c-default", "export-medistar-tomey-tl2000c-default", false };
        yield return new object[] { "interface-medistar-tomey-tl6000-default", "device-tomey-tl6000-default", "export-medistar-tomey-tl6000-default", false };
        yield return new object[] { "interface-medistar-tomey-tl7000-default", "device-tomey-tl7000-default", "export-medistar-tomey-tl7000-default", false };
        yield return new object[] { "interface-medistar-tomey-mr6000-default", "device-tomey-mr6000-default", "export-medistar-tomey-mr6000-default", false };
        yield return new object[] { "interface-medistar-tomey-top1000-default", "device-tomey-top1000-default", "export-medistar-tomey-top1000-default", false };
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
                DefaultDeviceProfileDefinitions.CreateTomeyCf2000Default(),
                DefaultDeviceProfileDefinitions.CreateTomeyTl2000CDefault(),
                DefaultDeviceProfileDefinitions.CreateTomeyTl6000Default(),
                DefaultDeviceProfileDefinitions.CreateTomeyTl7000Default(),
                DefaultDeviceProfileDefinitions.CreateTomeyMr6000Default(),
                DefaultDeviceProfileDefinitions.CreateTomeyTop1000Default()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarTomeyCf2000Default(),
                DefaultExportProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTomeyTl6000Default(),
                DefaultExportProfileDefinitions.CreateMedistarTomeyTl7000Default(),
                DefaultExportProfileDefinitions.CreateMedistarTomeyMr6000Default(),
                DefaultExportProfileDefinitions.CreateMedistarTomeyTop1000Default()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyCf2000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl6000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl7000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyMr6000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTop1000Default()
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
        return Path.Combine(folder, "medistar-tomey-v1.templatepackage.zip");
    }
}
