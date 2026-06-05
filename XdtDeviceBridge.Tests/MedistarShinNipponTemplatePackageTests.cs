using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarShinNipponTemplatePackageTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Theory]
    [MemberData(nameof(ShinNipponTemplateCandidates))]
    public void Export_ShouldCreateShinNipponTemplateCandidate(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        int expectedBaudRate,
        int expectedReadTimeoutMilliseconds)
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
        Assert.NotNull(interfaceProfile.SerialSettings);
        Assert.Equal(expectedBaudRate, interfaceProfile.SerialSettings!.BaudRate);
        Assert.Equal(8, interfaceProfile.SerialSettings.DataBits);
        Assert.Equal(SerialStopBitsSetting.One, interfaceProfile.SerialSettings.StopBits);
        Assert.Equal(SerialParitySetting.None, interfaceProfile.SerialSettings.Parity);
        Assert.Equal(SerialHandshakeSetting.None, interfaceProfile.SerialSettings.Handshake);
        Assert.False(interfaceProfile.SerialSettings.DtrEnable);
        Assert.False(interfaceProfile.SerialSettings.RtsEnable);
        Assert.False(interfaceProfile.SerialSettings.IsBidirectional);
        Assert.Equal(expectedReadTimeoutMilliseconds, interfaceProfile.SerialSettings.ReadTimeoutMilliseconds);
    }

    [Theory]
    [MemberData(nameof(ShinNipponTemplateCandidates))]
    public void Export_ShouldNotContainLivePathsCustomerDataOrReferencePackageNames(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        int expectedBaudRate,
        int expectedReadTimeoutMilliseconds)
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate(interfaceProfileId));

        Assert.Contains(deviceProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(exportProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedBaudRate.ToString(), archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedReadTimeoutMilliseconds.ToString(), archiveText, StringComparison.OrdinalIgnoreCase);

        var forbiddenMarkers = new[]
        {
            @"C:\",
            @"C:\\",
            @"\\",
            string.Concat("Mar", "cK"),
            "M.Kurze",
            "Kunde",
            "Praxis Dr.",
            "Mustermann",
            "11253",
            "4711",
            "4701-1",
            "Testfrau",
            "Anna",
            string.Concat("Kai", " ", "Zirle", "wagen"),
            string.Concat("Zirle", "wagen"),
            string.Concat("T", "2", "W"),
            string.Concat("Team", "2", "Work")
        };

        foreach (var marker in forbiddenMarkers)
        {
            Assert.DoesNotContain(marker, archiveText, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> ShinNipponTemplateCandidates()
    {
        yield return new object[]
        {
            "interface-medistar-shin-nippon-accuref-r800-default",
            "device-shin-nippon-accuref-r800-default",
            "export-medistar-shin-nippon-accuref-r800-default",
            115200,
            5000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-accuref-k900-default",
            "device-shin-nippon-accuref-k900-default",
            "export-medistar-shin-nippon-accuref-k900-default",
            115200,
            5000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-dl1000-default",
            "device-shin-nippon-dl1000-default",
            "export-medistar-shin-nippon-dl1000-default",
            9600,
            5000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-dl800-default",
            "device-shin-nippon-dl800-default",
            "export-medistar-shin-nippon-dl800-default",
            9600,
            5000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-dl900-default",
            "device-shin-nippon-dl900-default",
            "export-medistar-shin-nippon-dl900-default",
            9600,
            5000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-nct200-default",
            "device-shin-nippon-nct200-default",
            "export-medistar-shin-nippon-nct200-default",
            19200,
            30000
        };
        yield return new object[]
        {
            "interface-medistar-shin-nippon-slm4000-default",
            "device-shin-nippon-slm4000-default",
            "export-medistar-shin-nippon-slm4000-default",
            9600,
            5000
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
                DefaultDeviceProfileDefinitions.CreateShinNipponAccurefR800Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponAccurefK900Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponDl1000Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponDl800Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponDl900Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponNct200Default(),
                DefaultDeviceProfileDefinitions.CreateShinNipponSlm4000Default()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefR800Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponDl1000Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponDl800Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponDl900Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponNct200Default(),
                DefaultExportProfileDefinitions.CreateMedistarShinNipponSlm4000Default()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponAccurefR800Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl1000Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl800Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl900Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponNct200Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponSlm4000Default()
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
        return Path.Combine(folder, "medistar-shin-nippon-v1.templatepackage.zip");
    }
}
