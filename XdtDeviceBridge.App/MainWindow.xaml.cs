using Microsoft.Win32;
using System.Text;
using System.Windows;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;
using WinForms = System.Windows.Forms;

namespace XdtDeviceBridge.App;

public partial class MainWindow : Window
{
    private readonly ProcessingPipelineService _pipelineService = new();
    private readonly ExportFileNameBuilder _fileNameBuilder = new();
    private readonly FileExportService _fileExportService = new();
    private readonly AppDataPathProvider _appDataPathProvider = new();
    private readonly ProfileCatalogService _profileCatalogService = new();
    private readonly TemplatePackageExporter _templatePackageExporter = new();
    private readonly TemplatePackageImporter _templatePackageImporter = new();
    private readonly TemplatePackageImportValidator _templatePackageImportValidator = new();

    private ProcessingPipelineResult? _lastPipelineResult;
    private DeviceProfile _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
    private ProfileCatalog? _profileCatalog;
    private string? _plannedFileName;

    public MainWindow()
    {
        InitializeComponent();
        InitializeProfileOverview();
    }

    private void InitializeProfileOverview()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.EnsureDefaultProfiles(paths);
            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;

            AisProfileCountText.Text = catalog.AisProfiles.Count.ToString();
            DeviceProfileCountText.Text = catalog.DeviceProfiles.Count.ToString();
            ExportProfileCountText.Text = catalog.ExportProfiles.Count.ToString();
            InterfaceProfileCountText.Text = catalog.InterfaceProfiles.Count.ToString();
            ProfileBaseFolderText.Text = paths.BaseFolder;
        }
        catch (Exception ex)
        {
            AisProfileCountText.Text = "-";
            DeviceProfileCountText.Text = "-";
            ExportProfileCountText.Text = "-";
            InterfaceProfileCountText.Text = "-";
            _profileCatalog = null;
            ProfileBaseFolderText.Text = string.Empty;
            AppendMessage($"V2-Profile konnten nicht geladen werden: {ex.Message}");
        }
    }

    private void ExportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            AppendMessage("Templatepaket kann nicht exportiert werden, weil keine V2-Profile geladen sind.");
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Templatepaket (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
            FileName = "XdtDeviceBridge_MEDISTAR_NIDEK_ARK1S_Template.zip",
            DefaultExt = ".zip",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var request = CreateTemplatePackageExportRequest(_profileCatalog, DateTime.UtcNow);
            _templatePackageExporter.Export(dialog.FileName, request);
            AppendMessage($"Templatepaket erfolgreich exportiert: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AppendMessage($"Templatepaket konnte nicht exportiert werden: {ex.Message}");
        }
    }

    private static TemplatePackageExportRequest CreateTemplatePackageExportRequest(ProfileCatalog catalog, DateTime createdAtUtc)
    {
        var includedProfiles = catalog.AisProfiles.Select(profile => profile.Metadata)
            .Concat(catalog.DeviceProfiles.Select(profile => profile.Metadata))
            .Concat(catalog.ExportProfiles.Select(profile => profile.Metadata))
            .Concat(catalog.InterfaceProfiles.Select(profile => profile.Metadata))
            .ToArray();

        var package = new TemplatePackage(
            Metadata: new ProfileMetadata(
                Id: "package-medistar-nidek-ark1s",
                Name: "MEDISTAR + NIDEK ARK1S",
                ProfileKind: ProfileKind.TemplatePackage,
                Description: "Validated MEDISTAR/NIDEK ARK1S template package",
                Vendor: "XdtDeviceBridge",
                Product: "MEDISTAR/NIDEK ARK1S",
                Version: "1.0.0",
                CreatedAt: new DateTimeOffset(createdAtUtc),
                UpdatedAt: new DateTimeOffset(createdAtUtc),
                CreatedBy: Environment.UserName,
                IsBuiltIn: false,
                IsUserDefined: true),
            IncludedProfiles: includedProfiles,
            PackageFormatVersion: "1.0",
            CreatedAt: createdAtUtc,
            CreatedBy: Environment.UserName,
            Description: "Validated MEDISTAR/NIDEK ARK1S template package");

        return new TemplatePackageExportRequest(
            Package: package,
            AisProfiles: catalog.AisProfiles,
            DeviceProfiles: catalog.DeviceProfiles,
            ExportProfiles: catalog.ExportProfiles,
            InterfaceProfiles: catalog.InterfaceProfiles);
    }

    private void ImportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Templatepaket (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var importResult = _templatePackageImporter.Import(dialog.FileName);
            var validationResult = _templatePackageImportValidator.Validate(importResult);
            ShowTemplatePackageImportResult(importResult, validationResult);
        }
        catch (Exception ex)
        {
            AppendMessage($"Templatepaket konnte nicht importiert oder geprüft werden: {ex.Message}");
        }
    }

    private void ShowTemplatePackageImportResult(
        TemplatePackageImportResult importResult,
        TemplatePackageImportValidationResult validationResult)
    {
        var warningIssues = validationResult.Issues
            .Where(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Warning)
            .ToList();
        var errorIssues = validationResult.Issues
            .Where(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error)
            .ToList();

        AppendMessage($"Templatepaket importiert: {importResult.Package.Metadata.Name}");
        AppendMessage($"AIS-Profile: {importResult.AisProfiles.Count}");
        AppendMessage($"Geräteprofile: {importResult.DeviceProfiles.Count}");
        AppendMessage($"Exportprofile: {importResult.ExportProfiles.Count}");
        AppendMessage($"Schnittstellenprofile: {importResult.InterfaceProfiles.Count}");
        AppendMessage($"Warnings: {warningIssues.Count}");
        AppendMessage($"Errors: {errorIssues.Count}");

        if (errorIssues.Count > 0)
        {
            AppendMessage("Templatepaket enthält Fehler und wurde nicht übernommen.");
            foreach (var issue in errorIssues)
            {
                AppendMessage($"[Templatepaket] Error: {FormatTemplatePackageImportIssue(issue)}");
            }

            return;
        }

        if (warningIssues.Count > 0)
        {
            AppendMessage("Templatepaket ist grundsätzlich gültig, enthält aber Hinweise.");
            foreach (var issue in warningIssues)
            {
                AppendMessage($"[Templatepaket] Warning: {FormatTemplatePackageImportIssue(issue)}");
            }
        }

        AppendMessage("Templatepaket erfolgreich geprüft. Produktive Übernahme ist noch nicht implementiert.");
    }

    private static string FormatTemplatePackageImportIssue(TemplatePackageImportValidationIssue issue)
    {
        var profileKind = issue.ProfileKind?.ToString() ?? "Unbekannt";
        var profileId = string.IsNullOrWhiteSpace(issue.ProfileId) ? "ohne Profil-ID" : issue.ProfileId;

        return $"{issue.Message} ({profileKind}, {profileId})";
    }

    private void SelectAisFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "GDT/XDT (*.gdt;*.xdt)|*.gdt;*.xdt|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            AisFilePathTextBox.Text = dialog.FileName;
            AppendMessage($"AIS-Datei ausgewählt: {dialog.FileName}");
        }
    }

    private void SelectDeviceFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XML (*.xml)|*.xml|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            DeviceFilePathTextBox.Text = dialog.FileName;
            AppendMessage($"Geräte-Datei ausgewählt: {dialog.FileName}");
        }
    }

    private void Process_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AisFilePathTextBox.Text) || string.IsNullOrWhiteSpace(DeviceFilePathTextBox.Text))
        {
            AppendMessage("Bitte zuerst AIS- und Geräte-Datei auswählen.");
            return;
        }

        _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
        _lastPipelineResult = _pipelineService.ProcessFiles(AisFilePathTextBox.Text, DeviceFilePathTextBox.Text, _currentProfile);

        ShowPatient(_lastPipelineResult.Patient);
        MeasurementsGrid.ItemsSource = _lastPipelineResult.Measurements;
        ExportPreviewTextBox.Text = _lastPipelineResult.ExportContent;

        _plannedFileName = _fileNameBuilder.Build(_currentProfile, _lastPipelineResult.Patient, DateTime.Now);
        PlannedFileNameText.Text = $"Geplanter Dateiname: {_plannedFileName}";

        ShowIssues(_lastPipelineResult.Issues);

        if (_lastPipelineResult.HasErrors)
        {
            AppendMessage("Verarbeitung abgeschlossen mit Fehlern.");
        }
        else
        {
            AppendMessage("Verarbeitung erfolgreich abgeschlossen.");
        }
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_lastPipelineResult is null || string.IsNullOrWhiteSpace(_lastPipelineResult.ExportContent))
        {
            AppendMessage("Keine Exportvorschau vorhanden. Bitte zuerst verarbeiten.");
            return;
        }

        using var dialog = new WinForms.FolderBrowserDialog();
        if (dialog.ShowDialog() != WinForms.DialogResult.OK)
        {
            return;
        }

        var fileName = _plannedFileName ?? _fileNameBuilder.Build(_currentProfile, _lastPipelineResult.Patient, DateTime.Now);
        var exportResult = _fileExportService.Export(dialog.SelectedPath, fileName, _lastPipelineResult.ExportContent, _currentProfile.OutputEncoding);

        if (exportResult.HasErrors)
        {
            foreach (var issue in exportResult.Issues)
            {
                AppendMessage($"[Export] {issue.Severity}: {issue.Message}");
            }
        }
        else
        {
            AppendMessage($"Export erfolgreich geschrieben: {exportResult.FilePath}");
        }
    }

    private void ShowPatient(PatientData? patient)
    {
        PatientNumberText.Text = patient?.PatientNumber ?? string.Empty;
        LastNameText.Text = patient?.LastName ?? string.Empty;
        FirstNameText.Text = patient?.FirstName ?? string.Empty;
        BirthDateText.Text = patient?.BirthDate ?? string.Empty;
        StreetText.Text = patient?.Street ?? string.Empty;
        PostalCodeCityText.Text = patient?.PostalCodeCity ?? string.Empty;
    }

    private void ShowIssues(IEnumerable<ProcessingIssue> issues)
    {
        var visibleIssues = issues
            .Where(issue =>
                issue.Severity != ProcessingIssueSeverity.Warning ||
                !issue.Message.Contains("Declared length does not match actual line length", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (visibleIssues.Count == 0)
        {
            MessagesTextBox.Text = "Keine Fehler. Verarbeitung erfolgreich.";
            return;
        }

        var builder = new StringBuilder();

        foreach (var issue in visibleIssues)
        {
            builder.AppendLine($"[{issue.Stage}] {issue.Severity}: {issue.Message}");
        }

        MessagesTextBox.Text = builder.ToString();
    }

    private void AppendMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(MessagesTextBox.Text))
        {
            MessagesTextBox.AppendText(Environment.NewLine);
        }

        MessagesTextBox.AppendText(message);
    }
}
