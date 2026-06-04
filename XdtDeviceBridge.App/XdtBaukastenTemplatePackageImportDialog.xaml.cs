using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.App;

public partial class XdtBaukastenTemplatePackageImportDialog : Window
{
    private readonly ProfileCatalog _existingCatalog;
    private readonly AppDataPaths _paths;
    private readonly TemplatePackageImportSelectionService _selectionService = new();
    private readonly TemplatePackageImportDryRunService _dryRunService = new();
    private readonly TemplatePackageImportPreviewDisplayService _displayService = new();
    private readonly TemplatePackageImportExecutor _executor = new();

    private TemplatePackageImportResult _importResult;
    private TemplatePackageImportValidationResult _validationResult;
    private TemplatePackageImportAnalysisResult _analysisResult;
    private TemplatePackageImportPlan _basePlan;
    private TemplatePackageImportPlan _plan;
    private TemplatePackageImportDryRunResult _dryRunResult;
    private bool _updatingPreview;

    public XdtBaukastenTemplatePackageImportDialog(
        TemplatePackageImportPreviewResult previewResult,
        ProfileCatalog existingCatalog,
        AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(previewResult);
        _existingCatalog = existingCatalog ?? throw new ArgumentNullException(nameof(existingCatalog));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _importResult = previewResult.ImportResult;
        _validationResult = previewResult.ValidationResult;
        _analysisResult = previewResult.AnalysisResult;
        _basePlan = previewResult.BasePlan;
        _plan = previewResult.Plan;
        _dryRunResult = previewResult.DryRunResult;

        InitializeComponent();
        ShowPreview(previewResult.Display);
        UpdateImportButton();
    }

    public bool ImportSucceeded { get; private set; }

    public string? ImportedAisProfileId { get; private set; }

    public string? ImportedDeviceProfileId { get; private set; }

    public string? ImportedExportProfileId { get; private set; }

