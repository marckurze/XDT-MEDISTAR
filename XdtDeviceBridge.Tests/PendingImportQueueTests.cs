using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class PendingImportQueueTests
{
    private static readonly DateTime BaseTimeUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AddOrUpdate_ShouldAddFile()
    {
        var queue = new PendingImportQueue();
        var file = CreateFile("C:\\Import\\patient.gdt", ImportFileKind.AisGdt);

        queue.AddOrUpdate(file);

        var added = Assert.Single(queue.GetAll());
        Assert.Equal(file, added);
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingFile()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile("C:\\Import\\patient.gdt", ImportFileKind.AisGdt));

        queue.AddOrUpdate(CreateFile(
            "c:\\import\\PATIENT.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable,
            message: "stabil"));

        var updated = Assert.Single(queue.GetAll());
        Assert.Equal(PendingImportFileStatus.Stable, updated.Status);
        Assert.Equal("stabil", updated.Message);
    }

    [Fact]
    public void GetByKind_ShouldFilterFiles()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile("C:\\Import\\patient.gdt", ImportFileKind.AisGdt));
        queue.AddOrUpdate(CreateFile("C:\\Import\\device.xml", ImportFileKind.DeviceXml));

        var aisFiles = queue.GetByKind(ImportFileKind.AisGdt);

        var aisFile = Assert.Single(aisFiles);
        Assert.Equal("patient.gdt", aisFile.FileName);
    }

    [Fact]
    public void Remove_ShouldRemoveFile()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile("C:\\Import\\patient.gdt", ImportFileKind.AisGdt));

        var removed = queue.Remove("c:\\import\\PATIENT.gdt");

        Assert.True(removed);
        Assert.Empty(queue.GetAll());
    }

    [Fact]
    public void FindReadyPairs_ShouldReturnNoPairWhenOnlyAisFileExists()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable));

        Assert.Empty(queue.FindReadyPairs());
    }

    [Fact]
    public void FindReadyPairs_ShouldReturnNoPairWhenOnlyDeviceFileExists()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\device.xml",
            ImportFileKind.DeviceXml,
            status: PendingImportFileStatus.Stable));

        Assert.Empty(queue.FindReadyPairs());
    }

    [Fact]
    public void FindReadyPairs_ShouldReturnPairWhenStableAisAndDeviceFilesExist()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\device.xml",
            ImportFileKind.DeviceXml,
            status: PendingImportFileStatus.Stable));

        var pair = Assert.Single(queue.FindReadyPairs());

        Assert.True(pair.IsReady);
        Assert.Equal("patient.gdt", pair.AisFile.FileName);
        Assert.Equal("device.xml", pair.DeviceFile.FileName);
    }

    [Fact]
    public void FindReadyPairs_ShouldIgnoreFilesThatAreNotStable()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Detected));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\device.xml",
            ImportFileKind.DeviceXml,
            status: PendingImportFileStatus.Stable));

        Assert.Empty(queue.FindReadyPairs());
    }

    [Fact]
    public void FindReadyPairs_ShouldIgnoreAttachments()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\image.jpg",
            ImportFileKind.AttachmentImage,
            status: PendingImportFileStatus.Stable));

        Assert.Empty(queue.FindReadyPairs());
    }

    [Fact]
    public void FindReadyPairs_ShouldPairFilesByFifoDetectedAtUtc()
    {
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient-new.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable,
            detectedAtUtc: BaseTimeUtc.AddMinutes(2)));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\patient-old.gdt",
            ImportFileKind.AisGdt,
            status: PendingImportFileStatus.Stable,
            detectedAtUtc: BaseTimeUtc.AddMinutes(1)));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\device-new.xml",
            ImportFileKind.DeviceXml,
            status: PendingImportFileStatus.Stable,
            detectedAtUtc: BaseTimeUtc.AddMinutes(4)));
        queue.AddOrUpdate(CreateFile(
            "C:\\Import\\device-old.xml",
            ImportFileKind.DeviceXml,
            status: PendingImportFileStatus.Stable,
            detectedAtUtc: BaseTimeUtc.AddMinutes(3)));

        var pairs = queue.FindReadyPairs();

        Assert.Equal(2, pairs.Count);
        Assert.Equal("patient-old.gdt", pairs[0].AisFile.FileName);
        Assert.Equal("device-old.xml", pairs[0].DeviceFile.FileName);
        Assert.Equal("patient-new.gdt", pairs[1].AisFile.FileName);
        Assert.Equal("device-new.xml", pairs[1].DeviceFile.FileName);
    }

    private static PendingImportFile CreateFile(
        string filePath,
        ImportFileKind kind,
        PendingImportFileStatus status = PendingImportFileStatus.Detected,
        string? message = null,
        DateTime? detectedAtUtc = null)
    {
        return new PendingImportFile(
            FilePath: filePath,
            FileName: Path.GetFileName(filePath),
            Kind: kind,
            Status: status,
            DetectedAtUtc: detectedAtUtc ?? BaseTimeUtc,
            StableAtUtc: status == PendingImportFileStatus.Stable ? detectedAtUtc ?? BaseTimeUtc : null,
            Message: message);
    }
}
