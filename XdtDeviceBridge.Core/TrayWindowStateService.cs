namespace XdtDeviceBridge.Core;

public sealed class TrayWindowStateService
{
    public bool IsInTray { get; private set; }

    public bool IsExitRequested { get; private set; }

    public bool HasShownTrayHint { get; private set; }

    public TrayWindowStateDecision MinimizeToTray()
    {
        IsInTray = true;
        var shouldShowHint = !HasShownTrayHint;
        HasShownTrayHint = true;
        return new TrayWindowStateDecision(
            ShouldHideWindow: true,
            ShouldShowHint: shouldShowHint);
    }

    public TrayWindowStateDecision RestoreFromTray()
    {
        IsInTray = false;
        return new TrayWindowStateDecision(ShouldShowWindow: true);
    }

    public TrayWindowStateDecision RequestExit()
    {
        IsExitRequested = true;
        IsInTray = false;
        return new TrayWindowStateDecision(ShouldExit: true);
    }

    public bool ShouldCancelClose()
    {
        return !IsExitRequested;
    }
}
