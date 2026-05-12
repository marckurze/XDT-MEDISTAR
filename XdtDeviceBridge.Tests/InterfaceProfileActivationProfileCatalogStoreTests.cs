using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationProfileCatalogStoreTests
{
    private readonly ProfileCatalogService _catalogService = new();

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldReturnNotAvailableForMissingProfileId()
    {
        var store = new InterfaceProfileActivationProfileCatalogStore(CreateAppDataPaths());

        var result = store.LoadFreshUserDefinedProfile("");

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NotAvailable, result.Status);
        Assert.False(result.Success);
        Assert.False(result.Found);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.id.present" && !item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldReturnNotFoundForUnknownProfileId()
    {
        var store = new InterfaceProfileActivationProfileCatalogStore(CreateAppDataPaths());

        var result = store.LoadFreshUserDefinedProfile("interface-missing");

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NotFound, result.Status);
        Assert.False(result.Success);
        Assert.False(result.Found);
        Assert.Null(result.Profile);
        Assert.Contains(result.Preconditions, item => item.Code == "store.catalog.load" && item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldMarkBuiltInProfileAsBlocked()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        _catalogService.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.LoadFreshUserDefinedProfile(profile.Metadata.Id);

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked, result.Status);
        Assert.False(result.Success);
        Assert.True(result.Found);
        Assert.True(result.IsBuiltIn);
        Assert.False(result.IsUserDefined);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.notBuiltIn" && !item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldBlockNonUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateNonUserDefinedProfile();
        _catalogService.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.LoadFreshUserDefinedProfile(profile.Metadata.Id);

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked, result.Status);
        Assert.False(result.Success);
        Assert.True(result.Found);
        Assert.False(result.IsBuiltIn);
        Assert.False(result.IsUserDefined);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.userDefined" && !item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldLoadUserDefinedProfileFromCatalog()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserDefinedProfile();
        _catalogService.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.LoadFreshUserDefinedProfile(profile.Metadata.Id);

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.LoadedUserDefined, result.Status);
        Assert.True(result.Success);
        Assert.True(result.Found);
        Assert.NotNull(result.Profile);
        Assert.Equal(profile.Metadata.Id, result.Profile.Metadata.Id);
        Assert.True(result.IsUserDefined);
        Assert.False(result.IsBuiltIn);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldDryRunUserDefinedProfileWhenFinalReEvaluationExists()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserDefinedProfile() with { IsActive = false };
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: true));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.SaveWouldBeAllowed, result.Status);
        Assert.True(result.Success);
        Assert.True(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.False(result.ProfileChanged);
        Assert.Contains("nichts gespeichert", result.Message);
        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldBlockBuiltInProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: true));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.True(result.IsBuiltIn);
        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldBlockNonUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateNonUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: true));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NonUserDefinedBlocked, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.False(result.IsUserDefined);
        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldRequireFinalReEvaluation()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: false));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.MissingCapability, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.Contains(result.Preconditions, item =>
            item.Code == "executor.finalReEvaluation.completed" &&
            !item.IsSatisfied);
        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldNotProductivelySaveForActivateMode()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.Activate,
            FinalReEvaluationCompleted: true));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.SaveNotImplemented, result.Status);
        Assert.False(result.Success);
        Assert.True(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.Contains(result.Preconditions, item => item.Code == "store.validateOnly" && !item.IsSatisfied);
        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldNotMutateProfile()
    {
        var paths = CreateAppDataPaths();
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
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        _ = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: true));

        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldNotCreateProfileJson()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileCatalogStore(paths);

        _ = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(
            Profile: profile,
            OperationMode: InterfaceProfileActivationExecutorOperationMode.ValidateOnly,
            FinalReEvaluationCompleted: true));

        Assert.False(ProfileFileExists(paths, profile.Metadata.Id));
        Assert.False(Directory.Exists(Path.Combine(paths.ProfilesFolder, "interfaces")));
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static bool ProfileFileExists(AppDataPaths paths, string profileId)
    {
        return File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", $"{profileId}.json"));
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined-catalog-store",
                Name = "UserDefined Catalog Store Schnittstelle",
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
                Id = "interface-non-userdefined-catalog-store",
                Name = "Nicht UserDefined Catalog Store Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = false
            },
            IsActive = false
        };
    }
}
