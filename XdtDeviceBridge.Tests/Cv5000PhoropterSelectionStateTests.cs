using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class Cv5000PhoropterSelectionStateTests
{
    [Fact]
    public void ShouldStartWithTopMostEnabled()
    {
        var state = new Cv5000PhoropterSelectionState();

        Assert.True(state.IsTopMost);
        Assert.Equal(Cv5000PhoropterSelectionOutcome.Cancel, state.Outcome);
    }

    [Fact]
    public void ShouldToggleTopMost()
    {
        var state = new Cv5000PhoropterSelectionState();

        Assert.False(state.ToggleTopMost());
        Assert.False(state.IsTopMost);

        Assert.True(state.ToggleTopMost());
        Assert.True(state.IsTopMost);
    }

    [Fact]
    public void ShouldDistinguishSendNothingFromCancel()
    {
        var state = new Cv5000PhoropterSelectionState();

        state.SelectSendNothingAndWaitForDeviceResult();

        Assert.Equal(Cv5000PhoropterSelectionOutcome.SendNothingAndWaitForDeviceResult, state.Outcome);
        Assert.Equal(
            Cv5000DeviceOutputDialogAction.WaitForDeviceResultWithoutImport,
            Cv5000DeviceOutputDialogDecision.FromDialogResult(true, state.Outcome));

        state.Cancel();

        Assert.Equal(Cv5000PhoropterSelectionOutcome.Cancel, state.Outcome);
        Assert.Equal(
            Cv5000DeviceOutputDialogAction.CancelSelection,
            Cv5000DeviceOutputDialogDecision.FromDialogResult(false, state.Outcome));
    }

    [Fact]
    public void ShouldWriteImportFileOnlyForSendSelectedValues()
    {
        Assert.Equal(
            Cv5000DeviceOutputDialogAction.WriteImportFile,
            Cv5000DeviceOutputDialogDecision.FromDialogResult(
                true,
                Cv5000PhoropterSelectionOutcome.SendSelectedValues));
        Assert.Equal(
            Cv5000DeviceOutputDialogAction.CancelSelection,
            Cv5000DeviceOutputDialogDecision.FromDialogResult(
                false,
                Cv5000PhoropterSelectionOutcome.SendSelectedValues));
    }
}
