using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.App;

public partial class FloatingInterfaceProfileWindow : Window
{
    private bool _isClosingFromDock;
    private bool _isUpdatingState;
    private string _pilotOrbAnimationKey = "";
    private string _pilotOrbFlashKey = "";

    public FloatingInterfaceProfileWindow(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            throw new ArgumentException("Schnittstellenprofil-ID fehlt.", nameof(interfaceProfileId));
        }

        InterfaceProfileId = interfaceProfileId;
        InitializeComponent();
        DataContextChanged += FloatingInterfaceProfileWindow_DataContextChanged;
    }

    public event EventHandler? DockRequested;
    public event EventHandler<bool>? PinChanged;
    public event EventHandler<bool>? PositionMemoryChanged;
    public event EventHandler? PositionRememberRequested;
    public event EventHandler<int>? ScanIntervalChangeRequested;
    public event EventHandler? ResetRequested;

    public string InterfaceProfileId { get; }

    public void ApplyState(InterfaceProfileFloatingWindowState state)
    {
        if (!string.Equals(state.InterfaceProfileId, InterfaceProfileId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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
        if (_isClosingFromDock)
        {
            return;
        }

        _isClosingFromDock = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isClosingFromDock)
        {
            _isClosingFromDock = true;
            DockRequested?.Invoke(this, EventArgs.Empty);
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

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DecreaseScanIntervalButton_Click(object sender, RoutedEventArgs e)
    {
        ScanIntervalChangeRequested?.Invoke(this, -1);
    }

    private void IncreaseScanIntervalButton_Click(object sender, RoutedEventArgs e)
    {
        ScanIntervalChangeRequested?.Invoke(this, 1);
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

    private void FloatingMonitoringRadar_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateRadarAnimation();
    }

    private void FloatingMonitoringRadar_Unloaded(object sender, RoutedEventArgs e)
    {
        StopRadarAnimation();
    }

    private void FloatingMonitoringRadar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateRadarAnimation();
    }

    private void FloatingMonitoringRadar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateRadarAnimation();
    }

    private void UpdateRadarAnimation()
    {
        if (DataContext is not InterfaceMonitoringCardDisplay card)
        {
            StopRadarAnimation();
            return;
        }

        var scanBarTransform = EnsureMutableTranslateTransform(FloatingRadarScanBar);
        if (!card.IsScanAnimationActive)
        {
            StopRadarAnimation();
            FloatingMonitoringRadarSurface.Opacity = 0.72;
            scanBarTransform.X = 0;
            return;
        }

        FloatingMonitoringRadarSurface.Opacity = 1;
        var scanIntervalSeconds = Math.Clamp(card.ScanIntervalSeconds, 1, 60);
        var surfaceWidth = FloatingMonitoringRadarSurface.ActualWidth > 0 ? FloatingMonitoringRadarSurface.ActualWidth : 320;
        var barWidth = FloatingRadarScanBar.ActualWidth > 0
            ? FloatingRadarScanBar.ActualWidth
            : double.IsNaN(FloatingRadarScanBar.Width) || FloatingRadarScanBar.Width <= 0
                ? 17
                : FloatingRadarScanBar.Width;
        var travelDistance = Math.Max(0, surfaceWidth - barWidth);
        var oneWayDurationSeconds = Math.Max(0.4, scanIntervalSeconds / 2.0);

        var scanAnimation = new DoubleAnimation
        {
            From = 0,
            To = travelDistance,
            Duration = new Duration(TimeSpan.FromSeconds(oneWayDurationSeconds)),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        scanBarTransform.BeginAnimation(TranslateTransform.XProperty, scanAnimation);

        var pulseAnimation = new DoubleAnimation
        {
            From = 0.42,
            To = 0.9,
            Duration = new Duration(TimeSpan.FromSeconds(Math.Max(0.4, oneWayDurationSeconds))),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        FloatingRadarScanBar.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
    }

    private void StopRadarAnimation()
    {
        if (FloatingRadarScanBar.RenderTransform is TranslateTransform scanBarTransform
            && !scanBarTransform.IsFrozen)
        {
            scanBarTransform.BeginAnimation(TranslateTransform.XProperty, null);
        }

        FloatingRadarScanBar.BeginAnimation(UIElement.OpacityProperty, null);
    }

    private static TranslateTransform EnsureMutableTranslateTransform(FrameworkElement element)
    {
        if (element.RenderTransform is TranslateTransform transform && !transform.IsFrozen)
        {
            return transform;
        }

        transform = new TranslateTransform();
        element.RenderTransform = transform;
        return transform;
    }

    private void FloatingInterfaceProfileWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ApplyPilotWindowSizing();
    }

    private void FloatingPilotStatusOrb_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdatePilotStatusOrbAnimation(element);
        }
    }

    private void FloatingPilotStatusOrb_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            StopPilotStatusOrbAnimation(element);
        }
    }

    private void FloatingPilotStatusOrb_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdatePilotStatusOrbAnimation(element);
        }
    }

    private void ApplyPilotWindowSizing()
    {
        if (DataContext is not InterfaceMonitoringCardDisplay { UsesPilotDeviceVisual: true })
        {
            return;
        }

        MinWidth = Math.Max(MinWidth, 620);
        MinHeight = Math.Max(MinHeight, 430);
        if (Width < 690)
        {
            Width = 690;
        }

        if (Height < 540)
        {
            Height = 540;
        }
    }

    private void UpdatePilotStatusOrbAnimation(FrameworkElement element)
    {
        if (element.DataContext is not InterfaceMonitoringCardDisplay { UsesPilotDeviceVisual: true } card)
        {
            StopPilotStatusOrbAnimation(element);
            return;
        }

        var orb = FindVisualChildByTag<FrameworkElement>(element, "StatusOrb");
        if (orb is null)
        {
            return;
        }

        var pulseSeconds = InterfaceProfileUiPolicy.GetStatusOrbPulseDurationSeconds(card.ScanIntervalSeconds);
        var animationKey = $"{card.InterfaceProfileId}|{card.ScanIntervalSeconds}|{card.IsScanAnimationActive}";
        if (!string.Equals(_pilotOrbAnimationKey, animationKey, StringComparison.Ordinal))
        {
            _pilotOrbAnimationKey = animationKey;
            var scaleTransform = EnsureMutableScaleTransform(orb);
            var scaleAnimation = new DoubleAnimation
            {
                From = 0.92,
                To = 1.08,
                Duration = new Duration(TimeSpan.FromSeconds(pulseSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.72,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(pulseSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            orb.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        var flashKey = CreateDeviceInputFlashKey(card);
        if (!string.IsNullOrWhiteSpace(flashKey)
            && !string.Equals(_pilotOrbFlashKey, flashKey, StringComparison.Ordinal))
        {
            _pilotOrbFlashKey = flashKey;
            StartStatusOrbFlash(element);
        }
    }

    private void StopPilotStatusOrbAnimation(FrameworkElement element)
    {
        var orb = FindVisualChildByTag<FrameworkElement>(element, "StatusOrb");
        if (orb?.RenderTransform is ScaleTransform scaleTransform && !scaleTransform.IsFrozen)
        {
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        orb?.BeginAnimation(UIElement.OpacityProperty, null);
        _pilotOrbAnimationKey = "";
    }

    private static void StartStatusOrbFlash(DependencyObject element)
    {
        var flash = FindVisualChildByTag<FrameworkElement>(element, "StatusOrbFlash");
        if (flash is null)
        {
            return;
        }

        flash.Opacity = 0;
        var flashAnimation = new DoubleAnimation
        {
            From = 0.95,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(820)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        flash.BeginAnimation(UIElement.OpacityProperty, flashAnimation);
    }

    private static string CreateDeviceInputFlashKey(InterfaceMonitoringCardDisplay card)
    {
        return string.IsNullOrWhiteSpace(card.AisFileName) && string.IsNullOrWhiteSpace(card.DeviceFileName)
            ? string.Empty
            : $"{card.AisFileName}|{card.DeviceFileName}";
    }

    private static ScaleTransform EnsureMutableScaleTransform(FrameworkElement element)
    {
        if (element.RenderTransform is ScaleTransform transform && !transform.IsFrozen)
        {
            return transform;
        }

        transform = new ScaleTransform(1, 1);
        element.RenderTransform = transform;
        return transform;
    }

    private static T? FindVisualChildByTag<T>(DependencyObject parent, object tag)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild && Equals(typedChild.Tag, tag))
            {
                return typedChild;
            }

            var descendant = FindVisualChildByTag<T>(child, tag);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
