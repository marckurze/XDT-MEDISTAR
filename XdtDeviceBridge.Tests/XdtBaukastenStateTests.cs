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

    [Fact]
    public void State_ShouldAddAndRemoveWorkingRulesWithoutChangingSourceProfile()
    {
        var state = new XdtBaukastenState();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        state.SetExportProfile(exportProfile);

        var rule = new ExportRuleDefinition(
            "baukasten-new",
            "6228",
            "Neue Notiz",
            ExportRuleType.Template,
            null,
            "Testüberschrift",
            999,
            true,
            null);

        state.AddWorkingRule(rule);
        Assert.Contains(state.WorkingExportRules, current => current.Id == "baukasten-new");
        Assert.DoesNotContain(exportProfile.Rules, current => current.Id == "baukasten-new");

        Assert.True(state.RemoveWorkingRule("baukasten-new"));
        Assert.DoesNotContain(state.WorkingExportRules, current => current.Id == "baukasten-new");
    }

    [Fact]
    public void State_ShouldHoldAisAndDeviceOutputRulesSeparately()
    {
        var state = new XdtBaukastenState();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default();

        state.SetDeviceProfile(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());
        state.SetExportProfile(exportProfile);

        Assert.Equal(XdtBaukastenRuleDirection.AisExport, state.CurrentRuleDirection);
        Assert.NotEmpty(state.WorkingExportRules);
        Assert.NotEmpty(state.WorkingDeviceOutputRules);

        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var deviceRule = new ExportRuleDefinition(
            "device-output-custom",
            "DeviceOutput/Custom",
            "Custom",
            ExportRuleType.Template,
            null,
            "Custom",
            999,
            true,
            null);

        state.AddWorkingRule(deviceRule);

        Assert.Contains(state.WorkingDeviceOutputRules, rule => rule.Id == "device-output-custom");
        Assert.DoesNotContain(state.WorkingExportRules, rule => rule.Id == "device-output-custom");

        state.SetRuleDirection(XdtBaukastenRuleDirection.AisExport);
        Assert.Equal(exportProfile.Rules.Count, state.CurrentWorkingRules.Count);
    }

    [Fact]
    public void UndoBuffer_ShouldRestoreDeviceOutputRuleChanges()
    {
        var state = new XdtBaukastenState();
        state.SetDeviceProfile(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default());
        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var buffer = new XdtBaukastenUndoBuffer(10);

        buffer.Push(state.CreateSnapshot());
        var rule = state.WorkingDeviceOutputRules[0] with { OutputTemplate = "Geändert" };
        Assert.True(state.UpdateWorkingRule(rule));

        Assert.True(buffer.TryPop(out var snapshot));
        state.RestoreSnapshot(snapshot!);

        Assert.Equal(XdtBaukastenRuleDirection.DeviceOutput, state.CurrentRuleDirection);
        Assert.NotEqual("Geändert", state.WorkingDeviceOutputRules[0].OutputTemplate);
    }

    [Fact]
    public void UndoBuffer_ShouldKeepAtLeastTenStepsAndRestorePreviousState()
    {
        var state = new XdtBaukastenState();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        state.SetExportProfile(exportProfile);
        var buffer = new XdtBaukastenUndoBuffer(10);

        for (var i = 0; i < 12; i++)
        {
            buffer.Push(state.CreateSnapshot());
            var rule = state.WorkingExportRules[0] with { OutputTemplate = $"Schritt {i}" };
            Assert.True(state.UpdateWorkingRule(rule));
        }

        Assert.Equal(10, buffer.Count);
        Assert.True(buffer.TryPop(out var snapshot));
        Assert.NotNull(snapshot);
        state.RestoreSnapshot(snapshot!);

        Assert.Equal("Schritt 10", state.WorkingExportRules[0].OutputTemplate);
    }

    [Fact]
    public void Placeholder_ShouldExposeExampleValueForRightAlignedUi()
    {
        var placeholder = new XdtBaukastenPlaceholder(
            "AIS",
            "Patientennummer",
            "{AIS.PatientNumber}",
            "AIS-Patientennummer",
            "4701-1");

        Assert.Equal("4701-1", placeholder.ExampleValue);
    }
}
