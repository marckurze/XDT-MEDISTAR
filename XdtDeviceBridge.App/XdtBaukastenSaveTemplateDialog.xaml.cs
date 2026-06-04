using System.Windows;

namespace XdtDeviceBridge.App;

public partial class XdtBaukastenSaveTemplateDialog : Window
{
    private readonly IReadOnlyCollection<string> _existingExportProfileNames;

    public XdtBaukastenSaveTemplateDialog(
        string suggestedTemplateName,
        string suggestedDescription,
        IEnumerable<string> existingExportProfileNames)
    {
        _existingExportProfileNames = existingExportProfileNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        InitializeComponent();

        TemplateNameTextBox.Text = suggestedTemplateName;
        DescriptionTextBox.Text = suggestedDescription;
        Loaded += (_, _) =>
        {
            TemplateNameTextBox.Focus();
            TemplateNameTextBox.SelectAll();
        };
    }

    public string TemplateName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = TemplateNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowWarning("Bitte geben Sie einen Templatenamen ein.");
            return;
        }

        if (_existingExportProfileNames.Contains(name))
        {
            ShowWarning("Es existiert bereits ein Exportprofil mit diesem Namen. Bitte wählen Sie einen eindeutigen Templatenamen.");
            return;
        }

        TemplateName = name;
        Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
            ? null
            : DescriptionTextBox.Text.Trim();
        DialogResult = true;
    }

    private void ShowWarning(string message)
    {
        System.Windows.MessageBox.Show(
            this,
            message,
            "Baukasten-Konfiguration speichern",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
