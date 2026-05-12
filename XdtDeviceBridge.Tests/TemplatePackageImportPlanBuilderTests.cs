using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportPlanBuilderTests
{
    private readonly TemplatePackageImportPlanBuilder _builder = new();
    private static readonly DateTimeOffset GeneratedAt = new(2026, 5, 8, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Build_ShouldPlanImportAsNewForConflictFreeAnalysis()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(ProfileKind.AisProfile, "ais-new", "New AIS")),
            GeneratedAt);

        Assert.False(plan.HasBlockingItems);
        Assert.Equal("package-1", plan.PackageId);
        Assert.Equal(GeneratedAt, plan.GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.ImportAsNew, profilePlan.PlannedAction);
        Assert.False(profilePlan.RequiresUserDecision);
        Assert.False(profilePlan.IsBlocking);
        Assert.False(profilePlan.RequiresRename);
        Assert.Equal("ais-new", profilePlan.ProposedProfileId);
        Assert.Equal("New AIS", profilePlan.ProposedProfileName);
    }

    [Fact]
    public void Build_ShouldPlanSkipForBuiltInConflict()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.ExportProfile,
                "export-medistar",
                "MEDISTAR Export",
                conflictType: TemplatePackageImportConflictType.BuiltInProtected,
                suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                existingId: "export-medistar",
                existingName: "MEDISTAR Export",
                existingSource: TemplatePackageImportExistingProfileSource.BuiltIn)),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction);
        Assert.False(profilePlan.RequiresRename);
        Assert.True(profilePlan.RequiresUserDecision);
        Assert.False(profilePlan.IsBlocking);
        Assert.NotEqual(TemplatePackageImportAction.ReplaceExisting, profilePlan.PlannedAction);
        Assert.Null(profilePlan.ProposedProfileId);
        Assert.Null(profilePlan.ProposedProfileName);
        Assert.Contains("BuiltIn", profilePlan.Message);
    }

    [Fact]
    public void Build_ShouldPlanSkipForUserDefinedConflict()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.DeviceProfile,
                "device-existing",
                "Existing Device",
                conflictType: TemplatePackageImportConflictType.SameIdExists,
                suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                existingId: "device-existing",
                existingName: "Existing Device",
                existingSource: TemplatePackageImportExistingProfileSource.UserDefined)),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction);
        Assert.True(profilePlan.RequiresUserDecision);
        Assert.False(profilePlan.RequiresRename);
        Assert.False(profilePlan.IsBlocking);
        Assert.NotEqual(TemplatePackageImportAction.ReplaceExisting, profilePlan.PlannedAction);
    }

    [Fact]
    public void Build_ShouldBlockMissingDependency()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.InterfaceProfile,
                "interface-missing",
                "Missing Dependency Interface",
                conflictType: TemplatePackageImportConflictType.MissingDependency,
                suggestedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                message: "Interface profile references missing AIS profile: ais-missing")),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.True(plan.HasBlockingItems);
        Assert.Single(plan.BlockingItems);
        Assert.Equal(TemplatePackageImportAction.Blocked, profilePlan.PlannedAction);
        Assert.True(profilePlan.IsBlocking);
        Assert.True(profilePlan.RequiresUserDecision);
    }

    [Fact]
    public void Build_ShouldKeepUnsafeFolderPathAsReviewRequiredWarning()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.InterfaceProfile,
                "interface-unsafe",
                "Unsafe Interface",
                conflictType: TemplatePackageImportConflictType.UnsafeFolderPath,
                suggestedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                message: "Unsafe folder path.")),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.ImportAsNew, profilePlan.PlannedAction);
        Assert.False(profilePlan.IsBlocking);
        Assert.True(profilePlan.RequiresUserDecision);
        Assert.Contains(plan.Warnings, warning => warning.Contains("folder paths", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("must not be activated automatically", profilePlan.Message);
    }

    [Theory]
    [InlineData(TemplatePackageImportConflictType.InvalidProfile)]
    [InlineData(TemplatePackageImportConflictType.UnsupportedProfileKind)]
    public void Build_ShouldBlockInvalidOrUnsupportedProfiles(TemplatePackageImportConflictType conflictType)
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.AisProfile,
                "ais-invalid",
                "Invalid AIS",
                conflictType: conflictType,
                suggestedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true,
                message: "Invalid profile.")),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Blocked, profilePlan.PlannedAction);
        Assert.True(profilePlan.IsBlocking);
        Assert.True(plan.HasBlockingItems);
    }

    [Fact]
    public void Build_ShouldNotPreselectCopyNameForConflict()
    {
        var plan = _builder.Build(
            CreateAnalysis(Decision(
                ProfileKind.AisProfile,
                "ais-existing",
                "MEDISTAR",
                conflictType: TemplatePackageImportConflictType.SameNameExists,
                suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                existingId: "ais-local",
                existingName: "MEDISTAR",
                existingSource: TemplatePackageImportExistingProfileSource.UserDefined)),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction);
        Assert.Null(profilePlan.ProposedProfileName);
    }

    [Fact]
    public void Build_ShouldNotPreselectNumberedCopyNameWhenImportNameAlreadyExists()
    {
        var plan = _builder.Build(
            CreateAnalysis(
                Decision(
                    ProfileKind.AisProfile,
                    "ais-copy-name",
                    "MEDISTAR (Import)"),
                Decision(
                    ProfileKind.AisProfile,
                    "ais-existing",
                    "MEDISTAR",
                    conflictType: TemplatePackageImportConflictType.SameNameExists,
                    suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                    existingId: "ais-local",
                    existingName: "MEDISTAR",
                    existingSource: TemplatePackageImportExistingProfileSource.UserDefined)),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans, item => item.ImportedProfileName == "MEDISTAR");
        Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction);
        Assert.Null(profilePlan.ProposedProfileName);
    }

    [Fact]
    public void Build_ShouldNotPreselectCopyIdForConflict()
    {
        var plan = _builder.Build(
            CreateAnalysis(
                Decision(
                    ProfileKind.DeviceProfile,
                    "device-existing-import",
                    "Already Reserved"),
                Decision(
                    ProfileKind.DeviceProfile,
                    "device-existing",
                    "Existing Device",
                    conflictType: TemplatePackageImportConflictType.SameIdExists,
                    suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                    existingId: "device-existing",
                    existingName: "Existing Device",
                    existingSource: TemplatePackageImportExistingProfileSource.UserDefined)),
            GeneratedAt);

        var profilePlan = Assert.Single(plan.ProfilePlans, item => item.ImportedProfileId == "device-existing");
        Assert.Equal(TemplatePackageImportAction.Skip, profilePlan.PlannedAction);
        Assert.Null(profilePlan.ProposedProfileId);
    }

    [Fact]
    public void Build_ShouldCalculateCounts()
    {
        var plan = _builder.Build(
            CreateAnalysis(
                Decision(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Decision(
                    ProfileKind.DeviceProfile,
                    "device-conflict",
                    "Device Conflict",
                    conflictType: TemplatePackageImportConflictType.SameIdExists,
                    suggestedAction: TemplatePackageImportAction.ImportAsCopy,
                    existingId: "device-conflict",
                    existingName: "Existing Device",
                    existingSource: TemplatePackageImportExistingProfileSource.UserDefined),
                Decision(
                    ProfileKind.InterfaceProfile,
                    "interface-blocked",
                    "Blocked Interface",
                    conflictType: TemplatePackageImportConflictType.MissingDependency,
                    suggestedAction: TemplatePackageImportAction.Blocked,
                    isBlocking: true,
                    message: "Missing dependency.")),
            GeneratedAt);

        Assert.Equal(3, plan.TotalProfiles);
        Assert.Equal(1, plan.PlannedImportAsNew);
        Assert.Equal(0, plan.PlannedImportAsCopy);
        Assert.Equal(0, plan.PlannedReplaceExisting);
        Assert.Equal(0, plan.PlannedKeepExisting);
        Assert.Equal(1, plan.PlannedSkip);
        Assert.Equal(1, plan.PlannedBlocked);
    }

    [Fact]
    public void Build_ShouldCarryAttachmentSettingsWarning()
    {
        var plan = _builder.Build(
            CreateAnalysis(
                new[]
                {
                    Decision(ProfileKind.InterfaceProfile, "interface-attachment", "Attachment Interface")
                },
                warnings: new[]
                {
                    "Imported interface profile 'Attachment Interface' contains XDT attachment folder settings that must be reviewed before use."
                }),
            GeneratedAt);

        Assert.Contains(plan.Warnings, warning => warning.Contains("XDT attachment folder settings", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(plan.Warnings, warning => warning.Contains("dependency remapping", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_ShouldNotWriteFiles()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        _builder.Build(
            CreateAnalysis(Decision(ProfileKind.AisProfile, "ais-new", "New AIS")),
            GeneratedAt);

        Assert.Empty(Directory.EnumerateFileSystemEntries(folder));
    }

    private static TemplatePackageImportAnalysisResult CreateAnalysis(params TemplatePackageImportProfileDecision[] decisions)
    {
        return CreateAnalysis(decisions, Array.Empty<string>());
    }

    private static TemplatePackageImportAnalysisResult CreateAnalysis(
        IReadOnlyList<TemplatePackageImportProfileDecision> decisions,
        IReadOnlyList<string> warnings)
    {
        var blocking = decisions.Where(decision => decision.IsBlocking).ToList();

        return new TemplatePackageImportAnalysisResult(
            Success: true,
            PackageId: "package-1",
            PackageName: "Package 1",
            ProfileDecisions: decisions,
            BlockingConflicts: blocking,
            Warnings: warnings,
            ErrorMessage: null,
            TotalProfiles: decisions.Count,
            ImportableProfiles: decisions.Count(decision => decision.SuggestedAction != TemplatePackageImportAction.Blocked),
            ConflictingProfiles: decisions.Count(decision => decision.ConflictType != TemplatePackageImportConflictType.None),
            BlockedProfiles: blocking.Count);
    }

    private static TemplatePackageImportProfileDecision Decision(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportConflictType conflictType = TemplatePackageImportConflictType.None,
        TemplatePackageImportAction suggestedAction = TemplatePackageImportAction.ImportAsNew,
        bool isBlocking = false,
        string? existingId = null,
        string? existingName = null,
        TemplatePackageImportExistingProfileSource existingSource = TemplatePackageImportExistingProfileSource.None,
        string message = "Decision message.")
    {
        return new TemplatePackageImportProfileDecision(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            ImportedProfileVersion: "1.0.0",
            ExistingProfileId: existingId,
            ExistingProfileName: existingName,
            ExistingProfileSource: existingSource,
            ExistingProfileVersion: existingId is null ? null : "1.0.0",
            ConflictType: conflictType,
            SuggestedAction: suggestedAction,
            IsBlocking: isBlocking,
            Message: message);
    }
}
