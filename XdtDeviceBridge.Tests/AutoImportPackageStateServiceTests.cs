using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AutoImportPackageStateServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 6, 1, 12, 0, 0);

    [Fact]
    public void Evaluate_ShouldWaitForDeviceFileWhenOnlyAisFileExists()
    {
        var service = new AutoImportPackageStateService();
        var queue = CreateQueue(CreateAis("patient.gdt", Timestamp));

        var result = service.Evaluate(CreateProfile(), queue, Timestamp);

        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, result.Reason);
        Assert.Contains("Warte auf Gerätedatei.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReturnReadyPairWhenDeviceFileArrivesWithinTimeout()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        service.Evaluate(profile, CreateQueue(CreateAis("patient.gdt", Timestamp)), Timestamp);
        var queue = CreateQueue(
            CreateAis("patient.gdt", Timestamp),
            CreateDevice("device.xml", Timestamp.AddMinutes(4)));

        var result = service.Evaluate(profile, queue, Timestamp.AddMinutes(4));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("patient.gdt", pair.AisFile.FileName);
        Assert.Equal("device.xml", pair.DeviceFile.FileName);
        Assert.Equal(AutoImportPackageStateReason.ReadyForProcessing, result.Reason);
    }

    [Fact]
    public void Evaluate_ShouldExpireAisFileWhenDeviceFileDoesNotArrive()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        var queue = CreateQueue(CreateAis("patient.gdt", Timestamp));
        service.Evaluate(profile, queue, Timestamp);

        var result = service.Evaluate(profile, queue, Timestamp.AddMinutes(10));

        Assert.Empty(result.ReadyPairs);
        Assert.Single(result.ExpiredAisFiles);
        Assert.Equal(AutoImportPackageStateReason.AisFileExpired, result.Reason);
        Assert.Contains("AIS-Datei abgelaufen: keine Gerätedatei innerhalb der Wartezeit gefunden.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReplaceWaitingAisFileWithNewerAisFile()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        var oldAis = CreateAis("old.gdt", Timestamp);
        var newAis = CreateAis("new.gdt", Timestamp.AddMinutes(1));
        service.Evaluate(profile, CreateQueue(oldAis), Timestamp);

        var result = service.Evaluate(profile, CreateQueue(oldAis, newAis), Timestamp.AddMinutes(1));

        var replaced = Assert.Single(result.ReplacedAisFiles);
        Assert.Equal("old.gdt", replaced.FileName);
        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.AisFileReplaced, result.Reason);
        Assert.Contains("Vorherige AIS-Datei wurde durch neuere AIS-Datei ersetzt.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldUseNewerAisAfterReplacementWhenDeviceArrives()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        var oldAis = CreateAis("old.gdt", Timestamp);
        var newAis = CreateAis("new.gdt", Timestamp.AddMinutes(1));
        service.Evaluate(profile, CreateQueue(oldAis), Timestamp);
        service.Evaluate(profile, CreateQueue(oldAis, newAis), Timestamp.AddMinutes(1));

        var result = service.Evaluate(
            profile,
            CreateQueue(oldAis, newAis, CreateDevice("device.xml", Timestamp.AddMinutes(2))),
            Timestamp.AddMinutes(2));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("new.gdt", pair.AisFile.FileName);
    }

    [Fact]
    public void Evaluate_ShouldNotReturnPairForUnstableDeviceFile()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile();
        var queue = CreateQueue(
            CreateAis("patient.gdt", Timestamp),
            CreateDevice("device.xml", Timestamp, PendingImportFileStatus.Detected));

        var result = service.Evaluate(profile, queue, Timestamp);

        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, result.Reason);
    }

    private static InterfaceProfileDefinition CreateProfile(int deviceFileWaitTimeoutMinutes = 10)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = true,
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                DeviceFileWaitTimeoutMinutes = deviceFileWaitTimeoutMinutes
            }
        };
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

    private static PendingImportFile CreateAis(
        string fileName,
        DateTime detectedAt,
        PendingImportFileStatus status = PendingImportFileStatus.Stable)
    {
        return new PendingImportFile(
            FilePath: Path.Combine(@"C:\Import\AIS", fileName),
            FileName: fileName,
            Kind: ImportFileKind.AisGdt,
            Status: status,
            DetectedAtUtc: detectedAt,
            StableAtUtc: status == PendingImportFileStatus.Stable ? detectedAt : null,
            Message: null);
    }

    private static PendingImportFile CreateDevice(
        string fileName,
        DateTime detectedAt,
        PendingImportFileStatus status = PendingImportFileStatus.Stable)
    {
        return new PendingImportFile(
            FilePath: Path.Combine(@"C:\Import\Device", fileName),
            FileName: fileName,
            Kind: ImportFileKind.DeviceXml,
            Status: status,
            DetectedAtUtc: detectedAt,
            StableAtUtc: status == PendingImportFileStatus.Stable ? detectedAt : null,
            Message: null);
    }
}
