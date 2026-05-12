using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationProfileStoreStubTests
{
    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldReturnNotFoundForUnknownProfile()
    {
        var store = new InterfaceProfileActivationProfileStoreStub();

        var result = store.LoadFreshUserDefinedProfile("interface-missing");

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NotFound, result.Status);
        Assert.False(result.Success);
        Assert.False(result.Found);
        Assert.Null(result.Profile);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.present" && !item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldBlockBuiltInProfile()
    {
        var builtInProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var store = new InterfaceProfileActivationProfileStoreStub(new[] { builtInProfile });

        var result = store.LoadFreshUserDefinedProfile(builtInProfile.Metadata.Id);

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked, result.Status);
        Assert.False(result.Success);
        Assert.True(result.Found);
        Assert.True(result.IsBuiltIn);
        Assert.False(result.IsUserDefined);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.notBuiltIn" && !item.IsSatisfied);
    }

    [Fact]
    public void LoadFreshUserDefinedProfile_ShouldAcceptUserDefinedProfile()
    {
        var profile = CreateUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileStoreStub(new[] { profile });

        var result = store.LoadFreshUserDefinedProfile(profile.Metadata.Id);

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.LoadedUserDefined, result.Status);
        Assert.True(result.Success);
        Assert.True(result.Found);
        Assert.Same(profile, result.Profile);
        Assert.True(result.IsUserDefined);
        Assert.False(result.IsBuiltIn);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.userDefined" && item.IsSatisfied);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldReturnNotAvailableWithoutProfile()
    {
        var store = new InterfaceProfileActivationProfileStoreStub();

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(null));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.NotAvailable, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.False(result.ProfileChanged);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldBlockBuiltInProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var store = new InterfaceProfileActivationProfileStoreStub();

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(profile));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.BuiltInBlocked, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.True(result.IsBuiltIn);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.notBuiltIn" && !item.IsSatisfied);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldRequireUserDefinedProfile()
    {
        var profile = CreateNonUserDefinedProfile();
        var store = new InterfaceProfileActivationProfileStoreStub();

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(profile));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.UserDefinedRequired, result.Status);
        Assert.False(result.Success);
        Assert.False(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.False(result.IsUserDefined);
        Assert.Contains(result.Preconditions, item => item.Code == "profile.userDefined" && !item.IsSatisfied);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldModelSaveAsNotImplementedWithoutMutation()
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
        var store = new InterfaceProfileActivationProfileStoreStub(new[] { profile });

        var result = store.SaveUserDefinedProfile(new InterfaceProfileActivationProfileSaveRequest(profile));

        Assert.Equal(InterfaceProfileActivationProfileStoreStatus.SaveNotImplemented, result.Status);
        Assert.False(result.Success);
        Assert.True(result.WouldSave);
        Assert.False(result.WasSaved);
        Assert.False(result.ProfileChanged);
        Assert.Equal(originalId, profile.Metadata.Id);
        Assert.Equal(originalName, profile.Metadata.Name);
        Assert.Equal(originalIsActive, profile.IsActive);
        Assert.Equal(originalAttachmentEnabled, profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Contains("nichts gespeichert", result.Message);
    }

    [Fact]
    public void SaveUserDefinedProfile_ShouldExposeNonProductiveStorePrecondition()
    {
        var store = new InterfaceProfileActivationProfileStoreStub();

        var result = store.SaveUserDefinedProfile(
            new InterfaceProfileActivationProfileSaveRequest(CreateUserDefinedProfile()));

        Assert.Contains(result.Preconditions, item =>
            item.Code == "store.nonProductiveStub" &&
            item.IsRequired &&
            !item.IsSatisfied);
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-userdefined-store",
                Name = "UserDefined Store Schnittstelle",
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
                Id = "interface-non-userdefined-store",
                Name = "Nicht UserDefined Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = false
            },
            IsActive = false
        };
    }
}
