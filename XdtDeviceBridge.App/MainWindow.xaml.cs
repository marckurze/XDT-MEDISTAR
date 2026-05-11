using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;
using WinForms = System.Windows.Forms;

namespace XdtDeviceBridge.App;

public partial class MainWindow : Window
{
    private static readonly HashSet<string> SupportedBuilderAttachmentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".tif",
        ".tiff",
        ".dcm",
        ".txt"
    };

    private static readonly DependencyProperty RadarAnimationKeyProperty = DependencyProperty.RegisterAttached(
        "RadarAnimationKey",
        typeof(string),
        typeof(MainWindow),
        new PropertyMetadata(""));

    private readonly ProcessingPipelineService _pipelineService = new();
    private readonly ExportFileNameBuilder _fileNameBuilder = new();
    private readonly FileExportService _fileExportService = new();
    private readonly AppDataPathProvider _appDataPathProvider = new();
    private readonly ProfileCatalogService _profileCatalogService = new();
    private readonly TemplatePackageExporter _templatePackageExporter = new();
    private readonly TemplatePackageImporter _templatePackageImporter = new();
    private readonly TemplatePackageImportValidator _templatePackageImportValidator = new();
    private readonly TemplatePackageImportConflictAnalyzer _templatePackageImportConflictAnalyzer = new();
    private readonly TemplatePackageImportPlanBuilder _templatePackageImportPlanBuilder = new();
    private readonly TemplatePackageImportDryRunService _templatePackageImportDryRunService = new();
    private readonly TemplatePackageImportPreviewDisplayService _templatePackageImportPreviewDisplayService = new();
    private readonly TemplatePackageImportExecutor _templatePackageImportExecutor = new();
    private readonly TemplatePackageImportSelectionService _templatePackageImportSelectionService = new();
    private readonly InstallationInfoProvider _installationInfoProvider = new();
    private readonly LicenseFileRepository _licenseFileRepository = new();
    private readonly LicensedDeviceGracePeriodRepository _licensedDeviceGracePeriodRepository = new();
    private readonly LicenseEvaluator _licenseEvaluator = new();
    private readonly LicensedDeviceStateEvaluator _licensedDeviceStateEvaluator = new();
    private readonly LicensedDeviceGracePeriodService _licensedDeviceGracePeriodService = new();
    private readonly ActiveInterfaceProfileStatusService _activeInterfaceProfileStatusService = new();
    private readonly InterfaceMonitoringEventDeduplicationService _monitoringEventDeduplicationService = new();
    private readonly AutoImportScannerService _autoImportScannerService = new();
    private readonly PeriodicAutoImportScanService _periodicAutoImportScanService = new();
    private readonly InterfaceProfileManualProcessor _interfaceProfileManualProcessor = new();
    private readonly AutoImportPairProcessingCoordinator _autoImportPairProcessingCoordinator = new();
    private readonly AutoImportPackageStateService _autoImportPackageStateService = new();
    private readonly InterfaceMonitoringCardStatusService _interfaceMonitoringCardStatusService = new();
    private readonly AttachmentExternalLinkDiagnosticService _attachmentExternalLinkDiagnosticService = new();
    private readonly AttachmentImportFolderDiagnosticService _attachmentImportFolderDiagnosticService = new();
    private readonly BuilderTestExportService _builderTestExportService = new();
    private readonly AttachmentFileNameBuilder _attachmentFileNameBuilder = new();
    private readonly ExternalAisLinkFieldBuilder _externalAisLinkFieldBuilder = new();
    private readonly ExternalAisLinkXdtFieldAdapter _externalAisLinkXdtFieldAdapter = new();
    private readonly LicenseRequestBuilder _licenseRequestBuilder = new();
    private readonly LicenseRequestFileRepository _licenseRequestFileRepository = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();
    private readonly ExportProfileDraftService _exportProfileDraftService = new();
    private readonly InterfaceProfileConfigurationService _interfaceProfileConfigurationService = new();
    private readonly InterfaceProfileScanIntervalUpdateService _interfaceProfileScanIntervalUpdateService = new();
    private readonly InterfaceProfileActivationEvaluationService _interfaceProfileActivationEvaluationService = new();
    private readonly InterfaceProfileActivationPreviewDisplayService _interfaceProfileActivationPreviewDisplayService = new();
    private readonly InterfaceProfileActivationPreparationPreviewService _interfaceProfileActivationPreparationPreviewService = new();
    private readonly InterfaceProfileActivationGuardService _interfaceProfileActivationGuardService = new();
    private readonly ObservableCollection<PlaceholderRow> _aisPlaceholderRows = new();
    private readonly ObservableCollection<PlaceholderRow> _devicePlaceholderRows = new();
    private readonly ObservableCollection<ExportRuleDefinition> _visibleExportRules = new();
    private readonly ObservableCollection<LicenseDeviceStateRow> _licensedDeviceStateRows = new();
    private readonly ObservableCollection<ActiveInterfaceProfileStatusRow> _activeInterfaceProfileStatusRows = new();
    private readonly ObservableCollection<InterfaceMonitoringCardDisplay> _interfaceMonitoringCards = new();
    private readonly ObservableCollection<InterfaceProfileActivationFolderDisplay> _interfaceProfileActivationFolderRows = new();
    private readonly ObservableCollection<InterfaceProfileActivationAttachmentDisplay> _interfaceProfileActivationAttachmentRows = new();
    private readonly ObservableCollection<InterfaceProfileActivationPreviewRow> _interfaceProfileActivationPreviewRows = new();
    private readonly ObservableCollection<AttachmentImportCandidateDisplayRow> _attachmentImportCandidateRows = new();
    private readonly List<ExportRuleDefinition> _temporaryExportRules = new();
    private readonly Dictionary<string, InterfaceMonitoringRuntimeState> _interfaceMonitoringRuntimeStates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InterfaceMonitoringCardDisplay> _interfaceMonitoringRuntimeCards = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _monitoringDetailsExpandedByProfileId = new(StringComparer.OrdinalIgnoreCase);

    private ProcessingPipelineResult? _lastPipelineResult;
    private DeviceProfile _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
    private ProfileCatalog? _profileCatalog;
    private InstallationInfo? _installationInfo;
    private string? _plannedFileName;
    private bool _updatingPlaceholderRows;
    private bool _updatingAttachmentDiagnosticProfileSelection;
    private int _draftRuleSequence;
    private CancellationTokenSource? _periodicScanCancellationTokenSource;
    private Task? _periodicScanTask;
    private bool _refreshingInterfaceMonitoringCards;
    private IReadOnlyList<ExportFieldRecord> _builderTransientAttachmentFields = Array.Empty<ExportFieldRecord>();
    private AttachmentImportCandidateDisplayRow? _builderSelectedAttachmentCandidate;
    private string? _builderPreviewAttachmentTargetPath;
    private string? _builderPreviewAttachmentTargetFileName;
    private TemplatePackageImportResult? _lastTemplatePackageImportResult;
    private TemplatePackageImportValidationResult? _lastTemplatePackageImportValidationResult;
    private TemplatePackageImportAnalysisResult? _lastTemplatePackageImportAnalysisResult;
    private TemplatePackageImportPlan? _lastTemplatePackageImportBasePlan;
    private TemplatePackageImportPlan? _lastTemplatePackageImportPlan;
    private TemplatePackageImportDryRunResult? _lastTemplatePackageImportDryRunResult;
    private bool _updatingTemplatePackageImportPreview;

    public MainWindow()
    {
        InitializeComponent();
        DraftRuleTypeComboBox.ItemsSource = Enum.GetValues<ExportRuleType>();
        AisPlaceholdersGrid.ItemsSource = _aisPlaceholderRows;
        DevicePlaceholdersGrid.ItemsSource = _devicePlaceholderRows;
        ExportRulesGrid.ItemsSource = _visibleExportRules;
        LicensedDeviceStatesGrid.ItemsSource = _licensedDeviceStateRows;
        InterfaceMonitoringCardsItemsControl.ItemsSource = _interfaceMonitoringCards;
        InterfaceActivationPreviewFolderChecksGrid.ItemsSource = _interfaceProfileActivationFolderRows;
        InterfaceActivationPreviewAttachmentChecksGrid.ItemsSource = _interfaceProfileActivationAttachmentRows;
        InterfaceActivationPreviewChecksGrid.ItemsSource = _interfaceProfileActivationPreviewRows;
        BuilderAttachmentDiagnosticCandidatesGrid.ItemsSource = _attachmentImportCandidateRows;
        SyncBuilderTestPreviewArea();
        InitializeProfileOverview();
        InitializeLicenseOverview();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        StopPeriodicScan(updateUi: false);
        base.OnClosing(e);
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
            InitializeAttachmentDiagnosticProfiles(catalog);
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
            BuilderAttachmentDiagnosticInterfaceProfileComboBox.ItemsSource = null;
            SetAttachmentDiagnosticResultText("Keine Profile geladen.");
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
            return "Noch keine Beispielwerte geladen. Bitte zuerst AIS-Datei und Gerätedatei laden.";
        }

        var result = _lastPipelineResult;
        var patient = result.Patient ?? CreateEmptyPatientData();
        var effectiveRules = GetEffectiveExportRules(exportProfile, draftRule, replaceRuleId);
        var mappingRules = effectiveRules
            .Select(rule => CreatePreviewMappingRule(rule, result))
            .ToList();
        var mappingResult = _mappingEngine.Map(patient, result.Measurements, mappingRules);
        var exportRecords = BuilderTestExportService.AppendTransientAttachmentFields(
            mappingResult.Records,
            _builderTransientAttachmentFields);
        var exportResult = _xdtExportBuilder.Build(exportRecords);
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
        builder.AppendLine(_builderTransientAttachmentFields.Count > 0
            ? "XDT-Anhang-Linkfelder: 6302-6305 werden nur transient für diese Baukasten-Vorschau ergänzt."
            : "XDT-Anhang-Linkfelder: Kein XDT-Anhang eingelesen. Vorschau enthält keine 6302-6305.");
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

    private void InitializeAttachmentDiagnosticProfiles(ProfileCatalog catalog, string? selectedInterfaceProfileId = null)
    {
        var activeProfiles = catalog.InterfaceProfiles
            .Where(profile => profile.IsActive)
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        BuilderAttachmentDiagnosticInterfaceProfileComboBox.ItemsSource = activeProfiles;
        if (activeProfiles.Count == 0)
        {
            BuilderAttachmentDiagnosticInterfaceProfileComboBox.SelectedIndex = -1;
            _attachmentImportCandidateRows.Clear();
            UpdateAttachmentDiagnosticProfileDisplay();
            SetAttachmentDiagnosticResultText("Keine aktiven Schnittstellenprofile für den XDT-Anhang-Test geladen.");
            return;
        }

        var selectedProfile = string.IsNullOrWhiteSpace(selectedInterfaceProfileId)
            ? null
            : activeProfiles.FirstOrDefault(profile => string.Equals(profile.Metadata.Id, selectedInterfaceProfileId, StringComparison.Ordinal));

        SetAttachmentDiagnosticSelectedProfile(selectedProfile ?? activeProfiles[0]);
        UpdateAttachmentDiagnosticProfileDisplay();
        if (BuilderAttachmentDiagnosticResultTextBox.Text == "Keine aktiven Schnittstellenprofile für den XDT-Anhang-Test geladen.")
        {
            SetAttachmentDiagnosticResultText("Noch kein XDT-Anhang vorbereitet.");
        }
    }

    private void AttachmentDiagnosticInterfaceProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_updatingAttachmentDiagnosticProfileSelection)
        {
            return;
        }

        SyncAttachmentDiagnosticProfileSelection(sender as System.Windows.Controls.ComboBox);
        _attachmentImportCandidateRows.Clear();
        ClearBuilderAttachmentPreviewState(updatePreview: true);
        UpdateAttachmentDiagnosticProfileDisplay();
    }

    private void UpdateAttachmentDiagnosticProfileDisplay()
    {
        if (GetSelectedAttachmentDiagnosticProfile() is not InterfaceProfileDefinition profile)
        {
            UpdateBuilderAttachmentProfileDetails(null);
            return;
        }

        UpdateBuilderAttachmentProfileDetails(profile);
        SelectExportProfileForInterfaceProfile(profile);
    }

    private InterfaceProfileDefinition? GetSelectedAttachmentDiagnosticProfile()
    {
        return BuilderAttachmentDiagnosticInterfaceProfileComboBox.SelectedItem as InterfaceProfileDefinition;
    }

    private void SetAttachmentDiagnosticSelectedProfile(InterfaceProfileDefinition profile)
    {
        _updatingAttachmentDiagnosticProfileSelection = true;
        try
        {
            BuilderAttachmentDiagnosticInterfaceProfileComboBox.SelectedItem = profile;
        }
        finally
        {
            _updatingAttachmentDiagnosticProfileSelection = false;
        }
    }

    private void SyncAttachmentDiagnosticProfileSelection(System.Windows.Controls.ComboBox? source)
    {
        if (source?.SelectedItem is not InterfaceProfileDefinition profile)
        {
            return;
        }

        SetAttachmentDiagnosticSelectedProfile(profile);
    }

    private void UpdateBuilderAttachmentProfileDetails(InterfaceProfileDefinition? profile)
    {
        if (profile is null)
        {
            BuilderAttachmentProfileDetailsTextBox.Text = "Bitte erst Schnittstellenprofil anlegen.";
            BuilderReadXdtAttachmentButton.IsEnabled = false;
            return;
        }

        var options = profile.FolderOptions;
        var aisProfileName = _profileCatalog?.AisProfiles
            .FirstOrDefault(aisProfile => string.Equals(aisProfile.Metadata.Id, profile.AisProfileId, StringComparison.Ordinal))?
            .Metadata.Name ?? profile.AisProfileId;
        var deviceProfileName = _profileCatalog?.DeviceProfiles
            .FirstOrDefault(deviceProfile => string.Equals(deviceProfile.Metadata.Id, profile.DeviceProfileId, StringComparison.Ordinal))?
            .Metadata.Name ?? profile.DeviceProfileId;
        var exportProfileName = _profileCatalog?.ExportProfiles
            .FirstOrDefault(exportProfile => string.Equals(exportProfile.Metadata.Id, profile.ExportProfileId, StringComparison.Ordinal))?
            .Metadata.Name ?? profile.ExportProfileId;

        var builder = new StringBuilder();
        AppendSummaryLine(builder, "gewähltes Schnittstellenprofil", profile.Metadata.Name);
        AppendSummaryLine(builder, "AIS-Profil", aisProfileName);
        AppendSummaryLine(builder, "Geräteprofil", deviceProfileName);
        AppendSummaryLine(builder, "Exportprofil", exportProfileName);
        AppendSummaryLine(builder, "XDT-Anhänge für AIS aktiv", options.IsAttachmentProcessingEnabled ? "Ja" : "Nein");
        AppendSummaryLine(builder, "XDT-Anhang ist", options.AttachmentRequirementMode == AttachmentRequirementMode.Required ? "Pflicht" : "optional");
        AppendSummaryLine(builder, "XDT-Anhang Importordner", options.AttachmentImportFolder);
        AppendSummaryLine(builder, "XDT-Anhang Exportordner", options.AttachmentExportFolder);
        AppendSummaryLine(builder, "XDT-Anhang Dateiname", options.AttachmentFileNameTemplate);
        AppendSummaryLine(builder, "6302 Dokumentenname", options.AttachmentExternalLinkDocumentName);
        AppendSummaryLine(builder, "6303 Dateiformat", options.AttachmentExternalLinkFileFormat);
        AppendSummaryLine(builder, "6304 Beschreibung", options.AttachmentExternalLinkDescription);
        AppendSummaryLine(builder, "6305 vollständiger Dateipfad", options.AttachmentExternalLinkPathTemplate);

        BuilderAttachmentProfileDetailsTextBox.Text = builder.ToString().TrimEnd();
        BuilderReadXdtAttachmentButton.IsEnabled = true;
    }

    private void SelectExportProfileForInterfaceProfile(InterfaceProfileDefinition profile)
    {
        if (_profileCatalog is null || ExportProfileComboBox.ItemsSource is null)
        {
            return;
        }

        var exportProfile = _profileCatalog.ExportProfiles.FirstOrDefault(candidate =>
            string.Equals(candidate.Metadata.Id, profile.ExportProfileId, StringComparison.Ordinal));
        if (exportProfile is not null && !ReferenceEquals(ExportProfileComboBox.SelectedItem, exportProfile))
        {
            ExportProfileComboBox.SelectedItem = exportProfile;
        }
    }

    private static void SetAttachmentDiagnosticProfileDisplayValue(
        System.Windows.Controls.TextBlock primary,
        string value)
    {
        primary.Text = value;
        primary.ToolTip = value == "-" ? null : value;
    }

    private void InterfaceProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ShowInterfaceProfileForSelectedProfile();
    }

    private void RefreshInterfaceActivationPreview_Click(object sender, RoutedEventArgs e)
    {
        RefreshInterfaceActivationPreview();
    }

    private void PrepareInterfaceActivationPreview_Click(object sender, RoutedEventArgs e)
    {
        InterfaceProfileActivationPreparationPreview preview;
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition profile)
        {
            preview = _interfaceProfileActivationPreparationPreviewService.CreateEmpty();
            System.Windows.MessageBox.Show(
                preview.MessageText,
                preview.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (_profileCatalog is null)
        {
            preview = _interfaceProfileActivationPreparationPreviewService.CreateError("Profilkatalog ist nicht geladen.");
            System.Windows.MessageBox.Show(
                preview.MessageText,
                preview.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = _interfaceProfileActivationEvaluationService.Evaluate(
                profile,
                _profileCatalog,
                CreateLicenseStatesForActivationPreview());
            var guardResult = _interfaceProfileActivationGuardService.ValidateActivationRequest(
                new InterfaceProfileActivationRequest(
                    profile,
                    result,
                    WarningsAccepted: false,
                    Context: "PreviewOnly"));
            preview = _interfaceProfileActivationPreparationPreviewService.Create(profile, result, guardResult);
            var image = guardResult.CanProceed
                ? MessageBoxImage.Information
                : MessageBoxImage.Warning;
            System.Windows.MessageBox.Show(
                preview.MessageText,
                preview.Title,
                MessageBoxButton.OK,
                image);
        }
        catch (Exception ex)
        {
            preview = _interfaceProfileActivationPreparationPreviewService.CreateError(ex.Message);
            System.Windows.MessageBox.Show(
                preview.MessageText,
                preview.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
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
        InterfaceAutoImportScanIntervalSecondsTextBox.Text = profile.FolderOptions.AutoImportScanIntervalSeconds.ToString();
        InterfaceDeviceFileWaitTimeoutMinutesTextBox.Text = profile.FolderOptions.DeviceFileWaitTimeoutMinutes.ToString();
        InterfaceAttachmentImportFolderTextBox.Text = profile.FolderOptions.AttachmentImportFolder;
        InterfaceAttachmentExportFolderTextBox.Text = profile.FolderOptions.AttachmentExportFolder;
        InterfaceAttachmentFileNameTemplateTextBox.Text = profile.FolderOptions.AttachmentFileNameTemplate ?? string.Empty;
        InterfaceAttachmentTransferModeComboBox.SelectedValue = profile.FolderOptions.AttachmentTransferMode.ToString();
        InterfaceAttachmentProcessingEnabledCheckBox.IsChecked = profile.FolderOptions.IsAttachmentProcessingEnabled;
        InterfaceAttachmentRequirementModeComboBox.SelectedValue = profile.FolderOptions.AttachmentRequirementMode.ToString();
        InterfaceAttachmentWaitTimeoutSecondsTextBox.Text = profile.FolderOptions.AttachmentWaitTimeoutSeconds.ToString();
        InterfaceAttachmentFileStabilityWaitSecondsTextBox.Text = profile.FolderOptions.AttachmentFileStabilityWaitSeconds.ToString();
        InterfaceAttachmentLinkDocumentNameTextBox.Text = profile.FolderOptions.AttachmentExternalLinkDocumentName;
        InterfaceAttachmentLinkFileFormatTextBox.Text = profile.FolderOptions.AttachmentExternalLinkFileFormat;
        InterfaceAttachmentLinkDescriptionTextBox.Text = profile.FolderOptions.AttachmentExternalLinkDescription;
        InterfaceAttachmentLinkPathTemplateTextBox.Text = profile.FolderOptions.AttachmentExternalLinkPathTemplate;

        InterfaceClearAisImportFolderCheckBox.IsChecked = profile.FolderOptions.ClearAisImportFolderBeforeProcessing;
        InterfaceClearDeviceImportFolderCheckBox.IsChecked = profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing;
        InterfaceArchiveProcessedFilesCheckBox.IsChecked = profile.FolderOptions.ArchiveProcessedFiles;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked = profile.FolderOptions.MoveFailedFilesToErrorFolder;
        InterfaceArchiveModeComboBox.SelectedValue = profile.FolderOptions.ArchiveProcessedFileMode.ToString();
        InterfaceArchiveRetentionDaysTextBox.Text = profile.FolderOptions.ArchiveRetentionDays?.ToString() ?? string.Empty;

        RefreshInterfaceActivationPreview();
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
        InterfaceAutoImportScanIntervalSecondsTextBox.Text = "5";
        InterfaceDeviceFileWaitTimeoutMinutesTextBox.Text = "10";
        InterfaceAttachmentImportFolderTextBox.Text = string.Empty;
        InterfaceAttachmentExportFolderTextBox.Text = string.Empty;
        InterfaceAttachmentFileNameTemplateTextBox.Text = string.Empty;
        InterfaceAttachmentTransferModeComboBox.SelectedValue = AttachmentTransferMode.Move.ToString();
        InterfaceAttachmentProcessingEnabledCheckBox.IsChecked = false;
        InterfaceAttachmentRequirementModeComboBox.SelectedValue = AttachmentRequirementMode.Optional.ToString();
        InterfaceAttachmentWaitTimeoutSecondsTextBox.Text = "30";
        InterfaceAttachmentFileStabilityWaitSecondsTextBox.Text = "2";
        InterfaceAttachmentLinkDocumentNameTextBox.Text = string.Empty;
        InterfaceAttachmentLinkFileFormatTextBox.Text = string.Empty;
        InterfaceAttachmentLinkDescriptionTextBox.Text = string.Empty;
        InterfaceAttachmentLinkPathTemplateTextBox.Text = string.Empty;
        InterfaceClearAisImportFolderCheckBox.IsChecked = false;
        InterfaceClearDeviceImportFolderCheckBox.IsChecked = false;
        InterfaceArchiveProcessedFilesCheckBox.IsChecked = false;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked = false;
        InterfaceArchiveModeComboBox.SelectedValue = ArchiveProcessedFileMode.Copy.ToString();
        InterfaceArchiveRetentionDaysTextBox.Text = string.Empty;
        ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateEmpty());
    }

    private void RefreshInterfaceActivationPreview()
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition profile)
        {
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateEmpty());
            return;
        }

        if (_profileCatalog is null)
        {
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateError("Profilkatalog ist nicht geladen."));
            return;
        }

        try
        {
            var result = _interfaceProfileActivationEvaluationService.Evaluate(
                profile,
                _profileCatalog,
                CreateLicenseStatesForActivationPreview());
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.Create(profile, result));
        }
        catch (Exception ex)
        {
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateError(ex.Message));
        }
    }

    private IReadOnlyList<LicensedDeviceState> CreateLicenseStatesForActivationPreview()
    {
        if (_profileCatalog is null)
        {
            return Array.Empty<LicensedDeviceState>();
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var license = File.Exists(paths.LicenseFile)
                ? _licenseFileRepository.Load(paths.LicenseFile)
                : null;
            var gracePeriodStore = _licensedDeviceGracePeriodRepository.LoadOrEmpty(paths.DeviceGracePeriodsFile);

            return _licensedDeviceStateEvaluator.Evaluate(
                _profileCatalog.InterfaceProfiles,
                license,
                gracePeriodStore.GracePeriods,
                DateTime.UtcNow);
        }
        catch
        {
            return Array.Empty<LicensedDeviceState>();
        }
    }

    private void ShowInterfaceActivationPreview(InterfaceProfileActivationPreviewDisplay display)
    {
        InterfaceActivationPreviewStatusText.Text = display.StatusText;
        InterfaceActivationPreviewCanActivateText.Text = display.CanActivateText;
        InterfaceActivationPreviewCountsText.Text = display.SummaryText;
        InterfaceActivationPreviewHintText.Text = display.HintText;

        _interfaceProfileActivationFolderRows.Clear();
        foreach (var row in display.FolderChecks)
        {
            _interfaceProfileActivationFolderRows.Add(row);
        }

        _interfaceProfileActivationAttachmentRows.Clear();
        foreach (var row in display.AttachmentChecks)
        {
            _interfaceProfileActivationAttachmentRows.Add(row);
        }

        _interfaceProfileActivationPreviewRows.Clear();
        foreach (var row in display.Rows)
        {
            _interfaceProfileActivationPreviewRows.Add(row);
        }
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
        InterfaceFolderOptions folderOptions;
        try
        {
            folderOptions = CreateInterfaceFolderOptionsFromEditor();
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            AppendProfileMessage($"Schnittstellenprofil wurde nicht gespeichert: {ex.Message}");
            return;
        }

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
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: InterfaceArchiveProcessedFilesCheckBox.IsChecked == true,
            MoveFailedFilesToErrorFolder: InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked == true,
            ArchiveProcessedFileMode: ReadArchiveProcessedFileModeFromEditor(),
            ArchiveRetentionDays: ReadArchiveRetentionDaysFromEditor(),
            AutoImportScanIntervalSeconds: ReadAutoImportScanIntervalSecondsFromEditor(),
            DeviceFileWaitTimeoutMinutes: ReadDeviceFileWaitTimeoutMinutesFromEditor(),
            AttachmentImportFolder: InterfaceAttachmentImportFolderTextBox.Text.Trim(),
            AttachmentExportFolder: InterfaceAttachmentExportFolderTextBox.Text.Trim(),
            AttachmentFileNameTemplate: InterfaceAttachmentFileNameTemplateTextBox.Text.Trim(),
            AttachmentTransferMode: ReadAttachmentTransferModeFromEditor(),
            AttachmentExternalLinkDocumentName: InterfaceAttachmentLinkDocumentNameTextBox.Text.Trim(),
            AttachmentExternalLinkFileFormat: InterfaceAttachmentLinkFileFormatTextBox.Text.Trim(),
            AttachmentExternalLinkDescription: InterfaceAttachmentLinkDescriptionTextBox.Text.Trim(),
            AttachmentExternalLinkPathTemplate: InterfaceAttachmentLinkPathTemplateTextBox.Text.Trim(),
            IsAttachmentProcessingEnabled: InterfaceAttachmentProcessingEnabledCheckBox.IsChecked == true,
            AttachmentRequirementMode: ReadAttachmentRequirementModeFromEditor(),
            AttachmentWaitTimeoutSeconds: ReadAttachmentWaitTimeoutSecondsFromEditor(),
            AttachmentFileStabilityWaitSeconds: ReadAttachmentFileStabilityWaitSecondsFromEditor());
    }

    private AttachmentTransferMode ReadAttachmentTransferModeFromEditor()
    {
        return string.Equals(InterfaceAttachmentTransferModeComboBox.SelectedValue as string, AttachmentTransferMode.Move.ToString(), StringComparison.Ordinal)
            ? AttachmentTransferMode.Move
            : AttachmentTransferMode.Copy;
    }

    private AttachmentRequirementMode ReadAttachmentRequirementModeFromEditor()
    {
        return string.Equals(InterfaceAttachmentRequirementModeComboBox.SelectedValue as string, AttachmentRequirementMode.Required.ToString(), StringComparison.Ordinal)
            ? AttachmentRequirementMode.Required
            : AttachmentRequirementMode.Optional;
    }

    private int ReadAttachmentWaitTimeoutSecondsFromEditor()
    {
        var rawValue = InterfaceAttachmentWaitTimeoutSecondsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 30;
        }

        if (!int.TryParse(rawValue, out var timeoutSeconds))
        {
            throw new FormatException("Wartezeit auf XDT-Anhang muss eine ganze Zahl in Sekunden sein.");
        }

        if (timeoutSeconds < 0)
        {
            throw new ArgumentException("Wartezeit auf XDT-Anhang darf nicht negativ sein.");
        }

        return timeoutSeconds;
    }

    private int ReadAttachmentFileStabilityWaitSecondsFromEditor()
    {
        var rawValue = InterfaceAttachmentFileStabilityWaitSecondsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 2;
        }

        if (!int.TryParse(rawValue, out var stabilitySeconds))
        {
            throw new FormatException("Dateistabilität für XDT-Anhänge muss eine ganze Zahl in Sekunden sein.");
        }

        if (stabilitySeconds < 0)
        {
            throw new ArgumentException("Dateistabilität für XDT-Anhänge darf nicht negativ sein.");
        }

        return stabilitySeconds;
    }

    private int ReadAutoImportScanIntervalSecondsFromEditor()
    {
        var rawValue = InterfaceAutoImportScanIntervalSecondsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return PeriodicAutoImportScanService.DefaultScanIntervalSeconds;
        }

        if (!int.TryParse(rawValue, out var intervalSeconds))
        {
            throw new FormatException("Ordnerabfrage-Intervall muss eine ganze Zahl in Sekunden sein.");
        }

        if (intervalSeconds < PeriodicAutoImportScanService.MinimumScanIntervalSeconds)
        {
            throw new ArgumentException("Ordnerabfrage-Intervall muss mindestens 1 Sekunde betragen.");
        }

        return intervalSeconds;
    }

    private int ReadDeviceFileWaitTimeoutMinutesFromEditor()
    {
        var rawValue = InterfaceDeviceFileWaitTimeoutMinutesTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 10;
        }

        if (!int.TryParse(rawValue, out var timeoutMinutes))
        {
            throw new FormatException("Wartezeit auf Gerätedatei muss eine ganze Zahl in Minuten sein.");
        }

        if (timeoutMinutes < 0)
        {
            throw new ArgumentException("Wartezeit auf Gerätedatei darf nicht negativ sein.");
        }

        return timeoutMinutes;
    }

    private ArchiveProcessedFileMode ReadArchiveProcessedFileModeFromEditor()
    {
        return string.Equals(InterfaceArchiveModeComboBox.SelectedValue as string, ArchiveProcessedFileMode.Move.ToString(), StringComparison.Ordinal)
            ? ArchiveProcessedFileMode.Move
            : ArchiveProcessedFileMode.Copy;
    }

    private int? ReadArchiveRetentionDaysFromEditor()
    {
        var rawValue = InterfaceArchiveRetentionDaysTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (!int.TryParse(rawValue, out var retentionDays))
        {
            throw new FormatException("Archiv-Aufbewahrungsfrist muss leer, 0 oder eine ganze Zahl sein.");
        }

        return retentionDays == 0 ? null : retentionDays;
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
            "AttachmentImport" => InterfaceAttachmentImportFolderTextBox,
            "AttachmentExport" => InterfaceAttachmentExportFolderTextBox,
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
        InitializeAttachmentDiagnosticProfiles(catalog, selectedInterfaceProfileId);
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

        RefreshInterfaceMonitoringCards();

        ActiveInterfaceProfilesStatusText.Text = rows.Count == 0
            ? "Keine aktiven Schnittstellenprofile konfiguriert."
            : $"{rows.Count} aktive Schnittstellenprofil(e) für die spätere automatische Verarbeitung konfiguriert.";
    }

    private void ClearActiveInterfaceProfilesOverview(string message)
    {
        _activeInterfaceProfileStatusRows.Clear();
        _interfaceMonitoringCards.Clear();
        ActiveInterfaceProfilesStatusText.Text = message;
    }

    private void RefreshInterfaceMonitoringCards()
    {
        _refreshingInterfaceMonitoringCards = true;
        try
        {
            var isMonitoringActive = _periodicScanCancellationTokenSource is not null;
            var desiredCards = new List<InterfaceMonitoringCardDisplay>(_activeInterfaceProfileStatusRows.Count);
            foreach (var row in _activeInterfaceProfileStatusRows)
            {
                var runtimeState = GetMonitoringRuntimeState(row.MonitoringCard.InterfaceProfileId);
                var runtimeCard = GetRuntimeMonitoringCard(row);
                desiredCards.Add(runtimeCard with
                {
                    CurrentStatus = runtimeState.CurrentStatus,
                    StatusClass = runtimeState.StatusClass,
                    LastScanText = runtimeState.LastScanText,
                    IsScanAnimationActive = isMonitoringActive,
                    AutomaticProcessingText = EnableAutomaticPairProcessingCheckBox.IsChecked == true ? "Ja" : "Nein",
                    IsDetailsExpanded = GetMonitoringDetailsExpanded(row.MonitoringCard.InterfaceProfileId)
                });
            }

            SynchronizeMonitoringCards(desiredCards);
        }
        finally
        {
            _refreshingInterfaceMonitoringCards = false;
        }
    }

    private void SynchronizeMonitoringCards(IReadOnlyList<InterfaceMonitoringCardDisplay> desiredCards)
    {
        for (var index = _interfaceMonitoringCards.Count - 1; index >= 0; index--)
        {
            var card = _interfaceMonitoringCards[index];
            if (!desiredCards.Any(desired => string.Equals(desired.InterfaceProfileId, card.InterfaceProfileId, StringComparison.Ordinal)))
            {
                _interfaceMonitoringCards.RemoveAt(index);
            }
        }

        for (var desiredIndex = 0; desiredIndex < desiredCards.Count; desiredIndex++)
        {
            var desiredCard = desiredCards[desiredIndex];
            var currentIndex = IndexOfMonitoringCard(desiredCard.InterfaceProfileId);
            if (currentIndex < 0)
            {
                _interfaceMonitoringCards.Insert(Math.Min(desiredIndex, _interfaceMonitoringCards.Count), desiredCard);
                continue;
            }

            if (currentIndex != desiredIndex)
            {
                _interfaceMonitoringCards.Move(currentIndex, desiredIndex);
            }

            if (!Equals(_interfaceMonitoringCards[desiredIndex], desiredCard))
            {
                _interfaceMonitoringCards[desiredIndex] = desiredCard;
            }
        }
    }

    private int IndexOfMonitoringCard(string interfaceProfileId)
    {
        for (var index = 0; index < _interfaceMonitoringCards.Count; index++)
        {
            if (string.Equals(_interfaceMonitoringCards[index].InterfaceProfileId, interfaceProfileId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private bool GetMonitoringDetailsExpanded(string interfaceProfileId)
    {
        return _monitoringDetailsExpandedByProfileId.TryGetValue(interfaceProfileId, out var isExpanded)
            && isExpanded;
    }

    private void MonitoringDetailsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        SetMonitoringDetailsExpanded(sender, isExpanded: true);
    }

    private void MonitoringDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        SetMonitoringDetailsExpanded(sender, isExpanded: false);
    }

    private void SetMonitoringDetailsExpanded(object sender, bool isExpanded)
    {
        if (_refreshingInterfaceMonitoringCards)
        {
            return;
        }

        if (sender is not FrameworkElement element
            || element.DataContext is not InterfaceMonitoringCardDisplay card)
        {
            return;
        }

        _monitoringDetailsExpandedByProfileId[card.InterfaceProfileId] = isExpanded;
        if (_interfaceMonitoringRuntimeCards.TryGetValue(card.InterfaceProfileId, out var runtimeCard))
        {
            _interfaceMonitoringRuntimeCards[card.InterfaceProfileId] = runtimeCard with
            {
                IsDetailsExpanded = isExpanded
            };
        }
    }

    private InterfaceMonitoringRuntimeState GetMonitoringRuntimeState(string interfaceProfileId)
    {
        if (_interfaceMonitoringRuntimeCards.TryGetValue(interfaceProfileId, out var card))
        {
            return new InterfaceMonitoringRuntimeState(
                card.CurrentStatus,
                card.StatusClass,
                card.LastScanText);
        }

        return _interfaceMonitoringRuntimeStates.TryGetValue(interfaceProfileId, out var state)
            ? state
            : new InterfaceMonitoringRuntimeState("Gestoppt", "Neutral", "-");
    }

    private InterfaceMonitoringCardDisplay GetRuntimeMonitoringCard(ActiveInterfaceProfileStatusRow row)
    {
        if (!_interfaceMonitoringRuntimeCards.TryGetValue(row.MonitoringCard.InterfaceProfileId, out var runtimeCard))
        {
            return row.MonitoringCard;
        }

        return row.MonitoringCard with
        {
            CurrentStatus = runtimeCard.CurrentStatus,
            StatusClass = runtimeCard.StatusClass,
            LastScanText = runtimeCard.LastScanText,
            IsScanAnimationActive = runtimeCard.IsScanAnimationActive,
            AutomaticProcessingText = runtimeCard.AutomaticProcessingText,
            PatientDisplayText = runtimeCard.PatientDisplayText,
            AisFileName = runtimeCard.AisFileName,
            DeviceFileName = runtimeCard.DeviceFileName,
            AttachmentFileName = runtimeCard.AttachmentFileName,
            ExportFileName = runtimeCard.ExportFileName,
            LastSuccessfulExportText = runtimeCard.LastSuccessfulExportText,
            LastMessage = runtimeCard.LastMessage,
            ExpectedInputs = runtimeCard.ExpectedInputs
        };
    }

    private InterfaceMonitoringCardDisplay GetRuntimeMonitoringCard(InterfaceProfileDefinition profile)
    {
        var row = _activeInterfaceProfileStatusRows.FirstOrDefault(currentRow =>
            string.Equals(currentRow.MonitoringCard.InterfaceProfileId, profile.Metadata.Id, StringComparison.Ordinal));
        if (row is not null)
        {
            return GetRuntimeMonitoringCard(row);
        }

        return _interfaceMonitoringRuntimeCards.TryGetValue(profile.Metadata.Id, out var card)
            ? card
            : new InterfaceMonitoringCardDisplay(
                InterfaceProfileId: profile.Metadata.Id,
                InterfaceProfileName: profile.Metadata.Name,
                AisName: profile.AisProfileId,
                DeviceName: profile.DeviceProfileId,
                ExportProfileName: profile.ExportProfileId,
                CurrentStatus: "Gestoppt",
                StatusClass: "Neutral",
                ScanIntervalSeconds: Math.Max(1, profile.FolderOptions.AutoImportScanIntervalSeconds),
                ScanIntervalText: $"{Math.Max(1, profile.FolderOptions.AutoImportScanIntervalSeconds)} s",
                IsScanAnimationActive: false,
                LastScanText: "-",
                AutomaticProcessingText: "Nein",
                PatientDisplayText: "",
                AisFileName: "",
                DeviceFileName: "",
                AttachmentFileName: "",
                ExportFileName: "",
                LastSuccessfulExportText: "",
                LastMessage: "",
                ExpectedInputs: Array.Empty<ExpectedInputDisplayItem>(),
                FolderDetails: Array.Empty<InterfaceMonitoringDetailItem>(),
                AttachmentImportFolder: "",
                AttachmentExportFolder: "",
                AttachmentConfigurationStatus: "kein Anhang konfiguriert",
                IsDetailsExpanded: GetMonitoringDetailsExpanded(profile.Metadata.Id));
    }

    private void SetAllMonitoringRuntimeStates(string currentStatus, string statusClass)
    {
        foreach (var row in _activeInterfaceProfileStatusRows)
        {
            var previousState = GetMonitoringRuntimeState(row.MonitoringCard.InterfaceProfileId);
            _interfaceMonitoringRuntimeStates[row.MonitoringCard.InterfaceProfileId] = previousState with
            {
                CurrentStatus = currentStatus,
                StatusClass = statusClass
            };
            _interfaceMonitoringRuntimeCards[row.MonitoringCard.InterfaceProfileId] = GetRuntimeMonitoringCard(row) with
            {
                CurrentStatus = currentStatus,
                StatusClass = statusClass
            };
        }
    }

    private void SetMonitoringRuntimeState(
        string interfaceProfileId,
        string currentStatus,
        string statusClass,
        string? lastScanText = null)
    {
        var previousState = GetMonitoringRuntimeState(interfaceProfileId);
        _interfaceMonitoringRuntimeStates[interfaceProfileId] = previousState with
        {
            CurrentStatus = currentStatus,
            StatusClass = statusClass,
            LastScanText = string.IsNullOrWhiteSpace(lastScanText) ? previousState.LastScanText : lastScanText
        };

        if (_interfaceMonitoringRuntimeCards.TryGetValue(interfaceProfileId, out var card))
        {
            _interfaceMonitoringRuntimeCards[interfaceProfileId] = card with
            {
                CurrentStatus = currentStatus,
                StatusClass = statusClass,
                LastScanText = string.IsNullOrWhiteSpace(lastScanText) ? card.LastScanText : lastScanText
            };
        }
    }

    private void StartPeriodicScan_Click(object sender, RoutedEventArgs e)
    {
        if (_periodicScanCancellationTokenSource is not null)
        {
            AppendMessage("Überwachung läuft bereits.");
            return;
        }

        if (_profileCatalog is null)
        {
            AppendMonitoringEvent("monitoring", "monitoring-start-error", "Überwachung kann nicht gestartet werden: keine Profile geladen.", InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        var activeProfiles = _profileCatalog.InterfaceProfiles
            .Where(profile => profile.IsActive)
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (activeProfiles.Count == 0)
        {
            AppendMonitoringEvent("monitoring", "monitoring-start-error", "Überwachung kann nicht gestartet werden: keine aktiven Schnittstellenprofile vorhanden.", InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        _periodicScanCancellationTokenSource = new CancellationTokenSource();
        var token = _periodicScanCancellationTokenSource.Token;
        StartPeriodicScanButton.IsEnabled = false;
        StopPeriodicScanButton.IsEnabled = true;
        ActiveProfileScanButton.IsEnabled = false;
        PeriodicScanStatusText.Text = "Läuft";
        PeriodicScanLastRunText.Text = "-";
        PeriodicScanReadyPairsText.Text = "0";
        var scanInterval = PeriodicAutoImportScanService.GetEffectiveInterval(activeProfiles);
        PeriodicScanIntervalText.Text = $"{scanInterval.TotalSeconds:0} Sekunden";
        _monitoringEventDeduplicationService.Reset();
        SetAllMonitoringRuntimeStates("Scannt", "Active");
        RefreshInterfaceMonitoringCards();
        AppendMonitoringEvent("monitoring", "monitoring-state", "Überwachung gestartet.");

        _periodicScanTask = Task.Run(() => _periodicAutoImportScanService.StartAsync(
            activeProfiles,
            scanInterval,
            TimeSpan.FromMilliseconds(200),
            result => Dispatcher.Invoke(() => ShowPeriodicScanResult(result)),
            token), token);

        _periodicScanTask.ContinueWith(task =>
        {
            if (task.Exception is null)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                PeriodicScanStatusText.Text = "Gestoppt";
                AppendMonitoringEvent(
                    "monitoring",
                    "monitoring-task-error",
                    $"Überwachung wurde mit Fehler beendet: {task.Exception.GetBaseException().Message}",
                    InterfaceMonitoringEventSeverity.Error);
                StopPeriodicScan(updateUi: true);
            });
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    private void StopPeriodicScan_Click(object sender, RoutedEventArgs e)
    {
        StopPeriodicScan(updateUi: true);
    }

    private void EnableAutomaticPairProcessingCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        RefreshInterfaceMonitoringCards();
    }

    private void DecreaseMonitoringScanInterval_Click(object sender, RoutedEventArgs e)
    {
        ChangeMonitoringScanInterval(sender, -1);
    }

    private void IncreaseMonitoringScanInterval_Click(object sender, RoutedEventArgs e)
    {
        ChangeMonitoringScanInterval(sender, 1);
    }

    private void ChangeMonitoringScanInterval(object sender, int deltaSeconds)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not InterfaceMonitoringCardDisplay card)
        {
            return;
        }

        if (_profileCatalog is null)
        {
            AppendMonitoringEvent(
                "monitoring",
                "scan-interval-change-error",
                "Scanintervall kann nicht geändert werden, weil keine Profile geladen sind.",
                InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        var profile = _profileCatalog.InterfaceProfiles.FirstOrDefault(candidate =>
            string.Equals(candidate.Metadata.Id, card.InterfaceProfileId, StringComparison.Ordinal));
        if (profile is null)
        {
            AppendMonitoringEvent(
                card.InterfaceProfileId,
                "scan-interval-change-error",
                "Scanintervall kann nicht geändert werden, weil das Schnittstellenprofil nicht gefunden wurde.",
                InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        var result = _interfaceProfileScanIntervalUpdateService.ChangeBy(
            profile,
            deltaSeconds,
            DateTimeOffset.UtcNow,
            Environment.UserName);

        if (!result.Changed)
        {
            AppendMonitoringEvent(
                profile.Metadata.Id,
                result.ReachedMinimum ? "scan-interval-minimum" : "scan-interval-maximum",
                $"{profile.Metadata.Name}: {result.Message}",
                InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        if (!result.Success || result.Profile is null)
        {
            AppendMonitoringEvent(
                profile.Metadata.Id,
                "scan-interval-change-error",
                $"{profile.Metadata.Name}: Scanintervall wurde nicht gespeichert.",
                InterfaceMonitoringEventSeverity.Error);
            foreach (var issue in result.Issues.Where(issue => issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error))
            {
                var pathPart = string.IsNullOrWhiteSpace(issue.Path) ? string.Empty : $" ({issue.Path})";
                AppendMonitoringEvent(
                    profile.Metadata.Id,
                    $"scan-interval-validation:{issue.Message}",
                    $"{profile.Metadata.Name}: {issue.Message}{pathPart}",
                    InterfaceMonitoringEventSeverity.Error);
            }

            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var overwriteExisting = profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn;
            _profileCatalogService.SaveInterfaceProfileDefinition(paths, result.Profile, overwriteExisting);

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(
                catalog,
                selectedExportProfileId: (ExportProfileComboBox.SelectedItem as ExportProfileDefinition)?.Metadata.Id,
                selectedInterfaceProfileId: result.Profile.Metadata.Id);

            var activationText = _periodicScanCancellationTokenSource is null
                ? "Wird beim nächsten Start der Überwachung verwendet."
                : "Wird beim nächsten Start der Überwachung aktiv.";
            AppendMonitoringEvent(
                result.Profile.Metadata.Id,
                "scan-interval-changed",
                $"Scanintervall für {result.Profile.Metadata.Name} auf {result.EffectiveIntervalSeconds} Sekunden geändert. {activationText}");

            if (result.CreatedUserDefinedCopy)
            {
                AppendMonitoringEvent(
                    result.Profile.Metadata.Id,
                    "scan-interval-builtin-copy",
                    "BuiltIn-Profil wurde nicht überschrieben; die Änderung wurde als UserDefined-Konfiguration gespeichert.",
                    InterfaceMonitoringEventSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            AppendMonitoringEvent(
                profile.Metadata.Id,
                "scan-interval-save-error",
                $"{profile.Metadata.Name}: Scanintervall konnte nicht gespeichert werden: {ex.Message}",
                InterfaceMonitoringEventSeverity.Error);
        }
    }

    private void MonitoringRadar_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdateRadarAnimation(element);
        }
    }

    private void MonitoringRadar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdateRadarAnimation(element);
        }
    }

    private static void UpdateRadarAnimation(FrameworkElement element)
    {
        if (element.DataContext is not InterfaceMonitoringCardDisplay card)
        {
            StopRadarAnimation(element);
            return;
        }

        var scanBar = FindVisualChildByTag<FrameworkElement>(element, "RadarScanBar");
        var scanBarTransform = scanBar is null
            ? null
            : EnsureMutableTranslateTransform(scanBar);

        if (!card.IsScanAnimationActive)
        {
            element.SetValue(RadarAnimationKeyProperty, "");
            StopRadarAnimation(element);
            element.Opacity = 0.72;
            if (scanBarTransform is not null)
            {
                scanBarTransform.X = 0;
            }

            return;
        }

        element.Opacity = 1;
        var scanIntervalSeconds = Math.Clamp(card.ScanIntervalSeconds, 1, 60);
        if (scanBar is not null && scanBarTransform is not null)
        {
            var surfaceWidth = element.ActualWidth > 0 ? element.ActualWidth : 320;
            var barWidth = scanBar.ActualWidth > 0
                ? scanBar.ActualWidth
                : double.IsNaN(scanBar.Width) || scanBar.Width <= 0
                    ? 60
                    : scanBar.Width;
            var travelDistance = Math.Max(0, surfaceWidth - barWidth);
            var oneWayDurationSeconds = Math.Max(0.4, scanIntervalSeconds / 2.0);
            var animationKey = string.Create(
                CultureInfo.InvariantCulture,
                $"{card.InterfaceProfileId}|{card.ScanIntervalSeconds}|{surfaceWidth:0.##}|{barWidth:0.##}|{card.IsScanAnimationActive}");
            if (string.Equals(element.GetValue(RadarAnimationKeyProperty) as string, animationKey, StringComparison.Ordinal))
            {
                return;
            }

            element.SetValue(RadarAnimationKeyProperty, animationKey);
            var scanAnimation = new DoubleAnimation
            {
                From = 0,
                To = travelDistance,
                Duration = new Duration(TimeSpan.FromSeconds(oneWayDurationSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scanBarTransform.BeginAnimation(TranslateTransform.XProperty, scanAnimation);

            var pulseAnimation = new DoubleAnimation
            {
                From = 0.42,
                To = 0.9,
                Duration = new Duration(TimeSpan.FromSeconds(Math.Max(0.4, oneWayDurationSeconds))),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scanBar.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
        }
    }

    private void MonitoringRadar_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            StopRadarAnimation(element);
        }
    }

    private static void StopRadarAnimation(FrameworkElement element)
    {
        var scanBar = FindVisualChildByTag<FrameworkElement>(element, "RadarScanBar");
        if (scanBar?.RenderTransform is TranslateTransform scanBarTransform
            && !scanBarTransform.IsFrozen)
        {
            scanBarTransform.BeginAnimation(TranslateTransform.XProperty, null);
        }

        scanBar?.BeginAnimation(UIElement.OpacityProperty, null);
        element.SetValue(RadarAnimationKeyProperty, "");
    }

    private static TranslateTransform EnsureMutableTranslateTransform(FrameworkElement element)
    {
        if (element.RenderTransform is TranslateTransform transform && !transform.IsFrozen)
        {
            return transform;
        }

        transform = new TranslateTransform();
        element.RenderTransform = transform;
        return transform;
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindVisualChildByTag<T>(DependencyObject parent, object tag)
        where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild && Equals(typedChild.Tag, tag))
            {
                return typedChild;
            }

            var descendant = FindVisualChildByTag<T>(child, tag);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private void StopPeriodicScan(bool updateUi)
    {
        _periodicScanCancellationTokenSource?.Cancel();
        _periodicScanCancellationTokenSource?.Dispose();
        _periodicScanCancellationTokenSource = null;
        _periodicScanTask = null;

        if (!updateUi)
        {
            return;
        }

        PeriodicScanStatusText.Text = "Gestoppt";
        StartPeriodicScanButton.IsEnabled = true;
        StopPeriodicScanButton.IsEnabled = false;
        ActiveProfileScanButton.IsEnabled = true;
        SetAllMonitoringRuntimeStates("Gestoppt", "Neutral");
        RefreshInterfaceMonitoringCards();
        AppendMonitoringEvent("monitoring", "monitoring-state", "Überwachung gestoppt.");
    }

    private void ShowPeriodicScanResult(AutoImportScanResult result)
    {
        var profile = _profileCatalog?.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, result.InterfaceProfileId, StringComparison.Ordinal));
        var profileName = profile?.Metadata.Name ?? result.InterfaceProfileId;
        PeriodicScanLastRunText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        PeriodicScanReadyPairsText.Text = result.ReadyPairs.ToString();
        var timestamp = DateTime.Now;
        var packageEvaluation = profile is null
            ? null
            : _autoImportPackageStateService.Evaluate(profile, result.Queue, timestamp);
        if (profile is not null)
        {
            UpdateMonitoringCardFromScan(profile, result, packageEvaluation, timestamp);
        }

        RecordScanMonitoringEvents(profileName, result);

        var automaticProcessingResult = TryProcessReadyPairsAutomatically(profile, result, packageEvaluation);
        _ = automaticProcessingResult;

        RefreshInterfaceMonitoringCards();
    }

    private void UpdateMonitoringCardFromScan(
        InterfaceProfileDefinition profile,
        AutoImportScanResult result,
        AutoImportPackageEvaluationResult? packageEvaluation,
        DateTime timestamp)
    {
        var currentCard = GetRuntimeMonitoringCard(profile);
        var updatedCard = _interfaceMonitoringCardStatusService.ApplyScanResult(
            currentCard,
            profile,
            result,
            packageEvaluation,
            timestamp,
            EnableAutomaticPairProcessingCheckBox.IsChecked == true);
        updatedCard = updatedCard with
        {
            IsDetailsExpanded = GetMonitoringDetailsExpanded(profile.Metadata.Id)
        };
        _interfaceMonitoringRuntimeCards[profile.Metadata.Id] = updatedCard;
        _interfaceMonitoringRuntimeStates[profile.Metadata.Id] = new InterfaceMonitoringRuntimeState(
            updatedCard.CurrentStatus,
            updatedCard.StatusClass,
            updatedCard.LastScanText);
    }

    private void UpdateMonitoringCardFromProcessingResult(
        InterfaceProfileDefinition profile,
        AutoImportPairProcessingResult result,
        DateTime timestamp)
    {
        var currentCard = GetRuntimeMonitoringCard(profile);
        var updatedCard = _interfaceMonitoringCardStatusService.ApplyProcessingResult(
            currentCard,
            result,
            timestamp,
            EnableAutomaticPairProcessingCheckBox.IsChecked == true);
        updatedCard = updatedCard with
        {
            IsDetailsExpanded = GetMonitoringDetailsExpanded(profile.Metadata.Id)
        };
        _interfaceMonitoringRuntimeCards[profile.Metadata.Id] = updatedCard;
        _interfaceMonitoringRuntimeStates[profile.Metadata.Id] = new InterfaceMonitoringRuntimeState(
            updatedCard.CurrentStatus,
            updatedCard.StatusClass,
            updatedCard.LastScanText);
    }

    private static string CreateMonitoringStatusFromScanResult(AutoImportScanResult result)
    {
        if (result.Messages.Any(message => message.Contains("XDT-Anhang", StringComparison.OrdinalIgnoreCase)
            && message.Contains("warte", StringComparison.OrdinalIgnoreCase)))
        {
            return "Wartet auf XDT-Anhang";
        }

        if (result.Messages.Any(message => message.Contains("blockiert", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Fehler", StringComparison.OrdinalIgnoreCase)))
        {
            return "Fehler / blockiert";
        }

        if (result.ReadyPairs > 0)
        {
            return "AIS-/Geräte-Paar vollständig";
        }

        if (result.AisFilesDetected > 0 && result.DeviceFilesDetected == 0)
        {
            return "Wartet auf Gerät";
        }

        if (result.DeviceFilesDetected > 0 && result.AisFilesDetected == 0)
        {
            return "Wartet auf AIS";
        }

        return "Wartet auf AIS";
    }

    private static string CreateMonitoringStatusClassFromScanResult(AutoImportScanResult result)
    {
        if (result.Messages.Any(message => message.Contains("blockiert", StringComparison.OrdinalIgnoreCase)))
        {
            return "Blocked";
        }

        if (result.Messages.Any(message => message.Contains("Fehler", StringComparison.OrdinalIgnoreCase)))
        {
            return "Error";
        }

        if (result.ReadyPairs > 0)
        {
            return "Active";
        }

        return "Waiting";
    }

    private static string CreateMonitoringStatusFromAutomaticProcessing(AutoImportPairProcessingBatchResult result)
    {
        if (result.ErrorCount > 0)
        {
            return "Fehler / blockiert";
        }

        if (result.ProcessedCount > 0)
        {
            return "Export erfolgreich";
        }

        return "Scannt";
    }

    private static string CreateMonitoringStatusClassFromAutomaticProcessing(AutoImportPairProcessingBatchResult result)
    {
        if (result.ErrorCount > 0)
        {
            return "Error";
        }

        if (result.ProcessedCount > 0)
        {
            return "Success";
        }

        return "Active";
    }

    private AutoImportPairProcessingBatchResult? TryProcessReadyPairsAutomatically(
        InterfaceProfileDefinition? interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (EnableAutomaticPairProcessingCheckBox.IsChecked != true || _periodicScanCancellationTokenSource is null)
        {
            return null;
        }

        if (interfaceProfile is null)
        {
            AppendMonitoringEvent("monitoring", "automatic-processing-error", "Automatische Verarbeitung nicht möglich: Schnittstellenprofil wurde nicht gefunden.", InterfaceMonitoringEventSeverity.Error);
            return new AutoImportPairProcessingBatchResult(0, 0, 1, Array.Empty<AutoImportPairProcessingResult>());
        }

        var exportProfile = _profileCatalog?.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.ExportProfileId, StringComparison.Ordinal));
        if (exportProfile is null)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                "automatic-processing-export-profile-missing",
                $"Automatische Verarbeitung nicht möglich: Exportprofil fehlt für {interfaceProfile.Metadata.Name}.",
                InterfaceMonitoringEventSeverity.Error);
            return new AutoImportPairProcessingBatchResult(0, 0, 1, Array.Empty<AutoImportPairProcessingResult>());
        }

        var timestamp = DateTime.Now;
        packageEvaluation ??= _autoImportPackageStateService.Evaluate(
            interfaceProfile,
            scanResult.Queue,
            timestamp);
        foreach (var message in packageEvaluation.Messages)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                "package-state",
                $"{interfaceProfile.Metadata.Name}: {message}");
        }

        var readyPairs = packageEvaluation.ReadyPairs;
        var batchResult = _autoImportPairProcessingCoordinator.ProcessReadyPairs(
            interfaceProfile,
            exportProfile,
            readyPairs,
            automaticProcessingEnabled: true,
            timestamp,
            isMonitoringRunning: _periodicScanCancellationTokenSource is not null);

        foreach (var result in batchResult.Results)
        {
            if (result.WasSkipped)
            {
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                AppendPairMonitoringEvent(interfaceProfile, result, "status", $"{interfaceProfile.Metadata.Name}: {result.Status}");
                foreach (var message in result.Messages)
                {
                    AppendPairMonitoringEvent(interfaceProfile, result, $"message:{message}", message);
                }

                continue;
            }

            if (result.Success)
            {
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                AppendPairMonitoringEvent(
                    interfaceProfile,
                    result,
                    "status",
                    $"{interfaceProfile.Metadata.Name}: automatisch verarbeitet. Exportdatei: {result.ExportFilePath}");
            }
            else
            {
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                AppendPairMonitoringEvent(
                    interfaceProfile,
                    result,
                    "status",
                    $"{interfaceProfile.Metadata.Name}: automatische Verarbeitung fehlgeschlagen.",
                    InterfaceMonitoringEventSeverity.Error);
            }

            foreach (var message in result.Messages)
            {
                AppendPairMonitoringEvent(interfaceProfile, result, $"message:{message}", message);
            }
        }

        return batchResult;
    }

    private async void ScanActiveProfilesOnce_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            AppendMonitoringEvent("monitoring", "manual-scan-error", "Einmaliger Scan nicht möglich: keine Profile geladen.", InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        var activeProfiles = _profileCatalog.InterfaceProfiles
            .Where(profile => profile.IsActive)
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (activeProfiles.Count == 0)
        {
            AppendMonitoringEvent("monitoring", "manual-scan-error", "Einmaliger Scan nicht möglich: keine aktiven Schnittstellenprofile vorhanden.", InterfaceMonitoringEventSeverity.Warning);
            return;
        }

        ActiveProfileScanButton.IsEnabled = false;
        AppendMonitoringEvent("monitoring", "manual-scan-started", "Einmaliger Scan gestartet.");

        try
        {
            var scanTimestampText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            foreach (var profile in activeProfiles)
            {
                try
                {
                    var result = await _autoImportScannerService
                        .ScanOnceAsync(profile, TimeSpan.FromMilliseconds(200))
                        .ConfigureAwait(true);

                    var packageEvaluation = _autoImportPackageStateService.Evaluate(profile, result.Queue, DateTime.Now);
                    UpdateMonitoringCardFromScan(profile, result, packageEvaluation, DateTime.Now);
                    RecordScanMonitoringEvents(profile.Metadata.Name, result);
                }
                catch (Exception ex)
                {
                    SetMonitoringRuntimeState(profile.Metadata.Id, "Fehler / blockiert", "Error", scanTimestampText);
                    AppendMonitoringEvent(
                        profile.Metadata.Id,
                        "manual-scan-error",
                        $"{profile.Metadata.Name}: Scan-Fehler: {ex.Message}",
                        InterfaceMonitoringEventSeverity.Error);
                }
            }

            RefreshInterfaceMonitoringCards();
            AppendMonitoringEvent("monitoring", "manual-scan-finished", "Einmaliger Scan abgeschlossen.");
        }
        finally
        {
            ActiveProfileScanButton.IsEnabled = true;
        }
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
            var existingCatalog = _profileCatalog ?? CreateEmptyProfileCatalog();
            var analysisResult = _templatePackageImportConflictAnalyzer.Analyze(importResult, existingCatalog);
            var importPlan = _templatePackageImportPlanBuilder.Build(analysisResult);
            var dryRunResult = _templatePackageImportDryRunService.Preview(importResult, importPlan, existingCatalog);
            _lastTemplatePackageImportResult = importResult;
            _lastTemplatePackageImportValidationResult = validationResult;
            _lastTemplatePackageImportAnalysisResult = analysisResult;
            _lastTemplatePackageImportBasePlan = importPlan;
            _lastTemplatePackageImportPlan = importPlan;
            _lastTemplatePackageImportDryRunResult = dryRunResult;
            ShowTemplatePackageImportPreview(validationResult, analysisResult, importPlan, dryRunResult);
            UpdateTemplatePackageImportExecuteButton(dryRunResult);
            TemplatePackageImportExecutionResultTextBox.Text = "Noch keine Importübernahme ausgeführt.";
            ShowTemplatePackageImportResult(importResult, validationResult);
            AppendProfileMessage("Templatepaket-Importvorschau wurde aktualisiert. Es wurde nichts gespeichert.");
        }
        catch (Exception ex)
        {
            ClearTemplatePackageImportExecutionState();
            AppendProfileMessage($"Templatepaket konnte nicht importiert oder geprüft werden: {ex.Message}");
        }
    }

    private void TemplatePackageImportActionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_updatingTemplatePackageImportPreview
            || _lastTemplatePackageImportResult is null
            || _lastTemplatePackageImportValidationResult is null
            || _lastTemplatePackageImportAnalysisResult is null
            || _lastTemplatePackageImportBasePlan is null)
        {
            return;
        }

        try
        {
            var selections = GetTemplatePackageImportUserSelections();
            var updatedPlan = _templatePackageImportSelectionService.Apply(_lastTemplatePackageImportBasePlan, selections);
            var existingCatalog = _profileCatalog ?? CreateEmptyProfileCatalog();
            var dryRunResult = _templatePackageImportDryRunService.Preview(_lastTemplatePackageImportResult, updatedPlan, existingCatalog);
            _lastTemplatePackageImportPlan = updatedPlan;
            _lastTemplatePackageImportDryRunResult = dryRunResult;
            ShowTemplatePackageImportPreview(
                _lastTemplatePackageImportValidationResult,
                _lastTemplatePackageImportAnalysisResult,
                updatedPlan,
                dryRunResult);
            UpdateTemplatePackageImportExecuteButton(dryRunResult);
            TemplatePackageImportExecutionResultTextBox.Text = "Importvorschau wurde anhand der Benutzerentscheidung aktualisiert. Noch keine Importübernahme ausgeführt.";
        }
        catch (Exception ex)
        {
            TemplatePackageImportExecutionResultTextBox.Text = $"Importvorschau konnte nicht aktualisiert werden: {ex.Message}";
            AppendProfileMessage($"Templatepaket-Importvorschau konnte nicht aktualisiert werden: {ex.Message}");
        }
    }

    private void ExecuteTemplatePackageImport_Click(object sender, RoutedEventArgs e)
    {
        if (_lastTemplatePackageImportResult is null
            || _lastTemplatePackageImportPlan is null
            || _lastTemplatePackageImportDryRunResult is null)
        {
            AppendProfileMessage("Kein Templatepaket-Importplan vorhanden. Bitte zuerst ein Templatepaket importieren und prüfen.");
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var result = _templatePackageImportExecutor.Execute(
                _lastTemplatePackageImportResult,
                _lastTemplatePackageImportPlan,
                _lastTemplatePackageImportDryRunResult,
                paths);

            TemplatePackageImportExecutionResultTextBox.Text = FormatTemplatePackageImportExecutionResult(result);
            AppendProfileMessage($"Templatepaket-Import abgeschlossen: {result.ImportedProfiles.Count} Profil(e) als UserDefined importiert, {result.Skipped} übersprungen, {result.Blocked} blockiert.");
            AppendProfileMessage("BuiltIn-Profile wurden nicht überschrieben. Importierte Schnittstellenprofile wurden nicht automatisch aktiviert.");

            _profileCatalogService.EnsureDefaultProfiles(paths);
            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileUiAfterCatalogChange(catalog);
            ExecuteTemplatePackageImportButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            TemplatePackageImportExecutionResultTextBox.Text = $"Importübernahme konnte nicht ausgeführt werden: {ex.Message}";
            AppendProfileMessage($"Templatepaket-Importübernahme fehlgeschlagen: {ex.Message}");
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

    private void ShowTemplatePackageImportPreview(
        TemplatePackageImportValidationResult validationResult,
        TemplatePackageImportAnalysisResult analysisResult,
        TemplatePackageImportPlan importPlan,
        TemplatePackageImportDryRunResult dryRunResult)
    {
        _updatingTemplatePackageImportPreview = true;
        try
        {
            var display = _templatePackageImportPreviewDisplayService.Create(
                validationResult,
                analysisResult,
                importPlan,
                dryRunResult);

            TemplatePackageImportPreviewSummaryText.Text = display.Summary.SummaryText;
            TemplatePackageImportPreviewMessagesTextBox.Text = FormatTemplatePackageImportPreviewMessages(display);
            TemplatePackageImportPreviewGrid.ItemsSource = display.Rows;
            TemplatePackageImportDependencyPreviewGrid.ItemsSource = display.DependencyRows;
        }
        finally
        {
            _updatingTemplatePackageImportPreview = false;
        }
    }

    private void UpdateTemplatePackageImportExecuteButton(TemplatePackageImportDryRunResult dryRunResult)
    {
        ExecuteTemplatePackageImportButton.IsEnabled = dryRunResult.Items.Any(item =>
            item.WouldWrite
            && !item.IsBlocking
            && item.PlannedAction is TemplatePackageImportAction.ImportAsNew or TemplatePackageImportAction.ImportAsCopy);
    }

    private void ClearTemplatePackageImportExecutionState()
    {
        _lastTemplatePackageImportResult = null;
        _lastTemplatePackageImportValidationResult = null;
        _lastTemplatePackageImportAnalysisResult = null;
        _lastTemplatePackageImportBasePlan = null;
        _lastTemplatePackageImportPlan = null;
        _lastTemplatePackageImportDryRunResult = null;
        ExecuteTemplatePackageImportButton.IsEnabled = false;
    }

    private IReadOnlyList<TemplatePackageImportUserSelection> GetTemplatePackageImportUserSelections()
    {
        return (TemplatePackageImportPreviewGrid.ItemsSource as IEnumerable<TemplatePackageImportPreviewRow>
                ?? Array.Empty<TemplatePackageImportPreviewRow>())
            .Where(row => row.IsActionSelectionEnabled)
            .Select(row => new TemplatePackageImportUserSelection(
                ProfileKind: row.ProfileKindValue,
                ImportedProfileId: row.ImportedProfileId,
                SelectedAction: row.SelectedAction,
                TargetProfileId: null,
                TargetProfileName: null,
                IsValid: true,
                ValidationMessage: null))
            .ToList();
    }

    private void RefreshProfileUiAfterCatalogChange(ProfileCatalog catalog)
    {
        AisProfileCountText.Text = catalog.AisProfiles.Count.ToString();
        DeviceProfileCountText.Text = catalog.DeviceProfiles.Count.ToString();
        ExportProfileCountText.Text = catalog.ExportProfiles.Count.ToString();
        InterfaceProfileCountText.Text = catalog.InterfaceProfiles.Count.ToString();
        ShowProfileNameColumns(catalog);
        InitializeExportRulesView(catalog);
        InitializeInterfaceProfileConfiguration(catalog);
        InitializeAttachmentDiagnosticProfiles(catalog);
        UpdatePlaceholderTables();
        RefreshLicensedDeviceStatesFromLocalLicense();
    }

    private static string FormatTemplatePackageImportExecutionResult(TemplatePackageImportExecutionResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Import abgeschlossen: {result.ImportedProfiles.Count} Profil(e) als UserDefined importiert.");
        builder.AppendLine($"ImportAsNew: {result.ImportedAsNew}");
        builder.AppendLine($"ImportAsCopy: {result.ImportedAsCopy}");
        builder.AppendLine($"Übersprungen: {result.Skipped}");
        builder.AppendLine($"Blockiert: {result.Blocked}");
        builder.AppendLine($"Fehler: {result.Failed}");
        builder.AppendLine();

        if (result.ImportedProfiles.Count > 0)
        {
            builder.AppendLine("Geschriebene Profile:");
            foreach (var item in result.ImportedProfiles)
            {
                builder.AppendLine($"- {item.ProfileKind}: {item.TargetProfileName} ({item.TargetProfileId}) - {item.Message}");
            }
            builder.AppendLine();
        }

        if (result.SkippedProfiles.Count > 0)
        {
            builder.AppendLine("Übersprungen:");
            foreach (var item in result.SkippedProfiles)
            {
                builder.AppendLine($"- {item.ProfileKind}: {item.SourceProfileName} - {item.Message}");
            }
            builder.AppendLine();
        }

        if (result.BlockedProfiles.Count > 0)
        {
            builder.AppendLine("Blockiert:");
            foreach (var item in result.BlockedProfiles)
            {
                builder.AppendLine($"- {item.ProfileKind}: {item.SourceProfileName} - {item.Message}");
            }
            builder.AppendLine();
        }

        if (result.Warnings.Count > 0)
        {
            builder.AppendLine("Warnungen:");
            foreach (var warning in result.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
            builder.AppendLine();
        }

        builder.AppendLine("Importierte Schnittstellenprofile wurden deaktiviert und müssen vor Aktivierung geprüft werden.");
        builder.AppendLine("BuiltIn-Profile wurden nicht überschrieben.");
        builder.Append("ReplaceExisting wird in diesem Schritt noch nicht unterstützt.");

        return builder.ToString();
    }

    private static string FormatTemplatePackageImportPreviewMessages(TemplatePackageImportPreviewDisplay display)
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

    private static string FormatTemplatePackageImportIssue(TemplatePackageImportValidationIssue issue)
    {
        var profileKind = issue.ProfileKind?.ToString() ?? "Unbekannt";
        var profileId = string.IsNullOrWhiteSpace(issue.ProfileId) ? "ohne Profil-ID" : issue.ProfileId;

        return $"{issue.Message} ({profileKind}, {profileId})";
    }

    private static ProfileCatalog CreateEmptyProfileCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>());
    }

    private void SelectAisFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "GDT/XDT (*.gdt;*.xdt)|*.gdt;*.xdt|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            BuilderAisFilePathTextBox.Text = dialog.FileName;
            ClearBuilderAttachmentPreviewState(updatePreview: false);
            SyncBuilderTestPreviewArea();
            SetBuilderTestStatus("AIS-Datei geladen. Bitte als Nächstes die Gerätedatei laden.");
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
            BuilderDeviceFilePathTextBox.Text = dialog.FileName;
            ClearBuilderAttachmentPreviewState(updatePreview: false);
            SyncBuilderTestPreviewArea();
            SetBuilderTestStatus("Gerätedatei geladen. Die Exportvorschau kann jetzt aktualisiert werden.");
            AppendMessage($"Geräte-Datei ausgewählt: {dialog.FileName}");
        }
    }

    private void SelectXdtAttachmentFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XDT-Anhänge (*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.dcm;*.txt)|*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.dcm;*.txt|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            SetAttachmentDiagnosticFilePath(dialog.FileName);
            SetBuilderTestStatus("XDT-Anhang ausgewählt. Der Anhang kann jetzt vorbereitet werden.");
            AppendMessage($"XDT-Anhang ausgewählt: {dialog.FileName}");
        }
    }

    private void ScanXdtAttachmentImportFolder_Click(object sender, RoutedEventArgs e)
    {
        var selectedProfile = GetSelectedAttachmentDiagnosticProfile();
        var result = _attachmentImportFolderDiagnosticService.Scan(selectedProfile);

        ShowAttachmentImportFolderDiagnosticResult(result);
        SetBuilderTestStatus(result.Success
            ? "XDT-Anhang Importordner eingelesen."
            : "XDT-Anhang Importordner konnte nicht eingelesen werden.");
        AppendMessage(result.Message);
    }

    private void ReadBuilderXdtAttachment_Click(object sender, RoutedEventArgs e)
    {
        var selectedProfile = GetSelectedAttachmentDiagnosticProfile();
        if (selectedProfile is null)
        {
            _attachmentImportCandidateRows.Clear();
            ClearBuilderAttachmentPreviewState(updatePreview: true);
            const string message = "Bitte erst Schnittstellenprofil anlegen.";
            SetAttachmentDiagnosticResultText(message);
            SetBuilderTestStatus(message);
            AppendMessage(message);
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XDT-Anhänge (*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.dcm;*.txt)|*.pdf;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.dcm;*.txt|Alle Dateien (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var selectedFile = dialog.FileName;
        var extension = Path.GetExtension(selectedFile);
        var fileInfo = new FileInfo(selectedFile);
        if (!fileInfo.Exists)
        {
            _attachmentImportCandidateRows.Clear();
            ClearBuilderAttachmentPreviewState(updatePreview: true);
            var message = $"XDT-Anhangdatei wurde nicht gefunden: {selectedFile}";
            SetBuilderTestStatus(message);
            SetAttachmentDiagnosticResultText(message);
            AppendMessage(message);
            return;
        }

        var isSupported = SupportedBuilderAttachmentExtensions.Contains(extension);
        var selectedCandidate = new AttachmentImportCandidateDisplayRow(
            FileName: fileInfo.Name,
            Extension: extension,
            FullPath: fileInfo.FullName,
            SizeBytes: fileInfo.Length,
            LastWriteTimeUtc: fileInfo.LastWriteTimeUtc,
            IsSupported: isSupported,
            IsStable: true,
            Status: isSupported
                ? "Unterstützt, Baukasten-Dateiauswahl"
                : "Nicht unterstützt: Dateityp nicht unterstützt.");

        _attachmentImportCandidateRows.Clear();
        _attachmentImportCandidateRows.Add(selectedCandidate);
        BuilderAttachmentDiagnosticCandidatesGrid.SelectedItem = selectedCandidate;

        if (!isSupported)
        {
            ClearBuilderAttachmentPreviewState(updatePreview: true);
            var message = $"Nicht unterstützter XDT-Anhang-Dateityp: {extension}. Unterstützt sind PDF, JPG, JPEG, PNG, TIF, TIFF, DCM und TXT.";
            SetBuilderTestStatus(message);
            SetAttachmentDiagnosticResultText(message);
            AppendMessage(message);
            return;
        }

        if (!TryPrepareBuilderAttachmentPreview(selectedProfile, selectedCandidate, DateTime.Now, out var messageText))
        {
            ClearBuilderAttachmentPreviewState(updatePreview: true);
            SetBuilderTestStatus(messageText);
            AppendMessage(messageText);
            return;
        }

        if (HasManualTestInputFiles())
        {
            try
            {
                RefreshManualProcessingPreview();
                SetBuilderTestStatus("Exportvorschau mit XDT-Anhang-Linkfeldern aktualisiert.");
                AppendMessage("Exportvorschau mit XDT-Anhang-Linkfeldern aktualisiert.");
            }
            catch (Exception ex) when (ex is IOException
                or UnauthorizedAccessException
                or ArgumentException
                or NotSupportedException
                or PathTooLongException
                or InvalidOperationException)
            {
                SetBuilderTestStatus($"XDT-Anhang eingelesen. Exportvorschau konnte noch nicht aktualisiert werden: {ex.Message}");
                AppendMessage($"XDT-Anhang eingelesen, Exportvorschau konnte noch nicht aktualisiert werden: {ex.Message}");
            }
        }
        else
        {
            ShowFullExportPreviewForSelectedProfile();
            SetBuilderTestStatus("XDT-Anhang eingelesen. Exportvorschau wird aktualisiert, sobald AIS- und Gerätedatei geladen sind.");
        }

        AppendMessage(messageText);
    }

    private void AttachmentDiagnosticCandidatesGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if ((sender as System.Windows.Controls.DataGrid)?.SelectedItem is not AttachmentImportCandidateDisplayRow row)
        {
            return;
        }

        if (!row.IsSupported)
        {
            SetAttachmentDiagnosticFilePath(string.Empty);
            AppendMessage($"XDT-Anhang-Kandidat ist nicht unterstützt und wurde nicht ausgewählt: {row.FileName}");
            return;
        }

        if (!row.IsStable)
        {
            SetAttachmentDiagnosticFilePath(string.Empty);
            AppendMessage($"XDT-Anhang-Kandidat ist noch nicht stabil und wurde nicht ausgewählt: {row.FileName}");
            return;
        }

        SetAttachmentDiagnosticFilePath(row.FullPath);
        AppendMessage($"XDT-Anhang-Kandidat ausgewählt: {row.FullPath}");
    }

    private void PrepareXdtAttachment_Click(object sender, RoutedEventArgs e)
    {
        var selectedProfile = GetSelectedAttachmentDiagnosticProfile();
        var patient = GetPatientForAttachmentDiagnostic();
        var result = _attachmentExternalLinkDiagnosticService.Prepare(
            selectedProfile,
            patient,
            GetAttachmentDiagnosticFilePath(),
            DateTime.Now);

        ShowAttachmentDiagnosticResult(result);
        SetBuilderTestStatus(result.Success
            ? "XDT-Anhang vorbereitet."
            : "XDT-Anhang konnte nicht vorbereitet werden.");
        AppendMessage(result.Message);
    }

    private bool TryPrepareBuilderAttachmentPreview(
        InterfaceProfileDefinition interfaceProfile,
        AttachmentImportCandidateDisplayRow candidate,
        DateTime processingTimestamp,
        out string message)
    {
        ClearBuilderAttachmentPreviewState(updatePreview: false);

        var patient = GetPatientForAttachmentDiagnostic();
        if (patient is null || string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            message = "Für den XDT-Anhang-Test muss zuerst eine AIS-GDT/XDT-Datei mit Patientennummer geladen werden.";
            return false;
        }

        var options = interfaceProfile.FolderOptions;
        if (string.IsNullOrWhiteSpace(options.AttachmentExportFolder))
        {
            message = "XDT-Anhang Exportordner ist nicht gesetzt.";
            return false;
        }

        string desiredFileName;
        string targetFileName;
        string targetPath;
        try
        {
            desiredFileName = _attachmentFileNameBuilder.Build(
                options.AttachmentFileNameTemplate,
                patient,
                processingTimestamp,
                candidate.Extension);
            targetFileName = Directory.Exists(options.AttachmentExportFolder)
                ? _attachmentFileNameBuilder.BuildUniqueFileName(options.AttachmentExportFolder, desiredFileName)
                : desiredFileName;
            targetPath = Path.Combine(options.AttachmentExportFolder, targetFileName);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            message = $"XDT-Anhang-Zieldateiname konnte nicht vorbereitet werden: {ex.Message}";
            return false;
        }

        var fieldBuildResult = _externalAisLinkFieldBuilder.Build(options, targetPath, candidate.Extension);
        if (!fieldBuildResult.Success || fieldBuildResult.FieldSet is null)
        {
            message = fieldBuildResult.ErrorMessage ?? "XDT-Anhang-Linkfelder konnten nicht vorbereitet werden.";
            return false;
        }

        var adapterResult = _externalAisLinkXdtFieldAdapter.Adapt(fieldBuildResult.FieldSet);
        if (!adapterResult.Success)
        {
            message = adapterResult.ErrorMessage ?? "XDT-Anhang-Linkfelder konnten nicht in XDT-Felder umgesetzt werden.";
            return false;
        }

        _builderSelectedAttachmentCandidate = candidate;
        _builderPreviewAttachmentTargetFileName = targetFileName;
        _builderPreviewAttachmentTargetPath = targetPath;
        _builderTransientAttachmentFields = adapterResult.Fields;
        SetAttachmentDiagnosticFilePath(candidate.FullPath);
        SetAttachmentDiagnosticResultText(FormatBuilderAttachmentPreviewResult(
            candidate,
            targetFileName,
            targetPath,
            options.AttachmentTransferMode,
            adapterResult.Fields));

        message = "XDT-Anhang eingelesen. Linkfelder werden in der Vorschau berücksichtigt.";
        return true;
    }

    private static string FormatBuilderAttachmentPreviewResult(
        AttachmentImportCandidateDisplayRow candidate,
        string targetFileName,
        string targetPath,
        AttachmentTransferMode transferMode,
        IReadOnlyList<ExportFieldRecord> fields)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Status: XDT-Anhang eingelesen");
        builder.AppendLine("Einlesen verändert keine Dateien. Die Quelle kann aus einem beliebigen Speicherort stammen; 6305 simuliert den Schnittstellenprofil-Zielpfad.");
        builder.AppendLine();
        builder.AppendLine($"Quelle: {candidate.FullPath}");
        builder.AppendLine($"Ziel-Dateiname Vorschau: {targetFileName}");
        builder.AppendLine($"Simulierter 6305-Zielpfad: {targetPath}");
        builder.AppendLine($"Transfermodus: {transferMode}");
        builder.AppendLine();
        builder.AppendLine("Vorbereitete XDT-Felder:");
        foreach (var field in fields.OrderBy(field => field.SortOrder))
        {
            builder.AppendLine($"{field.FieldCode} = {field.Value}");
        }

        return builder.ToString().TrimEnd();
    }

    private void ClearBuilderAttachmentPreviewState(bool updatePreview)
    {
        _builderSelectedAttachmentCandidate = null;
        _builderPreviewAttachmentTargetPath = null;
        _builderPreviewAttachmentTargetFileName = null;
        _builderTransientAttachmentFields = Array.Empty<ExportFieldRecord>();
        SetAttachmentDiagnosticFilePath(string.Empty);

        if (updatePreview && _lastPipelineResult is not null)
        {
            ShowFullExportPreviewForSelectedProfile();
        }
    }

    private PatientData? GetPatientForAttachmentDiagnostic()
    {
        if (!string.IsNullOrWhiteSpace(_lastPipelineResult?.Patient?.PatientNumber))
        {
            return _lastPipelineResult.Patient;
        }

        var aisFilePath = BuilderAisFilePathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            return _lastPipelineResult?.Patient;
        }

        try
        {
            var gdtResult = new GdtParser().ParseFile(aisFilePath);
            var patient = new PatientDataMapper().Map(gdtResult.Records);
            if (!string.IsNullOrWhiteSpace(patient.PatientNumber))
            {
                ShowPatient(patient);
            }
            else
            {
                AppendMessage("AIS-Datei für XDT-Anhang-Test gelesen, aber keine Patientennummer gefunden.");
            }

            return patient;
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            AppendMessage($"AIS-Datei konnte für den XDT-Anhang-Test nicht gelesen werden: {ex.Message}");
            return _lastPipelineResult?.Patient;
        }
    }

    private void ShowAttachmentDiagnosticResult(AttachmentExternalLinkDiagnosticResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Status: {(result.Success ? "Erfolgreich vorbereitet" : "Fehler")}");
        builder.AppendLine(result.Message);

        if (result.PreparationResult is not null)
        {
            builder.AppendLine();
            builder.AppendLine($"Ziel-Dateiname: {result.PreparationResult.TargetFileName ?? "-"}");
            builder.AppendLine($"Zielpfad: {result.PreparationResult.TargetPath ?? "-"}");
            builder.AppendLine($"Transfer: {result.PreparationResult.TransferMode}");

            if (result.PreparationResult.ExportFields.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Vorbereitete XDT-Felder:");
                foreach (var field in result.PreparationResult.ExportFields.OrderBy(field => field.SortOrder))
                {
                    builder.AppendLine($"{field.FieldCode} = {field.Value}");
                }
            }
        }

        SetAttachmentDiagnosticResultText(builder.ToString().TrimEnd());
    }

    private void ShowAttachmentImportFolderDiagnosticResult(AttachmentImportFolderDiagnosticResult result)
    {
        _attachmentImportCandidateRows.Clear();
        foreach (var row in result.Candidates)
        {
            _attachmentImportCandidateRows.Add(row);
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Status: {(result.Success ? "Importordner eingelesen" : "Fehler")}");
        builder.AppendLine(result.Message);
        builder.AppendLine();
        builder.AppendLine($"Schnittstellenprofil: {DisplayOrDash(result.InterfaceProfileName)}");
        builder.AppendLine($"XDT-Anhang Importordner: {DisplayOrDash(result.ImportFolder)}");
        builder.AppendLine($"XDT-Anhang Exportordner: {DisplayOrDash(result.ExportFolder)}");
        builder.AppendLine($"Gefundene Kandidaten: {result.Candidates.Count}");
        builder.AppendLine($"Unterstützt: {result.Candidates.Count(row => row.IsSupported)}");
        builder.AppendLine($"Stabil unterstützt: {result.Candidates.Count(row => row.IsSupported && row.IsStable)}");
        builder.AppendLine($"Nicht unterstützt: {result.Candidates.Count(row => !row.IsSupported)}");

        SetAttachmentDiagnosticResultText(builder.ToString().TrimEnd());
    }

    private static string DisplayOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private void SetAttachmentDiagnosticFilePath(string value)
    {
        BuilderAttachmentDiagnosticFilePathTextBox.Text = value;
    }

    private string GetAttachmentDiagnosticFilePath()
    {
        return BuilderAttachmentDiagnosticFilePathTextBox.Text;
    }

    private void SetAttachmentDiagnosticResultText(string value)
    {
        BuilderAttachmentDiagnosticResultTextBox.Text = value;
        BuilderAttachmentDiagnosticResultTextBox.ScrollToHome();
    }

    private void Process_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BuilderAisFilePathTextBox.Text) || string.IsNullOrWhiteSpace(BuilderDeviceFilePathTextBox.Text))
        {
            AppendMessage("Bitte zuerst AIS- und Geräte-Datei auswählen.");
            SetBuilderTestStatus("Bitte zuerst AIS-Datei und Gerätedatei laden.");
            return;
        }

        RefreshManualProcessingPreview();

        if (_lastPipelineResult!.HasErrors)
        {
            SetBuilderTestStatus("Exportvorschau aktualisiert, Verarbeitung enthält Fehler.");
            AppendMessage("Verarbeitung abgeschlossen mit Fehlern.");
        }
        else
        {
            SetBuilderTestStatus(_builderTransientAttachmentFields.Count > 0
                ? $"Exportvorschau mit XDT-Anhang-Linkfeldern aktualisiert. {_lastPipelineResult.Measurements.Count} Messwerte erkannt. Exportprofil wurde nicht verändert."
                : $"Kein XDT-Anhang eingelesen. Vorschau enthält keine 6302-6305. {_lastPipelineResult.Measurements.Count} Messwerte erkannt.");
            AppendMessage("Verarbeitung erfolgreich abgeschlossen.");
        }
    }

    private bool HasManualTestInputFiles()
    {
        return !string.IsNullOrWhiteSpace(BuilderAisFilePathTextBox.Text)
            && !string.IsNullOrWhiteSpace(BuilderDeviceFilePathTextBox.Text);
    }

    private void RefreshManualProcessingPreview()
    {
        _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
        _lastPipelineResult = _pipelineService.ProcessFiles(BuilderAisFilePathTextBox.Text, BuilderDeviceFilePathTextBox.Text, _currentProfile);

        ShowPatient(_lastPipelineResult.Patient);
        BuilderMeasurementsGrid.ItemsSource = _lastPipelineResult.Measurements;
        UpdatePlaceholderTables();
        ShowExportRulePreviewForSelectedRule();
        ShowFullExportPreviewForSelectedProfile();

        _plannedFileName = _fileNameBuilder.Build(_currentProfile, _lastPipelineResult.Patient, DateTime.Now);

        ShowIssues(_lastPipelineResult.Issues);
        SyncBuilderTestPreviewArea();
    }

    private void RunBuilderTestExport_Click(object sender, RoutedEventArgs e)
    {
        if (_lastPipelineResult is null)
        {
            SetBuilderTestStatus("Bitte zuerst AIS-Datei und Gerätedatei laden.");
            AppendMessage("Testexport nicht gestartet: keine Beispielwerte geladen.");
            return;
        }

        if (ExportProfileComboBox.SelectedItem is not ExportProfileDefinition exportProfile)
        {
            SetBuilderTestStatus("Bitte zuerst ein Exportprofil auswählen.");
            AppendMessage("Testexport nicht gestartet: kein Exportprofil ausgewählt.");
            return;
        }

        using var dialog = new WinForms.FolderBrowserDialog
        {
            Description = "Temporären Zielordner für den Baukasten-Testexport auswählen"
        };
        if (dialog.ShowDialog() != WinForms.DialogResult.OK)
        {
            return;
        }

        var selectedProfile = GetSelectedAttachmentDiagnosticProfile();
        var sourceAttachmentPath = _builderTransientAttachmentFields.Count > 0
            ? _builderSelectedAttachmentCandidate?.FullPath ?? GetAttachmentDiagnosticFilePath()
            : null;
        var fileName = _plannedFileName ?? _fileNameBuilder.Build(_currentProfile, _lastPipelineResult.Patient, DateTime.Now);
        var result = _builderTestExportService.Export(new BuilderTestExportRequest(
            TargetFolder: dialog.SelectedPath,
            ExportFileName: fileName,
            OutputEncoding: exportProfile.OutputEncoding,
            ExportProfileName: exportProfile.Metadata.Name,
            ExportRules: GetEffectiveExportRules(exportProfile, null, null),
            Patient: _lastPipelineResult.Patient,
            Measurements: _lastPipelineResult.Measurements,
            FolderOptions: sourceAttachmentPath is null ? null : selectedProfile?.FolderOptions,
            SourceAttachmentPath: sourceAttachmentPath,
            IsSourceAttachmentStable: _builderSelectedAttachmentCandidate?.IsStable,
            ProcessingTimestamp: DateTime.Now));

        FullExportPreviewTextBox.Text = FormatBuilderTestExportResultPreview(exportProfile, result);

        if (!result.Success)
        {
            SetBuilderTestStatus("Testexport konnte nicht erstellt werden.");
            foreach (var issue in result.Issues)
            {
                AppendMessage($"[Baukasten-Testexport] {issue}");
            }

            return;
        }

        var statusBuilder = new StringBuilder();
        statusBuilder.AppendLine("Testexport erstellt.");
        statusBuilder.AppendLine($"Test-XDT-Datei: {result.ExportFilePath}");
        if (!string.IsNullOrWhiteSpace(result.AttachmentTargetPath))
        {
            statusBuilder.AppendLine($"Test-XDT-Anhang: {result.AttachmentTargetPath}");
        }

        if (!string.IsNullOrWhiteSpace(result.AttachmentSimulatedTargetPath))
        {
            statusBuilder.AppendLine($"Simulierter 6305-Zielpfad: {result.AttachmentSimulatedTargetPath}");
        }

        statusBuilder.Append("Exportprofil wurde nicht verändert.");
        SetBuilderTestStatus(statusBuilder.ToString());
        AppendMessage($"Testexport erstellt: {result.ExportFilePath}");
        if (!string.IsNullOrWhiteSpace(result.AttachmentTargetPath))
        {
            AppendMessage($"XDT-Anhang wurde in den Testexport-Zielordner übernommen: {result.AttachmentTargetPath}");
        }

        if (!string.IsNullOrWhiteSpace(result.AttachmentSimulatedTargetPath))
        {
            AppendMessage($"6305 verweist auf den simulierten Schnittstellenprofil-Zielpfad: {result.AttachmentSimulatedTargetPath}");
        }
    }

    private static string FormatBuilderTestExportResultPreview(
        ExportProfileDefinition exportProfile,
        BuilderTestExportResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Exportprofil: {exportProfile.Metadata.Name}");
        builder.AppendLine("Baukasten-Testexport: Exportprofil wurde nicht verändert.");
        if (!string.IsNullOrWhiteSpace(result.ExportFilePath))
        {
            builder.AppendLine($"Test-XDT-Datei: {result.ExportFilePath}");
        }

        if (!string.IsNullOrWhiteSpace(result.AttachmentTargetPath))
        {
            builder.AppendLine($"Test-XDT-Anhang: {result.AttachmentTargetPath}");
        }

        if (!string.IsNullOrWhiteSpace(result.AttachmentSimulatedTargetPath))
        {
            builder.AppendLine($"Simulierter 6305-Zielpfad: {result.AttachmentSimulatedTargetPath}");
        }

        if (result.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Hinweise:");
            foreach (var issue in result.Issues)
            {
                builder.AppendLine($"- {issue}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("ExportContent:");
        builder.Append(result.ExportContent.Length == 0 ? "(leer)" : result.ExportContent);
        return builder.ToString().TrimEnd();
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_lastPipelineResult is null || string.IsNullOrWhiteSpace(_lastPipelineResult.ExportContent))
        {
            AppendMessage("Keine Exportvorschau vorhanden. Bitte zuerst verarbeiten.");
            SetBuilderTestStatus("Bitte zuerst die Exportvorschau aktualisieren.");
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

            SetBuilderTestStatus("Testexportdatei konnte nicht geschrieben werden.");
        }
        else
        {
            SetBuilderTestStatus($"Testexportdatei geschrieben: {exportResult.FilePath}");
            AppendMessage($"Export erfolgreich geschrieben: {exportResult.FilePath}");
        }
    }

    private void ShowPatient(PatientData? patient)
    {
        UpdateBuilderPatientSummary(patient);
    }

    private void SyncBuilderTestPreviewArea()
    {
        if (_lastPipelineResult is not null)
        {
            BuilderMeasurementsGrid.ItemsSource = _lastPipelineResult.Measurements;
            BuilderMeasurementsExpander.Header = $"Schritt 4 - Messwerte prüfen ({_lastPipelineResult.Measurements.Count} Messwerte erkannt)";
        }
        else
        {
            BuilderMeasurementsExpander.Header = "Schritt 4 - Messwerte prüfen";
        }

        UpdateBuilderPatientSummary(_lastPipelineResult?.Patient);
        UpdateBuilderDeviceSummary();
    }

    private void SetBuilderTestStatus(string message)
    {
        BuilderTestStatusTextBox.Text = message;
    }

    private void UpdateBuilderPatientSummary(PatientData? patient)
    {
        if (patient is null && string.IsNullOrWhiteSpace(BuilderAisFilePathTextBox.Text))
        {
            BuilderPatientSummaryTextBox.Text = "Noch keine AIS-Testdaten geladen.";
            return;
        }

        var builder = new StringBuilder();
        AppendSummaryLine(builder, "PatientNumber", patient?.PatientNumber);
        AppendSummaryLine(builder, "FirstName", patient?.FirstName);
        AppendSummaryLine(builder, "LastName", patient?.LastName);
        AppendSummaryLine(builder, "BirthDate", patient?.BirthDate);
        AppendSummaryLine(builder, "Street", patient?.Street);
        AppendSummaryLine(builder, "PostalCodeCity", patient?.PostalCodeCity);
        AppendSummaryLine(builder, "ExaminationType", patient?.ExaminationType);

        if (patient is null)
        {
            builder.AppendLine();
            builder.Append("AIS-Datei ausgewählt, Patientendaten noch nicht eingelesen.");
        }

        BuilderPatientSummaryTextBox.Text = builder.ToString().TrimEnd();
    }

    private void UpdateBuilderDeviceSummary()
    {
        var deviceFilePath = BuilderDeviceFilePathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(deviceFilePath) && _lastPipelineResult is null)
        {
            BuilderDeviceSummaryTextBox.Text = "Noch keine Gerätedatei geladen.";
            return;
        }

        var builder = new StringBuilder();
        AppendSummaryLine(builder, "verwendetes Geräteprofil", _currentProfile.Name);
        AppendSummaryLine(builder, "Dateiname", string.IsNullOrWhiteSpace(deviceFilePath) ? null : Path.GetFileName(deviceFilePath));
        AppendSummaryLine(builder, "erkannte Messwerte Anzahl", _lastPipelineResult?.Measurements.Count.ToString());
        AppendSummaryLine(builder, "Status", CreateBuilderDeviceStatusText(deviceFilePath));
        AppendSummaryLine(builder, "Dateiformat/Endung", string.IsNullOrWhiteSpace(deviceFilePath) ? null : Path.GetExtension(deviceFilePath));
        BuilderDeviceSummaryTextBox.Text = builder.ToString().TrimEnd();
    }

    private string CreateBuilderDeviceStatusText(string deviceFilePath)
    {
        if (_lastPipelineResult is null)
        {
            return string.IsNullOrWhiteSpace(deviceFilePath)
                ? "Noch nicht geladen"
                : "Ausgewählt, noch nicht verarbeitet";
        }

        return _lastPipelineResult.HasErrors
            ? "Verarbeitet mit Fehlern"
            : "Verarbeitet";
    }

    private static void AppendSummaryLine(StringBuilder builder, string label, string? value)
    {
        builder.AppendLine($"{label}: {DisplayOrDash(value)}");
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

    private void RecordScanMonitoringEvents(string profileName, AutoImportScanResult result)
    {
        if (result.AisFilesDetected > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                "scan-ais-detected",
                $"{profileName}: AIS-Datei erkannt ({result.AisFilesDetected}).");
        }

        if (result.DeviceFilesDetected > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                "scan-device-detected",
                $"{profileName}: Gerätedatei erkannt ({result.DeviceFilesDetected}).");
        }

        if (result.ReadyPairs > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                "scan-ready-pair",
                $"{profileName}: AIS-/Geräte-Paar vollständig ({result.ReadyPairs}).");
        }

        foreach (var message in result.Messages)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                $"scan-message:{message}",
                $"{profileName}: {message}",
                InterfaceMonitoringEventSeverity.Warning);
        }
    }

    private void AppendPairMonitoringEvent(
        InterfaceProfileDefinition interfaceProfile,
        AutoImportPairProcessingResult result,
        string eventKey,
        string message,
        InterfaceMonitoringEventSeverity severity = InterfaceMonitoringEventSeverity.Info)
    {
        AppendMonitoringEvent(
            interfaceProfile.Metadata.Id,
            $"pair:{result.PairKey}:{eventKey}",
            message,
            severity);
    }

    private void AppendMonitoringEvent(
        string scopeId,
        string eventKey,
        string message,
        InterfaceMonitoringEventSeverity severity = InterfaceMonitoringEventSeverity.Info)
    {
        var entry = _monitoringEventDeduplicationService.Record(
            scopeId,
            eventKey,
            message,
            DateTime.Now,
            severity);

        if (entry is null)
        {
            return;
        }

        AppendMessage(entry.Message);
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

    private sealed record InterfaceMonitoringRuntimeState(
        string CurrentStatus,
        string StatusClass,
        string LastScanText);

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
