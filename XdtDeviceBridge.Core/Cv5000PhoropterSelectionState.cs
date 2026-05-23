namespace XdtDeviceBridge.Core;

public enum Cv5000PhoropterSelectionOutcome
{
    Cancel = 0,
    SendSelectedValues = 1,
    SendNothingAndWaitForDeviceResult = 2
}

public enum Cv5000DeviceOutputDialogAction
{
    CancelSelection = 0,
    WriteImportFile = 1,
    WaitForDeviceResultWithoutImport = 2
}

public sealed class Cv5000PhoropterSelectionState
{
    public Cv5000PhoropterSelectionState(bool isTopMost = true)
    {
        IsTopMost = isTopMost;
    }

    public bool IsTopMost { get; private set; }

    public Cv5000PhoropterSelectionOutcome Outcome { get; private set; } = Cv5000PhoropterSelectionOutcome.Cancel;

    public bool ToggleTopMost()
    {
        IsTopMost = !IsTopMost;
        return IsTopMost;
    }

    public void SetTopMost(bool isTopMost)
    {
        IsTopMost = isTopMost;
    }

    public void SelectValuesToSend()
    {
        Outcome = Cv5000PhoropterSelectionOutcome.SendSelectedValues;
    }

    public void SelectSendNothingAndWaitForDeviceResult()
    {
        Outcome = Cv5000PhoropterSelectionOutcome.SendNothingAndWaitForDeviceResult;
    }

    public void Cancel()
    {
        Outcome = Cv5000PhoropterSelectionOutcome.Cancel;
    }
}

public static class Cv5000DeviceOutputDialogDecision
{
    public static Cv5000DeviceOutputDialogAction FromDialogResult(
        bool? dialogResult,
        Cv5000PhoropterSelectionOutcome outcome)
    {
        if (dialogResult == true && outcome == Cv5000PhoropterSelectionOutcome.SendSelectedValues)
        {
            return Cv5000DeviceOutputDialogAction.WriteImportFile;
        }

        if (dialogResult == true && outcome == Cv5000PhoropterSelectionOutcome.SendNothingAndWaitForDeviceResult)
        {
            return Cv5000DeviceOutputDialogAction.WaitForDeviceResultWithoutImport;
        }

        return Cv5000DeviceOutputDialogAction.CancelSelection;
    }
}
