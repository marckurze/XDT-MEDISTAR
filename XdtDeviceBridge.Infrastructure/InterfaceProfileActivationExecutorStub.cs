using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationExecutorStub : IInterfaceProfileActivationExecutor
{
    private readonly IInterfaceProfileActivationProfileStore? _profileStore;

    public InterfaceProfileActivationExecutorStub()
        : this(null)
    {
    }

    public InterfaceProfileActivationExecutorStub(IInterfaceProfileActivationProfileStore? profileStore)
    {
        _profileStore = profileStore;
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
        var saveDryRunResult = shouldUseStore && effectiveProfile is not null
            ? _profileStore!.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
                Profile: effectiveProfile,
                Source: request.Source,
                RequestedAtUtc: request.RequestedAtUtc,
                ExpectedConfigurationFingerprint: request.ExpectedConfigurationFingerprint,
                OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
                FinalReEvaluationCompleted: false))
            : null;

        var preconditions = CreatePreconditions(
            request,
            effectiveProfile,
            loadResult,
            saveDryRunResult,
            _profileStore is not null,
            shouldUseStore);
        var status = DetermineStatus(request, effectiveProfile, loadResult, shouldUseStore);

        return Task.FromResult(new InterfaceProfileActivationExecutorResult(
            Status: status,
            Success: false,
            Message: CreateMessage(status, loadResult, saveDryRunResult, shouldUseStore),
            Preconditions: preconditions,
            ExecutedSteps: Array.Empty<InterfaceProfileActivationPlanStep>(),
            NotExecutedSteps: request.ActivationPlan?.PlannedSteps ?? Array.Empty<InterfaceProfileActivationPlanStep>(),
            ProfileChanged: false,
            Saved: false,
            ProcessingStarted: false,
            WasExecuted: false,
            WasPersisted: false,
            WasProfileChanged: false,
            RequiresFreshLoad: true,
            RequiresSafeUserDefinedStore: true,
            RequiresFinalReEvaluation: true,
            IsValidationOnly: request.OperationMode == InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            MissingCapabilities: CreateMissingCapabilities(preconditions)));
    }

    private static InterfaceProfileActivationExecutorStatus DetermineStatus(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
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
            request.EvaluationResult is null ||
            request.GuardResult is null ||
            request.ActivationPlan is null)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (effectiveProfile.Metadata.IsBuiltIn ||
            !effectiveProfile.Metadata.IsUserDefined ||
            request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Blocked ||
            request.EvaluationResult.Blockers.Count > 0 ||
            request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Blocked ||
            request.ActivationPlan.PlanStatus == InterfaceProfileActivationPlanStatus.Blocked)
        {
            return InterfaceProfileActivationExecutorStatus.Blocked;
        }

        if (request.EvaluationResult.ActivationStatus is InterfaceProfileActivationStatus.Unknown or
                InterfaceProfileActivationStatus.NotEvaluated ||
            request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Unknown ||
            request.ActivationPlan.PlanStatus is InterfaceProfileActivationPlanStatus.Unknown or
                InterfaceProfileActivationPlanStatus.NotAvailable)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            (!request.WarningsAccepted ||
             request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation ||
             request.ActivationPlan.PlanStatus == InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation))
        {
            return InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation;
        }

        if (request.OperationMode == InterfaceProfileActivationExecutorOperationMode.Deactivate)
        {
            return InterfaceProfileActivationExecutorStatus.NotImplemented;
        }

        if (request.ActivationPlan.PlanStatus is InterfaceProfileActivationPlanStatus.Ready or
            InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
        {
            return InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted;
        }

        return InterfaceProfileActivationExecutorStatus.NotImplemented;
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreatePreconditions(
        InterfaceProfileActivationExecutorRequest request,
        InterfaceProfileDefinition? effectiveProfile,
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult,
        bool hasProfileStore,
        bool shouldUseStore)
    {
        var profile = effectiveProfile;
        var evaluation = request.EvaluationResult;
        var guard = request.GuardResult;
        var plan = request.ActivationPlan;
        var effectiveProfileId = request.EffectiveInterfaceProfileId;
        var freshLoadSucceeded = loadResult?.Success == true;

        var preconditions = new List<InterfaceProfileActivationExecutorPrecondition>
        {
            Precondition(
                "executor.nonProductiveStub",
                "Defensive Executor-Stufe",
                "Diese Implementierung ist absichtlich nicht produktiv und führt keine Aktivierung aus.",
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
                "Sichere UserDefined-Speicherung nicht angebunden",
                "Eine produktive Speicherung des UserDefined-Schnittstellenprofils ist in dieser Executor-Stufe nicht angebunden.",
                isSatisfied: false,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.finalReEvaluation.required",
                "Finale Re-Evaluation erforderlich",
                "Preview-Daten im Request sind nur Kontext; direkt vor einer produktiven Ausführung muss frisch geladen und neu bewertet werden.",
                isSatisfied: false,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "operation.mode.modeled",
                "Ausführungsmodus modelliert",
                "Der Request unterscheidet ValidateOnly, Activate und Deactivate, ohne daraus im Stub produktive Aktionen abzuleiten.",
                Enum.IsDefined(typeof(InterfaceProfileActivationExecutorOperationMode), request.OperationMode),
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "profile.id.present",
                "Zielprofil-ID vorhanden",
                "Ein späterer Executor muss das Zielprofil eindeutig frisch laden können.",
                !string.IsNullOrWhiteSpace(effectiveProfileId)),
            Precondition(
                "profile.present",
                "Schnittstellenprofil vorhanden",
                "Ein Schnittstellenprofil muss ausgewählt sein.",
                profile is not null),
            Precondition(
                "profile.userDefined",
                "Profil ist UserDefined",
                "Aktivierung ist nur für UserDefined-Schnittstellenprofile vorgesehen.",
                profile?.Metadata.IsUserDefined == true),
            Precondition(
                "profile.notBuiltIn",
                "Profil ist nicht BuiltIn",
                "BuiltIn-Profile dürfen nicht direkt aktiviert oder verändert werden.",
                profile?.Metadata.IsBuiltIn == false),
            Precondition(
                "activationFlag.isActive",
                "Aktivierungsflag identifiziert",
                "`IsActive` ist im bestehenden Schnittstellenprofilmodell als Aktivierungskennzeichen vorhanden.",
                profile is not null,
                InterfaceProfileActivationSeverity.Info),
            Precondition(
                "evaluation.present",
                "Aktivierungsbewertung vorhanden",
                "Eine aktuelle Aktivierungsbewertung muss vorliegen.",
                evaluation is not null),
            Precondition(
                "evaluation.activatable",
                "Bewertung aktivierbar",
                "Die Bewertung muss Ready oder ReadyWithWarnings sein.",
                evaluation?.ActivationStatus is InterfaceProfileActivationStatus.Ready or
                    InterfaceProfileActivationStatus.ReadyWithWarnings),
            Precondition(
                "guard.present",
                "Guard-Entscheidung vorhanden",
                "Eine technische Schutzprüfung muss vorliegen.",
                guard is not null),
            Precondition(
                "guard.allowed",
                "Guard erlaubt Aktivierung",
                "Der Guard muss Allowed oder AllowedWithWarnings liefern.",
                guard?.Decision is InterfaceProfileActivationGuardDecision.Allowed or
                    InterfaceProfileActivationGuardDecision.AllowedWithWarnings),
            Precondition(
                "plan.ready",
                "Aktivierungsplan bereit",
                "Der Aktivierungsplan muss Ready oder ReadyWithAcceptedWarnings sein.",
                plan?.PlanStatus is InterfaceProfileActivationPlanStatus.Ready or
                    InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
        };

        if (shouldUseStore)
        {
            preconditions.AddRange(CreateStorePreconditions(loadResult, saveDryRunResult));
        }

        if (evaluation?.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings ||
            guard?.Decision is InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation or
                InterfaceProfileActivationGuardDecision.AllowedWithWarnings ||
            plan?.PlanStatus is InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation or
                InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings)
        {
            preconditions.Add(Precondition(
                "warnings.accepted",
                "Warnungen bewusst bestätigt",
                "ReadyWithWarnings darf nur nach bewusster Warnungsbestätigung weitergehen.",
                request.WarningsAccepted &&
                guard?.Decision == InterfaceProfileActivationGuardDecision.AllowedWithWarnings &&
                plan?.PlanStatus == InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings &&
                request.WarningConfirmationResult?.Status ==
                    InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
                InterfaceProfileActivationSeverity.Warning));
        }

        return preconditions;
    }

    private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> CreateStorePreconditions(
        InterfaceProfileActivationProfileLoadResult? loadResult,
        InterfaceProfileActivationProfileSaveResult? saveDryRunResult)
    {
        var finalReEvaluationCompleted = saveDryRunResult?.Preconditions.Any(precondition =>
            precondition.Code == "executor.finalReEvaluation.completed" &&
            precondition.IsSatisfied) == true;

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
                "store.saveDryRun.finalReEvaluation.completed",
                "Finale Re-Evaluation fuer Save-DryRun nachgewiesen",
                "Der Stub weist weiterhin aus, dass eine echte finale Re-Evaluation vor produktivem Speichern fehlt.",
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
        bool shouldUseStore)
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
                var saveDetail = saveDryRunResult?.Status == InterfaceProfileActivationProfileStoreStatus.MissingCapability
                    ? " Der Save-DryRun bleibt wegen fehlender finaler Re-Evaluation blockiert."
                    : string.Empty;

                return "ActivationExecutor ValidateOnly erfolgreich: Das Zielprofil wurde frisch geladen und als UserDefined erkannt; produktive Aktivierung und Speicherung bleiben nicht implementiert." +
                    saveDetail +
                    " Es wurde nichts aktiviert oder gespeichert.";
            }
        }

        return status switch
        {
            InterfaceProfileActivationExecutorStatus.NotAvailable =>
                "ActivationExecutor nicht ausgeführt: Zielprofil-ID, Profil oder erforderliche aktuelle Prüfgrundlage fehlt.",
            InterfaceProfileActivationExecutorStatus.Blocked =>
                "ActivationExecutor nicht ausgeführt: Aktivierung ist blockiert.",
            InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation =>
                "ActivationExecutor nicht ausgeführt: Warnungen müssen vor einer produktiven Aktivierung bewusst bestätigt werden.",
            InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted =>
                "ActivationExecutor ist defensiv nicht-produktiv: Die fachlichen Voraussetzungen wirken erfüllbar, aber frisches Laden und sichere UserDefined-Speicherung sind noch nicht angebunden. Es wurde nichts aktiviert oder gespeichert.",
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
}
