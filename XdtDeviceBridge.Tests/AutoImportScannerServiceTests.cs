using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AutoImportScannerServiceTests
{
    private static readonly TimeSpan StabilityDuration = TimeSpan.FromMilliseconds(20);

    private readonly AutoImportScannerService _scanner = new();

    [Fact]
    public async Task ScanOnceAsync_ShouldNotScanInactiveProfile()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        var profile = CreateProfile(folders, isActive: false);

        var result = await _scanner.ScanOnceAsync(profile, StabilityDuration);

        Assert.Equal("interface-test", result.InterfaceProfileId);
        Assert.Equal(0, result.FilesQueued);
        Assert.Empty(result.Queue.GetAll());
        Assert.Contains("Schnittstellenprofil ist nicht aktiv.", result.Messages);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldReportMissingAisImportFolder()
    {
        var folders = CreateImportFolders();
        var profile = CreateProfile(folders) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: string.Empty,
                deviceImportFolder: folders.DeviceFolder)
        };

        var result = await _scanner.ScanOnceAsync(profile, StabilityDuration);

        Assert.Contains("AIS-Importordner fehlt.", result.Messages);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldReportMissingDeviceImportFolder()
    {
        var folders = CreateImportFolders();
        var profile = CreateProfile(folders) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: folders.AisFolder,
                deviceImportFolder: string.Empty)
        };

        var result = await _scanner.ScanOnceAsync(profile, StabilityDuration);

        Assert.Contains("Geräte-Importordner fehlt.", result.Messages);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldQueueStableGdtAndXmlFiles()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "device.xml"), "<xml />");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Equal(1, result.AisFilesDetected);
        Assert.Equal(1, result.DeviceFilesDetected);
        Assert.Equal(2, result.FilesQueued);
        Assert.Equal(2, result.Queue.GetAll().Count);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldCreateReadyPairForStableGdtAndXmlFiles()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "device.xml"), "<xml />");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        var pair = Assert.Single(result.Queue.FindReadyPairs());
        Assert.Equal(1, result.ReadyPairs);
        Assert.Equal(ImportFileKind.AisGdt, pair.AisFile.Kind);
        Assert.Equal(ImportFileKind.DeviceXml, pair.DeviceFile.Kind);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldIgnoreAttachments()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "image.jpg"), "image");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "document.pdf"), "pdf");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Equal(1, result.AisFilesDetected);
        Assert.Equal(0, result.DeviceFilesDetected);
        Assert.Equal(1, result.FilesQueued);
        Assert.Empty(result.Queue.GetByKind(ImportFileKind.AttachmentImage));
        Assert.Empty(result.Queue.GetByKind(ImportFileKind.AttachmentPdf));
    }

    [Fact]
    public async Task ScanOnceAsync_AttachmentOnlyModeShouldQueueSupportedDocumentsAsDeviceInput()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "bericht.pdf"), "pdf");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "bild.jpg"), "image");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "dicom.dcm"), "dcm");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "video.mp4"), "video");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "anhaenge.xml"), "<root />");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "hinweis.txt"), "text");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders, isAttachmentOnlyMode: true), StabilityDuration);

        Assert.Equal(1, result.AisFilesDetected);
        Assert.Equal(6, result.DeviceFilesDetected);
        Assert.Equal(7, result.FilesQueued);
        Assert.Equal(1, result.ReadyPairs);
        Assert.Single(result.Queue.GetByKind(ImportFileKind.AttachmentPdf));
        Assert.Single(result.Queue.GetByKind(ImportFileKind.AttachmentImage));
        Assert.Equal(2, result.Queue.GetByKind(ImportFileKind.AttachmentFile).Count);
        Assert.Single(result.Queue.GetByKind(ImportFileKind.DeviceXml));
        Assert.Single(result.Queue.GetByKind(ImportFileKind.DeviceText));
    }

    [Fact]
    public async Task ScanOnceAsync_ManualDocumentSelectionShouldCreateReadyPairFromAisOnly()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "ignored.pdf"), "pdf");
        var profile = CreateProfile(
            folders,
            isAttachmentOnlyMode: true,
            attachmentOnlySourceMode: AttachmentOnlySourceMode.ManualUserSelection) with
        {
            FolderOptions = CreateFolderOptions(folders.AisFolder, string.Empty) with
            {
                IsAttachmentOnlyMode = true,
                AttachmentOnlySourceMode = AttachmentOnlySourceMode.ManualUserSelection
            }
        };

        var result = await _scanner.ScanOnceAsync(profile, StabilityDuration);

        Assert.Equal(1, result.AisFilesDetected);
        Assert.Equal(0, result.DeviceFilesDetected);
        Assert.Equal(1, result.FilesQueued);
        Assert.Equal(1, result.ReadyPairs);
        Assert.Contains(result.Messages, message => message.Contains("keine Geräte-Datei", StringComparison.OrdinalIgnoreCase));
        var pair = Assert.Single(result.Queue.FindReadyPairs(
            includeAttachmentDeviceFiles: true,
            allowAisOnlyManualSelection: true));
        Assert.Equal("Manuelle Dokumentauswahl", pair.DeviceFile.FileName);
    }

    [Fact]
    public async Task ScanOnceAsync_ManualDocumentSelectionShouldStayIdleWithoutAisFile()
    {
        var folders = CreateImportFolders();
        var profile = CreateProfile(
            folders,
            isAttachmentOnlyMode: true,
            attachmentOnlySourceMode: AttachmentOnlySourceMode.ManualUserSelection) with
        {
            FolderOptions = CreateFolderOptions(folders.AisFolder, string.Empty) with
            {
                IsAttachmentOnlyMode = true,
                AttachmentOnlySourceMode = AttachmentOnlySourceMode.ManualUserSelection
            }
        };

        var result = await _scanner.ScanOnceAsync(profile, StabilityDuration);

        Assert.Equal(0, result.AisFilesDetected);
        Assert.Equal(0, result.DeviceFilesDetected);
        Assert.Equal(0, result.FilesQueued);
        Assert.Equal(0, result.ReadyPairs);
        Assert.DoesNotContain(result.Messages, message => message.Contains("keine Geräte-Datei", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldIgnoreIrrelevantExtensions()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "note.dat"), "dat");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "device.bin"), "bin");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Equal(0, result.AisFilesDetected);
        Assert.Equal(0, result.DeviceFilesDetected);
        Assert.Equal(0, result.FilesQueued);
        Assert.Empty(result.Queue.GetAll());
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldReportMissingDirectoriesWithoutThrowing()
    {
        var baseFolder = CreateTempDirectoryPath();
        var folders = new ImportFolders(
            AisFolder: Path.Combine(baseFolder, "ais"),
            DeviceFolder: Path.Combine(baseFolder, "device"));

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Contains(result.Messages, message => message.Contains("AIS-Importordner existiert nicht", StringComparison.Ordinal));
        Assert.Contains(result.Messages, message => message.Contains("Geräte-Importordner existiert nicht", StringComparison.Ordinal));
        Assert.Equal(0, result.FilesQueued);
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldQueueOnlyStableFiles()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.gdt"), "gdt");
        var lockedDeviceFile = Path.Combine(folders.DeviceFolder, "device.xml");
        File.WriteAllText(lockedDeviceFile, "<xml />");

        await using var stream = new FileStream(lockedDeviceFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Equal(1, result.AisFilesDetected);
        Assert.Equal(1, result.DeviceFilesDetected);
        Assert.Equal(1, result.FilesQueued);
        Assert.Single(result.Queue.GetByKind(ImportFileKind.AisGdt));
        Assert.Empty(result.Queue.GetByKind(ImportFileKind.DeviceXml));
        Assert.Contains(result.Messages, message => message.Contains("device.xml", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ScanOnceAsync_ShouldQueueCorrectImportFileKinds()
    {
        var folders = CreateImportFolders();
        File.WriteAllText(Path.Combine(folders.AisFolder, "patient.xdt"), "xdt");
        File.WriteAllText(Path.Combine(folders.DeviceFolder, "device.csv"), "a;b");

        var result = await _scanner.ScanOnceAsync(CreateProfile(folders), StabilityDuration);

        Assert.Single(result.Queue.GetByKind(ImportFileKind.AisXdt));
        Assert.Single(result.Queue.GetByKind(ImportFileKind.DeviceCsv));
    }

    private static InterfaceProfileDefinition CreateProfile(
        ImportFolders folders,
        bool isActive = true,
        bool isAttachmentOnlyMode = false,
        AttachmentOnlySourceMode attachmentOnlySourceMode = AttachmentOnlySourceMode.DeviceFolder)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-test",
                Name = "Test-Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = CreateFolderOptions(folders.AisFolder, folders.DeviceFolder) with
            {
                IsAttachmentOnlyMode = isAttachmentOnlyMode,
                AttachmentOnlySourceMode = attachmentOnlySourceMode
            },
            IsActive = isActive,
            IsLicenseRequired = true
        };
    }

    private static InterfaceFolderOptions CreateFolderOptions(
        string aisImportFolder,
        string deviceImportFolder)
    {
        return new InterfaceFolderOptions(
            AisImportFolder: aisImportFolder,
            DeviceImportFolder: deviceImportFolder,
            ExportFolder: string.Empty,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: false,
            ClearDeviceImportFolderBeforeProcessing: false,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: true);
    }

    private static ImportFolders CreateImportFolders()
    {
        var baseFolder = CreateTempDirectoryPath();
        var folders = new ImportFolders(
            AisFolder: Path.Combine(baseFolder, "ais"),
            DeviceFolder: Path.Combine(baseFolder, "device"));
        Directory.CreateDirectory(folders.AisFolder);
        Directory.CreateDirectory(folders.DeviceFolder);
        return folders;
    }

    private static string CreateTempDirectoryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "XdtDeviceBridgeTests",
            Guid.NewGuid().ToString("N"));
    }

    private sealed record ImportFolders(string AisFolder, string DeviceFolder);
}
