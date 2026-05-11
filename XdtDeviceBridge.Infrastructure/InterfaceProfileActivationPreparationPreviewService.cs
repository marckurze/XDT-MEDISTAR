using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPreparationPreviewService
{
    private const int MaxImportantItems = 5;

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
                summary,
                safetyNotice));
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result)
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

    private static string BuildMessageText(
        string profileName,
        string statusText,
        string canActivateText,
        int blockerCount,
        int warningCount,
        int infoCount,
        IReadOnlyList<string> importantBlockers,
        IReadOnlyList<string> importantWarnings,
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
