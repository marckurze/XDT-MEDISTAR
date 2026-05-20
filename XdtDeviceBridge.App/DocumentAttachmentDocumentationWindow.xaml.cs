using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class DocumentAttachmentDocumentationWindow : Window
{
    private readonly bool _requiresTransferConfirmation;
    private readonly bool _capturesFileDescriptions;
    private readonly ObservableCollection<DocumentAttachmentFileItem> _files = new();
    private readonly HashSet<string> _knownFingerprints = new(StringComparer.OrdinalIgnoreCase);

    public DocumentAttachmentDocumentationWindow(
        string profileName,
        IReadOnlyList<DocumentAttachmentDialogFile> files,
        bool requiresTransferConfirmation = false,
        bool capturesDocumentationText = true)
    {
        InitializeComponent();
        _requiresTransferConfirmation = requiresTransferConfirmation;
        _capturesFileDescriptions = capturesDocumentationText;
        ProfileNameTextBlock.Text = string.IsNullOrWhiteSpace(profileName)
            ? "Dokumentgerät"
            : profileName;
        FileItemsControl.ItemsSource = _files;
        UpdateFiles(files);

        if (!_capturesFileDescriptions)
        {
            HintTextBlock.Text = "Prüfen Sie die gefundenen Dateien. Als Beschreibung wird jeweils der Originaldateiname verwendet.";
        }
    }

    public DocumentAttachmentDocumentationWindow(
        string profileName,
        IReadOnlyList<string> fileNames,
        bool requiresTransferConfirmation = false,
        bool capturesDocumentationText = true)
        : this(
            profileName,
            fileNames.Select(DocumentAttachmentDialogFile.FromFileName).ToList(),
            requiresTransferConfirmation,
            capturesDocumentationText)
    {
    }

    public string? DocumentationText { get; private set; }

    public string CurrentDocumentationText => string.Empty;

    public event EventHandler? TransferRequested;

    public event EventHandler? CancelRequested;

    public IReadOnlyDictionary<string, string> FileDescriptions => GetFileDescriptions();

    public void UpdateFileNames(IReadOnlyList<string> fileNames)
    {
        UpdateFiles(fileNames.Select(DocumentAttachmentDialogFile.FromFileName).ToList());
    }

    public bool UpdateFiles(IReadOnlyList<DocumentAttachmentDialogFile> files)
    {
        var newFiles = new List<DocumentAttachmentDialogFile>();
        foreach (var file in files)
        {
            if (!_knownFingerprints.Add(file.Fingerprint))
            {
                continue;
            }

            newFiles.Add(file);
        }

        if (newFiles.Count == 0)
        {
            return false;
        }

        foreach (var file in newFiles
            .OrderBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FullPath, StringComparer.OrdinalIgnoreCase))
        {
            _files.Add(new DocumentAttachmentFileItem(file, _capturesFileDescriptions));
        }

        return true;
    }

    public IReadOnlyDictionary<string, string> GetFileDescriptions()
    {
        FlushDescriptionBindings();
        var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in _files)
        {
            var value = item.DescriptionText ?? string.Empty;
            descriptions[item.Fingerprint] = value;
            if (!string.IsNullOrWhiteSpace(item.FullPath))
            {
                descriptions[item.FullPath] = value;
            }

            if (!string.IsNullOrWhiteSpace(item.FileName))
            {
                descriptions[item.FileName] = value;
            }
        }

        return descriptions;
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        FlushDescriptionBindings();
        DocumentationText = null;
        TransferRequested?.Invoke(this, EventArgs.Empty);
        if (_requiresTransferConfirmation)
        {
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DocumentationText = null;
        CancelRequested?.Invoke(this, EventArgs.Empty);
        if (_requiresTransferConfirmation)
        {
            return;
        }

        DialogResult = false;
    }

    private void FlushDescriptionBindings()
    {
        foreach (var textBox in FindVisualChildren<System.Windows.Controls.TextBox>(this))
        {
            BindingOperations.GetBindingExpression(textBox, System.Windows.Controls.TextBox.TextProperty)?.UpdateSource();
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }
}

