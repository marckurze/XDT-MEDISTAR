using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPlanService
{
    public InterfaceProfileActivationPlan CreatePlan(InterfaceProfileActivationPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var blockers = new List<InterfaceProfileActivationPlanReason>();
        var warnings = new List<InterfaceProfileActivationPlanReason>();
        var infos = new List<InterfaceProfileActivationPlanReason>();

        if (request.Profile is null)
        {
            blockers.Add(Reason(
                InterfaceProfileActivationSeverity.Blocker,
                "activationPlan.profile.missing",
                "Kein Schnittstellenprofil ausgewählt."));

            return CreatePlan(
                InterfaceProfileActivationPlanStatus.NotAvailable,
                canExecuteLater: false,
                profileId: "-",
                profileName: "-",
                summaryMessage: "Kein Schnittstellenprofil ausgewählt.",
                blockers,
                warnings,
                infos,
                CreateNotAvailableSteps("Schnittstellenprofil auswählen"),
                blockers);
        }

        var profileId = string.IsNullOrWhiteSpace(request.Profile.Metadata.Id)
            ? "-"
            : request.Profile.Metadata.Id;
        var profileName = string.IsNullOrWhiteSpace(request.Profile.Metadata.Name)
            ? profileId
            : request.Profile.Metadata.Name;

        if (request.EvaluationResult is null)
        {
            blockers.Add(Reason(
                InterfaceProfileActivationSeverity.Blocker,
                "activationPlan.evaluation.missing",
                "Keine aktuelle Aktivierungsbewertung vorhanden."));

            return CreatePlan(
                InterfaceProfileActivationPlanStatus.NotAvailable,
                canExecuteLater: false,
                profileId,
                profileName,
                "Keine aktuelle Aktivierungsbewertung vorhanden.",
                blockers,
                warnings,
                infos,
                CreateNotAvailableSteps("Aktivierungsprüfung aktualisieren"),
                blockers);
        }

        CopyEvaluationReasons(request.EvaluationResult, blockers, warnings, infos);
        CopyGuardReasons(request.GuardResult, blockers, warnings, infos);
        CopyWarningConfirmationReasons(request.WarningConfirmationResult, warnings);
        EvaluateProfileProtection(request.Profile, blockers);

        var safetyBlockers = blockers
            .Where(reason =>
                reason.Severity == InterfaceProfileActivationSeverity.Blocker &&
                reason.Code.StartsWith("activationPlan.profile.", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (safetyBlockers.Count > 0)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Blocked,
                canExecuteLater: false,
                profileId,
                profileName,
                CreateProfileProtectionMessage(safetyBlockers),
                blockers,
                warnings,
                infos,
                CreateBlockedSteps("Profilschutz verhindert eine spätere Aktivierung."),
                blockers);
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Blocked ||
            request.EvaluationResult.Blockers.Count > 0)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Blocked,
                canExecuteLater: false,
                profileId,
                profileName,
                "Aktivierungsplan blockiert. Bitte beheben Sie zuerst die blockierenden Punkte.",
                blockers,
                warnings,
                infos,
                CreateBlockedSteps("Blockierende Prüfpunkte müssen zuerst behoben werden."),
                blockers);
        }

        if (request.GuardResult is null)
        {
            blockers.Add(Reason(
                InterfaceProfileActivationSeverity.Blocker,
                "activationPlan.guard.missing",
                "Keine technische Guard-Entscheidung vorhanden."));

            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Unknown,
                canExecuteLater: false,
                profileId,
                profileName,
                "Keine technische Guard-Entscheidung vorhanden.",
                blockers,
                warnings,
                infos,
                CreateNotAvailableSteps("Technische Schutzprüfung erneut ausführen"),
                blockers);
        }

        if (request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Blocked)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Blocked,
                canExecuteLater: false,
                profileId,
                profileName,
                "Technische Schutzprüfung verhindert eine spätere Aktivierung.",
                blockers,
                warnings,
                infos,
                CreateBlockedSteps("Technische Schutzprüfung muss zuerst freigegeben sein."),
                blockers);
        }

        if (request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Unknown ||
            request.EvaluationResult.ActivationStatus is InterfaceProfileActivationStatus.Unknown or
                InterfaceProfileActivationStatus.NotEvaluated)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Unknown,
                canExecuteLater: false,
                profileId,
                profileName,
                "Aktivierungsplan ist nicht eindeutig.",
                blockers,
                warnings,
                infos,
                CreateNotAvailableSteps("Aktivierungsprüfung und Schutzprüfung aktualisieren"),
                blockers);
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.Allowed)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.Ready,
                canExecuteLater: true,
                profileId,
                profileName,
                "Eine spätere Aktivierung wäre technisch vorbereitet.",
                blockers,
                warnings,
                infos,
                CreateReadySteps(),
                Array.Empty<InterfaceProfileActivationPlanReason>());
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            request.WarningsAccepted &&
            request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.AllowedWithWarnings)
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings,
                canExecuteLater: true,
                profileId,
                profileName,
                "Eine spätere Aktivierung wäre nach bestätigten Warnungen technisch vorbereitet.",
                blockers,
                warnings,
                infos,
                CreateReadyWithAcceptedWarningsSteps(),
                Array.Empty<InterfaceProfileActivationPlanReason>());
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            (request.GuardResult.Decision == InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation ||
             request.WarningConfirmationResult?.Status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired ||
             warnings.Count > 0))
        {
            return CreatePlan(
                InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation,
                canExecuteLater: false,
                profileId,
                profileName,
                "Vor einer späteren Aktivierung müssen Warnungen bewusst bestätigt werden.",
                blockers,
                warnings,
                infos,
                CreateRequiresWarningConfirmationSteps(),
                warnings);
        }

        return CreatePlan(
            InterfaceProfileActivationPlanStatus.Unknown,
            canExecuteLater: false,
            profileId,
            profileName,
            "Aktivierungsplan ist nicht eindeutig.",
            blockers,
            warnings,
            infos,
            CreateNotAvailableSteps("Aktivierungsprüfung aktualisieren"),
            blockers);
    }

    private static void EvaluateProfileProtection(
        InterfaceProfileDefinition profile,
        List<InterfaceProfileActivationPlanReason> blockers)
    {
        if (profile.Metadata.IsBuiltIn)
        {
            AddUnique(blockers, Reason(
                InterfaceProfileActivationSeverity.Blocker,
                "activationPlan.profile.builtin",
                "BuiltIn-Profile dürfen nicht direkt aktiviert oder verändert werden.",
                "Bitte eine UserDefined-Kopie verwenden."));
        }

        if (!profile.Metadata.IsUserDefined)
        {
            AddUnique(blockers, Reason(
                InterfaceProfileActivationSeverity.Blocker,
                "activationPlan.profile.notUserDefined",
                "Aktivierungspläne sind nur für UserDefined-Schnittstellenprofile vorgesehen.",
                "Das Profil wird durch den Plan nicht verändert."));
        }
    }

    private static void CopyEvaluationReasons(
        InterfaceProfileActivationEvaluationResult evaluation,
        List<InterfaceProfileActivationPlanReason> blockers,
        List<InterfaceProfileActivationPlanReason> warnings,
        List<InterfaceProfileActivationPlanReason> infos)
    {
        foreach (var check in evaluation.Checks)
        {
            var reason = Reason(check.Severity, check.Code, check.Message, check.Detail);
            AddReasonBySeverity(reason, blockers, warnings, infos);
        }
    }

    private static void CopyGuardReasons(
        InterfaceProfileActivationGuardResult? guard,
        List<InterfaceProfileActivationPlanReason> blockers,
        List<InterfaceProfileActivationPlanReason> warnings,
        List<InterfaceProfileActivationPlanReason> infos)
    {
        if (guard is null)
        {
            return;
        }

        foreach (var reason in guard.BlockerReasons)
        {
            AddUnique(blockers, Reason(reason.Severity, reason.Code, reason.Message, reason.Detail));
        }

        foreach (var reason in guard.WarningReasons)
        {
            AddUnique(warnings, Reason(reason.Severity, reason.Code, reason.Message, reason.Detail));
        }

        foreach (var reason in guard.InfoReasons)
        {
            AddUnique(infos, Reason(reason.Severity, reason.Code, reason.Message, reason.Detail));
        }
    }

    private static void CopyWarningConfirmationReasons(
        InterfaceProfileActivationWarningConfirmationResult? warningConfirmation,
        List<InterfaceProfileActivationPlanReason> warnings)
    {
        if (warningConfirmation is null)
        {
            return;
        }

        foreach (var item in warningConfirmation.Warnings)
        {
            AddUnique(warnings, Reason(
                InterfaceProfileActivationSeverity.Warning,
                item.Code,
                item.Title,
                item.Detail));
        }
    }

    private static void AddReasonBySeverity(
        InterfaceProfileActivationPlanReason reason,
        List<InterfaceProfileActivationPlanReason> blockers,
        List<InterfaceProfileActivationPlanReason> warnings,
        List<InterfaceProfileActivationPlanReason> infos)
    {
        switch (reason.Severity)
        {
            case InterfaceProfileActivationSeverity.Blocker:
                AddUnique(blockers, reason);
                break;
            case InterfaceProfileActivationSeverity.Warning:
                AddUnique(warnings, reason);
                break;
            default:
                AddUnique(infos, reason);
                break;
        }
    }

    private static void AddUnique(
        List<InterfaceProfileActivationPlanReason> target,
        InterfaceProfileActivationPlanReason reason)
    {
        if (target.Any(existing => string.Equals(existing.Code, reason.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        target.Add(reason);
    }

    private static string CreateProfileProtectionMessage(
        IReadOnlyList<InterfaceProfileActivationPlanReason> blockers)
    {
        if (blockers.Any(reason => reason.Code == "activationPlan.profile.builtin"))
        {
            return "BuiltIn-Profile dürfen nicht direkt aktiviert oder verändert werden.";
        }

        if (blockers.Any(reason => reason.Code == "activationPlan.profile.notUserDefined"))
        {
            return "Aktivierungspläne sind nur für UserDefined-Schnittstellenprofile vorgesehen.";
        }

        return "Profilschutz verhindert eine spätere Aktivierung.";
    }

    private static IReadOnlyList<InterfaceProfileActivationPlanStep> CreateReadySteps()
    {
        return new[]
        {
            Step(
                "activate.profile",
                "Profil aktivieren",
                "Das Schnittstellenprofil würde bei einer späteren echten Aktivierung als aktiv markiert.",
                wouldExecuteLater: true,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Info),
            Step(
                "use.configuration",
                "Automatik gemäß Profilkonfiguration verwenden",
                "Die spätere Verarbeitung würde ausschließlich die bestehende Schnittstellenprofil-Konfiguration verwenden.",
                wouldExecuteLater: true,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Info),
            Step(
                "respect.attachment.configuration",
                "XDT-Anhang-Konfiguration berücksichtigen",
                "XDT-Anhang-Automatik würde nur entsprechend der bestehenden Profilkonfiguration berücksichtigt.",
                wouldExecuteLater: true,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Info),
            Step(
                "save.profile",
                "Profiländerung speichern",
                "Die Profiländerung würde erst bei einer späteren echten Aktivierung gespeichert.",
                wouldExecuteLater: true,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Info),
            Step(
                "processing.not.started",
                "Verarbeitung nicht starten",
                "Auch der Plan führt keine Verarbeitung aus und führt keine Verarbeitung beim App-Start ein.",
                wouldExecuteLater: false,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Info)
        };
    }

    private static IReadOnlyList<InterfaceProfileActivationPlanStep> CreateReadyWithAcceptedWarningsSteps()
    {
        return new[]
        {
            Step(
                "warnings.accepted.hypothetical",
                "Warnungen hypothetisch bestätigt",
                "Die Warnungsbestätigung ist nur für den Plan modelliert und wird nicht gespeichert.",
                wouldExecuteLater: false,
                isBlocked: false,
                InterfaceProfileActivationSeverity.Warning)
        }
        .Concat(CreateReadySteps())
        .ToList();
    }

    private static IReadOnlyList<InterfaceProfileActivationPlanStep> CreateRequiresWarningConfirmationSteps()
    {
        return new[]
        {
            Step(
                "warnings.confirm",
                "Warnungen bestätigen",
                "Vor einer späteren echten Aktivierung müssten die aufgeführten Warnungen bewusst bestätigt werden.",
                wouldExecuteLater: true,
                isBlocked: true,
                InterfaceProfileActivationSeverity.Warning),
            Step(
                "activate.profile",
                "Profil aktivieren",
                "Dieser spätere Schritt bleibt blockiert, bis Warnungen bewusst bestätigt wurden.",
                wouldExecuteLater: true,
                isBlocked: true,
                InterfaceProfileActivationSeverity.Warning)
        };
    }

    private static IReadOnlyList<InterfaceProfileActivationPlanStep> CreateBlockedSteps(string description)
    {
        return new[]
        {
            Step(
                "resolve.blockers",
                "Blocker beheben",
                description,
                wouldExecuteLater: false,
                isBlocked: true,
                InterfaceProfileActivationSeverity.Blocker)
        };
    }

    private static IReadOnlyList<InterfaceProfileActivationPlanStep> CreateNotAvailableSteps(string description)
    {
        return new[]
        {
            Step(
                "plan.notAvailable",
                "Plan nicht verfügbar",
                description,
                wouldExecuteLater: false,
                isBlocked: true,
                InterfaceProfileActivationSeverity.Info)
        };
    }

    private static InterfaceProfileActivationPlan CreatePlan(
        InterfaceProfileActivationPlanStatus status,
        bool canExecuteLater,
        string profileId,
        string profileName,
        string summaryMessage,
        IReadOnlyList<InterfaceProfileActivationPlanReason> blockers,
        IReadOnlyList<InterfaceProfileActivationPlanReason> warnings,
        IReadOnlyList<InterfaceProfileActivationPlanReason> infos,
        IReadOnlyList<InterfaceProfileActivationPlanStep> plannedSteps,
        IReadOnlyList<InterfaceProfileActivationPlanReason> missingRequirements)
    {
        return new InterfaceProfileActivationPlan(
            status,
            canExecuteLater,
            IsPreviewOnly: true,
            profileId,
            profileName,
            summaryMessage,
            blockers,
            warnings,
            infos,
            plannedSteps,
            missingRequirements);
    }

    private static InterfaceProfileActivationPlanReason Reason(
        InterfaceProfileActivationSeverity severity,
        string code,
        string message,
        string? detail = null)
    {
        return new InterfaceProfileActivationPlanReason(
            severity,
            code,
            message,
            detail);
    }

    private static InterfaceProfileActivationPlanStep Step(
        string code,
        string title,
        string description,
        bool wouldExecuteLater,
        bool isBlocked,
        InterfaceProfileActivationSeverity severity)
    {
        return new InterfaceProfileActivationPlanStep(
            code,
            title,
            description,
            wouldExecuteLater,
            isBlocked,
            severity);
    }
}
