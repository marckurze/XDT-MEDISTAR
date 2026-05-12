using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationExecutorStubTests
{
    private readonly IInterfaceProfileActivationExecutor _executor = new InterfaceProfileActivationExecutorStub();

    [Fact]
    public async Task ExecuteAsync_ShouldReturnReadyButNotExecutedForReadyUserDefinedProfile()
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

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Empty(result.ExecutedSteps);
        Assert.Contains(result.NotExecutedSteps, step => step.Code == "activate.profile");
        Assert.Contains("nicht-produktiv", result.Message);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.safePersistence.missing" && !item.IsSatisfied);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRequireWarningConfirmationForReadyWithWarningsWithoutConfirmation()
    {
        var result = await _executor.ExecuteAsync(CreateReadyWithWarningsRequest(
            CreateUserDefinedProfile(),
            warningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnReadyButNotExecutedForAcceptedWarningsWithoutPersistingConfirmation()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = false };

        var result = await _executor.ExecuteAsync(CreateReadyWithWarningsRequest(
            profile,
            warningsAccepted: true));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.False(profile.IsActive);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.freshReload.missing" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockBlockedEvaluation()
    {
        var profile = CreateUserDefinedProfile();
        var evaluation = Evaluation(
            InterfaceProfileActivationStatus.Blocked,
            canActivate: false,
            Check(InterfaceProfileActivationSeverity.Blocker, "folder.aisImport.missing", "AIS-Importordner fehlt."));

        var result = await _executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Blocked),
            EvaluationResult: evaluation,
            GuardResult: Guard(false, InterfaceProfileActivationGuardDecision.Blocked),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.Blocked),
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.False(result.Success);
        Assert.False(profile.IsActive);
        Assert.False(result.Saved);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBlockBuiltInProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.notBuiltIn" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotAvailableWithoutEvaluation()
    {
        var profile = CreateUserDefinedProfile();

        var result = await _executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Ready),
            EvaluationResult: null,
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings),
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.Contains(result.Preconditions, item => item.Code == "evaluation.present" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNeverStartProcessingOrReportExecutedSteps()
    {
        var result = await _executor.ExecuteAsync(CreateReadyRequest(CreateUserDefinedProfile()));

        Assert.Empty(result.ExecutedSteps);
        Assert.False(result.ProcessingStarted);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotAvailableWithoutProfileId()
    {
        var result = await _executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            ActivationPlan: null,
            EvaluationResult: null,
            GuardResult: null,
            WarningConfirmationResult: null,
            WarningsAccepted: false));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WasExecuted);
        Assert.False(result.WasPersisted);
        Assert.False(result.WasProfileChanged);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.id.present" && !item.IsSatisfied);
        Assert.Contains(result.MissingCapabilities ?? Array.Empty<string>(), item => item == "executor.freshReload.missing");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExposeFreshLoadAndSafeStoreRequirements()
    {
        var result = await _executor.ExecuteAsync(CreateReadyRequest(CreateUserDefinedProfile()));

        Assert.True(result.RequiresFreshLoad);
        Assert.True(result.RequiresSafeUserDefinedStore);
        Assert.True(result.RequiresFinalReEvaluation);
        Assert.False(result.FreshLoadPerformed);
        Assert.False(result.FinalReEvaluationPerformed);
        Assert.False(result.SaveDryRunPerformed);
        Assert.Contains(result.MissingCapabilities ?? Array.Empty<string>(), item => item == "executor.freshReload.missing");
        Assert.Contains(result.MissingCapabilities ?? Array.Empty<string>(), item => item == "executor.storeContext.missing");
        Assert.Contains(result.MissingCapabilities ?? Array.Empty<string>(), item => item == "executor.safePersistence.missing");
        Assert.Contains(result.MissingCapabilities ?? Array.Empty<string>(), item => item == "executor.finalReEvaluation.required");
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldKeepDefensiveBehaviorWithoutProfileId()
    {
        var store = new RecordingActivationProfileStore();
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(new InterfaceProfileActivationExecutorRequest(
            Profile: null,
            ActivationPlan: null,
            EvaluationResult: null,
            GuardResult: null,
            WarningConfirmationResult: null,
            WarningsAccepted: false,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly));

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.False(result.ProfileChanged);
        Assert.True(result.FreshLoadPerformed);
        Assert.True(result.RequiresFinalReEvaluation);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.id.present" && !item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.freshLoad.attempted" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.saveDryRun.checked" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldReturnNotAvailableForUnknownProfileId()
    {
        var store = new RecordingActivationProfileStore();
        var executor = new InterfaceProfileActivationExecutorStub(store);
        var profile = CreateUserDefinedProfile();

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null,
            InterfaceProfileId = "interface-missing"
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotAvailable, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.found" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldBlockBuiltInProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.notBuiltIn" && !item.IsSatisfied);
        Assert.False(result.SaveDryRunPerformed);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldBlockNonUserDefinedProfile()
    {
        var profile = CreateNonUserDefinedProfile();
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.Blocked, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.userDefined" && !item.IsSatisfied);
        Assert.False(result.SaveDryRunPerformed);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldLoadUserDefinedProfileButNotActivateOrSave()
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

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null,
            PreviewCreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-2),
            ExpectedConfigurationFingerprint = "preview-only"
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.False(result.WasExecuted);
        Assert.False(result.WasPersisted);
        Assert.False(result.WasProfileChanged);
        Assert.Contains("frisch geladen", result.Message);
        Assert.True(result.FreshLoadPerformed);
        Assert.False(result.FinalReEvaluationPerformed);
        Assert.True(result.RequiresFinalReEvaluation);
        Assert.False(result.SaveDryRunPerformed);
        Assert.True(result.SaveDryRunBlocked);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.userDefined" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.profile.notBuiltIn" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.finalEvaluationService.missing" && !item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.saveDryRun.checked" && !item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.saveDryRun.finalReEvaluation.completed" && !item.IsSatisfied);
        Assert.Null(store.LastSaveRequest);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndFinalServices_ShouldSimulateReadyReEvaluationAndSaveDryRun()
    {
        var context = CreateFinalEvaluationContext(isLicenseRequired: false);
        var profile = context.Profile;
        var originalIsActive = profile.IsActive;
        var originalAttachmentEnabled = profile.FolderOptions.IsAttachmentProcessingEnabled;
        var store = new RecordingActivationProfileStore(profile);
        var executor = CreateExecutorWithFinalServices(store, context.Catalog);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null,
            EvaluationResult = null,
            GuardResult = null,
            WarningConfirmationResult = null,
            ActivationPlan = null
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(1, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.False(result.ProfileChanged);
        Assert.False(result.ProcessingStarted);
        Assert.True(result.FreshLoadPerformed);
        Assert.True(result.FinalReEvaluationPerformed);
        Assert.True(result.GuardRechecked);
        Assert.True(result.WarningConfirmationRechecked);
        Assert.True(result.ActivationPlanRecreated);
        Assert.True(result.SaveDryRunPerformed);
        Assert.False(result.SaveDryRunBlocked);
        Assert.False(result.RequiresFinalReEvaluation);
        Assert.NotNull(store.LastSaveRequest);
        Assert.True(store.LastSaveRequest.FinalReEvaluationCompleted);
        Assert.Equal(InterfaceProfileActivationExecutorOperationMode.ValidateOnly, store.LastSaveRequest.OperationMode);
        Assert.False(profile.IsActive);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.finalEvaluation.performed" && item.IsSatisfied);
        Assert.Contains(result.Preconditions, item => item.Code == "store.saveDryRun.notPersisted" && item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndFinalServices_ShouldRequireWarningConfirmationWithoutAcceptance()
    {
        var context = CreateFinalEvaluationContext(isLicenseRequired: true);
        var profile = context.Profile;
        var store = new RecordingActivationProfileStore(profile);
        var executor = CreateExecutorWithFinalServices(store, context.Catalog);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null,
            EvaluationResult = null,
            GuardResult = null,
            WarningConfirmationResult = null,
            ActivationPlan = null,
            WarningsAccepted = false
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.RequiresWarningConfirmation, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.True(result.FinalReEvaluationPerformed);
        Assert.False(result.SaveDryRunPerformed);
        Assert.True(result.SaveDryRunBlocked);
        Assert.False(profile.IsActive);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && !item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreAndFinalServices_ShouldAllowAcceptedWarningsOnlyAsDryRun()
    {
        var context = CreateFinalEvaluationContext(isLicenseRequired: true);
        var profile = context.Profile;
        var store = new RecordingActivationProfileStore(profile);
        var executor = CreateExecutorWithFinalServices(store, context.Catalog);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            Profile = null,
            EvaluationResult = null,
            GuardResult = null,
            WarningConfirmationResult = null,
            ActivationPlan = null,
            WarningsAccepted = true,
            ConfirmedWarningCodes = new[] { "license.required" }
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.Equal(1, store.LoadCallCount);
        Assert.Equal(1, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.False(result.ProfileChanged);
        Assert.True(result.FinalReEvaluationPerformed);
        Assert.True(result.SaveDryRunPerformed);
        Assert.False(result.SaveDryRunBlocked);
        Assert.False(profile.IsActive);
        Assert.Contains(result.Preconditions, item => item.Code == "warnings.accepted" && item.IsSatisfied);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldNotUseStoreForActivateMode()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = false };
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            OperationMode = InterfaceProfileActivationExecutorOperationMode.Activate
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.Equal(0, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.False(result.ProfileChanged);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_WithStore_ShouldNotDeactivateOrSaveInDeactivateMode()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = true };
        var store = new RecordingActivationProfileStore(profile);
        var executor = new InterfaceProfileActivationExecutorStub(store);

        var result = await executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            OperationMode = InterfaceProfileActivationExecutorOperationMode.Deactivate
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotImplemented, result.Status);
        Assert.Equal(0, store.LoadCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.False(result.Success);
        Assert.False(result.Saved);
        Assert.False(result.ProfileChanged);
        Assert.True(profile.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTreatPreviewResultsAsNonFinalContext()
    {
        var profile = CreateUserDefinedProfile();
        var request = CreateReadyRequest(profile) with
        {
            PreviewCreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpectedConfigurationFingerprint = "preview-fingerprint",
            ExpectedEvaluationStatus = InterfaceProfileActivationStatus.Ready,
            ExpectedActivationPlanStatus = InterfaceProfileActivationPlanStatus.Ready
        };

        var result = await _executor.ExecuteAsync(request);

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.Success);
        Assert.True(result.RequiresFinalReEvaluation);
        Assert.Contains(result.Preconditions, item => item.Code == "executor.finalReEvaluation.required" && !item.IsSatisfied);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDescribeValidateOnlyModeWithoutExecution()
    {
        var result = await _executor.ExecuteAsync(CreateReadyRequest(CreateUserDefinedProfile()) with
        {
            OperationMode = InterfaceProfileActivationExecutorOperationMode.ValidateOnly
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.True(result.IsValidationOnly);
        Assert.False(result.WasExecuted);
        Assert.False(result.Saved);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnReadyButNotExecutedForActivateModeWithoutPersisting()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = false };

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            OperationMode = InterfaceProfileActivationExecutorOperationMode.Activate
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.ReadyButNotExecuted, result.Status);
        Assert.False(result.IsValidationOnly);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotDeactivateInDeactivateMode()
    {
        var profile = CreateUserDefinedProfile() with { IsActive = true };

        var result = await _executor.ExecuteAsync(CreateReadyRequest(profile) with
        {
            OperationMode = InterfaceProfileActivationExecutorOperationMode.Deactivate
        });

        Assert.Equal(InterfaceProfileActivationExecutorStatus.NotImplemented, result.Status);
        Assert.False(result.Success);
        Assert.False(result.ProfileChanged);
        Assert.False(result.Saved);
        Assert.False(result.ProcessingStarted);
        Assert.True(profile.IsActive);
    }

    private static InterfaceProfileActivationExecutorRequest CreateReadyRequest(
        InterfaceProfileDefinition profile)
    {
        return new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(profile, InterfaceProfileActivationPlanStatus.Ready),
            EvaluationResult: Evaluation(InterfaceProfileActivationStatus.Ready, canActivate: true),
            GuardResult: Guard(true, InterfaceProfileActivationGuardDecision.Allowed),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.NoWarnings),
            WarningsAccepted: false,
            InterfaceProfileId: profile.Metadata.Id,
            InterfaceProfileName: profile.Metadata.Name,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            Source: "Test");
    }

    private static InterfaceProfileActivationExecutorRequest CreateReadyWithWarningsRequest(
        InterfaceProfileDefinition profile,
        bool warningsAccepted)
    {
        return new InterfaceProfileActivationExecutorRequest(
            Profile: profile,
            ActivationPlan: Plan(
                profile,
                warningsAccepted
                    ? InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings
                    : InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation),
            EvaluationResult: Evaluation(
                InterfaceProfileActivationStatus.ReadyWithWarnings,
                canActivate: true,
                Check(InterfaceProfileActivationSeverity.Warning, "license.required", "Lizenzstatus prüfen.")),
            GuardResult: Guard(
                warningsAccepted,
                warningsAccepted
                    ? InterfaceProfileActivationGuardDecision.AllowedWithWarnings
                    : InterfaceProfileActivationGuardDecision.RequiresWarningConfirmation),
            WarningConfirmationResult: WarningConfirmation(InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired),
            WarningsAccepted: warningsAccepted,
            InterfaceProfileId: profile.Metadata.Id,
            InterfaceProfileName: profile.Metadata.Name,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            Source: "Test",
            ConfirmedWarningCodes: warningsAccepted
                ? new[] { "license.required" }
                : Array.Empty<string>());
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined",
                Name = "UserDefined Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            IsActive = false
        };
    }

    private static InterfaceProfileDefinition CreateNonUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-non-userdefined",
                Name = "Nicht UserDefined Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = false
            },
            IsActive = false
        };
    }

    private static InterfaceProfileActivationExecutorStub CreateExecutorWithFinalServices(
        IInterfaceProfileActivationProfileStore store,
        ProfileCatalog catalog)
    {
        return new InterfaceProfileActivationExecutorStub(
            store,
            new InterfaceProfileActivationEvaluationService(),
            new InterfaceProfileActivationGuardService(),
            new InterfaceProfileActivationWarningConfirmationService(),
            new InterfaceProfileActivationPlanService(),
            catalog);
    }

    private static FinalEvaluationTestContext CreateFinalEvaluationContext(bool isLicenseRequired)
    {
        var folders = CreateFinalEvaluationFolders();
        var aisProfile = DefaultAisProfiles.CreateMedistarDefault();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = isLicenseRequired
                    ? "interface-userdefined-final-warning"
                    : "interface-userdefined-final-ready",
                Name = isLicenseRequired
                    ? "UserDefined finale Warnung"
                    : "UserDefined finale Ready",
                IsBuiltIn = false,
                IsUserDefined = true
            },
            AisProfileId = aisProfile.Metadata.Id,
            DeviceProfileId = deviceProfile.Metadata.Id,
            ExportProfileId = exportProfile.Metadata.Id,
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
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
            },
            IsActive = false,
            IsLicenseRequired = isLicenseRequired
        };
        var catalog = new ProfileCatalog(
            AisProfiles: new[] { aisProfile },
            DeviceProfiles: new[] { deviceProfile },
            ExportProfiles: new[] { exportProfile },
            InterfaceProfiles: new[] { profile });

        return new FinalEvaluationTestContext(profile, catalog);
    }

    private static FinalEvaluationFolders CreateFinalEvaluationFolders()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        var aisImport = Path.Combine(baseFolder, "ais-import");
        var deviceImport = Path.Combine(baseFolder, "device-import");
        var export = Path.Combine(baseFolder, "export");
        var archive = Path.Combine(baseFolder, "archive");
        var error = Path.Combine(baseFolder, "error");
        Directory.CreateDirectory(aisImport);
        Directory.CreateDirectory(deviceImport);
        Directory.CreateDirectory(export);
        Directory.CreateDirectory(archive);
        Directory.CreateDirectory(error);

        return new FinalEvaluationFolders(aisImport, deviceImport, export, archive, error);
    }

    private static InterfaceProfileActivationEvaluationResult Evaluation(
        InterfaceProfileActivationStatus status,
        bool canActivate,
        params InterfaceProfileActivationCheckResult[] checks)
    {
        return new InterfaceProfileActivationEvaluationResult(status, canActivate, checks);
    }

    private static InterfaceProfileActivationCheckResult Check(
        InterfaceProfileActivationSeverity severity,
        string code,
        string message)
    {
        return new InterfaceProfileActivationCheckResult(
            Area: "Test",
            Code: code,
            Message: message,
            Severity: severity,
            Detail: null);
    }

    private static InterfaceProfileActivationGuardResult Guard(
        bool canProceed,
        InterfaceProfileActivationGuardDecision decision)
    {
        return new InterfaceProfileActivationGuardResult(
            canProceed,
            decision,
            BlockerReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            WarningReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            InfoReasons: Array.Empty<InterfaceProfileActivationGuardReason>(),
            Message: decision.ToString());
    }

    private static InterfaceProfileActivationWarningConfirmationResult WarningConfirmation(
        InterfaceProfileActivationWarningConfirmationStatus status)
    {
        return new InterfaceProfileActivationWarningConfirmationResult(
            CanRequestConfirmation: status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired,
            status,
            ProfileId: "interface-userdefined",
            ProfileName: "UserDefined Schnittstelle",
            Warnings: status == InterfaceProfileActivationWarningConfirmationStatus.ConfirmationRequired
                ? new[]
                {
                    new InterfaceProfileActivationWarningConfirmationItem(
                        Area: "Test",
                        Code: "license.required",
                        Title: "Lizenzstatus prüfen.",
                        Detail: null,
                        Severity: InterfaceProfileActivationSeverity.Warning,
                        IsRequiredForActivation: true)
                }
                : Array.Empty<InterfaceProfileActivationWarningConfirmationItem>(),
            BlockingReasons: Array.Empty<InterfaceProfileActivationWarningConfirmationReason>(),
            Message: status.ToString());
    }

    private static InterfaceProfileActivationPlan Plan(
        InterfaceProfileDefinition profile,
        InterfaceProfileActivationPlanStatus status)
    {
        var canExecute = status is InterfaceProfileActivationPlanStatus.Ready or
            InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings;

        return new InterfaceProfileActivationPlan(
            status,
            CanExecuteLater: canExecute,
            IsPreviewOnly: true,
            ProfileId: profile.Metadata.Id,
            ProfileName: profile.Metadata.Name,
            SummaryMessage: status.ToString(),
            Blockers: status == InterfaceProfileActivationPlanStatus.Blocked
                ? new[]
                {
                    new InterfaceProfileActivationPlanReason(
                        InterfaceProfileActivationSeverity.Blocker,
                        "plan.blocked",
                        "Plan blockiert.")
                }
                : Array.Empty<InterfaceProfileActivationPlanReason>(),
            Warnings: status is InterfaceProfileActivationPlanStatus.RequiresWarningConfirmation or
                InterfaceProfileActivationPlanStatus.ReadyWithAcceptedWarnings
                ? new[]
                {
                    new InterfaceProfileActivationPlanReason(
                        InterfaceProfileActivationSeverity.Warning,
                        "license.required",
                        "Lizenzstatus prüfen.")
                }
                : Array.Empty<InterfaceProfileActivationPlanReason>(),
            Infos: Array.Empty<InterfaceProfileActivationPlanReason>(),
            PlannedSteps: new[]
            {
                new InterfaceProfileActivationPlanStep(
                    Code: "activate.profile",
                    Title: "Profil aktivieren",
                    Description: "Beschreibender Schritt.",
                    WouldExecuteLater: true,
                    IsBlocked: !canExecute,
                    Severity: canExecute
                        ? InterfaceProfileActivationSeverity.Info
                        : InterfaceProfileActivationSeverity.Warning)
            },
            MissingRequirements: Array.Empty<InterfaceProfileActivationPlanReason>());
    }

    private sealed record FinalEvaluationTestContext(
        InterfaceProfileDefinition Profile,
        ProfileCatalog Catalog);

    private sealed record FinalEvaluationFolders(
        string AisImportFolder,
        string DeviceImportFolder,
        string ExportFolder,
        string ArchiveFolder,
        string ErrorFolder);

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
            var preconditions = SavePreconditions(request);
            if (profile is null)
            {
                return SaveResult(
                    InterfaceProfileActivationProfileStoreStatus.NotAvailable,
                    profile,
                    wouldSave: false,
                    success: false,
                    "Kein Profil uebergeben.",
                    preconditions);
            }

            if (profile.Metadata.IsBuiltIn)
            {
                return SaveResult(
                    InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked,
                    profile,
                    wouldSave: false,
                    success: false,
                    "BuiltIn blockiert.",
                    preconditions);
            }

            if (!profile.Metadata.IsUserDefined)
            {
                return SaveResult(
                    InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked,
                    profile,
                    wouldSave: false,
                    success: false,
                    "UserDefined erforderlich.",
                    preconditions);
            }

            if (!request.FinalReEvaluationCompleted)
            {
                return SaveResult(
                    InterfaceProfileActivationProfileStoreStatus.MissingCapability,
                    profile,
                    wouldSave: false,
                    success: false,
                    "Finale Re-Evaluation fehlt.",
                    preconditions);
            }

            return SaveResult(
                InterfaceProfileActivationProfileStoreStatus.SaveWouldBeAllowed,
                profile,
                wouldSave: true,
                success: true,
                "ValidateOnly wuerde speichern, hat aber nicht gespeichert.",
                preconditions);
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

        private static InterfaceProfileActivationProfileSaveResult SaveResult(
            InterfaceProfileActivationProfileStoreStatus status,
            InterfaceProfileDefinition? profile,
            bool wouldSave,
            bool success,
            string message,
            IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> preconditions)
        {
            return new InterfaceProfileActivationProfileSaveResult(
                status,
                success,
                profile?.Metadata.Id ?? string.Empty,
                profile?.Metadata.Name ?? string.Empty,
                wouldSave,
                WasSaved: false,
                ProfileChanged: false,
                profile?.Metadata.IsUserDefined == true,
                profile?.Metadata.IsBuiltIn == true,
                message,
                preconditions);
        }

        private static IReadOnlyList<InterfaceProfileActivationExecutorPrecondition> SavePreconditions(
            InterfaceProfileActivationProfileSaveRequest request)
        {
            return new[]
            {
                new InterfaceProfileActivationExecutorPrecondition(
                    "executor.finalReEvaluation.completed",
                    "Finale Re-Evaluation nachgewiesen",
                    "Produktives Speichern darf erst nach finaler Re-Evaluation erfolgen.",
                    IsRequired: true,
                    IsSatisfied: request.FinalReEvaluationCompleted,
                    Severity: request.FinalReEvaluationCompleted
                        ? InterfaceProfileActivationSeverity.Info
                        : InterfaceProfileActivationSeverity.Blocker)
            };
        }
    }
}
