using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AutoImportPairProcessingCoordinatorTests
{
    private static readonly DateTime Timestamp = new(2026, 6, 1, 12, 0, 0);

    [Fact]
    public void ProcessReadyPairs_ShouldNotProcessWhenAutomaticProcessingIsDisabled()
    {
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: false,
            Timestamp);

        Assert.Equal(0, processor.CallCount);
        Assert.Equal(0, result.ProcessedCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldProcessReadyPairWhenAutomaticProcessingIsEnabled()
    {
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(1, result.ProcessedCount);
        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal("Automatisch verarbeitet", processed.Status);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldNotProcessSamePairTwice()
    {
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var profile = CreateInterfaceProfile();
        var exportProfile = CreateExportProfile();
        var pair = CreatePair("patient.gdt", "device.xml");

        coordinator.ProcessReadyPairs(profile, exportProfile, new[] { pair }, true, Timestamp);
        var second = coordinator.ProcessReadyPairs(profile, exportProfile, new[] { pair }, true, Timestamp);

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(1, second.SkippedAlreadyProcessedCount);
        var skipped = Assert.Single(second.Results);
        Assert.True(skipped.WasSkipped);
        Assert.Equal("Bereits verarbeitet - bleibt im Importordner", skipped.Status);
        Assert.Contains("Paar wurde bereits verarbeitet und nicht erneut exportiert.", skipped.Messages);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldProcessDifferentPairs()
    {
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[]
            {
                CreatePair("patient-1.gdt", "device-1.xml"),
                CreatePair("patient-2.gdt", "device-2.xml")
            },
            automaticProcessingEnabled: true,
            Timestamp);

        Assert.Equal(2, processor.CallCount);
        Assert.Equal(2, result.ProcessedCount);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldReturnErrorStatusWhenManualProcessingFails()
    {
        var processor = new FakeManualProcessor(CreateFailureResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.txt") },
            automaticProcessingEnabled: true,
            Timestamp);

        Assert.Equal(1, result.ErrorCount);
        var failed = Assert.Single(result.Results);
        Assert.False(failed.Success);
        Assert.Equal("Automatischer Fehler", failed.Status);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldCarryArchiveStatusFromManualProcessor()
    {
        var processor = new FakeManualProcessor(CreateSuccessResult(new ProcessedFileArchiveResult(
            ArchivedFiles: new[] { "archive\\patient.gdt", "archive\\device.xml" },
            Issues: Array.Empty<string>(),
            HasErrors: false)));
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.Equal("Automatisch verarbeitet und archiviert", processed.Status);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldCarryFailedFileCopyStatusFromManualProcessor()
    {
        var processor = new FakeManualProcessor(CreateFailureResult(new FailedFileCopyResult(
            CopiedFiles: new[] { "error\\patient.gdt", "error\\device.txt" },
            Issues: Array.Empty<string>(),
            HasErrors: false)));
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.txt") },
            automaticProcessingEnabled: true,
            Timestamp);

        var failed = Assert.Single(result.Results);
        Assert.Equal("Automatischer Fehler, Dateien kopiert", failed.Status);
    }

    [Fact]
    public void ProcessReadyPairs_ShouldNotDeleteOrMoveImportFiles()
    {
        var folder = CreateTempFolder();
        var aisFilePath = Path.Combine(folder, "patient.gdt");
        var deviceFilePath = Path.Combine(folder, "device.xml");
        File.WriteAllText(aisFilePath, "gdt");
        File.WriteAllText(deviceFilePath, "xml");
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);

        coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(),
            CreateExportProfile(),
            new[] { CreatePair(aisFilePath, deviceFilePath) },
            automaticProcessingEnabled: true,
            Timestamp);

        Assert.True(File.Exists(aisFilePath));
        Assert.True(File.Exists(deviceFilePath));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateWithoutArchiveShouldKeepFilesInImportFolder()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var profile = CreateInterfaceProfile(
            archiveProcessedFiles: false,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        var result = Assert.Single(duplicate.Results);
        Assert.Equal("Bereits verarbeitet - bleibt im Importordner", result.Status);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateWithMoveArchiveShouldMoveFiles()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var archiveFolder = CreateTempFolder();
        var profile = CreateInterfaceProfile(
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        var result = Assert.Single(duplicate.Results);
        Assert.Equal("Bereits verarbeitet - ins Archiv verschoben", result.Status);
        Assert.False(File.Exists(files.AisFilePath));
        Assert.False(File.Exists(files.DeviceFilePath));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "AIS", "patient.gdt")));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "Device", "device.xml")));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateWithCopyArchiveShouldCopyFilesAndKeepOriginals()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var archiveFolder = CreateTempFolder();
        var profile = CreateInterfaceProfile(
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Copy,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        var result = Assert.Single(duplicate.Results);
        Assert.Equal("Bereits verarbeitet - ins Archiv kopiert", result.Status);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "AIS", "patient.gdt")));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "Device", "device.xml")));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateShouldArchiveOnlyAisFileWhenAisOptionIsEnabled()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var archiveFolder = CreateTempFolder();
        var profile = CreateInterfaceProfile(
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: false);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        Assert.Equal("Bereits verarbeitet - ins Archiv verschoben", Assert.Single(duplicate.Results).Status);
        Assert.False(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "AIS", "patient.gdt")));
        Assert.False(Directory.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "Device")));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateShouldArchiveOnlyDeviceFileWhenDeviceOptionIsEnabled()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var archiveFolder = CreateTempFolder();
        var profile = CreateInterfaceProfile(
            archiveFolder: archiveFolder,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: false,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        Assert.Equal("Bereits verarbeitet - ins Archiv verschoben", Assert.Single(duplicate.Results).Status);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.False(File.Exists(files.DeviceFilePath));
        Assert.False(Directory.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "AIS")));
        Assert.True(File.Exists(Path.Combine(archiveFolder, "2026", "06", "01", "MEDISTAR_NIDEK_ARK1S", "Device", "device.xml")));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateWithRemoveOptionsDisabledShouldMoveNoFiles()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var profile = CreateInterfaceProfile(
            archiveFolder: CreateTempFolder(),
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: false,
            clearDeviceImportFolderBeforeProcessing: false);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        Assert.Equal("Bereits verarbeitet - bleibt im Importordner", Assert.Single(duplicate.Results).Status);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateArchiveFailureShouldReportStatus()
    {
        var files = CreateTempImportFiles();
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var profile = CreateInterfaceProfile(
            archiveFolder: string.Empty,
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        var duplicate = coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        var result = Assert.Single(duplicate.Results);
        Assert.Equal("Bereits verarbeitet - Archivierung fehlgeschlagen", result.Status);
        Assert.Contains("Archivierung bereits verarbeiteter Dateien fehlgeschlagen: Archivordner fehlt.", result.Messages);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
    }

    [Fact]
    public void ProcessReadyPairs_DuplicateShouldNotMoveUnknownFiles()
    {
        var files = CreateTempImportFiles();
        var unknownFile = Path.Combine(files.Folder, "unknown.xml");
        File.WriteAllText(unknownFile, "unknown");
        var processor = new FakeManualProcessor(CreateSuccessResult());
        var coordinator = new AutoImportPairProcessingCoordinator(processor);
        var profile = CreateInterfaceProfile(
            archiveFolder: CreateTempFolder(),
            archiveProcessedFiles: true,
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            clearAisImportFolderBeforeProcessing: true,
            clearDeviceImportFolderBeforeProcessing: true);
        var pair = CreatePair(files.AisFilePath, files.DeviceFilePath);

        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);
        coordinator.ProcessReadyPairs(profile, CreateExportProfile(), new[] { pair }, true, Timestamp);

        Assert.True(File.Exists(unknownFile));
    }

    private static PendingImportPair CreatePair(string aisFilePath, string deviceFilePath)
    {
        return new PendingImportPair(
            AisFile: new PendingImportFile(
                FilePath: aisFilePath,
                FileName: Path.GetFileName(aisFilePath),
                Kind: ImportFileKind.AisGdt,
                Status: PendingImportFileStatus.Stable,
                DetectedAtUtc: Timestamp,
                StableAtUtc: Timestamp,
                Message: null),
            DeviceFile: new PendingImportFile(
                FilePath: deviceFilePath,
                FileName: Path.GetFileName(deviceFilePath),
                Kind: ImportFileKind.DeviceXml,
                Status: PendingImportFileStatus.Stable,
                DetectedAtUtc: Timestamp,
                StableAtUtc: Timestamp,
                Message: null),
            IsReady: true);
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string archiveFolder = "",
        bool archiveProcessedFiles = false,
        ArchiveProcessedFileMode archiveProcessedFileMode = ArchiveProcessedFileMode.Copy,
        bool clearAisImportFolderBeforeProcessing = false,
        bool clearDeviceImportFolderBeforeProcessing = false)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Name = "MEDISTAR + NIDEK ARK1S"
            },
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                ArchiveFolder = archiveFolder,
                ArchiveProcessedFiles = archiveProcessedFiles,
                ArchiveProcessedFileMode = archiveProcessedFileMode,
                ClearAisImportFolderBeforeProcessing = clearAisImportFolderBeforeProcessing,
                ClearDeviceImportFolderBeforeProcessing = clearDeviceImportFolderBeforeProcessing
            },
            IsActive = true
        };
    }

    private static ExportProfileDefinition CreateExportProfile()
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
    }

    private static InterfaceProfileManualProcessingResult CreateSuccessResult(
        ProcessedFileArchiveResult? archiveResult = null)
    {
        return new InterfaceProfileManualProcessingResult(
            Success: true,
            ExportFilePath: "export.xdt",
            ExportContent: "xdt",
            PipelineResult: null,
            ArchiveResult: archiveResult,
            FailedFileCopyResult: null,
            Messages: new[] { "ok" });
    }

    private static InterfaceProfileManualProcessingResult CreateFailureResult(
        FailedFileCopyResult? failedFileCopyResult = null)
    {
        return new InterfaceProfileManualProcessingResult(
            Success: false,
            ExportFilePath: null,
            ExportContent: null,
            PipelineResult: null,
            ArchiveResult: null,
            FailedFileCopyResult: failedFileCopyResult,
            Messages: new[] { "Fehler" });
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static TempImportFiles CreateTempImportFiles()
    {
        var folder = CreateTempFolder();
        var aisFilePath = Path.Combine(folder, "patient.gdt");
        var deviceFilePath = Path.Combine(folder, "device.xml");
        File.WriteAllText(aisFilePath, "gdt");
        File.WriteAllText(deviceFilePath, "xml");
        return new TempImportFiles(folder, aisFilePath, deviceFilePath);
    }

    private sealed record TempImportFiles(string Folder, string AisFilePath, string DeviceFilePath);

    private sealed class FakeManualProcessor : IInterfaceProfileManualProcessor
    {
        private readonly InterfaceProfileManualProcessingResult _result;

        public FakeManualProcessor(InterfaceProfileManualProcessingResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public InterfaceProfileManualProcessingResult Process(
            InterfaceProfileDefinition interfaceProfile,
            ExportProfileDefinition exportProfile,
            string aisFilePath,
            string deviceFilePath,
            DateTime timestamp)
        {
            CallCount++;
            return _result;
        }
    }
}
