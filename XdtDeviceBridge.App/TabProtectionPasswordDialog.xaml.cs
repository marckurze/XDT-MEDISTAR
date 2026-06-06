using System.Windows;
using System.Windows.Input;

namespace XdtDeviceBridge.App;

public partial class TabProtectionPasswordDialog : Window
{
    public TabProtectionPasswordDialog(string tabName)
    {
        InitializeComponent();
        PromptTextBlock.Text = string.IsNullOrWhiteSpace(tabName)
            ? "Bitte Passwort eingeben, um den geschützten Bereich zu öffnen."
            : $"Bitte Passwort eingeben, um „{tabName}“ zu öffnen.";
        PasswordBox.Focus();
    }

    public string EnteredPassword => PasswordBox.Password;

    public void ShowInvalidPasswordMessage()
    {
        ErrorTextBlock.Text = "Passwort nicht korrekt.";
        PasswordBox.Clear();
        PasswordBox.Focus();
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DialogResult = true;
        }
    }
}
