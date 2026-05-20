using System.Windows;

namespace XdtDeviceBridge.App;

public partial class DocumentAttachmentDocumentationWindow : Window
{
    private readonly bool _requiresTransferConfirmation;

    public DocumentAttachmentDocumentationWindow(
        string profileName,
        IReadOnlyList<string> fileNames,
        bool requiresTransferConfirmation = false)
    {
        InitializeComponent();
        _requiresTransferConfirmation = requiresTransferConfirmation;
        ProfileNameTextBlock.Text = string.IsNullOrWhiteSpace(profileName)
            ? "Dokumentgerät"
            : profileName;
        FileNamesItemsControl.ItemsSource = fileNames.Count == 0
            ? new[] { "Keine Datei ausgewählt." }
            : fileNames;
        if (_requiresTransferConfirmation)
        {
            Title = "Dokumente übertragen";
            ApplyButton.Content = "Übertragen";
            ContinueWithoutTextButton.Content = "Abbrechen";
        }
    }

    public string? DocumentationText { get; private set; }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        DocumentationText = DocumentationTextBox.Text.Trim();
        DialogResult = true;
    }

    private void ContinueWithoutText_Click(object sender, RoutedEventArgs e)
    {
        DocumentationText = null;
        DialogResult = _requiresTransferConfirmation ? false : true;
    }
}
