using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileActivationPreparationPreviewService
{
    private const int MaxImportantItems = 5;
    private const string SafetyNotice = "Dies ist nur eine Vorschau. Es wurden keine Warnungen bestätigt, keine Änderungen gespeichert und nichts aktiviert.";
    private const string GuardUnknownDecisionText = "Nicht eindeutig";
    private const string GuardNoCanProceedText = "Nein";
    private const string GuardUnknownMessage = "Es liegt keine eindeutige aktuelle Aktivierungsbewertung vor.";
    private const string WarningConfirmationNotAvailableText = "nicht verfügbar";
    private const string WarningConfirmationNotAvailableMessage = "Es liegt keine eindeutige Aktivierungsbewertung vor.";
    private const string ActivationPlanNotAvailableText = "nicht verfügbar";
    private const string ActivationPlanNoCanExecuteLaterText = "Nein";
    private const string ActivationPlanNotAvailableMessage = "Es liegt keine ausreichende Grundlage für einen Aktivierungsplan vor.";

    public InterfaceProfileActivationPreparationPreview CreateEmpty()
    {
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
            WarningConfirmationStatusText: WarningConfirmationNotAvailableText,
            WarningConfirmationMessage: "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.",
            WarningConfirmationItemCount: 0,
            WarningConfirmationItems: Array.Empty<string>(),
            ActivationPlanStatusText: ActivationPlanNotAvailableText,
            ActivationPlanCanExecuteLaterText: ActivationPlanNoCanExecuteLaterText,
            ActivationPlanMessage: "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.",
            ActivationPlanMissingRequirements: Array.Empty<string>(),
            ActivationPlanReasons: Array.Empty<string>(),
            ActivationPlanSteps: Array.Empty<string>(),
            SummaryMessage: summary,
            SafetyNotice: SafetyNotice,
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
                warningConfirmationStatusText: WarningConfirmationNotAvailableText,
                warningConfirmationMessage: "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.",
                warningConfirmationItemCount: 0,
                warningConfirmationItems: Array.Empty<string>(),
                activationPlanStatusText: ActivationPlanNotAvailableText,
                activationPlanCanExecuteLaterText: ActivationPlanNoCanExecuteLaterText,
                activationPlanMessage: "Bitte wählen Sie zuerst ein Schnittstellenprofil aus.",
                activationPlanMissingRequirements: Array.Empty<string>(),
                activationPlanReasons: Array.Empty<string>(),
                activationPlanSteps: Array.Empty<string>(),
                summary,
                SafetyNotice));
    }

    public InterfaceProfileActivationPreparationPreview CreateError(string message)
    {
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
            WarningConfirmationStatusText: WarningConfirmationNotAvailableText,
            WarningConfirmationMessage: "Die Warnungsbestätigungsvorschau konnte nicht eindeutig abgeschlossen werden.",
            WarningConfirmationItemCount: 0,
            WarningConfirmationItems: Array.Empty<string>(),
            ActivationPlanStatusText: ActivationPlanNotAvailableText,
            ActivationPlanCanExecuteLaterText: ActivationPlanNoCanExecuteLaterText,
            ActivationPlanMessage: "Die Aktivierungsplan-Vorschau konnte nicht eindeutig abgeschlossen werden.",
            ActivationPlanMissingRequirements: blockers,
            ActivationPlanReasons: blockers,
            ActivationPlanSteps: Array.Empty<string>(),
            SummaryMessage: summary,
            SafetyNotice: SafetyNotice,
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
                warningConfirmationStatusText: WarningConfirmationNotAvailableText,
                warningConfirmationMessage: "Die Warnungsbestätigungsvorschau konnte nicht eindeutig abgeschlossen werden.",
                warningConfirmationItemCount: 0,
                warningConfirmationItems: Array.Empty<string>(),
                activationPlanStatusText: ActivationPlanNotAvailableText,
                activationPlanCanExecuteLaterText: ActivationPlanNoCanExecuteLaterText,
                activationPlanMessage: "Die Aktivierungsplan-Vorschau konnte nicht eindeutig abgeschlossen werden.",
                activationPlanMissingRequirements: blockers,
                activationPlanReasons: blockers,
                activationPlanSteps: Array.Empty<string>(),
                summary,
                SafetyNotice));
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result)
    {
        return Create(profile, result, guardResult: null, warningConfirmationResult: null, activationPlan: null);
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result,
        InterfaceProfileActivationGuardResult? guardResult)
    {
        return Create(profile, result, guardResult, warningConfirmationResult: null, activationPlan: null);
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result,
        InterfaceProfileActivationGuardResult? guardResult,
        InterfaceProfileActivationWarningConfirmationResult? warningConfirmationResult)
    {
        return Create(profile, result, guardResult, warningConfirmationResult, activationPlan: null);
    }

    public InterfaceProfileActivationPreparationPreview Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult? result,
        InterfaceProfileActivationGuardResult? guardResult,
        InterfaceProfileActivationWarningConfirmationResult? warningConfirmationResult,
        InterfaceProfileActivationPlan? activationPlan)
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
        var warningConfirmationStatusText = warningConfirmationResult is null
            ? WarningConfirmationNotAvailableText
            : FormatWarningConfirmationStatus(warningConfirmationResult.Status);
        var warningConfirmationMessage = string.IsNullOrWhiteSpace(warningConfirmationResult?.Message)
            ? WarningConfirmationNotAvailableMessage
            : warningConfirmationResult.Message;
        var warningConfirmationItems = warningConfirmationResult is null
            ? Array.Empty<string>()
            : BuildWarningConfirmationItems(warningConfirmationResult);
        var warningConfirmationItemCount = warningConfirmationResult?.Warnings.Count ?? 0;
        var activationPlanStatusText = activationPlan is null
            ? ActivationPlanNotAvailableText
            : FormatActivationPlanStatus(activationPlan.PlanStatus);
        var activationPlanCanExecuteLaterText = activationPlan?.CanExecuteLater == true ? "Ja" : "Nein";
        var activationPlanMessage = string.IsNullOrWhiteSpace(activationPlan?.SummaryMessage)
            ? ActivationPlanNotAvailableMessage
            : activationPlan.SummaryMessage;
        var isWarningConfirmationRequired =
            guardResult?.Decision == InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation
            || warningConfirmationResult?.Status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired
            || activationPlan?.PlanStatus == InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation;
        var hasConfirmationWarnings = warningConfirmationItems.Count > 0;
        var shouldCentralizeWarningConfirmation = isWarningConfirmationRequired && hasConfirmationWarnings;
        var activationPlanMissingRequirements = activationPlan is null || shouldCentralizeWarningConfirmation
            ? Array.Empty<string>()
            : BuildActivationPlanReasons(activationPlan.MissingRequirements);
        var activationPlanReasons = activationPlan is null || shouldCentralizeWarningConfirmation
            ? Array.Empty<string>()
            : BuildActivationPlanReasons(activationPlan.Blockers.Concat(activationPlan.Warnings).ToList());
        var activationPlanSteps = activationPlan is null
            ? Array.Empty<string>()
            : BuildActivationPlanSteps(activationPlan, compactForWarningConfirmation: shouldCentralizeWarningConfirmation);
        var summary = BuildSummaryMessage(result);
        var displayGuardMessage = shouldCentralizeWarningConfirmation
            ? string.Empty
            : guardMessage;
        var displayActivationPlanMessage = shouldCentralizeWarningConfirmation
            ? "Siehe Abschnitt Warnungsbestätigung."
            : activationPlanMessage;

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
            GuardMessage: displayGuardMessage,
            GuardReasons: guardReasons,
            WarningConfirmationStatusText: warningConfirmationStatusText,
            WarningConfirmationMessage: warningConfirmationMessage,
            WarningConfirmationItemCount: warningConfirmationItemCount,
            WarningConfirmationItems: warningConfirmationItems,
            ActivationPlanStatusText: activationPlanStatusText,
            ActivationPlanCanExecuteLaterText: activationPlanCanExecuteLaterText,
            ActivationPlanMessage: displayActivationPlanMessage,
            ActivationPlanMissingRequirements: activationPlanMissingRequirements,
            ActivationPlanReasons: activationPlanReasons,
            ActivationPlanSteps: activationPlanSteps,
            SummaryMessage: summary,
            SafetyNotice: SafetyNotice,
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
                displayGuardMessage,
                guardReasons,
                warningConfirmationStatusText,
                warningConfirmationMessage,
                warningConfirmationItemCount,
                warningConfirmationItems,
                activationPlanStatusText,
                activationPlanCanExecuteLaterText,
                displayActivationPlanMessage,
                activationPlanMissingRequirements,
                activationPlanReasons,
                activationPlanSteps,
                summary,
                SafetyNotice));
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

    private static string FormatWarningConfirmationStatus(
        InterfaceProfileActivationWarningConfirmationStatus status)
    {
        return status switch
        {
            InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired => "erforderlich",
            InterfaceProfileActivationWarningConfirmationStatus.NoWarnings => "nicht erforderlich",
            InterfaceProfileActivationWarningConfirmationStatus.Blocked => "nicht möglich",
            InterfaceProfileActivationWarningConfirmationStatus.MissingEvaluation => WarningConfirmationNotAvailableText,
            InterfaceProfileActivationWarningConfirmationStatus.NotAvailable => WarningConfirmationNotAvailableText,
            InterfaceProfileActivationWarningConfirmationStatus.Unknown => WarningConfirmationNotAvailableText,
            _ => status.ToString()
        };
    }

    private static IReadOnlyList<string> BuildWarningConfirmationItems(
        InterfaceProfileActivationWarningConfirmationResult result)
    {
        return result.Warnings
            .Take(MaxImportantItems)
            .Select(FormatWarningConfirmationItem)
            .ToList();
    }

    private static string FormatWarningConfirmationItem(
        InterfaceProfileActivationWarningConfirmationItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Detail))
        {
            return item.Title;
        }

        return $"{item.Title} {item.Detail}";
    }

    private static string FormatActivationPlanStatus(InterfaceProfileActivationPlanStatus status)
    {
        return status switch
        {
            InterfaceProfileActivationPlanStatus.NotAvailable => ActivationPlanNotAvailableText,
            InterfaceProfileActivationPlanStatus.Blocked => "blockiert",
            InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation => "Warnungsbestätigung erforderlich",
            InterfaceProfileActivationPlanStatus.Ready => "vorbereitet",
            InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings => "vorbereitet nach Warnungsbestätigung",
            InterfaceProfileActivationPlanStatus.Unknown => "nicht eindeutig",
            _ => status.ToString()
        };
    }

    private static IReadOnlyList<string> BuildActivationPlanReasons(
        IReadOnlyList<InterfaceProfileActivationPlanReason> reasons)
    {
        return reasons
            .Take(MaxImportantItems)
            .Select(FormatActivationPlanReason)
            .ToList();
    }

    private static string FormatActivationPlanReason(InterfaceProfileActivationPlanReason reason)
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

    private static IReadOnlyList<string> BuildActivationPlanSteps(
        InterfaceProfileActivationPlan activationPlan,
        bool compactForWarningConfirmation)
    {
        if (compactForWarningConfirmation)
        {
            return activationPlan.PlannedSteps
                .Take(MaxImportantItems)
                .Select(FormatActivationPlanStepCompact)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return activationPlan.PlannedSteps
            .Take(MaxImportantItems)
            .Select(FormatActivationPlanStep)
            .ToList();
    }

    private static string FormatActivationPlanStepCompact(InterfaceProfileActivationPlanStep step)
    {
        if (step.Code.Contains("warning", StringComparison.OrdinalIgnoreCase)
            || step.Code.Contains("warnung", StringComparison.OrdinalIgnoreCase))
        {
            return "Warnungen prüfen und bestätigen";
        }

        if (step.Code.Contains("activate", StringComparison.OrdinalIgnoreCase)
            || step.Code.Contains("aktiv", StringComparison.OrdinalIgnoreCase))
        {
            return "Profil aktivieren";
        }

        if (step.Code.Contains("save", StringComparison.OrdinalIgnoreCase)
            || step.Code.Contains("speicher", StringComparison.OrdinalIgnoreCase))
        {
            return "Profiländerung speichern";
        }

        return step.Title;
    }

    private static string FormatActivationPlanStep(InterfaceProfileActivationPlanStep step)
    {
        var blockedSuffix = step.IsBlocked ? " (blockiert)" : string.Empty;
        return $"{step.Title}{blockedSuffix}: {step.Description}";
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
        string warningConfirmationStatusText,
        string warningConfirmationMessage,
        int warningConfirmationItemCount,
        IReadOnlyList<string> warningConfirmationItems,
        string activationPlanStatusText,
        string activationPlanCanExecuteLaterText,
        string activationPlanMessage,
        IReadOnlyList<string> activationPlanMissingRequirements,
        IReadOnlyList<string> activationPlanReasons,
        IReadOnlyList<string> activationPlanSteps,
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
        builder.AppendLine("Technische Schutzprüfung:");
        builder.AppendLine($"Entscheidung: {guardDecisionText}");
        builder.AppendLine($"Technisch freigegeben: {guardCanProceedText}");

        if (!string.IsNullOrWhiteSpace(guardMessage))
        {
            builder.AppendLine($"Hinweis: {guardMessage}");
        }

        if (guardReasons.Count > 0 && warningConfirmationItems.Count == 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Guard-Gründe:");
            AppendNumberedItems(builder, guardReasons);
        }

        builder.AppendLine();
        builder.AppendLine("Warnungsbestätigung:");
        builder.AppendLine($"Status: {warningConfirmationStatusText}");
        builder.AppendLine($"Bestätigungspflichtige Warnungen: {warningConfirmationItemCount}");
        builder.AppendLine($"Hinweis: {warningConfirmationMessage}");

        if (warningConfirmationItems.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine(warningConfirmationStatusText == "erforderlich"
                ? "Folgende Warnungen müssten vor einer späteren Aktivierung bewusst bestätigt werden:"
                : "Vorhandene Warnungen (noch nicht bestätigbar):");
            AppendNumberedItems(builder, warningConfirmationItems);
            builder.AppendLine("Diese Warnungen wurden in diesem Schritt nicht bestätigt.");
        }

        builder.AppendLine();
        builder.AppendLine("Aktivierungsplan:");
        builder.AppendLine($"Status: {activationPlanStatusText}");
        builder.AppendLine($"Spätere Aktivierung laut Plan möglich: {activationPlanCanExecuteLaterText}");
        builder.AppendLine($"Hinweis: {activationPlanMessage}");

        if (activationPlanMissingRequirements.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Fehlende Voraussetzungen:");
            AppendNumberedItems(builder, activationPlanMissingRequirements);
        }

        if (activationPlanReasons.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Plan-Gründe:");
            AppendNumberedItems(builder, activationPlanReasons);
        }

        if (activationPlanSteps.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Geplante spätere Schritte:");
            AppendNumberedItems(builder, activationPlanSteps);
            builder.AppendLine("Diese Schritte wurden in diesem Schritt nicht ausgeführt.");
        }

        if (importantBlockers.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Wichtigste Blocker:");
            AppendNumberedItems(builder, importantBlockers);
        }

        if (importantWarnings.Count > 0 && warningConfirmationItems.Count == 0)
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
