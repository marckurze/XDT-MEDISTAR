using System.ComponentModel;
using System.Windows;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.App;

public partial class FloatingInterfaceProfileWindow : Window
{
    private bool _isClosingFromDock;
    private bool _isUpdatingState;

    public FloatingInterfaceProfileWindow(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            throw new ArgumentException("Schnittstellenprofil-ID fehlt.", nameof(interfaceProfileId));
        }

        InterfaceProfileId = interfaceProfileId;
        InitializeComponent();
    }

    public event EventHandler? DockRequested;
    public event EventHandler<bool>? PinChanged;
    public event EventHandler<bool>? PositionMemoryChanged;
    public event EventHandler? PositionRememberRequested;

    public string InterfaceProfileId { get; }

    public void ApplyState(InterfaceProfileFloatingWindowState state)
    {
        _isUpdatingState = true;
        try
        {
            PinToggleButton.IsChecked = state.IsPinned;
            PositionMemoryToggleButton.IsChecked = state.IsPositionMemoryEnabled;
            Topmost = state.IsPinned;
        }
        finally
        {
            _isUpdatingState = false;
        }
    }

    public InterfaceProfileFloatingWindowBounds CaptureBounds()
    {
        return new InterfaceProfileFloatingWindowBounds(Left, Top, Width, Height);
    }

    public void CloseWithoutDockRequest()
    {
        _isClosingFromDock = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isClosingFromDock)
        {
            e.Cancel = true;
            DockRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        RememberPositionIfEnabled();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RememberPositionIfEnabled();
    }

    private void DockButton_Click(object sender, RoutedEventArgs e)
    {
        DockRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PinToggleButton_Changed(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingState)
        {
            return;
        }

        PinChanged?.Invoke(this, PinToggleButton.IsChecked == true);
    }

    private void PositionMemoryToggleButton_Changed(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingState)
        {
            return;
        }

        var isEnabled = PositionMemoryToggleButton.IsChecked == true;
        PositionMemoryChanged?.Invoke(this, isEnabled);
        if (isEnabled)
        {
            PositionRememberRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RememberPositionIfEnabled()
    {
        if (_isUpdatingState || PositionMemoryToggleButton.IsChecked != true)
        {
            return;
        }

        PositionRememberRequested?.Invoke(this, EventArgs.Empty);
    }
}
