using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileMonitoringResetServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Reset_ShouldIgnoreOnlySelectedProfileFiles()
    {
        var service = new InterfaceProfileMonitoringResetService();
        var ar360Queue = CreateQueue(
            CreateFile(@"C:\Import\AR360\patient.gdt", ImportFileKind.AisGdt),
            CreateFile(@"C:\Import\AR360\device.xml", ImportFileKind.DeviceXml));
        var ark1sQueue = CreateQueue(
            CreateFile(@"C:\Import\ARK1S\patient.gdt", ImportFileKind.AisGdt),
            CreateFile(@"C:\Import\ARK1S\device.xml", ImportFileKind.DeviceXml));

        var resetResult = service.Reset("interface-ar360", ar360Queue);
        var filteredAr360 = service.Apply(CreateResult("interface-ar360", ar360Queue));
        var filteredArk1s = service.Apply(CreateResult("interface-ark1s", ark1sQueue));

        Assert.Equal(2, resetResult.IgnoredFileCount);
        Assert.Empty(filteredAr360.Queue.GetAll());
        Assert.Equal(2, filteredArk1s.Queue.GetAll().Count);
        Assert.Equal(1, filteredArk1s.ReadyPairs);
    }

    [Fact]
    public void Apply_ShouldAllowChangedFileAfterReset()
    {
        var service = new InterfaceProfileMonitoringResetService();
        var originalQueue = CreateQueue(CreateFile(@"C:\Import\AR360\device.xml", ImportFileKind.DeviceXml, Timestamp));
        var changedQueue = CreateQueue(CreateFile(@"C:\Import\AR360\device.xml", ImportFileKind.DeviceXml, Timestamp.AddMinutes(1)));

        _ = service.Reset("interface-ar360", originalQueue);
        var filteredOriginal = service.Apply(CreateResult("interface-ar360", originalQueue));
        var filteredChanged = service.Apply(CreateResult("interface-ar360", changedQueue));

        Assert.Empty(filteredOriginal.Queue.GetAll());
        Assert.Single(filteredChanged.Queue.GetAll());
        Assert.Equal(1, filteredChanged.DeviceFilesDetected);
    }

    [Fact]
    public void Reset_ShouldIncludeInputFolderResetSummary()
    {
        var service = new InterfaceProfileMonitoringResetService();
        var queue = CreateQueue(CreateFile(@"C:\Import\AR360\patient.gdt", ImportFileKind.AisGdt));
        var folderResetResult = new InterfaceProfileInputFolderResetResult(
            CandidateFolderCount: 2,
            ProcessedFolderCount: 2,
            DeletedFileCount: 3,
            FailedFileCount: 0,
            MissingFolderCount: 0,
            SkippedFolderCount: 0,
            ProcessedFolders: Array.Empty<string>(),
            SkippedFolders: Array.Empty<string>(),
            FailedFiles: Array.Empty<string>(),
            Messages: Array.Empty<string>());

        var result = service.Reset("interface-ar360", queue, folderResetResult);

        Assert.True(result.FileOperationsPerformed);
        Assert.Equal("Vorgang zurückgesetzt. Eingangsordner geleert: 3 Datei(en) gelöscht.", result.Messages[0]);
        Assert.Same(folderResetResult, result.InputFolderResetResult);
    }

    [Fact]
    public void Apply_ShouldUpdateCountsAfterIgnoredFilesAreRemoved()
    {
        var service = new InterfaceProfileMonitoringResetService();
        var queue = CreateQueue(
            CreateFile(@"C:\Import\AR360\patient.gdt", ImportFileKind.AisGdt),
            CreateFile(@"C:\Import\AR360\device.xml", ImportFileKind.DeviceXml));

        _ = service.Reset("interface-ar360", queue);
        var filtered = service.Apply(CreateResult("interface-ar360", queue));

        Assert.Equal(0, filtered.AisFilesDetected);
        Assert.Equal(0, filtered.DeviceFilesDetected);
        Assert.Equal(0, filtered.FilesQueued);
        Assert.Equal(0, filtered.ReadyPairs);
    }

    [Fact]
    public void Apply_ShouldClearIgnoredFilesAfterEmptyScan()
    {
        var service = new InterfaceProfileMonitoringResetService();
        var queue = CreateQueue(CreateFile(@"C:\Import\AR360\patient.gdt", ImportFileKind.AisGdt));

        _ = service.Reset("interface-ar360", queue);
        _ = service.Apply(CreateResult("interface-ar360", CreateQueue()));
        var afterNewFileArrives = service.Apply(CreateResult("interface-ar360", queue));

        Assert.Single(afterNewFileArrives.Queue.GetAll());
        Assert.Equal(1, afterNewFileArrives.AisFilesDetected);
    }

    private static AutoImportScanResult CreateResult(string interfaceProfileId, PendingImportQueue queue)
    {
        return new AutoImportScanResult(
            interfaceProfileId,
            AisFilesDetected: queue.GetAll().Count(file => file.Kind is ImportFileKind.AisGdt or ImportFileKind.AisXdt),
            DeviceFilesDetected: queue.GetAll().Count(file => file.Kind is ImportFileKind.DeviceXml or ImportFileKind.DeviceText or ImportFileKind.DeviceCsv),
            FilesQueued: queue.GetAll().Count,
            ReadyPairs: queue.FindReadyPairs().Count,
            Messages: Array.Empty<string>(),
            Queue: queue);
    }

    private static PendingImportQueue CreateQueue(params PendingImportFile[] files)
    {
        var queue = new PendingImportQueue();
        foreach (var file in files)
        {
            queue.AddOrUpdate(file);
        }

        return queue;
    }

    private static PendingImportFile CreateFile(
        string path,
        ImportFileKind kind,
        DateTime? detectedAtUtc = null)
    {
        var detectedAt = detectedAtUtc ?? Timestamp;
        return new PendingImportFile(
            FilePath: path,
            FileName: Path.GetFileName(path),
            Kind: kind,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: detectedAt,
            StableAtUtc: detectedAt,
            Message: null);
    }
}
