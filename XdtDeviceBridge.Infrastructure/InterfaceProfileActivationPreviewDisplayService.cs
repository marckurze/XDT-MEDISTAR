using XdtDeviceBridge.Core;

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
            FolderChecks: Array.Empty<InterfaceProfileActivationFolderDisplay>(),
            AttachmentChecks: Array.Empty<InterfaceProfileActivationAttachmentDisplay>(),
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
            FolderChecks: Array.Empty<InterfaceProfileActivationFolderDisplay>(),
            AttachmentChecks: Array.Empty<InterfaceProfileActivationAttachmentDisplay>(),
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
        return Create(profile: null, result);
    }

    public InterfaceProfileActivationPreviewDisplay Create(
        InterfaceProfileDefinition? profile,
        InterfaceProfileActivationEvaluationResult result)
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
            FolderChecks: profile is null ? Array.Empty<InterfaceProfileActivationFolderDisplay>() : BuildFolderChecks(profile, result),
            AttachmentChecks: profile is null ? Array.Empty<InterfaceProfileActivationAttachmentDisplay>() : BuildAttachmentChecks(profile, result),
            Rows: rows);
    }

    private static IReadOnlyList<InterfaceProfileActivationFolderDisplay> BuildFolderChecks(
        InterfaceProfileDefinition profile,
        InterfaceProfileActivationEvaluationResult result)
    {
        var options = profile.FolderOptions;

        var isManualDocumentSelection = options.IsAttachmentOnlyMode
            && options.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var rows = new List<InterfaceProfileActivationFolderDisplay>
        {
            CreateFolderDisplay(result, "AIS-Importordner", options.AisImportFolder, "folder.aisImport"),
            CreateFolderDisplay(result, "AIS-Exportordner", options.ExportFolder, "folder.export"),
            CreateFolderDisplay(result, "Archivordner", options.ArchiveFolder, "folder.archive"),
            CreateFolderDisplay(result, "Fehlerordner", options.ErrorFolder, "folder.error"),
            CreateFolderDisplay(result, "XDT-Anhang-Exportordner", options.AttachmentExportFolder, "attachment.folder.export")
        };

        if (isManualDocumentSelection)
        {
            rows.Insert(1, new InterfaceProfileActivationFolderDisplay(
                Label: "Dokumentauswahl",
                Path: "manuell im Fenster",
                Status: "OK",
                Reachability: "Nicht geprüft",
                Severity: "INFO",
                Message: "Kein Geräte-Importordner erforderlich."));
        }
        else
        {
            rows.Insert(1, CreateFolderDisplay(
                result,
                options.IsAttachmentOnlyMode ? "Dokument-Importordner" : "Geräte-Importordner",
                options.DeviceImportFolder,
                "folder.deviceImport"));
        }

        if (!options.IsAttachmentOnlyMode)
        {
            rows.Insert(5, CreateFolderDisplay(result, "XDT-Anhang-Importordner", options.AttachmentImportFolder, "attachment.folder.import"));
        }

        return rows;
    }

    private static IReadOnlyList<InterfaceProfileActivationAttachmentDisplay> BuildAttachmentChecks(
        InterfaceProfileDefinition profile,
        InterfaceProfileActivationEvaluationResult result)
    {
        var options = profile.FolderOptions;

        var isManualDocumentSelection = options.IsAttachmentOnlyMode
            && options.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var rows = new List<InterfaceProfileActivationAttachmentDisplay>
        {
            CreateAttachmentDisplay(
                result,
                isManualDocumentSelection ? "Manuelle Dokumentübergabe" : options.IsAttachmentOnlyMode ? "Dokumentdateien als AIS-Anhänge" : "Anhangverarbeitung",
                isManualDocumentSelection
                    ? "Anwender wählt Dateien im Fenster"
                    : options.IsAttachmentOnlyMode
                    ? "aktiv über Dokument-Importordner"
                    : options.IsAttachmentProcessingEnabled ? "aktiv" : "inaktiv",
                "attachment.disabled"),
            CreateAttachmentDisplay(
                result,
                options.IsAttachmentOnlyMode ? "Dokumentdateien erforderlich" : "Modus",
                options.IsAttachmentOnlyMode
                    ? "Ja"
                    : options.AttachmentRequirementMode == AttachmentRequirementMode.Required ? "Pflicht" : "Optional",
                "attachment.requirementMode"),
            CreateAttachmentDisplay(result, "XDT-Anhang-Exportordner", options.AttachmentExportFolder, "attachment.folder.export"),
            CreateAttachmentDisplay(result, "Dateiname-Template", options.AttachmentFileNameTemplate ?? string.Empty, "attachment.filenameTemplate"),
            CreateAttachmentDisplay(result, "Transfermodus", options.AttachmentTransferMode.ToString(), "attachment.transferMode"),
            CreateAttachmentDisplay(result, "Wartezeit", $"{options.AttachmentWaitTimeoutSeconds} Sekunden", "attachment.waitTime"),
            CreateAttachmentDisplay(result, "Dateistabilität", $"{options.AttachmentFileStabilityWaitSeconds} Sekunden", "attachment.stabilityWait"),
            CreateAttachmentDisplay(result, "6302 Dokumentenname", options.AttachmentExternalLinkDocumentName, "attachment.6302"),
            CreateAttachmentDisplay(result, "6303 Dateiformat", options.AttachmentExternalLinkFileFormat, "attachment.6303"),
            CreateAttachmentDisplay(result, "6304 Beschreibung", options.AttachmentExternalLinkDescription, "attachment.6304"),
            CreateAttachmentDisplay(result, "6305 vollständiger Dateipfad", options.AttachmentExternalLinkPathTemplate, "attachment.6305")
        };

        if (isManualDocumentSelection)
        {
            rows.Insert(2, CreateAttachmentDisplay(result, "Dokumenteingang", "manuelle Dateiauswahl", "attachment.folder.import"));
            rows.Insert(3, CreateAttachmentDisplay(result, "Abschluss", "Übertragen im Dialog", "attachmentOnly.manualUserSelection"));
        }
        else if (options.IsAttachmentOnlyMode)
        {
            rows.Insert(2, CreateAttachmentDisplay(result, "Dokumenteingang", options.DeviceImportFolder, "attachment.folder.import"));
            rows.Insert(
                3,
                CreateAttachmentDisplay(
                    result,
                    "Dokumentgeräte-Abschluss",
                    options.AttachmentCompletionMode == AttachmentCompletionMode.ManualConfirmation
                        ? "Manuell bestätigen"
                        : $"Abschluss nach Wartezeit ({options.AttachmentQuietPeriodSeconds} Sekunden)",
                    "attachmentOnly."));
        }
        else
        {
            rows.Insert(2, CreateAttachmentDisplay(result, "XDT-Anhang-Importordner", options.AttachmentImportFolder, "attachment.folder.import"));
        }

        return rows;
    }

    private static InterfaceProfileActivationFolderDisplay CreateFolderDisplay(
        InterfaceProfileActivationEvaluationResult result,
        string label,
        string path,
        string codePrefix)
    {
        var check = FindCheck(result, codePrefix);
        return new InterfaceProfileActivationFolderDisplay(
            Label: label,
            Path: DisplayValue(path),
            Status: FormatCheckStatus(check),
            Reachability: FormatReachability(check, path),
            Severity: check is null ? "INFO" : FormatSeverity(check.Severity),
            Message: FormatCheckMessage(check));
    }

    private static InterfaceProfileActivationAttachmentDisplay CreateAttachmentDisplay(
        InterfaceProfileActivationEvaluationResult result,
        string label,
        string value,
        string codePrefix)
    {
        var check = FindCheck(result, codePrefix);
        return new InterfaceProfileActivationAttachmentDisplay(
            Label: label,
            Value: DisplayValue(value),
            Status: FormatCheckStatus(check),
            Severity: check is null ? "INFO" : FormatSeverity(check.Severity),
            Message: FormatCheckMessage(check));
    }

    private static InterfaceProfileActivationCheckResult? FindCheck(
        InterfaceProfileActivationEvaluationResult result,
        string codePrefix)
    {
        return result.Checks
            .Where(check => check.Code.StartsWith(codePrefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(GetSeverityOrder)
            .FirstOrDefault();
    }

    private static string FormatCheckStatus(InterfaceProfileActivationCheckResult? check)
    {
        if (check is null)
        {
            return "OK";
        }

        if (check.Severity == InterfaceProfileActivationSeverity.Blocker)
        {
            if (check.Code.EndsWith(".missing", StringComparison.OrdinalIgnoreCase))
            {
                return "fehlt";
            }

            if (check.Code.EndsWith(".notFound", StringComparison.OrdinalIgnoreCase))
            {
                return "nicht vorhanden";
            }

            return "Blockiert";
        }

        if (check.Severity == InterfaceProfileActivationSeverity.Warning)
        {
            return "Warnung";
        }

        return check.Code.EndsWith(".ok", StringComparison.OrdinalIgnoreCase) ? "OK" : "Hinweis";
    }

    private static string FormatCheckMessage(InterfaceProfileActivationCheckResult? check)
    {
        if (check is null)
        {
            return "Keine Auffälligkeit aus der Bewertung.";
        }

        return string.IsNullOrWhiteSpace(check.Detail)
            ? check.Message
            : $"{check.Message} {check.Detail}";
    }

    private static string FormatReachability(InterfaceProfileActivationCheckResult? check, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "Pfad fehlt";
        }

        if (check is null)
        {
            return "Nicht geprüft";
        }

        if (check.Code.EndsWith(".ok", StringComparison.OrdinalIgnoreCase))
        {
            return "Ja";
        }

        if (check.Code.EndsWith(".notFound", StringComparison.OrdinalIgnoreCase)
            || check.Code.EndsWith(".accessDenied", StringComparison.OrdinalIgnoreCase))
        {
            return "Nein";
        }

        if (check.Code.EndsWith(".missing", StringComparison.OrdinalIgnoreCase)
            || check.Code.EndsWith(".optionalMissing", StringComparison.OrdinalIgnoreCase))
        {
            return "Pfad fehlt";
        }

        return "Nicht geprüft";
    }

    private static string DisplayValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
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
