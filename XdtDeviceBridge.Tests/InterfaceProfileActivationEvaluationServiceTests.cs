using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationEvaluationServiceTests
{
    private readonly InterfaceProfileActivationEvaluationService _service = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Evaluate_ShouldMarkCompleteUserDefinedInterfaceProfileAsReady()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.True(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Ready, result.ActivationStatus);
        Assert.Empty(result.Blockers);
    }

    [Fact]
    public void Evaluate_ShouldBlockMissingAisProfile()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders, includeAisProfile: false);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "dependency.ais.notfound");
    }

    [Fact]
    public void Evaluate_ShouldBlockMissingDeviceProfile()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders, includeDeviceProfile: false);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "dependency.device.notfound");
    }

    [Fact]
    public void Evaluate_ShouldBlockMissingExportProfile()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders, includeExportProfile: false);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "dependency.export.notfound");
    }

    [Fact]
    public void Evaluate_ShouldBlockMissingRequiredAisImportFolder()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders);
        var profile = context.InterfaceProfile with
        {
            FolderOptions = context.InterfaceProfile.FolderOptions with
            {
                AisImportFolder = string.Empty
            }
        };

        var result = _service.Evaluate(profile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "folder.aisImport.missing");
    }

    [Fact]
    public void Evaluate_ShouldBlockRequiredAttachmentWhenImportFolderIsMissing()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(
            folders,
            configureAttachment: options => options with
            {
                AttachmentRequirementMode = AttachmentRequirementMode.Required,
                IsAttachmentProcessingEnabled = false,
                AttachmentImportFolder = string.Empty,
                AttachmentExportFolder = folders.AttachmentExportFolder
            });

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "attachment.folder.import.missing");
    }

    [Fact]
    public void Evaluate_ShouldBlockRequiredAttachmentWhenExportFolderIsMissing()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(
            folders,
            configureAttachment: options => options with
            {
                AttachmentRequirementMode = AttachmentRequirementMode.Required,
                IsAttachmentProcessingEnabled = false,
                AttachmentImportFolder = folders.AttachmentImportFolder,
                AttachmentExportFolder = string.Empty
            });

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "attachment.folder.export.missing");
    }

    [Fact]
    public void Evaluate_ShouldNotBlockOptionalDisabledAttachmentProcessing()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(
            folders,
            configureAttachment: options => options with
            {
                AttachmentRequirementMode = AttachmentRequirementMode.Optional,
                IsAttachmentProcessingEnabled = false,
                AttachmentImportFolder = string.Empty,
                AttachmentExportFolder = string.Empty
            });

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.True(result.CanActivate);
        Assert.DoesNotContain(result.Blockers, check => check.Code.StartsWith("attachment.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Infos, check => check.Code == "attachment.disabled");
    }

    [Fact]
    public void Evaluate_ShouldBlockBuiltInInterfaceProfileWithoutChangingIt()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders, isBuiltIn: true);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.False(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.Blocked, result.ActivationStatus);
        Assert.Contains(result.Blockers, check => check.Code == "profile.builtin");
        Assert.True(context.InterfaceProfile.Metadata.IsBuiltIn);
        Assert.False(context.InterfaceProfile.Metadata.IsUserDefined);
    }

    [Fact]
    public void Evaluate_ShouldReportLicenseRequiredProfileAsWarningOnly()
    {
        using var folders = TestFolders.Create();
        var context = CreateContext(folders, isLicenseRequired: true);

        var result = _service.Evaluate(context.InterfaceProfile, context.Catalog);

        Assert.True(result.CanActivate);
        Assert.Equal(InterfaceProfileActivationStatus.ReadyWithWarnings, result.ActivationStatus);
        Assert.Empty(result.Blockers);
        Assert.Contains(result.Warnings, check => check.Code == "license.required");
    }

    private ActivationEvaluationTestContext CreateContext(
        TestFolders folders,
        bool includeAisProfile = true,
        bool includeDeviceProfile = true,
        bool includeExportProfile = true,
        bool isBuiltIn = false,
        bool isLicenseRequired = false,
        Func<InterfaceFolderOptions, InterfaceFolderOptions>? configureAttachment = null)
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var folderOptions = CreateFolderOptions(folders);
        if (configureAttachment is not null)
        {
            folderOptions = configureAttachment(folderOptions);
        }

        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(
                "interface-imported-userdefined",
                "Importierte Schnittstelle",
                ProfileKind.InterfaceProfile,
                isBuiltIn),
            AisProfileId = aisProfile.Metadata.Id,
            DeviceProfileId = deviceProfile.Metadata.Id,
            ExportProfileId = exportProfile.Metadata.Id,
            FolderOptions = folderOptions,
            IsActive = false,
            IsLicenseRequired = isLicenseRequired
        };

        var catalog = new ProfileCatalog(
            AisProfiles: includeAisProfile ? new[] { aisProfile } : Array.Empty<AisProfile>(),
            DeviceProfiles: includeDeviceProfile ? new[] { deviceProfile } : Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: includeExportProfile ? new[] { exportProfile } : Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: new[] { interfaceProfile });

        return new ActivationEvaluationTestContext(interfaceProfile, catalog);
    }

    private static InterfaceFolderOptions CreateFolderOptions(TestFolders folders)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
        {
            AisImportFolder = folders.AisImportFolder,
            DeviceImportFolder = folders.DeviceImportFolder,
            ExportFolder = folders.ExportFolder,
            ArchiveFolder = folders.ArchiveFolder,
            ErrorFolder = folders.ErrorFolder,
            ArchiveProcessedFiles = false,
            MoveFailedFilesToErrorFolder = false,
            AttachmentImportFolder = string.Empty,
            AttachmentExportFolder = string.Empty,
            IsAttachmentProcessingEnabled = false,
            AttachmentRequirementMode = AttachmentRequirementMode.Optional,
            AttachmentWaitTimeoutSeconds = 30,
            AttachmentFileStabilityWaitSeconds = 2
        };
    }

    private ProfileMetadata CreateMetadata(string id, string name, ProfileKind profileKind, bool isBuiltIn)
    {
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: _timestamp,
            UpdatedAt: _timestamp,
            CreatedBy: "Tests",
            IsBuiltIn: isBuiltIn,
            IsUserDefined: !isBuiltIn);
    }

    private sealed record ActivationEvaluationTestContext(
        InterfaceProfileDefinition InterfaceProfile,
        ProfileCatalog Catalog);

    private sealed class TestFolders : IDisposable
    {
        private TestFolders(string root)
        {
            Root = root;
            AisImportFolder = CreateChild("ais-import");
            DeviceImportFolder = CreateChild("device-import");
            ExportFolder = CreateChild("export");
            ArchiveFolder = CreateChild("archive");
            ErrorFolder = CreateChild("error");
            AttachmentImportFolder = CreateChild("attachment-import");
            AttachmentExportFolder = CreateChild("attachment-export");
        }

        public string Root { get; }

        public string AisImportFolder { get; }

        public string DeviceImportFolder { get; }

        public string ExportFolder { get; }

        public string ArchiveFolder { get; }

        public string ErrorFolder { get; }

        public string AttachmentImportFolder { get; }

        public string AttachmentExportFolder { get; }

        public static TestFolders Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TestFolders(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private string CreateChild(string name)
        {
            var path = Path.Combine(Root, name);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
