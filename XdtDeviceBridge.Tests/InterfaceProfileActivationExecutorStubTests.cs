using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationExecutorStubTests
{
    [Fact]
    public async Task ExecuteAsync_WithoutStore_ShouldRemainNonProductive()
    {
        var profile = CreateUserDefinedProfile();
        var executor = new InterfaceProfileActivationExecutorStub();

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed)));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.True(result.RequiresFreshLoad);
        Assert.True(result.RequiresFinalEvaluation);
        Assert.False(result.FreshLoadPerformed);
        Assert.Contains(result.MissingCapabilities!, item => item == "executor.storeContext.missing");
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndMissingProfileId_ShouldNotActivateOrSave()
    {
        var store = new RecordingActivationProfileStore();
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: ""));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.True(result.FreshLoadPerformed);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndUnknownProfile_ShouldReturnNotAvailable()
    {
        var store = new RecordingActivationProfileStore();
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: "interface-missing"));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.True(result.FreshLoadPerformed);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.found" && !item.IsSatisfied);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndBuiltInProfile_ShouldBlock()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: profile.Metadata.Id));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.notBuiltIn" && !item.IsSatisfied);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndNonUserDefinedProfile_ShouldBlock()
    {
        var profile = CreateUserDefinedProfile() with
        {
            Metadata = CreateUserDefinedProfile().Metadata with
            {
                Id = "interface-non-userdefined",
                IsUserDefined = false,
                IsBuiltIn = false
            }
        };
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: profile.Metadata.Id));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.userDefined" && !item.IsSatisfied);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreButWithoutFinalServices_ShouldLoadButBlockSaveDryRun()
    {
        var profile = CreateUserDefinedProfile();
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed),
            InterfaceProfileId: profile.Metadata.Id));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.True(result.FreshLoadPerformed);
        Assert.False(result.FinalEvaluationPerformed);
        Assert.True(result.RequiresFinalEvaluation);
        Assert.True(result.SaveDryRunBlocked);
        Assert.False(result.SaveDryRunPerformed);
        Assert.Equal(0, store.SaveCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndFinalServicesReady_ShouldSimulateFinalEvaluationAndSaveDryRun()
    {
        using var folders = TestFolders.Create();
        var context = CreateFinalEvaluationContext(folders);
        var store = new RecordingActivationProfileStore(context.Profile);
        var executor = new InterfaceProfileActivationExecutorStub(
            store,
            new InterfaceProfileActivationEvaluationService(),
            new InterfaceProfileActivationGuardService(),
            context.Catalog);
        var originalIsActive = context.Profile.IsActive;
        var originalAttachmentEnabled = context.Profile.FolderOptions.IsAttachmentProcessingEnabled;

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: context.Profile.Metadata.Id));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.True(result.FinalEvaluationPerformed);
        Assert.True(result.GuardRechecked);
        Assert.False(result.RequiresFinalEvaluation);
        Assert.True(result.SaveDryRunPerformed);
        Assert.False(result.SaveDryRunBlocked);
        Assert.Equal(1, store.SaveCallCount);
        Assert.NotNull(store.LastSaveRequest);
        Assert.True(store.LastSaveRequest.FinalReEvaluationCompleted);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Equal(originalIsActive, context.Profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, context.Profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_WithFinalServicesReadyWithWarnings_ShouldBlockV1AndSkipSaveDryRun()
    {
        using var folders = TestFolders.Create();
        var context = CreateFinalEvaluationContext(folders, isLicenseRequired: true);
        var store = new RecordingActivationProfileStore(context.Profile);
        var executor = new InterfaceProfileActivationExecutorStub(
            store,
            new InterfaceProfileActivationEvaluationService(),
            new InterfaceProfileActivationGuardService(),
            context.Catalog);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            EvaluationResult: null,
            GuardResult: null,
            InterfaceProfileId: context.Profile.Metadata.Id));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.True(result.FinalEvaluationPerformed);
        Assert.True(result.SaveDryRunBlocked);
        Assert.False(result.SaveDryRunPerformed);
        Assert.Equal(0, store.SaveCallCount);
        Assert.Contains(result.Preconditions, item => item.Code == "evaluation.readyWithoutWarnings" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ActivateMode_ShouldNotUseStoreOrPersist()
    {
        var profile = CreateUserDefinedProfile();
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed),
            InterfaceProfileId: profile.Metadata.Id,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.Activate));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotImplemented, result.Status);
        Assert.False(result.IsValidationOnly);
        Assert.False(result.FreshLoadPerformed);
        Assert.False(result.SaveDryRunPerformed);
        Assert.Equal(0, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotMutateProfile()
    {
        var profile = CreateUserDefinedProfile() with
        {
            IsActive = false,
            FolderOptions = CreateUserDefinedProfile().FolderOptions with
            {
                IsAttachmentProcessingEnabled = false
            }
        };
        var originalId = profile.Metadata.Id;
        var originalName = profile.Metadata.Name;
        var originalIsActive = profile.IsActive;
        var originalAttachmentEnabled = profile.FolderOptions.IsAttachmentProcessingEnabled;
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        _ = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(canProceed: true, InterfaceProfileActivationGuardDecision.Allowed),
            InterfaceProfileId: profile.Metadata.Id));

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(0, store.SaveCallCount);
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined-executor",
                Name = "UserDefined Executor Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            IsActive = false,
            IsLicenseRequired = false
        };
    }

    private static InterfaceProfileActivationEvaluationResult Evaluation(
        InterfaceProfileActivationStatus status,
        bool canActivate,
        params InterfaceProfileActivationCheckResult[] checks)
    {
        return new InterfaceProfileActivationEvaluationResult(status, canActivate, checks);
    }

    private static InterfaceProfileActivationGuardResult Guard(
        bool canProceed,
        InterfaceProfileActivationGuardDecision decision)
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            Array.Empty<InterfaceProfileActivationGuardReason>(),
            Array.Empty<InterfaceProfileActivationGuardReason>(),
            Array.Empty<InterfaceProfileActivationGuardReason>(),
            "Guard-Test.");
    }

    private static FinalEvaluationTestContext CreateFinalEvaluationContext(
        TestFolders folders,
        bool isLicenseRequired = false)
    {
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var profile = CreateUserDefinedProfile() with
        {
            Metadata = CreateUserDefinedProfile().Metadata with
            {
                Id = isLicenseRequired ? "interface-ready-warning-executor" : "interface-ready-executor",
                Name = isLicenseRequired ? "Warnende Schnittstelle" : "Bereite Schnittstelle"
            },
            AisProfileId = aisProfile.Metadata.Id,
            DeviceProfileId = deviceProfile.Metadata.Id,
            ExportProfileId = exportProfile.Metadata.Id,
            FolderOptions = CreateFolderOptions(folders),
            IsLicenseRequired = isLicenseRequired
        };
        var catalog = new ProfileCatalog(
            new[] { aisProfile },
            new[] { deviceProfile },
            new[] { exportProfile },
            new[] { profile });

        return new FinalEvaluationTestContext(profile, catalog);
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

    private sealed record FinalEvaluationTestContext(
        InterfaceProfileDefinition Profile,
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
        }

        public string Root { get; }

        public string AisImportFolder { get; }

        public string DeviceImportFolder { get; }

        public string ExportFolder { get; }

        public string ArchiveFolder { get; }

        public string ErrorFolder { get; }

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

    private sealed class RecordingActivationProfileStore : IInterfaceProfileActivationProfileStore
    {
        private readonly IReadOnlyList<InterfaceProfileDefinition> _profiles;

        public RecordingActivationProfileStore(params InterfaceProfileDefinition[] profiles)
        {
            _profiles = profiles;
        }

        public int LoadCallCount { get; private set; }

        public int SaveCallCount { get; private set; }

        public InterfaceProfileActivationProfileSaveRequest? LastSaveRequest { get; private set; }

        public InterfaceProfileActivationProfileLoadResult LoadFreshUserDefinedProfile(string profileId)
        {
            LoadCallCount++;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                return LoadResult(
                    InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                    success: false,
                    profile: null,
                    requestedProfileId: profileId,
                    "Schnittstellenprofil-ID fehlt.");
            }

            var profile = _profiles.FirstOrDefault(item =>
                string.Equals(item.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
            if (profile is null)
            {
                return LoadResult(
                    InterfaceProfileActivationProfileStoreStatus.NotFound,
                    success: false,
                    profile: null,
                    requestedProfileId: profileId,
                    "Schnittstellenprofil wurde nicht gefunden.");
            }

            if (profile.Metadata.IsBuiltIn)
            {
                return LoadResult(
                    InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                    success: false,
                    profile,
                    requestedProfileId: profileId,
                    "BuiltIn-Schnittstellenprofile sind gesperrt.");
            }

            if (!profile.Metadata.IsUserDefined)
            {
                return LoadResult(
                    InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked,
                    success: false,
                    profile,
                    requestedProfileId: profileId,
                    "Schnittstellenprofil ist nicht UserDefined.");
            }

            return LoadResult(
                InterfaceProfileActivationProfileStoreStatus.LoadedUserDefined,
                success: true,
                profile,
                requestedProfileId: profileId,
                "UserDefined-Schnittstellenprofil geladen.");
        }

        public InterfaceProfileActivationProfileSaveResult SaveUserDefinedProfile(
            InterfaceProfileActivationProfileSaveRequest request)
        {
            SaveCallCount++;
            LastSaveRequest = request;

            var profile = request.Profile;
            var success = profile is not null &&
                profile.Metadata.IsUserDefined &&
                !profile.Metadata.IsBuiltIn &&
                request.FinalReEvaluationCompleted &&
                request.OperationMode == InterfaceProfileActivationExecutorOperationMode.ValidateOnly;

            return new InterfaceProfileActivationProfileSaveResult(
                success ? InterfaceProfileActivationProfileStoreStatus.SaveWouldBeAllowed : InterfaceProfileActivationProfileStoreStatus.MissingCapability,
                success,
                profile?.Metadata.Id ?? string.Empty,
                profile?.Metadata.Name ?? string.Empty,
                WouldSave: success,
                WasSaved: false,
                ProfileChanged: false,
                profile?.Metadata.IsUserDefined == true,
                profile?.Metadata.IsBuiltIn == true,
                success
                    ? "ValidateOnly wuerde speichern, hat aber nicht gespeichert."
                    : "Save-DryRun blockiert.",
                Array.Empty<InterfaceProfileActivationExecutorPrecondition>());
        }

        private static InterfaceProfileActivationProfileLoadResult LoadResult(
            InterfaceProfileActivationProfileStoreStatus status,
            bool success,
            InterfaceProfileDefinition? profile,
            string requestedProfileId,
            string message)
        {
            return new InterfaceProfileActivationProfileLoadResult(
                status,
                success,
                profile,
                profile?.Metadata.Id ?? requestedProfileId,
                profile?.Metadata.Name ?? string.Empty,
                profile is not null,
                profile?.Metadata.IsUserDefined == true,
                profile?.Metadata.IsBuiltIn == true,
                message,
                Array.Empty<InterfaceProfileActivationExecutorPrecondition>());
        }
    }
}
