using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportPreviewDisplayService
{
    public TemplatePackageImportPreviewDisplay Create(
        TemplatePackageImportValidationResult validationResult,
        TemplatePackageImportAnalysisResult analysisResult,
        TemplatePackageImportPlan plan,
        TemplatePackageImportDryRunResult dryRunResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);
        ArgumentNullException.ThrowIfNull(analysisResult);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(dryRunResult);

        var profilePlans = (plan.ProfilePlans ?? Array.Empty<TemplatePackageImportProfilePlan>())
            .ToDictionary(
                profilePlan => CreateProfileKey(profilePlan.ProfileKind, profilePlan.ImportedProfileId),
                StringComparer.OrdinalIgnoreCase);
        var rows = (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .Select(item => CreateRow(item, profilePlans))
            .ToList();
        var dependencyRows = (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .SelectMany(CreateDependencyRows)
            .ToList();
        var warningMessages = CreateWarningMessages(validationResult, analysisResult, plan, dryRunResult)
            .Select(LocalizeUserMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var dependencyEmptyStateMessage = CreateDependencyEmptyStateMessage(dryRunResult, dependencyRows);
        var messages = CreateMessages(validationResult, analysisResult, dryRunResult, warningMessages.Count).ToList();
        var summary = CreateSummary(analysisResult, dryRunResult, warningMessages.Count);

        return new TemplatePackageImportPreviewDisplay(
            Summary: summary,
            Rows: rows,
            DependencyRows: dependencyRows,
            DependencyEmptyStateMessage: dependencyEmptyStateMessage,
            Messages: messages,
            Warnings: warningMessages);
    }

    private static TemplatePackageImportPreviewSummary CreateSummary(
        TemplatePackageImportAnalysisResult analysisResult,
        TemplatePackageImportDryRunResult dryRunResult,
        int warningCount)
    {
        var packageId = DisplayOrDash(dryRunResult.PackageId ?? analysisResult.PackageId);
        var packageName = DisplayOrDash(dryRunResult.PackageName ?? analysisResult.PackageName);
        var keepOrSkip = dryRunResult.WouldSkip;
        var summaryText =
            $"Paket: {packageName} ({packageId}) | Profile gesamt: {dryRunResult.TotalItems} | importierbar: {analysisResult.ImportableProfiles} | als Kopie geplant: {dryRunResult.WouldImportAsCopy} | beibehalten/überspringen: {keepOrSkip} | blockiert: {dryRunResult.WouldBlock} | Warnungen: {warningCount} | blockierende Konflikte: {(dryRunResult.WouldBlock > 0 ? "Ja" : "Nein")}";

        return new TemplatePackageImportPreviewSummary(
            PackageId: packageId,
            PackageName: packageName,
            TotalProfiles: dryRunResult.TotalItems,
            ImportableProfiles: analysisResult.ImportableProfiles,
            PlannedImportAsCopy: dryRunResult.WouldImportAsCopy,
            PlannedKeepOrSkip: keepOrSkip,
            BlockedProfiles: dryRunResult.WouldBlock,
            WarningCount: warningCount,
            HasBlockingItems: dryRunResult.WouldBlock > 0,
            SummaryText: summaryText);
    }

    private static TemplatePackageImportPreviewRow CreateRow(
        TemplatePackageImportDryRunItem item,
        IReadOnlyDictionary<string, TemplatePackageImportProfilePlan> profilePlans)
    {
        profilePlans.TryGetValue(CreateProfileKey(item.ProfileKind, item.ImportedProfileId), out var plan);
        var conflict = plan?.ConflictType ?? TemplatePackageImportConflictType.None;
        var status = CreateStatus(item, conflict, plan);
        var availableActions = CreateAvailableActions(item, plan, conflict);
        var selectedAction = item.IsBlocking ? TemplatePackageImportAction.Blocked : item.PlannedAction;

        return new TemplatePackageImportPreviewRow(
            profileKindValue: item.ProfileKind,
            profileKind: FormatProfileKind(item.ProfileKind),
            importedProfileName: DisplayOrDash(item.ImportedProfileName),
            importedProfileId: DisplayOrDash(item.ImportedProfileId),
            selectedAction: selectedAction,
            availableActions: availableActions,
            isActionSelectionEnabled: !item.IsBlocking && availableActions.Count > 1,
            plannedAction: FormatAction(item.PlannedAction, plan?.ExistingProfileSource),
            targetProfileName: DisplayTargetName(item.TargetProfileName, item.PlannedAction),
            targetProfileId: DisplayOrDash(item.TargetProfileId),
            isTargetNameEditable: !item.IsBlocking && item.PlannedAction == TemplatePackageImportAction.ImportAsCopy,
            conflict: FormatConflict(conflict, plan?.ExistingProfileSource),
            status: status,
            message: LocalizeUserMessage(item.Message));
    }

    private static IReadOnlyList<TemplatePackageImportPreviewActionOption> CreateAvailableActions(
        TemplatePackageImportDryRunItem item,
        TemplatePackageImportProfilePlan? plan,
        TemplatePackageImportConflictType conflict)
    {
        if (item.IsBlocking || item.PlannedAction == TemplatePackageImportAction.Blocked)
        {
            return new[]
            {
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.Blocked, "Blockiert")
            };
        }

        if (conflict == TemplatePackageImportConflictType.None)
        {
            return new[]
            {
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.ImportAsNew, "Neu importieren"),
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.Skip, "Überspringen")
            };
        }

        if (plan?.ExistingProfileSource == TemplatePackageImportExistingProfileSource.UserDefined)
        {
            return new[]
            {
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.ImportAsCopy, "Als Kopie importieren"),
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.KeepExisting, "Bestehendes behalten"),
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.Skip, "Überspringen")
            };
        }

        if (plan?.ExistingProfileSource == TemplatePackageImportExistingProfileSource.None)
        {
            return new[]
            {
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.ImportAsNew, "Neu importieren"),
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.ImportAsCopy, "Als Kopie importieren"),
                new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.Skip, "Überspringen")
            };
        }

        return new[]
        {
            new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.ImportAsCopy, "Als Kopie importieren"),
            new TemplatePackageImportPreviewActionOption(TemplatePackageImportAction.Skip, "Überspringen")
        };
    }

    private static IEnumerable<TemplatePackageImportDependencyPreviewRow> CreateDependencyRows(
        TemplatePackageImportDryRunItem item)
    {
        if (item.ProfileKind != ProfileKind.InterfaceProfile)
        {
            yield break;
        }

        foreach (var remap in item.DependencyRemaps ?? Array.Empty<TemplatePackageImportDependencyRemap>())
        {
            yield return new TemplatePackageImportDependencyPreviewRow(
                InterfaceProfileName: DisplayOrDash(item.ImportedProfileName),
                DependencyKind: FormatDependencyKind(remap.DependencyKind),
                OriginalProfileName: DisplayOrDash(remap.OriginalProfileName),
                OriginalProfileId: DisplayOrDash(remap.OriginalProfileId),
                TargetProfileName: DisplayDependencyTarget(remap.TargetProfileName, remap.Resolution),
                TargetProfileId: DisplayDependencyTarget(remap.TargetProfileId, remap.Resolution),
                Resolution: FormatResolution(remap.Resolution),
                Status: remap.Resolution is TemplatePackageImportDependencyResolution.Missing
                    or TemplatePackageImportDependencyResolution.Blocked
                        ? "Blockiert"
                        : remap.Resolution == TemplatePackageImportDependencyResolution.ImportedAsCopy
                            ? "Warnung"
                            : "OK",
                Message: LocalizeUserMessage(remap.Message));
        }
    }

    private static IEnumerable<string> CreateWarningMessages(
        TemplatePackageImportValidationResult validationResult,
        TemplatePackageImportAnalysisResult analysisResult,
        TemplatePackageImportPlan plan,
        TemplatePackageImportDryRunResult dryRunResult)
    {
        foreach (var issue in validationResult.Issues ?? Array.Empty<TemplatePackageImportValidationIssue>())
        {
            yield return $"{issue.Severity}: {issue.Message}";
        }

        foreach (var warning in analysisResult.Warnings ?? Array.Empty<string>())
        {
            yield return warning;
        }

        foreach (var warning in plan.Warnings ?? Array.Empty<string>())
        {
            yield return warning;
        }

        foreach (var warning in dryRunResult.Warnings ?? Array.Empty<string>())
        {
            yield return warning;
        }

        foreach (var warning in (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .SelectMany(item => item.DependencyRemapWarnings ?? Array.Empty<string>()))
        {
            yield return warning;
        }
    }

    private static IEnumerable<string> CreateMessages(
        TemplatePackageImportValidationResult validationResult,
        TemplatePackageImportAnalysisResult analysisResult,
        TemplatePackageImportDryRunResult dryRunResult,
        int warningCount)
    {
        var validationErrors = validationResult.Issues.Count(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error);
        var validationWarnings = validationResult.Issues.Count(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Warning);

        yield return "Templatepaket wurde analysiert.";
        yield return $"Validierung: {validationErrors} Fehler, {validationWarnings} Warnungen.";
        yield return dryRunResult.WouldBlock == 0
            ? "Keine blockierenden Konflikte gefunden."
            : $"{dryRunResult.WouldBlock} Profil(e) sind blockiert.";

        if (dryRunResult.WouldImportAsCopy > 0)
        {
            yield return $"{dryRunResult.WouldImportAsCopy} Profil(e) würden als Kopie importiert.";
        }

        if (dryRunResult.WouldSkip > 0)
        {
            yield return $"{dryRunResult.WouldSkip} Profil(e) bleiben vorerst übersprungen. Bei Konflikten ist das der sichere Standard; Kopien müssen bewusst ausgewählt werden.";
        }

        if (analysisResult.ConflictingProfiles > 0)
        {
            yield return $"{analysisResult.ConflictingProfiles} Profilkonflikt(e) erkannt.";
        }

        if (warningCount > 0)
        {
            yield return $"{warningCount} Hinweis(e)/Warnung(en) sichtbar.";
        }

        yield return "Es wurde nichts gespeichert. Dies ist nur eine Vorschau.";
    }

    private static string CreateDependencyEmptyStateMessage(
        TemplatePackageImportDryRunResult dryRunResult,
        IReadOnlyList<TemplatePackageImportDependencyPreviewRow> dependencyRows)
    {
        if (dependencyRows.Count > 0)
        {
            return "";
        }

        var interfaceItems = (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .Where(item => item.ProfileKind == ProfileKind.InterfaceProfile)
            .ToList();

        if (interfaceItems.Count == 0)
        {
            return "Für dieses Paket sind keine Schnittstellenprofil-Abhängigkeiten anzuzeigen.";
        }

        if (interfaceItems.All(item => item.PlannedAction is TemplatePackageImportAction.Skip or TemplatePackageImportAction.KeepExisting))
        {
            return "Alle Schnittstellenprofile werden aktuell übersprungen. Daher ist keine Abhängigkeitsauflösung erforderlich.";
        }

        return "Für die aktuelle Auswahl sind keine Abhängigkeiten anzuzeigen.";
    }

    private static string CreateStatus(
        TemplatePackageImportDryRunItem item,
        TemplatePackageImportConflictType conflict,
        TemplatePackageImportProfilePlan? plan)
    {
        if (item.IsBlocking || item.PlannedAction == TemplatePackageImportAction.Blocked)
        {
            return "Blockiert";
        }

        if (conflict != TemplatePackageImportConflictType.None
            || item.RequiresDependencyRemap
            || (item.DependencyRemapWarnings?.Count ?? 0) > 0
            || (plan?.RequiresUserDecision ?? false))
        {
            return "Warnung";
        }

        return "OK";
    }

    private static string FormatProfileKind(ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.AisProfile => "AIS-Profil",
            ProfileKind.DeviceProfile => "Geräteprofil",
            ProfileKind.ExportProfile => "Exportprofil",
            ProfileKind.InterfaceProfile => "Schnittstellenprofil",
            ProfileKind.TemplatePackage => "Templatepaket",
            _ => profileKind.ToString()
        };
    }

    private static string FormatAction(
        TemplatePackageImportAction action,
        TemplatePackageImportExistingProfileSource? existingSource)
    {
        if (action == TemplatePackageImportAction.ReplaceExisting
            && existingSource == TemplatePackageImportExistingProfileSource.BuiltIn)
        {
            return "Blockiert";
        }

        return action switch
        {
            TemplatePackageImportAction.ImportAsNew => "Neu importieren",
            TemplatePackageImportAction.ImportAsCopy => "Als Kopie importieren",
            TemplatePackageImportAction.ReplaceExisting => "Bestehendes ersetzen",
            TemplatePackageImportAction.KeepExisting => "Bestehendes behalten",
            TemplatePackageImportAction.Skip => "Überspringen",
            TemplatePackageImportAction.Blocked => "Blockiert",
            _ => action.ToString()
        };
    }

    private static string FormatConflict(
        TemplatePackageImportConflictType conflict,
        TemplatePackageImportExistingProfileSource? existingSource)
    {
        if (existingSource == TemplatePackageImportExistingProfileSource.BuiltIn
            && conflict is TemplatePackageImportConflictType.SameIdExists
                or TemplatePackageImportConflictType.SameNameExists
                or TemplatePackageImportConflictType.BuiltInProtected)
        {
            return "BuiltIn geschützt";
        }

        return conflict switch
        {
            TemplatePackageImportConflictType.None => "Kein Konflikt",
            TemplatePackageImportConflictType.SameIdExists => "ID-Konflikt",
            TemplatePackageImportConflictType.SameNameExists => "Namenskonflikt",
            TemplatePackageImportConflictType.BuiltInProtected => "BuiltIn geschützt",
            TemplatePackageImportConflictType.VersionMismatch => "Versionskonflikt",
            TemplatePackageImportConflictType.MissingDependency => "Fehlende Abhängigkeit",
            TemplatePackageImportConflictType.InvalidProfile => "Ungültiges Profil",
            TemplatePackageImportConflictType.UnsafeFolderPath => "Unsicherer Ordnerpfad",
            TemplatePackageImportConflictType.UnsupportedProfileKind => "Nicht unterstützte Profilart",
            _ => conflict.ToString()
        };
    }

    private static string FormatDependencyKind(TemplatePackageImportDependencyKind dependencyKind)
    {
        return dependencyKind switch
        {
            TemplatePackageImportDependencyKind.AisProfile => "AIS-Profil",
            TemplatePackageImportDependencyKind.DeviceProfile => "Geräteprofil",
            TemplatePackageImportDependencyKind.ExportProfile => "Exportprofil",
            _ => dependencyKind.ToString()
        };
    }

    private static string FormatResolution(TemplatePackageImportDependencyResolution resolution)
    {
        return resolution switch
        {
            TemplatePackageImportDependencyResolution.LocalExisting => "lokal vorhanden",
            TemplatePackageImportDependencyResolution.ImportedAsNew => "aus Paket neu importiert",
            TemplatePackageImportDependencyResolution.ImportedAsCopy => "aus Paket als Kopie importiert",
            TemplatePackageImportDependencyResolution.Missing => "nicht aufgelöst",
            TemplatePackageImportDependencyResolution.Blocked => "blockiert",
            _ => resolution.ToString()
        };
    }

    private static string CreateProfileKey(ProfileKind profileKind, string profileId)
    {
        return $"{profileKind}:{profileId}";
    }

    private static string DisplayOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string LocalizeUserMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "";
        }

        if (message.StartsWith("Warning: ", StringComparison.OrdinalIgnoreCase))
        {
            return $"Warnung: {LocalizeUserMessage(message["Warning: ".Length..])}";
        }

        if (message.StartsWith("Error: ", StringComparison.OrdinalIgnoreCase))
        {
            return $"Fehler: {LocalizeUserMessage(message["Error: ".Length..])}";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' will not be activated automatically.", out var name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' wird nicht automatisch aktiviert.";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' is marked active in the package and must not be activated automatically.", out name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' ist im Paket als aktiv markiert und wird beim Import nicht automatisch aktiviert.";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' must be reviewed before productive activation.", out name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' muss vor späterer Nutzung geprüft werden.";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' contains XDT attachment folder settings that must be reviewed before use.", out name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' enthält XDT-Anhang-Ordner; Ordnerpfade müssen vor späterer Nutzung geprüft werden.";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' contains XDT attachment settings; folder paths and 6302/6303/6304/6305 must be reviewed before activation.", out name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' enthält XDT-Anhang-Einstellungen; Ordnerpfade und Felder 6302/6303/6304/6305 müssen vor späterer Nutzung geprüft werden.";
        }

        if (TryExtractQuotedName(message, "Imported interface profile '", "' was deactivated. Folder paths and XDT attachment settings must be reviewed before activation.", out name))
        {
            return $"Importiertes Schnittstellenprofil '{name}' wurde inaktiv importiert. Ordnerpfade und XDT-Anhang-Einstellungen müssen vor späterer Nutzung geprüft werden.";
        }

        if (TryExtractQuotedName(message, "Interface profile '", "' dependency remapping must be reviewed before productive import.", out name))
        {
            return $"Abhängigkeitszuordnung für Schnittstellenprofil '{name}' muss vor dem produktiven Import geprüft werden.";
        }

        if (TryExtractQuotedName(message, "Profile '", "' contains folder paths that must be reviewed before activation.", out name))
        {
            return $"Profil '{name}' enthält Ordnerpfade, die vor späterer Nutzung geprüft werden müssen.";
        }

        if (message.StartsWith("BuiltIn profile '", StringComparison.OrdinalIgnoreCase)
            && message.Contains("' is protected.", StringComparison.OrdinalIgnoreCase))
        {
            return message
                .Replace("BuiltIn profile", "BuiltIn-Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("is protected. Safe default is skip; choose import as copy consciously if needed.", "ist geschützt. Sicherer Standard ist Überspringen; wählen Sie Kopie importieren bewusst aus, wenn Sie das Profil benötigen.", StringComparison.OrdinalIgnoreCase);
        }

        if (message.Equals("BuiltIn profile is protected.", StringComparison.OrdinalIgnoreCase))
        {
            return "BuiltIn-Profil ist geschützt.";
        }

        if (message.StartsWith("A local profile with the same Id exists.", StringComparison.OrdinalIgnoreCase))
        {
            return "Ein lokales Profil mit derselben ID existiert bereits. Sicherer Standard ist Überspringen; wählen Sie Kopie importieren bewusst aus, wenn Sie das Profil benötigen.";
        }

        if (message.StartsWith("A local profile with the same name exists.", StringComparison.OrdinalIgnoreCase))
        {
            return "Ein lokales Profil mit demselben Namen existiert bereits. Sicherer Standard ist Überspringen; wählen Sie Kopie importieren bewusst aus, wenn Sie das Profil benötigen.";
        }

        if (message.StartsWith("A local profile with a different version exists.", StringComparison.OrdinalIgnoreCase))
        {
            return "Ein lokales Profil mit anderer Version existiert bereits. Sicherer Standard ist Überspringen; wählen Sie Kopie importieren bewusst aus, wenn Sie das Profil benötigen.";
        }

        if (message.Equals("Profile can be imported as new.", StringComparison.OrdinalIgnoreCase))
        {
            return "Profil kann neu importiert werden.";
        }

        if (message.Equals("Profile will be imported as new.", StringComparison.OrdinalIgnoreCase))
        {
            return "Profil wird neu importiert.";
        }

        if (message.Equals("Profile will be imported as a safe UserDefined copy.", StringComparison.OrdinalIgnoreCase))
        {
            return "Profil wird als sichere UserDefined-Kopie importiert.";
        }

        if (message.Equals("Imported profile will be skipped.", StringComparison.OrdinalIgnoreCase))
        {
            return "Importiertes Profil wird übersprungen.";
        }

        if (message.Equals("Existing local profile will be kept.", StringComparison.OrdinalIgnoreCase))
        {
            return "Bestehendes lokales Profil bleibt erhalten.";
        }

        if (message.Equals("BuiltIn profile replacement is not allowed.", StringComparison.OrdinalIgnoreCase)
            || message.Equals("BuiltIn profile replacement is not allowed. Dry-run blocks this item.", StringComparison.OrdinalIgnoreCase))
        {
            return "BuiltIn-Profil darf nicht ersetzt werden. Die Vorschau blockiert diesen Eintrag.";
        }

        if (message.Equals("Dependency remapped.", StringComparison.OrdinalIgnoreCase))
        {
            return "Abhängigkeit wurde zugeordnet.";
        }

        if (message.StartsWith("Dependency uses existing local ", StringComparison.OrdinalIgnoreCase))
        {
            return message
                .Replace("Dependency uses existing local", "Abhängigkeit nutzt lokales", StringComparison.OrdinalIgnoreCase)
                .Replace("AisProfile", "AIS-Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("DeviceProfile", "Geräteprofil", StringComparison.OrdinalIgnoreCase)
                .Replace("ExportProfile", "Exportprofil", StringComparison.OrdinalIgnoreCase);
        }

        if (message.StartsWith("Dependency is missing: ", StringComparison.OrdinalIgnoreCase))
        {
            return message
                .Replace("Dependency is missing:", "Abhängigkeit konnte nicht aufgelöst werden:", StringComparison.OrdinalIgnoreCase)
                .Replace("AisProfile", "AIS-Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("DeviceProfile", "Geräteprofil", StringComparison.OrdinalIgnoreCase)
                .Replace("ExportProfile", "Exportprofil", StringComparison.OrdinalIgnoreCase);
        }

        if (message.StartsWith("Dependency ", StringComparison.OrdinalIgnoreCase))
        {
            return message
                .Replace("Dependency AisProfile will use imported profile", "Abhängigkeit AIS-Profil nutzt importiertes Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency DeviceProfile will use imported profile", "Abhängigkeit Geräteprofil nutzt importiertes Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency ExportProfile will use imported profile", "Abhängigkeit Exportprofil nutzt importiertes Profil", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency AisProfile must be remapped to copied profile", "Abhängigkeit AIS-Profil wird auf kopiertes Profil zugeordnet", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency DeviceProfile must be remapped to copied profile", "Abhängigkeit Geräteprofil wird auf kopiertes Profil zugeordnet", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency ExportProfile must be remapped to copied profile", "Abhängigkeit Exportprofil wird auf kopiertes Profil zugeordnet", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency AisProfile will use existing target", "Abhängigkeit AIS-Profil nutzt bestehendes Ziel", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency DeviceProfile will use existing target", "Abhängigkeit Geräteprofil nutzt bestehendes Ziel", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency ExportProfile will use existing target", "Abhängigkeit Exportprofil nutzt bestehendes Ziel", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency AisProfile is blocked and prevents safe use.", "Abhängigkeit AIS-Profil ist blockiert und verhindert die sichere Nutzung.", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency DeviceProfile is blocked and prevents safe use.", "Abhängigkeit Geräteprofil ist blockiert und verhindert die sichere Nutzung.", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency ExportProfile is blocked and prevents safe use.", "Abhängigkeit Exportprofil ist blockiert und verhindert die sichere Nutzung.", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency AisProfile cannot be resolved.", "Abhängigkeit AIS-Profil konnte nicht aufgelöst werden.", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency DeviceProfile cannot be resolved.", "Abhängigkeit Geräteprofil konnte nicht aufgelöst werden.", StringComparison.OrdinalIgnoreCase)
                .Replace("Dependency ExportProfile cannot be resolved.", "Abhängigkeit Exportprofil konnte nicht aufgelöst werden.", StringComparison.OrdinalIgnoreCase);
        }

        if (message.Contains("dependency must be remapped from", StringComparison.OrdinalIgnoreCase))
        {
            return message
                .Replace("AisProfile dependency must be remapped from", "AIS-Profil-Abhängigkeit wird zugeordnet von", StringComparison.OrdinalIgnoreCase)
                .Replace("DeviceProfile dependency must be remapped from", "Geräteprofil-Abhängigkeit wird zugeordnet von", StringComparison.OrdinalIgnoreCase)
                .Replace("ExportProfile dependency must be remapped from", "Exportprofil-Abhängigkeit wird zugeordnet von", StringComparison.OrdinalIgnoreCase)
                .Replace(" to ", " nach ", StringComparison.OrdinalIgnoreCase);
        }

        return message;
    }

    private static bool TryExtractQuotedName(
        string message,
        string prefix,
        string suffix,
        out string name)
    {
        name = "";
        if (!message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || !message.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        name = message[prefix.Length..^suffix.Length];
        return true;
    }

    private static string DisplayTargetName(string? value, TemplatePackageImportAction plannedAction)
    {
        if (plannedAction == TemplatePackageImportAction.ImportAsCopy)
        {
            return value ?? "";
        }

        return DisplayOrDash(value);
    }

    private static string DisplayDependencyTarget(
        string? value,
        TemplatePackageImportDependencyResolution resolution)
    {
        if (resolution is TemplatePackageImportDependencyResolution.Missing
            or TemplatePackageImportDependencyResolution.Blocked)
        {
            return "Nicht aufgelöst";
        }

        return DisplayOrDash(value);
    }
}
