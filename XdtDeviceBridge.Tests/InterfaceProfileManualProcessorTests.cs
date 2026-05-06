using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileManualProcessorTests
{
    private readonly InterfaceProfileManualProcessor _processor = new();

    [Fact]
    public void Process_ShouldCreateExportFileFromGdtXmlAndExportProfile()
    {
        var exportFolder = CreateTempFolder();
        var interfaceProfile = CreateInterfaceProfile(exportFolder);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = _processor.Process(
            interfaceProfile,
            exportProfile,
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.True(File.Exists(result.ExportFilePath));
        Assert.NotNull(result.ExportContent);
        Assert.Contains("6310", result.ExportContent);
        Assert.Contains("PAT-100", result.ExportContent);
        Assert.Contains("R.:S=-1.25", result.ExportContent);
        Assert.StartsWith(exportFolder, result.ExportFilePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Process_ShouldReturnErrorForNonXmlDeviceFile()
    {
        var interfaceProfile = CreateInterfaceProfile(CreateTempFolder());
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
    }

    [Fact]
    public void Process_ShouldReturnErrorForMissingExportFolder()
    {
        var interfaceProfile = CreateInterfaceProfile(exportFolder: string.Empty);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            GetTestDataPath("nidek-ark1s-sample.xml"),
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Exportordner fehlt.", result.Messages);
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(string exportFolder)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-test",
                Name = "MEDISTAR + NIDEK ARK1S",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                ExportFolder = exportFolder
            },
            IsActive = true
        };
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetTestDataPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }
}
