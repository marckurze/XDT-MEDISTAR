using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class AppSettingsDialog : Window
{
    public AppSettingsDialog(XdtBoxAppSettings settings)
    {
        InitializeComponent();
        Settings = (settings ?? XdtBoxAppSettings.CreateDefault()).Clone();
        ShowSettings();
    }

    public XdtBoxAppSettings Settings { get; private set; }

    private void ShowSettings()
    {
        StartMinimizedToTrayCheckBox.IsChecked = Settings.StartMinimizedToTray;
        AutoStartMonitoringCheckBox.IsChecked = Settings.AutoStartMonitoringOnAppStart;
        CloseToTrayInsteadOfExitCheckBox.IsChecked = Settings.CloseToTrayInsteadOfExit;
        ConfirmExitWhileMonitoringCheckBox.IsChecked = Settings.ConfirmExitWhileMonitoring;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Settings = new XdtBoxAppSettings
        {
            StartMinimizedToTray = StartMinimizedToTrayCheckBox.IsChecked == true,
            AutoStartMonitoringOnAppStart = AutoStartMonitoringCheckBox.IsChecked == true,
            CloseToTrayInsteadOfExit = CloseToTrayInsteadOfExitCheckBox.IsChecked == true,
            ConfirmExitWhileMonitoring = ConfirmExitWhileMonitoringCheckBox.IsChecked == true
        };

        DialogResult = true;
    }
}
