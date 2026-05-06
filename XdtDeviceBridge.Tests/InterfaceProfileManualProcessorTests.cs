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
        Assert.Null(result.ArchiveResult);
        Assert.Contains("Archivierung ist für dieses Schnittstellenprofil deaktiviert.", result.Messages);
    }

    [Fact]
    public void Process_ShouldReturnErrorForNonXmlDeviceFile()
    {
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: CreateTempFolder(),
            moveFailedFilesToErrorFolder: false);
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
        Assert.Null(result.FailedFileCopyResult);
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

    [Fact]
    public void Process_ShouldArchiveFilesWhenArchiveProcessedFilesIsEnabled()
    {
        var exportFolder = CreateTempFolder();
        var archiveFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.NotNull(result.ArchiveResult);
        Assert.False(result.ArchiveResult.HasErrors);
        Assert.Equal(2, result.ArchiveResult.ArchivedFiles.Count);
        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
        Assert.Contains("Importdateien wurden archiviert:", result.Messages);
    }

    [Fact]
    public void Process_ShouldKeepExportFileWhenArchivingFails()
    {
        var exportFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = CopyTestDataToTemp("nidek-ark1s-sample.xml", "device.xml");
        var interfaceProfile = CreateInterfaceProfile(
            exportFolder,
            archiveFolder: string.Empty,
            archiveProcessedFiles: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.True(result.Success);
        Assert.NotNull(result.ExportFilePath);
        Assert.True(File.Exists(result.ExportFilePath));
        Assert.NotNull(result.ArchiveResult);
        Assert.True(result.ArchiveResult.HasErrors);
        Assert.Contains("Archivierung fehlgeschlagen: Archivordner fehlt.", result.Messages);
    }

    [Fact]
    public void Process_ShouldCopyFailedFilesWhenErrorCopyIsEnabled()
    {
        var errorFolder = CreateTempFolder();
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: errorFolder,
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            new DateTime(2026, 6, 1, 12, 0, 0));

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
        Assert.NotNull(result.FailedFileCopyResult);
        Assert.False(result.FailedFileCopyResult.HasErrors);
        Assert.Equal(3, result.FailedFileCopyResult.CopiedFiles.Count);
        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
        Assert.Contains("Fehlerhafte Importdateien wurden in den Fehlerordner kopiert; Originale bleiben erhalten:", result.Messages);
    }

    [Fact]
    public void Process_ShouldReportMissingErrorFolderWhenErrorCopyIsEnabled()
    {
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: string.Empty,
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            GetTestDataPath("sample-gdt-utf8.gdt"),
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.Contains("Dieser Dateityp wird für die manuelle Paarverarbeitung noch nicht unterstützt.", result.Messages);
        Assert.NotNull(result.FailedFileCopyResult);
        Assert.True(result.FailedFileCopyResult.HasErrors);
        Assert.Contains("Fehlerordner ist nicht konfiguriert.", result.Messages);
    }

    [Fact]
    public void Process_ShouldKeepOriginalFilesWhenErrorCopyIsEnabled()
    {
        var aisFilePath = CopyTestDataToTemp("sample-gdt-utf8.gdt", "patient.gdt");
        var deviceFilePath = Path.Combine(CreateTempFolder(), "device.txt");
        File.WriteAllText(deviceFilePath, "device");
        var interfaceProfile = CreateInterfaceProfile(
            CreateTempFolder(),
            errorFolder: CreateTempFolder(),
            moveFailedFilesToErrorFolder: true);

        var result = _processor.Process(
            interfaceProfile,
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            aisFilePath,
            deviceFilePath,
            DateTime.UtcNow);

        Assert.False(result.Success);
        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string exportFolder,
        string archiveFolder = "",
        bool archiveProcessedFiles = false,
        string errorFolder = "",
        bool moveFailedFilesToErrorFolder = true)
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
                ExportFolder = exportFolder,
                ArchiveFolder = archiveFolder,
                ArchiveProcessedFiles = archiveProcessedFiles,
                ErrorFolder = errorFolder,
                MoveFailedFilesToErrorFolder = moveFailedFilesToErrorFolder
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

    private static string CopyTestDataToTemp(string testDataFileName, string targetFileName)
    {
        var folder = CreateTempFolder();
        var targetFilePath = Path.Combine(folder, targetFileName);
        File.Copy(GetTestDataPath(testDataFileName), targetFilePath);
        return targetFilePath;
    }
}
