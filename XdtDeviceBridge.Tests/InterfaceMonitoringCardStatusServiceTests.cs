using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceMonitoringCardStatusServiceTests
{
    private readonly PatientData _patient = new(
        PatientNumber: "4701-1",
        LastName: "Kurze",
        FirstName: "Marc",
        BirthDate: "19800101",
        PostalCodeCity: "",
        Street: "",
        GenderCode: "",
        SourceSystem: "",
        TargetSystem: "",
        GdtVersion: "",
        ExaminationType: "Refraktion");

    [Fact]
    public void ApplyScanResult_ShouldShowWaitingForAisWhenNoFilesAreKnown()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var scanResult = CreateScanResult(profile, new PendingImportQueue());

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForAisFile), DateTime.Today, automaticProcessingEnabled: false);

        Assert.Equal("Wartet auf AIS", updated.CurrentStatus);
        Assert.Equal("Waiting", updated.StatusClass);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Status == "erwartet");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowPatientWhenAisFileIsStable()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1);

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForDeviceFile), DateTime.Today, automaticProcessingEnabled: false);

        Assert.Equal("Wartet auf Gerät", updated.CurrentStatus);
        Assert.Equal("Patient: Marc Kurze", updated.PatientDisplayText);
        Assert.Equal("patient.gdt", updated.AisFileName);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Status == "Patient erkannt" && input.Detail == "Patient: Marc Kurze");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowDeviceFileWhenDeviceIsKnown()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt));
        queue.AddOrUpdate(CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), DateTime.Today, automaticProcessingEnabled: false);

        Assert.Equal("ark1s.xml", updated.DeviceFileName);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "device" && input.Status == "gefunden" && input.Detail == "ark1s.xml");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowWaitingForAttachmentWhenPairIsCompleteAndAttachmentIsEnabled()
    {
        var profile = CreateProfile(isAttachmentEnabled: true);
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt));
        queue.AddOrUpdate(CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), DateTime.Today, automaticProcessingEnabled: true);

        Assert.Equal("Wartet auf XDT-Anhang", updated.CurrentStatus);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "wartet" && input.StatusClass == "Waiting");
    }

    [Fact]
    public void ApplyProcessingResult_ShouldShowSuccessfulAttachmentAndExportFile()
    {
        var profile = CreateProfile(isAttachmentEnabled: true);
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var result = CreateProcessingResult(
            success: true,
            status: "automatisch verarbeitet",
            exportFilePath: @"C:\Test\Export\result.xdt",
            attachmentStatus: new AttachmentProcessingStatus(
                WasAttempted: true,
                WasSkipped: false,
                Success: true,
                Reason: AttachmentProcessingStatusReason.PreparationSucceeded,
                Message: "XDT-Anhang vorbereitet.",
                SourcePath: @"C:\Test\AnhangImp\scan.pdf",
                TargetPath: @"C:\Test\AnhangExp\4701-1.pdf",
                TargetFileName: "4701-1.pdf",
                PreparedFields: Array.Empty<ExportFieldRecord>()));

        var updated = service.ApplyProcessingResult(card, result, new DateTime(2026, 5, 10, 12, 0, 0), automaticProcessingEnabled: true);

        Assert.Equal("Export erfolgreich", updated.CurrentStatus);
        Assert.Equal("Success", updated.StatusClass);
        Assert.Equal("result.xdt", updated.ExportFileName);
        Assert.Equal("4701-1.pdf", updated.AttachmentFileName);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "erfolgreich" && input.StatusClass == "Success");
    }

    [Fact]
    public void ApplyProcessingResult_ShouldShowRequiredAttachmentBlock()
    {
        var profile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required);
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var result = CreateProcessingResult(
            success: false,
            status: "blockiert",
            exportFilePath: null,
            attachmentStatus: new AttachmentProcessingStatus(
                WasAttempted: false,
                WasSkipped: true,
                Success: false,
                Reason: AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock,
                Message: "XDT-Anhang Pflicht: Timeout erreicht, Verarbeitung blockiert.",
                SourcePath: null,
                TargetPath: null,
                TargetFileName: null,
                PreparedFields: Array.Empty<ExportFieldRecord>()));

        var updated = service.ApplyProcessingResult(card, result, DateTime.Today, automaticProcessingEnabled: true);

        Assert.Equal("XDT-Anhang Pflicht blockiert", updated.CurrentStatus);
        Assert.Equal("Blocked", updated.StatusClass);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "blockiert" && input.StatusClass == "Blocked");
    }

    [Fact]
    public void ApplyProcessingResult_ShouldShowExportFailure()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var result = CreateProcessingResult(success: false, status: "Export fehlgeschlagen", exportFilePath: null, attachmentStatus: null);

        var updated = service.ApplyProcessingResult(card, result, DateTime.Today, automaticProcessingEnabled: true);

        Assert.Equal("Export fehlgeschlagen", updated.CurrentStatus);
        Assert.Equal("Error", updated.StatusClass);
        Assert.Equal("Export fehlgeschlagen", updated.LastMessage);
    }

    private static InterfaceMonitoringCardDisplay CreateCard(InterfaceProfileDefinition profile)
    {
        var service = new ActiveInterfaceProfileStatusService();
        return service.BuildRows(
                new[] { profile },
                new[] { DefaultAisProfiles.CreateMedistarDefault() },
                new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
                new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
                Array.Empty<LicensedDeviceState>())
            .Single()
            .MonitoringCard;
    }

    private static InterfaceProfileDefinition CreateProfile(
        bool isAttachmentEnabled = false,
        AttachmentRequirementMode attachmentRequirementMode = AttachmentRequirementMode.Optional)
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        return profile with
        {
            IsActive = true,
            FolderOptions = profile.FolderOptions with
            {
                AisImportFolder = @"C:\Test\AIS",
                DeviceImportFolder = @"C:\Test\Device",
                ExportFolder = @"C:\Test\Export",
                AttachmentImportFolder = isAttachmentEnabled ? @"C:\Test\AnhangImp" : "",
                AttachmentExportFolder = isAttachmentEnabled ? @"C:\Test\AnhangExp" : "",
                IsAttachmentProcessingEnabled = isAttachmentEnabled,
                AttachmentRequirementMode = attachmentRequirementMode
            }
        };
    }

    private static AutoImportScanResult CreateScanResult(
        InterfaceProfileDefinition profile,
        PendingImportQueue queue,
        int aisFiles = 0,
        int deviceFiles = 0,
        int readyPairs = 0)
    {
        return new AutoImportScanResult(
            InterfaceProfileId: profile.Metadata.Id,
            AisFilesDetected: aisFiles,
            DeviceFilesDetected: deviceFiles,
            FilesQueued: queue.GetAll().Count,
            ReadyPairs: readyPairs,
            Messages: Array.Empty<string>(),
            Queue: queue);
    }

    private static AutoImportPackageEvaluationResult CreatePackageEvaluation(AutoImportPackageStateReason reason)
    {
        return new AutoImportPackageEvaluationResult(
            ReadyPairs: Array.Empty<PendingImportPair>(),
            Messages: reason == AutoImportPackageStateReason.None ? Array.Empty<string>() : new[] { reason.ToString() },
            ReplacedAisFiles: Array.Empty<PendingImportFile>(),
            ExpiredAisFiles: Array.Empty<PendingImportFile>(),
            Reason: reason);
    }

    private static PendingImportFile CreateFile(string filePath, ImportFileKind kind)
    {
        return new PendingImportFile(
            FilePath: filePath,
            FileName: Path.GetFileName(filePath),
            Kind: kind,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: DateTime.UtcNow,
            StableAtUtc: DateTime.UtcNow,
            Message: null);
    }

    private static AutoImportPairProcessingResult CreateProcessingResult(
        bool success,
        string status,
        string? exportFilePath,
        AttachmentProcessingStatus? attachmentStatus)
    {
        return new AutoImportPairProcessingResult(
            PairKey: "pair-1",
            AisFilePath: @"C:\Test\AIS\patient.gdt",
            DeviceFilePath: @"C:\Test\Device\ark1s.xml",
            WasProcessed: true,
            WasSkipped: !success && attachmentStatus is not null,
            Success: success,
            Status: status,
            ExportFilePath: exportFilePath,
            ManualProcessingResult: null,
            Messages: new[] { status },
            AttachmentStatus: attachmentStatus);
    }

    private sealed class FakeAisPatientDataReader : IAisPatientDataReader
    {
        private readonly PatientData _patient;

        public FakeAisPatientDataReader(PatientData patient)
        {
            _patient = patient;
        }

        public AisPatientDataReadResult Read(string aisFilePath)
        {
            return new AisPatientDataReadResult(true, _patient, null);
        }
    }
}
