using System.Collections.ObjectModel;
using System.Windows;

namespace XdtDeviceBridge.App;

public partial class DocumentAttachmentDocumentationWindow : Window
{
    private readonly bool _requiresTransferConfirmation;
    private readonly bool _capturesDocumentationText;
    private readonly ObservableCollection<string> _fileNames = new();

    public DocumentAttachmentDocumentationWindow(
        string profileName,
        IReadOnlyList<string> fileNames,
        bool requiresTransferConfirmation = false,
        bool capturesDocumentationText = true)
    {
        InitializeComponent();
        _requiresTransferConfirmation = requiresTransferConfirmation;
        _capturesDocumentationText = capturesDocumentationText;
        ProfileNameTextBlock.Text = string.IsNullOrWhiteSpace(profileName)
            ? "Dokumentgerät"
            : profileName;
        FileNamesItemsControl.ItemsSource = _fileNames;
        UpdateFileNames(fileNames);
        if (!_capturesDocumentationText)
        {
            HintTextBlock.Text = "Erkannte Dateien werden als XDT-Anhänge übergeben. Es wird kein Dokumentationstext erfasst.";
            DocumentationTextBox.Visibility = Visibility.Collapsed;
        }

        if (_requiresTransferConfirmation)
        {
            Title = "Dokumente übertragen";
            ApplyButton.Content = "Übertragen";
            ContinueWithoutTextButton.Content = "Abbrechen";
        }
    }

    public string? DocumentationText { get; private set; }

    public string CurrentDocumentationText => _capturesDocumentationText ? DocumentationTextBox.Text.Trim() : string.Empty;

    public event EventHandler? TransferRequested;

    public event EventHandler? CancelRequested;

    public void UpdateFileNames(IReadOnlyList<string> fileNames)
    {
        _fileNames.Clear();
        if (fileNames.Count == 0)
        {
            _fileNames.Add("Keine Datei ausgewählt.");
            return;
        }

        foreach (var fileName in fileNames)
        {
            _fileNames.Add(fileName);
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        DocumentationText = CurrentDocumentationText;
        if (_requiresTransferConfirmation)
        {
            TransferRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        DialogResult = true;
    }

    private void ContinueWithoutText_Click(object sender, RoutedEventArgs e)
    {
        DocumentationText = null;
        if (_requiresTransferConfirmation)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        DialogResult = true;
    }
}
