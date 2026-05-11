using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPreparationPreviewService
{
    private const int MaxImportantItems = 5;
    private const string GuardUnknownDecisionText = "Nicht eindeutig";
    private const string GuardNoCanProceedText = "Nein";
    private const string GuardUnknownMessage = "Es liegt keine eindeutige aktuelle Aktivierungsbewertung vor.";

    public InterfaceProfileActivationPreparationPreview CreateEmpty()
    {
        const string safetyNotice = "Dies ist nur eine Vorschau. Es wurden keine Änderungen gespeichert.";
        const string summary = "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.";

        return new InterfaceProfileActivationPreparationPreview(
            Title: "Aktivierung vorbereiten",
            ProfileName: "-",
            StatusText: "Nicht bewertet",
            CanActivateText: "Nein",
            BlockerCount: 0,
            WarningCount: 0,
            InfoCount: 0,
            ImportantBlockers: Array.Empty<string>(),
            ImportantWarnings: Array.Empty<string>(),
            GuardDecisionText: GuardUnknownDecisionText,
            GuardCanProceedText: GuardNoCanProceedText,
            GuardMessage: GuardUnknownMessage,
            GuardReasons: Array.Empty<string>(),
            SummaryMessage: summary,
            SafetyNotice: safetyNotice,
            MessageText: BuildMessageText(
                profileName: "-",
                statusText: "Nicht bewertet",
                canActivateText: "Nein",
                blockerCount: 0,
                warningCount: 0,
                infoCount: 0,
                importantBlockers: Array.Empty<string>(),
                importantWarnings: Array.Empty<string>(),
                guardDecisionText: GuardUnknownDecisionText,
                guardCanProceedText: GuardNoCanProceedText,
                guardMessage: GuardUnknownMessage,
                guardReasons: Array.Empty<string>(),
                summary,
                safetyNotice));
    }

    public InterfaceProfileActivationPreparationPreview CreateError(string message)
    {
        const string safetyNotice = "Dies ist nur eine Vorschau. Es wurden keine Änderungen gespeichert.";
        var summary = string.IsNullOrWhiteSpace(message)
            ? "Die Aktivierungsvorschau konnte nicht erstellt werden."
            : message;
        var blockers = new[] { summary };

        return new InterfaceProfileActivationPreparationPreview(
            Title: "Aktivierung vorbereiten",
            ProfileName: "-",
            StatusText: "Fehler",
            CanActivateText: "Nein",
            BlockerCount: 1,
            WarningCount: 0,
            InfoCount: 0,
            ImportantBlockers: blockers,
            ImportantWarnings: Array.Empty<string>(),
            GuardDecisionText: GuardUnknownDecisionText,
            GuardCanProceedText: GuardNoCanProceedText,
            GuardMessage: "Die technische Schutzprüfung konnte nicht eindeutig abgeschlossen werden.",
            GuardReasons: blockers,
            SummaryMessage: summary,
            SafetyNotice: safetyNotice,
            MessageText: BuildMessageText(
                profileName: "-",
                statusText: "Fehler",
                canActivateText: "Nein",
                blockerCount: 1,
                warningCount: 0,
                infoCount: 0,
                importantBlockers: blockers,
                importantWarnings: Array.Empty<string>(),
                guardDecisionText: GuardUnknownDecisionText,
                guardCanProceedText: GuardNoCanProceedText,
                guardMessage: "Die technische Schutzprüfung konnte nicht eindeutig abgeschlossen werden.",
                guardReasons: blockers,
                summary,
                safetyNotice));
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

        var statusText = FormatStatus(result.ActivationStatus);
        var canActivateText = result.CanActivate ? "Ja" : "Nein";
        var blockers = result.Blockers
            .Take(MaxImportantItems)
            .Select(FormatCheck)
            .ToList();
        var warnings = result.Warnings
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
        var summary = BuildSummaryMessage(result);
        const string safetyNotice = "Dies ist nur eine Vorschau. Es wurden keine Änderungen gespeichert.";

        return new InterfaceProfileActivationPreparationPreview(
            Title: "Aktivierung vorbereiten",
            ProfileName: string.IsNullOrWhiteSpace(profile.Metadata.Name) ? profile.Metadata.Id : profile.Metadata.Name,
            StatusText: statusText,
            CanActivateText: canActivateText,
            BlockerCount: result.Blockers.Count,
            WarningCount: result.Warnings.Count,
            InfoCount: result.Infos.Count,
            ImportantBlockers: blockers,
            ImportantWarnings: warnings,
            GuardDecisionText: guardDecisionText,
            GuardCanProceedText: guardCanProceedText,
            GuardMessage: guardMessage,
            GuardReasons: guardReasons,
            SummaryMessage: summary,
            SafetyNotice: safetyNotice,
            MessageText: BuildMessageText(
                profileName: string.IsNullOrWhiteSpace(profile.Metadata.Name) ? profile.Metadata.Id : profile.Metadata.Name,
                statusText,
                canActivateText,
                result.Blockers.Count,
                result.Warnings.Count,
                result.Infos.Count,
                blockers,
                warnings,
                guardDecisionText,
                guardCanProceedText,
                guardMessage,
                guardReasons,
                summary,
                safetyNotice));
    }

    private static string BuildSummaryMessage(InterfaceProfileActivationEvaluationResult result)
    {
        return result.ActivationStatus switch
        {
            InterfaceProfileActivationStatus.Blocked =>
                "Dieses Profil kann aktuell nicht aktiviert werden. Bitte beheben Sie zuerst die blockierenden Punkte.",
            InterfaceProfileActivationStatus.ReadyWithWarnings =>
                "Dieses Profil wäre grundsätzlich aktivierbar. Bitte prüfen Sie die Warnungen vor einer späteren Aktivierung.",
            InterfaceProfileActivationStatus.Ready =>
                "Dieses Profil wäre grundsätzlich aktivierbar. In diesem Schritt wurde noch nichts aktiviert.",
            _ =>
                "Die Aktivierbarkeit ist unklar. Bitte prüfen Sie die Hinweise."
        };
    }

    private static string FormatStatus(InterfaceProfileActivationStatus status)
    {
        return status switch
        {
            InterfaceProfileActivationStatus.Ready => "Aktivierbar",
            InterfaceProfileActivationStatus.ReadyWithWarnings => "Aktivierbar mit Warnungen",
            InterfaceProfileActivationStatus.Blocked => "Blockiert",
            InterfaceProfileActivationStatus.NotEvaluated => "Nicht bewertet",
            InterfaceProfileActivationStatus.Unknown => "Unklar",
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
            InterfaceProfileActivationGuardDecision.AllowedWithWarnings => "Zulässig mit Warnungen",
            InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation => "Warnungsbestätigung erforderlich",
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
        string canActivateText,
        int blockerCount,
        int warningCount,
        int infoCount,
        IReadOnlyList<string> importantBlockers,
        IReadOnlyList<string> importantWarnings,
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
        builder.AppendLine("Schnittstellenprofil:");
        builder.AppendLine(profileName);
        builder.AppendLine();
        builder.AppendLine($"Status: {statusText}");
        builder.AppendLine($"Aktivierbar: {canActivateText}");
        builder.AppendLine($"Zusammenfassung: {blockerCount} Blocker, {warningCount} Warnungen, {infoCount} Hinweise");
        builder.AppendLine();
        builder.AppendLine(summaryMessage);
        builder.AppendLine();
        builder.AppendLine("Technische Schutzprüfung:");
        builder.AppendLine($"Entscheidung: {guardDecisionText}");
        builder.AppendLine($"Technisch freigegeben: {guardCanProceedText}");
        builder.AppendLine($"Hinweis: {guardMessage}");

        if (guardReasons.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Guard-Gründe:");
            AppendNumberedItems(builder, guardReasons);
        }

        if (importantBlockers.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Blocker:");
            AppendNumberedItems(builder, importantBlockers);
        }

        if (importantWarnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Warnungen:");
            AppendNumberedItems(builder, importantWarnings);
        }

        builder.AppendLine();
        builder.AppendLine(safetyNotice);

        return builder.ToString().TrimEnd();
    }

    private static void AppendNumberedItems(StringBuilder builder, IReadOnlyList<string> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            builder.AppendLine($"{index + 1}. {items[index]}");
        }
    }
}
