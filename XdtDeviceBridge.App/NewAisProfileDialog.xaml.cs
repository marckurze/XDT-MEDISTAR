using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class NewAisProfileDialog : Window
{
    public NewAisProfileDialog()
    {
        InitializeComponent();

        AisSystemComboBox.ItemsSource = new[] { "MEDISTAR", "Generisch / anderes AIS" };
        AisSystemComboBox.SelectedIndex = 0;
        EncodingComboBox.ItemsSource = new[] { "Windows-1252", "UTF-8" };
        EncodingComboBox.SelectedIndex = 0;
        Loaded += (_, _) => ProfileNameTextBox.Focus();
    }

    public UserDefinedAisProfileCreationRequest Request { get; private set; } =
        new(string.Empty, "MEDISTAR", "Windows-1252");

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Request = new UserDefinedAisProfileCreationRequest(
            ProfileName: ProfileNameTextBox.Text,
            SystemName: AisSystemComboBox.SelectedItem as string ?? string.Empty,
            DefaultEncoding: EncodingComboBox.SelectedItem as string ?? string.Empty);
        DialogResult = true;
    }
}
