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
        Assert.False(updated.ShouldFlashStatusOrb);
        Assert.Equal("StoppedRed", updated.StatusOrbVisualState);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Status == "gestoppt");
    }

    [Fact]
    public void ApplyScanResult_ShouldUseWaitingInputStatusForActiveStandardDeviceCard()
    {
        var baseProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var profile = baseProfile with
        {
            IsActive = true,
            FolderOptions = baseProfile.FolderOptions with
            {
                AisImportFolder = @"C:\Test\AIS",
                DeviceImportFolder = @"C:\Test\Device",
                ExportFolder = @"C:\Test\Export"
            }
        };
        var card = CreateCv5000Card(profile) with
        {
            IsScanAnimationActive = true
        };
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var scanResult = CreateScanResult(profile, new PendingImportQueue());

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForAisFile), DateTime.Today, automaticProcessingEnabled: true);

        Assert.True(updated.ShouldPulseStatusOrb);
        Assert.Equal("RunningGreen", updated.StatusOrbVisualState);
        Assert.DoesNotContain(updated.ExpectedInputs, input => input.Status == "erwartet");
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Status == "wartet");
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "device" && input.Status == "wartet");
    }

    [Fact]
    public void WithPilotMonitoringActivity_ShouldKeepDetectedInputStatuses()
    {
        var baseProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var profile = baseProfile with
        {
            IsActive = true,
            FolderOptions = baseProfile.FolderOptions with
            {
                AisImportFolder = @"C:\Test\AIS",
                DeviceImportFolder = @"C:\Test\Device",
                ExportFolder = @"C:\Test\Export"
            }
        };
        var card = CreateCv5000Card(profile) with
        {
            ExpectedInputs = new[]
            {
                new ExpectedInputDisplayItem("ais", "Testfrau Anna", @"C:\Test\AIS", "", "Success", "patient.gdt"),
                new ExpectedInputDisplayItem("device", "Empfangen", @"C:\Test\Device", "", "Success", "geraet.xml", "TOPCON CV-5000 / CV-5000S")
            }
        };

        var updated = card.WithPilotMonitoringActivity(isMonitoringActive: true);

        Assert.All(updated.ExpectedInputs, input => Assert.Equal("", input.Status));
        Assert.All(updated.ExpectedInputs, input => Assert.Equal("Success", input.StatusClass));
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
        Assert.False(updated.ShouldPulseStatusOrb);
        Assert.True(updated.ShouldFlashStatusOrb);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Name == "Marc Kurze" && input.Status == "" && input.DisplayDetail == "");
        Assert.DoesNotContain(updated.ExpectedInputs, input => input.Key == "ais" && input.DisplayDetail.Contains(@"C:\Test\AIS", StringComparison.OrdinalIgnoreCase));
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
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "device" && input.Name == "Empfangen" && input.Status == "" && !input.DisplayDetail.Contains(@"C:\Test\Device", StringComparison.OrdinalIgnoreCase));
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
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status.StartsWith("Optional", StringComparison.Ordinal) && input.StatusClass == "Waiting");
    }

    [Fact]
    public void ApplyScanResult_AttachmentOnlyShouldKeepDocumentInputNeutralBeforePackageStarts()
    {
        var profile = CreateProfile(
            isAttachmentEnabled: true,
            attachmentRequirementMode: AttachmentRequirementMode.Required,
            isAttachmentOnlyMode: true);
        var card = CreateCard(profile);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var scanResult = CreateScanResult(profile, new PendingImportQueue());

        var updated = service.ApplyScanResult(
            card,
            profile,
            scanResult,
            CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForAisFile),
            DateTime.Today,
            automaticProcessingEnabled: true);

        var documentInput = Assert.Single(updated.ExpectedInputs, input => input.Key == "device");
        Assert.Equal("Dokumentdateien", documentInput.Name);
        Assert.Equal("gestoppt", documentInput.Status);
        Assert.Equal("Neutral", documentInput.StatusClass);
        Assert.DoesNotContain(updated.ExpectedInputs, input => input.Key == "attachment");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowRemainingDeviceWaitTime()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 2, 0, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddMinutes(-2)));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1);

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForDeviceFile), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "device" && input.Detail.Contains("noch 08:00"));
    }

    [Fact]
    public void ApplyScanResult_ShouldNotShowDeviceTimeoutWhileStillWaitingForDevice()
    {
        var profile = CreateProfile();
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 20, 0, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddMinutes(-20)));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1);

        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.WaitingForDeviceFile), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input =>
            input.Key == "device"
            && input.Status == "wartet auf Gerät"
            && !input.Detail.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
            && !input.DisplayDetail.Contains("Timeout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyScanResult_ShouldShowRemainingRequiredAttachmentWaitTime()
    {
        var profile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required) with
        {
            FolderOptions = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required).FolderOptions with
            {
                AttachmentWaitTimeoutSeconds = 30
            }
        };
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 0, 20, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddSeconds(-20)));
        queue.AddOrUpdate(CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml, now.AddSeconds(-20)));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now.AddSeconds(-20), automaticProcessingEnabled: true);
        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Pflicht" && input.DisplayDetail == "noch 10 s");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowRemainingOptionalAttachmentWaitTime()
    {
        var baseProfile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Optional);
        var profile = baseProfile with
        {
            FolderOptions = baseProfile.FolderOptions with
            {
                AttachmentWaitTimeoutSeconds = 45
            }
        };
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 0, 15, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        var aisFile = CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddSeconds(-15));
        var deviceFile = CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml, now.AddSeconds(-15));
        queue.AddOrUpdate(aisFile);
        queue.AddOrUpdate(deviceFile);
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now.AddSeconds(-15), automaticProcessingEnabled: true);
        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Optional" && input.DisplayDetail == "noch 30 s");
    }

    [Fact]
    public void ApplyScanResult_ShouldUsePackageEvaluationReadyPairForAttachmentCountdown()
    {
        var baseProfile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required);
        var profile = baseProfile with
        {
            FolderOptions = baseProfile.FolderOptions with
            {
                AttachmentWaitTimeoutSeconds = 30
            }
        };
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 0, 20, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        var aisFile = CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddSeconds(-20));
        var deviceFile = CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml, now.AddSeconds(-20));
        var readyPair = new PendingImportPair(aisFile, deviceFile, IsReady: true);
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing, new[] { readyPair }), now.AddSeconds(-20), automaticProcessingEnabled: true);
        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing, new[] { readyPair }), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Pflicht" && input.DisplayDetail == "noch 10 s");
    }

    [Fact]
    public void ApplyScanResult_ShouldShowRequiredAttachmentTimeoutWhenWaitElapsed()
    {
        var baseProfile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required);
        var profile = baseProfile with
        {
            FolderOptions = baseProfile.FolderOptions with
            {
                AttachmentWaitTimeoutSeconds = 10
            }
        };
        var card = CreateCard(profile);
        var now = new DateTime(2026, 5, 10, 12, 0, 15, DateTimeKind.Utc);
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var queue = new PendingImportQueue();
        queue.AddOrUpdate(CreateFile(@"C:\Test\AIS\patient.gdt", ImportFileKind.AisGdt, now.AddSeconds(-15)));
        queue.AddOrUpdate(CreateFile(@"C:\Test\Device\ark1s.xml", ImportFileKind.DeviceXml, now.AddSeconds(-15)));
        var scanResult = CreateScanResult(profile, queue, aisFiles: 1, deviceFiles: 1, readyPairs: 1);

        service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now.AddSeconds(-15), automaticProcessingEnabled: true);
        var updated = service.ApplyScanResult(card, profile, scanResult, CreatePackageEvaluation(AutoImportPackageStateReason.ReadyForProcessing), now, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Pflicht blockiert" && input.DisplayDetail == "Timeout erreicht");
    }

    [Fact]
    public void ApplyProcessingResult_ShouldKeepAttachmentCountdownWhileWaiting()
    {
        var profile = CreateProfile(isAttachmentEnabled: true, attachmentRequirementMode: AttachmentRequirementMode.Required);
        var card = CreateCard(profile) with
        {
            ExpectedInputs = CreateCard(profile).ExpectedInputs
                .Select(input => input.Key == "attachment"
                    ? input with { Status = "Pflicht", DisplayDetail = "noch 18 s" }
                    : input)
                .ToList()
        };
        var service = new InterfaceMonitoringCardStatusService(new FakeAisPatientDataReader(_patient));
        var result = CreateProcessingResult(
            success: false,
            status: "wartet",
            exportFilePath: null,
            attachmentStatus: new AttachmentProcessingStatus(
                WasAttempted: false,
                WasSkipped: true,
                Success: false,
                Reason: AttachmentProcessingStatusReason.AttachmentWait,
                Message: "Warte auf XDT-Anhang.",
                SourcePath: null,
                TargetPath: null,
                TargetFileName: null,
                PreparedFields: Array.Empty<ExportFieldRecord>()));

        var updated = service.ApplyProcessingResult(card, result, DateTime.Today, automaticProcessingEnabled: true);

        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Pflicht" && input.DisplayDetail == "noch 18 s");
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
        Assert.Equal("", updated.AisFileName);
        Assert.Equal("", updated.DeviceFileName);
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "ais" && input.Status == "erwartet" && input.DisplayDetail == "");
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "device" && input.Status == "erwartet" && input.DisplayDetail == "");
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Name == "PDF Empfangen" && input.Status == "" && input.StatusClass == "Success");
        Assert.DoesNotContain(updated.ExpectedInputs, input => input.Key == "attachment" && input.DisplayDetail == "Timeout erreicht");
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
        Assert.Contains(updated.ExpectedInputs, input => input.Key == "attachment" && input.Status == "Pflicht blockiert" && input.StatusClass == "Blocked");
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

    private static InterfaceMonitoringCardDisplay CreateCv5000Card(InterfaceProfileDefinition profile)
    {
        var service = new ActiveInterfaceProfileStatusService();
        return service.BuildRows(
                new[] { profile },
                new[] { DefaultAisProfiles.CreateMedistarDefault() },
                new[] { DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() },
                new[] { DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default() },
                Array.Empty<LicensedDeviceState>())
            .Single()
            .MonitoringCard;
    }

    private static InterfaceProfileDefinition CreateProfile(
        bool isAttachmentEnabled = false,
        AttachmentRequirementMode attachmentRequirementMode = AttachmentRequirementMode.Optional,
        bool isAttachmentOnlyMode = false)
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
                AttachmentRequirementMode = attachmentRequirementMode,
                IsAttachmentOnlyMode = isAttachmentOnlyMode
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

    private static AutoImportPackageEvaluationResult CreatePackageEvaluation(
        AutoImportPackageStateReason reason,
        IReadOnlyList<PendingImportPair>? readyPairs = null)
    {
        return new AutoImportPackageEvaluationResult(
            ReadyPairs: readyPairs ?? Array.Empty<PendingImportPair>(),
            Messages: reason == AutoImportPackageStateReason.None ? Array.Empty<string>() : new[] { reason.ToString() },
            ReplacedAisFiles: Array.Empty<PendingImportFile>(),
            ExpiredAisFiles: Array.Empty<PendingImportFile>(),
            Reason: reason);
    }

    private static PendingImportFile CreateFile(string filePath, ImportFileKind kind, DateTime? detectedAtUtc = null)
    {
        var detectedAt = detectedAtUtc ?? DateTime.UtcNow;
        return new PendingImportFile(
            FilePath: filePath,
            FileName: Path.GetFileName(filePath),
            Kind: kind,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: detectedAt,
            StableAtUtc: detectedAt,
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
