using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TemplatePackageImportConflictAnalyzerTests
{
    private readonly TemplatePackageImportConflictAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_ShouldSuggestImportAsNewWhenNoConflictExists()
    {
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");
        var deviceProfile = CreateDeviceProfile("device-imported", "Imported Device");
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", aisProfile.Metadata.Id, deviceProfile.Metadata.Id);
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", aisProfile.Metadata.Id, deviceProfile.Metadata.Id, exportProfile.Metadata.Id);
        var importResult = CreateImportResult(
            aisProfiles: new[] { aisProfile },
            deviceProfiles: new[] { deviceProfile },
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile });
        var catalog = CreateCatalog();

        var result = _analyzer.Analyze(importResult, catalog);

        Assert.True(result.Success);
        Assert.Equal(4, result.TotalProfiles);
        Assert.Equal(4, result.ImportableProfiles);
        Assert.Equal(0, result.ConflictingProfiles);
        Assert.Equal(0, result.BlockedProfiles);
        Assert.All(result.ProfileDecisions, decision =>
        {
            Assert.Equal(TemplatePackageImportConflictType.None, decision.ConflictType);
            Assert.Equal(TemplatePackageImportAction.ImportAsNew, decision.SuggestedAction);
            Assert.False(decision.IsBlocking);
        });
    }

    [Fact]
    public void Analyze_ShouldDetectSameIdConflict()
    {
        var importedAis = CreateAisProfile("ais-imported", "Imported AIS");
        var existingAis = CreateAisProfile("ais-imported", "Existing AIS", isBuiltIn: false, isUserDefined: true);

        var result = _analyzer.Analyze(
            CreateImportResult(aisProfiles: new[] { importedAis }),
            CreateCatalog(aisProfiles: new[] { existingAis }));

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.SameIdExists, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.ImportAsCopy, decision.SuggestedAction);
        Assert.Equal(TemplatePackageImportExistingProfileSource.UserDefined, decision.ExistingProfileSource);
        Assert.False(decision.IsBlocking);
    }

    [Fact]
    public void Analyze_ShouldDetectSameNameConflict()
    {
        var importedAis = CreateAisProfile("ais-imported", "MEDISTAR Duplicate");
        var existingAis = CreateAisProfile("ais-existing", "MEDISTAR Duplicate", isBuiltIn: false, isUserDefined: true);

        var result = _analyzer.Analyze(
            CreateImportResult(aisProfiles: new[] { importedAis }),
            CreateCatalog(aisProfiles: new[] { existingAis }));

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.SameNameExists, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.ImportAsCopy, decision.SuggestedAction);
        Assert.False(decision.IsBlocking);
    }

    [Fact]
    public void Analyze_ShouldProtectBuiltInProfiles()
    {
        var importedAis = CreateAisProfile("ais-medistar-default", "MEDISTAR");
        var existingAis = DefaultAisProfiles.CreateMedistarDefault();

        var result = _analyzer.Analyze(
            CreateImportResult(aisProfiles: new[] { importedAis }),
            CreateCatalog(aisProfiles: new[] { existingAis }));

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.BuiltInProtected, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.ImportAsCopy, decision.SuggestedAction);
        Assert.NotEqual(TemplatePackageImportAction.ReplaceExisting, decision.SuggestedAction);
        Assert.Equal(TemplatePackageImportExistingProfileSource.BuiltIn, decision.ExistingProfileSource);
    }

    [Fact]
    public void Analyze_ShouldAllowUserDefinedConflictToBeResolvedLater()
    {
        var importedExport = CreateExportProfile("export-imported", "Imported Export", "ais-local", "device-local");
        var existingExport = CreateExportProfile(
            "export-imported",
            "Existing Export",
            "ais-local",
            "device-local",
            isBuiltIn: false,
            isUserDefined: true);
        var catalog = CreateCatalog(
            aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
            exportProfiles: new[] { existingExport });

        var result = _analyzer.Analyze(
            CreateImportResult(exportProfiles: new[] { importedExport }),
            catalog);

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.SameIdExists, decision.ConflictType);
        Assert.NotEqual(TemplatePackageImportAction.Blocked, decision.SuggestedAction);
        Assert.False(decision.IsBlocking);
    }

    [Fact]
    public void Analyze_ShouldBlockInterfaceProfileWithMissingAisDependency()
    {
        var interfaceProfile = CreateInterfaceProfile(
            "interface-imported",
            "Imported Interface",
            aisProfileId: "missing-ais",
            deviceProfileId: "device-local",
            exportProfileId: "export-local");
        var catalog = CreateCatalog(
            deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") },
            exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "missing-ais", "device-local") });

        var result = _analyzer.Analyze(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            catalog);

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.MissingDependency, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.Blocked, decision.SuggestedAction);
        Assert.True(decision.IsBlocking);
        Assert.Contains("fehlendes AIS-Profil", decision.Message);
    }

    [Fact]
    public void Analyze_ShouldBlockInterfaceProfileWithMissingDeviceDependency()
    {
        var interfaceProfile = CreateInterfaceProfile(
            "interface-imported",
            "Imported Interface",
            aisProfileId: "ais-local",
            deviceProfileId: "missing-device",
            exportProfileId: "export-local");
        var catalog = CreateCatalog(
            aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
            exportProfiles: new[] { CreateExportProfile("export-local", "Local Export", "ais-local", "missing-device") });

        var result = _analyzer.Analyze(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            catalog);

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.MissingDependency, decision.ConflictType);
        Assert.True(decision.IsBlocking);
        Assert.Contains("fehlendes Geräteprofil", decision.Message);
    }

    [Fact]
    public void Analyze_ShouldBlockInterfaceProfileWithMissingExportDependency()
    {
        var interfaceProfile = CreateInterfaceProfile(
            "interface-imported",
            "Imported Interface",
            aisProfileId: "ais-local",
            deviceProfileId: "device-local",
            exportProfileId: "missing-export");
        var catalog = CreateCatalog(
            aisProfiles: new[] { CreateAisProfile("ais-local", "Local AIS") },
            deviceProfiles: new[] { CreateDeviceProfile("device-local", "Local Device") });

        var result = _analyzer.Analyze(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            catalog);

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.MissingDependency, decision.ConflictType);
        Assert.True(decision.IsBlocking);
        Assert.Contains("fehlendes Exportprofil", decision.Message);
    }

    [Fact]
    public void Analyze_ShouldAcceptInterfaceDependenciesFromSamePackage()
    {
        var aisProfile = CreateAisProfile("ais-imported", "Imported AIS");
        var deviceProfile = CreateDeviceProfile("device-imported", "Imported Device");
        var exportProfile = CreateExportProfile("export-imported", "Imported Export", aisProfile.Metadata.Id, deviceProfile.Metadata.Id);
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", aisProfile.Metadata.Id, deviceProfile.Metadata.Id, exportProfile.Metadata.Id);

        var result = _analyzer.Analyze(
            CreateImportResult(
                aisProfiles: new[] { aisProfile },
                deviceProfiles: new[] { deviceProfile },
                exportProfiles: new[] { exportProfile },
                interfaceProfiles: new[] { interfaceProfile }),
            CreateCatalog());

        Assert.DoesNotContain(result.ProfileDecisions, decision => decision.ConflictType == TemplatePackageImportConflictType.MissingDependency);
        Assert.All(result.ProfileDecisions, decision => Assert.False(decision.IsBlocking));
    }

    [Fact]
    public void Analyze_ShouldAcceptInterfaceDependenciesFromLocalCatalog()
    {
        var aisProfile = CreateAisProfile("ais-local", "Local AIS");
        var deviceProfile = CreateDeviceProfile("device-local", "Local Device");
        var exportProfile = CreateExportProfile("export-local", "Local Export", aisProfile.Metadata.Id, deviceProfile.Metadata.Id);
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", aisProfile.Metadata.Id, deviceProfile.Metadata.Id, exportProfile.Metadata.Id);

        var result = _analyzer.Analyze(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreateCatalog(
                aisProfiles: new[] { aisProfile },
                deviceProfiles: new[] { deviceProfile },
                exportProfiles: new[] { exportProfile }));

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.None, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.ImportAsNew, decision.SuggestedAction);
    }

    [Fact]
    public void Analyze_ShouldKeepAttachmentSettingsAndWarnForReview()
    {
        var aisProfile = CreateAisProfile("ais-local", "Local AIS");
        var deviceProfile = CreateDeviceProfile("device-local", "Local Device");
        var exportProfile = CreateExportProfile("export-local", "Local Export", aisProfile.Metadata.Id, deviceProfile.Metadata.Id);
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", aisProfile.Metadata.Id, deviceProfile.Metadata.Id, exportProfile.Metadata.Id) with
        {
            IsActive = true,
            FolderOptions = CreateFolderOptions() with
            {
                AisImportFolder = @"C:\XdtBridge\Ais",
                DeviceImportFolder = @"C:\XdtBridge\Device",
                ExportFolder = @"C:\XdtBridge\Export",
                AttachmentImportFolder = @"C:\XdtBridge\AttachmentIn",
                AttachmentExportFolder = @"C:\XdtBridge\AttachmentOut",
                AttachmentExternalLinkDescription = "Messprotokoll"
            }
        };
        var importResult = CreateImportResult(interfaceProfiles: new[] { interfaceProfile });

        var result = _analyzer.Analyze(
            importResult,
            CreateCatalog(
                aisProfiles: new[] { aisProfile },
                deviceProfiles: new[] { deviceProfile },
                exportProfiles: new[] { exportProfile }));

        Assert.Equal(@"C:\XdtBridge\AttachmentIn", importResult.InterfaceProfiles[0].FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"C:\XdtBridge\AttachmentOut", importResult.InterfaceProfiles[0].FolderOptions.AttachmentExportFolder);
        Assert.Contains(result.Warnings, warning => warning.Contains("vor späterer Nutzung geprüft", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("wird beim Import nicht automatisch aktiviert", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Contains("XDT-Anhang-Ordner", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Analyze_ShouldBlockUnsafeFolderPath()
    {
        var aisProfile = CreateAisProfile("ais-local", "Local AIS");
        var deviceProfile = CreateDeviceProfile("device-local", "Local Device");
        var exportProfile = CreateExportProfile("export-local", "Local Export", aisProfile.Metadata.Id, deviceProfile.Metadata.Id);
        var interfaceProfile = CreateInterfaceProfile("interface-imported", "Imported Interface", aisProfile.Metadata.Id, deviceProfile.Metadata.Id, exportProfile.Metadata.Id) with
        {
            FolderOptions = CreateFolderOptions() with
            {
                AttachmentExportFolder = Path.GetPathRoot(Environment.CurrentDirectory) ?? @"C:\"
            }
        };

        var result = _analyzer.Analyze(
            CreateImportResult(interfaceProfiles: new[] { interfaceProfile }),
            CreateCatalog(
                aisProfiles: new[] { aisProfile },
                deviceProfiles: new[] { deviceProfile },
                exportProfiles: new[] { exportProfile }));

        var decision = Assert.Single(result.ProfileDecisions);
        Assert.Equal(TemplatePackageImportConflictType.UnsafeFolderPath, decision.ConflictType);
        Assert.Equal(TemplatePackageImportAction.Blocked, decision.SuggestedAction);
        Assert.True(decision.IsBlocking);
    }

    [Fact]
    public void Analyze_ShouldCalculateCounts()
    {
        var importableAis = CreateAisProfile("ais-new", "New AIS");
        var conflictingDevice = CreateDeviceProfile("device-conflict", "Conflicting Device");
        var blockedInterface = CreateInterfaceProfile(
            "interface-blocked",
            "Blocked Interface",
            aisProfileId: "missing-ais",
            deviceProfileId: "device-conflict",
            exportProfileId: "missing-export");
        var existingDevice = CreateDeviceProfile("device-conflict", "Existing Device", isBuiltIn: false, isUserDefined: true);

        var result = _analyzer.Analyze(
            CreateImportResult(
                aisProfiles: new[] { importableAis },
                deviceProfiles: new[] { conflictingDevice },
                interfaceProfiles: new[] { blockedInterface }),
            CreateCatalog(deviceProfiles: new[] { existingDevice }));

        Assert.Equal(3, result.TotalProfiles);
        Assert.Equal(2, result.ImportableProfiles);
        Assert.Equal(2, result.ConflictingProfiles);
        Assert.Equal(1, result.BlockedProfiles);
        Assert.Single(result.BlockingConflicts);
    }

    private static TemplatePackageImportResult CreateImportResult(
        IReadOnlyList<AisProfile>? aisProfiles = null,
        IReadOnlyList<DeviceProfileDefinition>? deviceProfiles = null,
        IReadOnlyList<ExportProfileDefinition>? exportProfiles = null,
        IReadOnlyList<InterfaceProfileDefinition>? interfaceProfiles = null)
    {
        var profiles = new List<ProfileMetadata>();
        profiles.AddRange((aisProfiles ?? Array.Empty<AisProfile>()).Select(profile => profile.Metadata));
        profiles.AddRange((deviceProfiles ?? Array.Empty<DeviceProfileDefinition>()).Select(profile => profile.Metadata));
        profiles.AddRange((exportProfiles ?? Array.Empty<ExportProfileDefinition>()).Select(profile => profile.Metadata));
        profiles.AddRange((interfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>()).Select(profile => profile.Metadata));

        return new TemplatePackageImportResult(
            Package: new TemplatePackage(
                Metadata: CreateMetadata("package-import", "Import Package", ProfileKind.TemplatePackage),
                IncludedProfiles: profiles,
                PackageFormatVersion: "1.0",
                CreatedAt: new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
                CreatedBy: "Tests",
                Description: "Conflict analyzer test package."),
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

    private static AisProfile CreateAisProfile(
        string id,
        string name,
        bool isBuiltIn = false,
        bool isUserDefined = true)
    {
        return DefaultAisProfiles.CreateMedistarDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.AisProfile, isBuiltIn, isUserDefined),
            Name = name
        };
    }

    private static DeviceProfileDefinition CreateDeviceProfile(
        string id,
        string name,
        bool isBuiltIn = false,
        bool isUserDefined = true)
    {
        return DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.DeviceProfile, isBuiltIn, isUserDefined)
        };
    }

    private static ExportProfileDefinition CreateExportProfile(
        string id,
        string name,
        string aisProfileId,
        string deviceProfileId,
        bool isBuiltIn = false,
        bool isUserDefined = true)
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.ExportProfile, isBuiltIn, isUserDefined),
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
        bool isBuiltIn = false,
        bool isUserDefined = true)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name, ProfileKind.InterfaceProfile, isBuiltIn, isUserDefined),
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

    private static ProfileMetadata CreateMetadata(
        string id,
        string name,
        ProfileKind profileKind,
        bool isBuiltIn = false,
        bool isUserDefined = true,
        string version = "1.0.0")
    {
        var timestamp = new DateTimeOffset(2026, 5, 8, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: version,
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "Tests",
            IsBuiltIn: isBuiltIn,
            IsUserDefined: isUserDefined);
    }
}
