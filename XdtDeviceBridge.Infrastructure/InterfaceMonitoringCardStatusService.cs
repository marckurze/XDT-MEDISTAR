using System.IO;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceMonitoringCardStatusService
{
    private readonly IAisPatientDataReader _aisPatientDataReader;

    public InterfaceMonitoringCardStatusService()
        : this(new AisPatientDataReader())
    {
    }

    public InterfaceMonitoringCardStatusService(IAisPatientDataReader aisPatientDataReader)
    {
        _aisPatientDataReader = aisPatientDataReader ?? throw new ArgumentNullException(nameof(aisPatientDataReader));
    }

    public InterfaceMonitoringCardDisplay ApplyScanResult(
        InterfaceMonitoringCardDisplay card,
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation,
        DateTime scanTimestamp,
        bool automaticProcessingEnabled)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(scanResult);

        var aisFile = FindFirstAisFile(scanResult.Queue);
        var deviceFile = FindFirstDeviceFile(scanResult.Queue);
        var readyPair = scanResult.Queue.FindReadyPairs().FirstOrDefault();
        var patient = TryReadPatient(aisFile);
        var statusText = CreateScanStatusText(scanResult, packageEvaluation, interfaceProfile);
        var statusClass = CreateScanStatusClass(scanResult, packageEvaluation);
        var lastMessage = packageEvaluation?.Messages.LastOrDefault()
            ?? scanResult.Messages.LastOrDefault()
            ?? "";

        var updatedCard = card with
        {
            CurrentStatus = statusText,
            StatusClass = statusClass,
            LastScanText = scanTimestamp.ToString("dd.MM.yyyy HH:mm:ss"),
            AutomaticProcessingText = automaticProcessingEnabled ? "Ja" : "Nein",
            PatientDisplayText = CreatePatientDisplay(patient),
            AisFileName = readyPair?.AisFile.FileName ?? aisFile?.FileName ?? "",
            DeviceFileName = readyPair?.DeviceFile.FileName ?? deviceFile?.FileName ?? "",
            LastMessage = lastMessage
        };

        var expectedInputs = updatedCard.ExpectedInputs
            .Select(input => UpdateInputFromScan(input, interfaceProfile, scanResult, packageEvaluation, aisFile, deviceFile, patient))
            .ToList();

        return updatedCard with
        {
            ExpectedInputs = expectedInputs
        };
    }

    public InterfaceMonitoringCardDisplay ApplyProcessingResult(
        InterfaceMonitoringCardDisplay card,
        AutoImportPairProcessingResult processingResult,
        DateTime timestamp,
        bool automaticProcessingEnabled)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(processingResult);

        var attachmentStatus = processingResult.AttachmentStatus;
        var currentStatus = CreateProcessingStatusText(processingResult, attachmentStatus);
        var statusClass = CreateProcessingStatusClass(processingResult, attachmentStatus);
        var patient = processingResult.ManualProcessingResult?.PipelineResult?.Patient;
        var exportFileName = string.IsNullOrWhiteSpace(processingResult.ExportFilePath)
            ? ""
            : Path.GetFileName(processingResult.ExportFilePath);
        var lastMessage = attachmentStatus?.Message
            ?? processingResult.Messages.LastOrDefault()
            ?? processingResult.Status;

        var updatedInputs = card.ExpectedInputs
            .Select(input => UpdateInputFromProcessing(input, processingResult, attachmentStatus, patient))
            .ToList();

        return card with
        {
            CurrentStatus = currentStatus,
            StatusClass = statusClass,
            AutomaticProcessingText = automaticProcessingEnabled ? "Ja" : "Nein",
            PatientDisplayText = CreatePatientDisplay(patient),
            AisFileName = Path.GetFileName(processingResult.AisFilePath),
            DeviceFileName = Path.GetFileName(processingResult.DeviceFilePath),
            AttachmentFileName = attachmentStatus?.TargetFileName ?? FileNameOrEmpty(attachmentStatus?.SourcePath),
            ExportFileName = exportFileName,
            LastSuccessfulExportText = processingResult.Success ? timestamp.ToString("dd.MM.yyyy HH:mm:ss") : card.LastSuccessfulExportText,
            LastMessage = lastMessage,
            ExpectedInputs = updatedInputs
        };
    }

    private ExpectedInputDisplayItem UpdateInputFromScan(
        ExpectedInputDisplayItem input,
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation,
        PendingImportFile? aisFile,
        PendingImportFile? deviceFile,
        PatientData? patient)
    {
        return input.Key switch
        {
            "ais" => CreateAisInput(input, aisFile, patient, packageEvaluation),
            "device" => CreateDeviceInput(input, deviceFile, packageEvaluation),
            "attachment" => CreateAttachmentInputFromScan(input, interfaceProfile, scanResult, packageEvaluation),
            _ => input
        };
    }

    private static ExpectedInputDisplayItem UpdateInputFromProcessing(
        ExpectedInputDisplayItem input,
        AutoImportPairProcessingResult processingResult,
        AttachmentProcessingStatus? attachmentStatus,
        PatientData? patient)
    {
        return input.Key switch
        {
            "ais" => input with
            {
                Status = patient is null ? "gefunden" : "Patient erkannt",
                StatusClass = "Success",
                Detail = CreatePatientDisplay(patient, Path.GetFileName(processingResult.AisFilePath))
            },
            "device" => input with
            {
                Status = "gefunden",
                StatusClass = "Success",
                Detail = Path.GetFileName(processingResult.DeviceFilePath)
            },
            "attachment" => CreateAttachmentInputFromProcessing(input, attachmentStatus),
            _ => input
        };
    }

    private ExpectedInputDisplayItem CreateAisInput(
        ExpectedInputDisplayItem input,
        PendingImportFile? aisFile,
        PatientData? patient,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileReplaced)
        {
            return input with
            {
                Status = "ersetzt",
                StatusClass = "Waiting",
                Detail = packageEvaluation.Messages.LastOrDefault() ?? "Vorherige AIS-Datei ersetzt."
            };
        }

        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileExpired)
        {
            return input with
            {
                Status = "abgelaufen",
                StatusClass = "Blocked",
                Detail = packageEvaluation.Messages.LastOrDefault() ?? "AIS-Datei abgelaufen."
            };
        }

        if (aisFile is null)
        {
            return input with
            {
                Status = "erwartet",
                StatusClass = "Waiting",
                Detail = input.FolderPath
            };
        }

        return input with
        {
            Status = patient is null ? "gefunden" : "Patient erkannt",
            StatusClass = patient is null ? "Active" : "Success",
            Detail = CreatePatientDisplay(patient, aisFile.FileName)
        };
    }

    private static ExpectedInputDisplayItem CreateDeviceInput(
        ExpectedInputDisplayItem input,
        PendingImportFile? deviceFile,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (deviceFile is null)
        {
            var waitsForDevice = packageEvaluation?.Reason == AutoImportPackageStateReason.WaitingForDeviceFile;
            return input with
            {
                Status = waitsForDevice ? "wartet auf Gerät" : "erwartet",
                StatusClass = waitsForDevice ? "Waiting" : "Neutral",
                Detail = waitsForDevice
                    ? packageEvaluation?.Messages.LastOrDefault() ?? "AIS-Datei vorhanden, Gerätedatei fehlt."
                    : input.FolderPath
            };
        }

        return input with
        {
            Status = deviceFile.Status == PendingImportFileStatus.Stable ? "gefunden" : "instabil",
            StatusClass = deviceFile.Status == PendingImportFileStatus.Stable ? "Success" : "Waiting",
            Detail = deviceFile.FileName
        };
    }

    private static ExpectedInputDisplayItem CreateAttachmentInputFromScan(
        ExpectedInputDisplayItem input,
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (!interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled)
        {
            return input with
            {
                Status = "konfiguriert",
                StatusClass = "Neutral",
                Detail = input.Detail
            };
        }

        var pairComplete = scanResult.ReadyPairs > 0
            || packageEvaluation?.Reason == AutoImportPackageStateReason.ReadyForProcessing;
        if (!pairComplete)
        {
            return input with
            {
                Status = interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required ? "Pflicht" : "optional",
                StatusClass = "Neutral",
                Detail = input.Detail
            };
        }

        return input with
        {
            Status = "wartet",
            StatusClass = "Waiting",
            Detail = interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required
                ? "Wartet auf verpflichtenden XDT-Anhang."
                : "Wartet auf optionalen XDT-Anhang."
        };
    }

    private static ExpectedInputDisplayItem CreateAttachmentInputFromProcessing(
        ExpectedInputDisplayItem input,
        AttachmentProcessingStatus? attachmentStatus)
    {
        if (attachmentStatus is null)
        {
            return input;
        }

        var (status, statusClass) = attachmentStatus.Reason switch
        {
            AttachmentProcessingStatusReason.PreparationSucceeded => ("erfolgreich", "Success"),
            AttachmentProcessingStatusReason.AttachmentWait => ("wartet", "Waiting"),
            AttachmentProcessingStatusReason.NoStableAttachment => ("nicht stabil", "Waiting"),
            AttachmentProcessingStatusReason.AttachmentOptionalTimeoutContinueWithoutAttachment => ("übersprungen", "Neutral"),
            AttachmentProcessingStatusReason.NoSupportedAttachment => ("übersprungen", "Neutral"),
            AttachmentProcessingStatusReason.MultipleSupportedAttachments => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.MultipleStableAttachments => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.MultipleAttachmentsAmbiguous => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock => ("blockiert", "Blocked"),
            AttachmentProcessingStatusReason.PreparationFailed => ("fehlgeschlagen", "Error"),
            AttachmentProcessingStatusReason.ScanError => ("fehlgeschlagen", "Error"),
            _ => attachmentStatus.Success ? ("erfolgreich", "Success") : ("übersprungen", "Neutral")
        };

        return input with
        {
            Status = status,
            StatusClass = statusClass,
            Detail = CreateAttachmentDetail(attachmentStatus)
        };
    }

    private static string CreateScanStatusText(
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation,
        InterfaceProfileDefinition interfaceProfile)
    {
        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileExpired)
        {
            return "AIS-Datei abgelaufen";
        }

        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileReplaced)
        {
            return "AIS-Datei ersetzt";
        }

        if (scanResult.ReadyPairs > 0 && interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled)
        {
            return "Wartet auf XDT-Anhang";
        }

        if (scanResult.ReadyPairs > 0)
        {
            return "AIS-/Geräte-Paar vollständig";
        }

        if (packageEvaluation?.Reason == AutoImportPackageStateReason.WaitingForDeviceFile
            || (scanResult.AisFilesDetected > 0 && scanResult.DeviceFilesDetected == 0))
        {
            return "Wartet auf Gerät";
        }

        if (scanResult.DeviceFilesDetected > 0 && scanResult.AisFilesDetected == 0)
        {
            return "Wartet auf AIS";
        }

        return "Wartet auf AIS";
    }

    private static string CreateScanStatusClass(
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileExpired)
        {
            return "Blocked";
        }

        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileReplaced)
        {
            return "Waiting";
        }

        if (scanResult.ReadyPairs > 0)
        {
            return "Active";
        }

        return "Waiting";
    }

    private static string CreateProcessingStatusText(
        AutoImportPairProcessingResult processingResult,
        AttachmentProcessingStatus? attachmentStatus)
    {
        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentWait)
        {
            return "Wartet auf XDT-Anhang";
        }

        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock)
        {
            return "XDT-Anhang Pflicht blockiert";
        }

        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentOptionalTimeoutContinueWithoutAttachment)
        {
            return processingResult.Success ? "XDT-Anhang übersprungen" : "Export fehlgeschlagen";
        }

        if (processingResult.Success)
        {
            return "Export erfolgreich";
        }

        if (processingResult.WasProcessed)
        {
            return "Export fehlgeschlagen";
        }

        return processingResult.WasSkipped ? processingResult.Status : "Verarbeitung läuft";
    }

    private static string CreateProcessingStatusClass(
        AutoImportPairProcessingResult processingResult,
        AttachmentProcessingStatus? attachmentStatus)
    {
        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentWait)
        {
            return "Waiting";
        }

        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock
            || attachmentStatus?.Reason == AttachmentProcessingStatusReason.MultipleAttachmentsAmbiguous)
        {
            return "Blocked";
        }

        if (processingResult.Success)
        {
            return "Success";
        }

        if (processingResult.WasProcessed && !processingResult.Success)
        {
            return "Error";
        }

        return "Active";
    }

    private PatientData? TryReadPatient(PendingImportFile? aisFile)
    {
        if (aisFile is null || aisFile.Status != PendingImportFileStatus.Stable)
        {
            return null;
        }

        var result = _aisPatientDataReader.Read(aisFile.FilePath);
        return result.Success ? result.Patient : null;
    }

    private static PendingImportFile? FindFirstAisFile(PendingImportQueue queue)
    {
        return queue.GetAll()
            .FirstOrDefault(file => file.Kind == ImportFileKind.AisGdt || file.Kind == ImportFileKind.AisXdt);
    }

    private static PendingImportFile? FindFirstDeviceFile(PendingImportQueue queue)
    {
        return queue.GetAll()
            .FirstOrDefault(file => file.Kind == ImportFileKind.DeviceXml
                || file.Kind == ImportFileKind.DeviceText
                || file.Kind == ImportFileKind.DeviceCsv);
    }

    private static string CreatePatientDisplay(PatientData? patient, string fallback = "")
    {
        if (patient is null)
        {
            return fallback;
        }

        var fullName = string.Join(" ", new[] { patient.FirstName, patient.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return $"Patient: {fullName}";
        }

        return string.IsNullOrWhiteSpace(patient.PatientNumber)
            ? fallback
            : $"Patient: {patient.PatientNumber}";
    }

    private static string CreateAttachmentDetail(AttachmentProcessingStatus attachmentStatus)
    {
        if (!string.IsNullOrWhiteSpace(attachmentStatus.TargetFileName))
        {
            return $"{attachmentStatus.Message} ({attachmentStatus.TargetFileName})";
        }

        if (!string.IsNullOrWhiteSpace(attachmentStatus.SourcePath))
        {
            return $"{attachmentStatus.Message} ({Path.GetFileName(attachmentStatus.SourcePath)})";
        }

        return attachmentStatus.Message;
    }

    private static string FileNameOrEmpty(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? "" : Path.GetFileName(path);
    }
}
