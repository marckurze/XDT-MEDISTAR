using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileUiPolicyTests
{
    [Fact]
    public void PilotMonitoringLayoutConstants_ShouldUseBroaderStandardWidths()
    {
        Assert.Equal(576d, InterfaceProfileUiPolicy.PilotMonitoringCardWidth);
        Assert.Equal(672d, InterfaceProfileUiPolicy.PilotFloatingWindowMinWidth);
        Assert.Equal(744d, InterfaceProfileUiPolicy.PilotFloatingWindowDefaultWidth);
        Assert.Equal(120d, InterfaceProfileUiPolicy.MonitoringInputBadgeMinWidth);
        Assert.Equal(176d, InterfaceProfileUiPolicy.MonitoringInputBadgeMaxWidth);
        Assert.Equal(128d, InterfaceProfileUiPolicy.FloatingInputBadgeMinWidth);
        Assert.Equal(188d, InterfaceProfileUiPolicy.FloatingInputBadgeMaxWidth);
        Assert.True(InterfaceProfileUiPolicy.MonitoringInputBadgeMaxWidth > InterfaceProfileUiPolicy.MonitoringInputBadgeMinWidth);
        Assert.True(InterfaceProfileUiPolicy.FloatingInputBadgeMaxWidth > InterfaceProfileUiPolicy.FloatingInputBadgeMinWidth);
    }

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

    [Fact]
    public void ShouldTriggerCv5000DeviceOutput_WhenCv5000OutputIsEnabled()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            DeviceOutput = new DeviceOutputConfiguration(
                IsEnabled: true,
                OutputFolder: @"C:\Phoropter",
                FileNameTemplate: "CVImport.xml",
                Format: "TOPCON CV-5000 XML")
        };

        Assert.True(InterfaceProfileUiPolicy.ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile));
        Assert.Null(InterfaceProfileUiPolicy.ValidateCv5000DeviceOutput(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void ShouldNotTriggerCv5000DeviceOutput_WhenOutputIsDisabled()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        Assert.False(InterfaceProfileUiPolicy.ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile));
        Assert.Equal(
            "Ausgabe an Gerät ist für dieses Schnittstellenprofil nicht aktiv.",
            InterfaceProfileUiPolicy.ValidateCv5000DeviceOutput(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void ShouldRejectCv5000DeviceOutput_WhenOutputFolderIsMissing()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            DeviceOutput = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default().DeviceOutput! with
            {
                IsEnabled = true,
                OutputFolder = string.Empty
            }
        };
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        Assert.True(InterfaceProfileUiPolicy.ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile));
        Assert.Equal("Ausgabeordner an Gerät fehlt.", InterfaceProfileUiPolicy.ValidateCv5000DeviceOutput(interfaceProfile, deviceProfile));
    }

    [Theory]
    [InlineData("ARK1S")]
    [InlineData("NT530P")]
    [InlineData("Dokumentanhang")]
    [InlineData("Manuelle Dokumentübergabe")]
    public void ShouldNotTriggerCv5000DeviceOutput_ForNonCv5000Profiles(string profileKind)
    {
        var (interfaceProfile, deviceProfile) = CreateNonCv5000Profile(profileKind);

        Assert.False(InterfaceProfileUiPolicy.ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void ShouldUseStandardMonitoringVisual_ForCv5000WithBuiltInImage()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();

        Assert.True(InterfaceProfileUiPolicy.ShouldUsePilotMonitoringVisual(interfaceProfile, deviceProfile));
        Assert.Equal(
            InterfaceProfileUiPolicy.TopconCv5000DeviceImagePath,
            InterfaceProfileUiPolicy.GetMonitoringDeviceImagePath(interfaceProfile, deviceProfile));
    }

    [Theory]
    [InlineData("ARK1S")]
    [InlineData("NT530P")]
    [InlineData("Solos")]
    [InlineData("CT800A")]
    [InlineData("KR1")]
    [InlineData("Dokumentanhang")]
    [InlineData("Manuelle Dokumentübergabe")]
    public void ShouldUseStandardMonitoringVisual_ForNonCv5000Profiles(string profileKind)
    {
        var (interfaceProfile, deviceProfile) = CreateNonCv5000Profile(profileKind);

        Assert.True(InterfaceProfileUiPolicy.ShouldUsePilotMonitoringVisual(interfaceProfile, deviceProfile));
        Assert.Equal(string.Empty, InterfaceProfileUiPolicy.GetMonitoringDeviceImagePath(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void GetMonitoringDeviceImagePath_ShouldPreferConfiguredDeviceImage()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() with
        {
            DeviceImagePath = @"C:\Praxis\Bilder\cv5000.png"
        };

        Assert.Equal(@"C:\Praxis\Bilder\cv5000.png", InterfaceProfileUiPolicy.GetMonitoringDeviceImagePath(interfaceProfile, deviceProfile));
    }

    [Theory]
    [InlineData("CV5000", "Phoropter")]
    [InlineData("Solos", "Lensmeter")]
    [InlineData("CT800A", "Tonometer")]
    [InlineData("KR1", "Keratorefraktometer")]
    [InlineData("KR800S", "Keratorefraktometer")]
    [InlineData("ARK1S", "Autorefraktor")]
    [InlineData("Dokumentanhang", "Dokumentgerät")]
    public void GetMonitoringDeviceTypeDisplay_ShouldUseCompactMonitoringLabels(string profileKind, string expectedDisplay)
    {
        var (_, deviceProfile) = profileKind == "CV5000"
            ? (DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default(), DefaultDeviceProfileDefinitions.CreateTopconCv5000Default())
            : CreateNonCv5000Profile(profileKind);

        Assert.Equal(expectedDisplay, InterfaceProfileUiPolicy.GetMonitoringDeviceTypeDisplay(deviceProfile));
    }

    [Fact]
    public void ShouldUseTextAboveImageMonitoringLayout_ShouldStayOffForNormalNames()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default();
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        Assert.False(InterfaceProfileUiPolicy.ShouldUseTextAboveImageMonitoringLayout(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void ShouldUseTextAboveImageMonitoringLayout_ShouldTurnOnForVeryLongDisplayNames()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default() with
        {
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default().Metadata with
            {
                Name = "MEDISTAR + TOPCON KR800S - sehr lange Raum- und Arbeitsplatz-Konfiguration"
            }
        };
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();

        Assert.True(InterfaceProfileUiPolicy.ShouldUseTextAboveImageMonitoringLayout(interfaceProfile, deviceProfile));
    }

    [Fact]
    public void GetStatusOrbPulseDurationSeconds_ShouldScaleWithScanInterval()
    {
        var fast = InterfaceProfileUiPolicy.GetStatusOrbPulseDurationSeconds(1);
        var normal = InterfaceProfileUiPolicy.GetStatusOrbPulseDurationSeconds(5);
        var slow = InterfaceProfileUiPolicy.GetStatusOrbPulseDurationSeconds(30);

        Assert.True(fast < normal);
        Assert.True(normal < slow);
        Assert.Equal(0.65, fast);
        Assert.Equal(2.8, slow);
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
            "Solos" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconSolosDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconSolosDefault()),
            "CT800A" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt800ADefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault()),
            "KR1" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr1Default(),
                DefaultDeviceProfileDefinitions.CreateTopconKr1Default()),
            "KR800S" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default(),
                DefaultDeviceProfileDefinitions.CreateTopconKr800Default()),
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
