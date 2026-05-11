namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationExecutorStub : IInterfaceProfileActivationExecutor
{
    public Task<InterfaceProfileActivationExecutorResult> ExecuteAsync(
        InterfaceProfileActivationExecutorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var preconditions = CreatePreconditions(request);
        var status = DetermineStatus(request);

        return Task.FromResult(new InterfaceProfileActivationExecutorResult(
            Status: status,
            Success: false,
            Message: CreateMessage(status),
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
        InterfaceProfileActivationExecutorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EffectiveInterfaceProfileId) ||
            request.Profile is null ||
            request.EvaluationResult is null ||
            request.GuardResult is null ||
            request.ActivationPlan is null)
        {
            return InterfaceProfileActivationExecutorStatus.NotAvailable;
        }

        if (request.Profile.Metadata.IsBuiltIn ||
            !request.Profile.Metadata.IsUserDefined ||
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
        InterfaceProfileActivationExecutorRequest request)
    {
        var profile = request.Profile;
        var evaluation = request.EvaluationResult;
        var guard = request.GuardResult;
        var plan = request.ActivationPlan;
        var effectiveProfileId = request.EffectiveInterfaceProfileId;

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
                "Frisches Laden nicht angebunden",
                "Der Request kann Zielprofil- und Preview-Kontext tragen; der Stub hat aber keinen Loader/Profilkatalog für eine finale frische Ladung.",
                isSatisfied: false,
                InterfaceProfileActivationSeverity.Blocker),
            Precondition(
                "executor.storeContext.missing",
                "Store-Kontext nicht angebunden",
                "Der Request enthält bewusst keine konkrete Store-Service-Referenz; ein produktiver Executor muss Loader/Store sicher per Konstruktor oder bestehendem Muster erhalten.",
                isSatisfied: false,
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

    private static string CreateMessage(InterfaceProfileActivationExecutorStatus status)
    {
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
