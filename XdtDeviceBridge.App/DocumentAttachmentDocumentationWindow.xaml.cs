using System.Windows;

namespace XdtDeviceBridge.App;

public partial class DocumentAttachmentDocumentationWindow : Window
{
    public DocumentAttachmentDocumentationWindow(string profileName, IReadOnlyList<string> fileNames)
    {
        InitializeComponent();
        ProfileNameTextBlock.Text = string.IsNullOrWhiteSpace(profileName)
            ? "Dokumentgerät"
            : profileName;
        FileNamesItemsControl.ItemsSource = fileNames.Count == 0
            ? new[] { "Keine Datei ausgewählt." }
            : fileNames;
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
        DialogResult = false;
    }
}
