using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileUiPolicyTests
{
    [Fact]
    public void ShouldShowDeviceOutput_ForCv5000AndHideAisAttachmentOptions()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        Assert.True(InterfaceProfileUiPolicy.ShouldShowDeviceOutput(interfaceProfile, deviceProfile));
        Assert.False(InterfaceProfileUiPolicy.ShouldShowAisAttachmentOptions(interfaceProfile, deviceProfile));
    }

    [Theory]
    [InlineData("ARK1S")]
    [InlineData("NT530P")]
    [InlineData("Dokumentanhang")]
    [InlineData("Manuelle Dokumentübergabe")]
    public void ShouldKeepAisAttachmentOptionsVisible_ForNonCv5000Profiles(string profileKind)
    {
        var (interfaceProfile, deviceProfile) = CreateNonCv5000Profile(profileKind);

        Assert.False(InterfaceProfileUiPolicy.ShouldShowDeviceOutput(interfaceProfile, deviceProfile));
        Assert.True(InterfaceProfileUiPolicy.ShouldShowAisAttachmentOptions(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void ShouldShowDeviceOutput_ForUserDefinedBidirectionalDevice()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            Metadata = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault().Metadata with
            {
                Id = "device-user-phoropter"
            },
            Model = "Praxis-Phoropter",
            IsBidirectional = true
        };
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            DeviceProfileId = deviceProfile.Metadata.Id
        };

        Assert.True(InterfaceProfileUiPolicy.ShouldShowDeviceOutput(interfaceProfile, deviceProfile));
        Assert.True(InterfaceProfileUiPolicy.ShouldShowAisAttachmentOptions(interfaceProfile, deviceProfile));
    }

    private static (InterfaceProfileDefinition InterfaceProfile, DeviceProfileDefinition DeviceProfile) CreateNonCv5000Profile(string profileKind)
    {
        return profileKind switch
        {
            "ARK1S" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault()),
            "NT530P" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530PDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault()),
            "Dokumentanhang" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
                DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault()),
            "Manuelle Dokumentübergabe" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault(),
                DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault()),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };
    }
}
