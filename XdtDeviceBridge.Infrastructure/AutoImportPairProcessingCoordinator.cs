using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AutoImportPairProcessingCoordinator
{
    private readonly IInterfaceProfileManualProcessor _manualProcessor;
    private readonly DuplicateImportFileHandler _duplicateImportFileHandler;
    private readonly AttachmentAutoProcessingEligibilityService _attachmentEligibilityService;
    private readonly IAttachmentImportFolderScannerService _attachmentScannerService;
    private readonly AttachmentAutoCandidateSelectionService _attachmentCandidateSelectionService;
    private readonly IAttachmentExternalLinkPreparationService _attachmentPreparationService;
    private readonly HashSet<string> _processedPairKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingPairKeys = new(StringComparer.OrdinalIgnoreCase);

    public AutoImportPairProcessingCoordinator()
        : this(new InterfaceProfileManualProcessor(), new DuplicateImportFileHandler())
    {
    }

    public AutoImportPairProcessingCoordinator(IInterfaceProfileManualProcessor manualProcessor)
        : this(manualProcessor, new DuplicateImportFileHandler())
    {
    }

    public AutoImportPairProcessingCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        DuplicateImportFileHandler duplicateImportFileHandler)
        : this(
            manualProcessor,
            duplicateImportFileHandler,
            new AttachmentAutoProcessingEligibilityService(),
            new AttachmentImportFolderScannerService(),
            new AttachmentAutoCandidateSelectionService(),
            new AttachmentExternalLinkPreparationService())
    {
    }

    public AutoImportPairProcessingCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        DuplicateImportFileHandler duplicateImportFileHandler,
        AttachmentAutoProcessingEligibilityService attachmentEligibilityService,
        IAttachmentImportFolderScannerService attachmentScannerService,
        AttachmentAutoCandidateSelectionService attachmentCandidateSelectionService,
        IAttachmentExternalLinkPreparationService attachmentPreparationService)
    {
        _manualProcessor = manualProcessor ?? throw new ArgumentNullException(nameof(manualProcessor));
        _duplicateImportFileHandler = duplicateImportFileHandler ?? throw new ArgumentNullException(nameof(duplicateImportFileHandler));
        _attachmentEligibilityService = attachmentEligibilityService ?? throw new ArgumentNullException(nameof(attachmentEligibilityService));
        _attachmentScannerService = attachmentScannerService ?? throw new ArgumentNullException(nameof(attachmentScannerService));
        _attachmentCandidateSelectionService = attachmentCandidateSelectionService ?? throw new ArgumentNullException(nameof(attachmentCandidateSelectionService));
        _attachmentPreparationService = attachmentPreparationService ?? throw new ArgumentNullException(nameof(attachmentPreparationService));
    }

    public AutoImportPairProcessingBatchResult ProcessReadyPairs(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        IEnumerable<PendingImportPair> readyPairs,
        bool automaticProcessingEnabled,
        DateTime timestamp,
        bool isMonitoringRunning = true)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(exportProfile);
        ArgumentNullException.ThrowIfNull(readyPairs);

        if (!automaticProcessingEnabled)
        {
            return new AutoImportPairProcessingBatchResult(
                ProcessedCount: 0,
                SkippedAlreadyProcessedCount: 0,
                ErrorCount: 0,
                Results: Array.Empty<AutoImportPairProcessingResult>());
        }

        var results = new List<AutoImportPairProcessingResult>();
        foreach (var pair in readyPairs.Where(pair => pair.IsReady))
        {
            var pairKey = CreatePairKey(interfaceProfile.Metadata.Id, pair.AisFile.FilePath, pair.DeviceFile.FilePath);
            if (_processedPairKeys.Contains(pairKey))
            {
                var duplicateResult = _duplicateImportFileHandler.HandleAlreadyProcessedPair(
                    interfaceProfile,
                    pair,
                    timestamp);
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairKey,
                    AisFilePath: pair.AisFile.FilePath,
                    DeviceFilePath: pair.DeviceFile.FilePath,
                    WasProcessed: false,
                    WasSkipped: true,
                    Success: false,
                    Status: duplicateResult.Status,
                    ExportFilePath: null,
                    ManualProcessingResult: null,
                    Messages: duplicateResult.Messages));
                continue;
            }

            if (_processingPairKeys.Contains(pairKey))
            {
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairKey,
                    AisFilePath: pair.AisFile.FilePath,
                    DeviceFilePath: pair.DeviceFile.FilePath,
                    WasProcessed: false,
                    WasSkipped: true,
                    Success: false,
                    Status: "Bereits in Verarbeitung",
                    ExportFilePath: null,
                    ManualProcessingResult: null,
                    Messages: new[] { "Paar wird bereits verarbeitet." }));
                continue;
            }

            _processingPairKeys.Add(pairKey);
            try
            {
                var processingResult = _manualProcessor.Process(
                    interfaceProfile,
                    exportProfile,
                    pair.AisFile.FilePath,
                    pair.DeviceFile.FilePath,
                    timestamp);
                var attachmentStatus = processingResult.Success
                    ? PrepareAttachmentIfAllowed(
                        interfaceProfile,
                        processingResult.PipelineResult?.Patient,
                        automaticProcessingEnabled,
                        isMonitoringRunning,
                        timestamp)
                    : null;
                _processedPairKeys.Add(pairKey);
                results.Add(CreateProcessedResult(interfaceProfile, pairKey, pair, processingResult, attachmentStatus));
            }
            catch (Exception ex)
            {
                _processedPairKeys.Add(pairKey);
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairKey,
                    AisFilePath: pair.AisFile.FilePath,
                    DeviceFilePath: pair.DeviceFile.FilePath,
                    WasProcessed: true,
                    WasSkipped: false,
                    Success: false,
                    Status: "Automatischer Fehler",
                    ExportFilePath: null,
                    ManualProcessingResult: null,
                    Messages: new[] { ex.Message }));
            }
            finally
            {
                _processingPairKeys.Remove(pairKey);
            }
        }

        return new AutoImportPairProcessingBatchResult(
            ProcessedCount: results.Count(result => result.WasProcessed && result.Success),
            SkippedAlreadyProcessedCount: results.Count(result => result.WasSkipped),
            ErrorCount: results.Count(result => result.WasProcessed && !result.Success),
            Results: results);
    }

    private static AutoImportPairProcessingResult CreateProcessedResult(
        InterfaceProfileDefinition interfaceProfile,
        string pairKey,
        PendingImportPair pair,
        InterfaceProfileManualProcessingResult processingResult,
        AttachmentProcessingStatus? attachmentStatus)
    {
        return new AutoImportPairProcessingResult(
            PairKey: pairKey,
            AisFilePath: pair.AisFile.FilePath,
            DeviceFilePath: pair.DeviceFile.FilePath,
            WasProcessed: true,
            WasSkipped: false,
            Success: processingResult.Success,
            Status: CreateStatus(interfaceProfile, processingResult),
            ExportFilePath: processingResult.ExportFilePath,
            ManualProcessingResult: processingResult,
            Messages: AppendAttachmentMessage(processingResult.Messages, attachmentStatus),
            AttachmentStatus: attachmentStatus);
    }

    private AttachmentProcessingStatus PrepareAttachmentIfAllowed(
        InterfaceProfileDefinition interfaceProfile,
        PatientData? patient,
        bool automaticProcessingEnabled,
        bool isMonitoringRunning,
        DateTime timestamp)
    {
        var eligibility = _attachmentEligibilityService.Evaluate(
            interfaceProfile,
            patient,
            isMonitoringRunning,
            automaticProcessingEnabled);

        if (!eligibility.IsAllowed)
        {
            return SkippedStatus(
                AttachmentProcessingStatusReason.EligibilityNotMet,
                $"XDT-Anhang übersprungen: {string.Join(" ", eligibility.Reasons)}");
        }

        var scanResult = _attachmentScannerService.Scan(interfaceProfile.FolderOptions);
        var selectionResult = _attachmentCandidateSelectionService.SelectCandidate(scanResult);
        if (!selectionResult.CanProcessAutomatically || selectionResult.SelectedCandidate is null)
        {
            return selectionResult.Reason switch
            {
                AttachmentAutoCandidateSelectionReason.ScanError => SkippedStatus(
                    AttachmentProcessingStatusReason.ScanError,
                    $"XDT-Anhang übersprungen: {selectionResult.ErrorMessage ?? "Importordner konnte nicht eingelesen werden."}"),
                AttachmentAutoCandidateSelectionReason.NoSupportedAttachment => SkippedStatus(
                    AttachmentProcessingStatusReason.NoSupportedAttachment,
                    "XDT-Anhang übersprungen: keine unterstützte Anhangdatei gefunden."),
                AttachmentAutoCandidateSelectionReason.MultipleSupportedAttachments => SkippedStatus(
                    AttachmentProcessingStatusReason.MultipleSupportedAttachments,
                    "XDT-Anhang übersprungen: mehrere unterstützte Anhänge gefunden, keine eindeutige Zuordnung."),
                _ => SkippedStatus(
                    AttachmentProcessingStatusReason.ScanError,
                    $"XDT-Anhang übersprungen: {selectionResult.ErrorMessage ?? "keine eindeutige Auswahl möglich."}")
            };
        }

        var request = new AttachmentExternalLinkPreparationRequest(
            FolderOptions: interfaceProfile.FolderOptions,
            SourceAttachmentPath: selectionResult.SelectedCandidate.FullPath,
            Patient: patient!,
            ProcessingTimestamp: timestamp);

        var preparationResult = _attachmentPreparationService.Prepare(request);
        if (!preparationResult.Success)
        {
            return new AttachmentProcessingStatus(
                WasAttempted: true,
                WasSkipped: false,
                Success: false,
                Reason: AttachmentProcessingStatusReason.PreparationFailed,
                Message: $"XDT-Anhang Vorbereitung fehlgeschlagen: {preparationResult.ErrorMessage}",
                SourcePath: selectionResult.SelectedCandidate.FullPath,
                TargetPath: preparationResult.TargetPath,
                TargetFileName: preparationResult.TargetFileName,
                PreparedFields: Array.Empty<ExportFieldRecord>());
        }

        return new AttachmentProcessingStatus(
            WasAttempted: true,
            WasSkipped: false,
            Success: true,
            Reason: AttachmentProcessingStatusReason.PreparationSucceeded,
            Message: $"XDT-Anhang vorbereitet: {preparationResult.TargetPath}",
            SourcePath: selectionResult.SelectedCandidate.FullPath,
            TargetPath: preparationResult.TargetPath,
            TargetFileName: preparationResult.TargetFileName,
            PreparedFields: preparationResult.ExportFields);
    }

    private static AttachmentProcessingStatus SkippedStatus(
        AttachmentProcessingStatusReason reason,
        string message)
    {
        return new AttachmentProcessingStatus(
            WasAttempted: false,
            WasSkipped: true,
            Success: false,
            Reason: reason,
            Message: message,
            SourcePath: null,
            TargetPath: null,
            TargetFileName: null,
            PreparedFields: Array.Empty<ExportFieldRecord>());
    }

    private static IReadOnlyList<string> AppendAttachmentMessage(
        IReadOnlyList<string> messages,
        AttachmentProcessingStatus? attachmentStatus)
    {
        if (attachmentStatus is null)
        {
            return messages;
        }

        return messages.Concat(new[] { attachmentStatus.Message }).ToList();
    }

    private static string CreateStatus(
        InterfaceProfileDefinition interfaceProfile,
        InterfaceProfileManualProcessingResult processingResult)
    {
        if (processingResult.Success)
        {
            if (processingResult.ArchiveResult is null)
            {
                return "Automatisch verarbeitet";
            }

            if (processingResult.ArchiveResult.HasErrors)
            {
                return "Automatisch verarbeitet, Archivierung mit Fehlern";
            }

            return interfaceProfile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move
                ? "Automatisch verarbeitet und ins Archiv verschoben"
                : "Automatisch verarbeitet und archiviert";
        }

        if (processingResult.FailedFileCopyResult is null)
        {
            return "Automatischer Fehler";
        }

        return processingResult.FailedFileCopyResult.HasErrors
            ? "Automatischer Fehler, Fehlerablage fehlgeschlagen"
            : "Automatischer Fehler, Dateien kopiert";
    }

    public static string CreatePairKey(string interfaceProfileId, string aisFilePath, string deviceFilePath)
    {
        return $"{interfaceProfileId}|{aisFilePath}|{deviceFilePath}";
    }
}
