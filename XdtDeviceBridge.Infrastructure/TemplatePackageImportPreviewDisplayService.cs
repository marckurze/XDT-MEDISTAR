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
        var warningMessages = CreateWarningMessages(validationResult, analysisResult, plan, dryRunResult).ToList();
        var rows = (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .Select(item => CreateRow(item, profilePlans))
            .ToList();
        var dependencyRows = (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>())
            .SelectMany(CreateDependencyRows)
            .ToList();
        var messages = CreateMessages(validationResult, analysisResult, dryRunResult, warningMessages.Count).ToList();
        var summary = CreateSummary(analysisResult, dryRunResult, warningMessages.Count);

        return new TemplatePackageImportPreviewDisplay(
            Summary: summary,
            Rows: rows,
            DependencyRows: dependencyRows,
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
            targetProfileName: DisplayOrDash(item.TargetProfileName),
            targetProfileId: DisplayOrDash(item.TargetProfileId),
            conflict: FormatConflict(conflict, plan?.ExistingProfileSource),
            status: status,
            message: item.Message);
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
                TargetProfileName: DisplayOrDash(remap.TargetProfileName),
                TargetProfileId: DisplayOrDash(remap.TargetProfileId),
                Resolution: FormatResolution(remap.Resolution),
                Status: remap.Resolution is TemplatePackageImportDependencyResolution.Missing
                    or TemplatePackageImportDependencyResolution.Blocked
                        ? "Blockiert"
                        : remap.Resolution == TemplatePackageImportDependencyResolution.ImportedAsCopy
                            ? "Warnung"
                            : "OK",
                Message: remap.Message);
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
            TemplatePackageImportDependencyResolution.Missing => "fehlt",
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
}