public sealed record DocumentAttachmentDialogFile(
    string Fingerprint,
    string FileName,
    string FullPath,
    string FileFormat,
    long SizeBytes,
    DateTime LastWriteTimeUtc)
{
    public static DocumentAttachmentDialogFile FromCandidate(AttachmentImportFileCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return new DocumentAttachmentDialogFile(
            AttachmentImportFileFingerprint.Create(candidate),
            candidate.FileName,
            candidate.FullPath,
            NormalizeFileFormat(candidate.Extension),
            candidate.SizeBytes,
            candidate.LastWriteTimeUtc);
    }

    public static DocumentAttachmentDialogFile FromPendingImportFile(PendingImportFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var sizeBytes = 0L;
        try
        {
            var fileInfo = new FileInfo(file.FilePath);
            if (fileInfo.Exists)
            {
                sizeBytes = fileInfo.Length;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UnauthorizedAccessException or IOException)
        {
            sizeBytes = 0L;
        }

        return new DocumentAttachmentDialogFile(
            AttachmentImportFileFingerprint.Create(file.FilePath, sizeBytes, file.DetectedAtUtc),
            file.FileName,
            file.FilePath,
            NormalizeFileFormat(Path.GetExtension(file.FileName)),
            sizeBytes,
            file.DetectedAtUtc);
    }

    public static DocumentAttachmentDialogFile FromFileName(string fileName)
    {
        var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "Datei" : fileName.Trim();
        var extension = Path.GetExtension(safeFileName);

        return new DocumentAttachmentDialogFile(
            AttachmentImportFileFingerprint.Create(safeFileName, 0, DateTime.MinValue),
            Path.GetFileName(safeFileName),
            safeFileName,
            NormalizeFileFormat(extension),
            0,
            DateTime.MinValue);
    }

    private static string NormalizeFileFormat(string? extension)
    {
        var value = string.IsNullOrWhiteSpace(extension)
            ? "DATEI"
            : extension.Trim().TrimStart('.').ToUpperInvariant();
        return value == "JPEG" ? "JPG" : value;
    }
}

public sealed class DocumentAttachmentFileItem : INotifyPropertyChanged
{
    private string _descriptionText = string.Empty;

    public DocumentAttachmentFileItem(DocumentAttachmentDialogFile file, bool capturesFileDescriptions)
    {
        Fingerprint = file.Fingerprint;
        FileName = file.FileName;
        FullPath = file.FullPath;
        FileFormatLabel = string.IsNullOrWhiteSpace(file.FileFormat) ? "DATEI" : file.FileFormat;
        DetailText = CreateDetailText(file);
        DescriptionVisibility = capturesFileDescriptions ? Visibility.Visible : Visibility.Collapsed;
    }

    public string Fingerprint { get; }

    public string FileName { get; }

    public string FullPath { get; }

    public string FileFormatLabel { get; }

    public string DetailText { get; }

    public Visibility DescriptionVisibility { get; }

    public string DescriptionText
    {
        get => _descriptionText;
        set
        {
            if (string.Equals(_descriptionText, value, StringComparison.Ordinal))
            {
                return;
            }

            _descriptionText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DescriptionText)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static string CreateDetailText(DocumentAttachmentDialogFile file)
    {
        var sizeText = file.SizeBytes > 0
            ? FormatSize(file.SizeBytes)
            : "Größe unbekannt";
        return $"{file.FileFormat} · {sizeText}";
    }

    private static string FormatSize(long sizeBytes)
    {
        if (sizeBytes < 1024)
        {
            return sizeBytes.ToString(CultureInfo.InvariantCulture) + " B";
        }

        var kilobytes = sizeBytes / 1024d;
        if (kilobytes < 1024)
        {
            return kilobytes.ToString("0.#", CultureInfo.InvariantCulture) + " KB";
        }

        var megabytes = kilobytes / 1024d;
        return megabytes.ToString("0.#", CultureInfo.InvariantCulture) + " MB";
    }
}
