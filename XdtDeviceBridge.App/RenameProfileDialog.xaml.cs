using System.Windows;

namespace XdtDeviceBridge.App;

public partial class RenameProfileDialog : Window
{
    public RenameProfileDialog(string currentName)
    {
        InitializeComponent();

        CurrentNameTextBox.Text = currentName;
        NewNameTextBox.Text = currentName;
        Loaded += (_, _) =>
        {
            NewNameTextBox.Focus();
            NewNameTextBox.SelectAll();
        };
    }

    public string NewName { get; private set; } = string.Empty;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var newName = NewNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            System.Windows.MessageBox.Show(
                this,
                "Bitte geben Sie einen neuen Namen ein.",
                "Profil umbenennen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        NewName = newName;
        DialogResult = true;
    }
}
