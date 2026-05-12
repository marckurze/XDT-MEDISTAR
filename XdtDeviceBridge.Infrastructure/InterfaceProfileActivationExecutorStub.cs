using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationExecutorStub : IInterfaceProfileActivationExecutor
{
    private readonly IInterfaceProfileActivationProfileStore? _profileStore;
    private readonly InterfaceProfileActivationEvaluationService? _evaluationService;
    private readonly InterfaceProfileActivationGuardService? _guardService;
    private readonly InterfaceProfileActivationWarningConfirmationService? _warningConfirmationService;
    private readonly InterfaceProfileActivationPlanService? _activationPlanService;
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
            warningConfirmationService: null,
            activationPlanService: null,
            finalEvaluationCatalog: null)
    {
    }

    public InterfaceProfileActivationExecutorStub(
        IInterfaceProfileActivationProfileStore? profileStore,
        InterfaceProfileActivationEvaluationService? evaluationService,
        InterfaceProfileActivationGuardService? guardService,
        InterfaceProfileActivationWarningConfirmationService? warningConfirmationService,
        InterfaceProfileActivationPlanService? activationPlanService,
        ProfileCatalog? finalEvaluationCatalog)
    {
        _profileStore = profileStore;
        _evaluationService = evaluationService;
        _guardService = guardService;
        _warningConfirmationService = warningConfirmationService;
        _activationPlanService = activationPlanService;
        _finalEvaluationCatalog = finalEvaluationCatalog;
    }

    public Task<InterfaceProfileActivationExecutorResult> ExecuteAsync(
        InterfaceProfileActivationExecutorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var shouldUseStore = request.OperationMode == InterfaceProfileActivationExecutorOperationMode.ValidateOnly &&
            _profileStore is not null;
        var loadResult = shouldUseStore
            ? _profileStore!.LoadFreshUserDefinedProfile(request.EffectiveInterfaceProfileId)
            : null;
        var effectiveProfile = loadResult?.Profile ?? request.Profile;
        var finalValidation = CreateFinalValidationContext(
            request,
            effectiveProfile,
            loadResult,
            shouldUseStore);

        var evaluation = finalValidation.EvaluationResult ?? request.EvaluationResult;
        var guard = finalValidation.GuardResult ?? request.GuardResult;
        var warningConfirmation = finalValidation.WarningConfirmationResult ?? request.WarningConfirmationResult;
        var plan = finalValidation.ActivationPlan ?? request.ActivationPlan;
        var saveDryRunBlocked = shouldUseStore &&
            effectiveProfile is not null &&
            loadResult?.Success == true &&
            !CanRunSaveDryRun(request, effectiveProfile, finalValidation);
        var saveDryRunResult = shouldUseStore &&
            CanRunSaveDryRun(request, effectiveProfile, finalValidation)
            ? _profileStore!.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
                Profile: effectiveProfile,
                Source: request.Source,
                RequestedAtUtc: request.RequestedAtUtc,
                ExpectedConfigurationFingerprint: request.ExpectedConfigurationFingerprint,
                OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
                FinalReEvaluationCompleted: true))
            : null;

        var preconditions = CreatePreconditions(
            request,
            effectiveProfile,
            evaluation,
            guard,
            warningConfirmation,
            plan,
            loadResult,
            saveDryRunResult,
            finalValidation,
            _profileStore is not null,
            shouldUseStore,
            saveDryRunBlocked);
        var status = DetermineStatus(
            request,
            effectiveProfile,
            evaluation,
            guard,
            plan,
            loadResult,
            shouldUseStore);

        return Task.FromResult(new InterfaceProfileActivationExecutorResult(
            Status: status,
            Success: false,
            Message: CreateMessage(status, loadResult, saveDryRunResult, finalValidation, shouldUseStore, saveDryRunBlocked),
            Preconditions: preconditions,
            ExecutedSteps: Array.Empty<InterfaceProfileActivationPlanStep>(),
            NotExecutedSteps: plan?.PlannedSteps ?? Array.Empty<InterfaceProfileActivationPlanStep>(),
            ProfileChanged: false,
            Saved: false,
            ProcessingStarted: false,
            WasExecuted: false,
            WasPersisted: false,
            WasProfileChanged: false,
            RequiresFreshLoad: loadResult?.Success != true,
            RequiresSafeUserDefinedStore: true,
            RequiresFinalReEvaluation: !finalValidation.IsComplete,
            IsValidationOnly: request.OperationMode == InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            MissingCapabilities: CreateMissingCapabilities(preconditions),
            FreshLoadPerformed: loadResult is not null,
            FinalReEvaluationPerformed: finalValidation.IsComplete,
            GuardRechecked: finalValidation.GuardRechecked,
            WarningConfirmationRechecked: finalValidation.WarningConfirmationRechecked,
            ActivationPlanRecreated: finalValidation.ActivationPlanRecreated,
            SaveDryRunPerformed: saveDryRunResult is not null,
            SaveDryRunBlocked: saveDryRunBlocked));
    }

    private FinalValidationContext CreateFinalValidationContext(
        InterfaceProfileActivationExecutorRequest request,
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
            _warningConfirmationService is not null &&
            _activationPlanService is not null &&
            _finalEvaluationCatalog is not null;

        if (!canRunFinalValidation)
        {
            return new FinalValidationContext(
                EvaluationResult: null,
                GuardResult: null,
                WarningConfirmationResult: null,
                ActivationPlan: null,
                EvaluationPerformed: false,
                GuardRechecked: false,
                WarningConfirmationRechecked: false,
                ActivationPlanRecreated: false,
                Preconditions: preconditions,
                IsComplete: false);
        }

        var evaluation = _evaluationService!.Evaluate(effectiveProfile, _finalEvaluationCatalog!);
        var guard = _guardService!.ValidateActivationRequest(new InterfaceProfileActivationRequest(
            effectiveProfile,
            evaluation,
            request.WarningsAccepted,
            Context: "ExecutorValidateOnlyFinalReEvaluation"));
        var warningConfirmation = _warningConfirmationService!.PrepareWarningConfirmation(
            new InterfaceProfileActivationWarningConfirmationRequest(
                effectiveProfile,
                evaluation,
                guard,
                EvaluatedAt: request.RequestedAtUtc ?? request.RequestedAt ?? request.PreviewCreatedAtUtc,
                Source: "ExecutorValidateOnlyFinalReEvaluation"));
        var plan = _activationPlanService!.CreatePlan(new InterfaceProfileActivationPlanRequest(
            effectiveProfile,
            evaluation,
            guard,
            warningConfirmation,
            request.WarningsAccepted,
            Context: "ExecutorValidateOnlyFinalReEvaluation"));

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
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "executor.finalWarningConfirmation.performed",
                "Warnungsbestaetigung erneut bewertet",
                "ValidateOnly hat die Warnungsbestaetigungsvorschau erneut aus der frischen Bewertung abgeleitet.",
                isSatisfied: true,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "executor.finalActivationPlan.performed",
                "Aktivierungsplan erneut erzeugt",
                "ValidateOnly hat den Aktivierungsplan erneut aus frischer Bewertung, Guard und Warnungsvorschau erzeugt.",
                isSatisfied: true,
                InterfaceProfileActivationSeverity.Info)
        });

        return new FinalValidationContext(
            evaluation,
            guard,
            warningConfirmation,
            plan,
            EvaluationPerformed: true,
            GuardRechecked: true,
            WarningConfirmationRechecked: true,
            ActivationPlanRecreated: true,
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
                "ValidateOnly benoetigt den Aktivierungsbewertungsservice fuer eine frische finale Re-Evaluation.",
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
                _guardService is not null),
            Precondition(
                "executor.finalWarningConfirmationService.missing",
                "Finaler Warnungsbestaetigungsservice verfuegbar",
                "ValidateOnly benoetigt den Warnungsbestaetigungsservice fuer eine erneute Warnungsvorschau.",
                _warningConfirmationService is not null),
            Precondition(
                "executor.finalActivationPlanService.missing",
                "Finaler ActivationPlan-Service verfuegbar",
                "ValidateOnly benoetigt den ActivationPlan-Service fuer einen neu erzeugten Aktivierungsplan.",
                _activationPlanService is not null)
        };
    }

    private static bool CanRunSaveDryRun(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
        FinalValidationContext finalValidation)
    {
        if (effectiveProfile is null ||
            effectiveProfile.Metadata.IsBuiltIn ||
            !effectiveProfile.Metadata.IsUserDefined ||
            !finalValidation.IsComplete ||
            finalValidation.EvaluationResult is null ||
            finalValidation.GuardResult is null ||
            finalValidation.ActivationPlan is null)
        {
            return false;
        }

        if (finalValidation.EvaluationResult.Blockers.Count > 0 ||
            finalValidation.ActivationPlan.Blockers.Count > 0)
        {
            return false;
        }

        if (finalValidation.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            finalValidation.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Allowed &&
            finalValidation.ActivationPlan.PlanStatus == InterfaceProfileActivationPlanStatus.Ready)
        {
            return true;
        }

        return finalValidation.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            request.WarningsAccepted &&
            finalValidation.GuardResult.Decision == InterfaceProfileActivationGuardDecision.AllowedWithWarnings &&
            finalValidation.ActivationPlan.PlanStatus == InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings;
    }

    private static InterfaceProfileActivationExecutorStatus DetermineStatus(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
        InterfaceProfileActivationEvaluationResult? evaluation,
        InterfaceProfileActivationGuardResult? guard,
        InterfaceProfileActivationPlan? plan,
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
            guard is null ||
            plan is null)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (effectiveProfile.Metadata.IsBuiltIn ||
            !effectiveProfile.Metadata.IsUserDefined ||
            evaluation.ActivationStatus == InterfaceProfileActivationStatus.Blocked ||
            evaluation.Blockers.Count > 0 ||
            guard.Decision == InterfaceProfileActivationGuardDecision.Blocked ||
            plan.PlanStatus == InterfaceProfileActivationPlanStatus.Blocked)
        {
            return InterfaceProfileActivationExecutorStatus.Blocked;
        }

        if (evaluation.ActivationStatus is InterfaceProfileActivationStatus.Unknown or
                InterfaceProfileActivationStatus.NotEvaluated ||
            guard.Decision == InterfaceProfileActivationGuardDecision.Unknown ||
            plan.PlanStatus is InterfaceProfileActivationPlanStatus.Unknown or
                InterfaceProfileActivationPlanStatus.NotAvailable)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (evaluation.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            (!request.WarningsAccepted ||
             guard.Decision == InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation ||
             plan.PlanStatus == InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation))
        {
            return InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation;
        }

        if (request.OperationMode == InterfaceProfileActivationExecutorOperationMode.Deactivate)
        {
            return InterfaceProfileActivationExecutorStatus.NotImplemented;
        }

        if (plan.PlanStatus is InterfaceProfileActivationPlanStatus.Ready or
            InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
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
        InterfaceProfileActivationWarningConfirmationResult? warningConfirmation,
        InterfaceProfileActivationPlan? plan,
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
                isSatisfied: false,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.freshReload.missing",
                hasProfileStore ? "Frisches Laden ueber Store moeglich" : "Frisches Laden nicht angebunden",
                hasProfileStore
                    ? "Im ValidateOnly-Modus kann der Stub ein Zielprofil ueber den angebundenen Store frisch laden; fuer produktive Ausfuehrung bleibt eine finale Re-Evaluation Pflicht."
                    : "Der Request kann Zielprofil- und Preview-Kontext tragen; der Stub hat aber keinen Loader/Profilkatalog fuer eine finale frische Ladung.",
                freshLoadSucceeded,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.storeContext.missing",
                hasProfileStore ? "Store-Kontext angebunden" : "Store-Kontext nicht angebunden",
                hasProfileStore
                    ? "Ein IInterfaceProfileActivationProfileStore ist am Stub angebunden; produktives Speichern bleibt trotzdem deaktiviert."
                    : "Der Request enthaelt bewusst keine konkrete Store-Service-Referenz; ein produktiver Executor muss Loader/Store sicher per Konstruktor oder bestehendem Muster erhalten.",
                hasProfileStore,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.safePersistence.missing",
                "Sichere UserDefined-Speicherung nicht produktiv angebunden",
                "Eine produktive Speicherung des UserDefined-Schnittstellenprofils ist in dieser Executor-Stufe nicht angebunden.",
                isSatisfied: false,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.finalReEvaluation.required",
                "Finale Re-Evaluation vorbereitet",
                "Preview-Daten im Request sind nur Kontext; ValidateOnly kann die finale Pruefstrecke nur bei angebundenen Services simulieren.",
                finalValidation.IsComplete,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "operation.mode.modeled",
                "Ausfuehrungsmodus modelliert",
                "Der Request unterscheidet ValidateOnly, Activate und Deactivate, ohne daraus im Stub produktive Aktionen abzuleiten.",
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
                "evaluation.activatable",
                "Bewertung aktivierbar",
                "Die Bewertung muss Ready oder ReadyWithWarnings sein.",
                evaluation?.ActivationStatus is InterfaceProfileActivationStatus.Ready or
                    InterfaceProfileActivationStatus.ReadyWithWarnings),
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
                "Der Guard muss Allowed oder AllowedWithWarnings liefern.",
                guard?.Decision is InterfaceProfileActivationGuardDecision.Allowed or
                    InterfaceProfileActivationGuardDecision.AllowedWithWarnings),
            Precondition(
                "warningConfirmation.present",
                finalValidation.WarningConfirmationRechecked
                    ? "Finale Warnungsbestaetigungsvorschau vorhanden"
                    : "Warnungsbestaetigungskontext vorhanden",
                "ValidateOnly soll die Warnungsbestaetigungsvorschau aus der frischen Bewertung ableiten koennen.",
                warningConfirmation is not null,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "plan.ready",
                finalValidation.ActivationPlanRecreated ? "Finaler Aktivierungsplan bereit" : "Aktivierungsplan bereit",
                "Der Aktivierungsplan muss Ready oder ReadyWithAcceptedWarnings sein.",
                plan?.PlanStatus is InterfaceProfileActivationPlanStatus.Ready or
                    InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
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

        if (evaluation?.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings ||
            guard?.Decision is InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation or
                InterfaceProfileActivationGuardDecision.AllowedWithWarnings ||
            plan?.PlanStatus is InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation or
                InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
        {
            preconditions.Add(Precondition(
                "warnings.accepted",
                "Warnungen bewusst bestaetigt",
                "ReadyWithWarnings darf nur nach bewusster Warnungsbestaetigung weitergehen.",
                request.WarningsAccepted &&
                guard?.Decision == InterfaceProfileActivationGuardDecision.AllowedWithWarnings &&
                plan?.PlanStatus == InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings &&
                warningConfirmation?.Status ==
                    InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
                InterfaceProfileActivationSeverity.Warning));
        }

        return preconditions;
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateStorePreconditions(
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult,
        bool finalReEvaluationCompleted,
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
                "store.saveDryRun.finalReEvaluation.completed",
                "Finale Re-Evaluation fuer Save-DryRun nachgewiesen",
                "Der Store-DryRun darf nur nach frischer finaler ValidateOnly-Pruefung freigegeben werden.",
                finalReEvaluationCompleted),
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
                    ? " Die finale ValidateOnly-Pruefkette wurde simuliert."
                    : " Die finale ValidateOnly-Pruefkette ist wegen fehlender Services noch nicht vollstaendig moeglich.";
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
                "ActivationExecutor nicht ausgefuehrt: Aktivierung ist blockiert.",
            InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation =>
                "ActivationExecutor nicht ausgefuehrt: Warnungen muessen vor einer produktiven Aktivierung bewusst bestaetigt werden.",
            InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted =>
                "ActivationExecutor ist defensiv nicht-produktiv: Die fachlichen Voraussetzungen wirken erfuellbar, aber produktive Speicherung ist nicht angebunden. Es wurde nichts aktiviert oder gespeichert.",
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
        InterfaceProfileActivationWarningConfirmationResult? WarningConfirmationResult,
        InterfaceProfileActivationPlan? ActivationPlan,
        bool EvaluationPerformed,
        bool GuardRechecked,
        bool WarningConfirmationRechecked,
        bool ActivationPlanRecreated,
        IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> Preconditions,
        bool IsComplete)
    {
        public static FinalValidationContext NotAttempted()
        {
            return new FinalValidationContext(
                EvaluationResult: null,
                GuardResult: null,
                WarningConfirmationResult: null,
                ActivationPlan: null,
                EvaluationPerformed: false,
                GuardRechecked: false,
                WarningConfirmationRechecked: false,
                ActivationPlanRecreated: false,
                Preconditions: Array.Empty<InterfaceProfileActivationExecutorPrecondition>(),
                IsComplete: false);
        }
    }
}
