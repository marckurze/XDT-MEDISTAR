using System.Windows;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class NewDeviceProfileDialog : Window
{
    public NewDeviceProfileDialog()
        : this(Array.Empty<string>())
    {
    }

    public NewDeviceProfileDialog(IEnumerable<string> parserModes)
    {
        InitializeComponent();

        var availableParserModes = parserModes
            .Where(mode => !string.IsNullOrWhiteSpace(mode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(mode => mode, StringComparer.CurrentCultureIgnoreCase)
            .Select(mode => new ParserModeOption(CreateParserModeDisplayName(mode), mode))
            .ToList();

        if (availableParserModes.Count == 0)
        {
            availableParserModes.Add(new ParserModeOption("XML", "Xml"));
        }

        ParserModeComboBox.ItemsSource = availableParserModes;
        ParserModeComboBox.SelectedIndex = 0;
        Loaded += (_, _) => ProfileNameTextBox.Focus();
    }

    public UserDefinedDeviceProfileCreationRequest Request { get; private set; } =
        new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Request = new UserDefinedDeviceProfileCreationRequest(
            ProfileName: ProfileNameTextBox.Text,
            Manufacturer: ManufacturerTextBox.Text,
            Model: ModelTextBox.Text,
            DeviceType: DeviceTypeTextBox.Text,
            ParserMode: ParserModeComboBox.SelectedItem is ParserModeOption option ? option.Value : string.Empty,
            IsBidirectional: BidirectionalDeviceCheckBox.IsChecked == true);
        DialogResult = true;
    }

    private static string CreateParserModeDisplayName(string parserMode)
    {
        return parserMode.Equals("Xml", StringComparison.OrdinalIgnoreCase)
            ? "XML"
            : parserMode.Equals("Csv", StringComparison.OrdinalIgnoreCase)
                ? "CSV"
                : parserMode;
    }

    private sealed record ParserModeOption(string DisplayName, string Value)
    {
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
