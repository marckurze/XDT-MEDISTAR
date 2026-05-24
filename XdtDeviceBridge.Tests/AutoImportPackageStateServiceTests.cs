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
    public void Evaluate_ShouldReplaceWaitingAisFileWithNewVersionOfSamePath()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        var oldAis = CreateAis("Patient.gdt", Timestamp);
        var newAis = CreateAis("Patient.gdt", Timestamp.AddMinutes(1));
        service.Evaluate(profile, CreateQueue(oldAis), Timestamp);

        var result = service.Evaluate(profile, CreateQueue(newAis), Timestamp.AddMinutes(1));

        var replaced = Assert.Single(result.ReplacedAisFiles);
        Assert.Equal("Patient.gdt", replaced.FileName);
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

    [Fact]
    public void Evaluate_Cv5000ShouldWaitForNewDeviceFileWhenOldPhoropterResultRemains()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        var aisFile = CreateAis("Patient.gdt", Timestamp.AddMinutes(10));
        var oldDeviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(5));

        var result = service.Evaluate(profile, CreateQueue(aisFile, oldDeviceFile), Timestamp.AddMinutes(10));

        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, result.Reason);
        Assert.Contains("Warte auf neue Phoropter-Rueckgabedatei.", result.Messages);
    }

    [Fact]
    public void Evaluate_Cv5000ShouldUseNewDeviceVersionAfterSameNameAisCycle()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        var aisFile = CreateAis("Patient.gdt", Timestamp.AddMinutes(10));
        var deviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(11));

        var result = service.Evaluate(profile, CreateQueue(aisFile, deviceFile), Timestamp.AddMinutes(11));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("Patient.gdt", pair.AisFile.FileName);
        Assert.Equal("M-Serial1234.xml", pair.DeviceFile.FileName);
        Assert.Equal(AutoImportPackageStateReason.ReadyForProcessing, result.Reason);
    }

    [Fact]
    public void MarkCv5000WaitingForDeviceResult_ShouldWaitWhenOnlyBaselineDeviceFileExists()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        var aisFile = CreateAis("Patient.gdt", Timestamp.AddMinutes(10));
        var oldDeviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(1));
        var phaseOneQueue = CreateQueue(aisFile, oldDeviceFile);
        service.MarkCv5000WaitingForDeviceResult(profile.Metadata.Id, aisFile, phaseOneQueue, Timestamp.AddMinutes(10));

        var result = service.Evaluate(profile, CreateQueue(aisFile, oldDeviceFile), Timestamp.AddMinutes(11));

        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, result.Reason);
        Assert.Contains("Warte auf neue oder geänderte Phoropter-Rueckgabedatei.", result.Messages);
    }

    [Fact]
    public void MarkCv5000WaitingForDeviceResult_ShouldAcceptNewDeviceEvenWhenDeviceTimestampIsOlderThanAis()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        var aisFile = CreateAis("Patient.gdt", Timestamp.AddMinutes(10));
        service.MarkCv5000WaitingForDeviceResult(profile.Metadata.Id, aisFile, CreateQueue(aisFile), Timestamp.AddMinutes(10));
        var deviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(1));

        var result = service.Evaluate(profile, CreateQueue(aisFile, deviceFile), Timestamp.AddMinutes(11));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("Patient.gdt", pair.AisFile.FileName);
        Assert.Equal("M-Serial1234.xml", pair.DeviceFile.FileName);
        Assert.Equal(AutoImportPackageStateReason.ReadyForProcessing, result.Reason);
    }

    [Fact]
    public void MarkCv5000WaitingForDeviceResult_ShouldAcceptOverwrittenDeviceWithNewFingerprint()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        var aisFile = CreateAis("Patient.gdt", Timestamp.AddMinutes(10));
        var oldDeviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(1));
        var newDeviceFile = CreateDevice("M-Serial1234.xml", Timestamp.AddMinutes(2));
        service.MarkCv5000WaitingForDeviceResult(
            profile.Metadata.Id,
            aisFile,
            CreateQueue(aisFile, oldDeviceFile),
            Timestamp.AddMinutes(10));

        var result = service.Evaluate(profile, CreateQueue(aisFile, newDeviceFile), Timestamp.AddMinutes(11));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("M-Serial1234.xml", pair.DeviceFile.FileName);
        Assert.Equal(AutoImportPackageStateReason.ReadyForProcessing, result.Reason);
    }

    [Fact]
    public void MarkCv5000WaitingForDeviceResult_ShouldKeepUnchangedObservedBaselineDeviceWaiting()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        using var files = TempCv5000Files.Create();
        files.WriteAis("gdt-cycle-2", Timestamp.AddMinutes(10));
        files.WriteDevice("cv5000-result", Timestamp.AddMinutes(1), Timestamp.AddMinutes(1));
        var aisFile = CreateRealAis(files.AisPath, Timestamp.AddMinutes(10));
        var baselineDevice = CreateRealDevice(files.DevicePath, Timestamp.AddMinutes(1));
        service.MarkCv5000WaitingForDeviceResult(
            profile.Metadata.Id,
            aisFile,
            CreateQueue(aisFile, baselineDevice),
            Timestamp.AddMinutes(10));

        var result = service.Evaluate(
            profile,
            CreateQueue(aisFile, CreateRealDevice(files.DevicePath, Timestamp.AddMinutes(1))),
            Timestamp.AddMinutes(11));

        Assert.Empty(result.ReadyPairs);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, result.Reason);
        Assert.Contains("Warte auf neue oder geänderte Phoropter-Rueckgabedatei.", result.Messages);
    }

    [Fact]
    public void MarkCv5000WaitingForDeviceResult_ShouldAcceptOverwrittenSameNameDeviceWhenObservedVersionChanges()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateCv5000Profile();
        using var files = TempCv5000Files.Create();
        files.WriteAis("gdt-cycle-2", Timestamp.AddMinutes(10));
        files.WriteDevice("cv5000-result", Timestamp.AddMinutes(1), Timestamp.AddMinutes(1));
        var aisFile = CreateRealAis(files.AisPath, Timestamp.AddMinutes(10));
        var baselineDevice = CreateRealDevice(files.DevicePath, Timestamp.AddMinutes(1));
        service.MarkCv5000WaitingForDeviceResult(
            profile.Metadata.Id,
            aisFile,
            CreateQueue(aisFile, baselineDevice),
            Timestamp.AddMinutes(10));

        files.TouchDeviceAccessTime(Timestamp.AddMinutes(12));
        var result = service.Evaluate(
            profile,
            CreateQueue(aisFile, CreateRealDevice(files.DevicePath, Timestamp.AddMinutes(1))),
            Timestamp.AddMinutes(12));

        var pair = Assert.Single(result.ReadyPairs);
        Assert.Equal("M-Serial1234.xml", pair.DeviceFile.FileName);
        Assert.Equal(AutoImportPackageStateReason.ReadyForProcessing, result.Reason);
        Assert.Contains("CV-5000-Phoropter-Rueckgabe wurde als neu oder geaendert erkannt.", result.Messages);
    }

    [Fact]
    public void ResetProfile_ShouldClearWaitingAisStateOnlyForSelectedProfile()
    {
        var service = new AutoImportPackageStateService();
        var profile = CreateProfile(deviceFileWaitTimeoutMinutes: 10);
        var otherProfile = profile with
        {
            Metadata = profile.Metadata with { Id = "interface-other" }
        };
        var firstAis = CreateAis("first.gdt", Timestamp);
        var secondAis = CreateAis("second.gdt", Timestamp.AddMinutes(1));
        service.Evaluate(profile, CreateQueue(firstAis), Timestamp);
        service.Evaluate(otherProfile, CreateQueue(firstAis), Timestamp);

        service.ResetProfile(profile.Metadata.Id);
        var resetProfileResult = service.Evaluate(profile, CreateQueue(firstAis, secondAis), Timestamp.AddMinutes(1));
        var otherProfileResult = service.Evaluate(otherProfile, CreateQueue(firstAis, secondAis), Timestamp.AddMinutes(1));

        Assert.Empty(resetProfileResult.ReplacedAisFiles);
        Assert.Equal(AutoImportPackageStateReason.WaitingForDeviceFile, resetProfileResult.Reason);
        Assert.Single(otherProfileResult.ReplacedAisFiles);
        Assert.Equal(AutoImportPackageStateReason.AisFileReplaced, otherProfileResult.Reason);
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

    private static InterfaceProfileDefinition CreateCv5000Profile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        return profile with
        {
            IsActive = true,
            FolderOptions = profile.FolderOptions with
            {
                DeviceFileWaitTimeoutMinutes = 10
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

    private static PendingImportFile CreateRealAis(
        string filePath,
        DateTime detectedAt,
        PendingImportFileStatus status = PendingImportFileStatus.Stable)
    {
        return new PendingImportFile(
            FilePath: filePath,
            FileName: Path.GetFileName(filePath),
            Kind: ImportFileKind.AisGdt,
            Status: status,
            DetectedAtUtc: detectedAt,
            StableAtUtc: status == PendingImportFileStatus.Stable ? detectedAt : null,
            Message: null);
    }

    private static PendingImportFile CreateRealDevice(
        string filePath,
        DateTime detectedAt,
        PendingImportFileStatus status = PendingImportFileStatus.Stable)
    {
        return new PendingImportFile(
            FilePath: filePath,
            FileName: Path.GetFileName(filePath),
            Kind: ImportFileKind.DeviceXml,
            Status: status,
            DetectedAtUtc: detectedAt,
            StableAtUtc: status == PendingImportFileStatus.Stable ? detectedAt : null,
            Message: null);
    }

    private sealed class TempCv5000Files : IDisposable
    {
        private TempCv5000Files(string folder)
        {
            Folder = folder;
            AisPath = Path.Combine(folder, "Patient.gdt");
            DevicePath = Path.Combine(folder, "M-Serial1234.xml");
        }

        public string Folder { get; }

        public string AisPath { get; }

        public string DevicePath { get; }

        public static TempCv5000Files Create()
        {
            var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return new TempCv5000Files(folder);
        }

        public void WriteAis(string content, DateTime lastWriteUtc)
        {
            File.WriteAllText(AisPath, content);
            File.SetLastWriteTimeUtc(AisPath, lastWriteUtc);
        }

        public void WriteDevice(string content, DateTime lastWriteUtc, DateTime lastAccessUtc)
        {
            File.WriteAllText(DevicePath, content);
            File.SetLastWriteTimeUtc(DevicePath, lastWriteUtc);
            File.SetLastAccessTimeUtc(DevicePath, lastAccessUtc);
        }

        public void TouchDeviceAccessTime(DateTime lastAccessUtc)
        {
            File.SetLastAccessTimeUtc(DevicePath, lastAccessUtc);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Folder, recursive: true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
