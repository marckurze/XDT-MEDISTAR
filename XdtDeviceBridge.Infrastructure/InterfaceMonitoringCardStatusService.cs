using System.IO;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceMonitoringCardStatusService
{
    private readonly IAisPatientDataReader _aisPatientDataReader;
    private readonly Dictionary<string, DateTime> _attachmentWaitStartedAtByPairKey = new(StringComparer.OrdinalIgnoreCase);

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
        var deviceFile = FindFirstDeviceFile(scanResult.Queue, interfaceProfile.FolderOptions.IsAttachmentOnlyMode);
        var readyPair = packageEvaluation?.ReadyPairs.FirstOrDefault()
            ?? scanResult.Queue.FindReadyPairs(interfaceProfile.FolderOptions.IsAttachmentOnlyMode).FirstOrDefault();
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
            .Select(input => UpdateInputFromScan(
                input,
                updatedCard.DeviceName,
                interfaceProfile,
                scanResult,
                packageEvaluation,
                aisFile,
                deviceFile,
                patient,
                scanTimestamp))
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
        if (processingResult.Success)
        {
            ResetProfile(card.InterfaceProfileId);
        }

        var updatedInputs = card.ExpectedInputs
            .Select(input => UpdateInputFromProcessing(input, processingResult, attachmentStatus, patient))
            .ToList();
        var clearProcessedInputs = processingResult.Success;

        return card with
        {
            CurrentStatus = currentStatus,
            StatusClass = statusClass,
            AutomaticProcessingText = automaticProcessingEnabled ? "Ja" : "Nein",
            PatientDisplayText = CreatePatientDisplay(patient),
            AisFileName = clearProcessedInputs ? "" : Path.GetFileName(processingResult.AisFilePath),
            DeviceFileName = clearProcessedInputs ? "" : Path.GetFileName(processingResult.DeviceFilePath),
            AttachmentFileName = attachmentStatus?.TargetFileName ?? FileNameOrEmpty(attachmentStatus?.SourcePath),
            ExportFileName = exportFileName,
            LastSuccessfulExportText = processingResult.Success ? timestamp.ToString("dd.MM.yyyy HH:mm:ss") : card.LastSuccessfulExportText,
            LastMessage = lastMessage,
            ExpectedInputs = updatedInputs
        };
    }

    public void ResetProfile(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        var keyPrefix = $"{interfaceProfileId.Trim()}|";
        foreach (var key in _attachmentWaitStartedAtByPairKey.Keys
            .Where(key => key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList())
        {
            _attachmentWaitStartedAtByPairKey.Remove(key);
        }
    }

    private ExpectedInputDisplayItem UpdateInputFromScan(
        ExpectedInputDisplayItem input,
        string deviceProfileName,
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation,
        PendingImportFile? aisFile,
        PendingImportFile? deviceFile,
        PatientData? patient,
        DateTime scanTimestamp)
    {
        return input.Key switch
        {
            "ais" => CreateAisInput(input, aisFile, patient, packageEvaluation),
            "device" => CreateDeviceInput(input, deviceProfileName, deviceFile, packageEvaluation, aisFile, scanTimestamp, interfaceProfile.FolderOptions.DeviceFileWaitTimeoutMinutes),
            "attachment" => CreateAttachmentInputFromScan(input, interfaceProfile, scanResult, packageEvaluation, scanTimestamp),
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
            "ais" => processingResult.Success
                ? input with
                {
                    Name = "AIS-Patientendatei",
                    Status = "erwartet",
                    StatusClass = "Waiting",
                    Detail = input.FolderPath,
                    DisplayDetail = ""
                }
                : input with
            {
                Name = patient is null ? "AIS-Patientendatei" : CreatePatientMainText(patient, Path.GetFileName(processingResult.AisFilePath)),
                Status = patient is null ? "gefunden" : "",
                StatusClass = "Success",
                Detail = JoinDetails(CreatePatientDisplay(patient, Path.GetFileName(processingResult.AisFilePath)), processingResult.AisFilePath),
                DisplayDetail = patient is null ? Path.GetFileName(processingResult.AisFilePath) : ""
            },
            "device" => processingResult.Success
                ? input with
                {
                    Name = "Geräte-Datei",
                    Status = "erwartet",
                    StatusClass = "Neutral",
                    Detail = input.FolderPath,
                    DisplayDetail = ""
                }
                : input with
            {
                Name = "Empfangen",
                Status = "",
                StatusClass = "Success",
                Detail = processingResult.DeviceFilePath,
                DisplayDetail = Path.GetFileName(processingResult.DeviceFilePath)
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
                Detail = packageEvaluation.Messages.LastOrDefault() ?? "Vorherige AIS-Datei ersetzt.",
                DisplayDetail = "ersetzt"
            };
        }

        if (packageEvaluation?.Reason == AutoImportPackageStateReason.AisFileExpired)
        {
            return input with
            {
                Status = "abgelaufen",
                StatusClass = "Blocked",
                Detail = packageEvaluation.Messages.LastOrDefault() ?? "AIS-Datei abgelaufen.",
                DisplayDetail = "abgelaufen"
            };
        }

        if (aisFile is null)
        {
            return input with
            {
                Name = "AIS-Patientendatei",
                Status = "erwartet",
                StatusClass = "Waiting",
                Detail = input.FolderPath,
                DisplayDetail = ""
            };
        }

        var patientMainText = CreatePatientMainText(patient, aisFile.FileName);
        return input with
        {
            Name = patient is null ? "AIS-Patientendatei" : patientMainText,
            Status = patient is null ? "gefunden" : "",
            StatusClass = patient is null ? "Active" : "Success",
            Detail = JoinDetails(CreatePatientDisplay(patient, aisFile.FilePath), aisFile.FilePath),
            DisplayDetail = patient is null ? aisFile.FileName : ""
        };
    }

    private static ExpectedInputDisplayItem CreateDeviceInput(
        ExpectedInputDisplayItem input,
        string deviceProfileName,
        PendingImportFile? deviceFile,
        AutoImportPackageEvaluationResult? packageEvaluation,
        PendingImportFile? aisFile,
        DateTime scanTimestamp,
        int deviceFileWaitTimeoutMinutes)
    {
        if (deviceFile is null)
        {
            var waitsForDevice = packageEvaluation?.Reason == AutoImportPackageStateReason.WaitingForDeviceFile;
            var remaining = waitsForDevice && aisFile is not null
                ? CreateRemainingTimeText(
                    aisFile.DetectedAtUtc,
                    TimeSpan.FromMinutes(Math.Max(0, deviceFileWaitTimeoutMinutes)),
                    scanTimestamp)
                : "";
            var visibleRemaining = string.Equals(remaining, "Timeout erreicht", StringComparison.Ordinal)
                ? ""
                : remaining;
            return input with
            {
                Name = "Geräte-Datei",
                Status = waitsForDevice ? "wartet auf Gerät" : "erwartet",
                StatusClass = waitsForDevice ? "Waiting" : "Neutral",
                Detail = waitsForDevice
                    ? JoinDetails(packageEvaluation?.Messages.LastOrDefault() ?? "AIS-Datei vorhanden, Gerätedatei fehlt.", visibleRemaining)
                    : input.FolderPath,
                DisplayDetail = waitsForDevice ? visibleRemaining : ""
            };
        }

        return input with
        {
            Name = deviceFile.Status == PendingImportFileStatus.Stable ? "Empfangen" : "Geräte-Datei",
            Status = deviceFile.Status == PendingImportFileStatus.Stable ? "" : "instabil",
            StatusClass = deviceFile.Status == PendingImportFileStatus.Stable ? "Success" : "Waiting",
            Detail = JoinDetails(deviceFile.FilePath, deviceProfileName),
            DisplayDetail = string.IsNullOrWhiteSpace(deviceProfileName) ? deviceFile.FileName : deviceProfileName
        };
    }

    private ExpectedInputDisplayItem CreateAttachmentInputFromScan(
        ExpectedInputDisplayItem input,
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation,
        DateTime scanTimestamp)
    {
        if (!interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled)
        {
            return input with
            {
                Name = "XDT-Anhang",
                Status = "konfiguriert",
                StatusClass = "Neutral",
                Detail = input.Detail,
                DisplayDetail = ""
            };
        }

        var readyPair = packageEvaluation?.ReadyPairs.FirstOrDefault()
            ?? scanResult.Queue.FindReadyPairs(interfaceProfile.FolderOptions.IsAttachmentOnlyMode).FirstOrDefault();
        var pairComplete = readyPair is not null
            || scanResult.ReadyPairs > 0
            || packageEvaluation?.Reason == AutoImportPackageStateReason.ReadyForProcessing;
        if (!pairComplete)
        {
            return input with
            {
                Name = "XDT-Anhang",
                Status = interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required ? "Pflicht" : "Optional",
                StatusClass = "Neutral",
                Detail = input.Detail,
                DisplayDetail = ""
            };
        }

        var waitStartedAt = readyPair is null
            ? scanTimestamp
            : GetOrCreateAttachmentWaitStartedAt(interfaceProfile, readyPair, scanTimestamp);
        var remaining = CreateRemainingTimeText(
            waitStartedAt,
            TimeSpan.FromSeconds(Math.Max(0, interfaceProfile.FolderOptions.AttachmentWaitTimeoutSeconds)),
            scanTimestamp);
        var isRequired = interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required;
        var timeoutReached = remaining == "Timeout erreicht";

        return input with
        {
            Name = "XDT-Anhang",
            Status = timeoutReached && isRequired
                ? "Pflicht blockiert"
                : isRequired ? "Pflicht" : "Optional",
            StatusClass = timeoutReached && isRequired ? "Blocked" : "Waiting",
            Detail = JoinDetails(
                isRequired ? "Wartet auf verpflichtenden XDT-Anhang." : "Wartet auf optionalen XDT-Anhang.",
                remaining),
            DisplayDetail = remaining
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
            AttachmentProcessingStatusReason.PreparationSucceeded => ("", "Success"),
            AttachmentProcessingStatusReason.AttachmentWait => (string.IsNullOrWhiteSpace(input.Status) ? "wartet" : input.Status, "Waiting"),
            AttachmentProcessingStatusReason.AttachmentQuietPeriodWait => ("wartet", "Waiting"),
            AttachmentProcessingStatusReason.AttachmentManualConfirmationWait => ("Bestätigung", "Waiting"),
            AttachmentProcessingStatusReason.NoStableAttachment => ("nicht stabil", "Waiting"),
            AttachmentProcessingStatusReason.AttachmentOptionalTimeoutContinueWithoutAttachment => ("übersprungen", "Neutral"),
            AttachmentProcessingStatusReason.NoSupportedAttachment => ("übersprungen", "Neutral"),
            AttachmentProcessingStatusReason.MultipleSupportedAttachments => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.MultipleStableAttachments => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.MultipleAttachmentsAmbiguous => ("mehrere Anhänge", "Blocked"),
            AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock => ("Pflicht blockiert", "Blocked"),
            AttachmentProcessingStatusReason.PreparationFailed => ("fehlgeschlagen", "Error"),
            AttachmentProcessingStatusReason.ScanError => ("fehlgeschlagen", "Error"),
            _ => attachmentStatus.Success ? ("erfolgreich", "Success") : ("übersprungen", "Neutral")
        };

        var receivedAttachmentName = attachmentStatus.Reason == AttachmentProcessingStatusReason.PreparationSucceeded
            ? $"{CreateAttachmentExtensionLabel(attachmentStatus)} Empfangen"
            : input.Name;

        return input with
        {
            Name = receivedAttachmentName,
            Status = status,
            StatusClass = statusClass,
            Detail = CreateAttachmentDetail(attachmentStatus),
            DisplayDetail = attachmentStatus.Reason == AttachmentProcessingStatusReason.PreparationSucceeded
                ? ""
                : attachmentStatus.Reason is AttachmentProcessingStatusReason.AttachmentWait
                    or AttachmentProcessingStatusReason.AttachmentQuietPeriodWait
                    or AttachmentProcessingStatusReason.AttachmentManualConfirmationWait
                    ? input.DisplayDetail
                    : attachmentStatus.Reason == AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock
                        ? "Timeout erreicht"
                        : ""
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
        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentQuietPeriodWait)
        {
            return "Dokumentgerät wartet auf weitere Dateien";
        }

        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentManualConfirmationWait)
        {
            return "Dokumentgerät wartet auf Benutzerbestätigung";
        }

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
        if (attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentWait
            || attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentQuietPeriodWait
            || attachmentStatus?.Reason == AttachmentProcessingStatusReason.AttachmentManualConfirmationWait)
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
            .FirstOrDefault(file => file.Kind.IsAisImportFile());
    }

    private static PendingImportFile? FindFirstDeviceFile(PendingImportQueue queue, bool includeAttachmentDeviceFiles)
    {
        return queue.GetAll()
            .FirstOrDefault(file => file.Kind.IsDeviceImportFile(includeAttachmentDeviceFiles));
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

    private static string CreatePatientMainText(PatientData? patient, string fallback = "")
    {
        if (patient is null)
        {
            return fallback;
        }

        var fullName = string.Join(" ", new[] { patient.FirstName, patient.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        return string.IsNullOrWhiteSpace(patient.PatientNumber)
            ? fallback
            : patient.PatientNumber;
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

    private static string CreateAttachmentExtensionLabel(AttachmentProcessingStatus attachmentStatus)
    {
        var fileName = attachmentStatus.TargetFileName ?? FileNameOrEmpty(attachmentStatus.SourcePath);
        var extension = Path.GetExtension(fileName);
        return string.IsNullOrWhiteSpace(extension)
            ? "Anhang"
            : extension.TrimStart('.').ToUpperInvariant();
    }

    private static string CreateRemainingTimeText(DateTime startedAtUtc, TimeSpan timeout, DateTime nowUtc)
    {
        if (timeout <= TimeSpan.Zero)
        {
            return "Timeout erreicht";
        }

        var remaining = timeout - (nowUtc.ToUniversalTime() - startedAtUtc.ToUniversalTime());
        if (remaining <= TimeSpan.Zero)
        {
            return "Timeout erreicht";
        }

        return remaining < TimeSpan.FromMinutes(1)
            ? $"noch {Math.Ceiling(remaining.TotalSeconds):0} s"
            : $"noch {remaining:mm\\:ss}";
    }

    private DateTime GetOrCreateAttachmentWaitStartedAt(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime scanTimestamp)
    {
        var key = CreateDisplayPairInstanceKey(interfaceProfile.Metadata.Id, pair);
        if (!_attachmentWaitStartedAtByPairKey.TryGetValue(key, out var startedAt))
        {
            startedAt = scanTimestamp;
            _attachmentWaitStartedAtByPairKey[key] = startedAt;
        }

        return startedAt;
    }

    private static string CreateDisplayPairInstanceKey(string interfaceProfileId, PendingImportPair pair)
    {
        return string.Join(
            "|",
            interfaceProfileId,
            pair.AisFile.FilePath,
            pair.AisFile.DetectedAtUtc.Ticks.ToString(),
            pair.DeviceFile.FilePath,
            pair.DeviceFile.DetectedAtUtc.Ticks.ToString());
    }

    private static DateTime MaxDate(DateTime first, DateTime second)
    {
        return first >= second ? first : second;
    }

    private static string JoinDetails(params string[] details)
    {
        return string.Join(" - ", details.Where(detail => !string.IsNullOrWhiteSpace(detail)));
    }
}
