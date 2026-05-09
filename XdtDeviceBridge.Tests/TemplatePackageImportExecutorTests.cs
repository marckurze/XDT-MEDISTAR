using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportExecutorTests
{
    private readonly TemplatePackageImportExecutor _executor = new();
    private readonly ProfileCatalogService _catalogService = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 9, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Execute_ShouldImportAsNewAisProfileAsUserDefined()
    {
        var paths = CreateAppDataPaths();
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS", isBuiltIn: true);

        var result = Execute(
            paths,
            CreateImportResult(aisProfiles: new[] { aisProfile }),
            Plan(ProfileKind.AisProfile, "ais-imported", "Imported AIS"),
            Item(ProfileKind.AisProfile, "ais-imported", "Imported AIS"));

        var loaded = Assert.Single(_catalogService.Load(paths).AisProfiles);
        Assert.True(result.Success);
        Assert.Equal(1, result.ImportedAsNew);
        Assert.Equal("ais-imported", loaded.Metadata.Id);
        Assert.Equal("Imported AIS", loaded.Metadata.Name);
        Assert.True(loaded.Metadata.IsUserDefined);
        Assert.False(loaded.Metadata.IsBuiltIn);
    }

    [Fact]
    public void Execute_ShouldImportAsCopyDeviceProfileWithProposedIdAndName()
    {
        var paths = CreateAppDataPaths();
        var deviceProfile = CreateDeviceProfile("device-imported", "Imported Device");

        var result = Execute(
            paths,
            CreateImportResult(deviceProfiles: new[] { deviceProfile }),
            Plan(
                ProfileKind.DeviceProfile,
                "device-imported",
                "Imported Device",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                proposedId: "device-imported-copy",
                proposedName: "Imported Device (Import)"),
            Item(
                ProfileKind.DeviceProfile,
                "device-imported",
                "Imported Device",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                targetId: "device-imported-copy",
                targetName: "Imported Device (Import)"));

        var loaded = Assert.Single(_catalogService.Load(paths).DeviceProfiles);
        Assert.True(result.Success);
        Assert.Equal(1, result.ImportedAsCopy);
        Assert.Equal("device-imported-copy", loaded.Metadata.Id);
        Assert.Equal("Imported Device (Import)", loaded.Metadata.Name);
        Assert.True(loaded.Metadata.IsUserDefined);
    }

    [Fact]
    public void Execute_ShouldNotOverwriteExistingUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var existingProfile = CreateDeviceProfile("device-existing", "Existing Device");
        _catalogService.SaveNewDeviceProfileDefinition(paths, existingProfile);

        var importedProfile = CreateDeviceProfile("device-imported", "Imported Device");
        var result = Execute(
            paths,
            CreateImportResult(deviceProfiles: new[] { importedProfile }),
            Plan(
                ProfileKind.DeviceProfile,
                "device-imported",
                "Imported Device",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                proposedId: "device-imported-copy",
                proposedName: "Imported Device (Import)"),
            Item(
                ProfileKind.DeviceProfile,
                "device-imported",
                "Imported Device",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                targetId: "device-imported-copy",
                targetName: "Imported Device (Import)"));

        var loaded = _catalogService.Load(paths).DeviceProfiles;
        Assert.True(result.Success);
        Assert.Contains(loaded, profile => profile.Metadata.Id == "device-existing" && profile.Metadata.Name == "Existing Device");
        Assert.Contains(loaded, profile => profile.Metadata.Id == "device-imported-copy" && profile.Metadata.Name == "Imported Device (Import)");
    }

    [Fact]
    public void Execute_ShouldNotOverwriteExistingBuiltInProfile()
    {
        var paths = CreateAppDataPaths();
        var builtInProfile = CreateAisProfile("ais-builtin", "BuiltIn AIS", isBuiltIn: true);
        _catalogService.SaveNewAisProfile(paths, builtInProfile);

        var importedProfile = CreateAisProfile("ais-builtin", "Imported AIS");
        var result = Execute(
            paths,
            CreateImportResult(aisProfiles: new[] { importedProfile }),
            Plan(
                ProfileKind.AisProfile,
                "ais-builtin",
                "Imported AIS",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                proposedId: "ais-builtin-import",
                proposedName: "Imported AIS (Import)"),
            Item(
                ProfileKind.AisProfile,
                "ais-builtin",
                "Imported AIS",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                targetId: "ais-builtin-import",
                targetName: "Imported AIS (Import)"));

        var loaded = _catalogService.Load(paths).AisProfiles;
        Assert.True(result.Success);
        Assert.Contains(loaded, profile => profile.Metadata.Id == "ais-builtin" && profile.Metadata.IsBuiltIn);
        Assert.Contains(loaded, profile => profile.Metadata.Id == "ais-builtin-import" && profile.Metadata.IsUserDefined);
    }

    [Fact]
    public void Execute_ShouldSkipReplaceExisting()
    {
        var paths = CreateAppDataPaths();
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", "ais", "device");

        var result = Execute(
            paths,
            CreateImportResult(exportProfiles: new[] { exportProfile }),
            Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Imported Export",
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                proposedId: "export-existing",
                proposedName: "Existing Export"),
            Item(
                ProfileKind.ExportProfile,
                "export-imported",
                "Imported Export",
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                targetId: "export-existing",
                targetName: "Existing Export",
                wouldWrite: true,
                wouldReplace: true));

        Assert.Empty(_catalogService.Load(paths).ExportProfiles);
        var skipped = Assert.Single(result.SkippedProfiles);
        Assert.Contains("ReplaceExisting", skipped.Message);
    }

    [Fact]
    public void Execute_ShouldNotWriteBlockedKeepExistingOrSkip()
    {
        var paths = CreateAppDataPaths();

        var result = Execute(
            paths,
            CreateImportResult(
                aisProfiles: new[]
                {
                    CreateAisProfile("ais-blocked", "Blocked AIS"),
                    CreateAisProfile("ais-keep", "Keep AIS"),
                    CreateAisProfile("ais-skip", "Skip AIS")
                }),
            new[]
            {
                Plan(ProfileKind.AisProfile, "ais-blocked", "Blocked AIS", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true),
                Plan(ProfileKind.AisProfile, "ais-keep", "Keep AIS", plannedAction: TemplatePackageImportAction.KeepExisting),
                Plan(ProfileKind.AisProfile, "ais-skip", "Skip AIS", plannedAction: TemplatePackageImportAction.Skip)
            },
            new[]
            {
                Item(ProfileKind.AisProfile, "ais-blocked", "Blocked AIS", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true, wouldWrite: false),
                Item(ProfileKind.AisProfile, "ais-keep", "Keep AIS", plannedAction: TemplatePackageImportAction.KeepExisting, wouldWrite: false, wouldSkip: true),
                Item(ProfileKind.AisProfile, "ais-skip", "Skip AIS", plannedAction: TemplatePackageImportAction.Skip, wouldWrite: false, wouldSkip: true)
            });

        Assert.Empty(_catalogService.Load(paths).AisProfiles);
        Assert.Equal(2, result.Skipped);
        Assert.Equal(1, result.Blocked);
    }

    [Fact]
    public void Execute_ShouldRemapImportedInterfaceProfileDependenciesAndDeactivateIt()
    {
        var paths = CreateAppDataPaths();
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");
        var deviceProfile = CreateDeviceProfile("device-imported", "Imported Device");
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", "ais-imported", "device-imported");
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-imported", "device-imported", "export-imported") with
        {
            IsActive = true
        };

        var result = Execute(
            paths,
            CreateImportResult(
                aisProfiles: new[] { aisProfile },
                deviceProfiles: new[] { deviceProfile },
                exportProfiles: new[] { exportProfile },
                interfaceProfiles: new[] { interfaceProfile }),
            new[]
            {
                Plan(ProfileKind.AisProfile, "ais-imported", "Imported AIS", proposedId: "ais-target", proposedName: "AIS Target"),
                Plan(ProfileKind.DeviceProfile, "device-imported", "Imported Device", proposedId: "device-target", proposedName: "Device Target"),
                Plan(ProfileKind.ExportProfile, "export-imported", "Imported Export", proposedId: "export-target", proposedName: "Export Target"),
                Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface", proposedId: "interface-target", proposedName: "Interface Target")
            },
            new[]
            {
                Item(ProfileKind.AisProfile, "ais-imported", "Imported AIS", targetId: "ais-target", targetName: "AIS Target"),
                Item(ProfileKind.DeviceProfile, "device-imported", "Imported Device", targetId: "device-target", targetName: "Device Target"),
                Item(ProfileKind.ExportProfile, "export-imported", "Imported Export", targetId: "export-target", targetName: "Export Target"),
                Item(
                    ProfileKind.InterfaceProfile,
                    "interface-imported",
                    "Imported Interface",
                    targetId: "interface-target",
                    targetName: "Interface Target",
                    dependencyRemaps: new[]
                    {
                        Remap(TemplatePackageImportDependencyKind.AisProfile, "ais-imported", "ais-target", "AIS Target", TemplatePackageImportDependencyResolution.ImportedAsNew),
                        Remap(TemplatePackageImportDependencyKind.DeviceProfile, "device-imported", "device-target", "Device Target", TemplatePackageImportDependencyResolution.ImportedAsNew),
                        Remap(TemplatePackageImportDependencyKind.ExportProfile, "export-imported", "export-target", "Export Target", TemplatePackageImportDependencyResolution.ImportedAsNew)
                    })
            });

        var loadedInterface = Assert.Single(_catalogService.Load(paths).InterfaceProfiles);
        Assert.True(result.Success);
        Assert.Equal("ais-target", loadedInterface.AisProfileId);
        Assert.Equal("device-target", loadedInterface.DeviceProfileId);
        Assert.Equal("export-target", loadedInterface.ExportProfileId);
        Assert.False(loadedInterface.IsActive);
        Assert.True(loadedInterface.Metadata.IsUserDefined);
    }

    [Fact]
    public void Execute_ShouldKeepAttachmentSettingsButDisableAttachmentProcessing()
    {
        var paths = CreateAppDataPaths();
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-local", "device-local", "export-local") with
        {
            IsActive = true,
            FolderOptions = CreateFolderOptions() with
            {
                AttachmentImportFolder = @"C:\XdtBridge\AttachmentIn",
                AttachmentExportFolder = @"C:\XdtBridge\AttachmentOut",
                IsAttachmentProcessingEnabled = true,
                AttachmentExternalLinkDescription = "Beschreibung"
            }
        };

        var result = Execute(
            paths,
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface"),
            Item(
                ProfileKind.InterfaceProfile,
                "interface-imported",
                "Imported Interface",
                dependencyRemaps: new[]
                {
                    Remap(TemplatePackageImportDependencyKind.AisProfile, "ais-local", "ais-local", "Local AIS", TemplatePackageImportDependencyResolution.LocalExisting),
                    Remap(TemplatePackageImportDependencyKind.DeviceProfile, "device-local", "device-local", "Local Device", TemplatePackageImportDependencyResolution.LocalExisting),
                    Remap(TemplatePackageImportDependencyKind.ExportProfile, "export-local", "export-local", "Local Export", TemplatePackageImportDependencyResolution.LocalExisting)
                }));

        var loadedInterface = Assert.Single(_catalogService.Load(paths).InterfaceProfiles);
        Assert.Equal(@"C:\XdtBridge\AttachmentIn", loadedInterface.FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"C:\XdtBridge\AttachmentOut", loadedInterface.FolderOptions.AttachmentExportFolder);
        Assert.Equal("Beschreibung", loadedInterface.FolderOptions.AttachmentExternalLinkDescription);
        Assert.False(loadedInterface.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Contains(result.Warnings, warning => warning.Contains("XDT attachment settings", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Execute_ShouldBlockUnsafeTargetProfileId()
    {
        var paths = CreateAppDataPaths();
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");

        var result = Execute(
            paths,
            CreateImportResult(aisProfiles: new[] { aisProfile }),
            Plan(ProfileKind.AisProfile, "ais-imported", "Imported AIS", proposedId: @"..\evil", proposedName: "Evil"),
            Item(ProfileKind.AisProfile, "ais-imported", "Imported AIS", targetId: @"..\evil", targetName: "Evil"));

        Assert.Empty(_catalogService.Load(paths).AisProfiles);
        Assert.Equal(1, result.Blocked);
        Assert.Contains(result.BlockedProfiles, item => item.Message.Contains("safe file name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Execute_ShouldReportCounts()
    {
        var paths = CreateAppDataPaths();

        var result = Execute(
            paths,
            CreateImportResult(
                aisProfiles: new[]
                {
                    CreateAisProfile("ais-new", "New AIS"),
                    CreateAisProfile("ais-copy", "Copy AIS"),
                    CreateAisProfile("ais-skip", "Skip AIS"),
                    CreateAisProfile("ais-blocked", "Blocked AIS")
                }),
            new[]
            {
                Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Plan(ProfileKind.AisProfile, "ais-copy", "Copy AIS", plannedAction: TemplatePackageImportAction.ImportAsCopy, proposedId: "ais-copy-import", proposedName: "Copy AIS (Import)"),
                Plan(ProfileKind.AisProfile, "ais-skip", "Skip AIS", plannedAction: TemplatePackageImportAction.Skip),
                Plan(ProfileKind.AisProfile, "ais-blocked", "Blocked AIS", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true)
            },
            new[]
            {
                Item(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Item(ProfileKind.AisProfile, "ais-copy", "Copy AIS", plannedAction: TemplatePackageImportAction.ImportAsCopy, targetId: "ais-copy-import", targetName: "Copy AIS (Import)"),
                Item(ProfileKind.AisProfile, "ais-skip", "Skip AIS", plannedAction: TemplatePackageImportAction.Skip, wouldWrite: false, wouldSkip: true),
                Item(ProfileKind.AisProfile, "ais-blocked", "Blocked AIS", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true, wouldWrite: false)
            });

        Assert.Equal(1, result.ImportedAsNew);
        Assert.Equal(1, result.ImportedAsCopy);
        Assert.Equal(1, result.Skipped);
        Assert.Equal(1, result.Blocked);
        Assert.Equal(0, result.Failed);
    }

    private TemplatePackageImportExecutionResult Execute(
        AppDataPaths paths,
        TemplatePackageImportResult importResult,
        TemplatePackageImportProfilePlan plan,
        TemplatePackageImportDryRunItem item)
    {
        return Execute(paths, importResult, new[] { plan }, new[] { item });
    }

    private TemplatePackageImportExecutionResult Execute(
        AppDataPaths paths,
        TemplatePackageImportResult importResult,
        IReadOnlyList<TemplatePackageImportProfilePlan> plans,
        IReadOnlyList<TemplatePackageImportDryRunItem> items)
    {
        return _executor.Execute(
            importResult,
            CreatePlan(plans),
            CreateDryRun(items),
            paths,
            _timestamp,
            "Tests");
    }

    private static TemplatePackageImportPlan CreatePlan(IReadOnlyList<TemplatePackageImportProfilePlan> profilePlans)
    {
        return new TemplatePackageImportPlan(
            PackageId: "package-1",
            PackageName: "Package 1",
            GeneratedAt: new DateTimeOffset(2026, 5, 9, 10, 0, 0, TimeSpan.Zero),
            ProfilePlans: profilePlans,
            HasBlockingItems: profilePlans.Any(plan => plan.IsBlocking),
            BlockingItems: profilePlans.Where(plan => plan.IsBlocking).ToList(),
            Warnings: Array.Empty<string>(),
            TotalProfiles: profilePlans.Count,
            PlannedImportAsNew: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Blocked));
    }

    private static TemplatePackageImportDryRunResult CreateDryRun(IReadOnlyList<TemplatePackageImportDryRunItem> items)
    {
        return new TemplatePackageImportDryRunResult(
            Success: true,
            PackageId: "package-1",
            PackageName: "Package 1",
            Items: items,
            BlockingItems: items.Where(item => item.IsBlocking).ToList(),
            Warnings: items.SelectMany(item => item.DependencyRemapWarnings).ToList(),
            ErrorMessage: null,
            TotalItems: items.Count,
            WouldImportAsNew: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            WouldImportAsCopy: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            WouldReplaceExisting: items.Count(item => item.WouldReplace),
            WouldKeepExisting: items.Count(item => item.WouldKeepExisting),
            WouldSkip: items.Count(item => item.WouldSkip),
            WouldBlock: items.Count(item => item.IsBlocking));
    }

    private static TemplatePackageImportProfilePlan Plan(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportAction plannedAction = TemplatePackageImportAction.ImportAsNew,
        bool isBlocking = false,
        string? proposedId = null,
        string? proposedName = null)
    {
        return new TemplatePackageImportProfilePlan(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            ExistingProfileId: null,
            ExistingProfileName: null,
            ExistingProfileSource: TemplatePackageImportExistingProfileSource.None,
            ConflictType: plannedAction == TemplatePackageImportAction.Blocked ? TemplatePackageImportConflictType.MissingDependency : TemplatePackageImportConflictType.None,
            PlannedAction: plannedAction,
            IsBlocking: isBlocking,
            RequiresUserDecision: plannedAction != TemplatePackageImportAction.ImportAsNew,
            RequiresRename: plannedAction == TemplatePackageImportAction.ImportAsCopy,
            ProposedProfileId: proposedId ?? importedId,
            ProposedProfileName: proposedName ?? importedName,
            Message: "Plan message.");
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
        bool wouldSkip = false,
        IReadOnlyList<TemplatePackageImportDependencyRemap>? dependencyRemaps = null)
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
            WouldKeepExisting: plannedAction == TemplatePackageImportAction.KeepExisting,
            WouldSkip: wouldSkip,
            RequiresDependencyRemap: dependencyRemaps?.Any(remap => remap.Resolution == TemplatePackageImportDependencyResolution.ImportedAsCopy) ?? false,
            DependencyRemaps: dependencyRemaps ?? Array.Empty<TemplatePackageImportDependencyRemap>(),
            DependencyRemapWarnings: profileKind == ProfileKind.InterfaceProfile
                ? new[] { "Imported interface profile contains XDT attachment settings; folder paths and 6302/6303/6304/6305 must be reviewed before activation." }
                : Array.Empty<string>(),
            Message: "Dry-run message.");
    }

    private static TemplatePackageImportDependencyRemap Remap(
        TemplatePackageImportDependencyKind dependencyKind,
        string originalId,
        string targetId,
        string targetName,
        TemplatePackageImportDependencyResolution resolution)
    {
        return new TemplatePackageImportDependencyRemap(
            DependencyKind: dependencyKind,
            OriginalProfileId: originalId,
            OriginalProfileName: originalId,
            TargetProfileId: targetId,
            TargetProfileName: targetName,
            Resolution: resolution,
            Message: "Dependency remapped.");
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
                Description: "Execution test package."),
            AisProfiles: aisProfiles ?? Array.Empty<AisProfile>(),
            DeviceProfiles: deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static AisProfile CreateAisProfile(string id, string name, bool isBuiltIn = false)
    {
        return DefaultAisProfiles.CreateMedistarDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.AisProfile, isBuiltIn),
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
            FolderOptions = CreateFolderOptions(),
            IsActive = false
        };
    }

    private static InterfaceFolderOptions CreateFolderOptions()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
        {
            AisImportFolder = "",
            DeviceImportFolder = "",
            ExportFolder = "",
            ArchiveFolder = "",
            ErrorFolder = "",
            AttachmentImportFolder = "",
            AttachmentExportFolder = "",
            MoveFailedFilesToErrorFolder = false
        };
    }

    private static ProfileMetadata CreateMetadata(
        string id,
        string name,
        ProfileKind profileKind,
        bool isBuiltIn = false)
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
            IsBuiltIn: isBuiltIn,
            IsUserDefined: !isBuiltIn);
    }
}
