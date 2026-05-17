namespace XdtDeviceBridge.Core;

public sealed record TrayWindowStateDecision(
    bool ShouldHideWindow = false,
    bool ShouldShowWindow = false,
    bool ShouldShowHint = false,
    bool ShouldExit = false);
