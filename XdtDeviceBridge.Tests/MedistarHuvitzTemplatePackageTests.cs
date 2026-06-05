using System.IO.Compression;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarHuvitzTemplatePackageTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly TemplatePackageExportSelectionService _selectionExportService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();

    [Theory]
    [MemberData(nameof(HuvitzTemplateCandidates))]
    public void Export_ShouldCreateHuvitzTemplateCandidate(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        int expectedBaudRate)
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
        Assert.False(interfaceProfile.SerialSettings.DtrEnable);
        Assert.False(interfaceProfile.SerialSettings.RtsEnable);
        Assert.False(interfaceProfile.SerialSettings.IsBidirectional);
    }

    [Theory]
    [MemberData(nameof(HuvitzTemplateCandidates))]
    public void Export_ShouldNotContainLivePathsCustomerDataOrReferencePackageNames(
        string interfaceProfileId,
        string deviceProfileId,
        string exportProfileId,
        int expectedBaudRate)
    {
        var archiveText = ReadAllZipText(ExportTemplateCandidate(interfaceProfileId));

        Assert.Contains(deviceProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(exportProfileId, archiveText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedBaudRate.ToString(), archiveText, StringComparison.OrdinalIgnoreCase);

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

    public static IEnumerable<object[]> HuvitzTemplateCandidates()
    {
        yield return new object[]
        {
            "interface-medistar-huvitz-hrk8000a-default",
            "device-huvitz-hrk8000a-default",
            "export-medistar-huvitz-hrk8000a-default",
            9600
        };
        yield return new object[]
        {
            "interface-medistar-huvitz-hrk9000a-default",
            "device-huvitz-hrk9000a-default",
            "export-medistar-huvitz-hrk9000a-default",
            9600
        };
        yield return new object[]
        {
            "interface-medistar-huvitz-hnt1p-default",
            "device-huvitz-hnt1p-default",
            "export-medistar-huvitz-hnt1p-default",
            115200
        };
        yield return new object[]
        {
            "interface-medistar-huvitz-htr1a-default",
            "device-huvitz-htr1a-default",
            "export-medistar-huvitz-htr1a-default",
            9600
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
                DefaultDeviceProfileDefinitions.CreateHuvitzHrk8000ADefault(),
                DefaultDeviceProfileDefinitions.CreateHuvitzHrk9000ADefault(),
                DefaultDeviceProfileDefinitions.CreateHuvitzHnt1PDefault(),
                DefaultDeviceProfileDefinitions.CreateHuvitzHtr1ADefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarHuvitzHrk8000ADefault(),
                DefaultExportProfileDefinitions.CreateMedistarHuvitzHrk9000ADefault(),
                DefaultExportProfileDefinitions.CreateMedistarHuvitzHnt1PDefault(),
                DefaultExportProfileDefinitions.CreateMedistarHuvitzHtr1ADefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHrk8000ADefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHrk9000ADefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHnt1PDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHtr1ADefault()
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
        return Path.Combine(folder, "medistar-huvitz-v1.templatepackage.zip");
    }
}
