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
    private readonly IAisPatientDataReader _aisPatientDataReader;
    private readonly AttachmentPackageDecisionService _attachmentPackageDecisionService;
    private readonly AttachmentCompletionService _attachmentCompletionService;
    private readonly TerminalBlockedImportFileHandler _terminalBlockedImportFileHandler;
    private readonly HashSet<string> _processedPairKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _processingPairKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blockedPairKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _pairReadySinceUtc = new(StringComparer.OrdinalIgnoreCase);

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
            new AttachmentExternalLinkPreparationService(),
            new AisPatientDataReader(),
            new AttachmentPackageDecisionService())
    {
    }

    public AutoImportPairProcessingCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        DuplicateImportFileHandler duplicateImportFileHandler,
        AttachmentAutoProcessingEligibilityService attachmentEligibilityService,
        IAttachmentImportFolderScannerService attachmentScannerService,
        AttachmentAutoCandidateSelectionService attachmentCandidateSelectionService,
        IAttachmentExternalLinkPreparationService attachmentPreparationService)
        : this(
            manualProcessor,
            duplicateImportFileHandler,
            attachmentEligibilityService,
            attachmentScannerService,
            attachmentCandidateSelectionService,
            attachmentPreparationService,
            new AisPatientDataReader(),
            new AttachmentPackageDecisionService())
    {
    }

    public AutoImportPairProcessingCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        DuplicateImportFileHandler duplicateImportFileHandler,
        AttachmentAutoProcessingEligibilityService attachmentEligibilityService,
        IAttachmentImportFolderScannerService attachmentScannerService,
        AttachmentAutoCandidateSelectionService attachmentCandidateSelectionService,
        IAttachmentExternalLinkPreparationService attachmentPreparationService,
        IAisPatientDataReader aisPatientDataReader,
        AttachmentPackageDecisionService attachmentPackageDecisionService)
        : this(
            manualProcessor,
            duplicateImportFileHandler,
            attachmentEligibilityService,
            attachmentScannerService,
            attachmentCandidateSelectionService,
            attachmentPreparationService,
            aisPatientDataReader,
            attachmentPackageDecisionService,
            new AttachmentCompletionService(),
            new TerminalBlockedImportFileHandler())
    {
    }

    public AutoImportPairProcessingCoordinator(
        IInterfaceProfileManualProcessor manualProcessor,
        DuplicateImportFileHandler duplicateImportFileHandler,
        AttachmentAutoProcessingEligibilityService attachmentEligibilityService,
        IAttachmentImportFolderScannerService attachmentScannerService,
        AttachmentAutoCandidateSelectionService attachmentCandidateSelectionService,
        IAttachmentExternalLinkPreparationService attachmentPreparationService,
        IAisPatientDataReader aisPatientDataReader,
        AttachmentPackageDecisionService attachmentPackageDecisionService,
        AttachmentCompletionService attachmentCompletionService,
        TerminalBlockedImportFileHandler terminalBlockedImportFileHandler)
    {
        _manualProcessor = manualProcessor ?? throw new ArgumentNullException(nameof(manualProcessor));
        _duplicateImportFileHandler = duplicateImportFileHandler ?? throw new ArgumentNullException(nameof(duplicateImportFileHandler));
        _attachmentEligibilityService = attachmentEligibilityService ?? throw new ArgumentNullException(nameof(attachmentEligibilityService));
        _attachmentScannerService = attachmentScannerService ?? throw new ArgumentNullException(nameof(attachmentScannerService));
        _attachmentCandidateSelectionService = attachmentCandidateSelectionService ?? throw new ArgumentNullException(nameof(attachmentCandidateSelectionService));
        _attachmentPreparationService = attachmentPreparationService ?? throw new ArgumentNullException(nameof(attachmentPreparationService));
        _aisPatientDataReader = aisPatientDataReader ?? throw new ArgumentNullException(nameof(aisPatientDataReader));
        _attachmentPackageDecisionService = attachmentPackageDecisionService ?? throw new ArgumentNullException(nameof(attachmentPackageDecisionService));
        _attachmentCompletionService = attachmentCompletionService ?? throw new ArgumentNullException(nameof(attachmentCompletionService));
        _terminalBlockedImportFileHandler = terminalBlockedImportFileHandler ?? throw new ArgumentNullException(nameof(terminalBlockedImportFileHandler));
    }

    public AutoImportPairProcessingBatchResult ProcessReadyPairs(
        InterfaceProfileDefinition interfaceProfile,
        ExportProfileDefinition exportProfile,
        IEnumerable<PendingImportPair> readyPairs,
        bool automaticProcessingEnabled,
        DateTime timestamp,
        bool isMonitoringRunning = true,
        Func<InterfaceProfileDefinition, PendingImportPair, IReadOnlyList<AttachmentImportFileCandidate>, AttachmentOnlyConfirmationResult>? attachmentOnlyConfirmationProvider = null)
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
            var pairInstanceKey = CreatePairInstanceKey(interfaceProfile.Metadata.Id, pair);
            var processedPairKey = CreateProcessedPairKey(interfaceProfile.Metadata.Id, pair);
            if (_processedPairKeys.Contains(processedPairKey))
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

            if (_blockedPairKeys.Contains(pairInstanceKey))
            {
                continue;
            }

            if (_processingPairKeys.Contains(pairInstanceKey))
            {
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairInstanceKey,
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

            var attachmentGate = EvaluateAttachmentPackageBeforeProcessing(
                interfaceProfile,
                pair,
                pairInstanceKey,
                automaticProcessingEnabled,
                isMonitoringRunning,
                timestamp,
                attachmentOnlyConfirmationProvider);
            if (attachmentGate.DeferredResult is not null)
            {
                results.Add(attachmentGate.DeferredResult);
                continue;
            }

            _processingPairKeys.Add(pairInstanceKey);
            try
            {
                var processingResult = _manualProcessor.Process(
                    interfaceProfile,
                    exportProfile,
                    pair.AisFile.FilePath,
                    pair.DeviceFile.FilePath,
                    timestamp,
                    attachmentGate.AttachmentPreparation,
                    attachmentGate.DocumentationTextProvider);
                _processedPairKeys.Add(processedPairKey);
                _pairReadySinceUtc.Remove(pairInstanceKey);
                _attachmentCompletionService.MarkCompleted(pairInstanceKey);
                results.Add(CreateProcessedResult(interfaceProfile, pairInstanceKey, pair, processingResult, processingResult.AttachmentStatus));
            }
            catch (Exception ex)
            {
                _processedPairKeys.Add(processedPairKey);
                _pairReadySinceUtc.Remove(pairInstanceKey);
                _attachmentCompletionService.MarkCompleted(pairInstanceKey);
                results.Add(new AutoImportPairProcessingResult(
                    PairKey: pairInstanceKey,
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
                _processingPairKeys.Remove(pairInstanceKey);
            }
        }

        return new AutoImportPairProcessingBatchResult(
            ProcessedCount: results.Count(result => result.WasProcessed && result.Success),
            SkippedAlreadyProcessedCount: results.Count(result => result.WasSkipped),
            ErrorCount: results.Count(result => result.WasProcessed && !result.Success),
            Results: results);
    }

    public void ResetProfile(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        var keyPrefix = $"{interfaceProfileId.Trim()}|";
        _processedPairKeys.RemoveWhere(key => key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase));
        _processingPairKeys.RemoveWhere(key => key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase));
        _blockedPairKeys.RemoveWhere(key => key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase));
        foreach (var key in _pairReadySinceUtc.Keys
            .Where(key => key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList())
        {
            _pairReadySinceUtc.Remove(key);
        }

        _attachmentCompletionService.ResetProfile(interfaceProfileId);
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

    private AttachmentGateDecision EvaluateAttachmentPackageBeforeProcessing(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        string pairInstanceKey,
        bool automaticProcessingEnabled,
        bool isMonitoringRunning,
        DateTime timestamp,
        Func<InterfaceProfileDefinition, PendingImportPair, IReadOnlyList<AttachmentImportFileCandidate>, AttachmentOnlyConfirmationResult>? attachmentOnlyConfirmationProvider)
    {
        var attachmentProfile = CreateAttachmentProcessingProfile(interfaceProfile);
        if (!attachmentProfile.FolderOptions.IsAttachmentProcessingEnabled
            || !automaticProcessingEnabled
            || !isMonitoringRunning)
        {
            return new AttachmentGateDecision(
                DeferredResult: null,
                AttachmentPreparation: patient => PrepareAttachmentIfAllowed(
                    attachmentProfile,
                    patient,
                    automaticProcessingEnabled,
                    isMonitoringRunning,
                    timestamp));
        }

        var patientReadResult = _aisPatientDataReader.Read(pair.AisFile.FilePath);
        if (!patientReadResult.Success
            || patientReadResult.Patient is null
            || string.IsNullOrWhiteSpace(patientReadResult.Patient.PatientNumber))
        {
            var status = SkippedStatus(
                AttachmentProcessingStatusReason.EligibilityNotMet,
                "XDT-Anhang übersprungen: AIS-Patientennummer fehlt; keine sichere Zuordnung möglich.");
            return new AttachmentGateDecision(DeferredResult: null, AttachmentPreparation: _ => status);
        }

        var eligibility = _attachmentEligibilityService.Evaluate(
            attachmentProfile,
            patientReadResult.Patient,
            isMonitoringRunning,
            automaticProcessingEnabled);
        if (!eligibility.IsAllowed)
        {
            var status = SkippedStatus(
                AttachmentProcessingStatusReason.EligibilityNotMet,
                $"XDT-Anhang übersprungen: {string.Join(" ", eligibility.Reasons)}");
            return new AttachmentGateDecision(DeferredResult: null, AttachmentPreparation: _ => status);
        }

        var scanResult = _attachmentScannerService.Scan(attachmentProfile.FolderOptions);
        var pairReadySince = GetPairReadySince(pairInstanceKey, timestamp);
        var hasWaitTimedOut = HasAttachmentWaitTimedOut(attachmentProfile, pairReadySince, timestamp);
        var packageDecision = _attachmentPackageDecisionService.Decide(
            attachmentProfile,
            patientReadResult.Patient,
            scanResult,
            isMonitoringRunning,
            automaticProcessingEnabled,
            hasWaitTimedOut);

        if (packageDecision.ShouldWait)
        {
            var status = SkippedStatus(
                    AttachmentProcessingStatusReason.AttachmentWait,
                    "Dateipaar vollständig, warte auf XDT-Anhang.");
            return new AttachmentGateDecision(
                DeferredResult: CreateDeferredResult(
                    pairInstanceKey,
                    pair,
                    "Dateipaar vollständig, warte auf XDT-Anhang.",
                    new[] { status.Message, packageDecision.Message },
                    status),
                AttachmentPreparation: null);
        }

        if (packageDecision.ShouldBlock)
        {
            var message = packageDecision.Reason == AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock
                ? "XDT-Anhang Pflicht: Timeout erreicht, Verarbeitung blockiert."
                : packageDecision.Message;
            var status = SkippedStatus(MapPackageDecisionReason(packageDecision.Reason), message);
            _pairReadySinceUtc.Remove(pairInstanceKey);
            if (packageDecision.Reason == AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock)
            {
                _blockedPairKeys.Add(pairInstanceKey);
                var handlingResult = _terminalBlockedImportFileHandler.Handle(
                    interfaceProfile,
                    pair,
                    timestamp.ToUniversalTime(),
                    message);
                var messages = new[] { message, packageDecision.Message }
                    .Concat(handlingResult.Messages)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                return new AttachmentGateDecision(
                    DeferredResult: CreateTerminalBlockedResult(pairInstanceKey, pair, message, messages, status),
                    AttachmentPreparation: null);
            }

            return new AttachmentGateDecision(
                DeferredResult: CreateDeferredResult(pairInstanceKey, pair, message, new[] { message, packageDecision.Message }, status),
                AttachmentPreparation: null);
        }

        if (packageDecision.CanContinueWithoutAttachment)
        {
            var message = packageDecision.Reason == AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment
                ? "XDT-Anhang optional: Timeout erreicht, Export ohne Anhang."
                : packageDecision.Message;
            var status = SkippedStatus(MapPackageDecisionReason(packageDecision.Reason), message);
            return new AttachmentGateDecision(DeferredResult: null, AttachmentPreparation: _ => status);
        }

        if (packageDecision.CanProcessAttachment && packageDecision.SelectedCandidates.Count > 0)
        {
            var selectedCandidates = packageDecision.SelectedCandidates;
            Func<PatientData, string?>? documentationTextProvider = null;
            if (interfaceProfile.FolderOptions.IsAttachmentOnlyMode)
            {
                var completionDecision = _attachmentCompletionService.Decide(
                    interfaceProfile,
                    pairInstanceKey,
                    selectedCandidates,
                    timestamp.ToUniversalTime());
                if (completionDecision.RequiresManualConfirmation)
                {
                    if (attachmentOnlyConfirmationProvider is null)
                    {
                        var status = SkippedStatus(
                            AttachmentProcessingStatusReason.AttachmentManualConfirmationWait,
                            completionDecision.Message);
                        return new AttachmentGateDecision(
                            DeferredResult: CreateDeferredResult(
                                pairInstanceKey,
                                pair,
                                "Dokumentgerät: wartet auf Benutzerbestätigung.",
                                new[] { completionDecision.Message },
                                status),
                            AttachmentPreparation: null);
                    }

                    var confirmationResult = attachmentOnlyConfirmationProvider(interfaceProfile, pair, completionDecision.SelectedCandidates);
                    if (!confirmationResult.ShouldProcess)
                    {
                        var status = SkippedStatus(
                            AttachmentProcessingStatusReason.AttachmentManualConfirmationWait,
                            "Dokumentgerät: Übertragung nicht bestätigt.");
                        return new AttachmentGateDecision(
                            DeferredResult: CreateDeferredResult(
                                pairInstanceKey,
                                pair,
                                "Dokumentgerät: wartet auf Benutzerbestätigung.",
                                new[] { "Dokumentgerät: Übertragung wurde nicht bestätigt." },
                                status),
                            AttachmentPreparation: null);
                    }

                    selectedCandidates = completionDecision.SelectedCandidates;
                    documentationTextProvider = _ => confirmationResult.DocumentationText;
                }
                else if (completionDecision.ShouldWait)
                {
                    var statusReason = completionDecision.Reason is AttachmentCompletionDecisionReason.QuietPeriodStarted
                        or AttachmentCompletionDecisionReason.QuietPeriodRestarted
                        or AttachmentCompletionDecisionReason.QuietPeriodWaiting
                            ? AttachmentProcessingStatusReason.AttachmentQuietPeriodWait
                            : AttachmentProcessingStatusReason.AttachmentWait;
                    var status = SkippedStatus(statusReason, completionDecision.Message);
                    return new AttachmentGateDecision(
                        DeferredResult: CreateDeferredResult(
                            pairInstanceKey,
                            pair,
                            "Dokumentgerät: wartet auf Abschluss der Dateisammlung.",
                            new[] { completionDecision.Message },
                            status),
                        AttachmentPreparation: null);
                }
                else
                {
                    selectedCandidates = completionDecision.SelectedCandidates;
                    if (interfaceProfile.FolderOptions.ShowAttachmentDocumentationDialog
                        && attachmentOnlyConfirmationProvider is not null)
                    {
                        var confirmationResult = attachmentOnlyConfirmationProvider(interfaceProfile, pair, selectedCandidates);
                        if (!confirmationResult.ShouldProcess)
                        {
                            var status = SkippedStatus(
                                AttachmentProcessingStatusReason.AttachmentManualConfirmationWait,
                                "Dokumentgerät: Dokumentation/Übertragung abgebrochen.");
                            return new AttachmentGateDecision(
                                DeferredResult: CreateDeferredResult(
                                    pairInstanceKey,
                                    pair,
                                    "Dokumentgerät: wartet auf Benutzerbestätigung.",
                                    new[] { "Dokumentgerät: Dokumentation/Übertragung wurde abgebrochen." },
                                    status),
                                AttachmentPreparation: null);
                        }

                        documentationTextProvider = _ => confirmationResult.DocumentationText;
                    }
                }
            }

            return new AttachmentGateDecision(
                DeferredResult: null,
                AttachmentPreparation: patient => PrepareSelectedAttachments(
                    attachmentProfile,
                    patient,
                    selectedCandidates,
                    timestamp),
                DocumentationTextProvider: documentationTextProvider);
        }

        var fallbackStatus = SkippedStatus(
            AttachmentProcessingStatusReason.ScanError,
            $"XDT-Anhang übersprungen: {packageDecision.Message}");
        return new AttachmentGateDecision(DeferredResult: null, AttachmentPreparation: _ => fallbackStatus);
    }

    private static InterfaceProfileDefinition CreateAttachmentProcessingProfile(InterfaceProfileDefinition interfaceProfile)
    {
        if (!interfaceProfile.FolderOptions.IsAttachmentOnlyMode)
        {
            return interfaceProfile;
        }

        return interfaceProfile with
        {
            FolderOptions = interfaceProfile.FolderOptions with
            {
                AttachmentImportFolder = interfaceProfile.FolderOptions.DeviceImportFolder,
                IsAttachmentProcessingEnabled = true,
                AttachmentRequirementMode = AttachmentRequirementMode.Required,
                AttachmentExternalLinkDocumentName = string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExternalLinkDocumentName)
                    ? "Datei"
                    : interfaceProfile.FolderOptions.AttachmentExternalLinkDocumentName,
                AttachmentExternalLinkFileFormat = string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExternalLinkFileFormat)
                    ? "{ExtensionUpperWithoutDot}"
                    : interfaceProfile.FolderOptions.AttachmentExternalLinkFileFormat,
                AttachmentExternalLinkPathTemplate = string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExternalLinkPathTemplate)
                    ? "{Attachment.TargetFullPath}"
                    : interfaceProfile.FolderOptions.AttachmentExternalLinkPathTemplate
            }
        };
    }

    private DateTime GetPairReadySince(string pairKey, DateTime timestamp)
    {
        if (!_pairReadySinceUtc.TryGetValue(pairKey, out var pairReadySince))
        {
            pairReadySince = timestamp;
            _pairReadySinceUtc[pairKey] = pairReadySince;
        }

        return pairReadySince;
    }

    private static bool HasAttachmentWaitTimedOut(
        InterfaceProfileDefinition interfaceProfile,
        DateTime pairReadySince,
        DateTime timestamp)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(0, interfaceProfile.FolderOptions.AttachmentWaitTimeoutSeconds));
        return timestamp - pairReadySince >= timeout;
    }

    private static AutoImportPairProcessingResult CreateDeferredResult(
        string pairKey,
        PendingImportPair pair,
        string status,
        IReadOnlyList<string> messages,
        AttachmentProcessingStatus? attachmentStatus)
    {
        return new AutoImportPairProcessingResult(
            PairKey: pairKey,
            AisFilePath: pair.AisFile.FilePath,
            DeviceFilePath: pair.DeviceFile.FilePath,
            WasProcessed: false,
            WasSkipped: true,
            Success: false,
            Status: status,
            ExportFilePath: null,
            ManualProcessingResult: null,
            Messages: messages,
            AttachmentStatus: attachmentStatus);
    }

    private static AutoImportPairProcessingResult CreateTerminalBlockedResult(
        string pairKey,
        PendingImportPair pair,
        string status,
        IReadOnlyList<string> messages,
        AttachmentProcessingStatus? attachmentStatus)
    {
        return new AutoImportPairProcessingResult(
            PairKey: pairKey,
            AisFilePath: pair.AisFile.FilePath,
            DeviceFilePath: pair.DeviceFile.FilePath,
            WasProcessed: true,
            WasSkipped: false,
            Success: false,
            Status: status,
            ExportFilePath: null,
            ManualProcessingResult: null,
            Messages: messages,
            AttachmentStatus: attachmentStatus);
    }

    private AttachmentProcessingStatus PrepareSelectedAttachments(
        InterfaceProfileDefinition interfaceProfile,
        PatientData patient,
        IReadOnlyList<AttachmentImportFileCandidate> selectedCandidates,
        DateTime timestamp)
    {
        var successfulResults = new List<AttachmentExternalLinkPreparationResult>();
        var failedMessages = new List<string>();

        foreach (var selectedCandidate in selectedCandidates
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase))
        {
            var preparationResult = _attachmentPreparationService.Prepare(new AttachmentExternalLinkPreparationRequest(
                FolderOptions: interfaceProfile.FolderOptions,
                SourceAttachmentPath: selectedCandidate.FullPath,
                Patient: patient,
                ProcessingTimestamp: timestamp,
                IsSourceStable: selectedCandidate.IsStable));
            if (preparationResult.Success)
            {
                successfulResults.Add(preparationResult);
                continue;
            }

            failedMessages.Add($"{selectedCandidate.FileName}: {preparationResult.ErrorMessage}");
        }

        if (successfulResults.Count == 0)
        {
            return new AttachmentProcessingStatus(
                WasAttempted: true,
                WasSkipped: false,
                Success: false,
                Reason: AttachmentProcessingStatusReason.PreparationFailed,
                Message: $"XDT-Anhang Vorbereitung fehlgeschlagen: {string.Join(" ", failedMessages)}",
                SourcePath: string.Join("; ", selectedCandidates.Select(candidate => candidate.FullPath)),
                TargetPath: null,
                TargetFileName: null,
                PreparedFields: Array.Empty<ExportFieldRecord>());
        }

        var fields = successfulResults.SelectMany(result => result.ExportFields).ToList();
        var message = failedMessages.Count == 0
            ? successfulResults.Count == 1
                ? $"XDT-Anhang vorbereitet: {successfulResults[0].TargetPath}"
                : $"XDT-Anhänge vorbereitet: {successfulResults.Count} Datei(en)."
            : $"XDT-Anhänge teilweise vorbereitet: {successfulResults.Count} Datei(en), {failedMessages.Count} fehlgeschlagen.";

        return new AttachmentProcessingStatus(
            WasAttempted: true,
            WasSkipped: false,
            Success: true,
            Reason: AttachmentProcessingStatusReason.PreparationSucceeded,
            Message: message,
            SourcePath: string.Join("; ", successfulResults.Select(result => result.SourcePath)),
            TargetPath: string.Join("; ", successfulResults.Select(result => result.TargetPath).Where(path => !string.IsNullOrWhiteSpace(path))),
            TargetFileName: string.Join("; ", successfulResults.Select(result => result.TargetFileName).Where(fileName => !string.IsNullOrWhiteSpace(fileName))),
            PreparedFields: fields);
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
        if (!selectionResult.CanProcessAutomatically || selectionResult.SelectedCandidates.Count == 0)
        {
            return selectionResult.Reason switch
            {
                AttachmentAutoCandidateSelectionReason.ScanError => SkippedStatus(
                    AttachmentProcessingStatusReason.ScanError,
                    $"XDT-Anhang übersprungen: {selectionResult.ErrorMessage ?? "Importordner konnte nicht eingelesen werden."}"),
                AttachmentAutoCandidateSelectionReason.NoSupportedAttachment => SkippedStatus(
                    AttachmentProcessingStatusReason.NoSupportedAttachment,
                    "XDT-Anhang übersprungen: keine unterstützte Anhangdatei gefunden."),
                AttachmentAutoCandidateSelectionReason.NoStableAttachment => SkippedStatus(
                    AttachmentProcessingStatusReason.NoStableAttachment,
                    "XDT-Anhang noch nicht stabil / wird später erneut geprüft."),
                AttachmentAutoCandidateSelectionReason.MultipleSupportedAttachments => SkippedStatus(
                    AttachmentProcessingStatusReason.MultipleSupportedAttachments,
                    "XDT-Anhang wartet: mehrere unterstützte Anhänge gefunden, aber noch nicht alle stabil."),
                AttachmentAutoCandidateSelectionReason.MultipleStableAttachments => SkippedStatus(
                    AttachmentProcessingStatusReason.MultipleStableAttachments,
                    "XDT-Anhang konnte trotz stabiler Mehrfachauswahl nicht vorbereitet werden."),
                _ => SkippedStatus(
                    AttachmentProcessingStatusReason.ScanError,
                    $"XDT-Anhang übersprungen: {selectionResult.ErrorMessage ?? "keine eindeutige Auswahl möglich."}")
            };
        }

        return PrepareSelectedAttachments(interfaceProfile, patient!, selectionResult.SelectedCandidates, timestamp);
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

        if (messages.Contains(attachmentStatus.Message))
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

    private static AttachmentProcessingStatusReason MapPackageDecisionReason(AttachmentPackageDecisionReason reason)
    {
        return reason switch
        {
            AttachmentPackageDecisionReason.AttachmentOptionalWait or AttachmentPackageDecisionReason.AttachmentRequiredWait
                => AttachmentProcessingStatusReason.AttachmentWait,
            AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment
                => AttachmentProcessingStatusReason.AttachmentOptionalTimeoutContinueWithoutAttachment,
            AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock
                => AttachmentProcessingStatusReason.AttachmentRequiredTimeoutBlock,
            AttachmentPackageDecisionReason.AttachmentNotStableWait
                => AttachmentProcessingStatusReason.NoStableAttachment,
            AttachmentPackageDecisionReason.MultipleAttachmentsAmbiguous
                => AttachmentProcessingStatusReason.MultipleAttachmentsAmbiguous,
            AttachmentPackageDecisionReason.ScanError
                => AttachmentProcessingStatusReason.ScanError,
            AttachmentPackageDecisionReason.MissingPatientNumber or AttachmentPackageDecisionReason.MissingAttachmentFolders
                => AttachmentProcessingStatusReason.EligibilityNotMet,
            _ => AttachmentProcessingStatusReason.None
        };
    }

    public static string CreatePairKey(string interfaceProfileId, string aisFilePath, string deviceFilePath)
    {
        return $"{interfaceProfileId}|{aisFilePath}|{deviceFilePath}";
    }

    private static string CreatePairInstanceKey(string interfaceProfileId, PendingImportPair pair)
    {
        return string.Join(
            "|",
            CreatePairKey(interfaceProfileId, pair.AisFile.FilePath, pair.DeviceFile.FilePath),
            pair.AisFile.DetectedAtUtc.Ticks.ToString(),
            pair.DeviceFile.DetectedAtUtc.Ticks.ToString());
    }

    private static string CreateProcessedPairKey(string interfaceProfileId, PendingImportPair pair)
    {
        return string.Join(
            "|",
            interfaceProfileId,
            "ais",
            ImportFileFingerprint.Create(pair.AisFile),
            "device",
            ImportFileFingerprint.Create(pair.DeviceFile));
    }

    private sealed record AttachmentGateDecision(
        AutoImportPairProcessingResult? DeferredResult,
        Func<PatientData, AttachmentProcessingStatus?>? AttachmentPreparation,
        Func<PatientData, string?>? DocumentationTextProvider = null);
}
