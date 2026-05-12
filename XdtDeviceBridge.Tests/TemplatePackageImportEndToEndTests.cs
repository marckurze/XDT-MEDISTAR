using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportEndToEndTests
{
    private readonly ProfileCatalogService _catalogService = new();
    private readonly TemplatePackageExporter _exporter = new();
    private readonly TemplatePackageImporter _importer = new();
    private readonly TemplatePackageImportValidator _validator = new();
    private readonly TemplatePackageImportConflictAnalyzer _analyzer = new();
    private readonly TemplatePackageImportPlanBuilder _planBuilder = new();
    private readonly TemplatePackageImportSelectionService _selectionService = new();
    private readonly TemplatePackageImportDryRunService _dryRunService = new();
    private readonly TemplatePackageImportExecutor _executor = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 9, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FullFlow_ShouldImportExportedConflictFreePackageAsUserDefined()
    {
        var paths = CreateAppDataPaths();
        var workspace = CreateWorkspaceFolder();
        var aisProfile = CreateAisProfile("ais-e2e", "E2E AIS");
        var deviceProfile = CreateDeviceProfile("device-e2e", "E2E Device");
        var exportProfile = CreateExportProfile("export-e2e", "E2E Export", "ais-e2e", "device-e2e");
        var interfaceProfile = CreateInterfaceProfile("interface-e2e", "E2E Interface", "ais-e2e", "device-e2e", "export-e2e", workspace) with
        {
            IsActive = true,
            FolderOptions = CreateFolderOptions(workspace) with
            {
                IsAttachmentProcessingEnabled = true,
                AttachmentImportFolder = Path.Combine(workspace, "attachment-in"),
                AttachmentExportFolder = Path.Combine(workspace, "attachment-out"),
                AttachmentExternalLinkDescription = "E2E-Anhang"
            }
        };
        var importResult = ExportAndImport(
            workspace,
            aisProfiles: new[] { aisProfile },
            deviceProfiles: new[] { deviceProfile },
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile });

        var result = RunImportFlow(paths, importResult);

        Assert.DoesNotContain(result.Validation.Issues, issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error);
        Assert.True(result.Execution.Success);
        Assert.Equal(4, result.Execution.ImportedAsNew);
        Assert.Equal(0, result.Execution.ImportedAsCopy);
        Assert.Equal(4, result.Execution.ImportedProfiles.Count);

        Assert.Contains(result.LoadedCatalog.AisProfiles, profile => profile.Metadata.Id == "ais-e2e" && profile.Metadata.IsUserDefined);
        Assert.Contains(result.LoadedCatalog.DeviceProfiles, profile => profile.Metadata.Id == "device-e2e" && profile.Metadata.IsUserDefined);
        Assert.Contains(result.LoadedCatalog.ExportProfiles, profile => profile.Metadata.Id == "export-e2e" && profile.Metadata.IsUserDefined);

        var importedInterface = Assert.Single(result.LoadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-e2e");
        Assert.True(importedInterface.Metadata.IsUserDefined);
        Assert.False(importedInterface.Metadata.IsBuiltIn);
        Assert.False(importedInterface.IsActive);
        Assert.False(importedInterface.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal("ais-e2e", importedInterface.AisProfileId);
        Assert.Equal("device-e2e", importedInterface.DeviceProfileId);
        Assert.Equal("export-e2e", importedInterface.ExportProfileId);
    }

    [Fact]
    public void FullFlow_ShouldProtectBuiltInProfileAndImportCopy()
    {
        var paths = CreateAppDataPaths();
        var builtInProfile = CreateAisProfile("ais-builtin", "BuiltIn AIS", isBuiltIn: true);
        _catalogService.SaveNewAisProfile(paths, builtInProfile);
        var importResult = CreateImportResult(aisProfiles: new[] { CreateAisProfile("ais-builtin", "BuiltIn AIS") });

        var result = RunImportFlow(
            paths,
            importResult,
            new[] { Selection(ProfileKind.AisProfile, "ais-builtin", TemplatePackageImportAction.ImportAsCopy) });

        var plan = Assert.Single(result.Plan.ProfilePlans);
        Assert.Equal(TemplatePackageImportAction.ImportAsCopy, plan.PlannedAction);
        Assert.NotEqual(TemplatePackageImportAction.ReplaceExisting, plan.PlannedAction);
        Assert.Equal(TemplatePackageImportExistingProfileSource.BuiltIn, plan.ExistingProfileSource);

        var loadedProfiles = result.LoadedCatalog.AisProfiles;
        Assert.Contains(loadedProfiles, profile => profile.Metadata.Id == "ais-builtin" && profile.Metadata.IsBuiltIn && profile.Metadata.Name == "BuiltIn AIS");
        Assert.Contains(loadedProfiles, profile => profile.Metadata.Id == "ais-builtin-import" && profile.Metadata.IsUserDefined && profile.Metadata.Name == "BuiltIn AIS (Import)");
        Assert.Equal(1, result.Execution.ImportedAsCopy);
    }

    [Fact]
    public void FullFlow_ShouldImportUserDefinedConflictAsCopyWithoutChangingExistingProfile()
    {
        var paths = CreateAppDataPaths();
        var existingProfile = CreateDeviceProfile("device-conflict", "Device Conflict");
        _catalogService.SaveNewDeviceProfileDefinition(paths, existingProfile);
        var importResult = CreateImportResult(deviceProfiles: new[] { CreateDeviceProfile("device-conflict", "Device Conflict") });

        var result = RunImportFlow(
            paths,
            importResult,
            new[] { Selection(ProfileKind.DeviceProfile, "device-conflict", TemplatePackageImportAction.ImportAsCopy) });

        var loadedProfiles = result.LoadedCatalog.DeviceProfiles;
        Assert.Contains(loadedProfiles, profile => profile.Metadata.Id == "device-conflict" && profile.Metadata.Name == "Device Conflict");
        Assert.Contains(loadedProfiles, profile => profile.Metadata.Id == "device-conflict-import" && profile.Metadata.Name == "Device Conflict (Import)" && profile.Metadata.IsUserDefined);
        Assert.Equal(1, result.Execution.ImportedAsCopy);
    }

    [Fact]
    public void FullFlow_ShouldRespectKeepExistingAndRemapInterfaceDependencyToLocalProfile()
    {
        var paths = CreateAppDataPaths();
        var workspace = CreateWorkspaceFolder();
        _catalogService.SaveNewExportProfile(paths, CreateExportProfile("export-shared", "Shared Export", "ais-package", "device-package"));
        var importResult = CreateImportResult(
            aisProfiles: new[] { CreateAisProfile("ais-package", "Package AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-package", "Package Device") },
            exportProfiles: new[] { CreateExportProfile("export-shared", "Shared Export", "ais-package", "device-package") },
            interfaceProfiles: new[] { CreateInterfaceProfile("interface-package", "Package Interface", "ais-package", "device-package", "export-shared", workspace) });

        var result = RunImportFlow(
            paths,
            importResult,
            new[] { Selection(ProfileKind.ExportProfile, "export-shared", TemplatePackageImportAction.KeepExisting) });

        Assert.Equal(3, result.Execution.ImportedProfiles.Count);
        Assert.Equal(1, result.Execution.Skipped);
        Assert.Single(result.LoadedCatalog.ExportProfiles, profile => profile.Metadata.Id == "export-shared");

        var importedInterface = Assert.Single(result.LoadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-package");
        Assert.Equal("ais-package", importedInterface.AisProfileId);
        Assert.Equal("device-package", importedInterface.DeviceProfileId);
        Assert.Equal("export-shared", importedInterface.ExportProfileId);
        Assert.Contains(result.DryRun.Items.Single(item => item.ProfileKind == ProfileKind.InterfaceProfile).DependencyRemaps, remap =>
            remap.DependencyKind == TemplatePackageImportDependencyKind.ExportProfile
            && remap.Resolution == TemplatePackageImportDependencyResolution.LocalExisting);
    }

    [Fact]
    public void FullFlow_ShouldBlockInterfaceProfileWhenUserSkipsRequiredDependency()
    {
        var paths = CreateAppDataPaths();
        var workspace = CreateWorkspaceFolder();
        var importResult = CreateImportResult(
            aisProfiles: new[] { CreateAisProfile("ais-package", "Package AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-package", "Package Device") },
            exportProfiles: new[] { CreateExportProfile("export-needed", "Needed Export", "ais-package", "device-package") },
            interfaceProfiles: new[] { CreateInterfaceProfile("interface-package", "Package Interface", "ais-package", "device-package", "export-needed", workspace) });

        var result = RunImportFlow(
            paths,
            importResult,
            new[] { Selection(ProfileKind.ExportProfile, "export-needed", TemplatePackageImportAction.Skip) });

        Assert.DoesNotContain(result.LoadedCatalog.ExportProfiles, profile => profile.Metadata.Id == "export-needed");
        Assert.DoesNotContain(result.LoadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-package");
        Assert.Contains(result.DryRun.BlockingItems, item => item.ProfileKind == ProfileKind.InterfaceProfile && item.ImportedProfileId == "interface-package");
        Assert.Contains(result.Execution.BlockedProfiles, item => item.ProfileKind == ProfileKind.InterfaceProfile && item.SourceProfileId == "interface-package");
    }

    [Fact]
    public void FullFlow_ShouldKeepXdtAttachmentSettingsButDisableImportedInterfaceAutomation()
    {
        var paths = CreateAppDataPaths();
        var workspace = CreateWorkspaceFolder();
        var interfaceProfile = CreateInterfaceProfile("interface-attachment", "Attachment Interface", "ais-attachment", "device-attachment", "export-attachment", workspace) with
        {
            IsActive = true,
            FolderOptions = CreateFolderOptions(workspace) with
            {
                AttachmentImportFolder = Path.Combine(workspace, "attachment-in"),
                AttachmentExportFolder = Path.Combine(workspace, "attachment-out"),
                AttachmentFileNameTemplate = "{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}",
                AttachmentTransferMode = AttachmentTransferMode.Move,
                AttachmentExternalLinkDocumentName = "Datei",
                AttachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
                AttachmentExternalLinkDescription = "Test M.Kurze",
                AttachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}",
                IsAttachmentProcessingEnabled = true
            }
        };
        var importResult = CreateImportResult(
            aisProfiles: new[] { CreateAisProfile("ais-attachment", "Attachment AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-attachment", "Attachment Device") },
            exportProfiles: new[] { CreateExportProfile("export-attachment", "Attachment Export", "ais-attachment", "device-attachment") },
            interfaceProfiles: new[] { interfaceProfile });

        var result = RunImportFlow(paths, importResult);

        var importedInterface = Assert.Single(result.LoadedCatalog.InterfaceProfiles, profile => profile.Metadata.Id == "interface-attachment");
        Assert.False(importedInterface.IsActive);
        Assert.False(importedInterface.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(Path.Combine(workspace, "attachment-in"), importedInterface.FolderOptions.AttachmentImportFolder);
        Assert.Equal(Path.Combine(workspace, "attachment-out"), importedInterface.FolderOptions.AttachmentExportFolder);
        Assert.Equal("{Ais.PatientNumber}_{Date:ddMMyyyy}_{Time:HHmmss}{ExtensionUpper}", importedInterface.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, importedInterface.FolderOptions.AttachmentTransferMode);
        Assert.Equal("Datei", importedInterface.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", importedInterface.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal("Test M.Kurze", importedInterface.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", importedInterface.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.Contains(result.Execution.Warnings, warning => warning.Contains("XDT-Anhang-Einstellungen", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Execution.Warnings, warning => warning.Contains("inaktiv importiert", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Execute_ShouldNotRunManipulatedReplaceExistingPlan()
    {
        var paths = CreateAppDataPaths();
        var existingProfile = CreateExportProfile("export-existing", "Existing Export", "ais-local", "device-local");
        _catalogService.SaveNewExportProfile(paths, existingProfile);
        var importResult = CreateImportResult(exportProfiles: new[] { CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local") });
        var existingCatalog = _catalogService.Load(paths);
        var plan = CreatePlan(new[]
        {
            new TemplatePackageImportProfilePlan(
                ProfileKind: ProfileKind.ExportProfile,
                ImportedProfileId: "export-imported",
                ImportedProfileName: "Imported Export",
                ExistingProfileId: "export-existing",
                ExistingProfileName: "Existing Export",
                ExistingProfileSource: TemplatePackageImportExistingProfileSource.UserDefined,
                ConflictType: TemplatePackageImportConflictType.SameIdExists,
                PlannedAction: TemplatePackageImportAction.ReplaceExisting,
                IsBlocking: false,
                RequiresUserDecision: true,
                RequiresRename: false,
                ProposedProfileId: null,
                ProposedProfileName: null,
                Message: "Manipulated ReplaceExisting plan.")
        });
        var dryRun = _dryRunService.Preview(importResult, plan, existingCatalog);

        var result = _executor.Execute(importResult, plan, dryRun, paths, _timestamp, "Tests");

        var loadedProfile = Assert.Single(_catalogService.Load(paths).ExportProfiles);
        Assert.Equal("export-existing", loadedProfile.Metadata.Id);
        Assert.Equal("Existing Export", loadedProfile.Metadata.Name);
        Assert.Empty(result.ImportedProfiles);
        Assert.Equal(1, result.Skipped);
        Assert.Contains(result.SkippedProfiles, item => item.Message.Contains("ReplaceExisting", StringComparison.OrdinalIgnoreCase));
    }

    private PipelineResult RunImportFlow(
        AppDataPaths paths,
        TemplatePackageImportResult importResult,
        IReadOnlyList<TemplatePackageImportUserSelection>? selections = null)
    {
        var existingCatalog = _catalogService.Load(paths);
        var validation = _validator.Validate(importResult);
        var analysis = _analyzer.Analyze(importResult, existingCatalog);
        var basePlan = _planBuilder.Build(analysis, _timestamp);
        var plan = _selectionService.Apply(basePlan, selections ?? Array.Empty<TemplatePackageImportUserSelection>());
        var dryRun = _dryRunService.Preview(importResult, plan, existingCatalog);
        var execution = _executor.Execute(importResult, plan, dryRun, paths, _timestamp, "Tests");
        var loadedCatalog = _catalogService.Load(paths);

        return new PipelineResult(validation, analysis, plan, dryRun, execution, loadedCatalog);
    }

    private TemplatePackageImportResult ExportAndImport(
        string workspace,
        IReadOnlyList<AisProfile>? aisProfiles = null,
        IReadOnlyList<DeviceProfileDefinition>? deviceProfiles = null,
        IReadOnlyList<ExportProfileDefinition>? exportProfiles = null,
        IReadOnlyList<InterfaceProfileDefinition>? interfaceProfiles = null)
    {
        var package = CreatePackage(
            aisProfiles ?? Array.Empty<AisProfile>(),
            deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
            exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
            interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
        var zipFilePath = Path.Combine(workspace, "template-package.zip");
        _exporter.Export(
            zipFilePath,
            new TemplatePackageExportRequest(
                package,
                aisProfiles ?? Array.Empty<AisProfile>(),
                deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
                exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
                interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>()));

        return _importer.Import(zipFilePath);
    }

    private static TemplatePackageImportResult CreateImportResult(
        IReadOnlyList<AisProfile>? aisProfiles = null,
        IReadOnlyList<DeviceProfileDefinition>? deviceProfiles = null,
        IReadOnlyList<ExportProfileDefinition>? exportProfiles = null,
        IReadOnlyList<InterfaceProfileDefinition>? interfaceProfiles = null)
    {
        return new TemplatePackageImportResult(
            Package: CreatePackage(
                aisProfiles ?? Array.Empty<AisProfile>(),
                deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
                exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
                interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>()),
            AisProfiles: aisProfiles ?? Array.Empty<AisProfile>(),
            DeviceProfiles: deviceProfiles ?? Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: exportProfiles ?? Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
    }

    private static TemplatePackage CreatePackage(
        IReadOnlyList<AisProfile> aisProfiles,
        IReadOnlyList<DeviceProfileDefinition> deviceProfiles,
        IReadOnlyList<ExportProfileDefinition> exportProfiles,
        IReadOnlyList<InterfaceProfileDefinition> interfaceProfiles)
    {
        var includedProfiles = new List<ProfileMetadata>();
        includedProfiles.AddRange(aisProfiles.Select(profile => profile.Metadata));
        includedProfiles.AddRange(deviceProfiles.Select(profile => profile.Metadata));
        includedProfiles.AddRange(exportProfiles.Select(profile => profile.Metadata));
        includedProfiles.AddRange(interfaceProfiles.Select(profile => profile.Metadata));

        return new TemplatePackage(
            Metadata: CreateMetadata("package-e2e", "E2E Template Package", ProfileKind.TemplatePackage),
            IncludedProfiles: includedProfiles,
            PackageFormatVersion: "1.0",
            CreatedAt: new DateTime(2026, 5, 9, 15, 0, 0, DateTimeKind.Utc),
            CreatedBy: "Tests",
            Description: "End-to-end import test package.");
    }

    private static TemplatePackageImportPlan CreatePlan(IReadOnlyList<TemplatePackageImportProfilePlan> profilePlans)
    {
        return new TemplatePackageImportPlan(
            PackageId: "package-e2e",
            PackageName: "E2E Template Package",
            GeneratedAt: new DateTimeOffset(2026, 5, 9, 15, 0, 0, TimeSpan.Zero),
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

    private static TemplatePackageImportUserSelection Selection(
        ProfileKind profileKind,
        string importedProfileId,
        TemplatePackageImportAction action)
    {
        return new TemplatePackageImportUserSelection(
            ProfileKind: profileKind,
            ImportedProfileId: importedProfileId,
            SelectedAction: action,
            TargetProfileId: null,
            TargetProfileName: null,
            IsValid: true,
            ValidationMessage: null);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        return new AppDataPathProvider().GetPaths(CreateWorkspaceFolder());
    }

    private static string CreateWorkspaceFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static AisProfile CreateAisProfile(string id, string name, bool isBuiltIn = false)
    {
        return DefaultAisProfiles.CreateMedistarDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.AisProfile, isBuiltIn),
            Name = name
        };
    }

    private static DeviceProfileDefinition CreateDeviceProfile(string id, string name, bool isBuiltIn = false)
    {
        return DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.DeviceProfile, isBuiltIn)
        };
    }

    private static ExportProfileDefinition CreateExportProfile(
        string id,
        string name,
        string aisProfileId,
        string deviceProfileId,
        bool isBuiltIn = false)
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.ExportProfile, isBuiltIn),
            TargetAisProfileId = aisProfileId,
            SourceDeviceProfileId = deviceProfileId
        };
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string id,
        string name,
        string aisProfileId,
        string deviceProfileId,
        string exportProfileId,
        string workspace,
        bool isBuiltIn = false)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.InterfaceProfile, isBuiltIn),
            AisProfileId = aisProfileId,
            DeviceProfileId = deviceProfileId,
            ExportProfileId = exportProfileId,
            FolderOptions = CreateFolderOptions(workspace),
            IsActive = false
        };
    }

    private static InterfaceFolderOptions CreateFolderOptions(string workspace)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
        {
            AisImportFolder = Path.Combine(workspace, "ais-in"),
            DeviceImportFolder = Path.Combine(workspace, "device-in"),
            ExportFolder = Path.Combine(workspace, "export"),
            ArchiveFolder = Path.Combine(workspace, "archive"),
            ErrorFolder = Path.Combine(workspace, "error"),
            ClearAisImportFolderBeforeProcessing = false,
            ClearDeviceImportFolderBeforeProcessing = false,
            ClearExportFolderAfterSuccessfulTransfer = false,
            ArchiveProcessedFiles = false,
            MoveFailedFilesToErrorFolder = false,
            AttachmentImportFolder = "",
            AttachmentExportFolder = ""
        };
    }

    private static ProfileMetadata CreateMetadata(
        string id,
        string name,
        ProfileKind profileKind,
        bool isBuiltIn = false)
    {
        var timestamp = new DateTimeOffset(2026, 5, 9, 15, 0, 0, TimeSpan.Zero);
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

    private sealed record PipelineResult(
        TemplatePackageImportValidationResult Validation,
        TemplatePackageImportAnalysisResult Analysis,
        TemplatePackageImportPlan Plan,
        TemplatePackageImportDryRunResult DryRun,
        TemplatePackageImportExecutionResult Execution,
        ProfileCatalog LoadedCatalog);
}
