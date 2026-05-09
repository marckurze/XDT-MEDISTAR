using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportSelectionServiceTests
{
    private readonly TemplatePackageImportSelectionService _service = new();
    private readonly TemplatePackageImportDryRunService _dryRunService = new();

    [Fact]
    public void Apply_ShouldKeepConflictFreeProfileAsImportAsNewByDefault()
    {
        var plan = CreatePlan(Plan(ProfileKind.AisProfile, "ais-new", "New AIS"));

        var updated = _service.Apply(plan, Array.Empty<TemplatePackageImportUserSelection>());

        var item = Assert.Single(updated.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.ImportAsNew, item.PlannedAction);
        Assert.False(item.IsBlocking);
    }

    [Fact]
    public void Apply_ShouldAllowSkipForConflictFreeProfile()
    {
        var plan = CreatePlan(Plan(ProfileKind.AisProfile, "ais-new", "New AIS"));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.AisProfile, "ais-new", TemplatePackageImportAction.Skip) });

        var item = Assert.Single(updated.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Skip, item.PlannedAction);
        Assert.False(item.IsBlocking);
        Assert.Equal(1, updated.PlannedSkip);
    }

    [Fact]
    public void Apply_ShouldPlanCopyForBuiltInConflictAndAllowSkip()
    {
        var plan = CreatePlan(Plan(
            ProfileKind.DeviceProfile,
            "device-imported",
            "Device",
            conflictType: TemplatePackageImportConflictType.BuiltInProtected,
            plannedAction: TemplatePackageImportAction.ImportAsCopy,
            existingSource: TemplatePackageImportExistingProfileSource.BuiltIn,
            existingId: "device-builtin",
            existingName: "BuiltIn Device"));

        var skipped = _service.Apply(plan, new[] { Selection(ProfileKind.DeviceProfile, "device-imported", TemplatePackageImportAction.Skip) });
        var copied = _service.Apply(plan, Array.Empty<TemplatePackageImportUserSelection>());

        Assert.Equal(TemplatePackageImportAction.Skip, Assert.Single(skipped.ProfilePlans).PlannedAction);
        var copiedPlan = Assert.Single(copied.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.ImportAsCopy, copiedPlan.PlannedAction);
        Assert.NotEqual("device-builtin", copiedPlan.ProposedProfileId);
    }

    [Fact]
    public void Apply_ShouldAllowKeepExistingForUserDefinedConflict()
    {
        var plan = CreatePlan(Plan(
            ProfileKind.ExportProfile,
            "export-imported",
            "Export",
            conflictType: TemplatePackageImportConflictType.SameIdExists,
            plannedAction: TemplatePackageImportAction.ImportAsCopy,
            existingSource: TemplatePackageImportExistingProfileSource.UserDefined,
            existingId: "export-local",
            existingName: "Local Export"));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.ExportProfile, "export-imported", TemplatePackageImportAction.KeepExisting) });

        var item = Assert.Single(updated.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.KeepExisting, item.PlannedAction);
        Assert.False(item.IsBlocking);
        Assert.Null(item.ProposedProfileId);
    }

    [Fact]
    public void Apply_ShouldNotAllowReplaceExisting()
    {
        var plan = CreatePlan(Plan(
            ProfileKind.ExportProfile,
            "export-imported",
            "Export",
            conflictType: TemplatePackageImportConflictType.SameIdExists,
            plannedAction: TemplatePackageImportAction.ImportAsCopy,
            existingSource: TemplatePackageImportExistingProfileSource.UserDefined,
            existingId: "export-local",
            existingName: "Local Export"));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.ExportProfile, "export-imported", TemplatePackageImportAction.ReplaceExisting) });

        var item = Assert.Single(updated.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Blocked, item.PlannedAction);
        Assert.True(item.IsBlocking);
        Assert.Contains("ReplaceExisting", item.Message);
    }

    [Fact]
    public void Apply_ShouldNotAllowBlockedProfileToBeUnblocked()
    {
        var plan = CreatePlan(Plan(
            ProfileKind.InterfaceProfile,
            "interface-blocked",
            "Interface",
            conflictType: TemplatePackageImportConflictType.MissingDependency,
            plannedAction: TemplatePackageImportAction.Blocked,
            isBlocking: true));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.InterfaceProfile, "interface-blocked", TemplatePackageImportAction.ImportAsNew) });

        var item = Assert.Single(updated.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.Blocked, item.PlannedAction);
        Assert.True(item.IsBlocking);
    }

    [Fact]
    public void Apply_ShouldCreateUniqueCopyNames()
    {
        var plan = CreatePlan(
            Plan(ProfileKind.DeviceProfile, "device-one", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy),
            Plan(ProfileKind.DeviceProfile, "device-two", "Device", plannedAction: TemplatePackageImportAction.ImportAsCopy));

        var updated = _service.Apply(plan, new[]
        {
            Selection(ProfileKind.DeviceProfile, "device-one", TemplatePackageImportAction.ImportAsCopy),
            Selection(ProfileKind.DeviceProfile, "device-two", TemplatePackageImportAction.ImportAsCopy)
        });

        Assert.Contains(updated.ProfilePlans, item => item.ProposedProfileName == "Device (Import)");
        Assert.Contains(updated.ProfilePlans, item => item.ProposedProfileName == "Device (Import 2)");
    }

    [Fact]
    public void Apply_ShouldMakeSkippedDependencyBlockInterfaceProfileInDryRun()
    {
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local");
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Interface", "ais-local", "device-local", "export-imported");
        var importResult = CreateImportResult(
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile });
        var plan = CreatePlan(
            Plan(ProfileKind.ExportProfile, "export-imported", "Imported Export"),
            Plan(ProfileKind.InterfaceProfile, "interface-imported", "Interface"));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.ExportProfile, "export-imported", TemplatePackageImportAction.Skip) });
        var dryRun = _dryRunService.Preview(importResult, updated, CreateCatalog(
            aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") }));

        var interfaceItem = Assert.Single(dryRun.Items, item => item.ProfileKind == ProfileKind.InterfaceProfile);
        Assert.True(interfaceItem.IsBlocking);
        Assert.Contains(interfaceItem.DependencyRemaps, remap =>
            remap.DependencyKind == TemplatePackageImportDependencyKind.ExportProfile
            && remap.Resolution == TemplatePackageImportDependencyResolution.Missing);
    }

    [Fact]
    public void Apply_ShouldLetKeepExistingResolveInterfaceDependencyInDryRun()
    {
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local");
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Interface", "ais-local", "device-local", "export-imported");
        var importResult = CreateImportResult(
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile });
        var plan = CreatePlan(
            Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Imported Export",
                conflictType: TemplatePackageImportConflictType.SameIdExists,
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                existingSource: TemplatePackageImportExistingProfileSource.UserDefined,
                existingId: "export-imported",
                existingName: "Local Export"),
            Plan(ProfileKind.InterfaceProfile, "interface-imported", "Interface"));

        var updated = _service.Apply(plan, new[] { Selection(ProfileKind.ExportProfile, "export-imported", TemplatePackageImportAction.KeepExisting) });
        var dryRun = _dryRunService.Preview(importResult, updated, CreateCatalog(
            aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
            exportProfiles: new[] { CreateExportProfile("export-imported", "Local Export", "ais-local", "device-local") }));

        var interfaceItem = Assert.Single(dryRun.Items, item => item.ProfileKind == ProfileKind.InterfaceProfile);
        Assert.False(interfaceItem.IsBlocking);
        Assert.Contains(interfaceItem.DependencyRemaps, remap =>
            remap.DependencyKind == TemplatePackageImportDependencyKind.ExportProfile
            && remap.Resolution == TemplatePackageImportDependencyResolution.LocalExisting);
    }

    private static TemplatePackageImportPlan CreatePlan(params TemplatePackageImportProfilePlan[] profilePlans)
    {
        return new TemplatePackageImportPlan(
            PackageId: "package-1",
            PackageName: "Package 1",
            GeneratedAt: new DateTimeOffset(2026, 5, 9, 10, 0, 0, TimeSpan.Zero),
            ProfilePlans: profilePlans,
            HasBlockingItems: profilePlans.Any(profilePlan => profilePlan.IsBlocking),
            BlockingItems: profilePlans.Where(profilePlan => profilePlan.IsBlocking).ToList(),
            Warnings: Array.Empty<string>(),
            TotalProfiles: profilePlans.Length,
            PlannedImportAsNew: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked: profilePlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.Blocked));
    }

    private static TemplatePackageImportProfilePlan Plan(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportConflictType conflictType = TemplatePackageImportConflictType.None,
        TemplatePackageImportAction plannedAction = TemplatePackageImportAction.ImportAsNew,
        bool isBlocking = false,
        TemplatePackageImportExistingProfileSource existingSource = TemplatePackageImportExistingProfileSource.None,
        string? existingId = null,
        string? existingName = null)
    {
        return new TemplatePackageImportProfilePlan(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            ExistingProfileId: existingId,
            ExistingProfileName: existingName,
            ExistingProfileSource: existingSource,
            ConflictType: conflictType,
            PlannedAction: plannedAction,
            IsBlocking: isBlocking,
            RequiresUserDecision: plannedAction != TemplatePackageImportAction.ImportAsNew,
            RequiresRename: plannedAction == TemplatePackageImportAction.ImportAsCopy,
            ProposedProfileId: plannedAction == TemplatePackageImportAction.ImportAsCopy ? $"{importedId}-import" : importedId,
            ProposedProfileName: plannedAction == TemplatePackageImportAction.ImportAsCopy ? $"{importedName} (Import)" : importedName,
            Message: "Plan message.");
    }

    private static TemplatePackageImportUserSelection Selection(
        ProfileKind profileKind,
        string importedId,
        TemplatePackageImportAction action)
    {
        return new TemplatePackageImportUserSelection(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            SelectedAction: action,
            TargetProfileId: null,
            TargetProfileName: null,
            IsValid: true,
            ValidationMessage: null);
    }

    private static TemplatePackageImportResult CreateImportResult(
        IReadOnlyList<AisProfile>? aisProfiles = null,
        IReadOnlyList<DeviceProfileDefinition>? deviceProfiles = null,
        IReadOnlyList<ExportProfileDefinition>? exportProfiles = null,
        IReadOnlyList<InterfaceProfileDefinition>? interfaceProfiles = null)
    {
        var includedProfiles = new List<ProfileMetadata>();
        includedProfiles.AddRange((aisProfiles ?? Array.Empty<AisProfile>()).Select(profile => profile.Metadata));
        includedProfiles.AddRange((deviceProfiles ?? Array.Empty<DeviceProfileDefinition>()).Select(profile => profile.Metadata));
        includedProfiles.AddRange((exportProfiles ?? Array.Empty<ExportProfileDefinition>()).Select(profile => profile.Metadata));
        includedProfiles.AddRange((interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>()).Select(profile => profile.Metadata));

        return new TemplatePackageImportResult(
            Package: new TemplatePackage(
                Metadata: CreateMetadata("package-1", "Package 1", ProfileKind.TemplatePackage),
                IncludedProfiles: includedProfiles,
                PackageFormatVersion: "1.0",
                CreatedAt: new DateTime(2026, 5, 9, 10, 0, 0, DateTimeKind.Utc),
                CreatedBy: "Tests",
                Description: "Selection test package."),
            AisProfiles: aisProfiles ?? Array.Empty<AisProfile>(),
            DeviceProfiles: deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
    }

    private static ProfileCatalog CreateCatalog(
        IReadOnlyList<AisProfile>? aisProfiles = null,
        IReadOnlyList<DeviceProfileDefinition>? deviceProfiles = null,
        IReadOnlyList<ExportProfileDefinition>? exportProfiles = null,
        IReadOnlyList<InterfaceProfileDefinition>? interfaceProfiles = null)
    {
        return new ProfileCatalog(
            AisProfiles: aisProfiles ?? Array.Empty<AisProfile>(),
            DeviceProfiles: deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
    }

    private static AisProfile CreateAisProfile(string id, string name)
    {
        return DefaultAisProfiles.CreateMedistarDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.AisProfile),
            Name = name
        };
    }

    private static DeviceProfileDefinition CreateDeviceProfile(string id, string name)
    {
        return DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.DeviceProfile)
        };
    }

    private static ExportProfileDefinition CreateExportProfile(
        string id,
        string name,
        string aisProfileId,
        string deviceProfileId)
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.ExportProfile),
            TargetAisProfileId = aisProfileId,
            SourceDeviceProfileId = deviceProfileId
        };
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string id,
        string name,
        string aisProfileId,
        string deviceProfileId,
        string exportProfileId)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.InterfaceProfile),
            AisProfileId = aisProfileId,
            DeviceProfileId = deviceProfileId,
            ExportProfileId = exportProfileId,
            IsActive = false
        };
    }

    private static ProfileMetadata CreateMetadata(string id, string name, ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 9, 10, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "Tests",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
