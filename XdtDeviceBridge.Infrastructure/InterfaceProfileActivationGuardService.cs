using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationGuardService
{
    public InterfaceProfileActivationGuardResult ValidateActivationRequest(InterfaceProfileActivationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var blockers = new List<InterfaceProfileActivationGuardReason>();
        var warnings = new List<InterfaceProfileActivationGuardReason>();
        var infos = new List<InterfaceProfileActivationGuardReason>();

        if (request.Profile is null)
        {
            blockers.Add(Blocker("guard.profile.missing", "Kein Schnittstellenprofil ausgewählt."));
        }
        else
        {
            EvaluateProfileSafety(request.Profile, blockers);
        }

        if (request.EvaluationResult is null)
        {
            blockers.Add(Blocker(
                "guard.evaluation.missing",
                "Keine aktuelle Aktivierungsbewertung vorhanden.",
                "Bitte zuerst die Aktivierungsprüfung aktualisieren."));
            return CreateResult(false, InterfaceProfileActivationGuardDecision.Unknown, blockers, warnings, infos);
        }

        CopyEvaluationReasons(request.EvaluationResult, blockers, warnings, infos);

        if (blockers.Count > 0)
        {
            return CreateResult(false, InterfaceProfileActivationGuardDecision.Blocked, blockers, warnings, infos);
        }

        return request.EvaluationResult.ActivationStatus switch
        {
            InterfaceProfileActivationStatus.Ready => CreateResult(
                true,
                InterfaceProfileActivationGuardDecision.Allowed,
                blockers,
                warnings,
                infos),

            InterfaceProfileActivationStatus.ReadyWithWarnings => CreateResult(
                false,
                InterfaceProfileActivationGuardDecision.Blocked,
                new List<InterfaceProfileActivationGuardReason>(blockers)
                {
                    Blocker(
                        "guard.evaluation.warningNotAllowedInV1",
                        "ReadyWithWarnings wird in V1 nicht produktiv aktiviert.",
                        "Warnungen werden angezeigt, aber nicht bestätigt oder übergangen.")
                },
                warnings,
                infos),

            InterfaceProfileActivationStatus.Blocked => CreateResult(
                false,
                InterfaceProfileActivationGuardDecision.Blocked,
                blockers.Count > 0
                    ? blockers
                    : new List<InterfaceProfileActivationGuardReason>
                    {
                        Blocker("guard.evaluation.blocked", "Aktivierungsbewertung ist blockiert.")
                    },
                warnings,
                infos),

            InterfaceProfileActivationStatus.NotEvaluated or InterfaceProfileActivationStatus.Unknown => CreateResult(
                false,
                InterfaceProfileActivationGuardDecision.Unknown,
                new List<InterfaceProfileActivationGuardReason>(blockers)
                {
                    Blocker("guard.evaluation.unknown", "Aktivierungsbewertung ist nicht eindeutig.")
                },
                warnings,
                infos),

            _ => CreateResult(
                false,
                InterfaceProfileActivationGuardDecision.Unknown,
                new List<InterfaceProfileActivationGuardReason>(blockers)
                {
                    Blocker("guard.evaluation.unknown", "Aktivierungsbewertung ist nicht eindeutig.")
                },
                warnings,
                infos)
        };
    }

    private static void EvaluateProfileSafety(
        InterfaceProfileDefinition profile,
        List<InterfaceProfileActivationGuardReason> blockers)
    {
        if (profile.Metadata.IsBuiltIn)
        {
            blockers.Add(Blocker(
                "guard.profile.builtin",
                "BuiltIn-Schnittstellenprofile dürfen nicht direkt aktiviert oder verändert werden.",
                "Bitte eine UserDefined-Kopie verwenden."));
        }

        if (!profile.Metadata.IsUserDefined)
        {
            blockers.Add(Blocker(
                "guard.profile.notUserDefined",
                "Aktivierung ist nur für UserDefined-Schnittstellenprofile vorgesehen.",
                "Das Profil wird durch die Schutzprüfung nicht verändert."));
        }
    }

    private static void CopyEvaluationReasons(
        InterfaceProfileActivationEvaluationResult evaluationResult,
        List<InterfaceProfileActivationGuardReason> blockers,
        List<InterfaceProfileActivationGuardReason> warnings,
        List<InterfaceProfileActivationGuardReason> infos)
    {
        blockers.AddRange(evaluationResult.Blockers.Select(ToGuardReason));
        warnings.AddRange(evaluationResult.Warnings.Select(ToGuardReason));
        infos.AddRange(evaluationResult.Infos.Select(ToGuardReason));
    }

    private static InterfaceProfileActivationGuardReason ToGuardReason(InterfaceProfileActivationCheckResult check)
    {
        return new InterfaceProfileActivationGuardReason(
            check.Severity,
            check.Code,
            check.Message,
            check.Detail);
    }

    private static InterfaceProfileActivationGuardResult CreateResult(
        bool canProceed,
        InterfaceProfileActivationGuardDecision decision,
        IReadOnlyList<InterfaceProfileActivationGuardReason> blockers,
        IReadOnlyList<InterfaceProfileActivationGuardReason> warnings,
        IReadOnlyList<InterfaceProfileActivationGuardReason> infos)
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            blockers,
            warnings,
            infos,
            CreateMessage(decision));
    }

    private static string CreateMessage(InterfaceProfileActivationGuardDecision decision)
    {
        return decision switch
        {
            InterfaceProfileActivationGuardDecision.Allowed =>
                "Schutzprüfung: Eine spätere V1-Aktivierung wäre technisch zulässig.",
            InterfaceProfileActivationGuardDecision.Blocked =>
                "Schutzprüfung: Aktivierung blockiert. Bitte beheben Sie zuerst die blockierenden Punkte.",
            InterfaceProfileActivationGuardDecision.Unknown =>
                "Schutzprüfung: Aktivierung nicht freigegeben, weil keine eindeutige aktuelle Bewertung vorliegt.",
            _ =>
                "Schutzprüfung: Aktivierung nicht freigegeben."
        };
    }

    private static InterfaceProfileActivationGuardReason Blocker(
        string code,
        string message,
        string? detail = null)
    {
        return new InterfaceProfileActivationGuardReason(
            InterfaceProfileActivationSeverity.Blocker,
            code,
            message,
            detail);
    }
}
