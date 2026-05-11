using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationWarningConfirmationService
{
    public InterfaceProfileActivationWarningConfirmationResult PrepareWarningConfirmation(
        InterfaceProfileActivationWarningConfirmationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var warnings = CreateWarningItems(request.EvaluationResult);
        var blockers = new List<InterfaceProfileActivationWarningConfirmationReason>();

        if (request.Profile is null)
        {
            blockers.Add(Blocker("warningConfirmation.profile.missing", "Kein Schnittstellenprofil ausgewählt."));
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.NotAvailable,
                "-",
                "-",
                warnings,
                blockers,
                "Kein Schnittstellenprofil ausgewählt.");
        }

        var profileId = string.IsNullOrWhiteSpace(request.Profile.Metadata.Id)
            ? "-"
            : request.Profile.Metadata.Id;
        var profileName = string.IsNullOrWhiteSpace(request.Profile.Metadata.Name)
            ? profileId
            : request.Profile.Metadata.Name;

        EvaluateProfileProtection(request.Profile, blockers);
        if (blockers.Count > 0)
        {
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.Blocked,
                profileId,
                profileName,
                warnings,
                blockers,
                CreateProfileProtectionMessage(blockers));
        }

        if (request.EvaluationResult is null)
        {
            blockers.Add(Blocker(
                "warningConfirmation.evaluation.missing",
                "Keine aktuelle Aktivierungsbewertung vorhanden."));
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.MissingEvaluation,
                profileId,
                profileName,
                warnings,
                blockers,
                "Keine aktuelle Aktivierungsbewertung vorhanden.");
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Blocked ||
            request.EvaluationResult.Blockers.Count > 0)
        {
            blockers.AddRange(request.EvaluationResult.Blockers.Select(ToReason));
            AddGuardBlockers(request.GuardResult, blockers);

            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.Blocked,
                profileId,
                profileName,
                warnings,
                blockers,
                "Warnungen können erst bestätigt werden, wenn keine Blocker mehr vorhanden sind.");
        }

        if (request.EvaluationResult.ActivationStatus is InterfaceProfileActivationStatus.Unknown or
            InterfaceProfileActivationStatus.NotEvaluated)
        {
            blockers.Add(Blocker(
                "warningConfirmation.evaluation.unknown",
                "Die Aktivierungsbewertung ist nicht eindeutig."));
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.Unknown,
                profileId,
                profileName,
                warnings,
                blockers,
                "Die Aktivierungsbewertung ist nicht eindeutig.");
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            warnings.Count == 0)
        {
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.NoWarnings,
                profileId,
                profileName,
                warnings,
                blockers,
                "Es sind keine Warnungen vorhanden, die bestätigt werden müssen.");
        }

        if (request.EvaluationResult.ActivationStatus == InterfaceProfileActivationStatus.ReadyWithWarnings &&
            warnings.Count > 0)
        {
            return CreateResult(
                true,
                InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
                profileId,
                profileName,
                warnings,
                blockers,
                "Vor einer späteren Aktivierung müssen diese Warnungen bewusst bestätigt werden.");
        }

        if (warnings.Count == 0)
        {
            return CreateResult(
                false,
                InterfaceProfileActivationWarningConfirmationStatus.NoWarnings,
                profileId,
                profileName,
                warnings,
                blockers,
                "Es sind keine Warnungen vorhanden, die bestätigt werden müssen.");
        }

        return CreateResult(
            false,
            InterfaceProfileActivationWarningConfirmationStatus.Unknown,
            profileId,
            profileName,
            warnings,
            blockers,
            "Die Aktivierungsbewertung ist nicht eindeutig.");
    }

    private static void EvaluateProfileProtection(
        InterfaceProfileDefinition profile,
        List<InterfaceProfileActivationWarningConfirmationReason> blockers)
    {
        if (profile.Metadata.IsBuiltIn)
        {
            blockers.Add(Blocker(
                "warningConfirmation.profile.builtin",
                "BuiltIn-Profile dürfen nicht direkt aktiviert oder bestätigt werden."));
        }

        if (!profile.Metadata.IsUserDefined)
        {
            blockers.Add(Blocker(
                "warningConfirmation.profile.notUserDefined",
                "Warnungsbestätigung ist nur für UserDefined-Schnittstellenprofile vorgesehen."));
        }
    }

    private static IReadOnlyList<InterfaceProfileActivationWarningConfirmationItem> CreateWarningItems(
        InterfaceProfileActivationEvaluationResult? evaluationResult)
    {
        if (evaluationResult is null)
        {
            return Array.Empty<InterfaceProfileActivationWarningConfirmationItem>();
        }

        return evaluationResult.Warnings
            .Select(check => new InterfaceProfileActivationWarningConfirmationItem(
                check.Area,
                check.Code,
                check.Message,
                check.Detail,
                check.Severity,
                IsRequiredForActivation: true))
            .ToList();
    }

    private static void AddGuardBlockers(
        InterfaceProfileActivationGuardResult? guardResult,
        List<InterfaceProfileActivationWarningConfirmationReason> blockers)
    {
        if (guardResult is null)
        {
            return;
        }

        foreach (var reason in guardResult.BlockerReasons)
        {
            if (blockers.Any(existing => string.Equals(existing.Code, reason.Code, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            blockers.Add(new InterfaceProfileActivationWarningConfirmationReason(
                reason.Severity,
                reason.Code,
                reason.Message,
                reason.Detail));
        }
    }

    private static InterfaceProfileActivationWarningConfirmationReason ToReason(
        InterfaceProfileActivationCheckResult check)
    {
        return new InterfaceProfileActivationWarningConfirmationReason(
            check.Severity,
            check.Code,
            check.Message,
            check.Detail);
    }

    private static string CreateProfileProtectionMessage(
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationReason> blockers)
    {
        if (blockers.Any(reason => reason.Code == "warningConfirmation.profile.builtin"))
        {
            return "BuiltIn-Profile dürfen nicht direkt aktiviert oder bestätigt werden.";
        }

        if (blockers.Any(reason => reason.Code == "warningConfirmation.profile.notUserDefined"))
        {
            return "Warnungsbestätigung ist nur für UserDefined-Schnittstellenprofile vorgesehen.";
        }

        return "Warnungsbestätigung ist für dieses Profil nicht verfügbar.";
    }

    private static InterfaceProfileActivationWarningConfirmationResult CreateResult(
        bool canRequestConfirmation,
        InterfaceProfileActivationWarningConfirmationStatus status,
        string profileId,
        string profileName,
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationItem> warnings,
        IReadOnlyList<InterfaceProfileActivationWarningConfirmationReason> blockers,
        string message)
    {
        return new InterfaceProfileActivationWarningConfirmationResult(
            canRequestConfirmation,
            status,
            profileId,
            profileName,
            warnings,
            blockers,
            message);
    }

    private static InterfaceProfileActivationWarningConfirmationReason Blocker(
        string code,
        string message,
        string? detail = null)
    {
        return new InterfaceProfileActivationWarningConfirmationReason(
            InterfaceProfileActivationSeverity.Blocker,
            code,
            message,
            detail);
    }
}
