using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenStateTests
{
    [Fact]
    public void State_ShouldHoldProfilesAndWorkingExportRulesWithoutChangingSourceProfile()
    {
        var state = new XdtBaukastenState();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default();

        state.SetAisProfile(DefaultAisProfiles.CreateMedistarDefault());
        state.SetDeviceProfile(DefaultDeviceProfileDefinitions.CreateNidekRt6100Default());
        state.SetExportProfile(exportProfile);

        Assert.NotNull(state.AisProfile);
        Assert.NotNull(state.DeviceProfile);
        Assert.NotNull(state.SourceExportProfile);
        Assert.Equal(exportProfile.Rules.Count, state.WorkingExportRules.Count);

        var firstRule = state.WorkingExportRules[0] with { OutputTemplate = "Baukasten-Entwurf" };
        Assert.True(state.UpdateWorkingRule(firstRule));
        var workingProfile = state.CreateWorkingExportProfile();

        Assert.NotNull(workingProfile);
        Assert.Equal("Baukasten-Entwurf", workingProfile!.Rules[0].OutputTemplate);
        Assert.NotEqual("Baukasten-Entwurf", exportProfile.Rules[0].OutputTemplate);
    }

    [Fact]
    public void State_ShouldSwitchPrimaryInputLabelsForSerialDevices()
    {
        var state = new XdtBaukastenState();
        var networkDevice = DefaultDeviceProfileDefinitions.CreateNidekRt6100Default();
        var serialDevice = networkDevice with
        {
            ConnectionKind = DeviceConnectionKind.SerialRs232,
            SerialSettings = SerialCommunicationSettings.Default
        };

        state.SetDeviceProfile(networkDevice);
        Assert.Equal("AIS Datei laden", state.PrimaryInputButtonText);
        Assert.Equal("Anzeige Rohdaten von AIS Datei", state.PrimaryRawInputTitle);

        state.SetDeviceProfile(serialDevice);
        Assert.Equal("COM Port abhören", state.PrimaryInputButtonText);
        Assert.Equal("Anzeige empfangene RS232-Rohdaten", state.PrimaryRawInputTitle);
    }

    [Fact]
    public void State_ShouldStoreSerialInputAsLeftRawInput()
    {
        var state = new XdtBaukastenState();
        var serialInput = new SerialRawDeviceInput(
            PortName: "COM7",
            Settings: SerialCommunicationSettings.Default with { PortName = "COM7" },
            ReceivedAt: new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero),
            Bytes: new byte[] { 0x01, 0x02 },
            RawText: "RAW",
            HexDump: "01 02");

        state.SetSerialInput(serialInput);

        Assert.Same(serialInput, state.SerialInput);
        Assert.NotNull(state.AisInput);
        Assert.Equal("COM7", state.AisInput!.SourcePath);
        Assert.Contains("RAW", state.AisInput.RawText);
        Assert.Contains("Hexdump", state.AisInput.RawText);
    }
}
