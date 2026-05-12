using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPreparationPreviewService
{
    private const int MaxImportantItems = 5;
    private const string SafetyNotice = "Dies ist nur eine Vorschau. Es wurde nichts gespeichert und nichts aktiviert.";
    private const string GuardUnknownDecisionText = "Nicht eindeutig";
    private const string GuardNoCanProceedText = "Nein";
    private const string GuardUnknownMessage = "Es liegt keine eindeutige aktuelle Aktivierungsbewertung vor.";

    public InterfaceProfileActivationPreparationPreview CreateEmpty()
    {
        const string summary = "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.";

        return CreatePreview(
            profileName: "-",
            statusText: "Nicht bewertet",
            v1CanActivateText: "Nein",
            blockerCount: 0,
            warningCount: 0,
            infoCount: 0,
            blockers: Array.Empty<string>(),
            warnings: Array.Empty<string>(),
            infos: Array.Empty<string>(),
            guardDecisionText: GuardUnknownDecisionText,
            guardCanProceedText: GuardNoCanProceedText,
            guardMessage: GuardUnknownMessage,
            guardReasons: Array.Empty<string>(),
            summary);
    }

    public InterfaceProfileActivationPreparationPreview CreateError(string message)
    {
        var summary = string.IsNullOrWhiteSpace(message)
            ? "Die Aktivierungsvorschau konnte nicht erstellt werden."
            : message;
        var blockers = new[] { summary };

        return CreatePreview(
            profileName: "-",
            statusText: "Fehler",
            v1CanActivateText: "Nein",
            blockerCount: 1,
            warningCount: 0,
            infoCount: 0,
            blockers,
            warnings: Array.Empty<string>(),
            infos: Array.Empty<string>(),
            guardDecisionText: GuardUnknownDecisionText,
            guardCanProceedText: GuardNoCanProceedText,
            guardMessage: "Die technische Schutzprüfung konnte nicht eindeutig abgeschlossen werden.",
            guardReasons: blockers,
            summary);
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result)
    {
        return Create(profile, result, guardResult: null);
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result,
        InterfaceProfileActivationGuardResult? guardResult)
    {
        if (profile is null || result is null)
        {
            return CreateEmpty();
        }

        var blockers = result.Blockers
            .Take(MaxImportantItems)
            .Select(FormatCheck)
            .ToList();
        var warnings = result.Warnings
            .Take(MaxImportantItems)
            .Select(FormatCheck)
            .ToList();
        var infos = result.Infos
            .Take(MaxImportantItems)
            .Select(FormatCheck)
            .ToList();
        var guardDecisionText = guardResult is null
            ? GuardUnknownDecisionText
            : FormatGuardDecision(guardResult.Decision);
        var guardCanProceedText = guardResult?.CanProceed == true ? "Ja" : "Nein";
        var guardMessage = string.IsNullOrWhiteSpace(guardResult?.Message)
            ? GuardUnknownMessage
            : guardResult.Message;
        var guardReasons = guardResult is null
            ? Array.Empty<string>()
            : BuildGuardReasons(guardResult);
        var v1CanActivate = IsV1Activatable(profile, result, guardResult);
        var summary = BuildSummaryMessage(result, v1CanActivate);

        return CreatePreview(
            profileName: string.IsNullOrWhiteSpace(profile.Metadata.Name) ? profile.Metadata.Id : profile.Metadata.Name,
            statusText: FormatStatus(result.ActivationStatus),
            v1CanActivateText: v1CanActivate ? "Ja" : "Nein",
            blockerCount: result.Blockers.Count,
            warningCount: result.Warnings.Count,
            infoCount: result.Infos.Count,
            blockers,
            warnings,
            infos,
            guardDecisionText,
            guardCanProceedText,
            guardMessage,
            guardReasons,
            summary);
    }

    private static InterfaceProfileActivationPreparationPreview CreatePreview(
        string profileName,
        string statusText,
        string v1CanActivateText,
        int blockerCount,
        int warningCount,
        int infoCount,
        IReadOnlyList<string> blockers,
        IReadOnlyList<string> warnings,
        IReadOnlyList<string> infos,
        string guardDecisionText,
        string guardCanProceedText,
        string guardMessage,
        IReadOnlyList<string> guardReasons,
        string summary)
    {
        return new InterfaceProfileActivationPreparationPreview(
            Title: "Aktivierung vorbereiten",
            ProfileName: profileName,
            StatusText: statusText,
            V1CanActivateText: v1CanActivateText,
            BlockerCount: blockerCount,
            WarningCount: warningCount,
            InfoCount: infoCount,
            ImportantBlockers: blockers,
            ImportantWarnings: warnings,
            ImportantInfos: infos,
            GuardDecisionText: guardDecisionText,
            GuardCanProceedText: guardCanProceedText,
            GuardMessage: guardMessage,
            GuardReasons: guardReasons,
            SummaryMessage: summary,
            SafetyNotice: SafetyNotice,
            MessageText: BuildMessageText(
                profileName,
                statusText,
                v1CanActivateText,
                blockerCount,
                warningCount,
                infoCount,
                blockers,
                warnings,
                infos,
                guardDecisionText,
                guardCanProceedText,
                guardMessage,
                guardReasons,
                summary,
                SafetyNotice));
    }

    private static bool IsV1Activatable(
        InterfaceProfileDefinition profile,
        InterfaceProfileActivationEvaluationResult result,
        InterfaceProfileActivationGuardResult? guardResult)
    {
        return profile.Metadata.IsUserDefined &&
            !profile.Metadata.IsBuiltIn &&
            result.ActivationStatus == InterfaceProfileActivationStatus.Ready &&
            result.Blockers.Count == 0 &&
            result.Warnings.Count == 0 &&
            guardResult?.Decision == InterfaceProfileActivationGuardDecision.Allowed &&
            guardResult.CanProceed;
    }

    private static string BuildSummaryMessage(
        InterfaceProfileActivationEvaluationResult result,
        bool v1CanActivate)
    {
        if (v1CanActivate)
        {
            return "Dieses Profil wäre nach V1 grundsätzlich aktivierbar.";
        }

        return result.ActivationStatus switch
        {
            InterfaceProfileActivationStatus.ReadyWithWarnings =>
                "Dieses Profil wird in V1 nicht aktiviert, weil Warnungen vorhanden sind.",
            InterfaceProfileActivationStatus.Blocked =>
                "Dieses Profil kann nicht aktiviert werden.",
            InterfaceProfileActivationStatus.Ready =>
                "Dieses Profil ist nach V1 nicht aktivierbar, weil eine technische Voraussetzung fehlt.",
            _ =>
                "Dieses Profil kann in V1 nicht aktiviert werden, weil die Bewertung nicht eindeutig ist."
        };
    }

    private static string FormatStatus(InterfaceProfileActivationStatus status)
    {
        return status switch
        {
            InterfaceProfileActivationStatus.Ready => "Ready",
            InterfaceProfileActivationStatus.ReadyWithWarnings => "ReadyWithWarnings",
            InterfaceProfileActivationStatus.Blocked => "Blocked",
            InterfaceProfileActivationStatus.NotEvaluated => "Nicht bewertet",
            InterfaceProfileActivationStatus.Unknown => "Unknown",
            _ => status.ToString()
        };
    }

    private static string FormatCheck(InterfaceProfileActivationCheckResult check)
    {
        if (string.IsNullOrWhiteSpace(check.Detail))
        {
            return check.Message;
        }

        return $"{check.Message} {check.Detail}";
    }

    private static string FormatGuardDecision(InterfaceProfileActivationGuardDecision decision)
    {
        return decision switch
        {
            InterfaceProfileActivationGuardDecision.Allowed => "Technisch zulässig",
            InterfaceProfileActivationGuardDecision.Blocked => "Blockiert",
            InterfaceProfileActivationGuardDecision.Unknown => GuardUnknownDecisionText,
            _ => decision.ToString()
        };
    }

    private static IReadOnlyList<string> BuildGuardReasons(InterfaceProfileActivationGuardResult guardResult)
    {
        return guardResult.BlockerReasons
            .Concat(guardResult.WarningReasons)
            .Concat(guardResult.InfoReasons)
            .Take(MaxImportantItems)
            .Select(FormatGuardReason)
            .ToList();
    }

    private static string FormatGuardReason(InterfaceProfileActivationGuardReason reason)
    {
        var severity = reason.Severity switch
        {
            InterfaceProfileActivationSeverity.Blocker => "Blocker",
            InterfaceProfileActivationSeverity.Warning => "Warnung",
            InterfaceProfileActivationSeverity.Info => "Hinweis",
            _ => reason.Severity.ToString()
        };

        if (string.IsNullOrWhiteSpace(reason.Detail))
        {
            return $"{severity}: {reason.Message}";
        }

        return $"{severity}: {reason.Message} {reason.Detail}";
    }

    private static string BuildMessageText(
        string profileName,
        string statusText,
        string v1CanActivateText,
        int blockerCount,
        int warningCount,
        int infoCount,
        IReadOnlyList<string> blockers,
        IReadOnlyList<string> warnings,
        IReadOnlyList<string> infos,
        string guardDecisionText,
        string guardCanProceedText,
        string guardMessage,
        IReadOnlyList<string> guardReasons,
        string summaryMessage,
        string safetyNotice)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Aktivierung vorbereiten");
        builder.AppendLine();
        builder.AppendLine($"Schnittstellenprofil: {profileName}");
        builder.AppendLine($"Status: {statusText}");
        builder.AppendLine($"Aktivierbar nach V1: {v1CanActivateText}");
        builder.AppendLine($"Zusammenfassung: {blockerCount} Blocker, {warningCount} Warnungen, {infoCount} Hinweise");
        builder.AppendLine();
        builder.AppendLine("Technische Schutzprüfung:");
        builder.AppendLine($"Entscheidung: {guardDecisionText}");
        builder.AppendLine($"Technisch freigegeben: {guardCanProceedText}");

        if (!string.IsNullOrWhiteSpace(guardMessage))
        {
            builder.AppendLine($"Hinweis: {guardMessage}");
        }

        AppendSection(builder, "Blocker", blockers);
        AppendSection(builder, "Warnungen", warnings);
        AppendSection(builder, "Hinweise", infos);

        if (guardReasons.Count > 0)
        {
            AppendSection(builder, "Technische Hinweise", guardReasons);
        }

        builder.AppendLine();
        builder.AppendLine(summaryMessage);
        builder.AppendLine(safetyNotice);

        return builder.ToString().TrimEnd();
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IReadOnlyList<string> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine($"{title}:");
        for (var index = 0; index < items.Count; index++)
        {
            builder.AppendLine($"{index + 1}. {items[index]}");
        }
    }
}
