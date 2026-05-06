using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
    private readonly InstallationInfoProvider _installationInfoProvider = new();
    private readonly LicenseFileRepository _licenseFileRepository = new();
    private readonly LicensedDeviceGracePeriodRepository _licensedDeviceGracePeriodRepository = new();
    private readonly LicenseEvaluator _licenseEvaluator = new();
    private readonly LicensedDeviceStateEvaluator _licensedDeviceStateEvaluator = new();
    private readonly LicensedDeviceGracePeriodService _licensedDeviceGracePeriodService = new();
    private readonly ActiveInterfaceProfileStatusService _activeInterfaceProfileStatusService = new();
    private readonly AutoImportScannerService _autoImportScannerService = new();
    private readonly InterfaceProfileManualProcessor _interfaceProfileManualProcessor = new();
    private readonly LicenseRequestBuilder _licenseRequestBuilder = new();
    private readonly LicenseRequestFileRepository _licenseRequestFileRepository = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();
    private readonly ExportProfileDraftService _exportProfileDraftService = new();
    private readonly InterfaceProfileConfigurationService _interfaceProfileConfigurationService = new();
    private readonly ObservableCollection<PlaceholderRow> _aisPlaceholderRows = new();
    private readonly ObservableCollection<PlaceholderRow> _devicePlaceholderRows = new();
    private readonly ObservableCollection<ExportRuleDefinition> _visibleExportRules = new();
    private readonly ObservableCollection<LicenseDeviceStateRow> _licensedDeviceStateRows = new();
    private readonly ObservableCollection<ActiveInterfaceProfileStatusRow> _activeInterfaceProfileStatusRows = new();
    private readonly ObservableCollection<ScannedImportPairRow> _scannedImportPairRows = new();
    private readonly List<ExportRuleDefinition> _temporaryExportRules = new();

    private ProcessingPipelineResult? _lastPipelineResult;
    private DeviceProfile _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
    private ProfileCatalog? _profileCatalog;
    private InstallationInfo? _installationInfo;
    private string? _plannedFileName;
    private bool _updatingPlaceholderRows;
    private int _draftRuleSequence;

    public MainWindow()
    {
        InitializeComponent();
        DraftRuleTypeComboBox.ItemsSource = Enum.GetValues<ExportRuleType>();
        AisPlaceholdersGrid.ItemsSource = _aisPlaceholderRows;
        DevicePlaceholdersGrid.ItemsSource = _devicePlaceholderRows;
        ExportRulesGrid.ItemsSource = _visibleExportRules;
        LicensedDeviceStatesGrid.ItemsSource = _licensedDeviceStateRows;
        ActiveInterfaceProfilesGrid.ItemsSource = _activeInterfaceProfileStatusRows;
        ScannedImportPairsGrid.ItemsSource = _scannedImportPairRows;
        InitializeProfileOverview();
        InitializeLicenseOverview();
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
            ShowProfileNameColumns(catalog);
            InitializeExportRulesView(catalog);
            InitializeInterfaceProfileConfiguration(catalog);
            UpdatePlaceholderTables();
            ProfileMessagesTextBox.Text = $"Profile geladen. AIS: {catalog.AisProfiles.Count}, Geräte: {catalog.DeviceProfiles.Count}, Export: {catalog.ExportProfiles.Count}, Schnittstellen: {catalog.InterfaceProfiles.Count}.";
        }
        catch (Exception ex)
        {
            AisProfileCountText.Text = "-";
            DeviceProfileCountText.Text = "-";
            ExportProfileCountText.Text = "-";
            InterfaceProfileCountText.Text = "-";
            _profileCatalog = null;
            ProfileBaseFolderText.Text = string.Empty;
            ClearProfileNameColumns();
            ExportProfileComboBox.ItemsSource = null;
            InterfaceProfileComboBox.ItemsSource = null;
            _visibleExportRules.Clear();
            _temporaryExportRules.Clear();
            ExportRulesStatusText.Text = "Keine Exportprofile geladen.";
            ExportRulePreviewTextBox.Text = "Keine Exportregel ausgewählt.";
            FullExportPreviewTextBox.Text = "Kein Exportprofil ausgewählt.";
            ClearDraftRuleEditor();
            ClearPlaceholderTables();
            ClearInterfaceProfileEditor();
            ClearActiveInterfaceProfilesOverview("Aktive Schnittstellenprofile konnten nicht geladen werden.");
            AppendProfileMessage($"V2-Profile konnten nicht geladen werden: {ex.Message}");
        }
    }

    private void InitializeExportRulesView(ProfileCatalog catalog, string? selectedExportProfileId = null)
    {
        if (catalog.ExportProfiles.Count == 0)
        {
            ExportProfileComboBox.ItemsSource = null;
            _visibleExportRules.Clear();
            _temporaryExportRules.Clear();
            ExportRulesStatusText.Text = "Keine Exportprofile geladen.";
            ExportRulePreviewTextBox.Text = "Keine Exportregel ausgewählt.";
            FullExportPreviewTextBox.Text = "Kein Exportprofil ausgewählt.";
            ClearDraftRuleEditor();
            AppendProfileMessage("Keine Exportprofile geladen. Exportregeln können nicht angezeigt werden.");
            return;
        }

        var exportProfiles = catalog.ExportProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        ExportProfileComboBox.ItemsSource = exportProfiles;
        var selectedProfile = string.IsNullOrWhiteSpace(selectedExportProfileId)
            ? null
            : exportProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, selectedExportProfileId, StringComparison.Ordinal));
        ExportProfileComboBox.SelectedItem = selectedProfile ?? exportProfiles[0];
        ShowExportRulesForSelectedProfile();
    }

    private void ExportProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ShowExportRulesForSelectedProfile();
    }

    private void ShowExportRulesForSelectedProfile()
    {
        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            _visibleExportRules.Clear();
            _temporaryExportRules.Clear();
            ExportRulesStatusText.Text = "Keine Exportprofile geladen.";
            FullExportPreviewTextBox.Text = "Kein Exportprofil ausgewählt.";
            NewExportProfileNameTextBox.Text = string.Empty;
            ClearDraftRuleEditor();
            return;
        }

        NewExportProfileNameTextBox.Text = $"{exportProfile.Metadata.Name} - Kopie";
        _temporaryExportRules.Clear();
        RebuildExportRulesGrid(exportProfile);
        ExportRulesStatusText.Text = $"{exportProfile.Metadata.Name}: {_visibleExportRules.Count} Exportregeln";
        ExportRulesGrid.SelectedIndex = _visibleExportRules.Count > 0 ? 0 : -1;
        ShowExportRulePreviewForSelectedRule();
        ShowFullExportPreviewForSelectedProfile();
    }

    private void RebuildExportRulesGrid(ExportProfileDefinition exportProfile, string? selectedRuleId = null)
    {
        _visibleExportRules.Clear();
        foreach (var rule in exportProfile.Rules.Concat(_temporaryExportRules).OrderBy(rule => rule.SortOrder))
        {
            _visibleExportRules.Add(rule);
        }

        ExportRulesStatusText.Text = $"{exportProfile.Metadata.Name}: {_visibleExportRules.Count} Exportregeln";
        if (!string.IsNullOrWhiteSpace(selectedRuleId))
        {
            var selectedRule = _visibleExportRules.FirstOrDefault(rule => rule.Id == selectedRuleId);
            if (selectedRule is not null)
            {
                ExportRulesGrid.SelectedItem = selectedRule;
                return;
            }
        }

        ExportRulesGrid.SelectedIndex = _visibleExportRules.Count > 0 ? 0 : -1;
    }

    private void ExportRulesGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        LoadDraftFromSelectedRule();
        ShowExportRulePreviewForSelectedRule();
        ShowFullExportPreviewForSelectedProfile();
    }

    private void AddDraftExportRule_Click(object sender, RoutedEventArgs e)
    {
        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            AppendProfileMessage("Neue Exportregel kann nicht angelegt werden, weil kein Exportprofil ausgewählt ist.");
            return;
        }

        var nextSortOrder = _visibleExportRules.Count == 0
            ? 1
            : _visibleExportRules.Max(rule => rule.SortOrder) + 1;
        var draftRule = new ExportRuleDefinition(
            Id: $"draft-rule-{++_draftRuleSequence}",
            TargetFieldCode: "6228",
            TargetName: "Neue Regel (Entwurf)",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: string.Empty,
            SortOrder: nextSortOrder,
            IsEnabled: true,
            Description: "Neue Entwurfsregel");

        _temporaryExportRules.Add(draftRule);
        RebuildExportRulesGrid(exportProfile, draftRule.Id);
        LoadDraftFromSelectedRule();
        UpdateDraftPreviewFromCurrentDraft();
        AppendProfileMessage("Neue Entwurfsregel aktiv. Sie erscheint nur in der Vorschau.");
    }

    private void ShowExportRulePreviewForSelectedRule()
    {
        if (ExportRulesGrid.SelectedItem is not ExportRuleDefinition rule)
        {
            ExportRulePreviewTextBox.Text = "Keine Exportregel ausgewählt.";
            return;
        }

        ExportRulePreviewTextBox.Text = FormatExportRulePreview(rule);
    }

    private string FormatExportRulePreview(ExportRuleDefinition rule)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"TargetFieldCode: {rule.TargetFieldCode}");
        builder.AppendLine($"TargetName: {rule.TargetName}");
        builder.AppendLine("OutputTemplate:");
        builder.AppendLine(rule.OutputTemplate);
        builder.AppendLine();

        var roundBracketPlaceholders = GetRoundBracketPlaceholderCandidates(rule.OutputTemplate);
        AppendRoundBracketPlaceholderHint(builder, roundBracketPlaceholders);

        if (_lastPipelineResult is null)
        {
            builder.AppendLine("Vorschau:");
            builder.AppendLine("Noch keine Beispielwerte geladen.");
            return builder.ToString().TrimEnd();
        }

        var result = _lastPipelineResult;
        var patient = result.Patient ?? CreateEmptyPatientData();
        var previewRule = CreatePreviewMappingRule(rule, result);

        var mappingResult = _mappingEngine.Map(
            patient,
            result.Measurements,
            new[] { previewRule });

        builder.AppendLine("Gerenderte Vorschau:");
        var renderedValue = mappingResult.Records.FirstOrDefault()?.Value;
        builder.AppendLine(renderedValue ?? string.Empty);

        var unresolvedPlaceholders = GetUnresolvedPlaceholders(rule.OutputTemplate, previewRule.SourcePath, patient, result.Measurements);
        if (mappingResult.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Mapping-Hinweise:");
            foreach (var issue in mappingResult.Issues)
            {
                builder.AppendLine($"- {issue.Severity}: {issue.Message} SourcePath={issue.SourcePath}, TargetFieldCode={issue.TargetFieldCode}");
            }
        }

        if (mappingResult.HasErrors || unresolvedPlaceholders.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Ein oder mehrere Platzhalter konnten nicht aufgelöst werden.");
            foreach (var placeholder in unresolvedPlaceholders)
            {
                builder.AppendLine($"- Platzhalter konnte nicht aufgelöst werden: {{{placeholder}}}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private void ShowFullExportPreviewForSelectedProfile()
    {
        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            FullExportPreviewTextBox.Text = "Kein Exportprofil ausgewählt.";
            return;
        }

        FullExportPreviewTextBox.Text = FormatFullExportPreview(exportProfile);
    }

    private string FormatFullExportPreview(
        ExportProfileDefinition exportProfile,
        ExportRuleDefinition? draftRule = null,
        string? replaceRuleId = null)
    {
        if (_lastPipelineResult is null)
        {
            return "Noch keine Beispielwerte geladen. Bitte zuerst im Tab Verarbeitung eine AIS-GDT-Datei und eine Gerätedatei verarbeiten.";
        }

        var result = _lastPipelineResult;
        var patient = result.Patient ?? CreateEmptyPatientData();
        var effectiveRules = GetEffectiveExportRules(exportProfile, draftRule, replaceRuleId);
        var mappingRules = effectiveRules
            .Select(rule => CreatePreviewMappingRule(rule, result))
            .ToList();
        var mappingResult = _mappingEngine.Map(patient, result.Measurements, mappingRules);
        var exportResult = _xdtExportBuilder.Build(mappingResult.Records);
        var unresolvedPlaceholders = effectiveRules
            .SelectMany(rule =>
            {
                var mappingRule = CreatePreviewMappingRule(rule, result);
                return GetUnresolvedPlaceholders(rule.OutputTemplate, mappingRule.SourcePath, patient, result.Measurements)
                    .Select(placeholder => $"Platzhalter konnte nicht aufgelöst werden: {{{placeholder}}} ({rule.TargetFieldCode} {rule.TargetName})");
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"Exportprofil: {exportProfile.Metadata.Name}");
        if (draftRule is not null)
        {
            builder.AppendLine("Entwurfsregel aktiv: Diese Vorschau ist temporär und wurde nicht gespeichert.");
        }

        if (mappingResult.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Mapping-Fehler:");
            foreach (var issue in mappingResult.Issues)
            {
                builder.AppendLine($"- {issue.Severity}: {issue.Message} SourcePath={issue.SourcePath}, TargetFieldCode={issue.TargetFieldCode}");
            }
        }

        if (unresolvedPlaceholders.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Ein oder mehrere Platzhalter konnten nicht aufgelöst werden:");
            foreach (var placeholder in unresolvedPlaceholders)
            {
                builder.AppendLine($"- {placeholder}");
            }
        }

        if (exportResult.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("ExportBuilder-Fehler:");
            foreach (var issue in exportResult.Issues)
            {
                builder.AppendLine($"- {issue.Severity}: {issue.Message} FieldCode={issue.FieldCode}, Value={issue.Value}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("ExportContent:");
        builder.Append(exportResult.Content.Length == 0 ? "(leer)" : exportResult.Content);

        return builder.ToString().TrimEnd();
    }

    private IReadOnlyList<ExportRuleDefinition> GetEffectiveExportRules(
        ExportProfileDefinition exportProfile,
        ExportRuleDefinition? draftRule,
        string? replaceRuleId)
    {
        var rules = exportProfile.Rules.Concat(_temporaryExportRules).ToList();
        if (draftRule is null || string.IsNullOrWhiteSpace(replaceRuleId))
        {
            return rules;
        }

        return rules
            .Select(rule => rule.Id == replaceRuleId ? draftRule : rule)
            .ToList();
    }

    private bool IsTemporaryRule(ExportRuleDefinition? rule)
    {
        return rule is not null && _temporaryExportRules.Any(temporaryRule => temporaryRule.Id == rule.Id);
    }

    private void UpdateDraftModeStatus(ExportRuleDefinition? rule)
    {
        DraftModeStatusText.Text = IsTemporaryRule(rule)
            ? "Entwurfsmodus: Änderungen und neue Regeln werden noch nicht gespeichert.\nNeue Entwurfsregel aktiv. Sie erscheint nur in der Vorschau."
            : "Entwurfsmodus: Änderungen und neue Regeln werden noch nicht gespeichert.";
    }

    private void LoadDraftFromSelectedRule()
    {
        if (ExportRulesGrid.SelectedItem is not ExportRuleDefinition rule)
        {
            ClearDraftRuleEditor();
            return;
        }

        DraftTargetFieldCodeTextBox.Text = rule.TargetFieldCode;
        DraftTargetNameTextBox.Text = rule.TargetName;
        DraftRuleTypeComboBox.SelectedItem = rule.RuleType;
        DraftSourcePathTextBox.Text = rule.SourcePath ?? string.Empty;
        DraftOutputTemplateTextBox.Text = rule.OutputTemplate;
        DraftSortOrderTextBox.Text = rule.SortOrder.ToString();
        DraftIsEnabledCheckBox.IsChecked = rule.IsEnabled;
        DraftDescriptionTextBox.Text = rule.Description ?? string.Empty;
        UpdateDraftModeStatus(rule);
        RefreshPlaceholderUsageFromDraft();
    }

    private void ClearDraftRuleEditor()
    {
        DraftTargetFieldCodeTextBox.Text = string.Empty;
        DraftTargetNameTextBox.Text = string.Empty;
        DraftRuleTypeComboBox.SelectedItem = ExportRuleType.Template;
        DraftSourcePathTextBox.Text = string.Empty;
        DraftOutputTemplateTextBox.Text = string.Empty;
        DraftSortOrderTextBox.Text = string.Empty;
        DraftIsEnabledCheckBox.IsChecked = false;
        DraftDescriptionTextBox.Text = string.Empty;
        UpdateDraftModeStatus(null);
        RefreshPlaceholderUsageFromDraft();
    }

    private void UpdateDraftPreview_Click(object sender, RoutedEventArgs e)
    {
        UpdateDraftPreviewFromCurrentDraft();
    }

    private void UpdateDraftPreviewFromCurrentDraft()
    {
        if (ExportRulesGrid.SelectedItem is not ExportRuleDefinition selectedRule)
        {
            ExportRulePreviewTextBox.Text = "Keine Exportregel ausgewählt.";
            FullExportPreviewTextBox.Text = "Keine Exportregel ausgewählt.";
            return;
        }

        if (!TryCreateDraftRule(selectedRule, out var draftRule, out var message))
        {
            ExportRulePreviewTextBox.Text = message;
            FullExportPreviewTextBox.Text = message;
            return;
        }

        ExportRulePreviewTextBox.Text = FormatExportRulePreview(draftRule);

        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            FullExportPreviewTextBox.Text = "Kein Exportprofil ausgewählt.";
            return;
        }

        FullExportPreviewTextBox.Text = FormatFullExportPreview(exportProfile, draftRule, selectedRule.Id);
    }

    private void ResetDraft_Click(object sender, RoutedEventArgs e)
    {
        LoadDraftFromSelectedRule();
        ShowExportRulePreviewForSelectedRule();
        ShowFullExportPreviewForSelectedProfile();
    }

    private void InitializeInterfaceProfileConfiguration(ProfileCatalog catalog, string? selectedInterfaceProfileId = null)
    {
        var interfaceProfiles = catalog.InterfaceProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        InterfaceProfileComboBox.ItemsSource = interfaceProfiles;
        if (interfaceProfiles.Count == 0)
        {
            InterfaceProfileComboBox.SelectedIndex = -1;
            ClearInterfaceProfileEditor();
            return;
        }

        var selectedProfile = string.IsNullOrWhiteSpace(selectedInterfaceProfileId)
            ? null
            : interfaceProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, selectedInterfaceProfileId, StringComparison.Ordinal));

        InterfaceProfileComboBox.SelectedItem = selectedProfile ?? interfaceProfiles[0];
        ShowInterfaceProfileForSelectedProfile();
    }

    private void InterfaceProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ShowInterfaceProfileForSelectedProfile();
    }

    private void ShowInterfaceProfileForSelectedProfile()
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition profile)
        {
            ClearInterfaceProfileEditor();
            return;
        }

        InterfaceAisProfileText.Text = GetAisProfileDisplayName(profile.AisProfileId);
        InterfaceDeviceProfileText.Text = GetDeviceProfileDisplayName(profile.DeviceProfileId);
        InterfaceExportProfileText.Text = GetExportProfileDisplayName(profile.ExportProfileId);
        InterfaceIsActiveCheckBox.IsChecked = profile.IsActive;
        InterfaceIsLicenseRequiredCheckBox.IsChecked = profile.IsLicenseRequired;

        InterfaceAisImportFolderTextBox.Text = profile.FolderOptions.AisImportFolder;
        InterfaceDeviceImportFolderTextBox.Text = profile.FolderOptions.DeviceImportFolder;
        InterfaceExportFolderTextBox.Text = profile.FolderOptions.ExportFolder;
        InterfaceArchiveFolderTextBox.Text = profile.FolderOptions.ArchiveFolder;
        InterfaceErrorFolderTextBox.Text = profile.FolderOptions.ErrorFolder;

        InterfaceClearAisImportFolderCheckBox.IsChecked = profile.FolderOptions.ClearAisImportFolderBeforeProcessing;
        InterfaceClearDeviceImportFolderCheckBox.IsChecked = profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing;
        InterfaceClearExportFolderCheckBox.IsChecked = profile.FolderOptions.ClearExportFolderAfterSuccessfulTransfer;
        InterfaceArchiveProcessedFilesCheckBox.IsChecked = profile.FolderOptions.ArchiveProcessedFiles;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked = profile.FolderOptions.MoveFailedFilesToErrorFolder;
    }

    private void ClearInterfaceProfileEditor()
    {
        InterfaceAisProfileText.Text = string.Empty;
        InterfaceDeviceProfileText.Text = string.Empty;
        InterfaceExportProfileText.Text = string.Empty;
        InterfaceIsActiveCheckBox.IsChecked = false;
        InterfaceIsLicenseRequiredCheckBox.IsChecked = false;
        InterfaceAisImportFolderTextBox.Text = string.Empty;
        InterfaceDeviceImportFolderTextBox.Text = string.Empty;
        InterfaceExportFolderTextBox.Text = string.Empty;
        InterfaceArchiveFolderTextBox.Text = string.Empty;
        InterfaceErrorFolderTextBox.Text = string.Empty;
        InterfaceClearAisImportFolderCheckBox.IsChecked = false;
        InterfaceClearDeviceImportFolderCheckBox.IsChecked = false;
        InterfaceClearExportFolderCheckBox.IsChecked = false;
        InterfaceArchiveProcessedFilesCheckBox.IsChecked = false;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked = false;
    }

    private void CreateInterfaceProfileForSelectedExport_Click(object sender, RoutedEventArgs e)
    {
        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            AppendProfileMessage("Schnittstellenprofil kann nicht erstellt werden, weil kein Exportprofil ausgewählt ist.");
            return;
        }

        try
        {
            var profile = _interfaceProfileConfigurationService.CreateForExportProfile(
                exportProfile,
                DateTimeOffset.UtcNow,
                Environment.UserName);
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(
                catalog,
                selectedExportProfileId: exportProfile.Metadata.Id,
                selectedInterfaceProfileId: profile.Metadata.Id);
            AppendProfileMessage($"Schnittstellenprofil erstellt: {profile.Metadata.Name}");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Schnittstellenprofil konnte nicht erstellt werden: {ex.Message}");
        }
    }

    private void SaveInterfaceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition selectedProfile)
        {
            AppendProfileMessage("Schnittstellenprofil kann nicht gespeichert werden, weil kein Schnittstellenprofil ausgewählt ist.");
            return;
        }

        var selectedExportProfileId = (ExportProfileComboBox.SelectedItem as ExportProfileDefinition)?.Metadata.Id;
        var folderOptions = CreateInterfaceFolderOptionsFromEditor();
        var result = _interfaceProfileConfigurationService.CreateConfiguredProfile(
            selectedProfile,
            folderOptions,
            InterfaceIsActiveCheckBox.IsChecked == true,
            InterfaceIsLicenseRequiredCheckBox.IsChecked == true,
            DateTimeOffset.UtcNow,
            Environment.UserName);

        if (!result.Success || result.Profile is null)
        {
            AppendProfileMessage("Schnittstellenprofil wurde nicht gespeichert:");
            AppendInterfaceConfigurationIssues(result.Issues);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var overwriteExisting = selectedProfile.Metadata.IsUserDefined && !selectedProfile.Metadata.IsBuiltIn;
            _profileCatalogService.SaveInterfaceProfileDefinition(paths, result.Profile, overwriteExisting);

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(
                catalog,
                selectedExportProfileId: selectedExportProfileId,
                selectedInterfaceProfileId: result.Profile.Metadata.Id);

            AppendInterfaceConfigurationIssues(result.Issues);
            AppendProfileMessage("Schnittstellenprofil gespeichert.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Schnittstellenprofil konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void RemoveInterfaceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition selectedProfile)
        {
            AppendProfileMessage("Schnittstellenprofil kann nicht entfernt werden, weil kein Schnittstellenprofil ausgewählt ist.");
            return;
        }

        if (selectedProfile.Metadata.IsBuiltIn)
        {
            AppendProfileMessage("Standard-Schnittstellenprofile können nicht gelöscht werden.");
            return;
        }

        if (!selectedProfile.Metadata.IsUserDefined)
        {
            AppendProfileMessage("Nur benutzerdefinierte Schnittstellenprofile können entfernt werden.");
            return;
        }

        var confirmation = System.Windows.MessageBox.Show(
            "Möchten Sie dieses Schnittstellenprofil wirklich entfernen? Es werden keine Import-/Exportordner geleert.",
            "Schnittstellenprofil entfernen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var selectedExportProfileId = (ExportProfileComboBox.SelectedItem as ExportProfileDefinition)?.Metadata.Id;
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var deleted = _profileCatalogService.DeleteInterfaceProfile(paths, selectedProfile.Metadata.Id);
            if (!deleted)
            {
                AppendProfileMessage($"Schnittstellenprofil wurde nicht gefunden: {selectedProfile.Metadata.Name}");
                return;
            }

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(catalog, selectedExportProfileId: selectedExportProfileId);
            AppendProfileMessage($"Schnittstellenprofil entfernt: {selectedProfile.Metadata.Name}");
        }
        catch (FileNotFoundException ex)
        {
            AppendProfileMessage($"Schnittstellenprofil-Datei wurde nicht gefunden: {ex.FileName ?? ex.Message}");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Schnittstellenprofil konnte nicht entfernt werden: {ex.Message}");
        }
    }

    private InterfaceFolderOptions CreateInterfaceFolderOptionsFromEditor()
    {
        return new InterfaceFolderOptions(
            AisImportFolder: InterfaceAisImportFolderTextBox.Text.Trim(),
            DeviceImportFolder: InterfaceDeviceImportFolderTextBox.Text.Trim(),
            ExportFolder: InterfaceExportFolderTextBox.Text.Trim(),
            ArchiveFolder: InterfaceArchiveFolderTextBox.Text.Trim(),
            ErrorFolder: InterfaceErrorFolderTextBox.Text.Trim(),
            ClearAisImportFolderBeforeProcessing: InterfaceClearAisImportFolderCheckBox.IsChecked == true,
            ClearDeviceImportFolderBeforeProcessing: InterfaceClearDeviceImportFolderCheckBox.IsChecked == true,
            ClearExportFolderAfterSuccessfulTransfer: InterfaceClearExportFolderCheckBox.IsChecked == true,
            ArchiveProcessedFiles: InterfaceArchiveProcessedFilesCheckBox.IsChecked == true,
            MoveFailedFilesToErrorFolder: InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked == true);
    }

    private void SelectInterfaceFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button || button.Tag is not string tag)
        {
            return;
        }

        var targetTextBox = tag switch
        {
            "AisImport" => InterfaceAisImportFolderTextBox,
            "DeviceImport" => InterfaceDeviceImportFolderTextBox,
            "Export" => InterfaceExportFolderTextBox,
            "Archive" => InterfaceArchiveFolderTextBox,
            "Error" => InterfaceErrorFolderTextBox,
            _ => null
        };

        if (targetTextBox is null)
        {
            return;
        }

        using var dialog = new WinForms.FolderBrowserDialog
        {
            Description = "Ordner auswählen. UNC-Pfade können direkt im Textfeld eingetragen werden."
        };

        var currentPath = targetTextBox.Text.Trim();
        if (Directory.Exists(currentPath))
        {
            dialog.SelectedPath = currentPath;
        }

        if (dialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            targetTextBox.Text = dialog.SelectedPath;
        }
    }

    private void AppendInterfaceConfigurationIssues(IReadOnlyList<InterfaceProfileConfigurationIssue> issues)
    {
        foreach (var issue in issues)
        {
            var pathPart = string.IsNullOrWhiteSpace(issue.Path) ? string.Empty : $" ({issue.Path})";
            AppendProfileMessage($"[Schnittstellenprofil] {issue.Severity}: {issue.Message}{pathPart}");
        }
    }

    private string GetAisProfileDisplayName(string profileId)
    {
        var profile = _profileCatalog?.AisProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.Ordinal));
        return profile is null ? profileId : $"{profile.Name} ({profileId})";
    }

    private string GetDeviceProfileDisplayName(string profileId)
    {
        var profile = _profileCatalog?.DeviceProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.Ordinal));
        return profile is null ? profileId : $"{profile.Metadata.Name} ({profileId})";
    }

    private string GetExportProfileDisplayName(string profileId)
    {
        var profile = _profileCatalog?.ExportProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, profileId, StringComparison.Ordinal));
        return profile is null ? profileId : $"{profile.Metadata.Name} ({profileId})";
    }

    private void SaveDraftAsNewExportProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            AppendProfileMessage("Neues Exportprofil kann nicht gespeichert werden, weil kein Exportprofil ausgewählt ist.");
            return;
        }

        if (ExportRulesGrid.SelectedItem is not ExportRuleDefinition selectedRule)
        {
            AppendProfileMessage("Neues Exportprofil kann nicht gespeichert werden, weil keine Exportregel ausgewählt ist.");
            return;
        }

        if (!TryCreateDraftRule(selectedRule, out var draftRule, out var draftMessage))
        {
            AppendProfileMessage(draftMessage);
            return;
        }

        var newProfileName = NewExportProfileNameTextBox.Text.Trim();
        var draftResult = _exportProfileDraftService.CreateUserDefinedCopy(
            exportProfile,
            newProfileName,
            draftRule,
            selectedRule.Id,
            _temporaryExportRules,
            DateTimeOffset.UtcNow,
            Environment.UserName);

        if (!draftResult.Success || draftResult.Profile is null)
        {
            AppendProfileMessage("Neues Exportprofil wurde nicht gespeichert:");
            foreach (var issue in draftResult.Issues)
            {
                AppendProfileMessage($"[Exportprofil-Entwurf] {issue}");
            }

            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewExportProfile(paths, draftResult.Profile);

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(catalog, selectedExportProfileId: draftResult.Profile.Metadata.Id);

            AppendProfileMessage($"Neues Exportprofil gespeichert: {draftResult.Profile.Metadata.Name}");
            AppendProfileMessage("Profil wurde als neues Exportprofil gespeichert. Das Originalprofil wurde nicht verändert.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Neues Exportprofil konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void RefreshProfileOverview(
        ProfileCatalog catalog,
        string? selectedExportProfileId = null,
        string? selectedInterfaceProfileId = null)
    {
        AisProfileCountText.Text = catalog.AisProfiles.Count.ToString();
        DeviceProfileCountText.Text = catalog.DeviceProfiles.Count.ToString();
        ExportProfileCountText.Text = catalog.ExportProfiles.Count.ToString();
        InterfaceProfileCountText.Text = catalog.InterfaceProfiles.Count.ToString();
        ShowProfileNameColumns(catalog);
        InitializeExportRulesView(catalog, selectedExportProfileId);
        InitializeInterfaceProfileConfiguration(catalog, selectedInterfaceProfileId);
        RefreshLicensedDeviceStatesFromLocalLicense();
    }

    private bool TryCreateDraftRule(
        ExportRuleDefinition selectedRule,
        out ExportRuleDefinition draftRule,
        out string message)
    {
        draftRule = selectedRule;
        message = string.Empty;

        var targetFieldCode = DraftTargetFieldCodeTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(targetFieldCode))
        {
            message = "TargetFieldCode darf nicht leer sein. Entwurfsvorschau wurde nicht aktualisiert.";
            return false;
        }

        if (!int.TryParse(DraftSortOrderTextBox.Text.Trim(), out var sortOrder))
        {
            message = "SortOrder muss eine ganze Zahl sein. Entwurfsvorschau wurde nicht aktualisiert.";
            return false;
        }

        var ruleType = DraftRuleTypeComboBox.SelectedItem is ExportRuleType selectedRuleType
            ? selectedRuleType
            : selectedRule.RuleType;

        draftRule = new ExportRuleDefinition(
            Id: selectedRule.Id,
            TargetFieldCode: targetFieldCode,
            TargetName: DraftTargetNameTextBox.Text.Trim(),
            RuleType: ruleType,
            SourcePath: string.IsNullOrWhiteSpace(DraftSourcePathTextBox.Text) ? null : DraftSourcePathTextBox.Text.Trim(),
            OutputTemplate: DraftOutputTemplateTextBox.Text,
            SortOrder: sortOrder,
            IsEnabled: DraftIsEnabledCheckBox.IsChecked == true,
            Description: string.IsNullOrWhiteSpace(DraftDescriptionTextBox.Text) ? null : DraftDescriptionTextBox.Text.Trim());

        return true;
    }

    private static MappingRule CreatePreviewMappingRule(ExportRuleDefinition rule, ProcessingPipelineResult result)
    {
        return new MappingRule(
            Id: $"preview-{rule.Id}",
            TargetFieldCode: string.IsNullOrWhiteSpace(rule.TargetFieldCode) ? "PREVIEW" : rule.TargetFieldCode,
            TargetName: rule.TargetName,
            SourcePath: GetPreviewSourcePath(rule, result),
            OutputTemplate: rule.OutputTemplate,
            SortOrder: rule.SortOrder,
            IsEnabled: rule.IsEnabled);
    }

    private static string GetPreviewSourcePath(ExportRuleDefinition rule, ProcessingPipelineResult result)
    {
        if (!string.IsNullOrWhiteSpace(rule.SourcePath))
        {
            return rule.SourcePath;
        }

        if (!string.IsNullOrWhiteSpace(result.Patient?.PatientNumber))
        {
            return "AIS.PatientNumber";
        }

        var firstMeasurementPath = result.Measurements.FirstOrDefault(measurement => !string.IsNullOrWhiteSpace(measurement.SourcePath))?.SourcePath;
        return firstMeasurementPath is null ? "AIS.PatientNumber" : $"Device.{firstMeasurementPath}";
    }

    private static PatientData CreateEmptyPatientData()
    {
        return new PatientData(
            PatientNumber: null,
            LastName: null,
            FirstName: null,
            BirthDate: null,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }

    private static IReadOnlyList<string> GetUnresolvedPlaceholders(
        string template,
        string previewSourcePath,
        PatientData patient,
        IReadOnlyList<MeasurementValue> measurements)
    {
        var unresolved = new List<string>();
        var measurementPaths = measurements
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var token in ExtractPlaceholderTokens(template))
        {
            var sourceToken = SplitPreviewFormatToken(token);
            if (string.Equals(sourceToken, "value", StringComparison.OrdinalIgnoreCase))
            {
                if (!CanResolvePreviewSource(previewSourcePath, patient, measurementPaths))
                {
                    unresolved.Add(token);
                }

                continue;
            }

            if (sourceToken.StartsWith("patient.", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(GetPatientPreviewValue($"AIS.{sourceToken[8..]}", patient)))
                {
                    unresolved.Add(token);
                }

                continue;
            }

            if (sourceToken.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(GetPatientPreviewValue(sourceToken, patient)))
                {
                    unresolved.Add(token);
                }

                continue;
            }

            if (sourceToken.StartsWith("Device.", StringComparison.OrdinalIgnoreCase))
            {
                if (!measurementPaths.Contains(sourceToken[7..]))
                {
                    unresolved.Add(token);
                }

                continue;
            }

            unresolved.Add(token);
        }

        return unresolved;
    }

    private static IReadOnlyList<string> GetRoundBracketPlaceholderCandidates(string template)
    {
        var candidates = new List<string>();
        var index = 0;

        while (index < template.Length)
        {
            var openIndex = template.IndexOf('(', index);
            if (openIndex < 0)
            {
                break;
            }

            var closeIndex = template.IndexOf(')', openIndex + 1);
            if (closeIndex < 0)
            {
                break;
            }

            var token = template[(openIndex + 1)..closeIndex].Trim();
            if (LooksLikePlaceholderToken(token))
            {
                candidates.Add(token);
            }

            index = closeIndex + 1;
        }

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool LooksLikePlaceholderToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var sourceToken = SplitPreviewFormatToken(token);
        return sourceToken.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase)
            || sourceToken.StartsWith("Device.", StringComparison.OrdinalIgnoreCase)
            || sourceToken.StartsWith("Patient.", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourceToken, "value", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendRoundBracketPlaceholderHint(StringBuilder builder, IReadOnlyList<string> placeholders)
    {
        if (placeholders.Count == 0)
        {
            return;
        }

        builder.AppendLine("Runde Klammern werden nicht als Platzhalter erkannt. Verwenden Sie geschweifte Klammern, z. B. {Device.Date}.");
        foreach (var placeholder in placeholders)
        {
            builder.AppendLine($"- ({placeholder})");
        }

        builder.AppendLine();
    }

    private void DraftOutputTemplateTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_updatingPlaceholderRows)
        {
            return;
        }

        RefreshPlaceholderUsageFromDraft();
    }

    private void PlaceholderUseCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_updatingPlaceholderRows || sender is not System.Windows.Controls.CheckBox checkBox)
        {
            return;
        }

        if (checkBox.DataContext is not PlaceholderRow row)
        {
            return;
        }

        if (row.IsUsed)
        {
            InsertPlaceholderIntoDraft(row);
        }
        else
        {
            RemovePlaceholderFromDraft(row);
        }

        RefreshPlaceholderUsageFromDraft();
        UpdateDraftPreviewFromCurrentDraft();
    }

    private void PlaceholderOutputModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_updatingPlaceholderRows || sender is not System.Windows.Controls.ComboBox comboBox)
        {
            return;
        }

        if (comboBox.DataContext is not PlaceholderRow row || !row.IsUsed)
        {
            return;
        }

        RemovePlaceholderFromDraft(row);
        InsertPlaceholderIntoDraft(row);
        RefreshPlaceholderUsageFromDraft();
        UpdateDraftPreviewFromCurrentDraft();
    }

    private void UpdatePlaceholderTables()
    {
        _updatingPlaceholderRows = true;
        try
        {
            _aisPlaceholderRows.Clear();
            foreach (var row in CreateAisPlaceholderRows())
            {
                _aisPlaceholderRows.Add(row);
            }

            _devicePlaceholderRows.Clear();
            foreach (var row in CreateDevicePlaceholderRows())
            {
                _devicePlaceholderRows.Add(row);
            }

            DevicePlaceholdersStatusText.Text = _lastPipelineResult is null
                ? "Device-Platzhalter - noch keine Gerätedaten geladen"
                : $"Device-Platzhalter - {_devicePlaceholderRows.Count} erkannt";
        }
        finally
        {
            _updatingPlaceholderRows = false;
        }

        RefreshPlaceholderUsageFromDraft();
    }

    private void ClearPlaceholderTables()
    {
        _updatingPlaceholderRows = true;
        try
        {
            _aisPlaceholderRows.Clear();
            _devicePlaceholderRows.Clear();
            DevicePlaceholdersStatusText.Text = "Device-Platzhalter - noch keine Gerätedaten geladen";
        }
        finally
        {
            _updatingPlaceholderRows = false;
        }
    }

    private IReadOnlyList<PlaceholderRow> CreateAisPlaceholderRows()
    {
        var patient = _lastPipelineResult?.Patient;
        var rows = new[]
        {
            CreatePlaceholderRow("AIS.PatientNumber", GetPatientPreviewValueOrNull("AIS.PatientNumber", patient), 0),
            CreatePlaceholderRow("AIS.LastName", GetPatientPreviewValueOrNull("AIS.LastName", patient), 1),
            CreatePlaceholderRow("AIS.FirstName", GetPatientPreviewValueOrNull("AIS.FirstName", patient), 2),
            CreatePlaceholderRow("AIS.BirthDate", GetPatientPreviewValueOrNull("AIS.BirthDate", patient), 3),
            CreatePlaceholderRow("AIS.ExaminationType", GetPatientPreviewValueOrNull("AIS.ExaminationType", patient), 4),
            CreatePlaceholderRow("AIS.Street", GetPatientPreviewValueOrNull("AIS.Street", patient), 5),
            CreatePlaceholderRow("AIS.PostalCodeCity", GetPatientPreviewValueOrNull("AIS.PostalCodeCity", patient), 6)
        };

        return rows
            .OrderBy(row => row.SortOrder)
            .ToList();
    }

    private IReadOnlyList<PlaceholderRow> CreateDevicePlaceholderRows()
    {
        if (_lastPipelineResult is null)
        {
            return Array.Empty<PlaceholderRow>();
        }

        return _lastPipelineResult.Measurements
            .Where(measurement => !string.IsNullOrWhiteSpace(measurement.SourcePath))
            .GroupBy(measurement => measurement.SourcePath, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var firstWithValue = group.FirstOrDefault(measurement => !string.IsNullOrWhiteSpace(measurement.Value));
                var measurement = firstWithValue ?? group.First();
                var placeholder = $"Device.{group.Key}";
                return new PlaceholderRow(
                    placeholder,
                    GetFriendlyPlaceholderName(placeholder, measurement.DisplayName),
                    measurement.Value ?? string.Empty,
                    sortOrder: GetDevicePlaceholderSortOrder(group.Key));
            })
            .OrderByDescending(row => row.HasValue)
            .ThenBy(row => row.SortOrder)
            .ThenBy(row => row.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(row => row.Placeholder, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static PlaceholderRow CreatePlaceholderRow(string placeholder, string? value, int sortOrder)
    {
        return new PlaceholderRow(
            placeholder,
            GetFriendlyPlaceholderName(placeholder, displayName: null),
            value ?? string.Empty,
            sortOrder);
    }

    private void RefreshPlaceholderUsageFromDraft()
    {
        var template = DraftOutputTemplateTextBox.Text ?? string.Empty;
        _updatingPlaceholderRows = true;
        try
        {
            foreach (var row in _aisPlaceholderRows.Concat(_devicePlaceholderRows))
            {
                var outputMode = DetectOutputMode(template, row);
                row.OutputMode = outputMode ?? PlaceholderRow.OutputModeAis;
                row.IsUsed = outputMode is not null;
            }
        }
        finally
        {
            _updatingPlaceholderRows = false;
        }
    }

    private void InsertPlaceholderIntoDraft(PlaceholderRow row)
    {
        var text = DraftOutputTemplateTextBox.Text ?? string.Empty;
        if (TemplateContainsPlaceholder(text, row.Placeholder))
        {
            return;
        }

        var token = BuildPlaceholderToken(row.Placeholder);
        var insertedText = string.Equals(row.OutputMode, PlaceholderRow.OutputModeHuman, StringComparison.OrdinalIgnoreCase)
            ? $"{row.DisplayName}: {token}"
            : token;
        var insertionIndex = DraftOutputTemplateTextBox.IsKeyboardFocusWithin
            ? Math.Clamp(DraftOutputTemplateTextBox.CaretIndex, 0, text.Length)
            : text.Length;
        var prefix = insertionIndex > 0 && !char.IsWhiteSpace(text[insertionIndex - 1]) ? " " : string.Empty;
        var suffix = insertionIndex < text.Length && !char.IsWhiteSpace(text[insertionIndex]) ? " " : string.Empty;
        var insertion = $"{prefix}{insertedText}{suffix}";

        DraftOutputTemplateTextBox.Text = text.Insert(insertionIndex, insertion);
        DraftOutputTemplateTextBox.CaretIndex = insertionIndex + prefix.Length + insertedText.Length;
        DraftOutputTemplateTextBox.Focus();
    }

    private void RemovePlaceholderFromDraft(PlaceholderRow row)
    {
        var text = DraftOutputTemplateTextBox.Text ?? string.Empty;
        var tokenPattern = CreatePlaceholderTokenPattern(row.Placeholder);
        var humanPattern = Regex.Escape(row.DisplayName) + @"\s*:\s*" + tokenPattern;
        var updatedText = Regex.Replace(text, humanPattern, string.Empty, RegexOptions.IgnoreCase);
        updatedText = Regex.Replace(updatedText, tokenPattern, string.Empty, RegexOptions.IgnoreCase);
        updatedText = CleanupRemovedPlaceholderWhitespace(updatedText);

        DraftOutputTemplateTextBox.Text = updatedText;
        DraftOutputTemplateTextBox.CaretIndex = Math.Min(DraftOutputTemplateTextBox.CaretIndex, updatedText.Length);
        DraftOutputTemplateTextBox.Focus();
    }

    private static string CleanupRemovedPlaceholderWhitespace(string text)
    {
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => Regex.Replace(line, @" {2,}", " ").TrimEnd());

        return string.Join(Environment.NewLine, lines);
    }

    private static bool TemplateContainsPlaceholder(string template, string placeholder)
    {
        return ExtractPlaceholderTokens(template)
            .Select(SplitPreviewFormatToken)
            .Any(token => string.Equals(token, placeholder, StringComparison.OrdinalIgnoreCase));
    }

    private static string? DetectOutputMode(string template, PlaceholderRow row)
    {
        var tokenPattern = CreatePlaceholderTokenPattern(row.Placeholder);
        var humanPattern = Regex.Escape(row.DisplayName) + @"\s*:\s*" + tokenPattern;
        if (Regex.IsMatch(template, humanPattern, RegexOptions.IgnoreCase))
        {
            return PlaceholderRow.OutputModeHuman;
        }

        return Regex.IsMatch(template, tokenPattern, RegexOptions.IgnoreCase)
            ? PlaceholderRow.OutputModeAis
            : null;
    }

    private static string CreatePlaceholderTokenPattern(string placeholder)
    {
        return @"\{" + Regex.Escape(placeholder) + @"(?::[^{}]+)?\}";
    }

    private static string BuildPlaceholderToken(string placeholder)
    {
        var suggestedFormat = GetSuggestedFormat(placeholder);
        return string.IsNullOrWhiteSpace(suggestedFormat)
            ? $"{{{placeholder}}}"
            : $"{{{placeholder}:{suggestedFormat}}}";
    }

    private static string? GetSuggestedFormat(string placeholder)
    {
        return PlaceholderDisplayHelper.GetSuggestedFormat(placeholder);
    }

    private static string GetFriendlyPlaceholderName(string placeholder, string? displayName)
    {
        return PlaceholderDisplayHelper.GetDisplayName(placeholder, displayName);
    }

    private static string GetFriendlyDevicePlaceholderName(string placeholder, string? displayName)
    {
        var sourcePath = placeholder.StartsWith("Device.", StringComparison.OrdinalIgnoreCase)
            ? placeholder[7..]
            : placeholder;
        var eye = GetFriendlyEyeName(sourcePath);
        var context = GetFriendlyMeasurementContext(sourcePath);

        string name;
        if (ContainsAny(sourcePath, "FarPD"))
        {
            name = "Pupillendistanz Ferne";
        }
        else if (ContainsAny(sourcePath, "NearPD"))
        {
            name = "Pupillendistanz Nähe";
        }
        else if (ContainsAny(sourcePath, "PD"))
        {
            name = "Pupillendistanz";
        }
        else if (ContainsAny(sourcePath, "Sphere", "Sphare"))
        {
            name = "Sphäre";
        }
        else if (ContainsAny(sourcePath, "Cylinder"))
        {
            name = "Zylinder";
        }
        else if (ContainsAny(sourcePath, "Axis"))
        {
            name = "Achse";
        }
        else if (ContainsAny(sourcePath, "SE"))
        {
            name = "Sphärisches Äquivalent";
        }
        else if (ContainsAny(sourcePath, "IOP", "CorrectedIOP", "/NT", "NT/"))
        {
            name = "Augeninnendruck";
        }
        else if (ContainsAny(sourcePath, "Pachy", "PACHY", "CCT"))
        {
            name = "Pachymetrie / Hornhautdicke";
        }
        else if (ContainsAny(sourcePath, "Prism"))
        {
            name = "Prisma";
        }
        else if (ContainsAny(sourcePath, "Keratometry", "K1", "K2", "Power", "Radius"))
        {
            name = "Keratometrie";
        }
        else
        {
            name = !string.IsNullOrWhiteSpace(displayName)
                ? displayName.Trim()
                : DeriveNameFromSourcePath(sourcePath);
        }

        var parts = new List<string> { name };
        if (!string.IsNullOrWhiteSpace(eye) && !name.Contains("Pupillendistanz", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(eye);
        }

        if (!string.IsNullOrWhiteSpace(context))
        {
            parts.Add(context);
        }

        return string.Join(" ", parts);
    }

    private static string GetFriendlyMeasurementContext(string sourcePath)
    {
        var measurementNumber = ExtractMeasurementNumber(sourcePath);
        if (ContainsAny(sourcePath, "ARMedian"))
        {
            return "Berechnung / Median";
        }

        if (ContainsAny(sourcePath, "ARList"))
        {
            return string.IsNullOrWhiteSpace(measurementNumber) ? "Messung" : $"Messung {measurementNumber}";
        }

        if (ContainsAny(sourcePath, "PDList"))
        {
            return string.IsNullOrWhiteSpace(measurementNumber) ? "Messung" : $"Messung {measurementNumber}";
        }

        if (ContainsAny(sourcePath, "/SR/", ".SR.", "SR/"))
        {
            return "SR";
        }

        if (ContainsAny(sourcePath, "TrialLens"))
        {
            return "TrialLens";
        }

        if (ContainsAny(sourcePath, "ContactLens"))
        {
            return "ContactLens";
        }

        return string.Empty;
    }

    private static string ExtractMeasurementNumber(string sourcePath)
    {
        const string marker = "[@No=";
        var markerIndex = sourcePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return string.Empty;
        }

        var valueStart = markerIndex + marker.Length;
        while (valueStart < sourcePath.Length && (sourcePath[valueStart] == '\'' || sourcePath[valueStart] == '"'))
        {
            valueStart++;
        }

        var valueEnd = valueStart;
        while (valueEnd < sourcePath.Length && sourcePath[valueEnd] != '\'' && sourcePath[valueEnd] != '"' && sourcePath[valueEnd] != ']')
        {
            valueEnd++;
        }

        return valueEnd <= valueStart ? string.Empty : sourcePath[valueStart..valueEnd];
    }

    private static int GetDevicePlaceholderSortOrder(string sourcePath)
    {
        return PlaceholderDisplayHelper.GetDeviceSortOrder(sourcePath);
    }

    private static int GetEyeSortOrder(string sourcePath)
    {
        var eye = GetFriendlyEyeName(sourcePath);
        return eye switch
        {
            "rechts" => 0,
            "links" => 1,
            _ => 2
        };
    }

    private static int GetMeasurementGroupSortOrder(string sourcePath)
    {
        if (ContainsAny(sourcePath, "ARMedian"))
        {
            return 0;
        }

        if (ContainsAny(sourcePath, "ARList"))
        {
            return 1;
        }

        if (ContainsAny(sourcePath, "/SR/", ".SR.", "SR/"))
        {
            return 2;
        }

        if (ContainsAny(sourcePath, "TrialLens"))
        {
            return 3;
        }

        if (ContainsAny(sourcePath, "ContactLens"))
        {
            return 4;
        }

        if (ContainsAny(sourcePath, "PDList"))
        {
            return 5;
        }

        return 50;
    }

    private static int GetMeasurementTypeSortOrder(string sourcePath)
    {
        if (ContainsAny(sourcePath, "Sphere", "Sphare"))
        {
            return 0;
        }

        if (ContainsAny(sourcePath, "Cylinder"))
        {
            return 1;
        }

        if (ContainsAny(sourcePath, "Axis"))
        {
            return 2;
        }

        if (ContainsAny(sourcePath, "SE"))
        {
            return 3;
        }

        if (ContainsAny(sourcePath, "FarPD"))
        {
            return 4;
        }

        if (ContainsAny(sourcePath, "NearPD"))
        {
            return 5;
        }

        return 50;
    }

    private static string GetFriendlyEyeName(string sourcePath)
    {
        var segments = sourcePath.Split(new[] { '/', '.', '[', ']', '@', '=', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(segment => string.Equals(segment, "R", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "Right", StringComparison.OrdinalIgnoreCase)))
        {
            return "rechts";
        }

        if (segments.Any(segment => string.Equals(segment, "L", StringComparison.OrdinalIgnoreCase)
            || string.Equals(segment, "Left", StringComparison.OrdinalIgnoreCase)))
        {
            return "links";
        }

        return string.Empty;
    }

    private static string DeriveNameFromSourcePath(string sourcePath)
    {
        var lastSegment = sourcePath
            .Split(new[] { '/', '.', '[', ']', '@', '=', '\'' }, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();

        return string.IsNullOrWhiteSpace(lastSegment)
            ? sourcePath
            : lastSegment;
    }

    private static bool ContainsAny(string value, params string[] parts)
    {
        return parts.Any(part => value.Contains(part, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> ExtractPlaceholderTokens(string template)
    {
        var index = 0;

        while (index < template.Length)
        {
            var openIndex = template.IndexOf('{', index);
            if (openIndex < 0)
            {
                break;
            }

            var closeIndex = template.IndexOf('}', openIndex + 1);
            if (closeIndex < 0)
            {
                break;
            }

            yield return template[(openIndex + 1)..closeIndex];
            index = closeIndex + 1;
        }
    }

    private static string SplitPreviewFormatToken(string token)
    {
        var separatorIndex = token.LastIndexOf(':');
        return separatorIndex < 0 ? token : token[..separatorIndex];
    }

    private static bool CanResolvePreviewSource(
        string sourcePath,
        PatientData patient,
        HashSet<string> measurementPaths)
    {
        if (sourcePath.StartsWith("Device.", StringComparison.OrdinalIgnoreCase))
        {
            return measurementPaths.Contains(sourcePath[7..]);
        }

        return !string.IsNullOrWhiteSpace(GetPatientPreviewValue(sourcePath, patient));
    }

    private static string? GetPatientPreviewValue(string sourcePath, PatientData patient)
    {
        return sourcePath switch
        {
            "AIS.PatientNumber" => patient.PatientNumber,
            "AIS.LastName" => patient.LastName,
            "AIS.FirstName" => patient.FirstName,
            "AIS.BirthDate" => patient.BirthDate,
            "AIS.Street" => patient.Street,
            "AIS.PostalCodeCity" => patient.PostalCodeCity,
            "AIS.GenderCode" => patient.GenderCode,
            "AIS.SourceSystem" => patient.SourceSystem,
            "AIS.TargetSystem" => patient.TargetSystem,
            "AIS.GdtVersion" => patient.GdtVersion,
            "AIS.ExaminationType" => patient.ExaminationType,
            _ => null
        };
    }

    private static string? GetPatientPreviewValueOrNull(string sourcePath, PatientData? patient)
    {
        return patient is null ? null : GetPatientPreviewValue(sourcePath, patient);
    }

    private void ShowProfileNameColumns(ProfileCatalog catalog)
    {
        AisProfileNamesTextBox.Text = FormatProfileNameColumn(catalog.AisProfiles.Select(profile => profile.Name));
        DeviceProfileNamesTextBox.Text = FormatProfileNameColumn(catalog.DeviceProfiles.Select(profile => profile.Metadata.Name));
        ExportProfileNamesTextBox.Text = FormatProfileNameColumn(catalog.ExportProfiles.Select(profile => profile.Metadata.Name));
        InterfaceProfileNamesTextBox.Text = FormatProfileNameColumn(catalog.InterfaceProfiles.Select(profile => profile.Metadata.Name));
    }

    private void ClearProfileNameColumns()
    {
        AisProfileNamesTextBox.Text = "Keine Profile geladen.";
        DeviceProfileNamesTextBox.Text = string.Empty;
        ExportProfileNamesTextBox.Text = string.Empty;
        InterfaceProfileNamesTextBox.Text = string.Empty;
    }

    private static string FormatProfileNameColumn(IEnumerable<string> names)
    {
        var orderedNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (orderedNames.Count == 0)
        {
            return "Keine Profile geladen.";
        }

        return string.Join(Environment.NewLine, orderedNames);
    }

    private void InitializeLicenseOverview()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;
            var activeLicensedDeviceCount = CountActiveLicensedDevices();

            if (!File.Exists(paths.LicenseFile))
            {
                ShowLicensedDeviceStates(license: null);
                ShowLicenseStatus(
                    installation,
                    "Nicht lizenziert / Test- oder Lizenzaktivierung erforderlich",
                    activeLicensedDeviceCount,
                    licensedDeviceCount: 0);
                LicenseMessagesTextBox.Text = "Keine lokale Lizenzdatei gefunden. Die bestehende Verarbeitung bleibt nutzbar.";
                return;
            }

            try
            {
                var license = _licenseFileRepository.Load(paths.LicenseFile);
                var evaluation = _licenseEvaluator.Evaluate(license, installation, activeLicensedDeviceCount, DateTime.UtcNow);

                ShowLicensedDeviceStates(license);
                ShowLicenseStatus(
                    installation,
                    FormatLicenseStatus(evaluation),
                    evaluation.ActiveLicensedDeviceCount,
                    evaluation.LicensedDeviceCount);
                LicenseMessagesTextBox.Text = "Lizenzstatus geladen.";
            }
            catch (Exception ex)
            {
                ShowLicensedDeviceStates(license: null);
                ShowLicenseStatus(
                    installation,
                    "Lizenzdatei konnte nicht geladen werden",
                    activeLicensedDeviceCount,
                    licensedDeviceCount: 0);
                AppendLicenseMessage($"Lizenzdatei konnte nicht geladen werden: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            LicenseInstallationIdText.Text = "-";
            LicenseMachineNameText.Text = "-";
            LicenseUserNameText.Text = "-";
            LicenseTerminalServerText.Text = "-";
            LicenseStatusText.Text = "Lizenzstatus konnte nicht initialisiert werden";
            LicenseActiveDeviceCountText.Text = "0";
            LicenseLicensedDeviceCountText.Text = "0";
            ClearLicensedDeviceStates();
            _installationInfo = null;
            AppendLicenseMessage($"Lizenzstatus konnte nicht initialisiert werden: {ex.Message}");
        }
    }

    private int CountActiveLicensedDevices()
    {
        return _profileCatalog?.InterfaceProfiles.Count(profile => profile.IsActive && profile.IsLicenseRequired) ?? 0;
    }

    private void ShowLicenseStatus(
        InstallationInfo installation,
        string status,
        int activeLicensedDeviceCount,
        int licensedDeviceCount)
    {
        LicenseInstallationIdText.Text = installation.InstallationId;
        LicenseMachineNameText.Text = installation.MachineName;
        LicenseUserNameText.Text = installation.UserName;
        LicenseTerminalServerText.Text = installation.IsTerminalServer ? "Ja" : "Nein";
        LicenseStatusText.Text = status;
        LicenseActiveDeviceCountText.Text = activeLicensedDeviceCount.ToString();
        LicenseLicensedDeviceCountText.Text = licensedDeviceCount.ToString();
    }

    private void RefreshLicensedDeviceStatesFromLocalLicense()
    {
        try
        {
            LicenseInfo? license = null;
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            if (File.Exists(paths.LicenseFile))
            {
                license = _licenseFileRepository.Load(paths.LicenseFile);
            }

            ShowLicensedDeviceStates(license);

            if (_installationInfo is null)
            {
                return;
            }

            var activeLicensedDeviceCount = CountActiveLicensedDevices();
            if (license is null)
            {
                ShowLicenseStatus(
                    _installationInfo,
                    "Nicht lizenziert / Test- oder Lizenzaktivierung erforderlich",
                    activeLicensedDeviceCount,
                    licensedDeviceCount: 0);
                return;
            }

            var evaluation = _licenseEvaluator.Evaluate(license, _installationInfo, activeLicensedDeviceCount, DateTime.UtcNow);
            ShowLicenseStatus(
                _installationInfo,
                FormatLicenseStatus(evaluation),
                evaluation.ActiveLicensedDeviceCount,
                evaluation.LicensedDeviceCount);
        }
        catch
        {
            ShowLicensedDeviceStates(license: null);
        }
    }

    private void ShowLicensedDeviceStates(LicenseInfo? license)
    {
        ShowLicensedDeviceStates(license, LoadGracePeriodStoreOrEmpty());
    }

    private void ShowLicensedDeviceStates(LicenseInfo? license, LicensedDeviceGracePeriodStore gracePeriodStore)
    {
        var states = _licensedDeviceStateEvaluator.Evaluate(
                _profileCatalog?.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>(),
                license,
                gracePeriodStore.GracePeriods,
                DateTime.UtcNow)
            .ToList();

        _licensedDeviceStateRows.Clear();
        foreach (var state in states.OrderBy(state => state.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            _licensedDeviceStateRows.Add(LicenseDeviceStateRow.FromState(state));
        }

        var licenseRequiredStates = states
            .Where(state => state.IsLicenseRequired)
            .ToList();
        var activeLicenseRequiredStates = licenseRequiredStates
            .Where(state => state.IsActive)
            .ToList();
        var coveredCount = activeLicenseRequiredStates.Count(state => state.IsCoveredByLicense);
        var uncoveredCount = activeLicenseRequiredStates.Count - coveredCount;
        var gracePeriodCount = activeLicenseRequiredStates.Count(state => state.IsInGracePeriod);

        LicensedDeviceTotalCountText.Text = licenseRequiredStates.Count.ToString();
        LicensedDeviceActiveCountText.Text = activeLicenseRequiredStates.Count.ToString();
        LicensedDeviceLicensedCountText.Text = (license?.LicensedDeviceCount ?? 0).ToString();
        LicensedDeviceCoveredCountText.Text = coveredCount.ToString();
        LicensedDeviceUncoveredCountText.Text = uncoveredCount.ToString();
        LicensedDeviceGraceCountText.Text = gracePeriodCount.ToString();

        LicensedDeviceStatusText.Text = CreateLicensedDeviceStatusText(
            license is not null,
            licenseRequiredStates.Count,
            activeLicenseRequiredStates.Count,
            uncoveredCount);

        ShowActiveInterfaceProfilesOverview(states);
    }

    private void ShowActiveInterfaceProfilesOverview(IReadOnlyList<LicensedDeviceState> licenseStates)
    {
        _activeInterfaceProfileStatusRows.Clear();

        if (_profileCatalog is null)
        {
            ActiveInterfaceProfilesStatusText.Text = "Keine Profile geladen.";
            return;
        }

        var rows = _activeInterfaceProfileStatusService.BuildRows(
                _profileCatalog.InterfaceProfiles,
                _profileCatalog.AisProfiles,
                _profileCatalog.DeviceProfiles,
                _profileCatalog.ExportProfiles,
                licenseStates)
            .OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        foreach (var row in rows)
        {
            _activeInterfaceProfileStatusRows.Add(row);
        }

        ActiveInterfaceProfilesStatusText.Text = rows.Count == 0
            ? "Keine aktiven Schnittstellenprofile konfiguriert."
            : $"{rows.Count} aktive Schnittstellenprofil(e) für die spätere automatische Verarbeitung konfiguriert.";
    }

    private void ClearActiveInterfaceProfilesOverview(string message)
    {
        _activeInterfaceProfileStatusRows.Clear();
        ActiveInterfaceProfilesStatusText.Text = message;
    }

    private async void ScanActiveProfilesOnce_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            ActiveProfileScanResultTextBox.Text = "Keine Profile geladen.";
            return;
        }

        var activeProfiles = _profileCatalog.InterfaceProfiles
            .Where(profile => profile.IsActive)
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (activeProfiles.Count == 0)
        {
            _scannedImportPairRows.Clear();
            ProcessSelectedImportPairButton.IsEnabled = false;
            ActiveProfileScanResultTextBox.Text = "Keine aktiven Schnittstellenprofile vorhanden.";
            return;
        }

        _scannedImportPairRows.Clear();
        ProcessSelectedImportPairButton.IsEnabled = false;
        ActiveProfileScanButton.IsEnabled = false;
        ActiveProfileScanResultTextBox.Text = "Scan läuft. Es wird nichts verarbeitet, gelöscht oder exportiert.";

        try
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Einmaliger Scan: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            builder.AppendLine("Hinweis: Es wird nichts verarbeitet, gelöscht oder exportiert.");

            foreach (var profile in activeProfiles)
            {
                builder.AppendLine();
                builder.AppendLine(profile.Metadata.Name);

                try
                {
                    var result = await _autoImportScannerService
                        .ScanOnceAsync(profile, TimeSpan.FromMilliseconds(200))
                        .ConfigureAwait(true);

                    builder.AppendLine($"AIS-Dateien erkannt: {result.AisFilesDetected}");
                    builder.AppendLine($"Geräte-Dateien erkannt: {result.DeviceFilesDetected}");
                    builder.AppendLine($"Dateien in Warteliste: {result.FilesQueued}");
                    builder.AppendLine($"Fertige Paare: {result.ReadyPairs}");

                    if (result.Messages.Count == 0)
                    {
                        builder.AppendLine("Meldungen: keine.");
                    }
                    else
                    {
                        builder.AppendLine("Meldungen:");
                        foreach (var message in result.Messages)
                        {
                            builder.AppendLine($"- {message}");
                        }
                    }

                    var exportProfileName = GetExportProfileDisplayName(profile.ExportProfileId);
                    foreach (var pair in result.Queue.FindReadyPairs())
                    {
                        _scannedImportPairRows.Add(new ScannedImportPairRow(
                            InterfaceProfileId: profile.Metadata.Id,
                            InterfaceProfileName: profile.Metadata.Name,
                            ExportProfileId: profile.ExportProfileId,
                            ExportProfileName: exportProfileName,
                            AisFilePath: pair.AisFile.FilePath,
                            DeviceFilePath: pair.DeviceFile.FilePath,
                            ExportFolder: profile.FolderOptions.ExportFolder,
                            Status: "Bereit"));
                    }
                }
                catch (Exception ex)
                {
                    builder.AppendLine($"Scan-Fehler: {ex.Message}");
                }
            }

            if (_scannedImportPairRows.Count == 0)
            {
                builder.AppendLine();
                builder.AppendLine("Keine verarbeitbaren Dateipaare gefunden.");
            }

            ActiveProfileScanResultTextBox.Text = builder.ToString().TrimEnd();
            ActiveProfileScanResultTextBox.ScrollToHome();
        }
        finally
        {
            ActiveProfileScanButton.IsEnabled = true;
        }
    }

    private void ScannedImportPairsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ProcessSelectedImportPairButton.IsEnabled = ScannedImportPairsGrid.SelectedItem is ScannedImportPairRow;
    }

    private void ProcessSelectedImportPair_Click(object sender, RoutedEventArgs e)
    {
        if (ScannedImportPairsGrid.SelectedItem is not ScannedImportPairRow selectedPair)
        {
            AppendMessage("Kein Dateipaar ausgewählt.");
            return;
        }

        if (_profileCatalog is null)
        {
            SetScannedPairStatus(selectedPair, "Fehler");
            AppendMessage("Dateipaar kann nicht verarbeitet werden, weil keine Profile geladen sind.");
            return;
        }

        var interfaceProfile = _profileCatalog.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, selectedPair.InterfaceProfileId, StringComparison.Ordinal));
        if (interfaceProfile is null)
        {
            SetScannedPairStatus(selectedPair, "Fehler");
            AppendMessage("Zugehöriges Schnittstellenprofil wurde nicht gefunden.");
            return;
        }

        var exportProfile = _profileCatalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, selectedPair.ExportProfileId, StringComparison.Ordinal));
        if (exportProfile is null)
        {
            SetScannedPairStatus(selectedPair, "Fehler");
            AppendMessage("Zugehöriges Exportprofil wurde nicht gefunden.");
            return;
        }

        try
        {
            SetScannedPairStatus(selectedPair, "Wird verarbeitet");
            var result = _interfaceProfileManualProcessor.Process(
                interfaceProfile,
                exportProfile,
                selectedPair.AisFilePath,
                selectedPair.DeviceFilePath,
                DateTime.Now);

            if (!result.Success)
            {
                SetScannedPairStatus(selectedPair, CreateFailedPairStatus(result));
                foreach (var message in result.Messages)
                {
                    AppendMessage(message);
                }

                return;
            }

            SetScannedPairStatus(selectedPair, CreateProcessedPairStatus(result));
            if (result.PipelineResult is not null)
            {
                _lastPipelineResult = result.PipelineResult;
                ShowPatient(result.PipelineResult.Patient);
                MeasurementsGrid.ItemsSource = result.PipelineResult.Measurements;
                ShowIssues(result.PipelineResult.Issues);
                UpdatePlaceholderTables();
                ShowExportRulePreviewForSelectedRule();
                ShowFullExportPreviewForSelectedProfile();
            }

            ExportPreviewTextBox.Text = result.ExportContent ?? string.Empty;
            PlannedFileNameText.Text = $"Exportdatei: {result.ExportFilePath}";
            AppendMessage($"Dateipaar erfolgreich verarbeitet. Exportdatei: {result.ExportFilePath}");
            foreach (var message in result.Messages.Where(message => !string.Equals(message, "Dateipaar erfolgreich verarbeitet.", StringComparison.Ordinal)))
            {
                AppendMessage(message);
            }
        }
        catch (Exception ex)
        {
            SetScannedPairStatus(selectedPair, "Fehler");
            AppendMessage($"Dateipaar konnte nicht verarbeitet werden: {ex.Message}");
        }
    }

    private static string CreateProcessedPairStatus(InterfaceProfileManualProcessingResult result)
    {
        if (result.ArchiveResult is null)
        {
            return "Verarbeitet";
        }

        return result.ArchiveResult.HasErrors
            ? "Verarbeitet, Archivierung mit Fehlern"
            : "Verarbeitet und archiviert";
    }

    private static string CreateFailedPairStatus(InterfaceProfileManualProcessingResult result)
    {
        if (result.FailedFileCopyResult is null)
        {
            return "Fehler";
        }

        return result.FailedFileCopyResult.HasErrors
            ? "Fehler, Fehlerablage fehlgeschlagen"
            : "Fehler, Dateien kopiert";
    }

    private void SetScannedPairStatus(ScannedImportPairRow row, string status)
    {
        row.Status = status;
        ScannedImportPairsGrid.Items.Refresh();
    }

    private static string CreateLicensedDeviceStatusText(
        bool hasLicense,
        int licenseRequiredCount,
        int activeLicenseRequiredCount,
        int uncoveredCount)
    {
        if (activeLicenseRequiredCount == 0 && licenseRequiredCount > 0)
        {
            return "Es gibt lizenzpflichtige Anbindungen, aber keine davon ist aktiv.";
        }

        if (activeLicenseRequiredCount == 0)
        {
            return "Keine aktiven lizenzpflichtigen Anbindungen.";
        }

        if (!hasLicense)
        {
            return "Keine Lizenzdatei vorhanden. Aktive lizenzpflichtige Anbindungen werden als nicht gedeckt angezeigt; die Verarbeitung bleibt weiterhin nutzbar.";
        }

        return uncoveredCount == 0
            ? "Alle aktiven lizenzpflichtigen Anbindungen sind durch die aktuelle Lizenz gedeckt."
            : "Mindestens eine aktive lizenzpflichtige Anbindung ist nicht durch die aktuelle Lizenz gedeckt. Die Anzeige sperrt keine Verarbeitung.";
    }

    private LicensedDeviceGracePeriodStore LoadGracePeriodStoreOrEmpty()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            return _licensedDeviceGracePeriodRepository.LoadOrEmpty(paths.DeviceGracePeriodsFile);
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Karenzzeiten konnten nicht geladen werden: {ex.Message}");
            return LicensedDeviceGracePeriodStore.Empty;
        }
    }

    private void ClearLicensedDeviceStates()
    {
        _licensedDeviceStateRows.Clear();
        LicensedDeviceTotalCountText.Text = "0";
        LicensedDeviceActiveCountText.Text = "0";
        LicensedDeviceLicensedCountText.Text = "0";
        LicensedDeviceCoveredCountText.Text = "0";
        LicensedDeviceUncoveredCountText.Text = "0";
        LicensedDeviceGraceCountText.Text = "0";
        LicensedDeviceStatusText.Text = "Lizenzierte Geräte / Anbindungen konnten nicht geladen werden.";
        ShowActiveInterfaceProfilesOverview(Array.Empty<LicensedDeviceState>());
    }

    private static string FormatLicenseStatus(LicenseEvaluationResult evaluation)
    {
        return evaluation.Status switch
        {
            LicenseStatus.TrialActive => "Testphase aktiv",
            LicenseStatus.Active => "Aktiv",
            LicenseStatus.Expired => "Abgelaufen",
            LicenseStatus.Invalid => "Ungültig",
            LicenseStatus.DeviceLimitExceeded => "Geräteanzahl überschritten",
            LicenseStatus.NotLicensed => "Nicht lizenziert / Test- oder Lizenzaktivierung erforderlich",
            _ => evaluation.Status.ToString()
        };
    }

    private void ExportLicenseRequest_Click(object sender, RoutedEventArgs e)
    {
        if (_installationInfo is null)
        {
            AppendLicenseMessage("Lizenzanfrage kann nicht exportiert werden, weil keine Installationsinformationen geladen sind.");
            return;
        }

        if (_profileCatalog is null)
        {
            AppendLicenseMessage("Lizenzanfrage kann nicht exportiert werden, weil keine V2-Profile geladen sind.");
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Lizenzanfrage (*.json)|*.json|Alle Dateien (*.*)|*.*",
            FileName = "XdtDeviceBridge_Lizenzanfrage.json",
            DefaultExt = ".json",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var request = _licenseRequestBuilder.Build(
                _installationInfo,
                _profileCatalog.InterfaceProfiles,
                "XDT_DEVICE_BRIDGE",
                "0.1.0",
                DateTime.UtcNow);

            _licenseRequestFileRepository.Save(dialog.FileName, request);
            AppendLicenseMessage($"Lizenzanfrage erfolgreich exportiert: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Lizenzanfrage konnte nicht exportiert werden: {ex.Message}");
        }
    }

    private void ImportLicenseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Lizenzdatei (*.json)|*.json|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var importedLicense = _licenseFileRepository.Load(dialog.FileName);
            var validationIssues = LicenseInfoValidator.Validate(importedLicense);
            if (validationIssues.Count > 0)
            {
                AppendLicenseMessage("Lizenzdatei ist ungültig und wurde nicht übernommen.");
                foreach (var issue in validationIssues)
                {
                    AppendLicenseMessage($"[Lizenz] {issue}");
                }

                return;
            }

            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;

            _licenseFileRepository.Save(paths.LicenseFile, importedLicense);

            var activeLicensedDeviceCount = CountActiveLicensedDevices();
            var evaluation = _licenseEvaluator.Evaluate(importedLicense, installation, activeLicensedDeviceCount, DateTime.UtcNow);
            ShowLicenseStatus(
                installation,
                FormatLicenseStatus(evaluation),
                evaluation.ActiveLicensedDeviceCount,
                evaluation.LicensedDeviceCount);
            ShowLicensedDeviceStates(importedLicense);

            AppendLicenseMessage($"Lizenzdatei erfolgreich importiert: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Lizenzdatei konnte nicht importiert werden: {ex.Message}");
        }
    }

    private void UpdateGracePeriods_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var license = File.Exists(paths.LicenseFile)
                ? _licenseFileRepository.Load(paths.LicenseFile)
                : null;
            var existingStore = _licensedDeviceGracePeriodRepository.LoadOrEmpty(paths.DeviceGracePeriodsFile);
            var existingGracePeriodIds = existingStore.GracePeriods
                .Select(gracePeriod => gracePeriod.InterfaceProfileId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var states = _licensedDeviceStateEvaluator.Evaluate(
                _profileCatalog?.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>(),
                license,
                existingStore.GracePeriods,
                nowUtc);
            var updatedStore = _licensedDeviceGracePeriodService.EnsureGracePeriodsForUncoveredDevices(
                states,
                existingStore,
                nowUtc,
                graceDays: 30);
            var newGracePeriodCount = updatedStore.GracePeriods.Count(gracePeriod => !existingGracePeriodIds.Contains(gracePeriod.InterfaceProfileId));

            var validationIssues = updatedStore.Validate();
            if (validationIssues.Count > 0)
            {
                AppendLicenseMessage("Karenzzeiten wurden nicht gespeichert, weil die Daten ungueltig sind.");
                foreach (var issue in validationIssues)
                {
                    AppendLicenseMessage($"[Karenzzeit] {issue}");
                }

                return;
            }

            _licensedDeviceGracePeriodRepository.Save(paths.DeviceGracePeriodsFile, updatedStore);
            ShowLicensedDeviceStates(license, updatedStore);
            if (newGracePeriodCount == 0)
            {
                AppendLicenseMessageOnce("Keine neuen Karenzzeiten erforderlich.");
                return;
            }

            AppendLicenseMessage($"Karenzzeiten aktualisiert: {newGracePeriodCount} neue Karenzzeit(en) angelegt.");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Karenzzeiten konnten nicht aktualisiert werden: {ex.Message}");
        }
    }

    private void ExportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            AppendProfileMessage("Templatepaket kann nicht exportiert werden, weil keine V2-Profile geladen sind.");
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
            AppendProfileMessage($"Templatepaket erfolgreich exportiert: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Templatepaket konnte nicht exportiert werden: {ex.Message}");
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
            AppendProfileMessage($"Templatepaket konnte nicht importiert oder geprüft werden: {ex.Message}");
        }
    }

    private void ProfileAssistantPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        const string message = "Funktion noch nicht implementiert. Dieser Bereich ist für den späteren Profil-Assistenten vorgesehen.";
        System.Windows.MessageBox.Show(message, "Profil-Assistent", MessageBoxButton.OK, MessageBoxImage.Information);
        AppendProfileMessage(message);
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

        AppendProfileMessage($"Templatepaket importiert: {importResult.Package.Metadata.Name}");
        AppendProfileMessage($"AIS-Profile: {importResult.AisProfiles.Count}");
        AppendProfileMessage($"Geräteprofile: {importResult.DeviceProfiles.Count}");
        AppendProfileMessage($"Exportprofile: {importResult.ExportProfiles.Count}");
        AppendProfileMessage($"Schnittstellenprofile: {importResult.InterfaceProfiles.Count}");
        AppendProfileMessage($"Warnings: {warningIssues.Count}");
        AppendProfileMessage($"Errors: {errorIssues.Count}");

        if (errorIssues.Count > 0)
        {
            AppendProfileMessage("Templatepaket enthält Fehler und wurde nicht übernommen.");
            foreach (var issue in errorIssues)
            {
                AppendProfileMessage($"[Templatepaket] Error: {FormatTemplatePackageImportIssue(issue)}");
            }

            return;
        }

        if (warningIssues.Count > 0)
        {
            AppendProfileMessage("Templatepaket ist grundsätzlich gültig, enthält aber Hinweise.");
            foreach (var issue in warningIssues)
            {
                AppendProfileMessage($"[Templatepaket] Warning: {FormatTemplatePackageImportIssue(issue)}");
            }
        }

        AppendProfileMessage("Templatepaket wurde geprüft. Es wurde noch nicht produktiv übernommen.");
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
        UpdatePlaceholderTables();
        ShowExportRulePreviewForSelectedRule();
        ShowFullExportPreviewForSelectedProfile();

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
        AppendText(MessagesTextBox, message);
    }

    private void AppendProfileMessage(string message)
    {
        AppendText(ProfileMessagesTextBox, message);
        AppendMessage(message);
    }

    private void AppendLicenseMessage(string message)
    {
        AppendText(LicenseMessagesTextBox, message);
        AppendMessage(message);
    }

    private void AppendLicenseMessageOnce(string message)
    {
        var currentText = LicenseMessagesTextBox.Text.TrimEnd();
        if (currentText.EndsWith(message, StringComparison.Ordinal))
        {
            return;
        }

        AppendLicenseMessage(message);
    }

    private static void AppendText(System.Windows.Controls.TextBox textBox, string message)
    {
        if (!string.IsNullOrWhiteSpace(textBox.Text))
        {
            textBox.AppendText(Environment.NewLine);
        }

        textBox.AppendText(message);
        textBox.ScrollToEnd();
    }

    private sealed class ScannedImportPairRow
    {
        public ScannedImportPairRow(
            string InterfaceProfileId,
            string InterfaceProfileName,
            string ExportProfileId,
            string ExportProfileName,
            string AisFilePath,
            string DeviceFilePath,
            string ExportFolder,
            string Status)
        {
            this.InterfaceProfileId = InterfaceProfileId;
            this.InterfaceProfileName = InterfaceProfileName;
            this.ExportProfileId = ExportProfileId;
            this.ExportProfileName = ExportProfileName;
            this.AisFilePath = AisFilePath;
            this.DeviceFilePath = DeviceFilePath;
            this.ExportFolder = ExportFolder;
            this.Status = Status;
        }

        public string InterfaceProfileId { get; }

        public string InterfaceProfileName { get; }

        public string ExportProfileId { get; }

        public string ExportProfileName { get; }

        public string AisFilePath { get; }

        public string DeviceFilePath { get; }

        public string ExportFolder { get; }

        public string Status { get; set; }
    }

    private sealed class PlaceholderRow : INotifyPropertyChanged
    {
        public const string OutputModeAis = "AIS";
        public const string OutputModeHuman = "Mensch";

        private static readonly IReadOnlyList<string> AvailableOutputModes =
        [
            OutputModeAis,
            OutputModeHuman
        ];

        private bool _isUsed;
        private string _outputMode = OutputModeAis;

        public PlaceholderRow(string placeholder, string displayName, string value, int sortOrder)
        {
            Placeholder = placeholder;
            DisplayName = displayName;
            Value = value;
            SortOrder = sortOrder;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Placeholder { get; }

        public string DisplayName { get; }

        public string Value { get; }

        public int SortOrder { get; }

        public bool HasValue => !string.IsNullOrWhiteSpace(Value);

        public IReadOnlyList<string> OutputModes => AvailableOutputModes;

        public string OutputMode
        {
            get => _outputMode;
            set
            {
                var nextValue = string.Equals(value, OutputModeHuman, StringComparison.OrdinalIgnoreCase)
                    ? OutputModeHuman
                    : OutputModeAis;

                if (string.Equals(_outputMode, nextValue, StringComparison.Ordinal))
                {
                    return;
                }

                _outputMode = nextValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputMode)));
            }
        }

        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                if (_isUsed == value)
                {
                    return;
                }

                _isUsed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUsed)));
            }
        }
    }
}