    private void ActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingPreview || !IsLoaded)
        {
            return;
        }

        Dispatcher.BeginInvoke((Action)(() => UpdatePreviewFromUserInput("Importvorschau wurde anhand der Benutzerentscheidung aktualisiert.")), DispatcherPriority.Background);
    }

    private void PreviewGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (_updatingPreview)
        {
            return;
        }

        Dispatcher.BeginInvoke((Action)(() => UpdatePreviewFromUserInput("Importvorschau wurde anhand der Zielnamen aktualisiert.")), DispatcherPriority.Background);
    }

    private void RefreshPreview_Click(object sender, RoutedEventArgs e)
    {
        UpdatePreviewFromUserInput("Importvorschau wurde aktualisiert.");
    }

    private void UpdatePreviewFromUserInput(string statusText)
    {
        if (_updatingPreview)
        {
            return;
        }

        try
        {
            var selections = GetUserSelections();
            _plan = _selectionService.Apply(_basePlan, selections);
            _dryRunResult = _dryRunService.Preview(_importResult, _plan, _existingCatalog);
            ShowPreview(_displayService.Create(_validationResult, _analysisResult, _plan, _dryRunResult));
            UpdateImportButton();
            ExecutionResultTextBox.Text = $"{statusText} Noch keine Importübernahme ausgeführt.";
        }
        catch (Exception ex)
        {
            ExecutionResultTextBox.Text = $"Importvorschau konnte nicht aktualisiert werden: {ex.Message}";
        }
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        if (!CanExecuteImport(_dryRunResult))
        {
            ExecutionResultTextBox.Text = "Import kann nicht übernommen werden, weil keine schreibbaren Profile geplant sind oder blockierende Konflikte bestehen.";
            return;
        }

        try
        {
            var result = _executor.Execute(_importResult, _plan, _dryRunResult, _paths);
            ExecutionResultTextBox.Text = FormatExecutionResult(result);
            ImportSucceeded = result.Success && result.ImportedProfiles.Count > 0;

            ImportedAisProfileId = FindImportedProfileId(result, ProfileKind.AisProfile);
            ImportedDeviceProfileId = FindImportedProfileId(result, ProfileKind.DeviceProfile);
            ImportedExportProfileId = FindImportedProfileId(result, ProfileKind.ExportProfile);

            if (ImportSucceeded)
            {
                ImportButton.IsEnabled = false;
                DialogResult = true;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            ExecutionResultTextBox.Text = $"Importübernahme konnte nicht ausgeführt werden: {ex.Message}";
        }
    }

    private void ShowPreview(TemplatePackageImportPreviewDisplay display)
    {
        _updatingPreview = true;
        try
        {
            SummaryTextBlock.Text = display.Summary.SummaryText;
            MessagesTextBox.Text = FormatPreviewMessages(display);
            PreviewGrid.ItemsSource = null;
            DependencyGrid.ItemsSource = null;
            PreviewGrid.ItemsSource = display.Rows;
            DependencyGrid.ItemsSource = display.DependencyRows;
            DependencyGrid.Visibility = display.DependencyRows.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            DependencyEmptyTextBlock.Text = display.DependencyEmptyStateMessage;
            DependencyEmptyTextBlock.Visibility = display.DependencyRows.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        finally
        {
            _updatingPreview = false;
        }
    }

    private IReadOnlyList<TemplatePackageImportUserSelection> GetUserSelections()
    {
        return (PreviewGrid.ItemsSource as IEnumerable<TemplatePackageImportPreviewRow>
                ?? Array.Empty<TemplatePackageImportPreviewRow>())
            .Where(row => row.IsActionSelectionEnabled)
            .Select(row => new TemplatePackageImportUserSelection(
                ProfileKind: row.ProfileKindValue,
                ImportedProfileId: row.ImportedProfileId,
                SelectedAction: row.SelectedAction,
                TargetProfileId: null,
                TargetProfileName: row.SelectedAction == TemplatePackageImportAction.ImportAsCopy && row.IsTargetNameEditable
                    ? row.TargetProfileName
                    : null,
                IsValid: row.SelectedAction != TemplatePackageImportAction.ImportAsCopy
                    || !row.IsTargetNameEditable
                    || !string.IsNullOrWhiteSpace(row.TargetProfileName),
                ValidationMessage: row.SelectedAction == TemplatePackageImportAction.ImportAsCopy
                    && row.IsTargetNameEditable
                    && string.IsNullOrWhiteSpace(row.TargetProfileName)
                        ? "Zielname darf nicht leer sein."
                        : null))
            .ToList();
    }

    private void UpdateImportButton()
    {
        ImportButton.IsEnabled = CanExecuteImport(_dryRunResult);
    }

    private static bool CanExecuteImport(TemplatePackageImportDryRunResult dryRunResult)
    {
        return dryRunResult.Items.Any(item =>
            item.WouldWrite
            && !item.IsBlocking
            && item.PlannedAction is TemplatePackageImportAction.ImportAsNew or TemplatePackageImportAction.ImportAsCopy);
    }

    private static string? FindImportedProfileId(TemplatePackageImportExecutionResult result, ProfileKind kind)
    {
        return result.ImportedProfiles
            .FirstOrDefault(item => item.ProfileKind == kind)
            ?.TargetProfileId;
    }

    private static string FormatPreviewMessages(TemplatePackageImportPreviewDisplay display)
    {
        var builder = new StringBuilder();
        foreach (var message in display.Messages)
        {
            builder.AppendLine(message);
        }

        if (display.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Hinweise/Warnungen:");
            foreach (var warning in display.Warnings.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"- {warning}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatExecutionResult(TemplatePackageImportExecutionResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Import abgeschlossen: {result.ImportedProfiles.Count} Profil(e) als UserDefined importiert.");
        builder.AppendLine($"ImportAsNew: {result.ImportedAsNew}");
        builder.AppendLine($"ImportAsCopy: {result.ImportedAsCopy}");
        builder.AppendLine($"Übersprungen: {result.Skipped}");
        builder.AppendLine($"Blockiert: {result.Blocked}");
        builder.AppendLine($"Fehler: {result.Failed}");

        if (result.ImportedProfiles.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Importierte Profile:");
            foreach (var item in result.ImportedProfiles)
            {
                builder.AppendLine($"- {item.ProfileKind}: {item.TargetProfileName} ({item.TargetProfileId})");
            }
        }

        if (result.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Hinweise:");
            foreach (var warning in result.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            builder.AppendLine();
            builder.AppendLine(result.ErrorMessage);
        }

        return builder.ToString().TrimEnd();
    }
}
