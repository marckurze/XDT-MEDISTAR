using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class MonitoringActivityEventKeyBuilderTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 18, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateDeviceDetectedKey_ShouldIncludeFileVersionForSameFileName()
    {
        var firstQueue = Queue(DeviceFile(@"C:\Import\NIDEK NT530P.xml", BaseTime));
        var secondQueue = Queue(DeviceFile(@"C:\Import\NIDEK NT530P.xml", BaseTime.AddMinutes(1)));

        var firstKey = MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(firstQueue);
        var secondKey = MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(secondQueue);

        Assert.StartsWith("scan-device-detected:", firstKey, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("scan-device-detected:", secondKey, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(firstKey, secondKey);
    }

    [Fact]
    public void CreateDeviceDetectedKey_ShouldAllowDeduplicationOnlyForSameFileVersion()
    {
        var eventDeduplicationService = new InterfaceMonitoringEventDeduplicationService();
        var firstQueue = Queue(DeviceFile(@"C:\Import\NIDEK NT530P.xml", BaseTime));
        var sameFileVersionQueue = Queue(DeviceFile(@"C:\Import\NIDEK NT530P.xml", BaseTime));
        var newFileVersionQueue = Queue(DeviceFile(@"C:\Import\NIDEK NT530P.xml", BaseTime.AddMinutes(1)));

        var first = eventDeduplicationService.Record(
            "interface-nt530p",
            MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(firstQueue),
            "MEDISTAR + NIDEK NT530P: Gerätedatei erkannt (1).",
            BaseTime);
        var duplicate = eventDeduplicationService.Record(
            "interface-nt530p",
            MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(sameFileVersionQueue),
            "MEDISTAR + NIDEK NT530P: Gerätedatei erkannt (1).",
            BaseTime.AddSeconds(5));
        var newVersion = eventDeduplicationService.Record(
            "interface-nt530p",
            MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(newFileVersionQueue),
            "MEDISTAR + NIDEK NT530P: Gerätedatei erkannt (1).",
            BaseTime.AddMinutes(1));

        Assert.NotNull(first);
        Assert.Null(duplicate);
        Assert.NotNull(newVersion);
    }

    [Fact]
    public void CreateReadyPairKey_ShouldChangeWhenAisOrDeviceVersionChanges()
    {
        var firstQueue = Queue(
            AisFile(@"C:\Import\Patient.XDT", BaseTime),
            DeviceFile(@"C:\Import\NIDEK LM7.xml", BaseTime));
        var secondQueue = Queue(
            AisFile(@"C:\Import\Patient.XDT", BaseTime.AddMinutes(1)),
            DeviceFile(@"C:\Import\NIDEK LM7.xml", BaseTime.AddMinutes(1)));

        var firstKey = MonitoringActivityEventKeyBuilder.CreateReadyPairKey(firstQueue);
        var secondKey = MonitoringActivityEventKeyBuilder.CreateReadyPairKey(secondQueue);

        Assert.StartsWith("scan-ready-pair:", firstKey, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("scan-ready-pair:", secondKey, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(firstKey, secondKey);
    }

    [Fact]
    public void CreateReadyPairKey_ShouldIncludeAttachmentFilesForAttachmentOnlyMode()
    {
        var queue = Queue(
            AisFile(@"C:\Import\Patient.XDT", BaseTime),
            File(@"C:\Import\bild.jpg", ImportFileKind.AttachmentImage, BaseTime));

        var defaultKey = MonitoringActivityEventKeyBuilder.CreateReadyPairKey(queue);
        var attachmentOnlyKey = MonitoringActivityEventKeyBuilder.CreateReadyPairKey(queue, includeAttachmentDeviceFiles: true);

        Assert.Equal("scan-ready-pair", defaultKey);
        Assert.StartsWith("scan-ready-pair:", attachmentOnlyKey, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bild.jpg", attachmentOnlyKey, StringComparison.OrdinalIgnoreCase);
    }

    private static PendingImportQueue Queue(params PendingImportFile[] files)
    {
        var queue = new PendingImportQueue();
        foreach (var file in files)
        {
            queue.AddOrUpdate(file);
        }

        return queue;
    }

    private static PendingImportFile AisFile(string path, DateTime detectedAtUtc)
    {
        return File(path, ImportFileKind.AisXdt, detectedAtUtc);
    }

    private static PendingImportFile DeviceFile(string path, DateTime detectedAtUtc)
    {
        return File(path, ImportFileKind.DeviceXml, detectedAtUtc);
    }

    private static PendingImportFile File(string path, ImportFileKind kind, DateTime detectedAtUtc)
    {
        return new PendingImportFile(
            FilePath: path,
            FileName: Path.GetFileName(path),
            Kind: kind,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: detectedAtUtc,
            StableAtUtc: detectedAtUtc.AddSeconds(2),
            Message: null);
    }
}
