namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileFloatingWindowRestoreGate
{
    private bool _isMainWindowReady;
    private bool _initialRestoreStarted;

    public bool CanShowFloatingWindows => _isMainWindowReady;

    public bool MarkMainWindowReady()
    {
        _isMainWindowReady = true;
        if (_initialRestoreStarted)
        {
            return false;
        }

        _initialRestoreStarted = true;
        return true;
    }
}
