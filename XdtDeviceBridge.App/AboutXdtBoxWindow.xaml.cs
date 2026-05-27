using System.Windows;

namespace XdtDeviceBridge.App;

public partial class AboutXdtBoxWindow : Window
{
    public AboutXdtBoxWindow(string versionText)
    {
        InitializeComponent();
        VersionTextBlock.Text = $"Version: {versionText}";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
