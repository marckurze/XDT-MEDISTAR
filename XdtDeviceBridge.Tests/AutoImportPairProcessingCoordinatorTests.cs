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
    public void ProcessReadyPairs_AttachmentDisabledShouldSkipWithoutScannerOrTransfer()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: false,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.NotNull(processed.AttachmentStatus);
        Assert.True(processed.AttachmentStatus!.WasSkipped);
        Assert.Equal(AttachmentProcessingStatusReason.EligibilityNotMet, processed.AttachmentStatus.Reason);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
        Assert.DoesNotContain("6302", processed.ManualProcessingResult!.ExportContent);
        Assert.DoesNotContain("6303", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6304", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6305", processed.ManualProcessingResult.ExportContent);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldNotRunWhenGlobalAutomaticProcessingIsDisabled()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: false,
            Timestamp);

        Assert.Empty(result.Results);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipWhenMonitoringIsNotRunning()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp,
            isMonitoringRunning: false);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(AttachmentProcessingStatusReason.EligibilityNotMet, processed.AttachmentStatus!.Reason);
        Assert.Contains("Überwachung läuft nicht", processed.AttachmentStatus.Message);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipWhenPatientNumberIsMissing()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(AttachmentProcessingStatusReason.EligibilityNotMet, processed.AttachmentStatus!.Reason);
        Assert.Contains("Patientennummer fehlt", processed.AttachmentStatus.Message);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipWhenImportFolderIsMissing()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: string.Empty,
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(AttachmentProcessingStatusReason.EligibilityNotMet, processed.AttachmentStatus!.Reason);
        Assert.Contains("Importordner fehlt", processed.AttachmentStatus.Message);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipWhenExportFolderIsMissing()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult());
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: string.Empty),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(AttachmentProcessingStatusReason.EligibilityNotMet, processed.AttachmentStatus!.Reason);
        Assert.Contains("Exportordner fehlt", processed.AttachmentStatus.Message);
        Assert.Equal(0, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipWhenScannerFindsNoSupportedCandidate()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult(
            new[] { CreateAttachmentCandidate("unsupported.exe", isSupported: false) }));
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(1, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
        Assert.True(processed.AttachmentStatus!.WasSkipped);
        Assert.Equal(AttachmentProcessingStatusReason.NoSupportedAttachment, processed.AttachmentStatus.Reason);
        Assert.DoesNotContain("6302", processed.ManualProcessingResult!.ExportContent);
        Assert.DoesNotContain("6303", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6304", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6305", processed.ManualProcessingResult.ExportContent);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldPrepareExactlyOneSupportedCandidate()
    {
        var candidate = CreateAttachmentCandidate(@"C:\Import\Attachments\report.pdf", isSupported: true);
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult(new[] { candidate }));
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(1, scanner.CallCount);
        Assert.Equal(1, preparationService.CallCount);
        Assert.Equal(candidate.FullPath, preparationService.LastRequest!.SourceAttachmentPath);
        Assert.NotNull(processed.AttachmentStatus);
        Assert.True(processed.AttachmentStatus!.Success);
        Assert.Equal(AttachmentProcessingStatusReason.PreparationSucceeded, processed.AttachmentStatus.Reason);
        Assert.Contains(processed.AttachmentStatus.PreparedFields, field => field.FieldCode == "6302");
        Assert.Contains(processed.AttachmentStatus.PreparedFields, field => field.FieldCode == "6303");
        Assert.Contains(processed.AttachmentStatus.PreparedFields, field => field.FieldCode == "6304");
        Assert.Contains(processed.AttachmentStatus.PreparedFields, field => field.FieldCode == "6305");
        Assert.Contains("6302PDF-Befund", processed.ManualProcessingResult!.ExportContent);
        Assert.Contains("6303PDF", processed.ManualProcessingResult.ExportContent);
        Assert.Contains("6304Messprotokoll", processed.ManualProcessingResult.ExportContent);
        Assert.Contains(@"6305C:\Export\Attachments\report.pdf", processed.ManualProcessingResult.ExportContent);
        Assert.Contains("XDT-Anhang-Linkfelder wurden in die Exportdatei übernommen.", processed.Messages);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipUnstableSupportedCandidate()
    {
        var candidate = CreateAttachmentCandidate(@"C:\Import\Attachments\report.pdf", isSupported: true, isStable: false);
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult(new[] { candidate }));
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(1, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
        Assert.Equal(AttachmentProcessingStatusReason.NoStableAttachment, processed.AttachmentStatus!.Reason);
        Assert.Contains("noch nicht stabil", processed.AttachmentStatus.Message);
        Assert.DoesNotContain("6302", processed.ManualProcessingResult!.ExportContent);
        Assert.DoesNotContain("6303", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6304", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6305", processed.ManualProcessingResult.ExportContent);
    }


    [Fact]
    public void ProcessReadyPairs_AttachmentShouldSkipMultipleSupportedCandidates()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult(new[]
        {
            CreateAttachmentCandidate("report-1.pdf", isSupported: true),
            CreateAttachmentCandidate("report-2.pdf", isSupported: true)
        }));
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult());
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal(1, scanner.CallCount);
        Assert.Equal(0, preparationService.CallCount);
        Assert.Equal(AttachmentProcessingStatusReason.MultipleStableAttachments, processed.AttachmentStatus!.Reason);
        Assert.Contains("mehrere stabile unterstützte Anhänge", processed.AttachmentStatus.Message);
        Assert.DoesNotContain("6302", processed.ManualProcessingResult!.ExportContent);
        Assert.DoesNotContain("6303", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6304", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6305", processed.ManualProcessingResult.ExportContent);
    }

    [Fact]
    public void ProcessReadyPairs_AttachmentPreparationFailureShouldNotBreakPairProcessing()
    {
        var scanner = new FakeAttachmentScanner(CreateAttachmentScanResult(
            new[] { CreateAttachmentCandidate("report.pdf", isSupported: true) }));
        var preparationService = new FakeAttachmentPreparationService(CreateAttachmentPreparationResult(success: false));
        var coordinator = CreateCoordinator(
            new FakeManualProcessor(CreateSuccessResult(patientNumber: "11253")),
            scanner,
            preparationService);

        var result = coordinator.ProcessReadyPairs(
            CreateInterfaceProfile(
                isAttachmentProcessingEnabled: true,
                attachmentImportFolder: @"C:\Import\Attachments",
                attachmentExportFolder: @"C:\Export\Attachments"),
            CreateExportProfile(),
            new[] { CreatePair("patient.gdt", "device.xml") },
            automaticProcessingEnabled: true,
            Timestamp);

        var processed = Assert.Single(result.Results);
        Assert.True(processed.Success);
        Assert.Equal("Automatisch verarbeitet", processed.Status);
        Assert.Equal(AttachmentProcessingStatusReason.PreparationFailed, processed.AttachmentStatus!.Reason);
        Assert.False(processed.AttachmentStatus.Success);
        Assert.Contains("fehlgeschlagen", processed.AttachmentStatus.Message);
        Assert.DoesNotContain("6302", processed.ManualProcessingResult!.ExportContent);
        Assert.DoesNotContain("6303", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6304", processed.ManualProcessingResult.ExportContent);
        Assert.DoesNotContain("6305", processed.ManualProcessingResult.ExportContent);
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
        bool clearDeviceImportFolderBeforeProcessing = false,
        bool isAttachmentProcessingEnabled = false,
        string attachmentImportFolder = "",
        string attachmentExportFolder = "")
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
                ClearDeviceImportFolderBeforeProcessing = clearDeviceImportFolderBeforeProcessing,
                IsAttachmentProcessingEnabled = isAttachmentProcessingEnabled,
                AttachmentImportFolder = attachmentImportFolder,
                AttachmentExportFolder = attachmentExportFolder
            },
            IsActive = true
        };
    }

    private static ExportProfileDefinition CreateExportProfile()
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
    }

    private static InterfaceProfileManualProcessingResult CreateSuccessResult(
        ProcessedFileArchiveResult? archiveResult = null,
        string? patientNumber = "11253")
    {
        return new InterfaceProfileManualProcessingResult(
            Success: true,
            ExportFilePath: "export.xdt",
            ExportContent: "xdt",
            PipelineResult: CreatePipelineResult(patientNumber),
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

    private static AutoImportPairProcessingCoordinator CreateCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        IAttachmentImportFolderScannerService scanner,
        IAttachmentExternalLinkPreparationService preparationService)
    {
        return new AutoImportPairProcessingCoordinator(
            manualProcessor,
            new DuplicateImportFileHandler(),
            new AttachmentAutoProcessingEligibilityService(),
            scanner,
            new AttachmentAutoCandidateSelectionService(),
            preparationService);
    }

    private static ProcessingPipelineResult CreatePipelineResult(string? patientNumber)
    {
        var patient = new PatientData(
            PatientNumber: patientNumber,
            LastName: "Muster",
            FirstName: "Mara",
            BirthDate: null,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);

        return new ProcessingPipelineResult(
            Patient: patient,
            Measurements: Array.Empty<MeasurementValue>(),
            ExportRecords: Array.Empty<ExportFieldRecord>(),
            ExportContent: "xdt",
            Issues: Array.Empty<ProcessingIssue>());
    }

    private static AttachmentImportFolderScanResult CreateAttachmentScanResult(
        IReadOnlyList<AttachmentImportFileCandidate>? candidates = null,
        bool success = true,
        string? errorMessage = null)
    {
        return new AttachmentImportFolderScanResult(
            Success: success,
            ScannedFolder: @"C:\Import\Attachments",
            Candidates: candidates ?? Array.Empty<AttachmentImportFileCandidate>(),
            ErrorMessage: errorMessage);
    }

    private static AttachmentImportFileCandidate CreateAttachmentCandidate(string pathOrFileName, bool isSupported)
    {
        return CreateAttachmentCandidate(pathOrFileName, isSupported, isStable: isSupported);
    }

    private static AttachmentImportFileCandidate CreateAttachmentCandidate(string pathOrFileName, bool isSupported, bool isStable)
    {
        var fullPath = Path.IsPathFullyQualified(pathOrFileName)
            ? pathOrFileName
            : Path.Combine(@"C:\Import\Attachments", pathOrFileName);

        return new AttachmentImportFileCandidate(
            FileName: Path.GetFileName(fullPath),
            Extension: Path.GetExtension(fullPath).ToLowerInvariant(),
            FullPath: fullPath,
            SizeBytes: 123,
            LastWriteTimeUtc: Timestamp,
            IsSupported: isSupported,
            StableStatus: isStable ? "Stabil." : "Noch nicht stabil.",
            ErrorMessage: isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.",
            IsStable: isStable);
    }

    private static AttachmentExternalLinkPreparationResult CreateAttachmentPreparationResult(bool success = true)
    {
        if (!success)
        {
            return new AttachmentExternalLinkPreparationResult(
                Success: false,
                SourcePath: @"C:\Import\Attachments\report.pdf",
                TargetPath: null,
                TargetFileName: null,
                TransferMode: AttachmentTransferMode.Move,
                ExternalAisLinkFieldSet: null,
                ExportFields: Array.Empty<ExportFieldRecord>(),
                ErrorMessage: "Attachment transfer failed.");
        }

        var fields = new[]
        {
            new ExportFieldRecord("6302", "PDF-Befund", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6304", "Messprotokoll", 3),
            new ExportFieldRecord("6305", @"C:\Export\Attachments\report.pdf", 4)
        };

        return new AttachmentExternalLinkPreparationResult(
            Success: true,
            SourcePath: @"C:\Import\Attachments\report.pdf",
            TargetPath: @"C:\Export\Attachments\report.pdf",
            TargetFileName: "report.pdf",
            TransferMode: AttachmentTransferMode.Move,
            ExternalAisLinkFieldSet: new ExternalAisLinkFieldSet(
                DocumentName: "PDF-Befund",
                FileFormat: "PDF",
                Description: "Messprotokoll",
                FullPath: @"C:\Export\Attachments\report.pdf"),
            ExportFields: fields,
            ErrorMessage: null);
    }

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
            DateTime timestamp,
            Func<PatientData, AttachmentProcessingStatus?>? attachmentPreparation = null)
        {
            CallCount++;
            if (!_result.Success || _result.PipelineResult?.Patient is null || attachmentPreparation is null)
            {
                return _result;
            }

            var attachmentStatus = attachmentPreparation(_result.PipelineResult.Patient);
            if (attachmentStatus is null)
            {
                return _result;
            }

            var messages = _result.Messages.ToList();
            messages.Add(attachmentStatus.Message);

            var exportRecords = _result.PipelineResult.ExportRecords;
            var exportContent = _result.ExportContent;
            if (attachmentStatus.Success
                && !attachmentStatus.WasSkipped
                && attachmentStatus.PreparedFields.Count > 0
                && attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6302")
                && attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6303")
                && attachmentStatus.PreparedFields.Any(field => field.FieldCode == "6305"))
            {
                exportRecords = AppendAttachmentFields(exportRecords, attachmentStatus.PreparedFields);
                exportContent = new XdtExportBuilder().Build(exportRecords).Content;
                messages.Add("XDT-Anhang-Linkfelder wurden in die Exportdatei übernommen.");
            }

            return _result with
            {
                ExportContent = exportContent,
                PipelineResult = _result.PipelineResult with
                {
                    ExportRecords = exportRecords,
                    ExportContent = exportContent ?? string.Empty
                },
                Messages = messages,
                AttachmentStatus = attachmentStatus
            };
        }

        private static IReadOnlyList<ExportFieldRecord> AppendAttachmentFields(
            IReadOnlyList<ExportFieldRecord> existingRecords,
            IReadOnlyList<ExportFieldRecord> attachmentFields)
        {
            var combined = existingRecords.ToList();
            var nextSortOrder = combined.Count == 0 ? 0 : combined.Max(record => record.SortOrder);
            foreach (var field in attachmentFields)
            {
                combined.Add(new ExportFieldRecord(field.FieldCode, field.Value, ++nextSortOrder));
            }

            return combined;
        }
    }

    private sealed class FakeAttachmentScanner : IAttachmentImportFolderScannerService
    {
        private readonly AttachmentImportFolderScanResult _result;

        public FakeAttachmentScanner(AttachmentImportFolderScanResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public InterfaceFolderOptions? LastOptions { get; private set; }

        public AttachmentImportFolderScanResult Scan(InterfaceFolderOptions folderOptions)
        {
            CallCount++;
            LastOptions = folderOptions;
            return _result;
        }

        public AttachmentImportFolderScanResult Scan(string attachmentImportFolder)
        {
            CallCount++;
            return _result;
        }
    }

    private sealed class FakeAttachmentPreparationService : IAttachmentExternalLinkPreparationService
    {
        private readonly AttachmentExternalLinkPreparationResult _result;

        public FakeAttachmentPreparationService(AttachmentExternalLinkPreparationResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public AttachmentExternalLinkPreparationRequest? LastRequest { get; private set; }

        public AttachmentExternalLinkPreparationResult Prepare(AttachmentExternalLinkPreparationRequest? request)
        {
            CallCount++;
            LastRequest = request;
            return _result;
        }
    }
}
