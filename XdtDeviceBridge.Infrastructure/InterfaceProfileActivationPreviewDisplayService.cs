namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPreviewDisplayService
{
    public InterfaceProfileActivationPreviewDisplay CreateEmpty()
    {
        return new InterfaceProfileActivationPreviewDisplay(
            StatusText: "Status: -",
            CanActivateText: "Aktivierbar: -",
            BlockerCount: 0,
            WarningCount: 0,
            InfoCount: 0,
            SummaryText: "Blocker: 0 | Warnungen: 0 | Hinweise: 0",
            HintText: "Bitte wählen Sie ein Schnittstellenprofil aus, um die Aktivierungsprüfung anzuzeigen.",
            Rows: Array.Empty<InterfaceProfileActivationPreviewRow>());
    }

    public InterfaceProfileActivationPreviewDisplay CreateError(string message)
    {
        return new InterfaceProfileActivationPreviewDisplay(
            StatusText: "Status: Fehler",
            CanActivateText: "Aktivierbar: Nein",
            BlockerCount: 1,
            WarningCount: 0,
            InfoCount: 0,
            SummaryText: "Blocker: 1 | Warnungen: 0 | Hinweise: 0",
            HintText: message,
            Rows: new[]
            {
                new InterfaceProfileActivationPreviewRow(
                    Severity: "BLOCKER",
                    Area: "Aktivierungsprüfung",
                    Message: "Bewertung konnte nicht ausgeführt werden.",
                    Detail: message)
            });
    }

    public InterfaceProfileActivationPreviewDisplay Create(InterfaceProfileActivationEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rows = result.Checks
            .OrderBy(GetSeverityOrder)
            .ThenBy(check => check.Area, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(check => check.Message, StringComparer.CurrentCultureIgnoreCase)
            .Select(check => new InterfaceProfileActivationPreviewRow(
                Severity: FormatSeverity(check.Severity),
                Area: check.Area,
                Message: check.Message,
                Detail: check.Detail ?? string.Empty))
            .ToList();

        var blockerCount = result.Blockers.Count;
        var warningCount = result.Warnings.Count;
        var infoCount = result.Infos.Count;

        return new InterfaceProfileActivationPreviewDisplay(
            StatusText: $"Status: {FormatStatus(result.ActivationStatus)}",
            CanActivateText: $"Aktivierbar: {(result.CanActivate ? "Ja" : "Nein")}",
            BlockerCount: blockerCount,
            WarningCount: warningCount,
            InfoCount: infoCount,
            SummaryText: $"Blocker: {blockerCount} | Warnungen: {warningCount} | Hinweise: {infoCount}",
            HintText: "Diese Prüfung ist nur eine Vorschau. Es wird nichts aktiviert, gespeichert oder verarbeitet.",
            Rows: rows);
    }

    private static int GetSeverityOrder(InterfaceProfileActivationCheckResult check)
    {
        return check.Severity switch
        {
            InterfaceProfileActivationSeverity.Blocker => 0,
            InterfaceProfileActivationSeverity.Warning => 1,
            InterfaceProfileActivationSeverity.Info => 2,
            _ => 3
        };
    }

    private static string FormatSeverity(InterfaceProfileActivationSeverity severity)
    {
        return severity switch
        {
            InterfaceProfileActivationSeverity.Blocker => "BLOCKER",
            InterfaceProfileActivationSeverity.Warning => "WARNUNG",
            InterfaceProfileActivationSeverity.Info => "INFO",
            _ => severity.ToString().ToUpperInvariant()
        };
    }

    private static string FormatStatus(InterfaceProfileActivationStatus status)
    {
        return status switch
        {
            InterfaceProfileActivationStatus.Ready => "Bereit",
            InterfaceProfileActivationStatus.ReadyWithWarnings => "Bereit mit Warnungen",
            InterfaceProfileActivationStatus.Blocked => "Blockiert",
            InterfaceProfileActivationStatus.NotEvaluated => "Nicht bewertet",
            InterfaceProfileActivationStatus.Unknown => "Unklar",
            _ => status.ToString()
        };
    }
}
