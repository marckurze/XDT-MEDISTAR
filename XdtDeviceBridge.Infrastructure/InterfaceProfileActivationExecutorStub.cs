using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationExecutorStub : IInterfaceProfileActivationExecutor
{
    private readonly IInterfaceProfileActivationProfileStore? _profileStore;
    private readonly InterfaceProfileActivationEvaluationService? _evaluationService;
    private readonly InterfaceProfileActivationGuardService? _guardService;
    private readonly ProfileCatalog? _finalEvaluationCatalog;

    public InterfaceProfileActivationExecutorStub()
        : this(null)
    {
    }

    public InterfaceProfileActivationExecutorStub(IInterfaceProfileActivationProfileStore? profileStore)
        : this(
            profileStore,
            evaluationService: null,
            guardService: null,
            finalEvaluationCatalog: null)
    {
    }

    public InterfaceProfileActivationExecutorStub(
        IInterfaceProfileActivationProfileStore? profileStore,
        InterfaceProfileActivationEvaluationService? evaluationService,
        InterfaceProfileActivationGuardService? guardService,
        ProfileCatalog? finalEvaluationCatalog)
    {
        _profileStore = profileStore;
        _evaluationService = evaluationService;
        _guardService = guardService;
        _finalEvaluationCatalog = finalEvaluationCatalog;
    }

    public Task<InterfaceProfileActivationExecutorResult> ExecuteAsync(
        InterfaceProfileActivationExecutorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.OperationMode == InterfaceProfileActivationExecutorOperationMode.Activate)
        {
            return Task.FromResult(CreateNonProductiveActivateResult(request));
        }

        var shouldUseStore = _profileStore is not null;
        var loadResult = shouldUseStore
            ? _profileStore!.LoadFreshUserDefinedProfile(request.EffectiveInterfaceProfileId)
            : null;
        var effectiveProfile = loadResult?.Profile ?? request.Profile;
        var finalValidation = CreateFinalValidationContext(effectiveProfile, loadResult, shouldUseStore);
        var evaluation = finalValidation.EvaluationResult ?? request.EvaluationResult;
        var guard = finalValidation.GuardResult ?? request.GuardResult;
        var canRunSaveDryRun = CanRunSaveDryRun(effectiveProfile, finalValidation);
        var saveDryRunBlocked = shouldUseStore &&
            effectiveProfile is not null &&
            loadResult?.Success == true &&
            !canRunSaveDryRun;
        var saveDryRunResult = canRunSaveDryRun
            ? _profileStore!.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
                Profile: effectiveProfile,
                Source: request.Source,
                RequestedAtUtc: request.RequestedAtUtc,
                OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
                FinalReEvaluationCompleted: true))
            : null;

        var preconditions = CreatePreconditions(
            request,
            effectiveProfile,
            evaluation,
            guard,
            loadResult,
            saveDryRunResult,
            finalValidation,
            hasProfileStore: _profileStore is not null,
            shouldUseStore,
            saveDryRunBlocked);
        var status = DetermineStatus(
            request,
            effectiveProfile,
            evaluation,
            guard,
            loadResult,
            shouldUseStore);

        return Task.FromResult(new InterfaceProfileActivationExecutorResult(
            Status: status,
            Success: false,
            Message: CreateMessage(status, loadResult, saveDryRunResult, finalValidation, shouldUseStore, saveDryRunBlocked),
            Preconditions: preconditions,
            ProfileChanged: false,
            Saved: false,
            ProcessingStarted: false,
            RequiresFreshLoad: loadResult?.Success != true,
            RequiresSafeUserDefinedStore: true,
            RequiresFinalEvaluation: !finalValidation.IsComplete,
            IsValidationOnly: true,
            MissingCapabilities: CreateMissingCapabilities(preconditions),
            FreshLoadPerformed: loadResult is not null,
            FinalEvaluationPerformed: finalValidation.EvaluationPerformed,
            GuardRechecked: finalValidation.GuardRechecked,
            SaveDryRunPerformed: saveDryRunResult is not null,
            SaveDryRunBlocked: saveDryRunBlocked));
    }

    private InterfaceProfileActivationExecutorResult CreateNonProductiveActivateResult(
        InterfaceProfileActivationExecutorRequest request)
    {
        var preconditions = new[]
        {
            Precondition(
                "executor.nonProductiveStub",
                "Defensive Executor-Stufe",
                "Diese Implementierung ist absichtlich nicht produktiv und fuehrt keine Aktivierung aus.",
                isSatisfied: false),
            Precondition(
                "operation.activate.notImplemented",
                "Produktive Aktivierung nicht implementiert",
                "Der Activate-Modus ist in V1-Vorbereitung bewusst nicht an UI, Speicherung oder Verarbeitung angebunden.",
                isSatisfied: false),
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Ein spaeterer Executor muss das Zielprofil eindeutig frisch laden koennen.",
                !string.IsNullOrWhiteSpace(request.EffectiveInterfaceProfileId))
        };

        return new InterfaceProfileActivationExecutorResult(
            Status: InterfaceProfileActivationExecutorStatus.NotImplemented,
            Success: false,
            Message: "Produktive Aktivierung ist nicht implementiert. Es wurde nichts aktiviert, gespeichert oder verarbeitet.",
            Preconditions: preconditions,
            ProfileChanged: false,
            Saved: false,
            ProcessingStarted: false,
            RequiresFreshLoad: true,
            RequiresSafeUserDefinedStore: true,
            RequiresFinalEvaluation: true,
            IsValidationOnly: false,
            MissingCapabilities: CreateMissingCapabilities(preconditions));
    }

    private FinalValidationContext CreateFinalValidationContext(
        InterfaceProfileDefinition? effectiveProfile,
        InterfaceProfileActivationProfileLoadResult? loadResult,
        bool shouldUseStore)
    {
        if (!shouldUseStore || loadResult?.Success != true || effectiveProfile is null)
        {
            return FinalValidationContext.NotAttempted();
        }

        var preconditions = CreateFinalValidationPreconditions().ToList();
        var canRunFinalValidation =
            _evaluationService is not null &&
            _guardService is not null &&
            _finalEvaluationCatalog is not null;

        if (!canRunFinalValidation)
        {
            return new FinalValidationContext(
                EvaluationResult: null,
                GuardResult: null,
                EvaluationPerformed: false,
                GuardRechecked: false,
                Preconditions: preconditions,
                IsComplete: false);
        }

        var evaluation = _evaluationService!.Evaluate(effectiveProfile, _finalEvaluationCatalog!);
        var guard = _guardService!.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            effectiveProfile,
            evaluation,
            Context: "ExecutorValidateOnlyFinalEvaluation"));

        preconditions.AddRange(new[]
        {
            Precondition(
                "executor.finalEvaluation.performed",
                "Finale Bewertung ausgefuehrt",
                "ValidateOnly hat eine frische Aktivierungsbewertung fuer das frisch geladene Profil erzeugt.",
                isSatisfied: true,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "executor.finalGuard.performed",
                "Guard erneut ausgefuehrt",
                "ValidateOnly hat die technische Schutzpruefung erneut auf Basis der frischen Bewertung ausgefuehrt.",
                isSatisfied: true,
                InterfaceProfileActivationSeverity.Info)
        });

        return new FinalValidationContext(
            evaluation,
            guard,
            EvaluationPerformed: true,
            GuardRechecked: true,
            Preconditions: preconditions,
            IsComplete: true);
    }

    private IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateFinalValidationPreconditions()
    {
        return new[]
        {
            Precondition(
                "executor.finalEvaluationService.missing",
                "Finaler Bewertungsservice verfuegbar",
                "ValidateOnly benoetigt den Aktivierungsbewertungsservice fuer eine frische finale Bewertung.",
                _evaluationService is not null),
            Precondition(
                "executor.finalProfileCatalog.missing",
                "Finaler Profilkatalog verfuegbar",
                "Die finale Aktivierungsbewertung benoetigt den aktuellen Profilkatalog fuer Abhaengigkeitspruefungen.",
                _finalEvaluationCatalog is not null),
            Precondition(
                "executor.finalGuardService.missing",
                "Finaler Guard-Service verfuegbar",
                "ValidateOnly benoetigt den Guard-Service fuer eine erneute technische Schutzpruefung.",
                _guardService is not null)
        };
    }

    private static bool CanRunSaveDryRun(
        InterfaceProfileDefinition? effectiveProfile,
        FinalValidationContext finalValidation)
    {
        if (effectiveProfile is null ||
            effectiveProfile.Metadata.IsBuiltIn ||
            !effectiveProfile.Metadata.IsUserDefined ||
            !finalValidation.IsComplete ||
            finalValidation.EvaluationResult is null ||
            finalValidation.GuardResult is null)
        {
            return false;
        }

        return finalValidation.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            finalValidation.EvaluationResult.Blockers.Count == 0 &&
            finalValidation.EvaluationResult.Warnings.Count == 0 &&
            finalValidation.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Allowed &&
            finalValidation.GuardResult.CanProceed;
    }

    private static InterfaceProfileActivationExecutorStatus DetermineStatus(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
        InterfaceProfileActivationEvaluationResult? evaluation,
        InterfaceProfileActivationGuardResult? guard,
        InterfaceProfileActivationProfileLoadResult? loadResult,
        bool shouldUseStore)
    {
        if (shouldUseStore && loadResult is not null)
        {
            if (string.IsNullOrWhiteSpace(request.EffectiveInterfaceProfileId) ||
                loadResult.Status is InterfaceProfileActivationProfileStoreStatus.NotAvailable or
                    InterfaceProfileActivationProfileStoreStatus.NotFound or
                    InterfaceProfileActivationProfileStoreStatus.Failed)
            {
                return InterfaceProfileActivationExecutorStatus.NotAvailable;
            }

            if (loadResult.Status is InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked or
                InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked or
                InterfaceProfileActivationProfileStoreStatus.UserDefinedRequired)
            {
                return InterfaceProfileActivationExecutorStatus.Blocked;
            }
        }

        if (string.IsNullOrWhiteSpace(request.EffectiveInterfaceProfileId) ||
            effectiveProfile is null ||
            evaluation is null ||
            guard is null)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (effectiveProfile.Metadata.IsBuiltIn ||
            !effectiveProfile.Metadata.IsUserDefined ||
            evaluation.ActivationStatus == InterfaceProfileActivationStatus.Blocked ||
            evaluation.Blockers.Count > 0 ||
            evaluation.Warnings.Count > 0 ||
            guard.Decision == InterfaceProfileActivationGuardDecision.Blocked)
        {
            return InterfaceProfileActivationExecutorStatus.Blocked;
        }

        if (evaluation.ActivationStatus is InterfaceProfileActivationStatus.Unknown or
                InterfaceProfileActivationStatus.NotEvaluated ||
            guard.Decision == InterfaceProfileActivationGuardDecision.Unknown)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (evaluation.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            guard.Decision == InterfaceProfileActivationGuardDecision.Allowed &&
            guard.CanProceed)
        {
            return InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted;
        }

        return InterfaceProfileActivationExecutorStatus.NotImplemented;
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreatePreconditions(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
        InterfaceProfileActivationEvaluationResult? evaluation,
        InterfaceProfileActivationGuardResult? guard,
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult,
        FinalValidationContext finalValidation,
        bool hasProfileStore,
        bool shouldUseStore,
        bool saveDryRunBlocked)
    {
        var effectiveProfileId = request.EffectiveInterfaceProfileId;
        var freshLoadSucceeded = loadResult?.Success == true;

        var preconditions = new List<InterfaceProfileActivationExecutorPrecondition>
        {
            Precondition(
                "executor.nonProductiveStub",
                "Defensive Executor-Stufe",
                "Diese Implementierung ist absichtlich nicht produktiv und fuehrt keine Aktivierung aus.",
                isSatisfied: false),
            Precondition(
                "executor.freshReload.missing",
                hasProfileStore ? "Frisches Laden ueber Store moeglich" : "Frisches Laden nicht angebunden",
                hasProfileStore
                    ? "Im ValidateOnly-Modus kann der Stub ein Zielprofil ueber den angebundenen Store frisch laden."
                    : "Der Stub hat keinen Loader/Profilkatalog fuer eine finale frische Ladung.",
                freshLoadSucceeded),
            Precondition(
                "executor.storeContext.missing",
                hasProfileStore ? "Store-Kontext angebunden" : "Store-Kontext nicht angebunden",
                hasProfileStore
                    ? "Ein IInterfaceProfileActivationProfileStore ist am Stub angebunden; produktives Speichern bleibt deaktiviert."
                    : "Ein produktiver Executor muss Loader/Store sicher erhalten.",
                hasProfileStore),
            Precondition(
                "executor.safePersistence.missing",
                "Sichere UserDefined-Speicherung nicht produktiv angebunden",
                "Eine produktive Speicherung des UserDefined-Schnittstellenprofils ist in dieser Executor-Stufe nicht angebunden.",
                isSatisfied: false),
            Precondition(
                "executor.finalEvaluation.required",
                "Finale Bewertung vorbereitet",
                "ValidateOnly kann die finale Bewertung nur bei angebundenem Bewertungsservice, Guard-Service und Profilkatalog simulieren.",
                finalValidation.IsComplete),
            Precondition(
                "operation.mode.modeled",
                "Ausfuehrungsmodus modelliert",
                "Der Request unterscheidet ValidateOnly und Activate, ohne daraus im Stub produktive Aktionen abzuleiten.",
                Enum.IsDefined(typeof(InterfaceProfileActivationExecutorOperationMode), request.OperationMode),
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Ein spaeterer Executor muss das Zielprofil eindeutig frisch laden koennen.",
                !string.IsNullOrWhiteSpace(effectiveProfileId)),
            Precondition(
                "profile.present",
                "Schnittstellenprofil vorhanden",
                "Ein Schnittstellenprofil muss ausgewaehlt oder frisch geladen sein.",
                effectiveProfile is not null),
            Precondition(
                "profile.userDefined",
                "Profil ist UserDefined",
                "Aktivierung ist nur fuer UserDefined-Schnittstellenprofile vorgesehen.",
                effectiveProfile?.Metadata.IsUserDefined == true),
            Precondition(
                "profile.notBuiltIn",
                "Profil ist nicht BuiltIn",
                "BuiltIn-Profile duerfen nicht direkt aktiviert oder veraendert werden.",
                effectiveProfile?.Metadata.IsBuiltIn == false),
            Precondition(
                "activationFlag.isActive",
                "Aktivierungsflag identifiziert",
                "`IsActive` ist im bestehenden Schnittstellenprofilmodell als Aktivierungskennzeichen vorhanden.",
                effectiveProfile is not null,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "evaluation.present",
                finalValidation.EvaluationPerformed ? "Finale Aktivierungsbewertung vorhanden" : "Aktivierungsbewertung vorhanden",
                finalValidation.EvaluationPerformed
                    ? "ValidateOnly hat eine frische Aktivierungsbewertung erzeugt."
                    : "Eine aktuelle Aktivierungsbewertung muss vorliegen.",
                evaluation is not null),
            Precondition(
                "evaluation.readyWithoutWarnings",
                "Bewertung ist Ready ohne Warnungen",
                "V1 aktiviert spaeter nur Ready ohne Blocker und ohne Warnungen.",
                evaluation?.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
                evaluation.Blockers.Count == 0 &&
                evaluation.Warnings.Count == 0),
            Precondition(
                "guard.present",
                finalValidation.GuardRechecked ? "Finale Guard-Entscheidung vorhanden" : "Guard-Entscheidung vorhanden",
                finalValidation.GuardRechecked
                    ? "ValidateOnly hat die technische Schutzpruefung erneut ausgefuehrt."
                    : "Eine technische Schutzpruefung muss vorliegen.",
                guard is not null),
            Precondition(
                "guard.allowed",
                "Guard erlaubt Aktivierung",
                "Der Guard muss Allowed liefern.",
                guard?.Decision == InterfaceProfileActivationGuardDecision.Allowed &&
                guard.CanProceed)
        };

        preconditions.AddRange(finalValidation.Preconditions);

        if (shouldUseStore)
        {
            preconditions.AddRange(CreateStorePreconditions(
                loadResult,
                saveDryRunResult,
                finalValidation.IsComplete,
                saveDryRunBlocked));
        }

        return preconditions;
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateStorePreconditions(
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult,
        bool finalEvaluationCompleted,
        bool saveDryRunBlocked)
    {
        return new[]
        {
            Precondition(
                "store.freshLoad.attempted",
                "Frisches Laden versucht",
                "ValidateOnly hat den angebundenen ActivationProfileStore fuer eine frische Profilladung verwendet.",
                loadResult is not null,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "store.profile.found",
                "Profil im Store gefunden",
                "Das Zielprofil muss im Profilkatalog bzw. Store gefunden werden.",
                loadResult?.Found == true),
            Precondition(
                "store.profile.userDefined",
                "Store-Profil ist UserDefined",
                "Nur UserDefined-Schnittstellenprofile duerfen spaeter produktiv gespeichert werden.",
                loadResult?.IsUserDefined == true),
            Precondition(
                "store.profile.notBuiltIn",
                "Store-Profil ist nicht BuiltIn",
                "BuiltIn-Schnittstellenprofile bleiben auch ueber den Store strikt gesperrt.",
                loadResult?.Found == true &&
                loadResult.IsBuiltIn == false),
            Precondition(
                "store.saveDryRun.checked",
                "Save-DryRun geprueft",
                "Der Store wurde nur fuer eine nicht-produktive Save-Vorpruefung verwendet.",
                saveDryRunResult is not null,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "store.saveDryRun.notBlocked",
                "Save-DryRun nicht blockiert",
                "Save-DryRun wird nur nach erfolgreicher finaler ValidateOnly-Pruefung freigegeben.",
                !saveDryRunBlocked,
                saveDryRunBlocked
                    ? InterfaceProfileActivationSeverity.Blocker
                    : InterfaceProfileActivationSeverity.Info),
            Precondition(
                "store.saveDryRun.finalEvaluation.completed",
                "Finale Bewertung fuer Save-DryRun nachgewiesen",
                "Der Store-DryRun darf nur nach frischer finaler ValidateOnly-Pruefung freigegeben werden.",
                finalEvaluationCompleted),
            Precondition(
                "store.saveDryRun.notPersisted",
                "Save-DryRun hat nicht gespeichert",
                "Der Store-DryRun darf keine JSON-Datei schreiben und kein Profil veraendern.",
                saveDryRunResult is not null &&
                !saveDryRunResult.WasSaved &&
                !saveDryRunResult.ProfileChanged,
                InterfaceProfileActivationSeverity.Info)
        };
    }

    private static IReadOnlyList<string> CreateMissingCapabilities(
        IEnumerable<InterfaceProfileActivationExecutorPrecondition> preconditions)
    {
        return preconditions
            .Where(precondition => precondition.IsRequired &&
                !precondition.IsSatisfied &&
                precondition.Code.StartsWith("executor.", StringComparison.Ordinal))
            .Select(precondition => precondition.Code)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string CreateMessage(
        InterfaceProfileActivationExecutorStatus status,
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult,
        FinalValidationContext finalValidation,
        bool shouldUseStore,
        bool saveDryRunBlocked)
    {
        if (shouldUseStore && loadResult is not null)
        {
            if (loadResult.Status == InterfaceProfileActivationProfileStoreStatus.NotFound)
            {
                return "ActivationExecutor ValidateOnly nicht ausgefuehrt: Das Zielprofil wurde im Store nicht gefunden. Es wurde nichts aktiviert oder gespeichert.";
            }

            if (loadResult.Status is InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked or
                InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked or
                InterfaceProfileActivationProfileStoreStatus.UserDefinedRequired)
            {
                return $"ActivationExecutor ValidateOnly blockiert: {loadResult.Message} Es wurde nichts aktiviert oder gespeichert.";
            }

            if (loadResult.Status is InterfaceProfileActivationProfileStoreStatus.NotAvailable or
                InterfaceProfileActivationProfileStoreStatus.Failed)
            {
                return $"ActivationExecutor ValidateOnly nicht verfuegbar: {loadResult.Message} Es wurde nichts aktiviert oder gespeichert.";
            }

            if (status == InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted)
            {
                var finalDetail = finalValidation.IsComplete
                    ? " Die finale V1-Pruefkette wurde simuliert."
                    : " Die finale V1-Pruefkette ist wegen fehlender Services noch nicht vollstaendig moeglich.";
                var saveDetail = saveDryRunResult is not null
                    ? " Der Save-DryRun wurde geprueft und hat nicht gespeichert."
                    : saveDryRunBlocked
                        ? " Der Save-DryRun bleibt blockiert."
                        : string.Empty;

                return "ActivationExecutor ValidateOnly erfolgreich: Das Zielprofil wurde frisch geladen und als UserDefined erkannt; produktive Aktivierung und Speicherung bleiben nicht implementiert." +
                    finalDetail +
                    saveDetail +
                    " Es wurde nichts aktiviert oder gespeichert.";
            }
        }

        return status switch
        {
            InterfaceProfileActivationExecutorStatus.NotAvailable =>
                "ActivationExecutor nicht ausgefuehrt: Zielprofil-ID, Profil oder erforderliche aktuelle Pruefgrundlage fehlt.",
            InterfaceProfileActivationExecutorStatus.Blocked =>
                "ActivationExecutor nicht ausgefuehrt: V1 erlaubt nur Ready ohne Warnungen fuer UserDefined-Profile.",
            InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted =>
                "ActivationExecutor ist defensiv nicht-produktiv: Die V1-Voraussetzungen wirken erfuellbar, aber produktive Speicherung ist nicht angebunden. Es wurde nichts aktiviert oder gespeichert.",
            _ =>
                "ActivationExecutor ist noch nicht produktiv implementiert. Es wurde nichts aktiviert oder gespeichert."
        };
    }

    private static InterfaceProfileActivationExecutorPrecondition Precondition(
        string code,
        string title,
        string description,
        bool isSatisfied,
        InterfaceProfileActivationSeverity severity = InterfaceProfileActivationSeverity.Blocker)
    {
        return new InterfaceProfileActivationExecutorPrecondition(
            code,
            title,
            description,
            IsRequired: true,
            IsSatisfied: isSatisfied,
            Severity: severity);
    }

    private sealed record FinalValidationContext(
        InterfaceProfileActivationEvaluationResult? EvaluationResult,
        InterfaceProfileActivationGuardResult? GuardResult,
        bool EvaluationPerformed,
        bool GuardRechecked,
        IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions,
        bool IsComplete)
    {
        public static FinalValidationContext NotAttempted()
        {
            return new FinalValidationContext(
                EvaluationResult: null,
                GuardResult: null,
                EvaluationPerformed: false,
                GuardRechecked: false,
                Preconditions: Array.Empty<InterfaceProfileActivationExecutorPrecondition>(),
                IsComplete: false);
        }
    }
}
