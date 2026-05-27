namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBoxAppSettings
{
    public bool StartMinimizedToTray { get; set; }

    public bool AutoStartMonitoringOnAppStart { get; set; } = true;

    public bool CloseToTrayInsteadOfExit { get; set; } = true;

    public bool ConfirmExitWhileMonitoring { get; set; } = true;

    public static XdtBoxAppSettings CreateDefault()
    {
        return new XdtBoxAppSettings();
    }

    public XdtBoxAppSettings Clone()
    {
        return new XdtBoxAppSettings
        {
            StartMinimizedToTray = StartMinimizedToTray,
            AutoStartMonitoringOnAppStart = AutoStartMonitoringOnAppStart,
            CloseToTrayInsteadOfExit = CloseToTrayInsteadOfExit,
            ConfirmExitWhileMonitoring = ConfirmExitWhileMonitoring
        };
    }
}
