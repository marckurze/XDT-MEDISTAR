using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportPreviewDisplayServiceTests
{
    private readonly TemplatePackageImportPreviewDisplayService _service = new();

    [Fact]
    public void Create_ShouldCreateOkRowsForConflictFreePackage()
    {
        var display = CreateDisplay(
            Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
            Item(ProfileKind.AisProfile, "ais-new", "New AIS"));

        var row = Assert.Single(display.Rows);
        Assert.Equal("AIS-Profil", row.ProfileKind);
        Assert.Equal("Neu importieren", row.PlannedAction);
        Assert.Equal("Kein Konflikt", row.Conflict);
        Assert.Equal("OK", row.Status);
        Assert.Contains("Profile gesamt: 1", display.Summary.SummaryText);
        Assert.Contains(display.Messages, message => message.Contains("nichts gespeichert", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_ShouldShowBuiltInProtection()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                conflictType: TemplatePackageImportConflictType.BuiltInProtected,
                plannedAction: TemplatePackageImportAction.Skip,
                existingSource: TemplatePackageImportExistingProfileSource.BuiltIn,
                requiresUserDecision: true,
                message: "BuiltIn profile is protected."),
            Item(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                plannedAction: TemplatePackageImportAction.Skip,
                wouldWrite: false,
                wouldSkip: true,
                message: "BuiltIn profile is protected."));

        var row = Assert.Single(display.Rows);
        Assert.Equal("BuiltIn geschützt", row.Conflict);
        Assert.Equal("Überspringen", row.PlannedAction);
        Assert.Equal(TemplatePackageImportAction.Skip, row.SelectedAction);
        Assert.Equal("Warnung", row.Status);
        Assert.DoesNotContain("Bestehendes ersetzen", row.PlannedAction);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ImportAsCopy);
        Assert.Contains("BuiltIn", row.Message);
        Assert.DoesNotContain("protected", row.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ShouldNotDisplayReplaceActionForBuiltInReplacement()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                conflictType: TemplatePackageImportConflictType.BuiltInProtected,
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                existingSource: TemplatePackageImportExistingProfileSource.BuiltIn,
                requiresUserDecision: true,
                message: "BuiltIn profile is protected."),
            Item(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                isBlocking: true,
                wouldWrite: false,
                message: "BuiltIn profile replacement is not allowed."));

        var row = Assert.Single(display.Rows);
        Assert.Equal("Blockiert", row.PlannedAction);
        Assert.DoesNotContain("ersetzen", row.PlannedAction, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ShouldShowMissingDependencyAsBlockingDependency()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.InterfaceProfile,
                "interface-imported",
                "Interface",
                conflictType: TemplatePackageImportConflictType.MissingDependency,
                plannedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                requiresUserDecision: true,
                message: "Missing dependency."),
            Item(
                ProfileKind.InterfaceProfile,
                "interface-imported",
                "Interface",
                plannedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                wouldWrite: false,
                message: "Missing dependency.",
                dependencyRemaps: new[]
                {
                    Remap(
                        TemplatePackageImportDependencyKind.ExportProfile,
                        "export-missing",
                        TemplatePackageImportDependencyResolution.Missing)
                }));

        var row = Assert.Single(display.Rows);
        var dependency = Assert.Single(display.DependencyRows);
        Assert.Equal("Blockiert", row.Status);
        Assert.Equal("Fehlende Abhängigkeit", row.Conflict);
        Assert.Equal("nicht aufgelöst", dependency.Resolution);
        Assert.Equal("Nicht aufgelöst", dependency.TargetProfileName);
        Assert.Equal("Blockiert", dependency.Status);
        Assert.True(display.Summary.HasBlockingItems);
    }

    [Fact]
    public void Create_ShouldShowAttachmentAndActivationWarnings()
    {
        var display = CreateDisplay(
            Plan(ProfileKind.InterfaceProfile, "interface-imported", "Interface"),
            Item(
                ProfileKind.InterfaceProfile,
                "interface-imported",
                "Interface",
                dependencyWarnings: new[]
                {
                    "Imported interface profile 'Interface' contains XDT attachment settings; folder paths and 6302/6303/6304/6305 must be reviewed before activation.",
                    "Imported interface profile 'Interface' will not be activated automatically.",
                    "Imported interface profile 'Interface' will not be activated automatically."
                }));

        var row = Assert.Single(display.Rows);
        Assert.Equal("Warnung", row.Status);
        Assert.Contains(display.Warnings, warning => warning.Contains("6302/6303/6304/6305", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(display.Warnings, warning => warning.Contains("wird nicht automatisch aktiviert", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(display.Warnings.Count, display.Warnings.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        AssertVisiblePreviewTextIsGerman(display);
    }

    [Fact]
    public void Create_ShouldExplainEmptyDependenciesWhenInterfaceProfilesAreSkipped()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.InterfaceProfile,
                "interface-skip",
                "Interface",
                plannedAction: TemplatePackageImportAction.Skip,
                requiresUserDecision: true),
            Item(
                ProfileKind.InterfaceProfile,
                "interface-skip",
                "Interface",
                plannedAction: TemplatePackageImportAction.Skip,
                wouldWrite: false,
                wouldSkip: true));

        Assert.Empty(display.DependencyRows);
        Assert.Contains("übersprungen", display.DependencyEmptyStateMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ShouldCalculateSummaryCounts()
    {
        var display = CreateDisplay(
            new[]
            {
                Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Plan(ProfileKind.DeviceProfile, "device-copy", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy, requiresUserDecision: true, requiresRename: true, proposedId: "device-copy-import", proposedName: "Device (Import)"),
                Plan(ProfileKind.ExportProfile, "export-skip", "Export", plannedAction: TemplatePackageImportAction.Skip, requiresUserDecision: true),
                Plan(ProfileKind.InterfaceProfile, "interface-blocked", "Interface", conflictType: TemplatePackageImportConflictType.MissingDependency, plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true, requiresUserDecision: true)
            },
            new[]
            {
                Item(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Item(ProfileKind.DeviceProfile, "device-copy", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy, targetId: "device-copy-import", targetName: "Device (Import)"),
                Item(ProfileKind.ExportProfile, "export-skip", "Export", plannedAction: TemplatePackageImportAction.Skip, wouldWrite: false, wouldSkip: true),
                Item(ProfileKind.InterfaceProfile, "interface-blocked", "Interface", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true, wouldWrite: false)
            });

        Assert.Equal(4, display.Summary.TotalProfiles);
        Assert.Equal(1, display.Summary.PlannedImportAsCopy);
        Assert.Equal(1, display.Summary.PlannedKeepOrSkip);
        Assert.Equal(1, display.Summary.BlockedProfiles);
        Assert.Contains(display.Messages, message => message.Contains("1 Profil(e) würden als Kopie", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_ShouldIncludeValidationWarningsInDisplay()
    {
        var display = CreateDisplay(
            Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
            Item(ProfileKind.AisProfile, "ais-new", "New AIS"),
            validationIssues: new[]
            {
                new TemplatePackageImportValidationIssue(
                    TemplatePackageImportValidationIssueSeverity.Warning,
                    "Test warning.",
                    "ais-new",
                    ProfileKind.AisProfile)
            });

        Assert.Contains(display.Warnings, warning => warning.Contains("Test warning", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(display.Messages, message => message.Contains("0 Fehler, 1 Warnungen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_ShouldOfferOnlyNewAndSkipForConflictFreeRows()
    {
        var display = CreateDisplay(
            Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
            Item(ProfileKind.AisProfile, "ais-new", "New AIS"));

        var row = Assert.Single(display.Rows);
        Assert.True(row.IsActionSelectionEnabled);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ImportAsNew);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.Skip);
        Assert.DoesNotContain(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ReplaceExisting);
    }

    [Fact]
    public void Create_ShouldOfferKeepExistingForUserDefinedConflict()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                conflictType: TemplatePackageImportConflictType.SameIdExists,
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                existingSource: TemplatePackageImportExistingProfileSource.UserDefined),
            Item(
                ProfileKind.ExportProfile,
                "export-imported",
                "Export",
                plannedAction: TemplatePackageImportAction.ImportAsCopy));

        var row = Assert.Single(display.Rows);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ImportAsCopy);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.KeepExisting);
        Assert.Contains(row.AvailableActions, action => action.Action == TemplatePackageImportAction.Skip);
        Assert.DoesNotContain(row.AvailableActions, action => action.Action == TemplatePackageImportAction.ReplaceExisting);
    }

    [Fact]
    public void Create_ShouldAllowTargetNameEditingOnlyForCopyRows()
    {
        var display = CreateDisplay(
            new[]
            {
                Plan(ProfileKind.DeviceProfile, "device-copy", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy, requiresUserDecision: true, requiresRename: true, proposedId: "device-copy-import", proposedName: "Device (Import)"),
                Plan(ProfileKind.AisProfile, "ais-skip", "AIS", plannedAction: TemplatePackageImportAction.Skip, requiresUserDecision: true)
            },
            new[]
            {
                Item(ProfileKind.DeviceProfile, "device-copy", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy, targetId: "device-copy-import", targetName: "Device (Import)"),
                Item(ProfileKind.AisProfile, "ais-skip", "AIS", plannedAction: TemplatePackageImportAction.Skip, wouldWrite: false, wouldSkip: true)
            });

        var copyRow = Assert.Single(display.Rows, row => row.ImportedProfileId == "device-copy");
        var skipRow = Assert.Single(display.Rows, row => row.ImportedProfileId == "ais-skip");
        Assert.True(copyRow.IsTargetNameEditable);
        Assert.Equal("Device (Import)", copyRow.TargetProfileName);
        Assert.False(skipRow.IsTargetNameEditable);
    }

    [Fact]
    public void Create_ShouldDisableActionSelectionForBlockedRows()
    {
        var display = CreateDisplay(
            Plan(
                ProfileKind.InterfaceProfile,
                "interface-blocked",
                "Interface",
                conflictType: TemplatePackageImportConflictType.MissingDependency,
                plannedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true),
            Item(
                ProfileKind.InterfaceProfile,
                "interface-blocked",
                "Interface",
                plannedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                wouldWrite: false));

        var row = Assert.Single(display.Rows);
        Assert.False(row.IsActionSelectionEnabled);
        var action = Assert.Single(row.AvailableActions);
        Assert.Equal(TemplatePackageImportAction.Blocked, action.Action);
    }

    private TemplatePackageImportPreviewDisplay CreateDisplay(
        TemplatePackageImportProfilePlan plan,
        TemplatePackageImportDryRunItem item,
        IReadOnlyList<TemplatePackageImportValidationIssue>? validationIssues = null)
    {
        return CreateDisplay(new[] { plan }, new[] { item }, validationIssues);
    }

    private TemplatePackageImportPreviewDisplay CreateDisplay(
        IReadOnlyList<TemplatePackageImportProfilePlan> plans,
        IReadOnlyList<TemplatePackageImportDryRunItem> items,
        IReadOnlyList<TemplatePackageImportValidationIssue>? validationIssues = null)
    {
        var validation = new TemplatePackageImportValidationResult(validationIssues ?? Array.Empty<TemplatePackageImportValidationIssue>());
        var analysis = new TemplatePackageImportAnalysisResult(
            Success: true,
            PackageId: "package-1",
            PackageName: "Package 1",
            ProfileDecisions: Array.Empty<TemplatePackageImportProfileDecision>(),
            BlockingConflicts: Array.Empty<TemplatePackageImportProfileDecision>(),
            Warnings: Array.Empty<string>(),
            ErrorMessage: null,
            TotalProfiles: plans.Count,
            ImportableProfiles: plans.Count(plan => plan.PlannedAction != TemplatePackageImportAction.Blocked),
            ConflictingProfiles: plans.Count(plan => plan.ConflictType != TemplatePackageImportConflictType.None),
            BlockedProfiles: plans.Count(plan => plan.IsBlocking || plan.PlannedAction == TemplatePackageImportAction.Blocked));
        var plan = new TemplatePackageImportPlan(
            PackageId: "package-1",
            PackageName: "Package 1",
            GeneratedAt: new DateTimeOffset(2026, 5, 9, 10, 0, 0, TimeSpan.Zero),
            ProfilePlans: plans,
            HasBlockingItems: plans.Any(plan => plan.IsBlocking || plan.PlannedAction == TemplatePackageImportAction.Blocked),
            BlockingItems: plans.Where(plan => plan.IsBlocking || plan.PlannedAction == TemplatePackageImportAction.Blocked).ToList(),
            Warnings: Array.Empty<string>(),
            TotalProfiles: plans.Count,
            PlannedImportAsNew: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Blocked));
        var dryRun = new TemplatePackageImportDryRunResult(
            Success: true,
            PackageId: "package-1",
            PackageName: "Package 1",
            Items: items,
            BlockingItems: items.Where(item => item.IsBlocking).ToList(),
            Warnings: items.SelectMany(item => item.DependencyRemapWarnings).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ErrorMessage: null,
            TotalItems: items.Count,
            WouldImportAsNew: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            WouldImportAsCopy: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            WouldReplaceExisting: items.Count(item => item.WouldReplace),
            WouldKeepExisting: items.Count(item => item.WouldKeepExisting),
            WouldSkip: items.Count(item => item.WouldSkip),
            WouldBlock: items.Count(item => item.IsBlocking));

        return _service.Create(validation, analysis, plan, dryRun);
    }

    private static TemplatePackageImportProfilePlan Plan(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportConflictType conflictType = TemplatePackageImportConflictType.None,
        TemplatePackageImportAction plannedAction = TemplatePackageImportAction.ImportAsNew,
        bool isBlocking = false,
        bool requiresUserDecision = false,
        bool requiresRename = false,
        TemplatePackageImportExistingProfileSource existingSource = TemplatePackageImportExistingProfileSource.None,
        string? proposedId = null,
        string? proposedName = null,
        string message = "Plan message.")
    {
        return new TemplatePackageImportProfilePlan(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            ExistingProfileId: existingSource == TemplatePackageImportExistingProfileSource.None ? null : $"{importedId}-existing",
            ExistingProfileName: existingSource == TemplatePackageImportExistingProfileSource.None ? null : $"{importedName} existing",
            ExistingProfileSource: existingSource,
            ConflictType: conflictType,
            PlannedAction: plannedAction,
            IsBlocking: isBlocking,
            RequiresUserDecision: requiresUserDecision,
            RequiresRename: requiresRename,
            ProposedProfileId: proposedId ?? importedId,
            ProposedProfileName: proposedName ?? importedName,
            Message: message);
    }

    private static TemplatePackageImportDryRunItem Item(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportAction plannedAction = TemplatePackageImportAction.ImportAsNew,
        string? targetId = null,
        string? targetName = null,
        bool isBlocking = false,
        bool wouldWrite = true,
        bool wouldReplace = false,
        bool wouldKeepExisting = false,
        bool wouldSkip = false,
        bool requiresDependencyRemap = false,
        IReadOnlyList<TemplatePackageImportDependencyRemap>? dependencyRemaps = null,
        IReadOnlyList<string>? dependencyWarnings = null,
        string message = "Dry-run message.")
    {
        return new TemplatePackageImportDryRunItem(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            PlannedAction: plannedAction,
            TargetProfileId: targetId ?? (wouldWrite ? importedId : null),
            TargetProfileName: targetName ?? (wouldWrite ? importedName : null),
            ExistingProfileId: null,
            ExistingProfileName: null,
            IsBlocking: isBlocking,
            WouldWrite: wouldWrite,
            WouldReplace: wouldReplace,
            WouldKeepExisting: wouldKeepExisting,
            WouldSkip: wouldSkip,
            RequiresDependencyRemap: requiresDependencyRemap,
            DependencyRemaps: dependencyRemaps ?? Array.Empty<TemplatePackageImportDependencyRemap>(),
            DependencyRemapWarnings: dependencyWarnings ?? Array.Empty<string>(),
            Message: message);
    }

    private static TemplatePackageImportDependencyRemap Remap(
        TemplatePackageImportDependencyKind dependencyKind,
        string originalId,
        TemplatePackageImportDependencyResolution resolution)
    {
        return new TemplatePackageImportDependencyRemap(
            DependencyKind: dependencyKind,
            OriginalProfileId: originalId,
            OriginalProfileName: null,
            TargetProfileId: null,
            TargetProfileName: null,
            Resolution: resolution,
            Message: $"Dependency {dependencyKind} {resolution}.");
    }

    private static void AssertVisiblePreviewTextIsGerman(TemplatePackageImportPreviewDisplay display)
    {
        var visibleText = string.Join(
            Environment.NewLine,
            display.Messages
                .Concat(display.Warnings)
                .Concat(display.Rows.Select(row => row.Message))
                .Concat(display.DependencyRows.Select(row => row.Message))
                .Concat(new[] { display.DependencyEmptyStateMessage }));

        Assert.DoesNotContain("Imported interface profile", visibleText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("must be reviewed", visibleText, StringComparison.OrdinalIgnoreCase);
    }
}
