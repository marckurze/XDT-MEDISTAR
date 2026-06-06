using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class AppSettingsDialog : Window
{
    private readonly bool _isTabProtectionPasswordSet;

    public AppSettingsDialog(XdtBoxAppSettings settings, bool isTabProtectionPasswordSet = false)
    {
        InitializeComponent();
        _isTabProtectionPasswordSet = isTabProtectionPasswordSet;
        Settings = (settings ?? XdtBoxAppSettings.CreateDefault()).Clone();
        ShowSettings();
    }

    public XdtBoxAppSettings Settings { get; private set; }

    public string? NewTabProtectionPassword { get; private set; }

    public bool RemoveTabProtection { get; private set; }

    private void ShowSettings()
    {
        StartMinimizedToTrayCheckBox.IsChecked = Settings.StartMinimizedToTray;
        AutoStartMonitoringCheckBox.IsChecked = Settings.AutoStartMonitoringOnAppStart;
        CloseToTrayInsteadOfExitCheckBox.IsChecked = Settings.CloseToTrayInsteadOfExit;
        ConfirmExitWhileMonitoringCheckBox.IsChecked = Settings.ConfirmExitWhileMonitoring;
        TabProtectionCurrentStateTextBlock.Text = _isTabProtectionPasswordSet
            ? "Aktuell ist ein Tab-Schutz-Passwort eingerichtet. Zum Ändern ein neues Passwort eintragen oder den Schutz entfernen."
            : "Aktuell ist kein Tab-Schutz-Passwort eingerichtet.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var newPassword = TabProtectionPasswordBox.Password;
        var repeatPassword = TabProtectionPasswordRepeatBox.Password;
        if (!string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(repeatPassword))
        {
            if (!string.Equals(newPassword, repeatPassword, StringComparison.Ordinal))
            {
                System.Windows.MessageBox.Show(
                    this,
                    "Die beiden Tab-Schutz-Passwörter stimmen nicht überein.",
                    "XDTBox Einstellungen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 4)
            {
                System.Windows.MessageBox.Show(
                    this,
                    "Bitte ein Tab-Schutz-Passwort mit mindestens vier Zeichen verwenden.",
                    "XDTBox Einstellungen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
        }

        Settings = new XdtBoxAppSettings
        {
            StartMinimizedToTray = StartMinimizedToTrayCheckBox.IsChecked == true,
            AutoStartMonitoringOnAppStart = AutoStartMonitoringCheckBox.IsChecked == true,
            CloseToTrayInsteadOfExit = CloseToTrayInsteadOfExitCheckBox.IsChecked == true,
            ConfirmExitWhileMonitoring = ConfirmExitWhileMonitoringCheckBox.IsChecked == true
        };
        NewTabProtectionPassword = string.IsNullOrWhiteSpace(newPassword) ? null : newPassword;
        RemoveTabProtection = RemoveTabProtectionCheckBox.IsChecked == true && NewTabProtectionPassword is null;

        DialogResult = true;
    }
}
