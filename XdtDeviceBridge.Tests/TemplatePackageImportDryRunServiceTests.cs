using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportDryRunServiceTests
{
    private readonly TemplatePackageImportDryRunService _service = new();

    [Fact]
    public void Preview_ShouldDescribeImportAsNew()
    {
        var aisProfile = CreateAisProfile("ais-new", "New AIS");

        var result = _service.Preview(
            CreateImportResult(aisProfiles: new[] { aisProfile }),
            CreatePlan(Plan(ProfileKind.AisProfile, "ais-new", "New AIS")),
            CreateCatalog());

        var item = Assert.Single(result.Items);
        Assert.True(result.Success);
        Assert.True(item.WouldWrite);
        Assert.False(item.WouldReplace);
        Assert.False(item.WouldSkip);
        Assert.Equal("ais-new", item.TargetProfileId);
        Assert.Equal("New AIS", item.TargetProfileName);
        Assert.Equal(1, result.WouldImportAsNew);
    }

    [Fact]
    public void Preview_ShouldUseProposedIdAndNameForImportAsCopy()
    {
        var deviceProfile = CreateDeviceProfile("device-imported", "Imported Device");

        var result = _service.Preview(
            CreateImportResult(deviceProfiles: new[] { deviceProfile }),
            CreatePlan(Plan(
                ProfileKind.DeviceProfile,
                "device-imported",
                "Imported Device",
                plannedAction: TemplatePackageImportAction.ImportAsCopy,
                proposedId: "device-imported-copy",
                proposedName: "Imported Device (Import)")),
            CreateCatalog());

        var item = Assert.Single(result.Items);
        Assert.True(item.WouldWrite);
        Assert.False(item.WouldReplace);
        Assert.Equal("device-imported-copy", item.TargetProfileId);
        Assert.Equal("Imported Device (Import)", item.TargetProfileName);
        Assert.Equal(1, result.WouldImportAsCopy);
    }

    [Fact]
    public void Preview_ShouldBlockBlockedItem()
    {
        var result = _service.Preview(
            CreateImportResult(aisProfiles: new[] { CreateAisProfile("ais-blocked", "Blocked AIS") }),
            CreatePlan(Plan(
                ProfileKind.AisProfile,
                "ais-blocked",
                "Blocked AIS",
                plannedAction: TemplatePackageImportAction.Blocked,
                isBlocking: true)),
            CreateCatalog());

        var item = Assert.Single(result.Items);
        Assert.True(item.IsBlocking);
        Assert.False(item.WouldWrite);
        Assert.Single(result.BlockingItems);
        Assert.Equal(1, result.WouldBlock);
    }

    [Fact]
    public void Preview_ShouldDescribeKeepExistingAndSkip()
    {
        var result = _service.Preview(
            CreateImportResult(
                aisProfiles: new[]
                {
                    CreateAisProfile("ais-keep", "Keep AIS"),
                    CreateAisProfile("ais-skip", "Skip AIS")
                }),
            CreatePlan(
                Plan(
                    ProfileKind.AisProfile,
                    "ais-keep",
                    "Keep AIS",
                    plannedAction: TemplatePackageImportAction.KeepExisting,
                    existingId: "ais-local",
                    existingName: "Local AIS"),
                Plan(
                    ProfileKind.AisProfile,
                    "ais-skip",
                    "Skip AIS",
                    plannedAction: TemplatePackageImportAction.Skip)),
            CreateCatalog());

        var keep = Assert.Single(result.Items, item => item.ImportedProfileId == "ais-keep");
        var skip = Assert.Single(result.Items, item => item.ImportedProfileId == "ais-skip");
        Assert.True(keep.WouldKeepExisting);
        Assert.True(keep.WouldSkip);
        Assert.True(skip.WouldSkip);
        Assert.False(keep.WouldWrite);
        Assert.False(skip.WouldWrite);
        Assert.Equal(1, result.WouldKeepExisting);
        Assert.Equal(2, result.WouldSkip);
    }

    [Fact]
    public void Preview_ShouldDescribeReplaceExistingForUserDefined()
    {
        var result = _service.Preview(
            CreateImportResult(exportProfiles: new[] { CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local") }),
            CreatePlan(Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Imported Export",
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                existingId: "export-local",
                existingName: "Local Export",
                existingSource: TemplatePackageImportExistingProfileSource.UserDefined)),
            CreateCatalog());

        var item = Assert.Single(result.Items);
        Assert.True(item.WouldWrite);
        Assert.True(item.WouldReplace);
        Assert.False(item.IsBlocking);
        Assert.Equal("export-local", item.TargetProfileId);
        Assert.Equal(1, result.WouldReplaceExisting);
    }

    [Fact]
    public void Preview_ShouldBlockReplaceExistingForBuiltIn()
    {
        var result = _service.Preview(
            CreateImportResult(exportProfiles: new[] { CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local") }),
            CreatePlan(Plan(
                ProfileKind.ExportProfile,
                "export-imported",
                "Imported Export",
                plannedAction: TemplatePackageImportAction.ReplaceExisting,
                existingId: "export-builtin",
                existingName: "BuiltIn Export",
                existingSource: TemplatePackageImportExistingProfileSource.BuiltIn)),
            CreateCatalog());

        var item = Assert.Single(result.Items);
        Assert.True(item.IsBlocking);
        Assert.False(item.WouldWrite);
        Assert.False(item.WouldReplace);
        Assert.Contains("BuiltIn", item.Message);
    }

    [Fact]
    public void Preview_ShouldResolveInterfaceDependencyFromLocalCatalog()
    {
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-local", "device-local", "export-local");

        var result = _service.Preview(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-local", "device-local") }));

        var item = Assert.Single(result.Items);
        Assert.All(item.DependencyRemaps, remap => Assert.Equal(TemplatePackageImportDependencyResolution.LocalExisting, remap.Resolution));
        Assert.False(item.RequiresDependencyRemap);
    }

    [Fact]
    public void Preview_ShouldResolveInterfaceDependencyImportedAsNew()
    {
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-imported", "device-local", "export-local");

        var result = _service.Preview(
            CreateImportResult(
                aisProfiles: new[] { aisProfile },
                interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(
                Plan(ProfileKind.AisProfile, "ais-imported", "Imported AIS"),
                Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-imported", "device-local") }));

        var item = Assert.Single(result.Items, item => item.ProfileKind == ProfileKind.InterfaceProfile);
        var remap = Assert.Single(item.DependencyRemaps, remap => remap.DependencyKind == TemplatePackageImportDependencyKind.AisProfile);
        Assert.Equal(TemplatePackageImportDependencyResolution.ImportedAsNew, remap.Resolution);
        Assert.Equal("ais-imported", remap.TargetProfileId);
    }

    [Fact]
    public void Preview_ShouldResolveInterfaceDependencyImportedAsCopy()
    {
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-imported", "device-local", "export-local");

        var result = _service.Preview(
            CreateImportResult(
                aisProfiles: new[] { aisProfile },
                interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(
                Plan(
                    ProfileKind.AisProfile,
                    "ais-imported",
                    "Imported AIS",
                    plannedAction: TemplatePackageImportAction.ImportAsCopy,
                    proposedId: "ais-imported-copy",
                    proposedName: "Imported AIS (Import)"),
                Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-imported", "device-local") }));

        var item = Assert.Single(result.Items, item => item.ProfileKind == ProfileKind.InterfaceProfile);
        var remap = Assert.Single(item.DependencyRemaps, remap => remap.DependencyKind == TemplatePackageImportDependencyKind.AisProfile);
        Assert.Equal(TemplatePackageImportDependencyResolution.ImportedAsCopy, remap.Resolution);
        Assert.Equal("ais-imported-copy", remap.TargetProfileId);
        Assert.True(item.RequiresDependencyRemap);
        Assert.Contains(item.DependencyRemapWarnings, warning => warning.Contains("remapped", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Preview_ShouldBlockInterfaceWhenDependencyIsMissing()
    {
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "missing-ais", "device-local", "export-local");

        var result = _service.Preview(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "missing-ais", "device-local") }));

        var item = Assert.Single(result.Items);
        var remap = Assert.Single(item.DependencyRemaps, remap => remap.DependencyKind == TemplatePackageImportDependencyKind.AisProfile);
        Assert.Equal(TemplatePackageImportDependencyResolution.Missing, remap.Resolution);
        Assert.True(item.IsBlocking);
        Assert.False(item.WouldWrite);
    }

    [Fact]
    public void Preview_ShouldWarnForAttachmentSettings()
    {
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-local", "device-local", "export-local") with
        {
            FolderOptions = CreateFolderOptions() with
            {
                AttachmentImportFolder = @"C:\XdtBridge\AttachmentIn",
                AttachmentExportFolder = @"C:\XdtBridge\AttachmentOut",
                AttachmentExternalLinkDescription = "Beschreibung"
            }
        };

        var result = _service.Preview(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-local", "device-local") }));

        var item = Assert.Single(result.Items);
        Assert.Contains(item.DependencyRemapWarnings, warning => warning.Contains("6302/6303/6304/6305", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("XDT attachment settings", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Preview_ShouldWarnForActiveInterfaceProfile()
    {
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", "ais-local", "device-local", "export-local") with
        {
            IsActive = true
        };

        var result = _service.Preview(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreatePlan(Plan(ProfileKind.InterfaceProfile, "interface-imported", "Imported Interface")),
            CreateCatalog(
                aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
                deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
                exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-local", "device-local") }));

        var item = Assert.Single(result.Items);
        Assert.Contains(item.DependencyRemapWarnings, warning => warning.Contains("will not be activated automatically", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Preview_ShouldCalculateCounts()
    {
        var result = _service.Preview(
            CreateImportResult(
                aisProfiles: new[]
                {
                    CreateAisProfile("ais-new", "New AIS"),
                    CreateAisProfile("ais-copy", "Copy AIS"),
                    CreateAisProfile("ais-skip", "Skip AIS"),
                    CreateAisProfile("ais-block", "Block AIS")
                }),
            CreatePlan(
                Plan(ProfileKind.AisProfile, "ais-new", "New AIS"),
                Plan(ProfileKind.AisProfile, "ais-copy", "Copy AIS", plannedAction: TemplatePackageImportAction.ImportAsCopy, proposedId: "ais-copy-import", proposedName: "Copy AIS (Import)"),
                Plan(ProfileKind.AisProfile, "ais-skip", "Skip AIS", plannedAction: TemplatePackageImportAction.Skip),
                Plan(ProfileKind.AisProfile, "ais-block", "Block AIS", plannedAction: TemplatePackageImportAction.Blocked, isBlocking: true)),
            CreateCatalog());

        Assert.Equal(4, result.TotalItems);
        Assert.Equal(1, result.WouldImportAsNew);
        Assert.Equal(1, result.WouldImportAsCopy);
        Assert.Equal(0, result.WouldReplaceExisting);
        Assert.Equal(0, result.WouldKeepExisting);
        Assert.Equal(1, result.WouldSkip);
        Assert.Equal(1, result.WouldBlock);
    }

    [Fact]
    public void Preview_ShouldNotWriteFiles()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        _service.Preview(
            CreateImportResult(aisProfiles: new[] { CreateAisProfile("ais-new", "New AIS") }),
            CreatePlan(Plan(ProfileKind.AisProfile, "ais-new", "New AIS")),
            CreateCatalog());

        Assert.Empty(Directory.EnumerateFileSystemEntries(folder));
    }

    private static TemplatePackageImportPlan CreatePlan(params TemplatePackageImportProfilePlan[] profilePlans)
    {
        return new TemplatePackageImportPlan(
            PackageId: "package-1",
            PackageName: "Package 1",
            GeneratedAt: new DateTimeOffset(2026, 5, 8, 15, 0, 0, TimeSpan.Zero),
            ProfilePlans: profilePlans,
            HasBlockingItems: profilePlans.Any(plan => plan.IsBlocking),
            BlockingItems: profilePlans.Where(plan => plan.IsBlocking).ToList(),
            Warnings: Array.Empty<string>(),
            TotalProfiles: profilePlans.Length,
            PlannedImportAsNew: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked: profilePlans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Blocked));
    }

    private static TemplatePackageImportProfilePlan Plan(
        ProfileKind profileKind,
        string importedId,
        string importedName,
        TemplatePackageImportAction plannedAction = TemplatePackageImportAction.ImportAsNew,
        bool isBlocking = false,
        string? existingId = null,
        string? existingName = null,
        TemplatePackageImportExistingProfileSource existingSource = TemplatePackageImportExistingProfileSource.None,
        string? proposedId = null,
        string? proposedName = null)
    {
        return new TemplatePackageImportProfilePlan(
            ProfileKind: profileKind,
            ImportedProfileId: importedId,
            ImportedProfileName: importedName,
            ExistingProfileId: existingId,
            ExistingProfileName: existingName,
            ExistingProfileSource: existingSource,
            ConflictType: plannedAction == TemplatePackageImportAction.Blocked ? TemplatePackageImportConflictType.MissingDependency : TemplatePackageImportConflictType.None,
            PlannedAction: plannedAction,
            IsBlocking: isBlocking,
            RequiresUserDecision: plannedAction != TemplatePackageImportAction.ImportAsNew,
            RequiresRename: plannedAction == TemplatePackageImportAction.ImportAsCopy,
            ProposedProfileId: proposedId ?? (plannedAction == TemplatePackageImportAction.ImportAsNew ? importedId : null),
            ProposedProfileName: proposedName ?? (plannedAction == TemplatePackageImportAction.ImportAsNew ? importedName : null),
            Message: "Plan message.");
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
                CreatedAt: new DateTime(2026, 5, 8, 15, 0, 0, DateTimeKind.Utc),
                CreatedBy: "Tests",
                Description: "Dry-run test package."),
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
            IsActive = false,
            FolderOptions = CreateFolderOptions()
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

    private static ProfileMetadata CreateMetadata(string id, string name, ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 8, 15, 0, 0, TimeSpan.Zero);

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
