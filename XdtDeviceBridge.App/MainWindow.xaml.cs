using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;
using WinForms = System.Windows.Forms;

namespace XdtDeviceBridge.App;

public sealed record XdtBaukastenRuleGridRow(int RowNumber, ExportRuleDefinition Rule);

public partial class MainWindow : Window
{
    private const string MonitoringNotificationSoundRelativePath = @"Assets\Sounds\04_praxis_terminal_signal.wav";
    private const string AppIconResourcePath = "Assets/App/XDTBox.ico";
    private const bool MonitoringNotificationSoundEnabled = true;
    private const string InterfaceProfileFilterAll = "Alle";

    private static readonly DependencyProperty RadarAnimationKeyProperty = DependencyProperty.RegisterAttached(
        "RadarAnimationKey",
        typeof(string),
        typeof(MainWindow),
        new PropertyMetadata(""));

    private static readonly DependencyProperty StatusOrbAnimationKeyProperty = DependencyProperty.RegisterAttached(
        "StatusOrbAnimationKey",
        typeof(string),
        typeof(MainWindow),
        new PropertyMetadata(""));

    private static readonly DependencyProperty StatusOrbFlashKeyProperty = DependencyProperty.RegisterAttached(
        "StatusOrbFlashKey",
        typeof(string),
        typeof(MainWindow),
        new PropertyMetadata(""));

    private readonly XdtBaukastenPreviewService _xdtBaukastenPreviewService = new();
    private readonly FileExportService _fileExportService = new();
    private readonly AppDataPathProvider _appDataPathProvider = new();
    private readonly ProfileCatalogService _profileCatalogService = new();
    private readonly TemplatePackageExporter _templatePackageExporter = new();
    private readonly TemplatePackageExportSelectionService _templatePackageExportSelectionService = new();
    private readonly TemplatePackageImportDryRunService _templatePackageImportDryRunService = new();
    private readonly TemplatePackageImportPreviewService _templatePackageImportPreviewService = new();
    private readonly TemplatePackageImportExecutor _templatePackageImportExecutor = new();
    private readonly TemplatePackageImportSelectionService _templatePackageImportSelectionService = new();
    private readonly XdtBaukastenTemplateLibraryService _xdtBaukastenTemplateLibraryService = new();
    private readonly InstallationInfoProvider _installationInfoProvider = new();
    private readonly LicenseFileRepository _licenseFileRepository = new();
    private readonly LicensedDeviceGracePeriodRepository _licensedDeviceGracePeriodRepository = new();
    private readonly LicenseEvaluator _licenseEvaluator = new();
    private readonly LicenseImportService _licenseImportService = new();
    private readonly LocalLicenseRemovalService _localLicenseRemovalService = new();
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
    private readonly InterfaceProfileMonitoringResetService _interfaceProfileMonitoringResetService = new();
    private readonly InterfaceProfileInputFolderResetService _interfaceProfileInputFolderResetService = new();
    private readonly AttachmentExternalLinkDiagnosticService _attachmentExternalLinkDiagnosticService = new();
    private readonly AttachmentImportFolderDiagnosticService _attachmentImportFolderDiagnosticService = new();
    private readonly MedistarHistoricalMeasurementParser _cv5000HistoryParser = new();
    private readonly TopconCv5000ImportXmlWriter _cv5000ImportWriter = new();
    private readonly NidekRt6100InputXmlWriter _nidekRt6100ImportWriter = new();
    private readonly LicenseRequestBuilder _licenseRequestBuilder = new();
    private readonly LicenseRequestFileRepository _licenseRequestFileRepository = new();
    private readonly LicenseCustomerDataRepository _licenseCustomerDataRepository = new();
    private readonly LicenseDeviceLocationRepository _licenseDeviceLocationRepository = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly XdtExportBuilder _xdtExportBuilder = new();
    private readonly UserDefinedProfileCreationService _userDefinedProfileCreationService = new();
    private readonly UserDefinedProfileRenameService _userDefinedProfileRenameService = new();
    private readonly ProfileManagementService _profileManagementService = new();
    private readonly InterfaceProfileConfigurationService _interfaceProfileConfigurationService = new();
    private readonly AisOutputInfoService _aisOutputInfoService = new();
    private readonly SaveFeedbackDisplayService _saveFeedbackDisplayService = new();
    private readonly ExportProfileDeletionService _exportProfileDeletionService = new();
    private readonly InterfaceProfileScanIntervalUpdateService _interfaceProfileScanIntervalUpdateService = new();
    private readonly InterfaceProfileAutoDetachService _interfaceProfileAutoDetachService = new();
    private readonly InterfaceProfileAutoRedockService _interfaceProfileAutoRedockService = new();
    private readonly DeviceProfileImageOverrideService _deviceProfileImageOverrideService = new();
    private readonly InterfaceProfileNotificationSoundService _interfaceProfileNotificationSoundService = new(isEnabled: MonitoringNotificationSoundEnabled);
    private readonly IInterfaceProfileNotificationSoundPlayer _interfaceProfileNotificationSoundPlayer = new WavInterfaceProfileNotificationSoundPlayer();
    private readonly TrayWindowStateService _trayWindowStateService = new();
    private readonly InterfaceProfileActivationEvaluationService _interfaceProfileActivationEvaluationService = new();
    private readonly InterfaceProfileActivationPreviewDisplayService _interfaceProfileActivationPreviewDisplayService = new();
    private readonly InterfaceProfileActivationPreparationPreviewService _interfaceProfileActivationPreparationPreviewService = new();
    private readonly InterfaceProfileActivationGuardService _interfaceProfileActivationGuardService = new();
    private readonly InterfaceProfileFolderSetupService _interfaceProfileFolderSetupService = new();
    private readonly IXdtBoxBackupService _xdtBoxBackupService = new XdtBoxBackupService();
    private readonly XdtBoxBackupPathService _xdtBoxBackupPathService = new();
    private readonly XdtBoxAppSettingsRepository _appSettingsRepository = new();
    private readonly TabProtectionService _tabProtectionService = new();
    private readonly TechnicianWhiteboardService _technicianWhiteboardService = new();
    private readonly ISerialPortDiscoveryService _serialPortDiscoveryService = new SerialPortDiscoveryService();
    private readonly ISerialDeviceCommunicationService _serialDeviceCommunicationService = new SerialDeviceCommunicationService();
    private readonly INidekRtSerialPhoropterCommunicationService _nidekRtSerialCommunicationService;
    private readonly InterfaceProfileFloatingWindowStateRepository _floatingWindowStateRepository = new();
    private readonly ObservableCollection<LicenseDeviceStateRow> _licensedDeviceStateRows = new();
    private readonly ObservableCollection<ActiveInterfaceProfileStatusRow> _activeInterfaceProfileStatusRows = new();
    private readonly ObservableCollection<InterfaceMonitoringCardDisplay> _interfaceMonitoringCards = new();
    private readonly ObservableCollection<InterfaceProfileActivationFolderDisplay> _interfaceProfileActivationFolderRows = new();
    private readonly ObservableCollection<InterfaceProfileActivationAttachmentDisplay> _interfaceProfileActivationAttachmentRows = new();
    private readonly ObservableCollection<InterfaceProfileActivationPreviewRow> _interfaceProfileActivationPreviewRows = new();
    private readonly ObservableCollection<ProfileManagementRow> _profileManagementRows = new();
    private readonly XdtBaukastenState _xdtBaukastenState = new();
    private readonly ObservableCollection<XdtBaukastenRuleGridRow> _xdtBaukastenExportRules = new();
    private readonly ObservableCollection<XdtBaukastenPreviewLine> _xdtBaukastenResultLines = new();
    private readonly ObservableCollection<XdtBaukastenPlaceholder> _xdtBaukastenAisPlaceholders = new();
    private readonly ObservableCollection<XdtBaukastenPlaceholder> _xdtBaukastenDevicePlaceholders = new();
    private readonly ObservableCollection<XdtBaukastenPlaceholder> _xdtBaukastenDeviceOutputPlaceholders = new();
    private readonly XdtBaukastenUndoBuffer _xdtBaukastenUndoBuffer = new(10);
    private readonly XdtBaukastenTextEncodingReader _xdtBaukastenTextEncodingReader = new();
    private readonly XdtBaukastenPlaceholderValueService _xdtBaukastenPlaceholderValueService = new();
    private readonly XmlDeviceParser _xdtBaukastenDeviceParser = new();
    private readonly HuvitzTextDeviceParser _xdtBaukastenHuvitzTextParser = new();
    private readonly ShinNipponDeviceParser _xdtBaukastenShinNipponParser = new();
    private readonly TomeyDeviceParser _xdtBaukastenTomeyParser = new();
    private readonly TomeyEmDeviceParser _xdtBaukastenTomeyEmParser = new();
    private readonly CanonZeissVisionixDeviceParser _xdtBaukastenCanonZeissVisionixParser = new();
    private readonly ZeissIolMaster700DeviceParser _xdtBaukastenZeissIolMaster700Parser = new();
    private readonly XdtBaukastenDeviceCompatibilityService _xdtBaukastenDeviceCompatibilityService = new();
    private readonly Dictionary<string, InterfaceMonitoringRuntimeState> _interfaceMonitoringRuntimeStates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InterfaceMonitoringCardDisplay> _interfaceMonitoringRuntimeCards = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PendingImportQueue> _lastMonitoringScanQueuesByProfileId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _monitoringDetailsExpandedByProfileId = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _nidekRtSerialListenOnlyProfiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _nidekRtSerialSendTestProfiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _nidekRtSerialReturnProcessingProfiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NidekRtSerialSendContext> _nidekRtSerialSendContexts = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _nidekRtSerialCompletedWorkflowProfiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly InterfaceProfileFloatingWindowStateService _floatingWindowStateService = new();
    private readonly InterfaceProfileFloatingWindowRestoreGate _floatingWindowRestoreGate = new();
    private readonly Dictionary<string, FloatingInterfaceProfileWindow> _floatingMonitoringWindows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PendingDocumentAttachmentConfirmation> _pendingDocumentAttachmentConfirmations = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _cv5000DeviceOutputHandledAisKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer _autoRedockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private WinForms.NotifyIcon? _trayIcon;
    private WinForms.ContextMenuStrip? _trayContextMenu;

    private DeviceProfile _currentProfile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
    private ProfileCatalog? _profileCatalog;
    private InstallationInfo? _installationInfo;
    private bool _updatingXdtBaukastenSelection;
    private bool _restoringXdtBaukastenUndo;
    private string? _xdtBaukastenSelectedRuleId;
    private CancellationTokenSource? _periodicScanCancellationTokenSource;
    private Task? _periodicScanTask;
    private bool _refreshingInterfaceMonitoringCards;
    private IReadOnlyList<ProfileManagementRow> _profileManagementAllRows = Array.Empty<ProfileManagementRow>();
    private bool _notificationSoundFailureReported;
    private DispatcherTimer? _interfaceProfileSaveFeedbackTimer;
    private bool _hasInterfaceProfileSaveButtonOriginalState;
    private bool _hasAutoStartedPeriodicScan;
    private bool _hasAppliedStartupTrayPreference;
    private bool _userStoppedPeriodicScan;
    private bool _isUpdatingInterfaceProfileFilters;
    private XdtBoxAppSettings _appSettings = XdtBoxAppSettings.CreateDefault();
    private TabProtectionSettings _tabProtectionSettings = TabProtectionSettings.Disabled;
    private object? _interfaceProfileSaveButtonOriginalContent;
    private System.Windows.Media.Brush? _interfaceProfileSaveButtonOriginalBackground;
    private System.Windows.Media.Brush? _interfaceProfileSaveButtonOriginalForeground;
    private System.Windows.Media.Brush? _interfaceProfileSaveButtonOriginalBorderBrush;
    private System.Windows.Point? _technicianWhiteboardDragStart;
    private FrameworkElement? _technicianWhiteboardDraggedElement;
    private Border? _technicianWhiteboardSelectedElement;
    private TechnicianWhiteboardMode _technicianWhiteboardMode = TechnicianWhiteboardMode.Hand;
    private System.Windows.Point _technicianWhiteboardContextMenuPosition;
    private TabItem? _lastAllowedMainTabItem;
    private bool _isTabProtectionUnlocked;
    private bool _isRestoringProtectedTabSelection;
    private bool _updatingTabProtectionUi;

    private enum NidekRtSerialSendTestMode
    {
        RequestReady,
        RequestReadyWithDtrToggle,
        DirectWriter,
        RsWriterWithoutSd
    }

    private enum TechnicianWhiteboardMode
    {
        Text,
        Hand
    }

    private sealed record NidekRtSerialSendContext(
        PatientData Patient,
        IReadOnlyList<AisHistoricalMeasurementRecord> SelectedMeasurements,
        PendingImportFile AisFile,
        string AisKey,
        string DeviceDisplayName,
        string DeviceOutputKey,
        DateTime Timestamp);

    public MainWindow()
    {
        _nidekRtSerialCommunicationService = new NidekRtSerialPhoropterCommunicationService(_serialDeviceCommunicationService);
        InitializeComponent();
        SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Hand);
        LoadAppSettings();
        LoadTabProtectionSettings();
        _lastAllowedMainTabItem = MainTabControl.SelectedItem as TabItem;
        UpdateTabProtectionUi();
        LoadFloatingWindowStates();
        LicensedDeviceStatesGrid.ItemsSource = _licensedDeviceStateRows;
        InterfaceMonitoringCardsItemsControl.ItemsSource = _interfaceMonitoringCards;
        InterfaceActivationPreviewFolderChecksGrid.ItemsSource = _interfaceProfileActivationFolderRows;
        InterfaceActivationPreviewAttachmentChecksGrid.ItemsSource = _interfaceProfileActivationAttachmentRows;
        InterfaceActivationPreviewChecksGrid.ItemsSource = _interfaceProfileActivationPreviewRows;
        ProfileManagementGrid.ItemsSource = _profileManagementRows;
        XdtBaukastenExportRulesGrid.ItemsSource = _xdtBaukastenExportRules;
        XdtBaukastenResultLinesGrid.ItemsSource = _xdtBaukastenResultLines;
        XdtBaukastenAisPlaceholderItems.ItemsSource = _xdtBaukastenAisPlaceholders;
        XdtBaukastenDevicePlaceholderItems.ItemsSource = _xdtBaukastenDevicePlaceholders;
        XdtBaukastenDeviceOutputPlaceholderItems.ItemsSource = _xdtBaukastenDeviceOutputPlaceholders;
        _autoRedockTimer.Tick += AutoRedockTimer_Tick;
        AttachInterfaceActivationPreviewDraftChangeHandlers();
        InitializeTrayIcon();
        RefreshSerialPortComboBox(InterfaceSerialPortComboBox);
        InterfaceSerialStatusTextBlock.Text = "RS232 ersetzt nur den Geräte-Eingangsordner. AIS-Patientendatei, Ergebnisordner, Archiv und Fehlerordner bleiben wie gewohnt konfigurierbar.";
        InitializeProfileOverview();
        InitializeLicenseOverview();
        InitializeBackupOverview();
        LoadTechnicianWhiteboard();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_appSettings.CloseToTrayInsteadOfExit && _trayWindowStateService.ShouldCancelClose())
        {
            e.Cancel = true;
            MinimizeMainWindowToTray();
            return;
        }

        if (_appSettings.ConfirmExitWhileMonitoring
            && _periodicScanCancellationTokenSource is not null
            && !_trayWindowStateService.IsExitRequested
            && !ConfirmExitWhileMonitoringRuns())
        {
            e.Cancel = true;
            return;
        }

        StopPeriodicScan(updateUi: false);
        _autoRedockTimer.Stop();
        SaveFloatingWindowStates();
        CloseAllFloatingMonitoringWindows();
        DisposeTrayIcon();
        base.OnClosing(e);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized)
        {
            MinimizeMainWindowToTray();
        }
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        RestoreFloatingWindowsOnce();
        Dispatcher.BeginInvoke((Action)TryAutoStartPeriodicScanOnce, DispatcherPriority.ContextIdle);
        Dispatcher.BeginInvoke((Action)ApplyStartupTrayPreferenceOnce, DispatcherPriority.ApplicationIdle);
    }

    private void LoadAppSettings()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _appSettings = _appSettingsRepository.LoadOrDefault(GetAppSettingsFilePath(paths));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            _appSettings = XdtBoxAppSettings.CreateDefault();
            AppendMessage($"App-Einstellungen konnten nicht geladen werden: {ex.Message}");
        }
    }

    private void SaveAppSettings()
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        _appSettingsRepository.Save(GetAppSettingsFilePath(paths), _appSettings);
    }

    private static string GetAppSettingsFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, "ui", "app-settings.json");
    }

    private void LoadTabProtectionSettings()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _tabProtectionSettings = _tabProtectionService.LoadOrDefault(TabProtectionService.GetDefaultSettingsFilePath(paths));
            _isTabProtectionUnlocked = false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            _tabProtectionSettings = TabProtectionSettings.Disabled;
            _isTabProtectionUnlocked = false;
            AppendMessage($"Tab-Schutz Einstellungen konnten nicht geladen werden: {ex.Message}");
        }
    }

    private void SaveTabProtectionSettings()
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        _tabProtectionService.Save(TabProtectionService.GetDefaultSettingsFilePath(paths), _tabProtectionSettings);
    }

    private bool IsTabProtectionConfigured()
    {
        return _tabProtectionSettings.HasPassword;
    }

    private static bool IsProtectedMainTab(TabItem? tabItem)
    {
        var header = tabItem?.Header?.ToString();
        return !string.Equals(header, "Verarbeitung", StringComparison.OrdinalIgnoreCase);
    }

    private TabItem? FindProcessingTab()
    {
        return MainTabControl.Items
            .OfType<TabItem>()
            .FirstOrDefault(tab => string.Equals(tab.Header?.ToString(), "Verarbeitung", StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateTabProtectionUi()
    {
        if (TabProtectionStatusTextBlock is null || TabProtectionToggleButton is null)
        {
            return;
        }

        _updatingTabProtectionUi = true;
        try
        {
            if (!IsTabProtectionConfigured())
            {
                TabProtectionStatusTextBlock.Text = "TAB Schutz nicht aktiviert";
                TabProtectionStatusTextBlock.Foreground = (System.Windows.Media.Brush)FindResource("XdtBoxMutedTextBrush");
                TabProtectionToggleButton.IsEnabled = false;
                TabProtectionToggleButton.IsChecked = false;
                TabProtectionToggleButton.Content = "AUS";
                return;
            }

            TabProtectionToggleButton.IsEnabled = true;
            TabProtectionToggleButton.IsChecked = _isTabProtectionUnlocked;
            if (_isTabProtectionUnlocked)
            {
                TabProtectionStatusTextBlock.Text = "TAB Schutz entsperrt";
                TabProtectionStatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 83, 45));
                TabProtectionToggleButton.Content = "AUF";
            }
            else
            {
                TabProtectionStatusTextBlock.Text = "TAB Schutz aktiv";
                TabProtectionStatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 29, 29));
                TabProtectionToggleButton.Content = "ZU";
            }
        }
        finally
        {
            _updatingTabProtectionUi = false;
        }
    }

    private bool ConfirmExitWhileMonitoringRuns()
    {
        var result = System.Windows.MessageBox.Show(
            this,
            "Die Überwachung läuft noch. Möchten Sie XDTBox wirklich beenden?",
            "XDTBox beenden",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    private void ApplyStartupTrayPreferenceOnce()
    {
        if (_hasAppliedStartupTrayPreference)
        {
            return;
        }

        _hasAppliedStartupTrayPreference = true;
        if (_appSettings.StartMinimizedToTray)
        {
            MinimizeMainWindowToTray();
        }
    }

    private void RefreshSerialPortComboBox(System.Windows.Controls.ComboBox comboBox)
    {
        var currentText = comboBox.Text;
        var ports = _serialPortDiscoveryService.GetAvailablePortNames();
        comboBox.ItemsSource = ports;
        if (!string.IsNullOrWhiteSpace(currentText))
        {
            comboBox.Text = currentText;
        }
        else if (ports.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private void AttachInterfaceActivationPreviewDraftChangeHandlers()
    {
        System.Windows.Controls.TextChangedEventHandler textChangedHandler = (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        foreach (var textBox in new[]
        {
            InterfaceAisImportFolderTextBox,
            InterfaceDeviceImportFolderTextBox,
            InterfaceExportFolderTextBox,
            InterfaceArchiveFolderTextBox,
            InterfaceErrorFolderTextBox,
            InterfaceDeviceOutputFolderTextBox,
            InterfaceDeviceOutputFileNameTextBox,
            InterfaceAutoImportScanIntervalSecondsTextBox,
            InterfaceDeviceFileWaitTimeoutMinutesTextBox,
            InterfaceAttachmentImportFolderTextBox,
            InterfaceAttachmentExportFolderTextBox,
            InterfaceAttachmentFileNameTemplateTextBox,
            InterfaceAttachmentWaitTimeoutSecondsTextBox,
            InterfaceAttachmentFileStabilityWaitSecondsTextBox,
            InterfaceAttachmentQuietPeriodSecondsTextBox,
            InterfaceAttachmentLinkDocumentNameTextBox,
            InterfaceAttachmentLinkFileFormatTextBox,
            InterfaceAttachmentLinkDescriptionTextBox,
            InterfaceAttachmentLinkPathTemplateTextBox,
            InterfaceArchiveRetentionDaysTextBox,
            InterfaceSerialBaudRateTextBox,
            InterfaceSerialDataBitsTextBox,
            InterfaceSerialReadTimeoutTextBox,
            InterfaceSerialWriteTimeoutTextBox
        })
        {
            textBox.TextChanged += textChangedHandler;
        }

        RoutedEventHandler routedHandler = (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceActivationStatusButton.Checked += routedHandler;
        InterfaceActivationStatusButton.Unchecked += routedHandler;
        InterfaceDeviceOutputEnabledCheckBox.Checked += routedHandler;
        InterfaceDeviceOutputEnabledCheckBox.Unchecked += routedHandler;
        InterfaceAttachmentProcessingEnabledCheckBox.Checked += routedHandler;
        InterfaceAttachmentProcessingEnabledCheckBox.Unchecked += routedHandler;
        InterfaceAttachmentShowDocumentationDialogCheckBox.Checked += routedHandler;
        InterfaceAttachmentShowDocumentationDialogCheckBox.Unchecked += routedHandler;
        InterfaceSerialBidirectionalCheckBox.Checked += routedHandler;
        InterfaceSerialBidirectionalCheckBox.Unchecked += routedHandler;
        InterfaceSerialDtrCheckBox.Checked += routedHandler;
        InterfaceSerialDtrCheckBox.Unchecked += routedHandler;
        InterfaceSerialRtsCheckBox.Checked += routedHandler;
        InterfaceSerialRtsCheckBox.Unchecked += routedHandler;
        InterfaceClearAisImportFolderCheckBox.Checked += routedHandler;
        InterfaceClearAisImportFolderCheckBox.Unchecked += routedHandler;
        InterfaceClearDeviceImportFolderCheckBox.Checked += routedHandler;
        InterfaceClearDeviceImportFolderCheckBox.Unchecked += routedHandler;
        InterfaceArchiveProcessedFilesCheckBox.Checked += routedHandler;
        InterfaceArchiveProcessedFilesCheckBox.Unchecked += routedHandler;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.Checked += routedHandler;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.Unchecked += routedHandler;

        InterfaceAttachmentTransferModeComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceAttachmentRequirementModeComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceDeviceOutputFormatComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceArchiveModeComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceSerialPortComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceSerialStopBitsComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceSerialParityComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceSerialHandshakeComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceNidekRtSerialSendModeComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
        InterfaceNidekRtSerialFrameVariantComboBox.SelectionChanged += (_, _) => RefreshInterfaceActivationPreviewForDraftChange();
    }

    private void RefreshInterfaceActivationPreviewForDraftChange()
    {
        if (!IsLoaded || InterfaceProfileComboBox.SelectedItem is null || _profileCatalog is null)
        {
            return;
        }

        RefreshInterfaceActivationPreview();
    }

    private void InitializeTrayIcon()
    {
        var openItem = new WinForms.ToolStripMenuItem("Öffnen");
        openItem.Click += TrayOpenMenuItem_Click;
        var exitItem = new WinForms.ToolStripMenuItem("Beenden");
        exitItem.Click += TrayExitMenuItem_Click;

        _trayContextMenu = new WinForms.ContextMenuStrip();
        _trayContextMenu.Items.Add(openItem);
        _trayContextMenu.Items.Add(exitItem);

        _trayIcon = new WinForms.NotifyIcon
        {
            Text = "XdtDeviceBridge - XDT Verwaltung",
            Icon = LoadNotifyIcon(),
            ContextMenuStrip = _trayContextMenu,
            Visible = true
        };
        _trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
    }

    private static System.Drawing.Icon LoadNotifyIcon()
    {
        try
        {
            var resourceInfo = System.Windows.Application.GetResourceStream(new Uri(AppIconResourcePath, UriKind.Relative));
            if (resourceInfo?.Stream is not null)
            {
                using var stream = resourceInfo.Stream;
                using var icon = new System.Drawing.Icon(stream);
                return (System.Drawing.Icon)icon.Clone();
            }
        }
        catch
        {
            // Fallback keeps the tray usable even if the resource cannot be loaded.
        }

        return System.Drawing.SystemIcons.Application;
    }

    private void MinimizeMainWindowToTray()
    {
        var decision = _trayWindowStateService.MinimizeToTray();
        if (_trayIcon is null)
        {
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
            return;
        }

        if (decision.ShouldHideWindow)
        {
            ShowInTaskbar = false;
            Hide();
        }

        if (decision.ShouldShowHint)
        {
            ShowTrayHint();
        }
    }

    private void RestoreMainWindowFromTray()
    {
        var decision = _trayWindowStateService.RestoreFromTray();
        if (!decision.ShouldShowWindow)
        {
            return;
        }

        ShowInTaskbar = true;
        if (!IsVisible)
        {
            Show();
        }

        WindowState = WindowState.Normal;
        _ = Activate();
    }

    private void RequestApplicationExit()
    {
        var decision = _trayWindowStateService.RequestExit();
        if (!decision.ShouldExit)
        {
            return;
        }

        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
        }

        Close();
    }

    private void ShowTrayHint()
    {
        const string message = "XDTBox läuft im Infobereich weiter. Über das Symbol neben der Uhr kann das Fenster wieder geöffnet oder die App beendet werden.";
        try
        {
            _trayIcon?.ShowBalloonTip(4000, "XDTBox", message, WinForms.ToolTipIcon.Info);
        }
        catch
        {
            AppendMessage(message);
        }
    }

    private void DisposeTrayIcon()
    {
        if (_trayIcon is not null)
        {
            _trayIcon.MouseDoubleClick -= TrayIcon_MouseDoubleClick;
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _trayContextMenu?.Dispose();
        _trayContextMenu = null;
    }

    private void TrayIcon_MouseDoubleClick(object? sender, WinForms.MouseEventArgs e)
    {
        if (e.Button == WinForms.MouseButtons.Left)
        {
            Dispatcher.BeginInvoke((Action)RestoreMainWindowFromTray);
        }
    }

    private void TrayOpenMenuItem_Click(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke((Action)RestoreMainWindowFromTray);
    }

    private void TrayExitMenuItem_Click(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke((Action)RequestApplicationExit);
    }

    private void AppExitButton_Click(object sender, RoutedEventArgs e)
    {
        RequestApplicationExit();
    }

    private void InitializeProfileOverview()
    {
        try
        {
            var (paths, catalog) = LoadProfileCatalogForUi();
            InitializeProfileDependentTabs(catalog);
            AppendProfileMessage($"Profile geladen. AIS: {catalog.AisProfiles.Count}, Geräte: {catalog.DeviceProfiles.Count}, Export: {catalog.ExportProfiles.Count}, Schnittstellen: {catalog.InterfaceProfiles.Count}. Profilordner: {paths.BaseFolder}");
        }
        catch (Exception ex)
        {
            _profileCatalog = null;
            ClearProfileDependentTabsOnProfileLoadFailure();
            AppendProfileMessage($"V2-Profile konnten nicht geladen werden: {ex.Message}");
        }
    }

    private (AppDataPaths Paths, ProfileCatalog Catalog) LoadProfileCatalogForUi()
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        _profileCatalogService.EnsureDefaultProfiles(paths);
        var catalog = _profileCatalogService.Load(paths);
        _profileCatalog = catalog;
        return (paths, catalog);
    }

    private void InitializeProfileDependentTabs(
        ProfileCatalog catalog,
        string? selectedInterfaceProfileId = null,
        string? selectedAisProfileId = null,
        string? selectedDeviceProfileId = null,
        string? selectedExportProfileId = null)
    {
        InitializeProfileManagementTab(catalog);
        InitializeInterfaceProfileConfiguration(catalog, selectedInterfaceProfileId);
        InitializeXdtBaukasten(catalog, selectedAisProfileId, selectedDeviceProfileId, selectedExportProfileId);
    }

    private void ClearProfileDependentTabsOnProfileLoadFailure()
    {
        ClearProfileManagementTabOnProfileLoadFailure();
        InterfaceProfileComboBox.ItemsSource = null;
        ClearInterfaceProfileEditor();
        ClearActiveInterfaceProfilesOverview("Aktive Schnittstellenprofile konnten nicht geladen werden.");
        XdtBaukastenAisProfileComboBox.ItemsSource = null;
        XdtBaukastenDeviceProfileComboBox.ItemsSource = null;
        XdtBaukastenExportProfileComboBox.ItemsSource = null;
        _xdtBaukastenExportRules.Clear();
        ClearXdtBaukastenDeviceIdentity();
        XdtBaukastenStatusText.Text = "Keine Profile geladen.";
        XdtBaukastenTopStatusText.Text = "Keine Profile geladen.";
    }

    private void InitializeProfileManagementTab(ProfileCatalog catalog)
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        _profileManagementAllRows = _profileManagementService.BuildRows(
            catalog,
            paths,
            _xdtBaukastenTemplateLibraryService);
        RefreshProfileManagementRows();
        ProfileManagementStatusText.Text =
            $"Profilverwaltung geladen: {_profileManagementAllRows.Count} Einträge. BuiltIn-Profile bleiben geschützt.";
    }

    private void ClearProfileManagementTabOnProfileLoadFailure()
    {
        _profileManagementAllRows = Array.Empty<ProfileManagementRow>();
        _profileManagementRows.Clear();
        ProfileManagementDetailsTextBox.Text = "Keine Profile geladen.";
        ProfileManagementStatusText.Text = "Keine Profile geladen.";
        UpdateProfileManagementActionButtons();
    }

    private ProfileManagementRow? SelectedProfileManagementRow =>
        ProfileManagementGrid?.SelectedItem as ProfileManagementRow;

    private bool IsProfileManagementUiReady =>
        ProfileManagementSearchTextBox is not null
        && ProfileManagementScopeFilterComboBox is not null
        && ProfileManagementKindFilterComboBox is not null
        && ProfileManagementGrid is not null
        && ProfileManagementDetailsTextBox is not null
        && ProfileManagementStatusText is not null
        && ProfileManagementNewAisButton is not null
        && ProfileManagementLoadDeviceButton is not null
        && ProfileManagementNewExportButton is not null
        && ProfileManagementNewInterfaceButton is not null
        && ProfileManagementNewTemplateButton is not null
        && ProfileManagementImportTemplatePackageButton is not null
        && ProfileManagementExportTemplateButton is not null
        && ProfileManagementOpenWorkbenchButton is not null
        && ProfileManagementOpenInterfaceButton is not null
        && ProfileManagementRenameButton is not null
        && ProfileManagementDuplicateButton is not null
        && ProfileManagementDeleteButton is not null;

    private void ProfileManagementFilter_Changed(object sender, EventArgs e)
    {
        if (!IsProfileManagementUiReady)
        {
            return;
        }

        RefreshProfileManagementRows();
    }

    private void ProfileManagementGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsProfileManagementUiReady)
        {
            return;
        }

        UpdateProfileManagementActionButtons();
    }

    private void RefreshProfileManagementRows()
    {
        if (!IsProfileManagementUiReady)
        {
            return;
        }

        var selected = SelectedProfileManagementRow;
        var search = ProfileManagementSearchTextBox.Text.Trim();
        var scope = GetSelectedComboBoxTag(ProfileManagementScopeFilterComboBox);
        var kind = GetSelectedComboBoxTag(ProfileManagementKindFilterComboBox);

        var rows = _profileManagementAllRows.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            rows = rows.Where(row =>
                row.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || row.Id.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.Type.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || row.Owner.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || row.UsedBy.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!scope.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            rows = rows.Where(row => row.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase));
        }

        if (!kind.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            rows = rows.Where(row => row.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase));
        }

        var filteredRows = rows.ToList();
        _profileManagementRows.Clear();
        foreach (var row in filteredRows)
        {
            _profileManagementRows.Add(row);
        }

        if (selected is not null)
        {
            ProfileManagementGrid.SelectedItem = _profileManagementRows.FirstOrDefault(row =>
                row.Kind == selected.Kind && row.Id.Equals(selected.Id, StringComparison.OrdinalIgnoreCase));
        }

        if (ProfileManagementGrid.SelectedItem is null && _profileManagementRows.Count > 0)
        {
            ProfileManagementGrid.SelectedItem = _profileManagementRows[0];
        }

        ProfileManagementStatusText.Text = $"{_profileManagementRows.Count} von {_profileManagementAllRows.Count} Einträgen sichtbar.";
        UpdateProfileManagementActionButtons();
    }

    private void UpdateProfileManagementActionButtons()
    {
        if (!IsProfileManagementUiReady)
        {
            return;
        }

        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementDetailsTextBox.Text = "Kein Eintrag ausgewählt.";
            ProfileManagementOpenWorkbenchButton.IsEnabled = false;
            ProfileManagementOpenInterfaceButton.IsEnabled = false;
            ProfileManagementRenameButton.IsEnabled = false;
            ProfileManagementDuplicateButton.IsEnabled = false;
            ProfileManagementDeleteButton.IsEnabled = false;
            ProfileManagementExportTemplateButton.IsEnabled = false;
            return;
        }

        var details = new StringBuilder(row.Details);
        if (_profileCatalog is not null)
        {
            var deleteEvaluation = _profileManagementService.EvaluateDelete(_profileCatalog, row);
            details.AppendLine();
            details.AppendLine();
            details.AppendLine($"Löschen: {deleteEvaluation.Message}");
        }

        ProfileManagementDetailsTextBox.Text = details.ToString();
        ProfileManagementOpenWorkbenchButton.IsEnabled = row.CanOpenInWorkbench;
        ProfileManagementOpenInterfaceButton.IsEnabled = row.CanOpenInInterfaceProfiles;
        ProfileManagementRenameButton.IsEnabled = row.CanRename;
        ProfileManagementDuplicateButton.IsEnabled = row.CanDuplicate;
        ProfileManagementDeleteButton.IsEnabled = row.CanDelete;
        ProfileManagementExportTemplateButton.IsEnabled =
            row.Kind is ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate or ProfileManagementRowKind.InterfaceProfile;
    }

    private void RefreshProfileManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.EnsureDefaultProfiles(paths);
            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(catalog);
            ProfileManagementStatusText.Text = "Profile wurden neu geladen. BuiltIn-Reparatur hat UserDefined-Profile unverändert gelassen.";
            AppendProfileMessage("Profilverwaltung neu geladen. BuiltIn-Reparatur hat UserDefined-Profile nicht überschrieben.");
        }
        catch (Exception ex)
        {
            ProfileManagementStatusText.Text = $"Profile konnten nicht neu geladen werden: {ex.Message}";
            AppendProfileMessage($"Profile konnten nicht neu geladen werden: {ex.Message}");
        }
    }

    private void ProfileManagementRepairBuiltIns_Click(object sender, RoutedEventArgs e)
    {
        RefreshProfileManagement_Click(sender, e);
    }

    private void ProfileManagementNewAis_Click(object sender, RoutedEventArgs e)
    {
        CreateNewAisProfile_Click(sender, e);
        ProfileManagementStatusText.Text = "AIS-Neuanlage wurde über die Profilverwaltung gestartet.";
    }

    private void ProfileManagementNewDevice_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDeviceProfile_Click(sender, e);
        ProfileManagementStatusText.Text = "Geräte-Neuanlage wurde über die Profilverwaltung gestartet.";
    }

    private void ProfileManagementLoadDevice_Click(object sender, RoutedEventArgs e)
    {
        LoadDeviceProfile_Click(sender, e);
        ProfileManagementStatusText.Text = "Gerätedialog geöffnet. Gerätebilder werden als lokale Overrides gepflegt.";
    }

    private void ProfileManagementNewExport_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var sourceRow = SelectedProfileManagementRow?.Kind == ProfileManagementRowKind.ExportProfile
            ? SelectedProfileManagementRow
            : _profileManagementAllRows.FirstOrDefault(row => row.Kind == ProfileManagementRowKind.ExportProfile);
        if (sourceRow is null)
        {
            ProfileManagementStatusText.Text = "Es ist kein Exportprofil als Vorlage vorhanden.";
            AppendProfileMessage("Neues Exportprofil konnte nicht angelegt werden: keine Vorlage vorhanden.");
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var result = _profileManagementService.Duplicate(catalog, paths, sourceRow, DateTimeOffset.UtcNow, Environment.UserName);
            if (!result.Success)
            {
                ProfileManagementStatusText.Text = result.Message;
                return;
            }

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog, selectedExportProfileId: result.ExportProfileId);
            SelectMainTabByHeader("XDT-Baukasten");
            ProfileManagementStatusText.Text = $"{result.Message} Die neue Arbeitskopie kann im XDT-Baukasten bearbeitet werden.";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Exportprofil konnte nicht angelegt werden: {ex.Message}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
    }

    private void ProfileManagementNewInterface_Click(object sender, RoutedEventArgs e)
    {
        CreateNewInterfaceProfile_Click(sender, e);
        ProfileManagementStatusText.Text = "Schnittstellenprofil-Neuanlage wurde über die Profilverwaltung gestartet.";
    }

    private void ProfileManagementNewTemplate_Click(object sender, RoutedEventArgs e)
    {
        SelectMainTabByHeader("XDT-Baukasten");
        XdtBaukastenSaveTemplate_Click(sender, e);
        ProfileManagementStatusText.Text = "Baukasten-Template-Erstellung wurde in den XDT-Baukasten übergeben.";
    }

    private void ProfileManagementImportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        SelectMainTabByHeader("XDT-Baukasten");
        XdtBaukastenImportTemplatePackage_Click(sender, e);
        ProfileManagementStatusText.Text = "Templatepaket-Import wird im Baukasten geprüft. BuiltIns werden nicht überschrieben.";
    }

    private void ProfileManagementExportTemplate_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst ein Template, Paket oder Schnittstellenprofil auswählen.";
            return;
        }

        if (row.Kind is ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate)
        {
            ExportLocalProfileManagementFile(row);
            return;
        }

        if (row.Kind == ProfileManagementRowKind.InterfaceProfile)
        {
            ExportTemplatePackageForInterfaceProfile(row.Id);
            return;
        }

        ProfileManagementStatusText.Text = "Dieser Eintrag kann nicht als Template/Paket exportiert werden.";
    }

    private void ExportLocalProfileManagementFile(ProfileManagementRow row)
    {
        if (string.IsNullOrWhiteSpace(row.FilePath) || !File.Exists(row.FilePath))
        {
            ProfileManagementStatusText.Text = "Die lokale Template-Datei wurde nicht gefunden.";
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Template/Paket exportieren",
            Filter = row.Kind == ProfileManagementRowKind.XdtBaukastenTemplate
                ? "Baukasten-Template (*.json)|*.json|Alle Dateien (*.*)|*.*"
                : "Templatepaket (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
            FileName = Path.GetFileName(row.FilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            File.Copy(row.FilePath, dialog.FileName, overwrite: true);
            ProfileManagementStatusText.Text = $"Template/Paket exportiert: {dialog.FileName}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Template/Paket konnte nicht exportiert werden: {ex.Message}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
    }

    private void ExportTemplatePackageForInterfaceProfile(string interfaceProfileId)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var selection = _templatePackageExportSelectionService.CreateForInterfaceProfile(catalog, interfaceProfileId, DateTimeOffset.UtcNow);
        if (!selection.Success || selection.Request is null)
        {
            ProfileManagementStatusText.Text = selection.ErrorMessage ?? "Templatepaket konnte für dieses Schnittstellenprofil nicht vorbereitet werden.";
            AppendProfileMessage(ProfileManagementStatusText.Text);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Templatepaket exportieren",
            Filter = "Templatepaket (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
            FileName = selection.SuggestedFileName
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            _templatePackageExporter.Export(dialog.FileName, selection.Request);
            ProfileManagementStatusText.Text = $"Templatepaket exportiert: {dialog.FileName}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Templatepaket konnte nicht exportiert werden: {ex.Message}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
    }

    private void ProfileManagementOpenWorkbench_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst einen Eintrag auswählen.";
            return;
        }

        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        if (row.Kind == ProfileManagementRowKind.XdtBaukastenTemplate && !string.IsNullOrWhiteSpace(row.FilePath))
        {
            try
            {
                var template = _xdtBaukastenTemplateLibraryService.Load(row.FilePath);
                LoadXdtBaukastenTemplate(template);
                SelectMainTabByHeader("XDT-Baukasten");
                ProfileManagementStatusText.Text = $"Baukasten-Template geladen: {template.Name}.";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
            {
                ProfileManagementStatusText.Text = $"Baukasten-Template konnte nicht geladen werden: {ex.Message}";
            }

            return;
        }

        if (row.Kind == ProfileManagementRowKind.TemplatePackage)
        {
            SelectMainTabByHeader("XDT-Baukasten");
            SetXdtBaukastenStatus("Templatepakete werden im Baukasten über 'Template Paket importieren' geprüft und importiert.");
            ProfileManagementStatusText.Text = "Templatepaket-Import im Baukasten geöffnet. Import prüft Konflikte vor dem Speichern.";
            return;
        }

        RefreshProfileOverview(
            catalog,
            selectedExportProfileId: row.Kind == ProfileManagementRowKind.ExportProfile ? row.Id : null,
            selectedAisProfileId: row.Kind == ProfileManagementRowKind.AisProfile ? row.Id : null,
            selectedDeviceProfileId: row.Kind == ProfileManagementRowKind.DeviceProfile ? row.Id : null);
        SelectMainTabByHeader("XDT-Baukasten");
        ProfileManagementStatusText.Text = $"Eintrag im XDT-Baukasten ausgewählt: {row.Name}.";
    }

    private void ProfileManagementOpenInterface_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst einen Eintrag auswählen.";
            return;
        }

        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        RefreshProfileOverview(catalog, selectedInterfaceProfileId: row.Id);
        SelectMainTabByHeader("Schnittstellenprofile");
        ProfileManagementStatusText.Text = $"Schnittstellenprofil geöffnet: {row.Name}.";
    }

    private void ProfileManagementRename_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst einen Eintrag auswählen.";
            return;
        }

        if (row.Kind is ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate)
        {
            RenameLocalTemplateEntry(row);
            return;
        }

        var renameKind = ToUserDefinedProfileRenameKind(row.Kind);
        if (renameKind is null)
        {
            ProfileManagementStatusText.Text = "Dieser Eintrag kann nicht umbenannt werden.";
            return;
        }

        RenameProfile(
            renameKind.Value,
            row.Id,
            row.Name,
            selectedAisProfileId: row.Kind == ProfileManagementRowKind.AisProfile ? row.Id : null,
            selectedDeviceProfileId: row.Kind == ProfileManagementRowKind.DeviceProfile ? row.Id : null,
            selectedExportProfileId: row.Kind == ProfileManagementRowKind.ExportProfile ? row.Id : null,
            selectedInterfaceProfileId: row.Kind == ProfileManagementRowKind.InterfaceProfile ? row.Id : null);
    }

    private void RenameLocalTemplateEntry(ProfileManagementRow row)
    {
        if (string.IsNullOrWhiteSpace(row.FilePath) || !File.Exists(row.FilePath))
        {
            ProfileManagementStatusText.Text = "Die lokale Template-Datei wurde nicht gefunden.";
            return;
        }

        var dialog = new RenameProfileDialog(row.Name)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            if (row.Kind == ProfileManagementRowKind.XdtBaukastenTemplate)
            {
                RenameXdtBaukastenTemplate(row.FilePath, dialog.NewName);
            }
            else
            {
                RenameTemplatePackageFile(row.FilePath, dialog.NewName);
            }

            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(catalog);
            ProfileManagementStatusText.Text = $"Lokaler Template-Eintrag umbenannt: {dialog.NewName}.";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Lokaler Template-Eintrag konnte nicht umbenannt werden: {ex.Message}";
            AppendProfileMessage(ProfileManagementStatusText.Text);
        }
    }

    private void RenameXdtBaukastenTemplate(string filePath, string newName)
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        var template = _xdtBaukastenTemplateLibraryService.Load(filePath);
        var updatedTemplate = template with
        {
            Name = newName,
            SavedAt = DateTimeOffset.UtcNow,
            SavedBy = Environment.UserName
        };
        var targetPath = _xdtBaukastenTemplateLibraryService.CreateDefaultFilePath(paths, newName);
        if (!string.Equals(Path.GetFullPath(filePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase)
            && File.Exists(targetPath))
        {
            throw new InvalidOperationException("Es existiert bereits ein Baukasten-Template mit diesem Dateinamen.");
        }

        _xdtBaukastenTemplateLibraryService.Save(targetPath, updatedTemplate, overwriteExisting: true);
        if (!string.Equals(Path.GetFullPath(filePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(filePath);
        }
    }

    private static void RenameTemplatePackageFile(string filePath, string newName)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(filePath))
            ?? throw new InvalidOperationException("Templatepaket-Ordner konnte nicht ermittelt werden.");
        var targetPath = Path.Combine(folder, TemplatePackageExportSelectionService.CreateSafeTemplatePackageFileName(newName));
        if (!string.Equals(Path.GetFullPath(filePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase)
            && File.Exists(targetPath))
        {
            throw new InvalidOperationException("Es existiert bereits ein Templatepaket mit diesem Dateinamen.");
        }

        File.Move(filePath, targetPath, overwrite: true);
    }

    private void ProfileManagementDuplicate_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst einen Eintrag auswählen.";
            return;
        }

        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var result = _profileManagementService.Duplicate(catalog, paths, row, DateTimeOffset.UtcNow, Environment.UserName);
            if (!result.Success)
            {
                ProfileManagementStatusText.Text = result.Message;
                return;
            }

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(
                updatedCatalog,
                selectedExportProfileId: result.ExportProfileId,
                selectedInterfaceProfileId: result.InterfaceProfileId,
                selectedAisProfileId: result.AisProfileId,
                selectedDeviceProfileId: result.DeviceProfileId);
            ProfileManagementStatusText.Text = result.Message;
            AppendProfileMessage(result.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Profil konnte nicht dupliziert werden: {ex.Message}";
            AppendProfileMessage($"Profil konnte nicht dupliziert werden: {ex.Message}");
        }
    }

    private void ProfileManagementDelete_Click(object sender, RoutedEventArgs e)
    {
        var row = SelectedProfileManagementRow;
        if (row is null)
        {
            ProfileManagementStatusText.Text = "Bitte zuerst einen Eintrag auswählen.";
            return;
        }

        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var evaluation = _profileManagementService.EvaluateDelete(catalog, row);
        if (!evaluation.Success)
        {
            ProfileManagementStatusText.Text = evaluation.Message;
            System.Windows.MessageBox.Show(
                this,
                evaluation.Message,
                "Profilverwaltung",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var confirmation = System.Windows.MessageBox.Show(
            this,
            $"{row.Type} wirklich löschen?{Environment.NewLine}{row.Name}{Environment.NewLine}{Environment.NewLine}BuiltIn-Profile und verwendete Profile bleiben geschützt.",
            "Profilverwaltung",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var result = _profileManagementService.Delete(catalog, paths, row);
            if (!result.Success)
            {
                ProfileManagementStatusText.Text = result.Message;
                return;
            }

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog);
            ProfileManagementStatusText.Text = result.Message;
            AppendProfileMessage(result.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            ProfileManagementStatusText.Text = $"Eintrag konnte nicht gelöscht werden: {ex.Message}";
            AppendProfileMessage($"Eintrag konnte nicht gelöscht werden: {ex.Message}");
        }
    }

    private static string GetSelectedComboBoxTag(System.Windows.Controls.ComboBox comboBox)
    {
        return comboBox.SelectedItem is ComboBoxItem item && item.Tag is not null
            ? item.Tag.ToString() ?? "All"
            : "All";
    }

    private static UserDefinedProfileRenameKind? ToUserDefinedProfileRenameKind(ProfileManagementRowKind kind)
    {
        return kind switch
        {
            ProfileManagementRowKind.AisProfile => UserDefinedProfileRenameKind.AisProfile,
            ProfileManagementRowKind.DeviceProfile => UserDefinedProfileRenameKind.DeviceProfile,
            ProfileManagementRowKind.ExportProfile => UserDefinedProfileRenameKind.ExportProfile,
            ProfileManagementRowKind.InterfaceProfile => UserDefinedProfileRenameKind.InterfaceProfile,
            _ => null
        };
    }

    private void SelectMainTabByHeader(string header)
    {
        foreach (var tabItem in MainTabControl.Items.OfType<TabItem>())
        {
            if (string.Equals(tabItem.Header?.ToString(), header, StringComparison.OrdinalIgnoreCase))
            {
                MainTabControl.SelectedItem = tabItem;
                return;
            }
        }
    }

    private void InitializeInterfaceProfileConfiguration(ProfileCatalog catalog, string? selectedInterfaceProfileId = null)
    {
        PopulateInterfaceProfileFilters(catalog);
        RefreshInterfaceProfileSelection(selectedInterfaceProfileId);
    }

    private void PopulateInterfaceProfileFilters(ProfileCatalog catalog)
    {
        _isUpdatingInterfaceProfileFilters = true;
        try
        {
            var previousManufacturer = InterfaceManufacturerFilterComboBox.SelectedItem as string;
            var previousAis = InterfaceAisFilterComboBox.SelectedItem as string;

            var manufacturerOptions = catalog.InterfaceProfiles
                .Select(profile => GetDeviceProfile(catalog, profile.DeviceProfileId)?.Manufacturer)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .Prepend(InterfaceProfileFilterAll)
                .ToArray();

            var aisOptions = catalog.InterfaceProfiles
                .Select(profile => GetAisProfile(catalog, profile.AisProfileId)?.Name ?? profile.AisProfileId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .Prepend(InterfaceProfileFilterAll)
                .ToArray();

            InterfaceManufacturerFilterComboBox.ItemsSource = manufacturerOptions;
            InterfaceAisFilterComboBox.ItemsSource = aisOptions;
            InterfaceManufacturerFilterComboBox.SelectedItem = manufacturerOptions.FirstOrDefault(option =>
                string.Equals(option, previousManufacturer, StringComparison.CurrentCultureIgnoreCase))
                ?? InterfaceProfileFilterAll;
            InterfaceAisFilterComboBox.SelectedItem = aisOptions.FirstOrDefault(option =>
                string.Equals(option, previousAis, StringComparison.CurrentCultureIgnoreCase))
                ?? InterfaceProfileFilterAll;
        }
        finally
        {
            _isUpdatingInterfaceProfileFilters = false;
        }
    }

    private void InterfaceProfileFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isUpdatingInterfaceProfileFilters)
        {
            return;
        }

        var selectedId = InterfaceProfileComboBox.SelectedItem is InterfaceProfileDefinition profile
            ? profile.Metadata.Id
            : null;
        RefreshInterfaceProfileSelection(selectedId);
    }

    private void RefreshInterfaceProfileSelection(string? selectedInterfaceProfileId = null)
    {
        if (_profileCatalog is null)
        {
            InterfaceProfileComboBox.ItemsSource = Array.Empty<InterfaceProfileDefinition>();
            InterfaceProfileComboBox.SelectedIndex = -1;
            ClearInterfaceProfileEditor();
            return;
        }

        var interfaceProfiles = GetFilteredInterfaceProfiles(_profileCatalog);

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

    private IReadOnlyList<InterfaceProfileDefinition> GetFilteredInterfaceProfiles(ProfileCatalog catalog)
    {
        var manufacturerFilter = InterfaceManufacturerFilterComboBox.SelectedItem as string;
        var aisFilter = InterfaceAisFilterComboBox.SelectedItem as string;

        return catalog.InterfaceProfiles
            .Where(profile => MatchesInterfaceManufacturerFilter(catalog, profile, manufacturerFilter))
            .Where(profile => MatchesInterfaceAisFilter(catalog, profile, aisFilter))
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static bool MatchesInterfaceManufacturerFilter(
        ProfileCatalog catalog,
        InterfaceProfileDefinition profile,
        string? manufacturerFilter)
    {
        if (string.IsNullOrWhiteSpace(manufacturerFilter)
            || string.Equals(manufacturerFilter, InterfaceProfileFilterAll, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }

        var manufacturer = GetDeviceProfile(catalog, profile.DeviceProfileId)?.Manufacturer;
        return string.Equals(manufacturer, manufacturerFilter, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool MatchesInterfaceAisFilter(
        ProfileCatalog catalog,
        InterfaceProfileDefinition profile,
        string? aisFilter)
    {
        if (string.IsNullOrWhiteSpace(aisFilter)
            || string.Equals(aisFilter, InterfaceProfileFilterAll, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }

        var aisName = GetAisProfile(catalog, profile.AisProfileId)?.Name ?? profile.AisProfileId;
        return string.Equals(aisName, aisFilter, StringComparison.CurrentCultureIgnoreCase);
    }

    private static AisProfile? GetAisProfile(ProfileCatalog catalog, string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        return catalog.AisProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private static DeviceProfileDefinition? GetDeviceProfile(ProfileCatalog catalog, string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        return catalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private void InterfaceProfileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ShowInterfaceProfileForSelectedProfile();
        UpdateProfileRenameActionButtons();
    }

    private void UpdateProfileRenameActionButtons()
    {
        UpdateProfileRenameButton(
            RenameInterfaceProfileButton,
            InterfaceProfileComboBox.SelectedItem is InterfaceProfileDefinition interfaceProfile ? interfaceProfile.Metadata : null,
            "Schnittstellenprofil");
    }

    private void ShowAisOutputInfo_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Profilkatalog ist nicht geladen.",
                "AIS Ausgabe Info",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition interfaceProfile)
        {
            System.Windows.MessageBox.Show(
                this,
                "Bitte zuerst ein Schnittstellenprofil auswählen.",
                "AIS Ausgabe Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var info = _aisOutputInfoService.Create(_profileCatalog, interfaceProfile);
        var window = new AisOutputInfoWindow(info)
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void ShowInterfaceDeviceTechnicalProfile_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition interfaceProfile)
        {
            ShowDeviceTechnicalProfileMissingMessage("Bitte zuerst ein Schnittstellenprofil auswählen.");
            return;
        }

        ShowDeviceTechnicalProfile(interfaceProfile.DeviceProfileId);
    }

    private void ShowDeviceTechnicalProfile(string? deviceProfileId)
    {
        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            ShowDeviceTechnicalProfileMissingMessage("Bitte zuerst ein Geräteprofil auswählen.");
            return;
        }

        try
        {
            var service = CreateDeviceTechnicalProfileService();
            if (service.LoadProfile(deviceProfileId) is null)
            {
                ShowDeviceTechnicalProfileMissingMessage("Für dieses Geräteprofil ist noch kein Gerätesteckbrief hinterlegt.");
                return;
            }

            var window = new DeviceTechnicalProfileWindow(service, deviceProfileId)
            {
                Owner = this
            };
            window.ShowDialog();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            ShowDeviceTechnicalProfileMissingMessage($"Der Gerätesteckbrief konnte nicht geöffnet werden: {ex.Message}");
        }
    }

    private DeviceTechnicalProfileService CreateDeviceTechnicalProfileService()
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        return new DeviceTechnicalProfileService(
            () => System.Windows.Application.GetResourceStream(new Uri("Assets/DeviceInfo/device-technical-profiles.de.json", UriKind.Relative))?.Stream,
            DeviceTechnicalProfileService.GetDefaultOverrideFilePath(paths));
    }

    private void ShowDeviceTechnicalProfileMissingMessage(string message)
    {
        System.Windows.MessageBox.Show(
            this,
            message,
            "Gerätesteckbrief",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void InterfaceActivationStatusButton_Changed(object sender, RoutedEventArgs e)
    {
        RefreshInterfaceActivationPreviewForDraftChange();
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
            ShowInterfaceActivationPreparationPreview(preview);
            return;
        }

        if (_profileCatalog is null)
        {
            preview = _interfaceProfileActivationPreparationPreviewService.CreateError("Profilkatalog ist nicht geladen.");
            ShowInterfaceActivationPreparationPreview(preview);
            return;
        }

        try
        {
            var previewProfile = CreateInterfaceProfileDraftForActivationPreview(profile);
            var result = _interfaceProfileActivationEvaluationService.Evaluate(
                previewProfile,
                _profileCatalog,
                CreateLicenseStatesForActivationPreview());
            var guardResult = _interfaceProfileActivationGuardService.ValidateActivationRequest(
                new InterfaceProfileActivationRequest(
                    previewProfile,
                    result,
                    Context: "PreviewOnly"));
            preview = _interfaceProfileActivationPreparationPreviewService.Create(
                previewProfile,
                result,
                guardResult);
            ShowInterfaceActivationPreparationPreview(preview);
        }
        catch (Exception ex)
        {
            preview = _interfaceProfileActivationPreparationPreviewService.CreateError(ex.Message);
            ShowInterfaceActivationPreparationPreview(preview);
        }
    }

    private void ShowInterfaceActivationPreparationPreview(
        InterfaceProfileActivationPreparationPreview preview)
    {
        var window = new InterfaceProfileActivationPreparationPreviewWindow(preview)
        {
            Owner = this
        };
        window.ShowDialog();
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
        InterfaceActivationStatusButton.IsChecked = profile.IsActive;
        UpdateInterfaceDeviceProfileVisual(profile);

        InterfaceAisImportFolderTextBox.Text = profile.FolderOptions.AisImportFolder;
        InterfaceDeviceImportFolderTextBox.Text = profile.FolderOptions.DeviceImportFolder;
        InterfaceExportFolderTextBox.Text = profile.FolderOptions.ExportFolder;
        InterfaceArchiveFolderTextBox.Text = profile.FolderOptions.ArchiveFolder;
        InterfaceErrorFolderTextBox.Text = profile.FolderOptions.ErrorFolder;
        InterfaceDeviceOutputEnabledCheckBox.IsChecked = profile.DeviceOutput?.IsEnabled == true;
        InterfaceDeviceOutputFolderTextBox.Text = profile.DeviceOutput?.OutputFolder ?? string.Empty;
        InterfaceDeviceOutputFileNameTextBox.Text = string.IsNullOrWhiteSpace(profile.DeviceOutput?.FileNameTemplate)
            ? "CVImport.xml"
            : profile.DeviceOutput!.FileNameTemplate;
        InterfaceDeviceOutputFormatComboBox.SelectedValue = string.IsNullOrWhiteSpace(profile.DeviceOutput?.Format)
            ? "TOPCON CV-5000 XML"
            : profile.DeviceOutput!.Format;
        InterfaceAutoImportScanIntervalSecondsTextBox.Text = profile.FolderOptions.AutoImportScanIntervalSeconds.ToString();
        InterfaceDeviceFileWaitTimeoutMinutesTextBox.Text = profile.FolderOptions.DeviceFileWaitTimeoutMinutes.ToString();
        ApplySerialSettingsToInterfaceEditor(GetSerialSettingsForProfile(profile));
        ApplyNidekRtSerialSendModeToInterfaceEditor(profile);
        ApplyNidekRtSerialOutputFrameVariantToInterfaceEditor(profile);
        InterfaceAttachmentImportFolderTextBox.Text = profile.FolderOptions.AttachmentImportFolder;
        InterfaceAttachmentExportFolderTextBox.Text = profile.FolderOptions.AttachmentExportFolder;
        InterfaceAttachmentFileNameTemplateTextBox.Text = profile.FolderOptions.AttachmentFileNameTemplate ?? string.Empty;
        InterfaceAttachmentTransferModeComboBox.SelectedValue = profile.FolderOptions.AttachmentTransferMode.ToString();
        InterfaceAttachmentProcessingEnabledCheckBox.IsChecked = profile.FolderOptions.IsAttachmentProcessingEnabled;
        InterfaceAttachmentRequirementModeComboBox.SelectedValue = profile.FolderOptions.AttachmentRequirementMode.ToString();
        InterfaceAttachmentWaitTimeoutSecondsTextBox.Text = profile.FolderOptions.AttachmentWaitTimeoutSeconds.ToString();
        InterfaceAttachmentFileStabilityWaitSecondsTextBox.Text = profile.FolderOptions.AttachmentFileStabilityWaitSeconds.ToString();
        InterfaceAttachmentCompletionModeComboBox.SelectedValue = profile.FolderOptions.AttachmentCompletionMode.ToString();
        InterfaceAttachmentQuietPeriodSecondsTextBox.Text = profile.FolderOptions.AttachmentQuietPeriodSeconds.ToString();
        InterfaceAttachmentShowDocumentationDialogCheckBox.IsChecked = profile.FolderOptions.ShowAttachmentDocumentationDialog;
        InterfaceAttachmentLinkDocumentNameTextBox.Text = profile.FolderOptions.AttachmentExternalLinkDocumentName;
        InterfaceAttachmentLinkFileFormatTextBox.Text = profile.FolderOptions.AttachmentExternalLinkFileFormat;
        InterfaceAttachmentLinkDescriptionTextBox.Text = profile.FolderOptions.AttachmentExternalLinkDescription;
        InterfaceAttachmentLinkPathTemplateTextBox.Text = profile.FolderOptions.AttachmentExternalLinkPathTemplate;
        InterfaceFolderSetupStatusTextBlock.Text = string.Empty;
        InterfaceAttachmentFolderSetupStatusTextBlock.Text = string.Empty;
        SyncAttachmentCompletionControls(profile);
        SyncInterfaceDeviceOutputAndAttachmentVisibility(profile);

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
        InterfaceActivationStatusButton.IsChecked = false;
        ClearInterfaceDeviceProfileVisual();
        InterfaceAisImportFolderTextBox.Text = string.Empty;
        InterfaceDeviceImportFolderTextBox.Text = string.Empty;
        InterfaceExportFolderTextBox.Text = string.Empty;
        InterfaceArchiveFolderTextBox.Text = string.Empty;
        InterfaceErrorFolderTextBox.Text = string.Empty;
        InterfaceDeviceOutputEnabledCheckBox.IsChecked = false;
        InterfaceDeviceOutputFolderTextBox.Text = string.Empty;
        InterfaceDeviceOutputFileNameTextBox.Text = "CVImport.xml";
        InterfaceDeviceOutputFormatComboBox.SelectedValue = "TOPCON CV-5000 XML";
        InterfaceAutoImportScanIntervalSecondsTextBox.Text = "5";
        InterfaceDeviceFileWaitTimeoutMinutesTextBox.Text = "10";
        ApplySerialSettingsToInterfaceEditor(SerialCommunicationSettings.Default);
        ApplyNidekRtSerialSendModeToInterfaceEditor(null);
        ApplyNidekRtSerialOutputFrameVariantToInterfaceEditor(null);
        InterfaceAttachmentImportFolderTextBox.Text = string.Empty;
        InterfaceAttachmentExportFolderTextBox.Text = string.Empty;
        InterfaceAttachmentFileNameTemplateTextBox.Text = string.Empty;
        InterfaceAttachmentTransferModeComboBox.SelectedValue = AttachmentTransferMode.Move.ToString();
        InterfaceAttachmentProcessingEnabledCheckBox.IsChecked = false;
        InterfaceAttachmentRequirementModeComboBox.SelectedValue = AttachmentRequirementMode.Optional.ToString();
        InterfaceAttachmentWaitTimeoutSecondsTextBox.Text = "30";
        InterfaceAttachmentFileStabilityWaitSecondsTextBox.Text = "2";
        InterfaceAttachmentCompletionModeComboBox.SelectedValue = AttachmentCompletionMode.WaitForQuietPeriod.ToString();
        InterfaceAttachmentQuietPeriodSecondsTextBox.Text = "10";
        InterfaceAttachmentShowDocumentationDialogCheckBox.IsChecked = false;
        InterfaceAttachmentLinkDocumentNameTextBox.Text = string.Empty;
        InterfaceAttachmentLinkFileFormatTextBox.Text = string.Empty;
        InterfaceAttachmentLinkDescriptionTextBox.Text = string.Empty;
        InterfaceAttachmentLinkPathTemplateTextBox.Text = string.Empty;
        InterfaceFolderSetupStatusTextBlock.Text = string.Empty;
        InterfaceAttachmentFolderSetupStatusTextBlock.Text = string.Empty;
        InterfaceClearAisImportFolderCheckBox.IsChecked = true;
        InterfaceClearDeviceImportFolderCheckBox.IsChecked = false;
        InterfaceArchiveProcessedFilesCheckBox.IsChecked = false;
        InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked = false;
        InterfaceArchiveModeComboBox.SelectedValue = ArchiveProcessedFileMode.Copy.ToString();
        InterfaceArchiveRetentionDaysTextBox.Text = string.Empty;
        SyncAttachmentCompletionControls(null);
        SyncInterfaceDeviceOutputAndAttachmentVisibility(null);
        ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateEmpty());
    }

    private void UpdateInterfaceDeviceProfileVisual(InterfaceProfileDefinition interfaceProfile)
    {
        var deviceProfile = GetDeviceProfile(interfaceProfile.DeviceProfileId);
        if (deviceProfile is null)
        {
            ClearInterfaceDeviceProfileVisual();
            return;
        }

        var imagePath = string.Empty;
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _deviceProfileImageOverrideService.LoadOverrides(paths).TryGetValue(deviceProfile.Metadata.Id, out var overridePath);
            imagePath = _deviceProfileImageOverrideService.ResolveEffectiveImagePath(deviceProfile, overridePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            imagePath = deviceProfile.DeviceImagePath;
        }

        InterfaceDeviceImagePathTextBox.Text = imagePath;
        InterfaceDeviceImagePlaceholder.Visibility = string.IsNullOrWhiteSpace(imagePath)
            ? Visibility.Visible
            : Visibility.Collapsed;
        InterfaceDeviceTechnicalProfileButton.IsEnabled = true;
    }

    private void ClearInterfaceDeviceProfileVisual()
    {
        InterfaceDeviceImagePathTextBox.Text = string.Empty;
        InterfaceDeviceImagePlaceholder.Visibility = Visibility.Visible;
        InterfaceDeviceTechnicalProfileButton.IsEnabled = false;
    }

    private void InterfaceAttachmentCompletionModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SyncAttachmentCompletionControls(InterfaceProfileComboBox.SelectedItem as InterfaceProfileDefinition);
        RefreshInterfaceActivationPreviewForDraftChange();
    }

    private void SyncAttachmentCompletionControls(InterfaceProfileDefinition? profile)
    {
        if (InterfaceAttachmentCompletionPanel is null || InterfaceAttachmentQuietPeriodPanel is null)
        {
            return;
        }

        var isAttachmentOnly = profile?.FolderOptions.IsAttachmentOnlyMode == true;
        var isManualDocumentSelection = isAttachmentOnly
            && profile?.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var usesSerialDevice = IsSerialInterfaceProfile(profile);
        InterfaceDeviceImportFolderLabel.Text = isAttachmentOnly
            ? "Dokument-Importordner:"
            : "Gerätedatei an XDTBox:";
        InterfaceDeviceFileWaitTimeoutLabel.Text = isAttachmentOnly
            ? "Wartezeit auf Dokumentdateien:"
            : "Wartezeit auf Gerätedatei:";
        InterfaceAttachmentSettingsGroupBox.Header = isManualDocumentSelection
            ? "Manuelle Dokumentübergabe an MEDISTAR"
            : isAttachmentOnly
            ? "Dokumentübergabe an MEDISTAR"
            : "XDT-Anhänge für AIS";
        InterfaceAttachmentExportFolderLabel.Text = isAttachmentOnly
            ? "Dokument-Exportordner:"
            : "XDT-Anhang Export:";
        InterfaceAttachmentFileNameTemplateLabel.Text = isAttachmentOnly
            ? "Dateiname für Dokumente:"
            : "XDT-Anhang Dateiname:";
        InterfaceAttachmentTransferModeLabel.Text = isAttachmentOnly
            ? "Übertragung:"
            : "XDT-Anhang Übertragung:";
        InterfaceAttachmentImportFolderLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentImportFolderTextBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentImportFolderButton.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentFolderSetupPanel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentProcessingEnabledPanel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentRequirementModeLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentRequirementModeComboBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentWaitTimeoutLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentWaitTimeoutPanel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkDocumentNameLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkDocumentNameTextBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkFileFormatLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkFileFormatTextBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkDescriptionLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkDescriptionTextBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkPathTemplateLabel.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        InterfaceAttachmentLinkPathTemplateTextBox.Visibility = isAttachmentOnly ? Visibility.Collapsed : Visibility.Visible;
        var showDeviceImportFolder = !isManualDocumentSelection && !usesSerialDevice;
        InterfaceDeviceImportFolderLabel.Visibility = showDeviceImportFolder ? Visibility.Visible : Visibility.Collapsed;
        InterfaceDeviceImportFolderTextBox.Visibility = showDeviceImportFolder ? Visibility.Visible : Visibility.Collapsed;
        InterfaceDeviceImportFolderButton.Visibility = showDeviceImportFolder ? Visibility.Visible : Visibility.Collapsed;
        InterfaceDeviceFileWaitTimeoutLabel.Visibility = showDeviceImportFolder ? Visibility.Visible : Visibility.Collapsed;
        InterfaceDeviceFileWaitTimeoutPanel.Visibility = showDeviceImportFolder ? Visibility.Visible : Visibility.Collapsed;
        InterfaceClearDeviceImportFolderCheckBox.Visibility = usesSerialDevice ? Visibility.Collapsed : Visibility.Visible;

        InterfaceAttachmentGeneralHintTextBlock.Text = isManualDocumentSelection
            ? "AIS startet die manuelle Dokumentübergabe. Dateien werden per Drag & Drop oder Dateiauswahl im Übertragungsfenster ergänzt; technische 6302-6305-Felder erzeugt die App intern."
            : isAttachmentOnly
            ? "Dokumentdateien kommen aus dem Dokument-Importordner. Die App übergibt sie als MEDISTAR-Anhänge; die technischen 6302-6305-Felder werden intern erzeugt."
            : "Optional: Nach Ablauf der Wartezeit werden Messwerte auch ohne Anhang übertragen. Pflicht: Ohne eindeutigen Anhang wird die Verarbeitung später als Fehler/Blockade behandelt. Gerätedateien und Anhänge werden erst verarbeitet, wenn sie vollständig geschrieben und stabil sind.";
        InterfaceAttachmentCompletionPanel.Visibility = isAttachmentOnly && !isManualDocumentSelection
            ? Visibility.Visible
            : Visibility.Collapsed;
        InterfaceAttachmentCompletionHintTextBlock.Visibility = isAttachmentOnly && !isManualDocumentSelection ? Visibility.Visible : Visibility.Collapsed;
        InterfaceAttachmentShowDocumentationDialogCheckBox.Visibility = isAttachmentOnly && !isManualDocumentSelection ? Visibility.Visible : Visibility.Collapsed;
        var isWaitMode = !string.Equals(
            InterfaceAttachmentCompletionModeComboBox.SelectedValue as string,
            AttachmentCompletionMode.ManualConfirmation.ToString(),
            StringComparison.Ordinal);
        InterfaceAttachmentQuietPeriodPanel.IsEnabled = isAttachmentOnly && !isManualDocumentSelection && isWaitMode;
    }

    private void SyncInterfaceDeviceOutputAndAttachmentVisibility(InterfaceProfileDefinition? profile)
    {
        var deviceProfile = GetDeviceProfile(profile?.DeviceProfileId);
        var showDeviceOutput = InterfaceProfileUiPolicy.ShouldShowDeviceOutput(profile, deviceProfile);
        var showAttachmentOptions = InterfaceProfileUiPolicy.ShouldShowAisAttachmentOptions(profile, deviceProfile);
        var showSerialCommunication = IsSerialInterfaceProfile(profile);
        var showNidekRtSerialSendMode = InterfaceProfileUiPolicy.IsNidekRtSerialPhoropter(profile, deviceProfile);

        InterfaceSerialCommunicationGroupBox.Visibility = showSerialCommunication ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialSendModeLabel.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialSendModeComboBox.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialSendModeHintTextBlock.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialFrameVariantLabel.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialFrameVariantComboBox.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceNidekRtSerialFrameVariantHintTextBlock.Visibility = showNidekRtSerialSendMode ? Visibility.Visible : Visibility.Collapsed;
        InterfaceDeviceOutputGroupBox.Visibility = showDeviceOutput ? Visibility.Visible : Visibility.Collapsed;
        InterfaceAttachmentSettingsGroupBox.Visibility = showAttachmentOptions ? Visibility.Visible : Visibility.Collapsed;
    }

    private DeviceProfileDefinition? GetDeviceProfile(string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        return _profileCatalog?.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, profileId, StringComparison.OrdinalIgnoreCase));
    }

    private SerialCommunicationSettings GetSerialSettingsForProfile(InterfaceProfileDefinition profile)
    {
        var deviceProfile = GetDeviceProfile(profile.DeviceProfileId);
        if (profile.SerialSettings is not null)
        {
            return profile.SerialSettings;
        }

        return deviceProfile?.ConnectionKind == DeviceConnectionKind.SerialRs232
            ? deviceProfile.SerialSettings ?? SerialCommunicationSettings.Default
            : SerialCommunicationSettings.Default;
    }

    private bool IsSerialInterfaceProfile(InterfaceProfileDefinition? profile)
    {
        if (profile is null)
        {
            return false;
        }

        return profile.SerialSettings is not null
            || GetDeviceProfile(profile.DeviceProfileId)?.ConnectionKind == DeviceConnectionKind.SerialRs232;
    }

    private void ApplySerialSettingsToInterfaceEditor(SerialCommunicationSettings settings)
    {
        InterfaceSerialPortComboBox.Text = settings.PortName ?? string.Empty;
        InterfaceSerialBaudRateTextBox.Text = settings.BaudRate.ToString(CultureInfo.InvariantCulture);
        InterfaceSerialDataBitsTextBox.Text = settings.DataBits.ToString(CultureInfo.InvariantCulture);
        InterfaceSerialStopBitsComboBox.SelectedValue = settings.StopBits.ToString();
        InterfaceSerialParityComboBox.SelectedValue = settings.Parity.ToString();
        InterfaceSerialHandshakeComboBox.SelectedValue = settings.Handshake.ToString();
        InterfaceSerialBidirectionalCheckBox.IsChecked = settings.IsBidirectional;
        InterfaceSerialDtrCheckBox.IsChecked = settings.DtrEnable;
        InterfaceSerialRtsCheckBox.IsChecked = settings.RtsEnable;
        InterfaceSerialReadTimeoutTextBox.Text = settings.ReadTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture);
        InterfaceSerialWriteTimeoutTextBox.Text = settings.WriteTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    private void ApplyNidekRtSerialSendModeToInterfaceEditor(InterfaceProfileDefinition? profile)
    {
        var mode = NidekRtSerialSendModeInfo.Resolve(profile?.NidekRtSerialSendMode);
        InterfaceNidekRtSerialSendModeComboBox.SelectedValue = mode.ToString();
    }

    private void ApplyNidekRtSerialOutputFrameVariantToInterfaceEditor(InterfaceProfileDefinition? profile)
    {
        var variant = NidekRtSerialOutputFrameVariantInfo.Resolve(profile?.NidekRtSerialOutputFrameVariant);
        InterfaceNidekRtSerialFrameVariantComboBox.SelectedValue = variant.ToString();
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
            var previewProfile = CreateInterfaceProfileDraftForActivationPreview(profile);
            var result = _interfaceProfileActivationEvaluationService.Evaluate(
                previewProfile,
                _profileCatalog,
                CreateLicenseStatesForActivationPreview());
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.Create(previewProfile, result));
        }
        catch (Exception ex)
        {
            ShowInterfaceActivationPreview(_interfaceProfileActivationPreviewDisplayService.CreateError(ex.Message));
        }
    }

    private InterfaceProfileDefinition CreateInterfaceProfileDraftForActivationPreview(InterfaceProfileDefinition profile)
    {
        return profile with
        {
            FolderOptions = CreateInterfaceFolderOptionsFromEditor(),
            DeviceOutput = CreateInterfaceDeviceOutputFromEditor(profile),
            SerialSettings = CreateInterfaceSerialSettingsFromEditor(profile),
            NidekRtSerialSendMode = CreateNidekRtSerialSendModeFromEditor(profile),
            NidekRtSerialOutputFrameVariant = CreateNidekRtSerialOutputFrameVariantFromEditor(profile),
            IsActive = InterfaceActivationStatusButton.IsChecked == true,
            IsLicenseRequired = InterfaceProfileLicensePolicy.IsLicenseRequired(profile)
        };
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
            var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;
            var license = LoadCurrentDisplayLicenseFromLocalSource(paths, installation);
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

    private void CreateNewInterfaceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var dialog = new NewInterfaceProfileDialog(catalog)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = _userDefinedProfileCreationService.CreateInterfaceProfile(
            catalog,
            dialog.Request,
            DateTimeOffset.UtcNow,
            Environment.UserName);
        if (!result.Success || result.Profile is null)
        {
            AppendProfileCreationIssues("Schnittstellenprofil wurde nicht gespeichert", result.Issues);
            System.Windows.MessageBox.Show(
                this,
                string.Join(Environment.NewLine, result.Issues),
                "Neues Schnittstellenprofil anlegen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewInterfaceProfileDefinition(paths, result.Profile);

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(
                updatedCatalog,
                selectedExportProfileId: result.Profile.ExportProfileId,
                selectedInterfaceProfileId: result.Profile.Metadata.Id,
                selectedAisProfileId: result.Profile.AisProfileId,
                selectedDeviceProfileId: result.Profile.DeviceProfileId);
            AppendProfileMessage($"Schnittstellenprofil gespeichert: {result.Profile.Metadata.Name}");
            AppendProfileMessage("Profil wurde als UserDefined und inaktiv gespeichert. Es wurde keine automatische Verarbeitung gestartet.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Schnittstellenprofil konnte nicht gespeichert werden: {ex.Message}");
            System.Windows.MessageBox.Show(
                this,
                $"Schnittstellenprofil konnte nicht gespeichert werden:{Environment.NewLine}{ex.Message}",
                "Neues Schnittstellenprofil anlegen",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveInterfaceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition selectedProfile)
        {
            AppendProfileMessage("Schnittstellenprofil kann nicht gespeichert werden, weil kein Schnittstellenprofil ausgewählt ist.");
            return;
        }

        var selectedExportProfileId = selectedProfile.ExportProfileId;
        InterfaceFolderOptions folderOptions;
        SerialCommunicationSettings? serialSettings;
        NidekRtSerialSendMode? nidekRtSerialSendMode;
        NidekRtSerialOutputFrameVariant? nidekRtSerialOutputFrameVariant;
        try
        {
            folderOptions = CreateInterfaceFolderOptionsFromEditor();
            serialSettings = CreateInterfaceSerialSettingsFromEditor(selectedProfile);
            nidekRtSerialSendMode = CreateNidekRtSerialSendModeFromEditor(selectedProfile);
            nidekRtSerialOutputFrameVariant = CreateNidekRtSerialOutputFrameVariantFromEditor(selectedProfile);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            AppendProfileMessage($"Schnittstellenprofil wurde nicht gespeichert: {ex.Message}");
            return;
        }

        var result = _interfaceProfileConfigurationService.CreateConfiguredProfile(
            selectedProfile,
            folderOptions,
            InterfaceActivationStatusButton.IsChecked == true,
            InterfaceProfileLicensePolicy.IsLicenseRequired(selectedProfile),
            CreateInterfaceDeviceOutputFromEditor(selectedProfile),
            serialSettings,
            nidekRtSerialSendMode,
            nidekRtSerialOutputFrameVariant,
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
            ShowInterfaceProfileSaveFeedback();
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Schnittstellenprofil konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void ShowInterfaceProfileSaveFeedback()
    {
        var display = _saveFeedbackDisplayService.CreateForSaveResult(isSuccessful: true);
        if (!display.ShowSuccess)
        {
            return;
        }

        ResetInterfaceProfileSaveFeedback();

        _interfaceProfileSaveButtonOriginalContent = SaveInterfaceProfileButton.Content;
        _interfaceProfileSaveButtonOriginalBackground = SaveInterfaceProfileButton.Background;
        _interfaceProfileSaveButtonOriginalForeground = SaveInterfaceProfileButton.Foreground;
        _interfaceProfileSaveButtonOriginalBorderBrush = SaveInterfaceProfileButton.BorderBrush;
        _hasInterfaceProfileSaveButtonOriginalState = true;

        SaveInterfaceProfileButton.Content = display.ButtonText;
        SaveInterfaceProfileButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(211, 239, 221));
        SaveInterfaceProfileButton.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(23, 91, 49));
        SaveInterfaceProfileButton.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(72, 157, 101));
        InterfaceProfileSaveFeedbackText.Text = display.StatusText;

        _interfaceProfileSaveFeedbackTimer = new DispatcherTimer
        {
            Interval = display.VisibleDuration
        };
        _interfaceProfileSaveFeedbackTimer.Tick += (_, _) => ResetInterfaceProfileSaveFeedback();
        _interfaceProfileSaveFeedbackTimer.Start();
    }

    private void ResetInterfaceProfileSaveFeedback()
    {
        if (_interfaceProfileSaveFeedbackTimer is not null)
        {
            _interfaceProfileSaveFeedbackTimer.Stop();
            _interfaceProfileSaveFeedbackTimer = null;
        }

        if (_hasInterfaceProfileSaveButtonOriginalState)
        {
            SaveInterfaceProfileButton.Content = _interfaceProfileSaveButtonOriginalContent;
            SaveInterfaceProfileButton.Background = _interfaceProfileSaveButtonOriginalBackground;
            SaveInterfaceProfileButton.Foreground = _interfaceProfileSaveButtonOriginalForeground;
            SaveInterfaceProfileButton.BorderBrush = _interfaceProfileSaveButtonOriginalBorderBrush;
            _hasInterfaceProfileSaveButtonOriginalState = false;
            _interfaceProfileSaveButtonOriginalContent = null;
            _interfaceProfileSaveButtonOriginalBackground = null;
            _interfaceProfileSaveButtonOriginalForeground = null;
            _interfaceProfileSaveButtonOriginalBorderBrush = null;
        }

        if (InterfaceProfileSaveFeedbackText is not null)
        {
            InterfaceProfileSaveFeedbackText.Text = string.Empty;
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
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var deleted = _profileCatalogService.DeleteInterfaceProfile(paths, selectedProfile.Metadata.Id);
            if (!deleted)
            {
                AppendProfileMessage($"Schnittstellenprofil wurde nicht gefunden: {selectedProfile.Metadata.Name}");
                return;
            }

            var catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            RefreshProfileOverview(catalog, selectedExportProfileId: selectedProfile.ExportProfileId);
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
        var selectedProfile = InterfaceProfileComboBox.SelectedItem as InterfaceProfileDefinition;
        var isAttachmentOnly = selectedProfile?.FolderOptions.IsAttachmentOnlyMode == true;
        var isManualDocumentSelection = isAttachmentOnly
            && selectedProfile?.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var usesSerialDevice = IsSerialInterfaceProfile(selectedProfile);
        return new InterfaceFolderOptions(
            AisImportFolder: InterfaceAisImportFolderTextBox.Text.Trim(),
            DeviceImportFolder: isManualDocumentSelection || usesSerialDevice ? string.Empty : InterfaceDeviceImportFolderTextBox.Text.Trim(),
            ExportFolder: InterfaceExportFolderTextBox.Text.Trim(),
            ArchiveFolder: InterfaceArchiveFolderTextBox.Text.Trim(),
            ErrorFolder: InterfaceErrorFolderTextBox.Text.Trim(),
            ClearAisImportFolderBeforeProcessing: InterfaceClearAisImportFolderCheckBox.IsChecked == true,
            ClearDeviceImportFolderBeforeProcessing: !isManualDocumentSelection && !usesSerialDevice && InterfaceClearDeviceImportFolderCheckBox.IsChecked == true,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: InterfaceArchiveProcessedFilesCheckBox.IsChecked == true,
            MoveFailedFilesToErrorFolder: InterfaceMoveFailedFilesToErrorFolderCheckBox.IsChecked == true,
            ArchiveProcessedFileMode: ReadArchiveProcessedFileModeFromEditor(),
            ArchiveRetentionDays: ReadArchiveRetentionDaysFromEditor(),
            AutoImportScanIntervalSeconds: ReadAutoImportScanIntervalSecondsFromEditor(),
            DeviceFileWaitTimeoutMinutes: ReadDeviceFileWaitTimeoutMinutesFromEditor(),
            AttachmentImportFolder: isAttachmentOnly ? string.Empty : InterfaceAttachmentImportFolderTextBox.Text.Trim(),
            AttachmentExportFolder: InterfaceAttachmentExportFolderTextBox.Text.Trim(),
            AttachmentFileNameTemplate: InterfaceAttachmentFileNameTemplateTextBox.Text.Trim(),
            AttachmentTransferMode: isManualDocumentSelection ? AttachmentTransferMode.Copy : ReadAttachmentTransferModeFromEditor(),
            AttachmentExternalLinkDocumentName: isAttachmentOnly
                ? DefaultIfWhiteSpace(InterfaceAttachmentLinkDocumentNameTextBox.Text, "Datei")
                : InterfaceAttachmentLinkDocumentNameTextBox.Text.Trim(),
            AttachmentExternalLinkFileFormat: isAttachmentOnly
                ? DefaultIfWhiteSpace(InterfaceAttachmentLinkFileFormatTextBox.Text, "{ExtensionUpperWithoutDot}")
                : InterfaceAttachmentLinkFileFormatTextBox.Text.Trim(),
            AttachmentExternalLinkDescription: InterfaceAttachmentLinkDescriptionTextBox.Text.Trim(),
            AttachmentExternalLinkPathTemplate: isAttachmentOnly
                ? DefaultIfWhiteSpace(InterfaceAttachmentLinkPathTemplateTextBox.Text, "{Attachment.TargetFullPath}")
                : InterfaceAttachmentLinkPathTemplateTextBox.Text.Trim(),
            IsAttachmentProcessingEnabled: isAttachmentOnly || InterfaceAttachmentProcessingEnabledCheckBox.IsChecked == true,
            AttachmentRequirementMode: isAttachmentOnly ? AttachmentRequirementMode.Required : ReadAttachmentRequirementModeFromEditor(),
            AttachmentWaitTimeoutSeconds: ReadAttachmentWaitTimeoutSecondsFromEditor(),
            AttachmentFileStabilityWaitSeconds: ReadAttachmentFileStabilityWaitSecondsFromEditor(),
            IsAttachmentOnlyMode: isAttachmentOnly,
            ShowAttachmentDocumentationDialog: isManualDocumentSelection
                ? true
                : isAttachmentOnly
                ? InterfaceAttachmentShowDocumentationDialogCheckBox.IsChecked == true
                : selectedProfile?.FolderOptions.ShowAttachmentDocumentationDialog == true,
            AttachmentCompletionMode: isManualDocumentSelection ? AttachmentCompletionMode.ManualConfirmation : ReadAttachmentCompletionModeFromEditor(),
            AttachmentQuietPeriodSeconds: ReadAttachmentQuietPeriodSecondsFromEditor(),
            AttachmentOnlySourceMode: selectedProfile?.FolderOptions.AttachmentOnlySourceMode ?? AttachmentOnlySourceMode.DeviceFolder);
    }

    private DeviceOutputConfiguration? CreateInterfaceDeviceOutputFromEditor(InterfaceProfileDefinition selectedProfile)
    {
        var deviceProfile = GetDeviceProfile(selectedProfile.DeviceProfileId);
        if (!InterfaceProfileUiPolicy.ShouldShowDeviceOutput(selectedProfile, deviceProfile))
        {
            return null;
        }

        return new DeviceOutputConfiguration(
            IsEnabled: InterfaceDeviceOutputEnabledCheckBox.IsChecked == true,
            OutputFolder: InterfaceDeviceOutputFolderTextBox.Text.Trim(),
            FileNameTemplate: DefaultIfWhiteSpace(InterfaceDeviceOutputFileNameTextBox.Text, "CVImport.xml"),
            Format: InterfaceDeviceOutputFormatComboBox.SelectedValue as string ?? "TOPCON CV-5000 XML");
    }

    private SerialCommunicationSettings? CreateInterfaceSerialSettingsFromEditor(InterfaceProfileDefinition selectedProfile)
    {
        if (!IsSerialInterfaceProfile(selectedProfile))
        {
            return null;
        }

        return CreateSerialSettingsFromValues(
            InterfaceSerialPortComboBox.Text,
            InterfaceSerialBaudRateTextBox.Text,
            InterfaceSerialDataBitsTextBox.Text,
            InterfaceSerialStopBitsComboBox.SelectedValue as string,
            InterfaceSerialParityComboBox.SelectedValue as string,
            InterfaceSerialHandshakeComboBox.SelectedValue as string,
            InterfaceSerialDtrCheckBox.IsChecked == true,
            InterfaceSerialRtsCheckBox.IsChecked == true,
            InterfaceSerialBidirectionalCheckBox.IsChecked == true,
            InterfaceSerialReadTimeoutTextBox.Text,
            InterfaceSerialWriteTimeoutTextBox.Text);
    }

    private NidekRtSerialSendMode? CreateNidekRtSerialSendModeFromEditor(InterfaceProfileDefinition selectedProfile)
    {
        var deviceProfile = GetDeviceProfile(selectedProfile.DeviceProfileId);
        if (!InterfaceProfileUiPolicy.IsNidekRtSerialPhoropter(selectedProfile, deviceProfile))
        {
            return null;
        }

        var value = InterfaceNidekRtSerialSendModeComboBox.SelectedValue as string;
        return Enum.TryParse<NidekRtSerialSendMode>(value, ignoreCase: true, out var mode)
            ? mode
            : NidekRtSerialSendModeInfo.Default;
    }

    private NidekRtSerialOutputFrameVariant? CreateNidekRtSerialOutputFrameVariantFromEditor(InterfaceProfileDefinition selectedProfile)
    {
        var deviceProfile = GetDeviceProfile(selectedProfile.DeviceProfileId);
        if (!InterfaceProfileUiPolicy.IsNidekRtSerialPhoropter(selectedProfile, deviceProfile))
        {
            return null;
        }

        var value = InterfaceNidekRtSerialFrameVariantComboBox.SelectedValue as string;
        return Enum.TryParse<NidekRtSerialOutputFrameVariant>(value, ignoreCase: true, out var variant)
            ? variant
            : NidekRtSerialOutputFrameVariantInfo.Default;
    }

    private static string DefaultIfWhiteSpace(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static SerialCommunicationSettings CreateSerialSettingsFromValues(
        string? portName,
        string? baudRate,
        string? dataBits,
        string? stopBits,
        string? parity,
        string? handshake,
        bool dtrEnable,
        bool rtsEnable,
        bool isBidirectional,
        string? readTimeout,
        string? writeTimeout)
    {
        var settings = new SerialCommunicationSettings(
            PortName: portName?.Trim(),
            BaudRate: ReadPositiveIntOrDefault(baudRate, 9600, "Baudrate"),
            DataBits: ReadPositiveIntOrDefault(dataBits, 8, "Datenbits"),
            StopBits: ReadEnumOrDefault(stopBits, SerialStopBitsSetting.One),
            Parity: ReadEnumOrDefault(parity, SerialParitySetting.None),
            Handshake: ReadEnumOrDefault(handshake, SerialHandshakeSetting.None),
            DtrEnable: dtrEnable,
            RtsEnable: rtsEnable,
            IsBidirectional: isBidirectional,
            ReadTimeoutMilliseconds: ReadNonNegativeIntOrDefault(readTimeout, 1000, "ReadTimeout"),
            WriteTimeoutMilliseconds: ReadNonNegativeIntOrDefault(writeTimeout, 1000, "WriteTimeout"));

        var validationIssue = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: false);
        if (validationIssue is not null)
        {
            throw new ArgumentException(validationIssue);
        }

        return settings;
    }

    private static int ReadPositiveIntOrDefault(string? rawValue, int fallback, string label)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        if (!int.TryParse(rawValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"{label} muss eine ganze Zahl sein.");
        }

        if (value <= 0)
        {
            throw new ArgumentException($"{label} muss größer als 0 sein.");
        }

        return value;
    }

    private static int ReadNonNegativeIntOrDefault(string? rawValue, int fallback, string label)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        if (!int.TryParse(rawValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"{label} muss eine ganze Zahl sein.");
        }

        if (value < 0)
        {
            throw new ArgumentException($"{label} darf nicht negativ sein.");
        }

        return value;
    }

    private static TEnum ReadEnumOrDefault<TEnum>(string? rawValue, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var value)
            && Enum.IsDefined(value)
            ? value
            : fallback;
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

    private AttachmentCompletionMode ReadAttachmentCompletionModeFromEditor()
    {
        return string.Equals(InterfaceAttachmentCompletionModeComboBox.SelectedValue as string, AttachmentCompletionMode.ManualConfirmation.ToString(), StringComparison.Ordinal)
            ? AttachmentCompletionMode.ManualConfirmation
            : AttachmentCompletionMode.WaitForQuietPeriod;
    }

    private int ReadAttachmentQuietPeriodSecondsFromEditor()
    {
        var rawValue = InterfaceAttachmentQuietPeriodSecondsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 10;
        }

        if (!int.TryParse(rawValue, out var quietPeriodSeconds))
        {
            throw new FormatException("Wartezeit nach letzter Datei muss eine ganze Zahl in Sekunden sein.");
        }

        if (quietPeriodSeconds is < 1 or > 300)
        {
            throw new ArgumentException("Wartezeit nach letzter Datei muss zwischen 1 und 300 Sekunden liegen.");
        }

        return quietPeriodSeconds;
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

    private void ApplyInterfaceFolderDefaults_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedInterfaceDeviceProfile(out _, out var deviceProfile))
        {
            return;
        }

        var defaults = _interfaceProfileFolderSetupService.CreateMainDefaultFolders(deviceProfile);
        InterfaceAisImportFolderTextBox.Text = defaults.AisImportFolder;
        InterfaceDeviceImportFolderTextBox.Text = deviceProfile.ConnectionKind == DeviceConnectionKind.SerialRs232
            ? string.Empty
            : defaults.DeviceImportFolder;
        InterfaceExportFolderTextBox.Text = defaults.ExportFolder;
        InterfaceArchiveFolderTextBox.Text = defaults.ArchiveFolder;
        InterfaceErrorFolderTextBox.Text = defaults.ErrorFolder;

        SetFolderSetupStatus(
            InterfaceFolderSetupStatusTextBlock,
            "Standardpfade eingetragen. Bitte prüfen und speichern.",
            isSuccess: true);
    }

    private void CreateInterfaceFolders_Click(object sender, RoutedEventArgs e)
    {
        var result = _interfaceProfileFolderSetupService.CreateDirectories(CreateMainFolderCreationRequestsFromEditor());

        ShowFolderCreationResult(
            result,
            InterfaceFolderSetupStatusTextBlock,
            "Ordner wurden angelegt.");
    }

    private void ApplyInterfaceAttachmentFolderDefaults_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedInterfaceDeviceProfile(out _, out var deviceProfile))
        {
            return;
        }

        var defaults = _interfaceProfileFolderSetupService.CreateAttachmentDefaultFolders(deviceProfile);
        InterfaceAttachmentImportFolderTextBox.Text = defaults.AttachmentImportFolder;
        InterfaceAttachmentExportFolderTextBox.Text = defaults.AttachmentExportFolder;

        SetFolderSetupStatus(
            InterfaceAttachmentFolderSetupStatusTextBlock,
            "Standardpfade eingetragen. Bitte prüfen und speichern.",
            isSuccess: true);
    }

    private void CreateInterfaceAttachmentFolders_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceAttachmentSettingsGroupBox.Visibility != Visibility.Visible
            || InterfaceAttachmentFolderSetupPanel.Visibility != Visibility.Visible)
        {
            return;
        }

        var result = _interfaceProfileFolderSetupService.CreateDirectories(CreateAttachmentFolderCreationRequestsFromEditor());

        ShowFolderCreationResult(
            result,
            InterfaceAttachmentFolderSetupStatusTextBlock,
            "XDT-Anhang-Ordner wurden angelegt.");
    }

    private void RefreshInterfaceSerialPorts_Click(object sender, RoutedEventArgs e)
    {
        RefreshSerialPortComboBox(InterfaceSerialPortComboBox);
        InterfaceSerialStatusTextBlock.Text = InterfaceSerialPortComboBox.Items.Count == 0
            ? "Keine COM-Ports gefunden. Port kann bei Bedarf manuell eingetragen werden."
            : "COM-Ports aktualisiert.";
    }

    private void OpenInterfaceSerialDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition selectedProfile
            || !IsSerialInterfaceProfile(selectedProfile))
        {
            InterfaceSerialStatusTextBlock.Text = "Bitte zuerst ein serielles Schnittstellenprofil auswählen.";
            return;
        }

        SerialCommunicationSettings settings;
        try
        {
            settings = CreateInterfaceSerialSettingsFromEditor(selectedProfile) ?? SerialCommunicationSettings.Default;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            InterfaceSerialStatusTextBlock.Text = ex.Message;
            return;
        }

        var dialog = new XdtBaukastenSerialCaptureWindow(
            _serialPortDiscoveryService,
            _serialDeviceCommunicationService,
            settings,
            allowWorkbenchAccept: false)
        {
            Owner = this
        };
        _ = dialog.ShowDialog();
        InterfaceSerialStatusTextBlock.Text = "RS232-Diagnose geschlossen. Es wurde keine produktive Verarbeitung gestartet.";
    }

    private bool TryGetSelectedInterfaceDeviceProfile(
        out InterfaceProfileDefinition profile,
        out DeviceProfileDefinition deviceProfile)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition selectedProfile)
        {
            profile = null!;
            deviceProfile = null!;
            System.Windows.MessageBox.Show(
                this,
                "Bitte zuerst ein Schnittstellenprofil auswählen.",
                "Ordner vorbereiten",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return false;
        }

        var selectedDeviceProfile = GetDeviceProfile(selectedProfile.DeviceProfileId);
        if (selectedDeviceProfile is null)
        {
            profile = null!;
            deviceProfile = null!;
            System.Windows.MessageBox.Show(
                this,
                "Das Geräteprofil zum ausgewählten Schnittstellenprofil wurde nicht gefunden.",
                "Ordner vorbereiten",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        profile = selectedProfile;
        deviceProfile = selectedDeviceProfile;
        return true;
    }

    private void XdtBaukastenDeleteExportRule_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { Tag: ExportRuleDefinition rule })
        {
            return;
        }

        PushXdtBaukastenUndoState();
        if (!_xdtBaukastenState.RemoveWorkingRule(rule.Id))
        {
            XdtBaukastenDraftStatusText.Text = "Exportregel konnte nicht gelöscht werden.";
            return;
        }

        RefreshXdtBaukastenRuleGrid();
        XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? "Geräteausgabe-Regel aus der Baukasten-Arbeitskopie gelöscht. Das Originalprofil bleibt unverändert."
            : "Exportregel aus der Baukasten-Arbeitskopie gelöscht. Das Originalprofil bleibt unverändert.";
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private void XdtBaukastenAddExportRule_Click(object sender, RoutedEventArgs e)
    {
        PushXdtBaukastenUndoState();
        var currentRules = _xdtBaukastenState.CurrentWorkingRules;
        var nextSortOrder = currentRules.Count == 0
            ? 1
            : currentRules.Max(rule => rule.SortOrder) + 10;
        var isDeviceOutput = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput;
        var rule = new ExportRuleDefinition(
            Id: $"baukasten-rule-{Guid.NewGuid():N}",
            TargetFieldCode: isDeviceOutput ? "DeviceOutput/Custom" : "6228",
            TargetName: isDeviceOutput ? "Neue Geräteausgabe" : "Neue Regel",
            RuleType: ExportRuleType.Template,
            SourcePath: null,
            OutputTemplate: isDeviceOutput ? "Neue Geräteausgabe" : "Neue feste Notiz",
            SortOrder: nextSortOrder,
            IsEnabled: true,
            Description: isDeviceOutput
                ? "Neue Geräteausgabe-Regel in der Baukasten-Arbeitskopie."
                : "Neue Baukasten-Regel in der Arbeitskopie.");

        _xdtBaukastenState.AddWorkingRule(rule);
        RefreshXdtBaukastenRuleGrid();
        var addedRow = _xdtBaukastenExportRules.FirstOrDefault(row => string.Equals(row.Rule.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (addedRow is not null)
        {
            XdtBaukastenExportRulesGrid.SelectedItem = addedRow;
            XdtBaukastenExportRulesGrid.ScrollIntoView(addedRow);
        }
        XdtBaukastenDraftStatusText.Text = isDeviceOutput
            ? "Neue Geräteausgabe-Regel in der Baukasten-Arbeitskopie angelegt."
            : "Neue Exportregel in der Baukasten-Arbeitskopie angelegt.";
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private IReadOnlyList<InterfaceProfileFolderCreationRequest> CreateMainFolderCreationRequestsFromEditor()
    {
        var requests = new List<InterfaceProfileFolderCreationRequest>
        {
            new InterfaceProfileFolderCreationRequest("AIS-Patienten Datei an XDTBox", InterfaceAisImportFolderTextBox.Text),
            new InterfaceProfileFolderCreationRequest("Ergebnisdatei an AIS", InterfaceExportFolderTextBox.Text),
            new InterfaceProfileFolderCreationRequest("Archiv", InterfaceArchiveFolderTextBox.Text),
            new InterfaceProfileFolderCreationRequest("Fehler", InterfaceErrorFolderTextBox.Text)
        };

        if (InterfaceProfileComboBox.SelectedItem is InterfaceProfileDefinition profile
            && !IsSerialInterfaceProfile(profile))
        {
            requests.Insert(1, new InterfaceProfileFolderCreationRequest("Gerätedatei an XDTBox", InterfaceDeviceImportFolderTextBox.Text));
        }

        return requests;
    }

    private IReadOnlyList<InterfaceProfileFolderCreationRequest> CreateAttachmentFolderCreationRequestsFromEditor()
    {
        return new[]
        {
            new InterfaceProfileFolderCreationRequest("XDT-Anhang Import", InterfaceAttachmentImportFolderTextBox.Text),
            new InterfaceProfileFolderCreationRequest("XDT-Anhang Export", InterfaceAttachmentExportFolderTextBox.Text)
        };
    }

    private void ShowFolderCreationResult(
        InterfaceProfileFolderCreationResult result,
        TextBlock statusTextBlock,
        string successMessage)
    {
        if (result.Success)
        {
            SetFolderSetupStatus(statusTextBlock, successMessage, isSuccess: true);
            return;
        }

        var statusMessage = result.HasCreatedOrExistingFolders
            ? "Ordner teilweise angelegt. Bitte Details prüfen."
            : "Ordner konnten nicht angelegt werden. Bitte Details prüfen.";
        SetFolderSetupStatus(statusTextBlock, statusMessage, isSuccess: false);

        System.Windows.MessageBox.Show(
            this,
            BuildFolderCreationErrorMessage(result),
            "Ordner anlegen",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private static void SetFolderSetupStatus(TextBlock statusTextBlock, string message, bool isSuccess)
    {
        statusTextBlock.Text = message;
        statusTextBlock.Foreground = isSuccess
            ? System.Windows.Media.Brushes.SeaGreen
            : System.Windows.Media.Brushes.DarkRed;
    }

    private static string BuildFolderCreationErrorMessage(InterfaceProfileFolderCreationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Folgende Ordner konnten nicht angelegt werden:");

        foreach (var entry in result.Entries.Where(entry => !entry.Success))
        {
            var path = string.IsNullOrWhiteSpace(entry.Path) ? "(Pfad fehlt)" : entry.Path;
            builder.AppendLine($"- {entry.Label}: {path}");
            builder.AppendLine($"  Grund: {entry.ErrorMessage ?? "Unbekannter Fehler"}");
        }

        return builder.ToString().TrimEnd();
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
            "DeviceOutput" => InterfaceDeviceOutputFolderTextBox,
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

    private void CreateNewAisProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var dialog = new NewAisProfileDialog
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = _userDefinedProfileCreationService.CreateAisProfile(
            catalog,
            dialog.Request,
            DateTimeOffset.UtcNow,
            Environment.UserName);
        if (!result.Success || result.Profile is null)
        {
            AppendProfileCreationIssues("AIS-Profil wurde nicht gespeichert", result.Issues);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewAisProfile(paths, result.Profile);

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog);
            AppendProfileMessage($"AIS-Profil gespeichert: {result.Profile.Metadata.Name}");
            AppendProfileMessage("Profil wurde als UserDefined gespeichert. BuiltIn-Profile wurden nicht verändert.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"AIS-Profil konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void CreateNewDeviceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var dialog = new NewDeviceProfileDialog(GetAvailableDeviceParserModes(catalog))
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = _userDefinedProfileCreationService.CreateDeviceProfile(
            catalog,
            dialog.Request,
            DateTimeOffset.UtcNow,
            Environment.UserName);
        if (!result.Success || result.Profile is null)
        {
            AppendProfileCreationIssues("Geräteprofil wurde nicht gespeichert", result.Issues);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewDeviceProfileDefinition(paths, result.Profile);

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog);
            AppendProfileMessage($"Geräteprofil gespeichert: {result.Profile.Metadata.Name}");
            AppendProfileMessage("Profil wurde als UserDefined gespeichert. BuiltIn-Profile wurden nicht verändert.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Geräteprofil konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void LoadDeviceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var dialog = new LoadDeviceProfileDialog(catalog.DeviceProfiles, paths, _deviceProfileImageOverrideService)
            {
                Owner = this
            };

            _ = dialog.ShowDialog();
            if (!dialog.HasChanges)
            {
                return;
            }

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog);
            AppendProfileMessage("Gerätebild gespeichert. BuiltIn-Fachprofile wurden nicht überschrieben.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Geräteprofil konnte nicht geladen werden: {ex.Message}");
        }
    }

    private void RenameSelectedInterfaceProfile_Click(object sender, RoutedEventArgs e)
    {
        if (InterfaceProfileComboBox.SelectedItem is not InterfaceProfileDefinition profile)
        {
            AppendProfileMessage("Schnittstellenprofil kann nicht umbenannt werden, weil kein Profil ausgewählt ist.");
            return;
        }

        RenameProfile(
            UserDefinedProfileRenameKind.InterfaceProfile,
            profile.Metadata.Id,
            profile.Metadata.Name,
            selectedInterfaceProfileId: profile.Metadata.Id);
    }

    private void RenameProfile(
        UserDefinedProfileRenameKind kind,
        string profileId,
        string currentName,
        string? selectedAisProfileId = null,
        string? selectedDeviceProfileId = null,
        string? selectedExportProfileId = null,
        string? selectedInterfaceProfileId = null)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var evaluation = _userDefinedProfileRenameService.Evaluate(catalog, kind, profileId, currentName);
        if (!evaluation.Success && evaluation.Issues.Count > 0)
        {
            AppendProfileMessage(evaluation.Message);
            System.Windows.MessageBox.Show(
                this,
                evaluation.Message,
                "Profil umbenennen",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var dialog = new RenameProfileDialog(currentName)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var result = _userDefinedProfileRenameService.Rename(
                catalog,
                paths,
                kind,
                profileId,
                dialog.NewName);
            if (!result.Success)
            {
                AppendProfileMessage(result.Message);
                System.Windows.MessageBox.Show(
                    this,
                    result.Message,
                    "Profil umbenennen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(
                updatedCatalog,
                selectedExportProfileId,
                selectedInterfaceProfileId,
                selectedAisProfileId,
                selectedDeviceProfileId);

            AppendProfileMessage(result.NoChange
                ? result.Message
                : $"Profilname geändert: {result.OldName} → {result.NewName}");
            AppendProfileMessage("Nur der sichtbare Name wurde geändert. IDs, Referenzen und Einstellungen bleiben unverändert.");
        }
        catch (Exception ex)
        {
            AppendProfileMessage($"Profil konnte nicht umbenannt werden: {ex.Message}");
        }
    }

    private bool TryGetProfileCatalogForProfileAction(out ProfileCatalog catalog)
    {
        if (_profileCatalog is not null)
        {
            catalog = _profileCatalog;
            return true;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.EnsureDefaultProfiles(paths);
            catalog = _profileCatalogService.Load(paths);
            _profileCatalog = catalog;
            return true;
        }
        catch (Exception ex)
        {
            catalog = null!;
            AppendProfileMessage($"Profile konnten nicht geladen werden: {ex.Message}");
            return false;
        }
    }

    private static IReadOnlyList<string> GetAvailableDeviceParserModes(ProfileCatalog catalog)
    {
        return catalog.DeviceProfiles
            .Select(profile => profile.ParserMode)
            .Where(parserMode => !string.IsNullOrWhiteSpace(parserMode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(parserMode => parserMode, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private void AppendProfileCreationIssues(string title, IReadOnlyList<string> issues)
    {
        AppendProfileMessage($"{title}:");
        foreach (var issue in issues)
        {
            AppendProfileMessage($"[Profilanlage] {issue}");
        }
    }

    private void RefreshProfileOverview(
        ProfileCatalog catalog,
        string? selectedExportProfileId = null,
        string? selectedInterfaceProfileId = null,
        string? selectedAisProfileId = null,
        string? selectedDeviceProfileId = null)
    {
        InitializeProfileDependentTabs(
            catalog,
            selectedInterfaceProfileId,
            selectedAisProfileId,
            selectedDeviceProfileId,
            selectedExportProfileId);
        RefreshLicensedDeviceStatesFromLocalLicense();
    }

    private static TProfile? SelectProfileById<TProfile>(
        IReadOnlyList<TProfile> profiles,
        string? profileId,
        Func<TProfile, string> getId)
    {
        if (profiles.Count == 0)
        {
            return default;
        }

        if (!string.IsNullOrWhiteSpace(profileId))
        {
            var selectedProfile = profiles.FirstOrDefault(profile =>
                string.Equals(getId(profile), profileId, StringComparison.Ordinal));
            if (selectedProfile is not null)
            {
                return selectedProfile;
            }
        }

        return profiles[0];
    }

    private static void UpdateProfileRenameButton(
        System.Windows.Controls.Button button,
        ProfileMetadata? metadata,
        string profileKindLabel)
    {
        if (metadata is null)
        {
            button.IsEnabled = false;
            button.ToolTip = $"Bitte zuerst ein {profileKindLabel} auswählen.";
            return;
        }

        if (metadata.IsBuiltIn)
        {
            button.IsEnabled = false;
            button.ToolTip = "BuiltIn-Profile können nicht umbenannt werden.";
            return;
        }

        if (!metadata.IsUserDefined)
        {
            button.IsEnabled = false;
            button.ToolTip = "Nur UserDefined-Profile können umbenannt werden.";
            return;
        }

        button.IsEnabled = true;
        button.ToolTip = $"Ändert nur den sichtbaren Namen dieses UserDefined-{profileKindLabel}s.";
    }

    private void InitializeLicenseOverview()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;
            LoadLicenseCustomerDataIntoEditor(paths);
            var activeLicensedDeviceCount = CountActiveLicensedDevices();
            var signedLicenseFile = GetSignedLicenseFilePath(paths);

            if (File.Exists(signedLicenseFile))
            {
                ShowSignedLicenseStatus(signedLicenseFile, installation);
                return;
            }

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
                LicenseMessagesTextBox.Text = "Legacy-Lizenzstatus geladen (Signatur nicht kryptografisch geprüft).";
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
        return _profileCatalog?.InterfaceProfiles.Count(InterfaceProfileLicensePolicy.IsActiveLicenseRelevant) ?? 0;
    }

    private int CountActiveDeviceConnectionsForLicenseV1()
    {
        return LicenseV1DeviceConnectionCounter.CountActiveDeviceConnections(
            _profileCatalog?.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>());
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

    private void LoadLicenseCustomerDataIntoEditor(AppDataPaths paths)
    {
        try
        {
            var customer = _licenseCustomerDataRepository.LoadOrEmpty(GetLicenseCustomerDataFilePath(paths));
            ShowLicenseCustomerData(customer);
        }
        catch (Exception ex)
        {
            ShowLicenseCustomerData(LicenseRequestCustomer.Empty);
            LicenseCustomerDataStatusText.Text = $"Kundendaten konnten nicht geladen werden: {ex.Message}";
        }
    }

    private void InitializeBackupOverview()
    {
        BackupTargetPathTextBox.Text = _xdtBoxBackupPathService.CreateDefaultBackupFilePath();
        BackupManifestTextBlock.Text = "Keine Sicherung ausgewählt.";
        BackupCreateStatusText.Text = string.Empty;
        BackupRestoreStatusText.Text = string.Empty;
    }

    private void SelectBackupTarget_Click(object sender, RoutedEventArgs e)
    {
        var initialPath = string.IsNullOrWhiteSpace(BackupTargetPathTextBox.Text)
            ? _xdtBoxBackupPathService.CreateDefaultBackupFilePath()
            : BackupTargetPathTextBox.Text.Trim();
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "XDTBox-Sicherung speichern",
            Filter = "XDTBox-Sicherung (*.xdtboxbackup)|*.xdtboxbackup|Alle Dateien (*.*)|*.*",
            DefaultExt = ".xdtboxbackup",
            FileName = Path.GetFileName(initialPath),
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(initialPath))
                ? Path.GetDirectoryName(initialPath)
                : _xdtBoxBackupPathService.GetDefaultBackupFolder()
        };

        if (dialog.ShowDialog(this) == true)
        {
            BackupTargetPathTextBox.Text = dialog.FileName;
        }
    }

    private void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;
            var backupPath = string.IsNullOrWhiteSpace(BackupTargetPathTextBox.Text)
                ? _xdtBoxBackupPathService.CreateDefaultBackupFilePath()
                : BackupTargetPathTextBox.Text.Trim();
            var result = _xdtBoxBackupService.CreateBackup(
                paths,
                backupPath,
                GetApplicationVersionText(),
                installation.InstallationId,
                includeLicenseFile: true);

            BackupCreateStatusText.Text = string.Join(" ", result.Messages);
            if (result.Success && result.BackupFilePath is not null)
            {
                BackupTargetPathTextBox.Text = result.BackupFilePath;
                BackupCreateStatusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 111, 78));
            }
            else
            {
                BackupCreateStatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
            }
        }
        catch (Exception ex)
        {
            BackupCreateStatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
            BackupCreateStatusText.Text = $"Sicherung konnte nicht erstellt werden: {ex.Message}";
        }
    }

    private void SelectBackupRestoreFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "XDTBox-Sicherung auswählen",
            Filter = "XDTBox-Sicherung (*.xdtboxbackup)|*.xdtboxbackup|Alle Dateien (*.*)|*.*",
            DefaultExt = ".xdtboxbackup",
            InitialDirectory = _xdtBoxBackupPathService.GetDefaultBackupFolder()
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        BackupRestorePathTextBox.Text = dialog.FileName;
        var preview = _xdtBoxBackupService.PreviewRestore(dialog.FileName);
        BackupManifestTextBlock.Text = preview.Manifest is null
            ? string.Join(Environment.NewLine, preview.Messages)
            : FormatBackupManifest(preview.Manifest, preview.Messages);
        BackupRestoreStatusText.Text = preview.Success ? "Sicherung ist bereit zur Wiederherstellung." : "Sicherung kann nicht wiederhergestellt werden.";
        BackupRestoreStatusText.Foreground = preview.Success
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 111, 78))
            : System.Windows.Media.Brushes.DarkRed;
    }

    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BackupRestorePathTextBox.Text))
        {
            BackupRestoreStatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
            BackupRestoreStatusText.Text = "Bitte zuerst eine Sicherung auswählen.";
            return;
        }

        if (_periodicScanCancellationTokenSource is not null)
        {
            BackupRestoreStatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
            BackupRestoreStatusText.Text = "Bitte stoppen Sie zuerst die Überwachung.";
            return;
        }

        var confirmation = System.Windows.MessageBox.Show(
            this,
            "Die Wiederherstellung ersetzt lokale XDTBox-Konfigurationen. Bitte stellen Sie sicher, dass keine Verarbeitung läuft.",
            "XDTBox-Sicherung wiederherstellen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        var paths = _appDataPathProvider.GetDefaultUserPaths();
        var result = _xdtBoxBackupService.RestoreBackup(paths, BackupRestorePathTextBox.Text.Trim(), isMonitoringRunning: false);
        BackupRestoreStatusText.Text = string.Join(" ", result.Messages);
        BackupRestoreStatusText.Foreground = result.Success
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(47, 111, 78))
            : System.Windows.Media.Brushes.DarkRed;

        if (!result.Success)
        {
            return;
        }

        InitializeProfileOverview();
        InitializeLicenseOverview();
        LoadFloatingWindowStates();
        RefreshInterfaceMonitoringCards();
    }

    private static string FormatBackupManifest(XdtBoxBackupManifest manifest, IReadOnlyList<string> messages)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Erstellt: {manifest.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss}");
        builder.AppendLine($"AppVersion: {manifest.AppVersion}");
        builder.AppendLine($"SourceInstallationId: {manifest.SourceInstallationId}");
        builder.AppendLine($"Enthaltene Bereiche: {string.Join(", ", manifest.IncludedAreas)}");
        builder.AppendLine($"Lizenzdatei enthalten: {(manifest.IncludesLicenseFile ? "Ja" : "Nein")}");
        builder.AppendLine(manifest.HardwareMigrationNotice);
        foreach (var message in messages)
        {
            builder.AppendLine(message);
        }

        return builder.ToString().Trim();
    }

    private static string GetApplicationVersionText()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return string.IsNullOrWhiteSpace(informationalVersion)
            ? assembly.GetName().Version?.ToString() ?? "unbekannt"
            : informationalVersion;
    }

    private void TabHelpButton_Click(object sender, RoutedEventArgs e)
    {
        TabHelpButton.ContextMenu.PlacementTarget = TabHelpButton;
        TabHelpButton.ContextMenu.IsOpen = true;
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, MainTabControl) || _isRestoringProtectedTabSelection)
        {
            return;
        }

        var selectedTab = MainTabControl.SelectedItem as TabItem;
        if (!IsProtectedMainTab(selectedTab) || !IsTabProtectionConfigured() || _isTabProtectionUnlocked)
        {
            _lastAllowedMainTabItem = selectedTab;
            return;
        }

        if (PromptForTabProtectionPassword(selectedTab?.Header?.ToString() ?? "geschützten Bereich"))
        {
            _isTabProtectionUnlocked = true;
            _lastAllowedMainTabItem = selectedTab;
            UpdateTabProtectionUi();
            return;
        }

        _isRestoringProtectedTabSelection = true;
        try
        {
            MainTabControl.SelectedItem = _lastAllowedMainTabItem ?? FindProcessingTab();
        }
        finally
        {
            _isRestoringProtectedTabSelection = false;
        }
    }

    private void TabProtectionToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_updatingTabProtectionUi)
        {
            return;
        }

        if (!IsTabProtectionConfigured())
        {
            _isTabProtectionUnlocked = false;
            UpdateTabProtectionUi();
            return;
        }

        if (TabProtectionToggleButton.IsChecked == true)
        {
            if (PromptForTabProtectionPassword("Konfiguration"))
            {
                _isTabProtectionUnlocked = true;
                UpdateTabProtectionUi();
                return;
            }

            _isTabProtectionUnlocked = false;
            UpdateTabProtectionUi();
            return;
        }

        _isTabProtectionUnlocked = false;
        if (IsProtectedMainTab(MainTabControl.SelectedItem as TabItem))
        {
            _isRestoringProtectedTabSelection = true;
            try
            {
                MainTabControl.SelectedItem = FindProcessingTab();
                _lastAllowedMainTabItem = MainTabControl.SelectedItem as TabItem;
            }
            finally
            {
                _isRestoringProtectedTabSelection = false;
            }
        }

        UpdateTabProtectionUi();
    }

    private bool PromptForTabProtectionPassword(string tabName)
    {
        var dialog = new TabProtectionPasswordDialog(tabName)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        var isValid = _tabProtectionService.VerifyAnyPassword(_tabProtectionSettings, dialog.EnteredPassword);
        if (!isValid)
        {
            System.Windows.MessageBox.Show(
                this,
                "Passwort nicht korrekt.",
                "Tab-Schutz",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        return isValid;
    }

    private void OpenAppSettings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppSettingsDialog(_appSettings, IsTabProtectionConfigured())
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _appSettings = dialog.Settings.Clone();
        try
        {
            SaveAppSettings();
            if (dialog.NewTabProtectionPassword is not null)
            {
                _tabProtectionSettings = _tabProtectionService.CreateEnabledSettings(dialog.NewTabProtectionPassword);
                _isTabProtectionUnlocked = true;
                SaveTabProtectionSettings();
            }
            else if (dialog.RemoveTabProtection)
            {
                _tabProtectionSettings = TabProtectionSettings.Disabled;
                _isTabProtectionUnlocked = false;
                SaveTabProtectionSettings();
            }

            UpdateTabProtectionUi();
            AppendMessage("App-Einstellungen gespeichert.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Windows.MessageBox.Show(
                this,
                $"App-Einstellungen konnten nicht gespeichert werden: {ex.Message}",
                "XDTBox Einstellungen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void OpenHelpCenter_Click(object sender, RoutedEventArgs e)
    {
        var window = new HelpCenterWindow
        {
            Owner = this
        };
        window.Show();
    }

    private void OpenAboutDialog_Click(object sender, RoutedEventArgs e)
    {
        var window = new AboutXdtBoxWindow(GetApplicationVersionText())
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void ShowLicenseCustomerData(LicenseRequestCustomer customer)
    {
        LicenseCustomerNumberTextBox.Text = customer.CustomerNumber ?? string.Empty;
        LicenseCustomerNameTextBox.Text = customer.CustomerName;
        LicenseCustomerStreetTextBox.Text = customer.Street;
        LicenseCustomerPostalCodeTextBox.Text = customer.PostalCode;
        LicenseCustomerCityTextBox.Text = customer.City;
        LicenseCustomerPhoneTextBox.Text = customer.Phone;
        LicenseCustomerEmailTextBox.Text = customer.Email ?? string.Empty;
        LicenseCustomerContactPersonTextBox.Text = customer.ContactPerson ?? string.Empty;
        LicenseCustomerIbanTextBox.Text = customer.Iban ?? string.Empty;
        LicenseCustomerBicTextBox.Text = customer.Bic ?? string.Empty;
        LicenseCustomerAccountHolderTextBox.Text = customer.AccountHolder ?? string.Empty;
        LicenseSepaConsentCheckBox.IsChecked = customer.SepaDirectDebitConsent;
        LicenseAlwaysInvoiceCheckBox.IsChecked = customer.AlwaysInvoice;
        LicenseInvoiceEmailTextBox.Text = customer.InvoiceEmail ?? string.Empty;
    }

    private LicenseRequestCustomer ReadLicenseCustomerDataFromEditor()
    {
        return new LicenseRequestCustomer(
            CustomerName: LicenseCustomerNameTextBox.Text.Trim(),
            Street: LicenseCustomerStreetTextBox.Text.Trim(),
            PostalCode: LicenseCustomerPostalCodeTextBox.Text.Trim(),
            City: LicenseCustomerCityTextBox.Text.Trim(),
            Phone: LicenseCustomerPhoneTextBox.Text.Trim(),
            Email: NormalizeOptionalText(LicenseCustomerEmailTextBox.Text),
            ContactPerson: NormalizeOptionalText(LicenseCustomerContactPersonTextBox.Text),
            Iban: NormalizeOptionalText(LicenseCustomerIbanTextBox.Text),
            Bic: NormalizeOptionalText(LicenseCustomerBicTextBox.Text),
            AccountHolder: NormalizeOptionalText(LicenseCustomerAccountHolderTextBox.Text),
            SepaDirectDebitConsent: LicenseSepaConsentCheckBox.IsChecked == true,
            AlwaysInvoice: LicenseAlwaysInvoiceCheckBox.IsChecked == true,
            InvoiceEmail: NormalizeOptionalText(LicenseInvoiceEmailTextBox.Text),
            CustomerNumber: NormalizeOptionalText(LicenseCustomerNumberTextBox.Text));
    }

    private IReadOnlyList<string> ValidateLicenseCustomerDataForExport(LicenseRequestCustomer customer)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(customer.CustomerName))
        {
            issues.Add("Praxis-/Firmenname fehlt.");
        }

        if (string.IsNullOrWhiteSpace(customer.Phone) && string.IsNullOrWhiteSpace(customer.Email))
        {
            issues.Add("Bitte Telefonnummer oder E-Mail für Rückfragen angeben.");
        }

        if (customer.SepaDirectDebitConsent && customer.AlwaysInvoice)
        {
            issues.Add("Bitte entweder SEPA-Lastschrift oder Rechnung auswählen, nicht beides.");
        }

        if (customer.SepaDirectDebitConsent && string.IsNullOrWhiteSpace(customer.Iban))
        {
            issues.Add("Für SEPA-Lastschrift bitte eine IBAN angeben.");
        }

        if (customer.SepaDirectDebitConsent && string.IsNullOrWhiteSpace(customer.AccountHolder))
        {
            issues.Add("Für SEPA-Lastschrift bitte den Kontoinhaber angeben.");
        }

        return issues;
    }

    private void LicensePaymentOption_Checked(object sender, RoutedEventArgs e)
    {
        if (sender == LicenseSepaConsentCheckBox && LicenseSepaConsentCheckBox.IsChecked == true)
        {
            LicenseAlwaysInvoiceCheckBox.IsChecked = false;
        }
        else if (sender == LicenseAlwaysInvoiceCheckBox && LicenseAlwaysInvoiceCheckBox.IsChecked == true)
        {
            LicenseSepaConsentCheckBox.IsChecked = false;
        }
    }

    private void SaveLicenseCustomerData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var customer = ReadLicenseCustomerDataFromEditor();
            _licenseCustomerDataRepository.Save(GetLicenseCustomerDataFilePath(paths), customer);
            LicenseCustomerDataStatusText.Text = "Kundendaten gespeichert. Gerätenamen bleiben reine Dokumentation; lizenzpflichtig ist nur die Anzahl aktiver Geräteanbindungen.";
            AppendLicenseMessage("Kundendaten für Lizenzanforderung gespeichert.");
        }
        catch (Exception ex)
        {
            LicenseCustomerDataStatusText.Text = $"Kundendaten konnten nicht gespeichert werden: {ex.Message}";
            AppendLicenseMessage($"Kundendaten konnten nicht gespeichert werden: {ex.Message}");
        }
    }

    private sealed record TechnicianWhiteboardElementTag(string Id, string Kind, string? ImagePath);

    private void LoadTechnicianWhiteboard()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var state = _technicianWhiteboardService.LoadOrEmpty(paths);
            TechnicianWhiteboardCanvas.Children.Clear();
            foreach (var item in state.TextItems)
            {
                AddTechnicianWhiteboardTextElement(item);
            }

            foreach (var item in state.ImageItems)
            {
                AddTechnicianWhiteboardImageElement(item);
            }

            TechnicianWhiteboardStatusText.Text = state.TextItems.Count == 0 && state.ImageItems.Count == 0
                ? "Techniker-Notizen bereit. Es werden keine Patientendaten automatisch eingefügt."
                : "Techniker-Notizen geladen.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or JsonException or NotSupportedException)
        {
            TechnicianWhiteboardStatusText.Text = $"Techniker-Notizen konnten nicht geladen werden: {ex.Message}";
        }
    }

    private void TechnicianWhiteboardTextMode_Click(object sender, RoutedEventArgs e)
    {
        SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Text);
    }

    private void TechnicianWhiteboardHandMode_Click(object sender, RoutedEventArgs e)
    {
        SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Hand);
    }

    private void TechnicianWhiteboardAddText_Click(object sender, RoutedEventArgs e)
    {
        SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Text);
        AddTechnicianWhiteboardTextAt(new System.Windows.Point(40, 40 + TechnicianWhiteboardCanvas.Children.Count * 18));
    }

    private Border AddTechnicianWhiteboardTextAt(System.Windows.Point point, string text = "Neue Notiz")
    {
        var item = new TechnicianWhiteboardTextItem(
            Guid.NewGuid().ToString("N"),
            text,
            Math.Max(0, point.X),
            Math.Max(0, point.Y),
            260,
            90,
            TechnicianWhiteboardBoldCheckBox.IsChecked == true,
            TechnicianWhiteboardItalicCheckBox.IsChecked == true,
            TechnicianWhiteboardUnderlineCheckBox.IsChecked == true,
            TechnicianWhiteboardColorComboBox.SelectedValue as string ?? "#24313A",
            GetSelectedTechnicianWhiteboardFontSize());
        var element = AddTechnicianWhiteboardTextElement(item);
        FocusTechnicianWhiteboardTextElement(element);
        return element;
    }

    private void TechnicianWhiteboardAddImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Bild für Techniker-Notiz auswählen",
            Filter = "Bilder (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var copiedImage = _technicianWhiteboardService.CopyImageIntoWhiteboard(paths, dialog.FileName);
            AddTechnicianWhiteboardImageElement(new TechnicianWhiteboardImageItem(
                Guid.NewGuid().ToString("N"),
                copiedImage,
                60,
                60 + TechnicianWhiteboardCanvas.Children.Count * 18,
                220,
                160));
            TechnicianWhiteboardStatusText.Text = "Bild eingefügt. Beim Speichern wird es als Kundendatenbestand gesichert.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            TechnicianWhiteboardStatusText.Text = $"Bild konnte nicht eingefügt werden: {ex.Message}";
        }
    }

    private void TechnicianWhiteboardCanvas_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files)
        {
            return;
        }

        var dropPoint = e.GetPosition(TechnicianWhiteboardCanvas);
        foreach (var file in files)
        {
            try
            {
                var paths = _appDataPathProvider.GetDefaultUserPaths();
                var copiedImage = _technicianWhiteboardService.CopyImageIntoWhiteboard(paths, file);
                AddTechnicianWhiteboardImageElement(new TechnicianWhiteboardImageItem(
                    Guid.NewGuid().ToString("N"),
                    copiedImage,
                    dropPoint.X,
                    dropPoint.Y,
                    220,
                    160));
                dropPoint.Offset(24, 24);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
            {
                TechnicianWhiteboardStatusText.Text = $"Ein Bild konnte nicht eingefügt werden: {ex.Message}";
            }
        }
    }

    private void TechnicianWhiteboardCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not Canvas)
        {
            return;
        }

        if (_technicianWhiteboardMode == TechnicianWhiteboardMode.Text)
        {
            AddTechnicianWhiteboardTextAt(e.GetPosition(TechnicianWhiteboardCanvas), string.Empty);
            e.Handled = true;
            return;
        }

        ClearTechnicianWhiteboardSelection();
    }

    private void TechnicianWhiteboardCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _technicianWhiteboardContextMenuPosition = e.GetPosition(TechnicianWhiteboardCanvas);
        if (FindTechnicianWhiteboardElement(e.OriginalSource as DependencyObject) is { } element)
        {
            SelectTechnicianWhiteboardElement(element);
        }
    }

    private void TechnicianWhiteboardElement_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _technicianWhiteboardContextMenuPosition = e.GetPosition(TechnicianWhiteboardCanvas);
        if (sender is Border element)
        {
            SelectTechnicianWhiteboardElement(element);
        }
    }

    private void TechnicianWhiteboardContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        var hasSelection = _technicianWhiteboardSelectedElement is not null;
        TechnicianWhiteboardCopyMenuItem.IsEnabled = hasSelection;
        TechnicianWhiteboardRemoveMenuItem.IsEnabled = hasSelection;
        TechnicianWhiteboardPasteMenuItem.IsEnabled = System.Windows.Clipboard.ContainsImage() || System.Windows.Clipboard.ContainsText();
    }

    private void TechnicianWhiteboardCopy_Click(object sender, RoutedEventArgs e)
    {
        if (_technicianWhiteboardSelectedElement?.Child is not Grid grid)
        {
            return;
        }

        if (grid.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault() is { } textBox)
        {
            System.Windows.Clipboard.SetText(textBox.Text ?? string.Empty);
            TechnicianWhiteboardStatusText.Text = "Text in die Zwischenablage kopiert.";
            return;
        }

        if (grid.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault()?.Source is BitmapSource bitmap)
        {
            System.Windows.Clipboard.SetImage(bitmap);
            TechnicianWhiteboardStatusText.Text = "Bild in die Zwischenablage kopiert.";
        }
    }

    private void TechnicianWhiteboardPaste_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                var imagePath = SaveClipboardImageToTechnicianWhiteboard();
                AddTechnicianWhiteboardImageElement(new TechnicianWhiteboardImageItem(
                    Guid.NewGuid().ToString("N"),
                    imagePath,
                    _technicianWhiteboardContextMenuPosition.X,
                    _technicianWhiteboardContextMenuPosition.Y,
                    220,
                    160));
                TechnicianWhiteboardStatusText.Text = "Bild aus der Zwischenablage eingefügt.";
                return;
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Text);
                AddTechnicianWhiteboardTextAt(_technicianWhiteboardContextMenuPosition, System.Windows.Clipboard.GetText());
                TechnicianWhiteboardStatusText.Text = "Text aus der Zwischenablage eingefügt.";
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ExternalException or NotSupportedException)
        {
            TechnicianWhiteboardStatusText.Text = $"Einfügen ist fehlgeschlagen: {ex.Message}";
        }
    }

    private void TechnicianWhiteboardRemove_Click(object sender, RoutedEventArgs e)
    {
        if (_technicianWhiteboardSelectedElement is null)
        {
            return;
        }

        TechnicianWhiteboardCanvas.Children.Remove(_technicianWhiteboardSelectedElement);
        _technicianWhiteboardSelectedElement = null;
        TechnicianWhiteboardStatusText.Text = "Element entfernt.";
    }

    private string SaveClipboardImageToTechnicianWhiteboard()
    {
        var bitmap = System.Windows.Clipboard.GetImage() ?? throw new InvalidOperationException("Die Zwischenablage enthält kein unterstütztes Bild.");
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        var imagesFolder = _technicianWhiteboardService.GetImagesFolder(paths);
        Directory.CreateDirectory(imagesFolder);
        var targetPath = Path.Combine(imagesFolder, $"{Guid.NewGuid():N}.png");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(targetPath);
        encoder.Save(stream);
        return targetPath;
    }

    private static Border? FindTechnicianWhiteboardElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Border { Tag: TechnicianWhiteboardElementTag })
            {
                return (Border)source;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }

    private void TechnicianWhiteboardInsertActiveDevices_Click(object sender, RoutedEventArgs e)
    {
        if (_profileCatalog is null)
        {
            TechnicianWhiteboardStatusText.Text = "Profilkatalog ist nicht geladen.";
            return;
        }

        var activeProfiles = _profileCatalog.InterfaceProfiles
            .Where(profile => profile.IsActive)
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        if (activeProfiles.Length == 0)
        {
            TechnicianWhiteboardStatusText.Text = "Es sind aktuell keine Geräteanbindungen aktiv.";
            return;
        }

        var y = 40d;
        foreach (var interfaceProfile in activeProfiles)
        {
            var deviceProfile = GetDeviceProfile(interfaceProfile.DeviceProfileId);
            var text = deviceProfile is null
                ? interfaceProfile.Metadata.Name
                : $"{deviceProfile.Metadata.Name}\n{deviceProfile.Manufacturer}";
            AddTechnicianWhiteboardTextElement(new TechnicianWhiteboardTextItem(
                Guid.NewGuid().ToString("N"),
                text,
                40,
                y,
                260,
                70,
                IsBold: true,
                IsItalic: false,
                IsUnderline: false,
                Color: "#24313A",
                FontSize: 16));

            if (deviceProfile is not null)
            {
                var imagePath = ResolveDeviceImagePath(deviceProfile);
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    AddTechnicianWhiteboardImageElement(new TechnicianWhiteboardImageItem(
                        Guid.NewGuid().ToString("N"),
                        imagePath,
                        330,
                        y,
                        130,
                        95));
                }
            }

            y += 120;
        }

        TechnicianWhiteboardStatusText.Text = "Aktivierte Geräte wurden ohne Patientendaten, Ordnerpfade oder Lizenzdetails eingefügt.";
    }

    private void TechnicianWhiteboardSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _technicianWhiteboardService.Save(paths, CreateTechnicianWhiteboardStateFromCanvas());
            TechnicianWhiteboardStatusText.Text = "Techniker-Notizen gespeichert.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            TechnicianWhiteboardStatusText.Text = $"Techniker-Notizen konnten nicht gespeichert werden: {ex.Message}";
        }
    }

    private void TechnicianWhiteboardExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Techniker-Notizen als PDF speichern",
            Filter = "PDF-Datei (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = $"XDTBox-Techniker-Notizen-{DateTime.Now:yyyyMMdd-HHmm}.pdf"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ExportWhiteboardCanvasAsPdf(TechnicianWhiteboardCanvas, dialog.FileName);
            TechnicianWhiteboardStatusText.Text = $"PDF gespeichert: {dialog.FileName}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException)
        {
            TechnicianWhiteboardStatusText.Text = $"PDF konnte nicht gespeichert werden: {ex.Message}";
        }
    }

    private Border AddTechnicianWhiteboardTextElement(TechnicianWhiteboardTextItem item)
    {
        var textBox = new System.Windows.Controls.TextBox
        {
            Text = item.Text,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            BorderThickness = new Thickness(0),
            BorderBrush = System.Windows.Media.Brushes.Transparent,
            Background = System.Windows.Media.Brushes.Transparent,
            FocusVisualStyle = null,
            Padding = new Thickness(4),
            FontWeight = item.IsBold ? FontWeights.Bold : FontWeights.Normal,
            FontStyle = item.IsItalic ? FontStyles.Italic : FontStyles.Normal,
            TextDecorations = item.IsUnderline ? TextDecorations.Underline : null,
            Foreground = CreateBrush(item.Color),
            FontSize = NormalizeTechnicianWhiteboardFontSize(item.FontSize),
            IsReadOnly = _technicianWhiteboardMode == TechnicianWhiteboardMode.Hand
        };

        var border = CreateWhiteboardElementBorder(item.Id, "Text", null, item.Width, item.Height, textBox);
        Canvas.SetLeft(border, item.X);
        Canvas.SetTop(border, item.Y);
        TechnicianWhiteboardCanvas.Children.Add(border);
        UpdateTechnicianWhiteboardElementVisual(border);
        return border;
    }

    private Border AddTechnicianWhiteboardImageElement(TechnicianWhiteboardImageItem item)
    {
        var image = new System.Windows.Controls.Image
        {
            Stretch = Stretch.Uniform,
            Source = new DeviceImageSourceConverter().Convert(item.ImagePath, typeof(ImageSource), null, CultureInfo.CurrentCulture) as ImageSource
        };
        var border = CreateWhiteboardElementBorder(item.Id, "Image", item.ImagePath, item.Width, item.Height, image);
        Canvas.SetLeft(border, item.X);
        Canvas.SetTop(border, item.Y);
        TechnicianWhiteboardCanvas.Children.Add(border);
        UpdateTechnicianWhiteboardElementVisual(border);
        return border;
    }

    private Border CreateWhiteboardElementBorder(string id, string kind, string? imagePath, double width, double height, UIElement content)
    {
        var root = new Grid();
        root.Children.Add(content);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Left, System.Windows.VerticalAlignment.Top, System.Windows.Input.Cursors.SizeNWSE, true, true, false, false);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Stretch, System.Windows.VerticalAlignment.Top, System.Windows.Input.Cursors.SizeNS, false, true, false, false);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Right, System.Windows.VerticalAlignment.Top, System.Windows.Input.Cursors.SizeNESW, false, true, true, false);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Left, System.Windows.VerticalAlignment.Stretch, System.Windows.Input.Cursors.SizeWE, true, false, false, false);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Right, System.Windows.VerticalAlignment.Stretch, System.Windows.Input.Cursors.SizeWE, false, false, true, false);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Left, System.Windows.VerticalAlignment.Bottom, System.Windows.Input.Cursors.SizeNESW, true, false, false, true);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Stretch, System.Windows.VerticalAlignment.Bottom, System.Windows.Input.Cursors.SizeNS, false, false, false, true);
        AddWhiteboardResizeThumb(root, System.Windows.HorizontalAlignment.Right, System.Windows.VerticalAlignment.Bottom, System.Windows.Input.Cursors.SizeNWSE, false, false, true, true);

        var element = new Border
        {
            Width = width,
            Height = height,
            MinWidth = 80,
            MinHeight = 45,
            Padding = new Thickness(0),
            Background = System.Windows.Media.Brushes.Transparent,
            BorderBrush = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            FocusVisualStyle = null,
            Tag = new TechnicianWhiteboardElementTag(id, kind, imagePath),
            Child = root
        };
        element.PreviewMouseLeftButtonDown += TechnicianWhiteboardElement_MouseLeftButtonDown;
        element.PreviewMouseMove += TechnicianWhiteboardElement_MouseMove;
        element.PreviewMouseLeftButtonUp += TechnicianWhiteboardElement_MouseLeftButtonUp;
        element.PreviewMouseRightButtonDown += TechnicianWhiteboardElement_PreviewMouseRightButtonDown;
        if (content is System.Windows.Controls.TextBox textBox)
        {
            textBox.GotKeyboardFocus += (_, _) => SelectTechnicianWhiteboardElement(element);
            textBox.PreviewMouseLeftButtonDown += (_, _) => SelectTechnicianWhiteboardElement(element);
        }

        return element;
    }

    private static void AddWhiteboardResizeThumb(
        Grid root,
        System.Windows.HorizontalAlignment horizontalAlignment,
        System.Windows.VerticalAlignment verticalAlignment,
        System.Windows.Input.Cursor cursor,
        bool resizeLeft,
        bool resizeTop,
        bool resizeRight,
        bool resizeBottom)
    {
        var isHorizontalEdge = horizontalAlignment == System.Windows.HorizontalAlignment.Stretch;
        var isVerticalEdge = verticalAlignment == System.Windows.VerticalAlignment.Stretch;
        var thumb = new Thumb
        {
            Width = isVerticalEdge ? 8 : isHorizontalEdge ? double.NaN : 11,
            Height = isHorizontalEdge ? 8 : isVerticalEdge ? double.NaN : 11,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Cursor = cursor,
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = CreateBrush("#2F7FD1"),
            BorderThickness = new Thickness(0),
            FocusVisualStyle = null,
            Template = CreateWhiteboardThumbTemplate(),
            Opacity = 1,
            Visibility = Visibility.Collapsed,
            Tag = "WhiteboardResizeThumb"
        };
        thumb.DragDelta += (_, args) =>
        {
            if (root.Parent is Border border)
            {
                ResizeTechnicianWhiteboardElement(border, resizeLeft, resizeTop, resizeRight, resizeBottom, args.HorizontalChange, args.VerticalChange);
            }
        };
        root.Children.Add(thumb);
    }

    private static ControlTemplate CreateWhiteboardThumbTemplate()
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.BackgroundProperty, System.Windows.Media.Brushes.White);
        border.SetValue(Border.BorderBrushProperty, CreateBrush("#2F7FD1"));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(1));
        return new ControlTemplate(typeof(Thumb))
        {
            VisualTree = border
        };
    }

    private static void ResizeTechnicianWhiteboardElement(Border element, bool resizeLeft, bool resizeTop, bool resizeRight, bool resizeBottom, double dx, double dy)
    {
        const double minWidth = 80;
        const double minHeight = 45;
        var left = Canvas.GetLeft(element);
        var top = Canvas.GetTop(element);
        left = double.IsNaN(left) ? 0 : left;
        top = double.IsNaN(top) ? 0 : top;
        var width = element.Width;
        var height = element.Height;

        if (resizeLeft)
        {
            var constrainedDx = Math.Min(dx, width - minWidth);
            constrainedDx = Math.Max(constrainedDx, -left);
            element.Width = width - constrainedDx;
            Canvas.SetLeft(element, left + constrainedDx);
        }
        else if (resizeRight)
        {
            element.Width = Math.Max(minWidth, width + dx);
        }

        if (resizeTop)
        {
            var constrainedDy = Math.Min(dy, height - minHeight);
            constrainedDy = Math.Max(constrainedDy, -top);
            element.Height = height - constrainedDy;
            Canvas.SetTop(element, top + constrainedDy);
        }
        else if (resizeBottom)
        {
            element.Height = Math.Max(minHeight, height + dy);
        }
    }

    private void TechnicianWhiteboardElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        SelectTechnicianWhiteboardElement(element);
        if (IsWhiteboardResizeThumbSource(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (element.Tag is TechnicianWhiteboardElementTag { Kind: "Text" } && e.ClickCount >= 2)
        {
            SetTechnicianWhiteboardMode(TechnicianWhiteboardMode.Text);
            FocusTechnicianWhiteboardTextElement(element);
            e.Handled = true;
            return;
        }

        if (_technicianWhiteboardMode != TechnicianWhiteboardMode.Hand
            && IsWhiteboardInteractiveSource(e.OriginalSource as DependencyObject))
        {
            return;
        }

        _technicianWhiteboardDraggedElement = element;
        _technicianWhiteboardDragStart = e.GetPosition(TechnicianWhiteboardCanvas);
        element.CaptureMouse();
        e.Handled = true;
    }

    private void TechnicianWhiteboardElement_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_technicianWhiteboardDraggedElement is null || _technicianWhiteboardDragStart is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = e.GetPosition(TechnicianWhiteboardCanvas);
        var dx = current.X - _technicianWhiteboardDragStart.Value.X;
        var dy = current.Y - _technicianWhiteboardDragStart.Value.Y;
        Canvas.SetLeft(_technicianWhiteboardDraggedElement, Math.Max(0, Canvas.GetLeft(_technicianWhiteboardDraggedElement) + dx));
        Canvas.SetTop(_technicianWhiteboardDraggedElement, Math.Max(0, Canvas.GetTop(_technicianWhiteboardDraggedElement) + dy));
        _technicianWhiteboardDragStart = current;
    }

    private void TechnicianWhiteboardElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _technicianWhiteboardDraggedElement?.ReleaseMouseCapture();
        _technicianWhiteboardDraggedElement = null;
        _technicianWhiteboardDragStart = null;
    }

    private void SelectTechnicianWhiteboardElement(Border element)
    {
        if (_technicianWhiteboardSelectedElement is not null && !ReferenceEquals(_technicianWhiteboardSelectedElement, element))
        {
            UpdateTechnicianWhiteboardElementVisual(_technicianWhiteboardSelectedElement);
        }

        _technicianWhiteboardSelectedElement = element;
        UpdateTechnicianWhiteboardElementVisual(element);
    }

    private void ClearTechnicianWhiteboardSelection()
    {
        if (_technicianWhiteboardSelectedElement is not null)
        {
            var previous = _technicianWhiteboardSelectedElement;
            _technicianWhiteboardSelectedElement = null;
            UpdateTechnicianWhiteboardElementVisual(previous);
        }
    }

    private void UpdateTechnicianWhiteboardElementVisual(Border element)
    {
        var isSelectedInHandMode = ReferenceEquals(_technicianWhiteboardSelectedElement, element)
            && _technicianWhiteboardMode == TechnicianWhiteboardMode.Hand;
        element.BorderBrush = isSelectedInHandMode
            ? CreateBrush("#2F7FD1")
            : System.Windows.Media.Brushes.Transparent;
        element.BorderThickness = isSelectedInHandMode
            ? new Thickness(1)
            : new Thickness(0);
        element.Padding = isSelectedInHandMode
            ? new Thickness(4)
            : new Thickness(0);

        if (element.Child is Grid grid)
        {
            foreach (var thumb in grid.Children.OfType<Thumb>())
            {
                thumb.Visibility = isSelectedInHandMode ? Visibility.Visible : Visibility.Collapsed;
            }

            if (grid.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault() is { } textBox)
            {
                textBox.IsReadOnly = _technicianWhiteboardMode == TechnicianWhiteboardMode.Hand;
                textBox.Cursor = _technicianWhiteboardMode == TechnicianWhiteboardMode.Hand
                    ? System.Windows.Input.Cursors.SizeAll
                    : System.Windows.Input.Cursors.IBeam;
            }
        }
    }

    private void SetTechnicianWhiteboardMode(TechnicianWhiteboardMode mode)
    {
        _technicianWhiteboardMode = mode;
        if (TechnicianWhiteboardTextModeButton is not null)
        {
            TechnicianWhiteboardTextModeButton.IsChecked = mode == TechnicianWhiteboardMode.Text;
        }

        if (TechnicianWhiteboardHandModeButton is not null)
        {
            TechnicianWhiteboardHandModeButton.IsChecked = mode == TechnicianWhiteboardMode.Hand;
        }

        TechnicianWhiteboardCanvas.Cursor = mode == TechnicianWhiteboardMode.Text
            ? System.Windows.Input.Cursors.IBeam
            : System.Windows.Input.Cursors.Arrow;

        foreach (var child in TechnicianWhiteboardCanvas.Children.OfType<Border>())
        {
            UpdateTechnicianWhiteboardElementVisual(child);
        }

        TechnicianWhiteboardStatusText.Text = mode == TechnicianWhiteboardMode.Text
            ? "Textmodus aktiv: Klicken Sie auf das Whiteboard und schreiben Sie direkt."
            : "Handmodus aktiv: Elemente können ausgewählt, verschoben und skaliert werden.";
    }

    private void FocusTechnicianWhiteboardTextElement(Border element)
    {
        if (element.Child is Grid grid
            && grid.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault() is { } textBox)
        {
            SelectTechnicianWhiteboardElement(element);
            textBox.Focus();
            textBox.CaretIndex = textBox.Text.Length;
        }
    }

    private void TechnicianWhiteboardFormatting_Changed(object sender, RoutedEventArgs e)
    {
        ApplyTechnicianWhiteboardFormattingToSelectedText();
    }

    private void TechnicianWhiteboardFormatting_Changed(object sender, SelectionChangedEventArgs e)
    {
        ApplyTechnicianWhiteboardFormattingToSelectedText();
    }

    private void ApplyTechnicianWhiteboardFormattingToSelectedText()
    {
        if (_technicianWhiteboardSelectedElement?.Tag is not TechnicianWhiteboardElementTag tag
            || !string.Equals(tag.Kind, "Text", StringComparison.OrdinalIgnoreCase)
            || _technicianWhiteboardSelectedElement.Child is not Grid grid
            || grid.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault() is not { } textBox)
        {
            return;
        }

        textBox.FontWeight = TechnicianWhiteboardBoldCheckBox.IsChecked == true ? FontWeights.Bold : FontWeights.Normal;
        textBox.FontStyle = TechnicianWhiteboardItalicCheckBox.IsChecked == true ? FontStyles.Italic : FontStyles.Normal;
        textBox.TextDecorations = TechnicianWhiteboardUnderlineCheckBox.IsChecked == true ? TextDecorations.Underline : null;
        textBox.Foreground = CreateBrush(TechnicianWhiteboardColorComboBox.SelectedValue as string ?? "#24313A");
        textBox.FontSize = GetSelectedTechnicianWhiteboardFontSize();
    }

    private double GetSelectedTechnicianWhiteboardFontSize()
    {
        var rawValue = TechnicianWhiteboardFontSizeComboBox.SelectedValue?.ToString();
        return double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize)
            ? NormalizeTechnicianWhiteboardFontSize(fontSize)
            : 16;
    }

    private static double NormalizeTechnicianWhiteboardFontSize(double fontSize)
    {
        if (double.IsNaN(fontSize) || double.IsInfinity(fontSize) || fontSize <= 0)
        {
            return 16;
        }

        return Math.Clamp(fontSize, 8, 72);
    }

    private static bool IsWhiteboardInteractiveSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is System.Windows.Controls.TextBox || source is Thumb)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static bool IsWhiteboardResizeThumbSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is Thumb)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private TechnicianWhiteboardState CreateTechnicianWhiteboardStateFromCanvas()
    {
        var textItems = new List<TechnicianWhiteboardTextItem>();
        var imageItems = new List<TechnicianWhiteboardImageItem>();
        foreach (var child in TechnicianWhiteboardCanvas.Children.OfType<Border>())
        {
            if (child.Tag is not TechnicianWhiteboardElementTag tag)
            {
                continue;
            }

            var x = Canvas.GetLeft(child);
            var y = Canvas.GetTop(child);
            if (string.Equals(tag.Kind, "Text", StringComparison.OrdinalIgnoreCase)
                && child.Child is Grid grid
                && grid.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault() is { } textBox)
            {
                textItems.Add(new TechnicianWhiteboardTextItem(
                    tag.Id,
                    textBox.Text,
                    x,
                    y,
                    child.Width,
                    child.Height,
                    textBox.FontWeight == FontWeights.Bold,
                    textBox.FontStyle == FontStyles.Italic,
                    textBox.TextDecorations is not null && textBox.TextDecorations.Count > 0,
                    BrushToHex(textBox.Foreground),
                    textBox.FontSize));
            }
            else if (string.Equals(tag.Kind, "Image", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(tag.ImagePath))
            {
                imageItems.Add(new TechnicianWhiteboardImageItem(tag.Id, tag.ImagePath, x, y, child.Width, child.Height));
            }
        }

        return new TechnicianWhiteboardState(textItems, imageItems);
    }

    private string ResolveDeviceImagePath(DeviceProfileDefinition deviceProfile)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _deviceProfileImageOverrideService.LoadOverrides(paths).TryGetValue(deviceProfile.Metadata.Id, out var overridePath);
            return _deviceProfileImageOverrideService.ResolveEffectiveImagePath(deviceProfile, overridePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return deviceProfile.DeviceImagePath;
        }
    }

    private static SolidColorBrush CreateBrush(string colorText)
    {
        try
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(colorText)!;
        }
        catch (Exception ex) when (ex is FormatException or NotSupportedException)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 49, 58));
        }
    }

    private static string BrushToHex(System.Windows.Media.Brush brush)
    {
        return brush is SolidColorBrush solid
            ? $"#{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
            : "#24313A";
    }

    private static void ExportWhiteboardCanvasAsPdf(Canvas canvas, string filePath)
    {
        var width = Math.Max(1, (int)Math.Ceiling(canvas.ActualWidth > 0 ? canvas.ActualWidth : canvas.Width));
        var height = Math.Max(1, (int)Math.Ceiling(canvas.ActualHeight > 0 ? canvas.ActualHeight : canvas.Height));
        canvas.Measure(new System.Windows.Size(width, height));
        canvas.Arrange(new Rect(0, 0, width, height));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);
        var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var imageStream = new MemoryStream();
        encoder.Save(imageStream);
        WriteSingleImagePdf(filePath, imageStream.ToArray(), width, height);
    }

    private static void WriteSingleImagePdf(string filePath, byte[] jpegBytes, int width, int height)
    {
        var offsets = new List<long> { 0 };
        using var stream = File.Create(filePath);
        WriteAscii(stream, "%PDF-1.4\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        offsets.Add(stream.Position);
        WriteAscii(stream, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        offsets.Add(stream.Position);
        WriteAscii(stream, $"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
        offsets.Add(stream.Position);
        WriteAscii(stream, $"4 0 obj\n<< /Type /XObject /Subtype /Image /Width {width} /Height {height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpegBytes.Length} >>\nstream\n");
        stream.Write(jpegBytes, 0, jpegBytes.Length);
        WriteAscii(stream, "\nendstream\nendobj\n");
        var content = $"q\n{width} 0 0 {height} 0 0 cm\n/Im0 Do\nQ\n";
        offsets.Add(stream.Position);
        WriteAscii(stream, $"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream\nendobj\n");

        var xrefOffset = stream.Position;
        WriteAscii(stream, "xref\n0 6\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(stream, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(stream, $"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
    }

    private static void WriteAscii(Stream stream, string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    private void RefreshLicensedDeviceStatesFromLocalLicense()
    {
        try
        {
            LicenseInfo? license = null;
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var signedLicenseFile = GetSignedLicenseFilePath(paths);
            if (_installationInfo is not null && File.Exists(signedLicenseFile))
            {
                ShowSignedLicenseStatus(signedLicenseFile, _installationInfo);
                return;
            }

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

    private LicenseInfo? LoadCurrentDisplayLicenseFromLocalSource(AppDataPaths paths, InstallationInfo? installation)
    {
        var signedLicenseFile = GetSignedLicenseFilePath(paths);
        if (installation is not null && File.Exists(signedLicenseFile))
        {
            var result = _licenseImportService.ImportFromFile(
                signedLicenseFile,
                installation,
                CountActiveDeviceConnectionsForLicenseV1(),
                DateTime.UtcNow);

            return result.Payload is not null && result.SignatureStatus == LicenseSignatureVerificationStatus.Valid
                ? CreateDisplayLicenseInfo(result.Payload, "RSA-PSS-SHA256 geprüft")
                : null;
        }

        return File.Exists(paths.LicenseFile)
            ? _licenseFileRepository.Load(paths.LicenseFile)
            : null;
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
        var deviceLocationsByInterfaceProfileId = LoadLicenseDeviceLocationStoreOrEmpty().ToDictionary();

        _licensedDeviceStateRows.Clear();
        foreach (var state in states.OrderBy(state => state.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            deviceLocationsByInterfaceProfileId.TryGetValue(state.InterfaceProfileId, out var location);
            _licensedDeviceStateRows.Add(LicenseDeviceStateRow.FromState(state, location));
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
                licenseStates,
                LoadDeviceImageOverrides())
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
        _lastMonitoringScanQueuesByProfileId.Clear();
        CloseAllFloatingMonitoringWindows();
        ActiveInterfaceProfilesStatusText.Text = message;
    }

    private IReadOnlyDictionary<string, string> LoadDeviceImageOverrides()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            return _deviceProfileImageOverrideService.LoadOverrides(paths);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string GetSignedLicenseFilePath(AppDataPaths paths)
    {
        return LocalLicenseRemovalService.GetSignedLicenseFilePath(paths);
    }

    private static string GetLicenseCustomerDataFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.LicensesFolder, "license-customer-data.json");
    }

    private static string GetLicenseDeviceLocationsFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.LicensesFolder, "license-device-locations.json");
    }

    private static string? NormalizeOptionalText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void ShowSignedLicenseStatus(string licenseFile, InstallationInfo installation)
    {
        var result = _licenseImportService.ImportFromFile(
            licenseFile,
            installation,
            CountActiveDeviceConnectionsForLicenseV1(),
            DateTime.UtcNow);

        var license = result.Payload is not null && result.SignatureStatus == LicenseSignatureVerificationStatus.Valid
            ? CreateDisplayLicenseInfo(result.Payload, "RSA-PSS-SHA256 geprüft")
            : null;

        ShowLicensedDeviceStates(license);
        ShowLicenseStatus(
            installation,
            result.UserMessage,
            CountActiveDeviceConnectionsForLicenseV1(),
            result.Payload?.MaxActiveDeviceConnections ?? 0);
        LicenseMessagesTextBox.Text = result.UserMessage;
    }

    private static LicenseInfo CreateDisplayLicenseInfo(LicensePayload payload, string signatureLabel)
    {
        return new LicenseInfo(
            LicenseId: payload.LicenseId,
            CustomerName: payload.LicenseeName,
            CustomerNumber: payload.CustomerNumber ?? string.Empty,
            InstallationId: payload.InstallationId,
            LicensedDeviceCount: payload.MaxActiveDeviceConnections,
            ValidFrom: payload.ValidFromUtc,
            ValidUntil: payload.ValidUntilUtc,
            LicenseType: MapLicenseType(payload.LicenseType),
            ProductCode: payload.ProductCode,
            MinimumAppVersion: string.Empty,
            IssuedAt: payload.IssuedAtUtc,
            Signature: signatureLabel);
    }

    private static LicenseType MapLicenseType(string? licenseType)
    {
        return (licenseType ?? string.Empty).Trim() switch
        {
            var value when value.Equals("Trial", StringComparison.OrdinalIgnoreCase) => LicenseType.Trial,
            var value when value.Equals("Monthly", StringComparison.OrdinalIgnoreCase) => LicenseType.Monthly,
            var value when value.Equals("Perpetual", StringComparison.OrdinalIgnoreCase) => LicenseType.Perpetual,
            _ => LicenseType.Annual
        };
    }

    private void LoadFloatingWindowStates()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _floatingWindowStateService.ReplaceAll(_floatingWindowStateRepository.Load(paths));
        }
        catch
        {
            // UI-Zustand ist optional; ein defekter State darf den App-Start nicht blockieren.
            _floatingWindowStateService.ReplaceAll(Array.Empty<InterfaceProfileFloatingWindowState>());
        }
    }

    private void SaveFloatingWindowStates()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _floatingWindowStateRepository.Save(paths, _floatingWindowStateService.GetAll());
        }
        catch (Exception ex)
        {
            AppendMessage($"Fensterpositionen konnten nicht gespeichert werden: {ex.Message}");
        }
    }

    private void RestoreFloatingWindowsOnce()
    {
        if (!_floatingWindowRestoreGate.MarkMainWindowReady())
        {
            return;
        }

        RefreshInterfaceMonitoringCards();
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
                var hasPendingNidekRtSerialReturn = _nidekRtSerialSendContexts.ContainsKey(row.MonitoringCard.InterfaceProfileId);
                var desiredCard = (runtimeCard with
                {
                    CurrentStatus = runtimeState.CurrentStatus,
                    StatusClass = runtimeState.StatusClass,
                    LastScanText = runtimeState.LastScanText,
                    AutomaticProcessingText = IsAutomaticPairProcessingEnabled() ? "Ja" : "Nein",
                    IsDetailsExpanded = GetMonitoringDetailsExpanded(row.MonitoringCard.InterfaceProfileId),
                    HasNidekRtSerialPendingReturn = hasPendingNidekRtSerialReturn
                }).WithPilotMonitoringActivity(isMonitoringActive);
                var floatingState = _floatingWindowStateService.GetOrCreate(row.MonitoringCard.InterfaceProfileId);
                if (floatingState.IsDetached && _floatingWindowRestoreGate.CanShowFloatingWindows)
                {
                    if (TryShowOrUpdateFloatingMonitoringWindow(desiredCard, floatingState))
                    {
                        continue;
                    }
                }

                if (!floatingState.IsDetached)
                {
                    CloseFloatingMonitoringWindow(row.MonitoringCard.InterfaceProfileId);
                }

                desiredCards.Add(desiredCard);
            }

            CloseRemovedFloatingMonitoringWindows();
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

    private void DetachMonitoringCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not InterfaceMonitoringCardDisplay card)
        {
            return;
        }

        var state = _floatingWindowStateService.Detach(card.InterfaceProfileId);
        SaveFloatingWindowStates();
        var detached = false;
        if (_floatingWindowRestoreGate.CanShowFloatingWindows)
        {
            detached = TryShowOrUpdateFloatingMonitoringWindow(card, state);
        }

        RefreshInterfaceMonitoringCards();
        if (detached)
        {
            AppendMonitoringEvent(
                card.InterfaceProfileId,
                "monitoring-card-detached",
                $"{card.InterfaceProfileName}: Gerätekarte abgedockt.");
        }
    }

    private void ResetMonitoringCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element
            || element.DataContext is not InterfaceMonitoringCardDisplay card)
        {
            return;
        }

        ResetMonitoringProfile(card.InterfaceProfileId);
    }

    private void DockFloatingMonitoringCard(string interfaceProfileId)
    {
        _interfaceProfileAutoRedockService.NotifyDocked(interfaceProfileId);
        EnsureAutoRedockTimerState();
        var state = _floatingWindowStateService.Dock(interfaceProfileId);
        _ = state;
        SaveFloatingWindowStates();
        CloseFloatingMonitoringWindow(interfaceProfileId);
        RefreshInterfaceMonitoringCards();

        var profileName = _interfaceMonitoringRuntimeCards.TryGetValue(interfaceProfileId, out var card)
            ? card.InterfaceProfileName
            : interfaceProfileId;
        AppendMonitoringEvent(
            interfaceProfileId,
            "monitoring-card-docked",
            $"{profileName}: Gerätekarte wieder angedockt.");
    }

    private bool TryShowOrUpdateFloatingMonitoringWindow(
        InterfaceMonitoringCardDisplay card,
        InterfaceProfileFloatingWindowState state)
    {
        try
        {
            ShowOrUpdateFloatingMonitoringWindow(card, state);
            return true;
        }
        catch (Exception ex)
        {
            HandleFloatingWindowRestoreFailure(card, ex);
            return false;
        }
    }

    private void ShowOrUpdateFloatingMonitoringWindow(
        InterfaceMonitoringCardDisplay card,
        InterfaceProfileFloatingWindowState state)
    {
        if (!_floatingMonitoringWindows.TryGetValue(card.InterfaceProfileId, out var window))
        {
            window = new FloatingInterfaceProfileWindow(card.InterfaceProfileId);
            window.DockRequested += FloatingMonitoringWindow_DockRequested;
            window.PinChanged += FloatingMonitoringWindow_PinChanged;
            window.PositionMemoryChanged += FloatingMonitoringWindow_PositionMemoryChanged;
            window.PositionRememberRequested += FloatingMonitoringWindow_PositionRememberRequested;
            window.ScanIntervalChangeRequested += FloatingMonitoringWindow_ScanIntervalChangeRequested;
            window.ResetRequested += FloatingMonitoringWindow_ResetRequested;
            window.SerialListenOnlyRequested += FloatingMonitoringWindow_SerialListenOnlyRequested;
            window.SerialProcessReturnRequested += FloatingMonitoringWindow_SerialProcessReturnRequested;
            window.Closed += FloatingMonitoringWindow_Closed;
            _floatingMonitoringWindows[card.InterfaceProfileId] = window;
            ApplyFloatingWindowPlacement(window, state);
            window.Show();
        }

        window.Title = card.InterfaceProfileName;
        window.DataContext = card;
        window.ApplyState(state);
        if (!window.IsVisible)
        {
            window.Show();
        }

        window.Activate();
    }

    private void FloatingMonitoringWindow_Closed(object? sender, EventArgs e)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        _floatingMonitoringWindows.Remove(window.InterfaceProfileId);
    }

    private void HandleFloatingWindowRestoreFailure(InterfaceMonitoringCardDisplay card, Exception ex)
    {
        _interfaceProfileAutoRedockService.NotifyDocked(card.InterfaceProfileId);
        EnsureAutoRedockTimerState();
        _floatingWindowStateService.Dock(card.InterfaceProfileId);
        SaveFloatingWindowStates();
        CloseFloatingMonitoringWindow(card.InterfaceProfileId);
        AppendMonitoringEvent(
            card.InterfaceProfileId,
            "floating-window-restore-failed",
            $"{card.InterfaceProfileName}: Abgedocktes Fenster konnte nicht wiederhergestellt werden und wurde angedockt angezeigt. {ex.Message}",
            InterfaceMonitoringEventSeverity.Warning);
    }

    private void TryAutoDetachMonitoringCardForActivity(InterfaceMonitoringEventEntry entry)
    {
        if (!_floatingWindowRestoreGate.CanShowFloatingWindows)
        {
            return;
        }

        var row = _activeInterfaceProfileStatusRows.FirstOrDefault(item =>
            string.Equals(item.MonitoringCard.InterfaceProfileId, entry.ScopeId, StringComparison.OrdinalIgnoreCase));
        if (row is null)
        {
            return;
        }

        var currentState = _floatingWindowStateService.GetOrCreate(entry.ScopeId);
        var allowAutoDetach = !IsManualDocumentSelectionProfile(entry.ScopeId);
        if (allowAutoDetach
            && IsNidekRtSerialWorkflowProfile(entry.ScopeId)
            && (_nidekRtSerialCompletedWorkflowProfiles.Contains(entry.ScopeId)
                || !_nidekRtSerialSendContexts.ContainsKey(entry.ScopeId)))
        {
            return;
        }

        var decision = _interfaceProfileAutoDetachService.Evaluate(entry, currentState, allowAutoDetach);
        if (!decision.IsRelevantActivity || decision.IsSuppressedByCooldown)
        {
            return;
        }

        if (!allowAutoDetach)
        {
            return;
        }

        var state = currentState;
        if (decision.ShouldDetach)
        {
            state = _floatingWindowStateService.Detach(entry.ScopeId);
            _interfaceProfileAutoRedockService.MarkAutoDetached(entry.ScopeId, state, entry.Timestamp);
        }

        var card = GetRuntimeMonitoringCard(row);
        var windowWasShown = TryShowOrUpdateFloatingMonitoringWindow(card, state);
        if (!windowWasShown)
        {
            RefreshInterfaceMonitoringCards();
            return;
        }

        if (decision.ShouldBringToFront)
        {
            BringFloatingMonitoringWindowToFront(entry.ScopeId, state);
        }

        if (decision.ShouldDetach)
        {
            RefreshInterfaceMonitoringCards();
            AppendMessage($"{card.InterfaceProfileName}: Fenster automatisch geöffnet.");
        }
    }

    private void ShowNidekRtSerialWorkflowWindow(string interfaceProfileId)
    {
        var row = _activeInterfaceProfileStatusRows.FirstOrDefault(item =>
            string.Equals(item.MonitoringCard.InterfaceProfileId, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (row is null)
        {
            return;
        }

        var state = _floatingWindowStateService.Detach(interfaceProfileId);
        var card = GetRuntimeMonitoringCard(row);
        if (TryShowOrUpdateFloatingMonitoringWindow(card, state))
        {
            BringFloatingMonitoringWindowToFront(interfaceProfileId, state);
        }
    }

    private bool IsManualDocumentSelectionProfile(string interfaceProfileId)
    {
        return _profileCatalog?.InterfaceProfiles.Any(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase)
            && profile.FolderOptions.IsAttachmentOnlyMode
            && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection) == true;
    }

    private bool IsNidekRtSerialWorkflowProfile(string interfaceProfileId)
    {
        if (_profileCatalog is null)
        {
            return false;
        }

        var interfaceProfile = _profileCatalog.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (interfaceProfile is null)
        {
            return false;
        }

        var deviceProfile = _profileCatalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        return InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile);
    }

    private static bool IsManualDocumentSelectionProfile(InterfaceProfileDefinition interfaceProfile)
    {
        return interfaceProfile.FolderOptions.IsAttachmentOnlyMode
            && interfaceProfile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
    }

    private void TryUpdateAutoRedockForActivity(InterfaceMonitoringEventEntry entry)
    {
        var row = _activeInterfaceProfileStatusRows.FirstOrDefault(item =>
            string.Equals(item.MonitoringCard.InterfaceProfileId, entry.ScopeId, StringComparison.OrdinalIgnoreCase));
        if (row is null)
        {
            return;
        }

        var floatingState = _floatingWindowStateService.GetOrCreate(entry.ScopeId);
        _ = _interfaceProfileAutoRedockService.RecordMonitoringEvent(entry, floatingState);
        EnsureAutoRedockTimerState();
    }

    private void TryPlayNotificationSoundForDeviceFiles(AutoImportScanResult result)
    {
        foreach (var deviceFile in result.Queue.GetAll().Where(IsStableDeviceImportFile))
        {
            var soundResult = _interfaceProfileNotificationSoundService.TryPlayForDeviceFileDetected(
                result.InterfaceProfileId,
                deviceFile.FilePath,
                deviceFile.DetectedAtUtc,
                DateTime.Now,
                GetMonitoringNotificationSoundPath(),
                _interfaceProfileNotificationSoundPlayer);

            if (!_notificationSoundFailureReported
                && !soundResult.WasPlayed
                && !soundResult.IsSuppressedByCooldown
                && !string.IsNullOrWhiteSpace(soundResult.Message))
            {
                _notificationSoundFailureReported = true;
                AppendMessage(soundResult.Message);
            }

            if (soundResult.ShouldPlay)
            {
                break;
            }
        }
    }

    private static bool IsStableDeviceImportFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind.IsMeasurementDeviceFile() || file.Kind.IsAttachmentImportFile());
    }

    private AutoImportScanResult ApplyMonitoringResetState(AutoImportScanResult result, InterfaceProfileDefinition? profile)
    {
        var filtered = _interfaceProfileMonitoringResetService.Apply(
            result,
            profile?.FolderOptions.IsAttachmentOnlyMode == true);
        _lastMonitoringScanQueuesByProfileId[filtered.InterfaceProfileId] = filtered.Queue;
        return filtered;
    }

    private void ResetMonitoringProfile(string interfaceProfileId)
    {
        var row = _activeInterfaceProfileStatusRows.FirstOrDefault(item =>
            string.Equals(item.MonitoringCard.InterfaceProfileId, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (row is null)
        {
            AppendMessage("Vorgang konnte nicht zurückgesetzt werden: Schnittstellenprofil nicht gefunden.");
            return;
        }

        var profile = _profileCatalog?.InterfaceProfiles.FirstOrDefault(item =>
            string.Equals(item.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (profile is null)
        {
            AppendMessage("Vorgang konnte nicht zurückgesetzt werden: Schnittstellenprofil nicht gefunden.");
            return;
        }

        Window confirmationOwner = _floatingMonitoringWindows.TryGetValue(interfaceProfileId, out var floatingWindow)
            && floatingWindow.IsVisible
                ? floatingWindow
                : this;
        var confirmation = System.Windows.MessageBox.Show(
            confirmationOwner,
            "Der aktuelle Vorgang für dieses Schnittstellenprofil wird verworfen. Die überwachten Eingangsordner dieses Profils werden geleert, damit falsche AIS-/Gerätedateien nicht erneut verarbeitet werden.\n\nBetroffen sind nur die Importordner dieses Schnittstellenprofils. Export-, Archiv- und Fehlerordner werden nicht geleert.\n\nJetzt zurücksetzen und leeren?",
            "Vorgang zurücksetzen und Eingangsordner leeren?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        _lastMonitoringScanQueuesByProfileId.TryGetValue(interfaceProfileId, out var currentQueue);
        var folderResetResult = _interfaceProfileInputFolderResetService.ClearInputFolders(profile);
        var result = _interfaceProfileMonitoringResetService.Reset(interfaceProfileId, currentQueue, folderResetResult);
        _autoImportPackageStateService.ResetProfile(interfaceProfileId);
        _autoImportPairProcessingCoordinator.ResetProfile(interfaceProfileId);
        _interfaceMonitoringCardStatusService.ResetProfile(interfaceProfileId);
        _monitoringEventDeduplicationService.ResetProfile(interfaceProfileId);
        _cv5000DeviceOutputHandledAisKeys.RemoveWhere(key => key.StartsWith($"{interfaceProfileId}|", StringComparison.OrdinalIgnoreCase));
        _interfaceProfileAutoRedockService.NotifyDocked(interfaceProfileId);
        _interfaceProfileAutoDetachService.ResetProfile(interfaceProfileId);
        _interfaceProfileNotificationSoundService.ResetProfile(interfaceProfileId);
        ResetDocumentAttachmentConfirmations(interfaceProfileId);
        EnsureAutoRedockTimerState();

        ResetMonitoringRuntimeCard(row, result);
        RefreshInterfaceMonitoringCards();

        var profileName = row.MonitoringCard.InterfaceProfileName;
        AppendMonitoringEvent(
            interfaceProfileId,
            $"monitoring-reset:{DateTime.UtcNow.Ticks}",
            $"{profileName}: {result.Messages.FirstOrDefault() ?? "Vorgang zurückgesetzt."}");
        foreach (var message in result.Messages.Skip(1))
        {
            AppendMessage($"{profileName}: {message}");
        }
    }

    private void ResetMonitoringRuntimeCard(
        ActiveInterfaceProfileStatusRow row,
        InterfaceProfileMonitoringResetResult resetResult)
    {
        var interfaceProfileId = row.MonitoringCard.InterfaceProfileId;
        _lastMonitoringScanQueuesByProfileId[interfaceProfileId] = new PendingImportQueue();
        var lastScanText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        var resetCard = row.MonitoringCard with
        {
            CurrentStatus = "Wartet auf AIS",
            StatusClass = "Waiting",
            LastScanText = lastScanText,
            IsScanAnimationActive = _periodicScanCancellationTokenSource is not null,
            AutomaticProcessingText = IsAutomaticPairProcessingEnabled() ? "Ja" : "Nein",
            PatientDisplayText = "",
            AisFileName = "",
            DeviceFileName = "",
            AttachmentFileName = "",
            ExportFileName = "",
            LastMessage = resetResult.Messages.FirstOrDefault() ?? "Vorgang zurückgesetzt.",
            ExpectedInputs = row.MonitoringCard.ExpectedInputs,
            IsDetailsExpanded = GetMonitoringDetailsExpanded(interfaceProfileId)
        };

        _interfaceMonitoringRuntimeCards[interfaceProfileId] = resetCard;
        _interfaceMonitoringRuntimeStates[interfaceProfileId] = new InterfaceMonitoringRuntimeState(
            resetCard.CurrentStatus,
            resetCard.StatusClass,
            resetCard.LastScanText);
    }

    private static string GetMonitoringNotificationSoundPath()
    {
        return Path.Combine(AppContext.BaseDirectory, MonitoringNotificationSoundRelativePath);
    }

    private void AutoRedockTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        foreach (var interfaceProfileId in _floatingMonitoringWindows.Keys.ToArray())
        {
            var state = _floatingWindowStateService.GetOrCreate(interfaceProfileId);
            var decision = _interfaceProfileAutoRedockService.EvaluateDue(interfaceProfileId, state, now);
            if (!decision.ShouldRedockNow)
            {
                continue;
            }

            var profileName = _interfaceMonitoringRuntimeCards.TryGetValue(interfaceProfileId, out var card)
                ? card.InterfaceProfileName
                : interfaceProfileId;
            AppendMessage($"{profileName}: Fenster automatisch angedockt.");
            DockFloatingMonitoringCard(interfaceProfileId);
        }

        EnsureAutoRedockTimerState();
    }

    private void EnsureAutoRedockTimerState()
    {
        if (_interfaceProfileAutoRedockService.HasPendingCountdowns)
        {
            if (!_autoRedockTimer.IsEnabled)
            {
                _autoRedockTimer.Start();
            }

            return;
        }

        if (_autoRedockTimer.IsEnabled)
        {
            _autoRedockTimer.Stop();
        }
    }

    private void BringFloatingMonitoringWindowToFront(
        string interfaceProfileId,
        InterfaceProfileFloatingWindowState state)
    {
        if (!_floatingMonitoringWindows.TryGetValue(interfaceProfileId, out var window))
        {
            return;
        }

        if (!window.IsVisible)
        {
            window.Show();
        }

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Topmost = true;
        _ = window.Activate();
        if (!state.IsPinned)
        {
            window.Topmost = false;
        }
    }

    private static void ApplyFloatingWindowPlacement(
        FloatingInterfaceProfileWindow window,
        InterfaceProfileFloatingWindowState state)
    {
        if (!state.IsPositionMemoryEnabled || state.Bounds is null)
        {
            return;
        }

        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = state.Bounds.Left;
        window.Top = state.Bounds.Top;
        window.Width = Math.Max(window.MinWidth, state.Bounds.Width);
        window.Height = Math.Max(window.MinHeight, state.Bounds.Height);
    }

    private void FloatingMonitoringWindow_DockRequested(object? sender, EventArgs e)
    {
        if (sender is FloatingInterfaceProfileWindow window)
        {
            DockFloatingMonitoringCard(window.InterfaceProfileId);
        }
    }

    private void FloatingMonitoringWindow_PinChanged(object? sender, bool isPinned)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        var state = _floatingWindowStateService.SetPinned(window.InterfaceProfileId, isPinned);
        SaveFloatingWindowStates();
        _ = _interfaceProfileAutoRedockService.NotifyPinnedChanged(window.InterfaceProfileId, isPinned, state, DateTime.Now);
        EnsureAutoRedockTimerState();
        window.ApplyState(state);
    }

    private void FloatingMonitoringWindow_PositionMemoryChanged(object? sender, bool isEnabled)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        var state = _floatingWindowStateService.SetPositionMemoryEnabled(window.InterfaceProfileId, isEnabled);
        SaveFloatingWindowStates();
        window.ApplyState(state);
    }

    private void FloatingMonitoringWindow_PositionRememberRequested(object? sender, EventArgs e)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        var bounds = window.CaptureBounds();
        var state = _floatingWindowStateService.RememberPosition(
            window.InterfaceProfileId,
            bounds.Left,
            bounds.Top,
            bounds.Width,
            bounds.Height);
        SaveFloatingWindowStates();
        window.ApplyState(state);
    }

    private void FloatingMonitoringWindow_ScanIntervalChangeRequested(object? sender, int deltaSeconds)
    {
        if (sender is FloatingInterfaceProfileWindow { DataContext: InterfaceMonitoringCardDisplay card })
        {
            ChangeMonitoringScanInterval(card, deltaSeconds);
        }
    }

    private void FloatingMonitoringWindow_ResetRequested(object? sender, EventArgs e)
    {
        if (sender is FloatingInterfaceProfileWindow window)
        {
            ResetMonitoringProfile(window.InterfaceProfileId);
        }
    }

    private async void FloatingMonitoringWindow_SerialListenOnlyRequested(object? sender, EventArgs e)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        await RunNidekRtSerialListenOnlyAsync(window.InterfaceProfileId).ConfigureAwait(true);
    }

    private async void FloatingMonitoringWindow_SerialProcessReturnRequested(object? sender, EventArgs e)
    {
        if (sender is not FloatingInterfaceProfileWindow window)
        {
            return;
        }

        await RunNidekRtSerialProcessReturnAsync(window.InterfaceProfileId).ConfigureAwait(true);
    }

    private void CloseFloatingMonitoringWindow(string interfaceProfileId)
    {
        if (!_floatingMonitoringWindows.Remove(interfaceProfileId, out var window))
        {
            return;
        }

        window.DockRequested -= FloatingMonitoringWindow_DockRequested;
        window.PinChanged -= FloatingMonitoringWindow_PinChanged;
        window.PositionMemoryChanged -= FloatingMonitoringWindow_PositionMemoryChanged;
        window.PositionRememberRequested -= FloatingMonitoringWindow_PositionRememberRequested;
        window.ScanIntervalChangeRequested -= FloatingMonitoringWindow_ScanIntervalChangeRequested;
        window.ResetRequested -= FloatingMonitoringWindow_ResetRequested;
        window.SerialListenOnlyRequested -= FloatingMonitoringWindow_SerialListenOnlyRequested;
        window.SerialProcessReturnRequested -= FloatingMonitoringWindow_SerialProcessReturnRequested;
        window.Closed -= FloatingMonitoringWindow_Closed;
        _interfaceProfileAutoRedockService.NotifyDocked(interfaceProfileId);
        EnsureAutoRedockTimerState();
        window.CloseWithoutDockRequest();
    }

    private void CloseRemovedFloatingMonitoringWindows()
    {
        var activeIds = _activeInterfaceProfileStatusRows
            .Select(row => row.MonitoringCard.InterfaceProfileId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var interfaceProfileId in _floatingMonitoringWindows.Keys.ToArray())
        {
            if (!activeIds.Contains(interfaceProfileId))
            {
                CloseFloatingMonitoringWindow(interfaceProfileId);
            }
        }
    }

    private void CloseAllFloatingMonitoringWindows()
    {
        foreach (var interfaceProfileId in _floatingMonitoringWindows.Keys.ToArray())
        {
            CloseFloatingMonitoringWindow(interfaceProfileId);
        }
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
            ExpectedInputs = runtimeCard.ExpectedInputs,
            SerialDiagnosticsText = runtimeCard.SerialDiagnosticsText,
            HasNidekRtSerialPendingReturn = runtimeCard.HasNidekRtSerialPendingReturn
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
                AutomaticProcessingText: "Ja",
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

    private void AppendNidekRtSerialDiagnostic(
        InterfaceProfileDefinition interfaceProfile,
        string eventKey,
        string message,
        InterfaceMonitoringEventSeverity severity = InterfaceMonitoringEventSeverity.Info)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var profileId = interfaceProfile.Metadata.Id;
        var currentCard = GetRuntimeMonitoringCard(interfaceProfile);
        var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var diagnosticEntry = $"{timestamp} {message.Trim()}";
        var combinedDiagnostics = string.IsNullOrWhiteSpace(currentCard.SerialDiagnosticsText)
            ? diagnosticEntry
            : $"{currentCard.SerialDiagnosticsText.TrimEnd()}{Environment.NewLine}{diagnosticEntry}";
        combinedDiagnostics = TrimSerialDiagnostics(combinedDiagnostics);

        _monitoringDetailsExpandedByProfileId[profileId] = true;
        _interfaceMonitoringRuntimeCards[profileId] = currentCard with
        {
            LastMessage = CreateShortSerialDiagnosticMessage(message),
            IsDetailsExpanded = true,
            UsesSerialDevice = true,
            SerialDiagnosticsText = combinedDiagnostics
        };

        AppendMonitoringEvent(
            profileId,
            eventKey,
            $"{interfaceProfile.Metadata.Name}: {CreateShortSerialDiagnosticMessage(message)}",
            severity);
    }

    private static string TrimSerialDiagnostics(string text)
    {
        const int maxCharacters = 24000;
        const int maxLines = 140;
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        if (lines.Length > maxLines)
        {
            text = string.Join(Environment.NewLine, lines.Skip(lines.Length - maxLines));
        }

        if (text.Length <= maxCharacters)
        {
            return text;
        }

        return text[^maxCharacters..];
    }

    private static string CreateShortSerialDiagnosticMessage(string message)
    {
        var firstLine = message
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim();
        if (string.IsNullOrWhiteSpace(firstLine))
        {
            return "Serielle Diagnose aktualisiert.";
        }

        const int maxLength = 220;
        return firstLine.Length <= maxLength
            ? firstLine
            : firstLine[..maxLength] + "...";
    }

    private void StartPeriodicScan_Click(object sender, RoutedEventArgs e)
    {
        StartPeriodicScan(initiatedByAutoStart: false);
    }

    private void StartPeriodicScan(bool initiatedByAutoStart)
    {
        if (_periodicScanCancellationTokenSource is not null)
        {
            if (!initiatedByAutoStart)
            {
                AppendMessage("Überwachung läuft bereits.");
            }

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
        _userStoppedPeriodicScan = true;
        StopPeriodicScan(updateUi: true);
    }

    private void TryAutoStartPeriodicScanOnce()
    {
        if (_hasAutoStartedPeriodicScan || _userStoppedPeriodicScan)
        {
            return;
        }

        _hasAutoStartedPeriodicScan = true;
        if (!_appSettings.AutoStartMonitoringOnAppStart)
        {
            AppendMonitoringEvent("monitoring", "monitoring-autostart-disabled", "Automatischer Überwachungsstart ist in den App-Einstellungen deaktiviert.", InterfaceMonitoringEventSeverity.Info);
            return;
        }

        StartPeriodicScan(initiatedByAutoStart: true);
    }

    private static bool IsAutomaticPairProcessingEnabled()
    {
        return true;
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

        ChangeMonitoringScanInterval(card, deltaSeconds);
    }

    private void ChangeMonitoringScanInterval(InterfaceMonitoringCardDisplay card, int deltaSeconds)
    {
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
                selectedExportProfileId: result.Profile.ExportProfileId,
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

    private void MonitoringPilotStatusOrb_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdatePilotStatusOrbAnimation(element);
        }
    }

    private void MonitoringPilotStatusOrb_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            StopPilotStatusOrbAnimation(element);
        }
    }

    private void MonitoringPilotStatusOrb_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdatePilotStatusOrbAnimation(element);
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

    private static void UpdatePilotStatusOrbAnimation(FrameworkElement element)
    {
        if (element.DataContext is not InterfaceMonitoringCardDisplay { UsesPilotDeviceVisual: true } card)
        {
            StopPilotStatusOrbAnimation(element);
            return;
        }

        var orb = FindVisualChildByTag<FrameworkElement>(element, "StatusOrb");
        if (orb is null)
        {
            return;
        }

        var flashKey = CreateDeviceInputFlashKey(card);
        if (!string.IsNullOrWhiteSpace(flashKey)
            && !string.Equals(element.GetValue(StatusOrbFlashKeyProperty) as string, flashKey, StringComparison.Ordinal))
        {
            element.SetValue(StatusOrbFlashKeyProperty, flashKey);
            StartStatusOrbFlash(element);
        }

        if (!card.ShouldPulseStatusOrb)
        {
            StopPilotStatusOrbAnimation(element);
            return;
        }

        var pulseSeconds = InterfaceProfileUiPolicy.GetStatusOrbPulseDurationSeconds(card.ScanIntervalSeconds);
        var animationKey = $"{card.InterfaceProfileId}|{card.ScanIntervalSeconds}|{card.IsScanAnimationActive}";
        if (!string.Equals(element.GetValue(StatusOrbAnimationKeyProperty) as string, animationKey, StringComparison.Ordinal))
        {
            element.SetValue(StatusOrbAnimationKeyProperty, animationKey);
            var scaleTransform = EnsureMutableScaleTransform(orb);
            var scaleAnimation = new DoubleAnimation
            {
                From = 0.86,
                To = 1.14,
                Duration = new Duration(TimeSpan.FromSeconds(pulseSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.68,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(pulseSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            orb.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }
    }

    private static void StopPilotStatusOrbAnimation(FrameworkElement element)
    {
        var orb = FindVisualChildByTag<FrameworkElement>(element, "StatusOrb");
        if (orb?.RenderTransform is ScaleTransform scaleTransform && !scaleTransform.IsFrozen)
        {
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
        }

        orb?.BeginAnimation(UIElement.OpacityProperty, null);
        if (orb is not null)
        {
            orb.Opacity = 0.92;
        }
        element.SetValue(StatusOrbAnimationKeyProperty, "");
    }

    private static void StartStatusOrbFlash(DependencyObject element)
    {
        var flash = FindVisualChildByTag<FrameworkElement>(element, "StatusOrbFlash");
        if (flash is null)
        {
            return;
        }

        flash.BeginAnimation(UIElement.OpacityProperty, null);
        flash.Opacity = 1;
        if (flash is System.Windows.Shapes.Shape shape)
        {
            var flashBrush = new SolidColorBrush(Colors.White);
            shape.Fill = flashBrush;
            var colorAnimation = new ColorAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(330))
            };
            colorAnimation.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.White, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            colorAnimation.KeyFrames.Add(new DiscreteColorKeyFrame(System.Windows.Media.Color.FromRgb(255, 216, 64), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(110))));
            colorAnimation.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.White, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
            flashBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        var flashAnimation = new DoubleAnimationUsingKeyFrames
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(430))
        };
        flashAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        flashAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.92, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(110))));
        flashAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
        flashAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(430))));
        flash.BeginAnimation(UIElement.OpacityProperty, flashAnimation);
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

    private static ScaleTransform EnsureMutableScaleTransform(FrameworkElement element)
    {
        if (element.RenderTransform is ScaleTransform transform && !transform.IsFrozen)
        {
            return transform;
        }

        transform = new ScaleTransform(1, 1);
        element.RenderTransform = transform;
        return transform;
    }

    private static string CreateDeviceInputFlashKey(InterfaceMonitoringCardDisplay card)
    {
        return !card.ShouldFlashStatusOrb
            ? string.Empty
            : $"{card.AisFileName}|{card.DeviceFileName}";
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
        SetAllMonitoringRuntimeStates("Gestoppt", "Neutral");
        RefreshInterfaceMonitoringCards();
        AppendMonitoringEvent("monitoring", "monitoring-state", "Überwachung gestoppt.");
    }

    private void ShowPeriodicScanResult(AutoImportScanResult result)
    {
        var profile = _profileCatalog?.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, result.InterfaceProfileId, StringComparison.Ordinal));
        result = ApplyMonitoringResetState(result, profile);
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

        RecordScanMonitoringEvents(profile, profileName, result);

        TryHandleCv5000DeviceOutput(profile, result, timestamp);

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
            IsAutomaticPairProcessingEnabled());
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
            IsAutomaticPairProcessingEnabled());
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

    private bool TryHandleCv5000DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        AutoImportScanResult scanResult,
        DateTime timestamp)
    {
        if (interfaceProfile is null || _profileCatalog is null)
        {
            return false;
        }

        var deviceProfile = _profileCatalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        var isCv5000Output = InterfaceProfileUiPolicy.ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile);
        var isRt6100Output = InterfaceProfileUiPolicy.ShouldTriggerNidekRt6100DeviceOutput(interfaceProfile, deviceProfile);
        var isNidekRtSerialOutput = InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile);
        if (!isCv5000Output && !isRt6100Output && !isNidekRtSerialOutput)
        {
            return false;
        }
        var deviceOutputKey = isNidekRtSerialOutput
            ? "nidek-rt-serial-com-workflow"
            : isRt6100Output ? "rt6100-device-output" : "cv5000-device-output";
        var deviceDisplayName = isNidekRtSerialOutput
            ? CreateNidekRtSerialDisplayName(deviceProfile)
            : isRt6100Output ? "RT-6100" : "CV-5000";
        var writerDisplayName = isNidekRtSerialOutput
            ? $"{deviceDisplayName}-COM-Übergabe"
            : isRt6100Output ? "RT-6100-Importdatei" : "CV-5000-Importdatei";

        var aisFile = scanResult.Queue.GetAll()
            .Where(IsStableAisImportFile)
            .OrderBy(file => file.DetectedAtUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase)
            .LastOrDefault();
        if (aisFile is null)
        {
            return false;
        }

        var aisKey = CreateDeviceOutputAisKey(interfaceProfile.Metadata.Id, deviceOutputKey, aisFile);
        if (_cv5000DeviceOutputHandledAisKeys.Contains(aisKey))
        {
            return false;
        }

        var validationMessage = isNidekRtSerialOutput
            ? ValidateNidekRtSerialWorkflow(interfaceProfile, deviceProfile)
            : isRt6100Output
            ? InterfaceProfileUiPolicy.ValidateNidekRt6100DeviceOutput(interfaceProfile, deviceProfile)
            : InterfaceProfileUiPolicy.ValidateCv5000DeviceOutput(interfaceProfile, deviceProfile);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-missing-config:{validationMessage}",
                $"{interfaceProfile.Metadata.Name}: {validationMessage}",
                InterfaceMonitoringEventSeverity.Warning);
            return false;
        }

        MedistarHistoricalMeasurementParseResult parseResult;
        try
        {
            parseResult = _cv5000HistoryParser.ParseFile(aisFile.FilePath);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-read-error:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: AIS-Historienwerte konnten nicht gelesen werden: {ex.Message}",
                InterfaceMonitoringEventSeverity.Error);
            return true;
        }

        var dialogOptions = isNidekRtSerialOutput
            ? Cv5000PhoropterSelectionDialogOptions.CreateNidekRtSerial(deviceDisplayName)
            : isRt6100Output
                ? Cv5000PhoropterSelectionDialogOptions.CreateNidekRt6100()
                : Cv5000PhoropterSelectionDialogOptions.CreateTopconCv5000();
        var dialog = new Cv5000PhoropterSelectionDialog(parseResult, dialogOptions)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        dialog.Loaded += (_, _) =>
        {
            dialog.Activate();
            dialog.Focus();
        };
        var dialogResult = dialog.ShowDialog();
        var dialogAction = Cv5000DeviceOutputDialogDecision.FromDialogResult(dialogResult, dialog.SelectionOutcome);
        if (isNidekRtSerialOutput)
        {
            return TryStartNidekRtSerialWorkflow(
                interfaceProfile,
                deviceProfile,
                scanResult,
                aisFile,
                aisKey,
                dialogAction,
                dialog.SelectedMeasurements,
                parseResult,
                deviceDisplayName,
                deviceOutputKey,
                timestamp);
        }

        if (dialogAction == Cv5000DeviceOutputDialogAction.WaitForDeviceResultWithoutImport)
        {
            _cv5000DeviceOutputHandledAisKeys.Add(aisKey);
            _autoImportPackageStateService.MarkBidirectionalPhoropterWaitingForDeviceResult(
                interfaceProfile.Metadata.Id,
                aisFile,
                scanResult.Queue,
                timestamp);
            SetMonitoringRuntimeState(
                interfaceProfile.Metadata.Id,
                "Warte auf Phoropter-Rückgabe",
                "Active",
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-send-nothing:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: Keine Werte an den {deviceDisplayName} gesendet. Warte auf {deviceDisplayName}-Rückgabe.");
            return true;
        }

        if (dialogAction == Cv5000DeviceOutputDialogAction.CancelSelection)
        {
            _cv5000DeviceOutputHandledAisKeys.Add(aisKey);
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-canceled:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: Ausgabe an {deviceDisplayName} abgebrochen.");
            return true;
        }

        var selection = new Cv5000ImportSelection(parseResult.Patient, dialog.SelectedMeasurements, null, null);
        var writeResult = isRt6100Output
            ? _nidekRt6100ImportWriter.WriteFile(selection, interfaceProfile, new DateTimeOffset(timestamp))
            : _cv5000ImportWriter.WriteFile(selection, interfaceProfile, new DateTimeOffset(timestamp));
        if (!writeResult.Success)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-failed:{aisKey}:{writeResult.ErrorMessage}",
                $"{interfaceProfile.Metadata.Name}: {writerDisplayName} konnte nicht erzeugt werden: {writeResult.ErrorMessage}",
                InterfaceMonitoringEventSeverity.Error);
            return true;
        }

        _cv5000DeviceOutputHandledAisKeys.Add(aisKey);
        _autoImportPackageStateService.MarkBidirectionalPhoropterWaitingForDeviceResult(
            interfaceProfile.Metadata.Id,
            aisFile,
            scanResult.Queue,
            timestamp);
        SetMonitoringRuntimeState(interfaceProfile.Metadata.Id, $"{writerDisplayName} erzeugt", "Success", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        AppendMonitoringEvent(
            interfaceProfile.Metadata.Id,
            $"{deviceOutputKey}-success:{aisKey}",
            $"{interfaceProfile.Metadata.Name}: {writerDisplayName} wurde für {deviceDisplayName} erzeugt: {writeResult.TargetPath}");
        return true;
    }

    private static bool IsStableAisImportFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && file.Kind.IsAisImportFile();
    }

    private static string CreateDeviceOutputAisKey(string interfaceProfileId, string outputKind, PendingImportFile aisFile)
    {
        return string.Join("|", interfaceProfileId, outputKind, ImportFileFingerprint.Create(aisFile));
    }

    private bool TryStartNidekRtSerialWorkflow(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile,
        AutoImportScanResult scanResult,
        PendingImportFile aisFile,
        string aisKey,
        Cv5000DeviceOutputDialogAction dialogAction,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        MedistarHistoricalMeasurementParseResult parseResult,
        string deviceDisplayName,
        string deviceOutputKey,
        DateTime timestamp)
    {
        _ = scanResult;
        _nidekRtSerialCompletedWorkflowProfiles.Remove(interfaceProfile.Metadata.Id);

        if (dialogAction == Cv5000DeviceOutputDialogAction.CancelSelection)
        {
            _cv5000DeviceOutputHandledAisKeys.Add(aisKey);
            SetMonitoringRuntimeState(
                interfaceProfile.Metadata.Id,
                $"Auswahl für {deviceDisplayName} abgebrochen",
                "Neutral",
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-canceled:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: Ausgabe an {deviceDisplayName} abgebrochen.");
            RefreshInterfaceMonitoringCards();
            return true;
        }

        _cv5000DeviceOutputHandledAisKeys.Add(aisKey);
        var sendSelectedValues = dialogAction == Cv5000DeviceOutputDialogAction.WriteImportFile;
        _nidekRtSerialSendContexts[interfaceProfile.Metadata.Id] = new NidekRtSerialSendContext(
            parseResult.Patient,
            selectedMeasurements.ToArray(),
            aisFile,
            aisKey,
            deviceDisplayName,
            deviceOutputKey,
            timestamp);
        var initialStatus = sendSelectedValues
            ? $"Sende Daten an {deviceDisplayName}"
            : $"Warte auf Rückgabe vom {deviceDisplayName}";
        SetMonitoringRuntimeState(
            interfaceProfile.Metadata.Id,
            initialStatus,
            "Active",
            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        AppendMonitoringEvent(
            interfaceProfile.Metadata.Id,
            $"{deviceOutputKey}-workflow-start:{aisKey}",
            sendSelectedValues
                ? $"{interfaceProfile.Metadata.Name}: Sende ausgewählte LM-/AR-Werte an {deviceDisplayName} und warte danach auf Rückgabe."
                : $"{interfaceProfile.Metadata.Name}: Keine Werte an {deviceDisplayName} gesendet. XDTBox wartet auf die serielle Rückgabe.");
        RefreshInterfaceMonitoringCards();
        ShowNidekRtSerialWorkflowWindow(interfaceProfile.Metadata.Id);

        _ = RunNidekRtSerialWorkflowAsync(
            interfaceProfile,
            deviceProfile,
            aisFile,
            aisKey,
            selectedMeasurements,
            parseResult,
            deviceDisplayName,
            deviceOutputKey,
            timestamp,
            sendSelectedValues);
        return true;
    }

    private async Task RunNidekRtSerialListenOnlyAsync(string interfaceProfileId)
    {
        if (_profileCatalog is null)
        {
            AppendMessage("COM-Port nur abhören nicht möglich: Profilkatalog ist nicht geladen.");
            return;
        }

        var interfaceProfile = _profileCatalog.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (interfaceProfile is null)
        {
            AppendMessage("COM-Port nur abhören nicht möglich: Schnittstellenprofil wurde nicht gefunden.");
            return;
        }

        var deviceProfile = _profileCatalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        if (!InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-listen-only-not-supported",
                "COM-Port nur abhören ist für den produktiven NIDEK-RT-Phoropterworkflow vorgesehen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        if (!_nidekRtSerialListenOnlyProfiles.Add(interfaceProfileId))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-listen-only-already-running",
                "COM-Port nur abhören läuft bereits für dieses Profil.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        var deviceDisplayName = CreateNidekRtSerialDisplayName(deviceProfile);
        try
        {
            var settings = GetSerialSettingsForProfile(interfaceProfile);
            var validationMessage = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    "nidek-rt-serial-listen-only-validation",
                    validationMessage,
                    InterfaceMonitoringEventSeverity.Warning);
                RefreshInterfaceMonitoringCards();
                return;
            }

            SetMonitoringRuntimeState(interfaceProfileId, "COM-Port nur abhören", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-listen-only-start:{DateTime.UtcNow.Ticks}",
                $"Nur-Abhören für {deviceDisplayName} startet. Es wird nichts gesendet. {SerialDiagnosticsFormatter.FormatSettings(settings)}");
            RefreshInterfaceMonitoringCards();

            var result = await _nidekRtSerialCommunicationService
                .ReceiveReturnAsync(settings, _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None)
                .ConfigureAwait(true);

            var index = 0;
            foreach (var message in result.Messages.Where(message => !string.IsNullOrWhiteSpace(message)))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-listen-only-message:{DateTime.UtcNow.Ticks}:{index++}",
                    message,
                    result.Success ? InterfaceMonitoringEventSeverity.Info : InterfaceMonitoringEventSeverity.Warning);
            }

            if (result.Success)
            {
                var hasPendingReturn = _nidekRtSerialSendContexts.TryGetValue(interfaceProfileId, out var context);
                SetMonitoringRuntimeState(
                    interfaceProfileId,
                    hasPendingReturn ? $"Warte auf Rückgabe vom {context!.DeviceDisplayName}" : "COM-Mitschnitt empfangen",
                    hasPendingReturn ? "Active" : "Success",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-listen-only-success:{DateTime.UtcNow.Ticks}",
                    hasPendingReturn
                        ? $"Nur-Abhören abgeschlossen: {result.ReceivedBytes.Length} Bytes vom {deviceDisplayName} empfangen. Das ist Diagnose; der wartende Patientenkontext bleibt erhalten. Für produktive XDT-Erzeugung bitte `Rückgabe abhören und verarbeiten` verwenden."
                        : $"Nur-Abhören abgeschlossen: {result.ReceivedBytes.Length} Bytes vom {deviceDisplayName} empfangen. Es wurde keine XDT-Ausgabe erzeugt.");
            }
            else
            {
                SetMonitoringRuntimeState(interfaceProfileId, "COM-Abhören ohne Rückgabe", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-listen-only-error:{DateTime.UtcNow.Ticks}",
                    result.ErrorMessage ?? $"Keine Daten vom {deviceDisplayName} empfangen.",
                    InterfaceMonitoringEventSeverity.Error);
            }

            RefreshInterfaceMonitoringCards();
        }
        catch (OperationCanceledException)
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-listen-only-canceled:{DateTime.UtcNow.Ticks}",
                $"Nur-Abhören für {deviceDisplayName} wurde abgebrochen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
        }
        finally
        {
            _nidekRtSerialListenOnlyProfiles.Remove(interfaceProfileId);
        }
    }

    private async Task RunNidekRtSerialProcessReturnAsync(string interfaceProfileId)
    {
        if (_profileCatalog is null)
        {
            AppendMessage("Rückgabe abhören und verarbeiten nicht möglich: Profilkatalog ist nicht geladen.");
            return;
        }

        var interfaceProfile = _profileCatalog.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (interfaceProfile is null)
        {
            AppendMessage("Rückgabe abhören und verarbeiten nicht möglich: Schnittstellenprofil wurde nicht gefunden.");
            return;
        }

        if (!_nidekRtSerialSendContexts.TryGetValue(interfaceProfileId, out var context))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-process-return-no-context",
                "Keine wartende NIDEK-RT-Rückgabe mit Patientenkontext vorhanden. Bitte zuerst AIS-Datei empfangen und Werte an den Phoropter senden.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        var deviceProfile = _profileCatalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        if (!InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-process-return-not-supported",
                "Rückgabe abhören und verarbeiten ist für den produktiven NIDEK-RT-Phoropterworkflow vorgesehen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        if (!_nidekRtSerialReturnProcessingProfiles.Add(interfaceProfileId))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-process-return-already-running",
                "Rückgabe abhören und verarbeiten läuft bereits für dieses Profil.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        string? temporaryReturnPath = null;
        try
        {
            var settings = GetSerialSettingsForProfile(interfaceProfile);
            var validationMessage = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    "nidek-rt-serial-process-return-validation",
                    validationMessage,
                    InterfaceMonitoringEventSeverity.Warning);
                RefreshInterfaceMonitoringCards();
                return;
            }

            SetMonitoringRuntimeState(interfaceProfileId, $"Rückgabe vom {context.DeviceDisplayName} abhören", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-process-return-start:{DateTime.UtcNow.Ticks}",
                $"Produktives Rückgabe-Abhören für {context.DeviceDisplayName} startet. Es wird nichts gesendet; der wartende AIS-Patientenkontext bleibt aktiv. {SerialDiagnosticsFormatter.FormatSettings(settings)}");
            RefreshInterfaceMonitoringCards();

            var result = await _nidekRtSerialCommunicationService
                .ReceiveReturnAsync(settings, _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None)
                .ConfigureAwait(true);

            var index = 0;
            foreach (var message in result.Messages.Where(message => !string.IsNullOrWhiteSpace(message)))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-process-return-message:{DateTime.UtcNow.Ticks}:{index++}",
                    message,
                    result.Success ? InterfaceMonitoringEventSeverity.Info : InterfaceMonitoringEventSeverity.Warning);
            }

            if (!result.Success || result.ReceivedBytes.Length == 0)
            {
                SetMonitoringRuntimeState(interfaceProfileId, $"Warte auf Rückgabe vom {context.DeviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-process-return-timeout:{DateTime.UtcNow.Ticks}",
                    "Keine Rückgabe innerhalb der Wartezeit empfangen. Der Patientenkontext bleibt erhalten; Sie können erneut `Rückgabe abhören und verarbeiten` starten oder den Vorgang abbrechen.",
                    InterfaceMonitoringEventSeverity.Warning);
                RefreshInterfaceMonitoringCards();
                return;
            }

            SetMonitoringRuntimeState(interfaceProfileId, "Rückgabe vollständig, Verarbeitung startet", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-process-return-stable:{DateTime.UtcNow.Ticks}",
                $"Rückgabe vom {context.DeviceDisplayName} vollständig empfangen. Produktive MEDISTAR-XDT-Verarbeitung mit wartendem Patientenkontext startet.");

            temporaryReturnPath = WriteNidekRtSerialReturnTempFile(interfaceProfileId, result.ReceivedBytes, DateTime.Now);
            if (ProcessNidekRtSerialReturn(
                    interfaceProfile,
                    context.AisFile,
                    temporaryReturnPath,
                    DateTime.Now,
                    context.DeviceDisplayName,
                    context.DeviceOutputKey,
                    context.AisKey))
            {
                _nidekRtSerialSendContexts.Remove(interfaceProfileId);
            }
            else
            {
                SetMonitoringRuntimeState(interfaceProfileId, $"Warte auf Rückgabe vom {context.DeviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-process-return-processing-failed:{DateTime.UtcNow.Ticks}",
                    "Die empfangene Rückgabe konnte nicht exportiert werden. Der Patientenkontext bleibt erhalten; bitte Diagnose prüfen.",
                    InterfaceMonitoringEventSeverity.Error);
            }
        }
        catch (OperationCanceledException)
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-process-return-canceled:{DateTime.UtcNow.Ticks}",
                $"Produktives Rückgabe-Abhören für {context.DeviceDisplayName} wurde abgebrochen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
        }
        finally
        {
            TryDeleteTemporaryNidekRtSerialReturnFile(temporaryReturnPath);
            _nidekRtSerialReturnProcessingProfiles.Remove(interfaceProfileId);
        }
    }

    private async Task RunNidekRtSerialSendTestAsync(
        string interfaceProfileId,
        NidekRtSerialSendTestMode mode,
        NidekRtSerialOutputFrameVariant frameVariant = NidekRtSerialOutputFrameVariant.ReferenceWithoutIdArAl,
        bool appendCarriageReturnAfterEot = false)
    {
        if (_profileCatalog is null)
        {
            AppendMessage("NIDEK-RT-Sendetest nicht möglich: Profilkatalog ist nicht geladen.");
            return;
        }

        var interfaceProfile = _profileCatalog.InterfaceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (interfaceProfile is null)
        {
            AppendMessage("NIDEK-RT-Sendetest nicht möglich: Schnittstellenprofil wurde nicht gefunden.");
            return;
        }

        var deviceProfile = _profileCatalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        if (!InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-send-test-not-supported",
                "Sendetest ist nur für den produktiven NIDEK-RT-Phoropterworkflow vorgesehen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        if (!_nidekRtSerialSendTestProfiles.Add(interfaceProfileId))
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                "nidek-rt-serial-send-test-already-running",
                "Ein NIDEK-RT-Sendetest läuft bereits für dieses Profil.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
            return;
        }

        var deviceDisplayName = CreateNidekRtSerialDisplayName(deviceProfile);
        try
        {
            var settings = GetSerialSettingsForProfile(interfaceProfile);
            var validationMessage = SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    "nidek-rt-serial-send-test-validation",
                    validationMessage,
                    InterfaceMonitoringEventSeverity.Warning);
                RefreshInterfaceMonitoringCards();
                return;
            }

            var model = DetectNidekRtSerialModel(deviceProfile);
            var options = mode == NidekRtSerialSendTestMode.RequestReadyWithDtrToggle
                ? NidekRtSerialPhoropterSendTestOptions.WithDtrToggle
                : NidekRtSerialPhoropterSendTestOptions.None;
            if (appendCarriageReturnAfterEot)
            {
                options = options with { AppendCarriageReturnToPayload = true };
            }

            var resolvedFrameVariant = NidekRtSerialOutputFrameVariantInfo.Resolve(frameVariant);
            if (mode is NidekRtSerialSendTestMode.DirectWriter or NidekRtSerialSendTestMode.RsWriterWithoutSd
                && !ConfirmNidekRtSerialSendTest(mode, deviceDisplayName))
            {
                return;
            }

            var modeText = CreateNidekRtSerialSendTestModeText(mode);
            SetMonitoringRuntimeState(interfaceProfileId, $"Sendetest: {modeText}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-send-test-start:{mode}:{DateTime.UtcNow.Ticks}",
                $"{modeText} für {deviceDisplayName} startet. Frame-Variante: {NidekRtSerialOutputFrameVariantInfo.ToDisplayName(resolvedFrameVariant)}. CR nach EOT: {(appendCarriageReturnAfterEot ? "Ja" : "Nein")}. {SerialDiagnosticsFormatter.FormatSettings(settings)}");
            RefreshInterfaceMonitoringCards();

            NidekRtSerialPhoropterCommunicationResult result;
            if (mode is NidekRtSerialSendTestMode.RequestReady or NidekRtSerialSendTestMode.RequestReadyWithDtrToggle)
            {
                result = await _nidekRtSerialCommunicationService
                        .RequestReadyToSendAsync(settings, model, options, _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None)
                    .ConfigureAwait(true);
            }
            else
            {
                if (!_nidekRtSerialSendContexts.TryGetValue(interfaceProfileId, out var context)
                    || context.SelectedMeasurements.Count == 0)
                {
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        "nidek-rt-serial-send-test-no-context",
                        "Für diesen Sendetest fehlen ausgewählte LM-/AR-Werte. Bitte zuerst eine AIS-Datei empfangen und im Auswahlfenster Werte auswählen.",
                        InterfaceMonitoringEventSeverity.Warning);
                    RefreshInterfaceMonitoringCards();
                    return;
                }

                result = mode == NidekRtSerialSendTestMode.DirectWriter
                    ? await _nidekRtSerialCommunicationService
                        .SendSelectionDirectAsync(settings, context.Patient, context.SelectedMeasurements, model, options, _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None, resolvedFrameVariant)
                        .ConfigureAwait(true)
                    : await _nidekRtSerialCommunicationService
                        .SendSelectionWithoutWaitingForSdAsync(settings, context.Patient, context.SelectedMeasurements, model, options, _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None, resolvedFrameVariant)
                        .ConfigureAwait(true);
            }

            var index = 0;
            foreach (var message in result.Messages.Where(message => !string.IsNullOrWhiteSpace(message)))
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"nidek-rt-serial-send-test-message:{mode}:{DateTime.UtcNow.Ticks}:{index++}",
                    message,
                    result.Success ? InterfaceMonitoringEventSeverity.Info : InterfaceMonitoringEventSeverity.Warning);
            }

            SetMonitoringRuntimeState(
                interfaceProfileId,
                result.Success ? $"Sendetest abgeschlossen: {modeText}" : $"Sendetest ohne Erfolg: {modeText}",
                result.Success ? "Success" : "Error",
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-send-test-complete:{mode}:{DateTime.UtcNow.Ticks}",
                result.Success
                    ? $"{modeText} abgeschlossen. Es wurde keine produktive XDT-Ausgabe erzeugt."
                    : result.ErrorMessage ?? $"{modeText} ohne erfolgreiche Antwort abgeschlossen.",
                result.Success ? InterfaceMonitoringEventSeverity.Info : InterfaceMonitoringEventSeverity.Error);
            RefreshInterfaceMonitoringCards();
        }
        catch (OperationCanceledException)
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"nidek-rt-serial-send-test-canceled:{mode}:{DateTime.UtcNow.Ticks}",
                $"Sendetest für {deviceDisplayName} wurde abgebrochen.",
                InterfaceMonitoringEventSeverity.Warning);
            RefreshInterfaceMonitoringCards();
        }
        finally
        {
            _nidekRtSerialSendTestProfiles.Remove(interfaceProfileId);
        }
    }

    private async Task RunNidekRtSerialWorkflowAsync(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile,
        PendingImportFile aisFile,
        string aisKey,
        IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements,
        MedistarHistoricalMeasurementParseResult parseResult,
        string deviceDisplayName,
        string deviceOutputKey,
        DateTime timestamp,
        bool sendSelectedValues)
    {
        var profileId = interfaceProfile.Metadata.Id;
        string? temporaryReturnPath = null;
        try
        {
            var cancellationToken = _periodicScanCancellationTokenSource?.Token ?? CancellationToken.None;
            var settings = GetSerialSettingsForProfile(interfaceProfile);
            var model = DetectNidekRtSerialModel(deviceProfile);
            var sendMode = ResolveNidekRtSerialProductiveSendMode(interfaceProfile, deviceProfile);
            var frameVariant = NidekRtSerialOutputFrameVariantInfo.Resolve(interfaceProfile.NidekRtSerialOutputFrameVariant);
            var deviceLogName = CreateNidekRtSerialLogName(model);
            var directWriterSendOnly = sendSelectedValues && sendMode == NidekRtSerialSendMode.DirectWriterFrame;
            NidekRtSerialPhoropterCommunicationResult communicationResult;
            if (sendSelectedValues)
            {
                SetMonitoringRuntimeState(profileId, $"Sende Daten an {deviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-send-start:{aisKey}",
                    "Senden angefordert.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-send-mode:{aisKey}",
                    $"Sende Daten an {deviceDisplayName} über {settings.PortName}. Sendemodus: {NidekRtSerialSendModeInfo.ToDisplayName(sendMode)}. Frame-Variante: {NidekRtSerialOutputFrameVariantInfo.ToDisplayName(frameVariant)}. {SerialDiagnosticsFormatter.FormatSettings(settings)}");
                if (selectedMeasurements.Count == 0)
                {
                    SetMonitoringRuntimeState(profileId, $"Senden an {deviceDisplayName} abgebrochen", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-send-no-selection:{aisKey}",
                        "Senden abgebrochen: Es wurden keine V0/V1-Werte ausgewählt.",
                        InterfaceMonitoringEventSeverity.Error);
                    RefreshInterfaceMonitoringCards();
                    return;
                }

                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-send-selected-values:{aisKey}",
                    $"Ausgewählte Werte: {CreateNidekRtSerialSelectionSummary(selectedMeasurements)}");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-build-start:{aisKey}",
                    "Writer-Frame wird erzeugt.");
                var writerFrame = _nidekRtSerialCommunicationService.BuildSelectionFrame(parseResult.Patient, selectedMeasurements, model, frameVariant);
                if (!writerFrame.Success)
                {
                    SetMonitoringRuntimeState(profileId, $"Senden an {deviceDisplayName} abgebrochen", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-writer-build-failed:{aisKey}",
                        $"Senden abgebrochen: Kein Writer-Frame erzeugt. {writerFrame.ErrorMessage}",
                        InterfaceMonitoringEventSeverity.Error);
                    RefreshInterfaceMonitoringCards();
                    return;
                }

                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-built:{aisKey}",
                    $"Writer-Frame erzeugt: {writerFrame.Bytes.Length} Bytes.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-visible:{aisKey}",
                    $"Writer-Frame sichtbar: {writerFrame.VisibleContent}");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-hex:{aisKey}",
                    $"Writer-Hexdump: {writerFrame.HexDump}");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-port-open-start:{aisKey}",
                    $"COM-Port wird geöffnet: {settings.PortName}.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-send-start:{aisKey}",
                    "Writer-Frame wird gesendet.");
                RefreshInterfaceMonitoringCards();

                communicationResult = directWriterSendOnly
                    ? await _nidekRtSerialCommunicationService.SendSelectionDirectWithoutReturnAsync(
                        settings,
                        parseResult.Patient,
                        selectedMeasurements,
                        model,
                        cancellationToken,
                        frameVariant)
                    : await _nidekRtSerialCommunicationService.SendSelectionAndReceiveAsync(
                        settings,
                        parseResult.Patient,
                        selectedMeasurements,
                        model,
                        sendMode,
                        cancellationToken,
                        frameVariant);
            }
            else
            {
                SetMonitoringRuntimeState(profileId, $"Warte auf Rückgabe vom {deviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-send-nothing:{aisKey}",
                    "Keine Werte an den Phoropter gesendet.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-wait-after-send-nothing:{aisKey}",
                    $"{deviceLogName}: Warte auf Rückgabe vom Phoropter.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-receive-start:{aisKey}",
                    $"Warte auf serielle Rückgabe vom {deviceDisplayName}; es wird nichts gesendet. {SerialDiagnosticsFormatter.FormatSettings(settings)}");
                communicationResult = await _nidekRtSerialCommunicationService.ReceiveReturnAsync(settings, cancellationToken);
            }

            var sendCompletedWithoutImmediateReturn = sendSelectedValues
                && communicationResult.SendCompleted
                && communicationResult.ReceivedBytes.Length == 0;
            var messageIndex = 0;
            foreach (var message in communicationResult.Messages.Where(message => !string.IsNullOrWhiteSpace(message)))
            {
                var diagnosticMessage = sendCompletedWithoutImmediateReturn && IsNidekRtSerialNoImmediateReturnMessage(message)
                    ? "Noch keine Rückgabe empfangen. Der Sendeschritt war erfolgreich; XDTBox wartet auf die spätere Rückgabe."
                    : message;
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-serial-message:{aisKey}:{messageIndex++}",
                    diagnosticMessage,
                communicationResult.Success || sendCompletedWithoutImmediateReturn ? InterfaceMonitoringEventSeverity.Info : InterfaceMonitoringEventSeverity.Warning);
            }

            if (sendSelectedValues
                && sendMode == NidekRtSerialSendMode.RsThenWriterWithoutSd
                && communicationResult.SendCompleted)
            {
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-rs-without-sd-rs-sent:{aisKey}",
                    $"{deviceLogName}: RS gesendet.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-rs-without-sd-writer-sent:{aisKey}",
                    $"{deviceLogName}: Writer-Frame ohne SD-Warten gesendet.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-rs-without-sd-waiting:{aisKey}",
                    $"{deviceLogName}: Warte auf Rückgabe vom Phoropter.");
            }

            if (!communicationResult.Success)
            {
                if (sendCompletedWithoutImmediateReturn)
                {
                    SetMonitoringRuntimeState(profileId, $"Warte auf Rückgabe vom {deviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-send-write-confirmed:{aisKey}",
                        $"Writer-Frame wurde technisch erfolgreich auf {communicationResult.PortName} geschrieben. Ob der Phoropter die Daten fachlich übernommen hat, muss am Gerät geprüft werden.");
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-send-complete-waiting:{aisKey}",
                        $"Daten wurden an {deviceDisplayName} gesendet. XDTBox wartet auf die spätere Rückgabe vom Phoropter. Bitte Untersuchung durchführen und danach PRINT/SEND am Phoropter auslösen.");
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-send-complete-waiting-action:{aisKey}",
                        "Noch keine Rückgabe empfangen. Sie können `Rückgabe abhören und verarbeiten` starten, `COM-Port nur abhören` diagnostisch nutzen oder den Vorgang abbrechen. Es wurde kein leeres XDT erzeugt.");
                    RefreshInterfaceMonitoringCards();
                    return;
                }

                var errorMessage = string.IsNullOrWhiteSpace(communicationResult.ErrorMessage)
                    ? $"Keine Rückgabe vom {deviceDisplayName} empfangen."
                    : communicationResult.ErrorMessage!;
                SetMonitoringRuntimeState(profileId, $"Fehler beim {deviceDisplayName}-Austausch", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-serial-error:{aisKey}:{errorMessage}",
                    errorMessage,
                    InterfaceMonitoringEventSeverity.Error);
                RefreshInterfaceMonitoringCards();
                return;
            }

            if (directWriterSendOnly)
            {
                if (!communicationResult.SendCompleted)
                {
                    SetMonitoringRuntimeState(profileId, $"Fehler beim {deviceDisplayName}-Senden", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    AppendNidekRtSerialDiagnostic(
                        interfaceProfile,
                        $"{deviceOutputKey}-writer-send-not-confirmed:{aisKey}",
                        "Senden abgebrochen: Der Writer-Frame wurde vom seriellen Schreibpfad nicht vollständig bestätigt.",
                        InterfaceMonitoringEventSeverity.Error);
                    RefreshInterfaceMonitoringCards();
                    return;
                }

                SetMonitoringRuntimeState(profileId, $"Warte auf Rückgabe vom {deviceDisplayName}", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-writer-send-complete:{aisKey}",
                    "Writer-Frame gesendet.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-send-step-complete:{aisKey}",
                    $"Sendeschritt abgeschlossen. Daten wurden an {deviceDisplayName} gesendet; XDTBox wartet auf die spätere Rückgabe vom Phoropter.");
                RefreshInterfaceMonitoringCards();
                return;
            }

            SetMonitoringRuntimeState(profileId, "Rückgabe vollständig, Verarbeitung startet", "Active", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"{deviceOutputKey}-return-received:{aisKey}",
                $"{deviceLogName}: Rückgabe empfangen.");
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"{deviceOutputKey}-return-stable:{aisKey}",
                $"Rückgabe vom {deviceDisplayName} vollständig empfangen, Verarbeitung startet.");

            temporaryReturnPath = WriteNidekRtSerialReturnTempFile(profileId, communicationResult.ReceivedBytes, DateTime.Now);
            if (ProcessNidekRtSerialReturn(
                interfaceProfile,
                aisFile,
                temporaryReturnPath,
                timestamp,
                deviceDisplayName,
                deviceOutputKey,
                aisKey))
            {
                _nidekRtSerialSendContexts.Remove(profileId);
            }
        }
        catch (OperationCanceledException)
        {
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"{deviceOutputKey}-canceled-by-stop:{aisKey}",
                $"Serieller {deviceDisplayName}-Workflow wurde abgebrochen.",
                InterfaceMonitoringEventSeverity.Warning);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or InvalidOperationException)
        {
            SetMonitoringRuntimeState(profileId, $"Fehler beim {deviceDisplayName}-Workflow", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"{deviceOutputKey}-workflow-error:{aisKey}:{ex.Message}",
                $"Serieller {deviceDisplayName}-Workflow fehlgeschlagen: {ex.Message}",
                InterfaceMonitoringEventSeverity.Error);
            RefreshInterfaceMonitoringCards();
        }
        catch (Exception ex)
        {
            SetMonitoringRuntimeState(profileId, $"Fehler beim {deviceDisplayName}-Workflow", "Error", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            AppendNidekRtSerialDiagnostic(
                interfaceProfile,
                $"{deviceOutputKey}-workflow-unexpected-error:{aisKey}:{ex.GetType().Name}:{ex.Message}",
                $"Senden abgebrochen: Unerwarteter Fehler im seriellen {deviceDisplayName}-Workflow: {ex.Message}",
                InterfaceMonitoringEventSeverity.Error);
            RefreshInterfaceMonitoringCards();
        }
        finally
        {
            TryDeleteTemporaryNidekRtSerialReturnFile(temporaryReturnPath);
        }
    }

    private static bool IsNidekRtSerialNoImmediateReturnMessage(string message)
    {
        return message.Contains("Keine Rückgabe vom Phoropter empfangen", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Noch keine Rückgabe vom Phoropter empfangen", StringComparison.OrdinalIgnoreCase);
    }

    private bool ProcessNidekRtSerialReturn(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportFile aisFile,
        string temporaryReturnPath,
        DateTime timestamp,
        string deviceDisplayName,
        string deviceOutputKey,
        string aisKey)
    {
        if (_profileCatalog is null)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-export-profile-missing:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: Verarbeitung nicht möglich, Profilkatalog ist nicht geladen.",
                InterfaceMonitoringEventSeverity.Error);
            return false;
        }

        var exportProfile = _profileCatalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, interfaceProfile.ExportProfileId, StringComparison.OrdinalIgnoreCase));
        if (exportProfile is null)
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                $"{deviceOutputKey}-export-profile-missing:{aisKey}",
                $"{interfaceProfile.Metadata.Name}: Verarbeitung nicht möglich, Exportprofil fehlt.",
                InterfaceMonitoringEventSeverity.Error);
            return false;
        }

        var processedSuccessfully = false;
        var detectedAt = timestamp.ToUniversalTime();
        var deviceFile = new PendingImportFile(
            FilePath: temporaryReturnPath,
            FileName: Path.GetFileName(temporaryReturnPath),
            Kind: ImportFileKind.DeviceText,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: detectedAt,
            StableAtUtc: detectedAt,
            Message: $"{deviceDisplayName}-Rückgabe aus COM-Port.");
        var pair = new PendingImportPair(aisFile, deviceFile, IsReady: true);
        var batchResult = _autoImportPairProcessingCoordinator.ProcessReadyPairs(
            interfaceProfile,
            exportProfile,
            new[] { pair },
            automaticProcessingEnabled: true,
            timestamp,
            isMonitoringRunning: _periodicScanCancellationTokenSource is not null,
            attachmentOnlyConfirmationProvider: RequestAttachmentOnlyConfirmation);

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
                processedSuccessfully = true;
                MarkNidekRtSerialWorkflowCompleted(interfaceProfile.Metadata.Id);
                _lastMonitoringScanQueuesByProfileId[interfaceProfile.Metadata.Id] = new PendingImportQueue();
                _autoImportPackageStateService.ResetProfile(interfaceProfile.Metadata.Id);
                _interfaceMonitoringCardStatusService.ResetProfile(interfaceProfile.Metadata.Id);
                _cv5000DeviceOutputHandledAisKeys.RemoveWhere(key => key.StartsWith($"{interfaceProfile.Metadata.Id}|", StringComparison.OrdinalIgnoreCase));
                CompleteManualDocumentTransferState(interfaceProfile);
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                NotifyAutoRedockProcessingCompleted(interfaceProfile.Metadata.Id, timestamp);
                AppendPairMonitoringEvent(
                    interfaceProfile,
                    result,
                    "status",
                    $"{interfaceProfile.Metadata.Name}: Ausgabedatei an AIS erstellt: {result.ExportFilePath}");
                var logName = CreateNidekRtSerialLogName(deviceDisplayName);
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-ais-output-created:{aisKey}",
                    $"{logName}: AIS-Ausgabe erzeugt.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-workflow-completed:{aisKey}",
                    $"{logName}: Workflow abgeschlossen.");
                AppendNidekRtSerialDiagnostic(
                    interfaceProfile,
                    $"{deviceOutputKey}-workflow-window-closed:{aisKey}",
                    $"{logName}: Gerätefenster geschlossen; wartet im Hintergrund auf nächste Patientendatei.");
            }
            else
            {
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                AppendPairMonitoringEvent(
                    interfaceProfile,
                    result,
                    "status",
                    $"{interfaceProfile.Metadata.Name}: {deviceDisplayName}-Rückgabe konnte nicht verarbeitet werden.",
                    InterfaceMonitoringEventSeverity.Error);
            }

            foreach (var message in result.Messages)
            {
                AppendPairMonitoringEvent(interfaceProfile, result, $"message:{message}", message);
            }
        }

        RefreshInterfaceMonitoringCards();
        if (processedSuccessfully)
        {
            CloseFloatingMonitoringWindow(interfaceProfile.Metadata.Id);
        }

        return processedSuccessfully;
    }

    private void MarkNidekRtSerialWorkflowCompleted(string interfaceProfileId)
    {
        _nidekRtSerialCompletedWorkflowProfiles.Add(interfaceProfileId);
        _nidekRtSerialSendContexts.Remove(interfaceProfileId);
        _interfaceProfileAutoRedockService.NotifyDocked(interfaceProfileId);
        EnsureAutoRedockTimerState();
        _floatingWindowStateService.Dock(interfaceProfileId);
        SaveFloatingWindowStates();
        CloseFloatingMonitoringWindow(interfaceProfileId);
    }

    private string? ValidateNidekRtSerialWorkflow(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (!InterfaceProfileUiPolicy.ShouldTriggerNidekRtSerialPhoropterWorkflow(interfaceProfile, deviceProfile))
        {
            return "Serieller NIDEK-RT-Phoropterworkflow ist für dieses Profil nicht aktiv.";
        }

        var settings = GetSerialSettingsForProfile(interfaceProfile);
        return SerialDeviceCommunicationService.ValidateSettings(settings, requirePortName: true);
    }

    private static string CreateNidekRtSerialSelectionSummary(IReadOnlyList<AisHistoricalMeasurementRecord> selectedMeasurements)
    {
        if (selectedMeasurements.Count == 0)
        {
            return "keine Werte";
        }

        return string.Join("; ", selectedMeasurements.Select(record =>
        {
            var source = record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter
                ? "LM"
                : record.SourceKind == AisHistoricalMeasurementSourceKind.Autorefraction
                    ? "AR"
                    : record.SourceKind.ToString();
            var date = record.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var right = record.RightEye?.HasExportableRefraction == true
                ? $"R S={record.RightEye.Sphere} Z={record.RightEye.Cylinder} A={record.RightEye.Axis}"
                : "R -";
            var left = record.LeftEye?.HasExportableRefraction == true
                ? $"L S={record.LeftEye.Sphere} Z={record.LeftEye.Cylinder} A={record.LeftEye.Axis}"
                : "L -";
            return $"{source} {date}: {right}, {left}";
        }));
    }

    private static string CreateNidekRtSerialSendTestModeText(NidekRtSerialSendTestMode mode)
    {
        return mode switch
        {
            NidekRtSerialSendTestMode.RequestReady => "RS anfordern",
            NidekRtSerialSendTestMode.RequestReadyWithDtrToggle => "DTR-Toggle + RS anfordern",
            NidekRtSerialSendTestMode.DirectWriter => "Direkt Writer-Frame senden",
            NidekRtSerialSendTestMode.RsWriterWithoutSd => "RS + Writer ohne SD-Warten",
            _ => "Sendetest"
        };
    }

    private static bool ConfirmNidekRtSerialSendTest(NidekRtSerialSendTestMode mode, string deviceDisplayName)
    {
        var modeText = CreateNidekRtSerialSendTestModeText(mode);
        var message = mode == NidekRtSerialSendTestMode.DirectWriter
            ? $"Testmodus: {modeText} sendet direkt an den {deviceDisplayName}, ohne vorher RS/SD abzuwarten. Nur verwenden, wenn RS/SD keine Antwort liefert. Fortfahren?"
            : $"Testmodus: {modeText} sendet auch ohne SD-Bestätigung an den {deviceDisplayName}. Nur für die Praxisdiagnose verwenden. Fortfahren?";
        return System.Windows.MessageBox.Show(
            message,
            "NIDEK-RT-Sendetest",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private static string CreateNidekRtSerialDisplayName(DeviceProfileDefinition? deviceProfile)
    {
        var model = deviceProfile?.Model;
        if (!string.IsNullOrWhiteSpace(model))
        {
            return model.Trim();
        }

        var product = deviceProfile?.Metadata.Product;
        if (!string.IsNullOrWhiteSpace(product))
        {
            return product.Trim();
        }

        return "NIDEK RT";
    }

    private static string CreateNidekRtSerialLogName(NidekRtSerialPhoropterModel model)
    {
        return model switch
        {
            NidekRtSerialPhoropterModel.Rt2100 => "RT-2100",
            NidekRtSerialPhoropterModel.Rt5100 => "RT-5100",
            _ => "RT-3100"
        };
    }

    private static string CreateNidekRtSerialLogName(string deviceDisplayName)
    {
        if (deviceDisplayName.Contains("2100", StringComparison.OrdinalIgnoreCase))
        {
            return "RT-2100";
        }

        if (deviceDisplayName.Contains("5100", StringComparison.OrdinalIgnoreCase))
        {
            return "RT-5100";
        }

        return "RT-3100";
    }

    private static NidekRtSerialSendMode ResolveNidekRtSerialProductiveSendMode(
        InterfaceProfileDefinition interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        _ = interfaceProfile;
        _ = deviceProfile;
        return NidekRtSerialSendMode.RsThenWriterWithoutSd;
    }

    private static NidekRtSerialPhoropterModel DetectNidekRtSerialModel(DeviceProfileDefinition? deviceProfile)
    {
        var combined = string.Join(
            " ",
            deviceProfile?.Metadata.Id,
            deviceProfile?.Metadata.Name,
            deviceProfile?.Metadata.Product,
            deviceProfile?.Model);
        if (combined.Contains("2100", StringComparison.OrdinalIgnoreCase))
        {
            return NidekRtSerialPhoropterModel.Rt2100;
        }

        if (combined.Contains("5100", StringComparison.OrdinalIgnoreCase))
        {
            return NidekRtSerialPhoropterModel.Rt5100;
        }

        if (combined.Contains("3100", StringComparison.OrdinalIgnoreCase))
        {
            return NidekRtSerialPhoropterModel.Rt3100;
        }

        return NidekRtSerialPhoropterModel.Rt3100;
    }

    private static string WriteNidekRtSerialReturnTempFile(string interfaceProfileId, byte[] bytes, DateTime timestamp)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XDTBox", "SerialReturns");
        Directory.CreateDirectory(folder);
        var fileName = $"{CreateSafeFileName(interfaceProfileId)}_{timestamp:yyyyMMdd_HHmmss_fff}_RT_Return.txt";
        var path = Path.Combine(folder, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static string CreateSafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(invalid.Contains(character) ? '_' : character);
        }

        return builder.Length == 0 ? "interface" : builder.ToString();
    }

    private static void TryDeleteTemporaryNidekRtSerialReturnFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            // Temporäre Rückgabedateien dürfen liegen bleiben, falls ein Virenscanner oder Archivlauf sie kurz hält.
        }
    }

    private AutoImportPairProcessingBatchResult? TryProcessReadyPairsAutomatically(
        InterfaceProfileDefinition? interfaceProfile,
        AutoImportScanResult scanResult,
        AutoImportPackageEvaluationResult? packageEvaluation)
    {
        if (!IsAutomaticPairProcessingEnabled() || _periodicScanCancellationTokenSource is null)
        {
            return null;
        }

        if (interfaceProfile is null)
        {
            AppendMonitoringEvent("monitoring", "automatic-processing-error", "Automatische Verarbeitung nicht möglich: Schnittstellenprofil wurde nicht gefunden.", InterfaceMonitoringEventSeverity.Error);
            return new AutoImportPairProcessingBatchResult(0, 0, 1, Array.Empty<AutoImportPairProcessingResult>());
        }

        if (TryUpdatePendingDocumentAttachmentConfirmationFromScan(interfaceProfile, scanResult))
        {
            return new AutoImportPairProcessingBatchResult(0, 0, 0, Array.Empty<AutoImportPairProcessingResult>());
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
        if (readyPairs.Count > 0 && InterfaceProfileUiPolicy.IsCv5000(interfaceProfile, deviceProfile: null))
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                "cv5000-ready-pair-processing-start",
                $"{interfaceProfile.Metadata.Name}: CV-5000-Paar vollständig, Export wird gestartet.");
        }
        else if (readyPairs.Count > 0 && InterfaceProfileUiPolicy.IsNidekRt6100(interfaceProfile, deviceProfile: null))
        {
            AppendMonitoringEvent(
                interfaceProfile.Metadata.Id,
                "rt6100-ready-pair-processing-start",
                $"{interfaceProfile.Metadata.Name}: RT-6100-Paar vollständig, Export wird gestartet.");
        }

        var batchResult = _autoImportPairProcessingCoordinator.ProcessReadyPairs(
            interfaceProfile,
            exportProfile,
            readyPairs,
            automaticProcessingEnabled: true,
            timestamp,
            isMonitoringRunning: _periodicScanCancellationTokenSource is not null,
            attachmentOnlyConfirmationProvider: RequestAttachmentOnlyConfirmation);

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
                _lastMonitoringScanQueuesByProfileId[interfaceProfile.Metadata.Id] = new PendingImportQueue();
                _autoImportPackageStateService.ResetProfile(interfaceProfile.Metadata.Id);
                _interfaceMonitoringCardStatusService.ResetProfile(interfaceProfile.Metadata.Id);
                _cv5000DeviceOutputHandledAisKeys.RemoveWhere(key => key.StartsWith($"{interfaceProfile.Metadata.Id}|", StringComparison.OrdinalIgnoreCase));
                CompleteManualDocumentTransferState(interfaceProfile);
                UpdateMonitoringCardFromProcessingResult(interfaceProfile, result, timestamp);
                NotifyAutoRedockProcessingCompleted(interfaceProfile.Metadata.Id, timestamp);
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

    private void NotifyAutoRedockProcessingCompleted(string interfaceProfileId, DateTime timestamp)
    {
        var floatingState = _floatingWindowStateService.GetOrCreate(interfaceProfileId);
        _ = _interfaceProfileAutoRedockService.NotifyProcessingCompleted(interfaceProfileId, floatingState, timestamp);
        EnsureAutoRedockTimerState();
    }

    private void CompleteManualDocumentTransferState(InterfaceProfileDefinition interfaceProfile)
    {
        if (!IsManualDocumentSelectionProfile(interfaceProfile))
        {
            return;
        }

        ResetDocumentAttachmentConfirmations(interfaceProfile.Metadata.Id);
        _autoImportPairProcessingCoordinator.ResetProfile(interfaceProfile.Metadata.Id);
    }

    private bool TryUpdatePendingDocumentAttachmentConfirmationFromScan(
        InterfaceProfileDefinition interfaceProfile,
        AutoImportScanResult scanResult)
    {
        if (!interfaceProfile.FolderOptions.IsAttachmentOnlyMode
            || interfaceProfile.FolderOptions.AttachmentCompletionMode != AttachmentCompletionMode.ManualConfirmation)
        {
            return false;
        }

        var prefix = $"{interfaceProfile.Metadata.Id}|";
        var activeStates = _pendingDocumentAttachmentConfirmations
            .Where(entry => entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && !entry.Value.IsTransferConfirmed
                && !entry.Value.IsCanceled
                && !entry.Value.IsCompleting)
            .Select(entry => entry.Value)
            .ToList();
        if (activeStates.Count == 0)
        {
            return false;
        }

        var documentFiles = scanResult.Queue
            .GetAll()
            .Where(file => file.Status == PendingImportFileStatus.Stable
                && file.Kind.IsDeviceImportFile(true))
            .Select(DocumentAttachmentDialogFile.FromPendingImportFile)
            .ToList();
        if (documentFiles.Count == 0)
        {
            return true;
        }

        foreach (var state in activeStates)
        {
            state.Window.UpdateFiles(documentFiles);
        }

        return true;
    }

    private AttachmentOnlyConfirmationResult RequestAttachmentOnlyConfirmation(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        IReadOnlyList<AttachmentImportFileCandidate> selectedCandidates)
    {
        if (!interfaceProfile.FolderOptions.IsAttachmentOnlyMode)
        {
            return AttachmentOnlyConfirmationResult.Proceed(null, null);
        }

        var isManualDocumentSelection = interfaceProfile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var requiresTransferConfirmation = interfaceProfile.FolderOptions.AttachmentCompletionMode == AttachmentCompletionMode.ManualConfirmation
            || isManualDocumentSelection;
        if (!requiresTransferConfirmation && !interfaceProfile.FolderOptions.ShowAttachmentDocumentationDialog)
        {
            return AttachmentOnlyConfirmationResult.Proceed(null, null);
        }

        if (requiresTransferConfirmation)
        {
            return RequestAttachmentOnlyManualConfirmation(interfaceProfile, pair, selectedCandidates);
        }

        Window? owner = _floatingMonitoringWindows.TryGetValue(interfaceProfile.Metadata.Id, out var floatingWindow)
            && floatingWindow.IsVisible
                ? floatingWindow
                : IsVisible ? this : null;
        var dialog = new DocumentAttachmentDocumentationWindow(
            interfaceProfile.Metadata.Name,
            CreateDocumentAttachmentDialogFiles(selectedCandidates),
            requiresTransferConfirmation,
            capturesDocumentationText: interfaceProfile.FolderOptions.ShowAttachmentDocumentationDialog,
            allowsManualFileSelection: isManualDocumentSelection);
        if (owner is not null)
        {
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return dialog.ShowDialog() == true
            ? AttachmentOnlyConfirmationResult.Proceed(null, dialog.FileDescriptions, dialog.SelectedCandidates)
            : AttachmentOnlyConfirmationResult.Cancel();
    }

    private AttachmentOnlyConfirmationResult RequestAttachmentOnlyManualConfirmation(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        IReadOnlyList<AttachmentImportFileCandidate> selectedCandidates)
    {
        var confirmationKey = CreateDocumentAttachmentConfirmationKey(interfaceProfile, pair);
        var isManualDocumentSelection = interfaceProfile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        if (_pendingDocumentAttachmentConfirmations.TryGetValue(confirmationKey, out var existingState))
        {
            if (existingState.IsTransferConfirmed)
            {
                _pendingDocumentAttachmentConfirmations.Remove(confirmationKey);
                return AttachmentOnlyConfirmationResult.Proceed(
                    null,
                    existingState.FileDescriptions,
                    existingState.SelectedCandidates);
            }

            if (existingState.IsCanceled)
            {
                return AttachmentOnlyConfirmationResult.Cancel();
            }

            existingState.Window.UpdateFiles(CreateDocumentAttachmentDialogFiles(selectedCandidates));

            return AttachmentOnlyConfirmationResult.Cancel();
        }

        Window? owner = _floatingMonitoringWindows.TryGetValue(interfaceProfile.Metadata.Id, out var floatingWindow)
            && floatingWindow.IsVisible
                ? floatingWindow
                : IsVisible ? this : null;
        var dialog = new DocumentAttachmentDocumentationWindow(
            interfaceProfile.Metadata.Name,
            CreateDocumentAttachmentDialogFiles(selectedCandidates),
            requiresTransferConfirmation: true,
            capturesDocumentationText: interfaceProfile.FolderOptions.ShowAttachmentDocumentationDialog,
            allowsManualFileSelection: isManualDocumentSelection);
        if (owner is not null)
        {
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        var state = new PendingDocumentAttachmentConfirmation(dialog);
        dialog.TransferRequested += (_, _) =>
        {
            state.FileDescriptions = dialog.FileDescriptions;
            state.SelectedCandidates = dialog.SelectedCandidates;
            state.IsTransferConfirmed = true;
            state.IsCompleting = true;
            dialog.Close();
        };
        dialog.CancelRequested += (_, _) =>
        {
            state.IsCanceled = true;
            state.IsCompleting = true;
            dialog.Close();
        };
        dialog.Closed += (_, _) =>
        {
            if (!state.IsCompleting && !state.IsTransferConfirmed)
            {
                state.IsCanceled = true;
            }
        };

        _pendingDocumentAttachmentConfirmations[confirmationKey] = state;
        dialog.Show();
        return AttachmentOnlyConfirmationResult.Cancel();
    }

    private static string CreateDocumentAttachmentConfirmationKey(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair)
    {
        return string.Join(
            "|",
            interfaceProfile.Metadata.Id,
            "ais",
            ImportFileFingerprint.Create(pair.AisFile));
    }

    private static IReadOnlyList<DocumentAttachmentDialogFile> CreateDocumentAttachmentDialogFiles(
        IReadOnlyList<AttachmentImportFileCandidate> candidates)
    {
        return candidates
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
            .Select(DocumentAttachmentDialogFile.FromCandidate)
            .ToList();
    }

    private void ResetDocumentAttachmentConfirmations(string interfaceProfileId)
    {
        var prefix = $"{interfaceProfileId}|";
        foreach (var key in _pendingDocumentAttachmentConfirmations.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList())
        {
            var state = _pendingDocumentAttachmentConfirmations[key];
            state.IsCompleting = true;
            if (state.Window.IsVisible)
            {
                state.Window.Close();
            }

            _pendingDocumentAttachmentConfirmations.Remove(key);
        }
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

        AppendMonitoringEvent("monitoring", "manual-scan-started", "Einmaliger Scan gestartet.");

        var scanTimestampText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        foreach (var profile in activeProfiles)
        {
            try
            {
                var result = await _autoImportScannerService
                    .ScanOnceAsync(profile, TimeSpan.FromMilliseconds(200))
                    .ConfigureAwait(true);
                result = ApplyMonitoringResetState(result, profile);

                var packageEvaluation = _autoImportPackageStateService.Evaluate(profile, result.Queue, DateTime.Now);
                UpdateMonitoringCardFromScan(profile, result, packageEvaluation, DateTime.Now);
                RecordScanMonitoringEvents(profile, profile.Metadata.Name, result);
                TryHandleCv5000DeviceOutput(profile, result, DateTime.Now);
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

    private LicenseDeviceLocationStore LoadLicenseDeviceLocationStoreOrEmpty()
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            return _licenseDeviceLocationRepository.LoadOrEmpty(GetLicenseDeviceLocationsFilePath(paths));
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Gerätestandorte konnten nicht geladen werden: {ex.Message}");
            return LicenseDeviceLocationStore.Empty;
        }
    }

    private LicenseDeviceLocationStore CreateLicenseDeviceLocationStoreFromRows()
    {
        CommitLicenseDeviceLocationEdits();

        var locations = _licensedDeviceStateRows
            .Where(row => !string.IsNullOrWhiteSpace(row.InterfaceProfileId))
            .Select(row => new LicenseDeviceLocation(
                row.InterfaceProfileId,
                NormalizeOptionalText(row.Standort)))
            .ToArray();

        return new LicenseDeviceLocationStore(locations);
    }

    private IReadOnlyDictionary<string, string?> CreateLicenseDeviceLocationDictionaryFromRows()
    {
        return CreateLicenseDeviceLocationStoreFromRows().ToDictionary();
    }

    private void SaveLicenseDeviceLocations_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _licenseDeviceLocationRepository.Save(
                GetLicenseDeviceLocationsFilePath(paths),
                CreateLicenseDeviceLocationStoreFromRows());
            AppendLicenseMessage("Gerätestandorte für Lizenzanfragen gespeichert.");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Gerätestandorte konnten nicht gespeichert werden: {ex.Message}");
        }
    }

    private void CommitLicenseDeviceLocationEdits()
    {
        LicensedDeviceStatesGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        LicensedDeviceStatesGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
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

        var customer = ReadLicenseCustomerDataFromEditor();
        var customerIssues = ValidateLicenseCustomerDataForExport(customer);
        if (customerIssues.Count > 0)
        {
            AppendLicenseMessage("Lizenzanfrage wurde nicht exportiert, weil Kundendaten fehlen:");
            foreach (var issue in customerIssues)
            {
                AppendLicenseMessage($"[Kundendaten] {issue}");
            }

            LicenseCustomerDataStatusText.Text = string.Join(" ", customerIssues);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Lizenzanfrage (*.json)|*.json|Alle Dateien (*.*)|*.*",
            FileName = "XDTBox_Lizenzanfrage.json",
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
            var deviceLocationsByInterfaceProfileId = CreateLicenseDeviceLocationDictionaryFromRows();
            var request = _licenseRequestBuilder.Build(
                _installationInfo,
                _profileCatalog.InterfaceProfiles,
                _profileCatalog.DeviceProfiles,
                customer,
                XdtBoxLicenseConstants.ProductCode,
                GetApplicationVersionText(),
                DateTime.UtcNow,
                deviceLocationsByInterfaceProfileId);

            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _licenseCustomerDataRepository.Save(GetLicenseCustomerDataFilePath(paths), customer);
            _licenseDeviceLocationRepository.Save(GetLicenseDeviceLocationsFilePath(paths), CreateLicenseDeviceLocationStoreFromRows());
            _licenseRequestFileRepository.Save(dialog.FileName, request);
            AppendLicenseMessage($"Lizenzanfrage erfolgreich exportiert: {dialog.FileName}");
            AppendLicenseMessage($"Aktive Geräteanbindungen in der Anfrage: {request.ActiveLicensedDeviceCount}. Gerätenamen dienen nur der Dokumentation.");
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
            Filter = "XDTBox-Lizenz (*.xdtboxlic)|*.xdtboxlic|Legacy-Lizenz (*.json)|*.json|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            if (string.Equals(Path.GetExtension(dialog.FileName), ".xdtboxlic", StringComparison.OrdinalIgnoreCase))
            {
                ImportSignedLicenseFile(dialog.FileName);
                return;
            }

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

            AppendLicenseMessage($"Legacy-Lizenzdatei importiert (Signatur nicht kryptografisch geprüft): {dialog.FileName}");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Lizenzdatei konnte nicht importiert werden: {ex.Message}");
        }
    }

    private void ImportSignedLicenseFile(string filePath)
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
        _installationInfo = installation;

        var result = _licenseImportService.ImportFromFile(
            filePath,
            installation,
            CountActiveDeviceConnectionsForLicenseV1(),
            DateTime.UtcNow);

        var persistedSignedLicense = false;
        if (result.CanPersistLicenseFile)
        {
            Directory.CreateDirectory(paths.LicensesFolder);
            File.Copy(filePath, GetSignedLicenseFilePath(paths), overwrite: true);
            persistedSignedLicense = true;
        }

        var displayLicense = result.Payload is not null && result.SignatureStatus == LicenseSignatureVerificationStatus.Valid
            ? CreateDisplayLicenseInfo(result.Payload, "RSA-PSS-SHA256 geprüft")
            : null;

        ShowLicensedDeviceStates(displayLicense);
        ShowLicenseStatus(
            installation,
            result.UserMessage,
            CountActiveDeviceConnectionsForLicenseV1(),
            result.Payload?.MaxActiveDeviceConnections ?? 0);
        AppendLicenseMessage(result.UserMessage);
        if (persistedSignedLicense
            && result.Payload is not null
            && result.SignatureStatus == LicenseSignatureVerificationStatus.Valid
            && result.PolicyEvaluation?.Status == LicenseV1PolicyStatus.Valid)
        {
            AppendLicenseMessage(XdtBoxLicenseConstants.CreateSuccessfulLicenseImportMessage(result.Payload.MaxActiveDeviceConnections));
        }
    }

    private void RemoveLicenseFile_Click(object sender, RoutedEventArgs e)
    {
        var confirmation = System.Windows.MessageBox.Show(
            this,
            "Möchten Sie die lokal importierte XDTBox-Lizenz wirklich entfernen? Die Geräteanbindungen gelten danach als nicht lizenziert, bis eine gültige Lizenz erneut importiert wird.",
            "XDTBox-Lizenz entfernen",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (confirmation != System.Windows.MessageBoxResult.Yes)
        {
            AppendLicenseMessage("Lizenz entfernen abgebrochen. Die lokale Lizenz bleibt erhalten.");
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;

            var result = _localLicenseRemovalService.RemoveLocalLicense(paths);
            var activeLicensedDeviceCount = CountActiveLicensedDevices();

            ShowLicensedDeviceStates(license: null);
            ShowLicenseStatus(
                installation,
                "Nicht lizenziert / keine lokale Lizenz vorhanden",
                activeLicensedDeviceCount,
                licensedDeviceCount: 0);

            var removalText = result.RemovedAnyLicense
                ? "Lokale Lizenz entfernt. Die aktiven Geräteanbindungen gelten bis zum erneuten Lizenzimport als nicht gedeckt."
                : "Keine lokale Lizenzdatei vorhanden. Die aktiven Geräteanbindungen gelten weiterhin als nicht gedeckt.";
            AppendLicenseMessage(removalText);
            AppendLicenseMessage("Lizenzanfragen können weiterhin mit dem aktuellen Gesamtzustand der Einrichtung exportiert werden.");
        }
        catch (Exception ex)
        {
            AppendLicenseMessage($"Lizenz konnte nicht entfernt werden: {ex.Message}");
        }
    }

    private void UpdateGracePeriods_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var nowUtc = DateTime.UtcNow;
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var installation = _installationInfo ?? _installationInfoProvider.GetOrCreate(paths.BaseFolder);
            _installationInfo = installation;
            var license = LoadCurrentDisplayLicenseFromLocalSource(paths, installation);
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
            RefreshLicensedDeviceStatesFromLocalLicense();
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

    private void RefreshProfileUiAfterCatalogChange(ProfileCatalog catalog)
    {
        InitializeProfileDependentTabs(catalog);
        RefreshLicensedDeviceStatesFromLocalLicense();
    }

    private static ProfileCatalog CreateEmptyProfileCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>());
    }

    private static string DisplayOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private void InitializeXdtBaukasten(
        ProfileCatalog catalog,
        string? selectedAisProfileId = null,
        string? selectedDeviceProfileId = null,
        string? selectedExportProfileId = null)
    {
        _updatingXdtBaukastenSelection = true;
        try
        {
            var aisProfiles = catalog.AisProfiles
                .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var deviceProfiles = catalog.DeviceProfiles
                .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var exportProfiles = catalog.ExportProfiles
                .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            XdtBaukastenAisProfileComboBox.ItemsSource = aisProfiles;
            XdtBaukastenDeviceProfileComboBox.ItemsSource = deviceProfiles;
            XdtBaukastenExportProfileComboBox.ItemsSource = exportProfiles;

            XdtBaukastenAisProfileComboBox.SelectedItem = SelectProfileById(
                aisProfiles,
                selectedAisProfileId ?? _xdtBaukastenState.AisProfile?.Metadata.Id);
            XdtBaukastenDeviceProfileComboBox.SelectedItem = SelectProfileById(
                deviceProfiles,
                selectedDeviceProfileId ?? _xdtBaukastenState.DeviceProfile?.Metadata.Id);
            XdtBaukastenExportProfileComboBox.SelectedItem = SelectProfileById(
                exportProfiles,
                selectedExportProfileId ?? _xdtBaukastenState.SourceExportProfile?.Metadata.Id);
        }
        finally
        {
            _updatingXdtBaukastenSelection = false;
        }

        UpdateXdtBaukastenAisProfileFromSelection();
        UpdateXdtBaukastenDeviceProfileFromSelection();
        UpdateXdtBaukastenExportProfileFromSelection();
        UpdateXdtBaukastenPlaceholders();
        UpdateXdtBaukastenUndoButtonState();
    }

    private static TProfile? SelectProfileById<TProfile>(IEnumerable<TProfile> profiles, string? profileId)
        where TProfile : class
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return null;
        }

        return profiles.FirstOrDefault(profile =>
        {
            var metadata = profile switch
            {
                AisProfile aisProfile => aisProfile.Metadata,
                DeviceProfileDefinition deviceProfile => deviceProfile.Metadata,
                ExportProfileDefinition exportProfile => exportProfile.Metadata,
                InterfaceProfileDefinition interfaceProfile => interfaceProfile.Metadata,
                _ => null
            };

            return metadata is not null
                && string.Equals(metadata.Id, profileId, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void XdtBaukastenAisProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingXdtBaukastenSelection)
        {
            return;
        }

        PushXdtBaukastenUndoState();
        UpdateXdtBaukastenAisProfileFromSelection();
    }

    private void XdtBaukastenDeviceProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingXdtBaukastenSelection)
        {
            return;
        }

        PushXdtBaukastenUndoState();
        UpdateXdtBaukastenDeviceProfileFromSelection();
    }

    private void XdtBaukastenExportProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingXdtBaukastenSelection)
        {
            return;
        }

        PushXdtBaukastenUndoState();
        UpdateXdtBaukastenExportProfileFromSelection();
    }

    private void UpdateXdtBaukastenAisProfileFromSelection()
    {
        _xdtBaukastenState.SetAisProfile(XdtBaukastenAisProfileComboBox.SelectedItem as AisProfile);
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private void UpdateXdtBaukastenDeviceProfileFromSelection()
    {
        var profile = XdtBaukastenDeviceProfileComboBox.SelectedItem as DeviceProfileDefinition;
        var previousProfileId = _xdtBaukastenState.DeviceProfile?.Metadata.Id;
        var profileChanged = !string.Equals(previousProfileId, profile?.Metadata.Id, StringComparison.OrdinalIgnoreCase);
        _xdtBaukastenState.SetDeviceProfile(profile);
        if (profileChanged)
        {
            _xdtBaukastenState.ClearPreviewResult();
            if (_xdtBaukastenState.DeviceInput is not null)
            {
                var compatibility = EvaluateCurrentDeviceInputCompatibility();
                SetXdtBaukastenStatus(compatibility.IsWarning
                    ? compatibility.Message
                    : "Geräteprofil geändert. Geladene Gerätedatei bleibt im Baukasten erhalten und wurde neu bewertet.");
            }
        }

        UpdateXdtBaukastenDeviceIdentity(profile);
        RefreshXdtBaukastenRuleDirectionUi();
        RefreshXdtBaukastenRuleGrid();
        UpdateXdtBaukastenPlaceholders();
        XdtBaukastenLoadAisOrSerialButton.Content = _xdtBaukastenState.PrimaryInputButtonText;
        XdtBaukastenPrimaryRawGroupBox.Header = _xdtBaukastenState.PrimaryRawInputTitle;
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private void UpdateXdtBaukastenExportProfileFromSelection()
    {
        var profile = XdtBaukastenExportProfileComboBox.SelectedItem as ExportProfileDefinition;
        _xdtBaukastenState.SetExportProfile(profile);
        RefreshXdtBaukastenRuleDirectionUi();
        RefreshXdtBaukastenRuleGrid();

        SetXdtBaukastenStatus(profile is null
            ? "Bitte ein Mapping-/Exportprofil auswählen."
            : $"Exportprofil als Arbeitskopie geladen: {profile.Metadata.Name}. BuiltIn-Profile werden nicht direkt verändert.");
        XdtBaukastenDraftStatusText.Text = "Keine Exportregel ausgewählt.";
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private void XdtBaukastenRuleDirection_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingXdtBaukastenSelection)
        {
            return;
        }

        var direction = sender == XdtBaukastenRuleDirectionDeviceOutputRadioButton
            ? XdtBaukastenRuleDirection.DeviceOutput
            : XdtBaukastenRuleDirection.AisExport;

        if (direction == _xdtBaukastenState.CurrentRuleDirection)
        {
            return;
        }

        _xdtBaukastenState.SetRuleDirection(direction);
        RefreshXdtBaukastenRuleDirectionUi();
        RefreshXdtBaukastenRuleGrid();
        SelectXdtBaukastenResultViewForCurrentRuleDirection();
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private void RefreshXdtBaukastenRuleDirectionUi()
    {
        if (XdtBaukastenRuleDirectionAisRadioButton is null
            || XdtBaukastenRuleDirectionDeviceOutputRadioButton is null)
        {
            return;
        }

        _updatingXdtBaukastenSelection = true;
        try
        {
            XdtBaukastenRuleDirectionAisRadioButton.IsChecked = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.AisExport;
            XdtBaukastenRuleDirectionDeviceOutputRadioButton.IsChecked = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput;
            XdtBaukastenRuleDirectionDeviceOutputRadioButton.IsEnabled = _xdtBaukastenState.IsBidirectionalDevice;
        }
        finally
        {
            _updatingXdtBaukastenSelection = false;
        }

        if (XdtBaukastenRuleTargetColumn is not null)
        {
            XdtBaukastenRuleTargetColumn.Header = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                ? "Ziel / Target"
                : "Feld";
        }

        if (XdtBaukastenDraftTargetLabel is not null)
        {
            XdtBaukastenDraftTargetLabel.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                ? "Ziel-Element:"
                : "Ziel-Feld:";
        }

        if (XdtBaukastenAddExportRuleButton is not null)
        {
            XdtBaukastenAddExportRuleButton.ToolTip = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                ? "Neue Geräteausgabe-Regel in der Baukasten-Arbeitskopie hinzufügen"
                : "Neue Exportregel in der Baukasten-Arbeitskopie hinzufügen";
        }

        if (XdtBaukastenRuleDirectionHintText is not null)
        {
            XdtBaukastenRuleDirectionHintText.Text = _xdtBaukastenState.IsBidirectionalDevice
                ? _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                    ? "Export an Gerät ist aktiv. Änderungen betreffen nur die Baukasten-Arbeitskopie und schreiben keine produktive Datei."
                    : "Export an AIS ist aktiv. Die Geräteausgabe-Regeln bleiben separat erhalten."
                : "Dieses Gerät hat keine Ausgabe an das Gerät.";
        }
    }

    private void RefreshXdtBaukastenRuleGrid()
    {
        var selectedRuleId = _xdtBaukastenSelectedRuleId;
        _xdtBaukastenExportRules.Clear();
        var rowNumber = 1;
        foreach (var rule in _xdtBaukastenState.CurrentWorkingRules.OrderBy(rule => rule.SortOrder).ThenBy(rule => rule.Id, StringComparer.OrdinalIgnoreCase))
        {
            _xdtBaukastenExportRules.Add(new XdtBaukastenRuleGridRow(rowNumber++, rule));
        }

        var selected = string.IsNullOrWhiteSpace(selectedRuleId)
            ? null
            : _xdtBaukastenExportRules.FirstOrDefault(row => string.Equals(row.Rule.Id, selectedRuleId, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
        {
            XdtBaukastenExportRulesGrid.SelectedItem = selected;
            return;
        }

        _xdtBaukastenSelectedRuleId = null;
        XdtBaukastenExportRulesGrid.SelectedItem = null;
        ClearXdtBaukastenRuleDraft();
    }

    private void ClearXdtBaukastenRuleDraft()
    {
        XdtBaukastenDraftTargetFieldCodeTextBox.Text = string.Empty;
        XdtBaukastenDraftTargetNameTextBox.Text = string.Empty;
        XdtBaukastenDraftSourcePathTextBox.Text = string.Empty;
        XdtBaukastenDraftOutputTemplateTextBox.Text = string.Empty;
        XdtBaukastenDraftStatusText.Text = "Keine Exportregel ausgewählt.";
    }

    private void UpdateXdtBaukastenDeviceIdentity(DeviceProfileDefinition? profile)
    {
        if (profile is null)
        {
            ClearXdtBaukastenDeviceIdentity();
            return;
        }

        XdtBaukastenDeviceNameText.Text = profile.Metadata.Name;
        XdtBaukastenManufacturerText.Text = DisplayOrDash(profile.Manufacturer);
        XdtBaukastenConnectionKindText.Text = profile.ConnectionKind == DeviceConnectionKind.SerialRs232 ? "Seriell RS232" : "LAN / Datei / UNC";
        XdtBaukastenDeviceTypeText.Text = DisplayOrDash(profile.DeviceType);
        XdtBaukastenBidirectionalText.Text = profile.IsBidirectional ? "Ja" : "Nein";
        XdtBaukastenParserText.Text = DisplayOrDash(profile.ParserMode);
        XdtBaukastenProfileNameText.Text = profile.Metadata.Name;
        XdtBaukastenDeviceTechnicalProfileButton.IsEnabled = true;

        var imagePath = string.Empty;
        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _deviceProfileImageOverrideService.LoadOverrides(paths).TryGetValue(profile.Metadata.Id, out var overridePath);
            imagePath = _deviceProfileImageOverrideService.ResolveEffectiveImagePath(profile, overridePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            imagePath = profile.DeviceImagePath;
        }

        XdtBaukastenDeviceImagePathTextBox.Text = imagePath;
        XdtBaukastenDeviceImagePlaceholder.Visibility = string.IsNullOrWhiteSpace(imagePath)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ClearXdtBaukastenDeviceIdentity()
    {
        XdtBaukastenDeviceNameText.Text = "Kein Gerät geladen";
        XdtBaukastenManufacturerText.Text = "-";
        XdtBaukastenConnectionKindText.Text = "-";
        XdtBaukastenDeviceTypeText.Text = "-";
        XdtBaukastenBidirectionalText.Text = "-";
        XdtBaukastenParserText.Text = "-";
        XdtBaukastenProfileNameText.Text = "-";
        XdtBaukastenDeviceTechnicalProfileButton.IsEnabled = false;
        XdtBaukastenDeviceImagePathTextBox.Text = string.Empty;
        XdtBaukastenDeviceImagePlaceholder.Visibility = Visibility.Visible;
    }

    private void ShowXdtBaukastenDeviceTechnicalProfile_Click(object sender, RoutedEventArgs e)
    {
        ShowDeviceTechnicalProfile(_xdtBaukastenState.DeviceProfile?.Metadata.Id);
    }

    private void XdtBaukastenLoadTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        var paths = _appDataPathProvider.GetDefaultUserPaths();
        var templateFolder = _xdtBaukastenTemplateLibraryService.GetTemplateFolder(paths);
        var hasLocalTemplates = _xdtBaukastenTemplateLibraryService.ListTemplateFiles(paths).Count > 0;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Baukasten-Template laden",
            Filter = "Baukasten-Templates (*.xdtbaukasten.template.json)|*.xdtbaukasten.template.json|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };
        if (Directory.Exists(templateFolder))
        {
            dialog.InitialDirectory = templateFolder;
        }

        if (!hasLocalTemplates)
        {
            SetXdtBaukastenStatus("Noch kein lokales Baukasten-Template gefunden. Speichern Sie zuerst eine Konfiguration oder wählen Sie eine Template-Datei manuell.", showDialog: true);
        }

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var template = _xdtBaukastenTemplateLibraryService.Load(dialog.FileName);
            LoadXdtBaukastenTemplate(template);
            SetXdtBaukastenStatus($"Baukasten-Template geladen: {template.Name}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            SetXdtBaukastenStatus($"Baukasten-Template konnte nicht geladen werden: {ex.Message}", showDialog: true);
        }
    }

    private async void XdtBaukastenImportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Template Paket importieren",
            Filter = "Templatepakete (*.zip;*.templatepackage.zip)|*.zip;*.templatepackage.zip|Alle Dateien (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            XdtBaukastenImportTemplatePackageButton.IsEnabled = false;
            SetXdtBaukastenStatus("Template Paket wird geprüft. Es wird noch nichts gespeichert.");
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var existingCatalog = _profileCatalog ?? CreateEmptyProfileCatalog();
            var preview = await Task.Run(() => _templatePackageImportPreviewService.Create(dialog.FileName, existingCatalog));
            if (preview.ValidationResult.HasErrors)
            {
                SetXdtBaukastenStatus("Templatepaket konnte nicht validiert werden: "
                    + string.Join("; ", preview.ValidationResult.Issues.Where(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error).Select(issue => issue.Message)));
                return;
            }

            var importDialog = new XdtBaukastenTemplatePackageImportDialog(preview, existingCatalog, paths)
            {
                Owner = this
            };
            if (importDialog.ShowDialog() == true && importDialog.ImportSucceeded)
            {
                var updatedCatalog = _profileCatalogService.Load(paths);
                _profileCatalog = updatedCatalog;
                RefreshProfileOverview(
                    updatedCatalog,
                    selectedExportProfileId: importDialog.ImportedExportProfileId,
                    selectedAisProfileId: importDialog.ImportedAisProfileId,
                    selectedDeviceProfileId: importDialog.ImportedDeviceProfileId);
                SetXdtBaukastenStatus("Template Paket wurde im Baukasten importiert und geladen. BuiltIn-Profile wurden nicht überschrieben.");
            }
            else
            {
                SetXdtBaukastenStatus("Template Paket wurde geprüft. Es wurde nichts übernommen.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            SetXdtBaukastenStatus($"Templatepaket konnte nicht gelesen werden: {ex.Message}");
        }
        finally
        {
            XdtBaukastenImportTemplatePackageButton.IsEnabled = true;
        }
    }

    private void XdtBaukastenNewAis_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var dialog = new NewAisProfileDialog { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = _userDefinedProfileCreationService.CreateAisProfile(catalog, dialog.Request, DateTimeOffset.UtcNow, Environment.UserName);
        if (!result.Success || result.Profile is null)
        {
            XdtBaukastenStatusText.Text = string.Join(Environment.NewLine, result.Issues);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewAisProfile(paths, result.Profile);
            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog, selectedAisProfileId: result.Profile.Metadata.Id);
            XdtBaukastenStatusText.Text = $"AIS-Profil angelegt und in den Baukasten geladen: {result.Profile.Metadata.Name}.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            XdtBaukastenStatusText.Text = $"AIS-Profil konnte nicht gespeichert werden: {ex.Message}";
        }
    }

    private void XdtBaukastenLoadDevice_Click(object sender, RoutedEventArgs e)
    {
        if (XdtBaukastenDeviceProfileComboBox.SelectedItem is DeviceProfileDefinition profile)
        {
            PushXdtBaukastenUndoState();
            UpdateXdtBaukastenDeviceProfileFromSelection();
            XdtBaukastenStatusText.Text = $"Geräteprofil in den Baukasten geladen: {profile.Metadata.Name}.";
            return;
        }

        XdtBaukastenDeviceProfileComboBox.IsDropDownOpen = true;
        XdtBaukastenStatusText.Text = "Bitte ein Geräteprofil auswählen.";
    }

    private void XdtBaukastenSaveTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        if (_xdtBaukastenState.AisProfile is null
            || _xdtBaukastenState.DeviceProfile is null
            || _xdtBaukastenState.SourceExportProfile is null)
        {
            SetXdtBaukastenStatus("Bitte zuerst AIS-Profil, Geräteprofil und Exportprofil im Baukasten auswählen.", showDialog: true);
            return;
        }

        var suggestedName = UserDefinedProfileCreationService.CreateAvailableProfileName(
            catalog.ExportProfiles.Select(profile => profile.Metadata.Name),
            $"{_xdtBaukastenState.AisProfile.Metadata.Name} + {_xdtBaukastenState.DeviceProfile.Metadata.Name} Baukasten");
        var suggestedDescription = $"Baukasten-Template aus {_xdtBaukastenState.SourceExportProfile.Metadata.Name}.";
        var dialog = new XdtBaukastenSaveTemplateDialog(
            suggestedName,
            suggestedDescription,
            catalog.ExportProfiles.Select(profile => profile.Metadata.Name))
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var workingExportProfile = _xdtBaukastenState.CreateWorkingExportProfile();
        if (workingExportProfile is null)
        {
            SetXdtBaukastenStatus("Baukasten-Konfiguration konnte nicht gespeichert werden, weil keine Exportprofil-Arbeitskopie vorhanden ist.", showDialog: true);
            return;
        }

        var createResult = _userDefinedProfileCreationService.CreateExportProfile(
            catalog,
            new UserDefinedExportProfileCreationRequest(
                dialog.TemplateName,
                _xdtBaukastenState.AisProfile.Metadata.Id,
                _xdtBaukastenState.DeviceProfile.Metadata.Id,
                workingExportProfile.OutputEncoding,
                workingExportProfile.Rules.ToList()),
            DateTimeOffset.UtcNow,
            Environment.UserName,
            idFactory: () => UserDefinedProfileCreationService.CreateUniqueProfileId(
                "export",
                dialog.TemplateName,
                catalog.ExportProfiles.Select(profile => profile.Metadata.Id)));

        if (!createResult.Success || createResult.Profile is null)
        {
            SetXdtBaukastenStatus("Baukasten-Konfiguration wurde nicht gespeichert: " + string.Join("; ", createResult.Issues), showDialog: true);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewExportProfile(paths, createResult.Profile);

            var template = new XdtBaukastenTemplate(
                Id: CreateXdtBaukastenTemplateId(dialog.TemplateName),
                Name: dialog.TemplateName,
                Description: dialog.Description,
                SavedAt: DateTimeOffset.UtcNow,
                SavedBy: Environment.UserName,
                AisProfileId: _xdtBaukastenState.AisProfile.Metadata.Id,
                DeviceProfileId: _xdtBaukastenState.DeviceProfile.Metadata.Id,
                ExportProfileId: createResult.Profile.Metadata.Id,
                AisExportRules: createResult.Profile.Rules.ToList(),
                DeviceOutputRules: _xdtBaukastenState.WorkingDeviceOutputRules.ToList());
            var templateFilePath = CreateAvailableXdtBaukastenTemplatePath(paths, dialog.TemplateName);
            _xdtBaukastenTemplateLibraryService.Save(templateFilePath, template, overwriteExisting: false);

            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(
                updatedCatalog,
                selectedExportProfileId: createResult.Profile.Metadata.Id,
                selectedAisProfileId: _xdtBaukastenState.AisProfile.Metadata.Id,
                selectedDeviceProfileId: _xdtBaukastenState.DeviceProfile.Metadata.Id);
            _xdtBaukastenState.ReplaceWorkingDeviceOutputRules(template.DeviceOutputRules);
            RefreshXdtBaukastenRuleDirectionUi();
            RefreshXdtBaukastenRuleGrid();
            RefreshXdtBaukastenPreviewIfPossible();
            SetXdtBaukastenStatus($"Baukasten-Konfiguration gespeichert: UserDefined-Exportprofil '{createResult.Profile.Metadata.Name}' und lokales Template '{templateFilePath}'.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            SetXdtBaukastenStatus($"Baukasten-Konfiguration konnte nicht gespeichert werden: {ex.Message}", showDialog: true);
        }
    }

    private void XdtBaukastenExportTemplatePackage_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var interfaceProfile = ResolveXdtBaukastenInterfaceProfile();
        if (interfaceProfile is null)
        {
            XdtBaukastenStatusText.Text = "Template Paket kann nicht exportiert werden, weil zur aktuellen AIS-/Geräte-/Export-Auswahl kein Schnittstellenprofil gefunden wurde.";
            return;
        }

        var selection = _templatePackageExportSelectionService.CreateForInterfaceProfile(catalog, interfaceProfile.Metadata.Id, DateTimeOffset.UtcNow);
        if (!selection.Success || selection.Request is null)
        {
            XdtBaukastenStatusText.Text = selection.ErrorMessage ?? "Template Paket kann mit der aktuellen Auswahl nicht exportiert werden.";
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Template Paket exportieren",
            Filter = "Templatepaket (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
            FileName = selection.SuggestedFileName
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            _templatePackageExporter.Export(dialog.FileName, selection.Request);
            XdtBaukastenStatusText.Text = $"Template Paket exportiert: {dialog.FileName}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            XdtBaukastenStatusText.Text = $"Template Paket konnte nicht exportiert werden: {ex.Message}";
        }
    }

    private void LoadXdtBaukastenTemplate(XdtBaukastenTemplate template)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var aisProfile = catalog.AisProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, template.AisProfileId, StringComparison.OrdinalIgnoreCase));
        var deviceProfile = catalog.DeviceProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, template.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        var exportProfile = catalog.ExportProfiles.FirstOrDefault(profile =>
            string.Equals(profile.Metadata.Id, template.ExportProfileId, StringComparison.OrdinalIgnoreCase));

        var missing = new List<string>();
        if (aisProfile is null)
        {
            missing.Add($"AIS-Profil {template.AisProfileId}");
        }

        if (deviceProfile is null)
        {
            missing.Add($"Geräteprofil {template.DeviceProfileId}");
        }

        if (exportProfile is null)
        {
            missing.Add($"Exportprofil {template.ExportProfileId}");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException("Template kann nicht geladen werden, weil Profile fehlen: " + string.Join(", ", missing));
        }

        PushXdtBaukastenUndoState();
        InitializeXdtBaukasten(
            catalog,
            selectedAisProfileId: aisProfile!.Metadata.Id,
            selectedDeviceProfileId: deviceProfile!.Metadata.Id,
            selectedExportProfileId: exportProfile!.Metadata.Id);
        _xdtBaukastenState.ReplaceWorkingExportRules(template.AisExportRules);
        _xdtBaukastenState.ReplaceWorkingDeviceOutputRules(template.DeviceOutputRules);
        RefreshXdtBaukastenRuleDirectionUi();
        RefreshXdtBaukastenRuleGrid();
        UpdateXdtBaukastenPlaceholders();
        RefreshXdtBaukastenPreviewIfPossible();
    }

    private string CreateAvailableXdtBaukastenTemplatePath(AppDataPaths paths, string templateName)
    {
        var candidate = _xdtBaukastenTemplateLibraryService.CreateDefaultFilePath(paths, templateName);
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var folder = Path.GetDirectoryName(candidate) ?? _xdtBaukastenTemplateLibraryService.GetTemplateFolder(paths);
        var nameWithoutExtension = Path.GetFileName(candidate).Replace(".xdtbaukasten.template.json", "", StringComparison.OrdinalIgnoreCase);
        for (var index = 2; index < 10_000; index++)
        {
            var indexedCandidate = Path.Combine(folder, $"{nameWithoutExtension}-{index}.xdtbaukasten.template.json");
            if (!File.Exists(indexedCandidate))
            {
                return indexedCandidate;
            }
        }

        return Path.Combine(folder, $"{nameWithoutExtension}-{Guid.NewGuid():N}.xdtbaukasten.template.json");
    }

    private static string CreateXdtBaukastenTemplateId(string templateName)
    {
        var safeName = TemplatePackageExportSelectionService.CreateSafeTemplatePackageFileName(templateName)
            .Replace(".templatepackage.zip", "", StringComparison.OrdinalIgnoreCase);
        return $"baukasten-template-{safeName}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    }

    private void XdtBaukastenChooseAis_Click(object sender, RoutedEventArgs e)
    {
        XdtBaukastenAisProfileComboBox.Focus();
        XdtBaukastenAisProfileComboBox.IsDropDownOpen = true;
    }

    private void XdtBaukastenNewDevice_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        var dialog = new NewDeviceProfileDialog(GetAvailableDeviceParserModes(catalog)) { Owner = this };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var result = _userDefinedProfileCreationService.CreateDeviceProfile(catalog, dialog.Request, DateTimeOffset.UtcNow, Environment.UserName);
        if (!result.Success || result.Profile is null)
        {
            XdtBaukastenStatusText.Text = string.Join(Environment.NewLine, result.Issues);
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            _profileCatalogService.SaveNewDeviceProfileDefinition(paths, result.Profile);
            var updatedCatalog = _profileCatalogService.Load(paths);
            _profileCatalog = updatedCatalog;
            RefreshProfileOverview(updatedCatalog, selectedDeviceProfileId: result.Profile.Metadata.Id);
            XdtBaukastenStatusText.Text = $"Geräteprofil angelegt und in den Baukasten geladen: {result.Profile.Metadata.Name}.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            XdtBaukastenStatusText.Text = $"Geräteprofil konnte nicht gespeichert werden: {ex.Message}";
        }
    }

    private void XdtBaukastenDeviceImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!TryGetProfileCatalogForProfileAction(out var catalog))
        {
            return;
        }

        try
        {
            var paths = _appDataPathProvider.GetDefaultUserPaths();
            var dialog = new LoadDeviceProfileDialog(catalog.DeviceProfiles, paths, _deviceProfileImageOverrideService) { Owner = this };
            _ = dialog.ShowDialog();
            if (dialog.HasChanges)
            {
                UpdateXdtBaukastenDeviceProfileFromSelection();
                XdtBaukastenStatusText.Text = "Gerätebild aktualisiert. BuiltIn-Fachprofile wurden nicht überschrieben.";
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            XdtBaukastenStatusText.Text = $"Gerätebild konnte nicht geändert werden: {ex.Message}";
        }
    }

    private void XdtBaukastenLoadAisOrSerial_Click(object sender, RoutedEventArgs e)
    {
        if (_xdtBaukastenState.IsSerialDevice)
        {
            var dialog = new XdtBaukastenSerialCaptureWindow(
                _serialPortDiscoveryService,
                _serialDeviceCommunicationService,
                _xdtBaukastenState.DeviceProfile?.SerialSettings)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true && dialog.CapturedInput is not null)
            {
                PushXdtBaukastenUndoState();
                _xdtBaukastenState.SetSerialInput(dialog.CapturedInput);
                XdtBaukastenAisRawTextBox.Text = _xdtBaukastenState.AisInput?.RawText ?? string.Empty;
                XdtBaukastenStatusText.Text = "RS232-Rohdaten in den Baukasten übernommen.";
                UpdateXdtBaukastenPlaceholders();
            }

            return;
        }

        var file = SelectTestInputFile("AIS-GDT/XDT-Datei laden", "AIS-Dateien (*.gdt;*.xdt;*.txt)|*.gdt;*.xdt;*.txt|Alle Dateien (*.*)|*.*");
        if (file is null)
        {
            return;
        }

        LoadRawFileIntoBaukasten(file, isAisFile: true);
    }

    private void XdtBaukastenLoadAttachment_Click(object sender, RoutedEventArgs e)
    {
        var file = SelectTestInputFile("Dateianhang laden", "Dateianhänge (*.pdf;*.jpg;*.jpeg;*.png;*.txt;*.xml;*.dcm)|*.pdf;*.jpg;*.jpeg;*.png;*.txt;*.xml;*.dcm|Alle Dateien (*.*)|*.*");
        if (file is null)
        {
            return;
        }

        var displayText = $"{Path.GetFileName(file)} ({file})";
        PushXdtBaukastenUndoState();
        _xdtBaukastenState.SetAttachmentInput(new XdtBaukastenLoadedInput(file, Path.GetFileName(file), displayText));
        XdtBaukastenAttachmentStatusText.Text = $"Dateianhang geladen: {displayText}";
    }

    private void XdtBaukastenLoadDeviceFile_Click(object sender, RoutedEventArgs e)
    {
        var file = SelectTestInputFile("Gerätedatei laden", "Gerätedateien (*.xml;*.csv;*.txt)|*.xml;*.csv;*.txt|Alle Dateien (*.*)|*.*");
        if (file is null)
        {
            return;
        }

        LoadRawFileIntoBaukasten(file, isAisFile: false);
    }

    private string? SelectTestInputFile(string title, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true
        };

        return dialog.ShowDialog(this) == true ? dialog.FileName : null;
    }

    private void LoadRawFileIntoBaukasten(string filePath, bool isAisFile)
    {
        try
        {
            var rawText = ReadTextFilePreview(filePath);
            var input = new XdtBaukastenLoadedInput(filePath, Path.GetFileName(filePath), rawText);
            PushXdtBaukastenUndoState();
            if (isAisFile)
            {
                _xdtBaukastenState.SetAisInput(input);
                _xdtBaukastenState.ClearPreviewResult();
                XdtBaukastenAisRawTextBox.Text = rawText;
                SetXdtBaukastenStatus($"AIS-Testdatei geladen: {filePath}");
            }
            else
            {
                _xdtBaukastenState.SetDeviceInput(input);
                _xdtBaukastenState.ClearPreviewResult();
                XdtBaukastenDeviceRawTextBox.Text = rawText;
                var compatibility = EvaluateCurrentDeviceInputCompatibility();
                SetXdtBaukastenStatus(compatibility.IsWarning
                    ? compatibility.Message
                    : compatibility.AllowsPreview
                        ? $"Gerätetestdatei geladen: {filePath}"
                        : compatibility.Message);
            }

            UpdateXdtBaukastenPlaceholders();
            RefreshXdtBaukastenPreviewIfPossible();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            SetXdtBaukastenStatus($"Testdatei konnte nicht gelesen werden: {ex.Message}");
        }
    }

    private string ReadTextFilePreview(string filePath)
    {
        return _xdtBaukastenTextEncodingReader.ReadText(filePath);
    }

    private void SetXdtBaukastenStatus(string message, bool showDialog = false)
    {
        if (XdtBaukastenStatusText is not null)
        {
            XdtBaukastenStatusText.Text = message;
        }

        if (XdtBaukastenTopStatusText is not null)
        {
            XdtBaukastenTopStatusText.Text = message;
        }

        if (showDialog)
        {
            System.Windows.MessageBox.Show(
                this,
                message,
                "XDT-Baukasten",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void XdtBaukastenChooseExportProfile_Click(object sender, RoutedEventArgs e)
    {
        XdtBaukastenExportProfileComboBox.Focus();
        XdtBaukastenExportProfileComboBox.IsDropDownOpen = true;
    }

    private void XdtBaukastenStartProcessing_Click(object sender, RoutedEventArgs e)
    {
        RunXdtBaukastenPreview();
    }

    private void RunXdtBaukastenPreview()
    {
        var compatibility = EvaluateCurrentDeviceInputCompatibility();
        if (!compatibility.AllowsPreview)
        {
            _xdtBaukastenState.ClearPreviewResult();
            UpdateXdtBaukastenResultView();
            UpdateXdtBaukastenPlaceholders();
            SetXdtBaukastenStatus(compatibility.Message);
            return;
        }

        var result = _xdtBaukastenPreviewService.BuildPreview(
            _xdtBaukastenState,
            ResolveXdtBaukastenInterfaceProfile(),
            DateTimeOffset.Now);
        _xdtBaukastenState.SetPreviewResult(result);
        if (result.Success)
        {
            const string successMessage = "Baukasten-Vorschau aktualisiert. Es wurde keine produktive Datei geschrieben.";
            SetXdtBaukastenStatus(compatibility.IsWarning
                ? $"{compatibility.Message} {successMessage}"
                : successMessage);
        }
        else
        {
            var message = string.Join(Environment.NewLine, result.Messages.Take(4));
            SetXdtBaukastenStatus(compatibility.IsWarning
                ? $"{compatibility.Message}{Environment.NewLine}{message}"
                : message);
        }
        UpdateXdtBaukastenResultView();
        UpdateXdtBaukastenPlaceholders();
    }

    private void RefreshXdtBaukastenPreviewIfPossible()
    {
        if (_xdtBaukastenState.SourceExportProfile is null
            || _xdtBaukastenState.AisInput is null
            || _xdtBaukastenState.DeviceInput is null)
        {
            return;
        }

        var compatibility = EvaluateCurrentDeviceInputCompatibility();
        if (!compatibility.AllowsPreview)
        {
            _xdtBaukastenState.ClearPreviewResult();
            UpdateXdtBaukastenResultView();
            UpdateXdtBaukastenPlaceholders();
            SetXdtBaukastenStatus(compatibility.Message);
            return;
        }

        RunXdtBaukastenPreview();
    }

    private void XdtBaukastenResultViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (XdtBaukastenResultLinesGrid is not null)
        {
            UpdateXdtBaukastenResultView();
        }
    }

    private void UpdateXdtBaukastenResultView()
    {
        var output = _xdtBaukastenState.PreviewResult?.Output;
        if (output is null)
        {
            XdtBaukastenResultGroupBox.Header = GetXdtBaukastenResultHeader();
            SetXdtBaukastenResultDocument(new XdtBaukastenPreviewDocument(
                GetSelectedXdtBaukastenResultView(),
                "Noch keine Vorschau erzeugt.",
                new[]
                {
                    new XdtBaukastenPreviewLine(
                        1,
                        "Noch keine Vorschau erzeugt.",
                        GetSelectedXdtBaukastenResultView(),
                        _xdtBaukastenState.CurrentRuleDirection)
                }));
            return;
        }

        var selectedTag = (XdtBaukastenResultViewComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        XdtBaukastenResultGroupBox.Header = GetXdtBaukastenResultHeader(selectedTag);
        var viewKind = GetSelectedXdtBaukastenResultView(selectedTag);
        if (output.Documents?.TryGetValue(viewKind, out var document) == true)
        {
            SetXdtBaukastenResultDocument(HighlightXdtBaukastenDocument(document));
            return;
        }

        var plainText = selectedTag switch
        {
            "AisView" => output.AisView,
            "DeviceOutput" => output.DeviceOutput,
            "Diagnostics" => output.Diagnostics,
            _ => output.RawXdt
        };
        SetXdtBaukastenResultDocument(HighlightXdtBaukastenDocument(CreateFallbackXdtBaukastenDocument(viewKind, plainText)));
    }

    private void SetXdtBaukastenResultDocument(XdtBaukastenPreviewDocument document)
    {
        XdtBaukastenResultTextBox.Text = document.PlainText;
        _xdtBaukastenResultLines.Clear();
        foreach (var line in document.Lines)
        {
            _xdtBaukastenResultLines.Add(line);
        }

        var highlighted = _xdtBaukastenResultLines.FirstOrDefault(line => line.IsHighlighted);
        if (highlighted is not null)
        {
            XdtBaukastenResultLinesGrid.ScrollIntoView(highlighted);
        }

        UpdateXdtBaukastenRuleOutputStatus(document);
    }

    private XdtBaukastenPreviewDocument HighlightXdtBaukastenDocument(XdtBaukastenPreviewDocument document)
    {
        if (string.IsNullOrWhiteSpace(_xdtBaukastenSelectedRuleId))
        {
            return document;
        }

        var lines = document.Lines
            .Select(line => line with { IsHighlighted = string.Equals(line.RuleId, _xdtBaukastenSelectedRuleId, StringComparison.OrdinalIgnoreCase) })
            .ToArray();
        return document with { Lines = lines };
    }

    private void UpdateXdtBaukastenRuleOutputStatus(XdtBaukastenPreviewDocument document)
    {
        if (string.IsNullOrWhiteSpace(_xdtBaukastenSelectedRuleId)
            || XdtBaukastenExportRulesGrid.SelectedItem is not XdtBaukastenRuleGridRow row)
        {
            return;
        }

        var hasHighlightedLine = document.Lines.Any(line => line.IsHighlighted);
        if (hasHighlightedLine)
        {
            var firstLine = document.Lines.First(line => line.IsHighlighted);
            XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                ? $"Geräteausgabe-Regel {row.RowNumber} markiert Ausgabezeile {firstLine.LineNumber}."
                : $"Exportregel {row.RowNumber} markiert Ausgabezeile {firstLine.LineNumber}.";
            return;
        }

        XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? $"Geräteausgabe-Regel {row.RowNumber} erzeugt aktuell keine Ausgabezeile in dieser Ansicht."
            : $"Exportregel {row.RowNumber} erzeugt aktuell keine Ausgabezeile in dieser Ansicht.";
    }

    private static XdtBaukastenPreviewDocument CreateFallbackXdtBaukastenDocument(XdtBaukastenResultView viewKind, string plainText)
    {
        var lines = SplitXdtBaukastenPreviewLines(plainText)
            .Select((line, index) => new XdtBaukastenPreviewLine(
                index + 1,
                line,
                viewKind,
                viewKind == XdtBaukastenResultView.DeviceOutput ? XdtBaukastenRuleDirection.DeviceOutput : XdtBaukastenRuleDirection.AisExport))
            .ToArray();
        return new XdtBaukastenPreviewDocument(viewKind, plainText, lines);
    }

    private static IReadOnlyList<string> SplitXdtBaukastenPreviewLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<string>();
        }

        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    private XdtBaukastenResultView GetSelectedXdtBaukastenResultView(string? selectedTag = null)
    {
        selectedTag ??= (XdtBaukastenResultViewComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        return selectedTag switch
        {
            "AisView" => XdtBaukastenResultView.AisView,
            "DeviceOutput" => XdtBaukastenResultView.DeviceOutput,
            "Diagnostics" => XdtBaukastenResultView.Diagnostics,
            _ => XdtBaukastenResultView.RawXdt
        };
    }

    private void SelectXdtBaukastenResultViewForCurrentRuleDirection()
    {
        var desiredTag = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? "DeviceOutput"
            : GetSelectedXdtBaukastenResultView() == XdtBaukastenResultView.DeviceOutput
                ? "RawXdt"
                : null;
        if (desiredTag is not null)
        {
            SelectXdtBaukastenResultViewByTag(desiredTag);
        }
    }

    private void SelectXdtBaukastenResultViewByTag(string tag)
    {
        foreach (var item in XdtBaukastenResultViewComboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
            {
                XdtBaukastenResultViewComboBox.SelectedItem = item;
                return;
            }
        }
    }

    private string GetXdtBaukastenResultHeader(string? selectedTag = null)
    {
        selectedTag ??= (XdtBaukastenResultViewComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
        return selectedTag switch
        {
            "AisView" => "Ansicht im AIS / Karteikartenansicht",
            "DeviceOutput" => "Ausgabe an das Gerät",
            "Diagnostics" => "Diagnose / erkannte Rohdaten und Verarbeitungsdetails",
            _ => "Roh-XDT-Ausgabe an das AIS"
        };
    }

    private InterfaceProfileDefinition? ResolveXdtBaukastenInterfaceProfile()
    {
        var aisProfileId = _xdtBaukastenState.AisProfile?.Metadata.Id;
        var deviceProfileId = _xdtBaukastenState.DeviceProfile?.Metadata.Id;
        var exportProfileId = _xdtBaukastenState.SourceExportProfile?.Metadata.Id;

        return _profileCatalog?.InterfaceProfiles
            .Where(profile =>
                (string.IsNullOrWhiteSpace(aisProfileId) || string.Equals(profile.AisProfileId, aisProfileId, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(deviceProfileId) || string.Equals(profile.DeviceProfileId, deviceProfileId, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(exportProfileId) || string.Equals(profile.ExportProfileId, exportProfileId, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(profile => profile.IsActive)
            .ThenBy(profile => profile.Metadata.IsBuiltIn ? 1 : 0)
            .ThenBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault();
    }

    private void XdtBaukastenExportRulesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (XdtBaukastenExportRulesGrid.SelectedItem is not XdtBaukastenRuleGridRow row)
        {
            _xdtBaukastenSelectedRuleId = null;
            XdtBaukastenDraftStatusText.Text = "Keine Exportregel ausgewählt.";
            UpdateXdtBaukastenResultView();
            return;
        }

        var rule = row.Rule;
        _xdtBaukastenSelectedRuleId = rule.Id;
        XdtBaukastenDraftTargetFieldCodeTextBox.Text = rule.TargetFieldCode;
        XdtBaukastenDraftTargetNameTextBox.Text = rule.TargetName;
        XdtBaukastenDraftSourcePathTextBox.Text = rule.SourcePath ?? string.Empty;
        XdtBaukastenDraftOutputTemplateTextBox.Text = rule.OutputTemplate;
        XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
            ? $"Geräteausgabe-Regel im Entwurf: {rule.TargetFieldCode} {rule.TargetName}"
            : $"Regel im Entwurf: {rule.TargetFieldCode} {rule.TargetName}";
        SelectXdtBaukastenResultViewForCurrentRuleDirection();
        UpdateXdtBaukastenResultView();
    }

    private void XdtBaukastenApplyDraftRule_Click(object sender, RoutedEventArgs e)
    {
        PushXdtBaukastenUndoState();
        if (TryApplyXdtBaukastenDraftRule(updateStatus: true))
        {
            RefreshXdtBaukastenPreviewIfPossible();
        }
    }

    private bool TryApplyXdtBaukastenDraftRule(bool updateStatus)
    {
        if (string.IsNullOrWhiteSpace(_xdtBaukastenSelectedRuleId))
        {
            if (updateStatus)
            {
                XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                    ? "Bitte zuerst eine Geräteausgabe-Regel auswählen."
                    : "Bitte zuerst eine Exportregel auswählen.";
            }

            return false;
        }

        var existing = _xdtBaukastenState.CurrentWorkingRules.FirstOrDefault(rule =>
            string.Equals(rule.Id, _xdtBaukastenSelectedRuleId, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            if (updateStatus)
            {
                XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                    ? "Die ausgewählte Geräteausgabe-Regel wurde in der Arbeitskopie nicht gefunden."
                    : "Die ausgewählte Exportregel wurde in der Arbeitskopie nicht gefunden.";
            }

            return false;
        }

        var targetFieldCode = XdtBaukastenDraftTargetFieldCodeTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(targetFieldCode))
        {
            if (updateStatus)
            {
                XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                    ? "Ziel-Element darf nicht leer sein."
                    : "TargetFieldCode darf nicht leer sein.";
            }

            return false;
        }

        if (string.IsNullOrWhiteSpace(XdtBaukastenDraftSourcePathTextBox.Text)
            && string.IsNullOrWhiteSpace(XdtBaukastenDraftOutputTemplateTextBox.Text))
        {
            if (updateStatus)
            {
                XdtBaukastenDraftStatusText.Text = "Leerer SourcePath ist nur erlaubt, wenn ein fester Regeltext eingetragen ist.";
            }

            return false;
        }

        var updated = existing with
        {
            TargetFieldCode = targetFieldCode,
            TargetName = XdtBaukastenDraftTargetNameTextBox.Text.Trim(),
            SourcePath = string.IsNullOrWhiteSpace(XdtBaukastenDraftSourcePathTextBox.Text)
                ? null
                : XdtBaukastenDraftSourcePathTextBox.Text.Trim(),
            OutputTemplate = XdtBaukastenDraftOutputTemplateTextBox.Text
        };

        if (!_xdtBaukastenState.UpdateWorkingRule(updated))
        {
            if (updateStatus)
            {
                XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                    ? "Geräteausgabe-Regel konnte nicht aktualisiert werden."
                    : "Exportregel konnte nicht aktualisiert werden.";
            }

            return false;
        }

        var index = _xdtBaukastenExportRules.ToList().FindIndex(row => string.Equals(row.Rule.Id, updated.Id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            var updatedRow = _xdtBaukastenExportRules[index] with { Rule = updated };
            _xdtBaukastenExportRules[index] = updatedRow;
            XdtBaukastenExportRulesGrid.SelectedItem = updatedRow;
        }

        if (updateStatus)
        {
            XdtBaukastenDraftStatusText.Text = _xdtBaukastenState.CurrentRuleDirection == XdtBaukastenRuleDirection.DeviceOutput
                ? "Geräteausgabe-Entwurf wurde in die Baukasten-Arbeitskopie übernommen. Das Originalprofil bleibt unverändert."
                : "Entwurf wurde in die Baukasten-Arbeitskopie übernommen. Das Originalprofil bleibt unverändert.";
        }

        return true;
    }

    private void UpdateXdtBaukastenPlaceholders()
    {
        var patient = GetXdtBaukastenPatientForPlaceholderValues();
        var measurements = GetXdtBaukastenMeasurementsForPlaceholders();

        _xdtBaukastenAisPlaceholders.Clear();
        foreach (var placeholder in CreateXdtBaukastenAisPlaceholders(patient))
        {
            _xdtBaukastenAisPlaceholders.Add(placeholder);
        }

        _xdtBaukastenDevicePlaceholders.Clear();
        foreach (var placeholder in _xdtBaukastenPlaceholderValueService.CreateDevicePlaceholders(_xdtBaukastenState.DeviceProfile, measurements))
        {
            _xdtBaukastenDevicePlaceholders.Add(placeholder);
        }

        _xdtBaukastenDeviceOutputPlaceholders.Clear();
        foreach (var placeholder in XdtBaukastenDeviceOutputRuleService.CreatePlaceholders(
                     _xdtBaukastenState.DeviceProfile,
                     patient,
                     GetXdtBaukastenHistoricalRecordsForDeviceOutput()))
        {
            _xdtBaukastenDeviceOutputPlaceholders.Add(placeholder);
        }
    }

    private PatientData? GetXdtBaukastenPatientForPlaceholderValues()
    {
        if (_xdtBaukastenState.PreviewResult?.PipelineResult?.Patient is { } previewPatient)
        {
            return previewPatient;
        }

        if (_xdtBaukastenState.AisInput is null || !File.Exists(_xdtBaukastenState.AisInput.SourcePath))
        {
            return null;
        }

        try
        {
            return _cv5000HistoryParser.ParseFile(_xdtBaukastenState.AisInput.SourcePath).Patient;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            try
            {
                var parser = new GdtParser();
                var mapper = new PatientDataMapper();
                var result = parser.ParseFile(_xdtBaukastenState.AisInput.SourcePath);
                return result.HasErrors ? null : mapper.Map(result.Records);
            }
            catch (Exception nestedEx) when (nestedEx is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                return null;
            }
        }
    }

    private IReadOnlyList<MeasurementValue> GetXdtBaukastenMeasurementsForPlaceholders()
    {
        var measurements = _xdtBaukastenState.PreviewResult?.PipelineResult?.Measurements
            ?? Array.Empty<MeasurementValue>();

        if (_xdtBaukastenPlaceholderValueService.IsCompatibleWithDeviceProfile(_xdtBaukastenState.DeviceProfile, measurements))
        {
            return measurements.ToArray();
        }

        return TryParseCurrentDeviceInputMeasurements(out var parsedMeasurements)
            ? parsedMeasurements
            : Array.Empty<MeasurementValue>();
    }

    private static IReadOnlyList<XdtBaukastenPlaceholder> CreateXdtBaukastenAisPlaceholders(PatientData? patient)
    {
        return new[]
        {
            new XdtBaukastenPlaceholder("AIS", "Patientennummer", "{AIS.PatientNumber}", "AIS-Patientennummer / GDT 3000", DisplayPlaceholderValue(patient?.PatientNumber)),
            new XdtBaukastenPlaceholder("AIS", "Nachname", "{AIS.LastName}", "AIS-Nachname / GDT 3101", DisplayPlaceholderValue(patient?.LastName)),
            new XdtBaukastenPlaceholder("AIS", "Vorname", "{AIS.FirstName}", "AIS-Vorname / GDT 3102", DisplayPlaceholderValue(patient?.FirstName)),
            new XdtBaukastenPlaceholder("AIS", "Geburtsdatum", "{AIS.DateOfBirth}", "AIS-Geburtsdatum / GDT 3103", DisplayPlaceholderValue(patient?.BirthDate)),
            new XdtBaukastenPlaceholder("AIS", "Untersuchungsart", "{AIS.ExamType}", "Untersuchungsart 8402 aus AIS", DisplayPlaceholderValue(patient?.ExaminationType)),
            new XdtBaukastenPlaceholder("AIS", "Datum", "{Date:ddMMyyyy}", "Aktuelles Datum", DateTime.Now.ToString("ddMMyyyy", CultureInfo.InvariantCulture)),
            new XdtBaukastenPlaceholder("AIS", "Uhrzeit", "{Time:HHmmss}", "Aktuelle Uhrzeit", DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture))
        };
    }

    private IReadOnlyList<AisHistoricalMeasurementRecord> GetXdtBaukastenHistoricalRecordsForDeviceOutput()
    {
        if (_xdtBaukastenState.AisInput is null
            || string.IsNullOrWhiteSpace(_xdtBaukastenState.AisInput.SourcePath)
            || !File.Exists(_xdtBaukastenState.AisInput.SourcePath))
        {
            return Array.Empty<AisHistoricalMeasurementRecord>();
        }

        try
        {
            return _cv5000HistoryParser.ParseFile(_xdtBaukastenState.AisInput.SourcePath).Records;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return Array.Empty<AisHistoricalMeasurementRecord>();
        }
    }

    private bool IsCurrentDeviceInputCompatible()
    {
        if (_xdtBaukastenState.DeviceProfile is null)
        {
            return true;
        }

        if (_xdtBaukastenState.DeviceInput is null)
        {
            return true;
        }

        return EvaluateCurrentDeviceInputCompatibility().AllowsPreview;
    }

    private XdtBaukastenDeviceCompatibilityResult EvaluateCurrentDeviceInputCompatibility()
    {
        if (_xdtBaukastenState.DeviceProfile is null || _xdtBaukastenState.DeviceInput is null)
        {
            return XdtBaukastenDeviceCompatibilityResult.Compatible(Array.Empty<MeasurementValue>());
        }

        return _xdtBaukastenDeviceCompatibilityService.EvaluateForWorkbench(
            _xdtBaukastenState.DeviceProfile,
            _xdtBaukastenState.DeviceInput.SourcePath);
    }

    private bool TryParseCurrentDeviceInputMeasurements(out IReadOnlyList<MeasurementValue> measurements)
    {
        measurements = Array.Empty<MeasurementValue>();
        if (_xdtBaukastenState.DeviceInput is null
            || string.IsNullOrWhiteSpace(_xdtBaukastenState.DeviceInput.SourcePath)
            || !File.Exists(_xdtBaukastenState.DeviceInput.SourcePath))
        {
            return false;
        }

        try
        {
            var parserMode = _xdtBaukastenState.DeviceProfile?.ParserMode;
            var result = HuvitzTextDeviceParser.IsParserMode(parserMode)
                ? _xdtBaukastenHuvitzTextParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                : ShinNipponDeviceParser.IsParserMode(parserMode)
                    ? _xdtBaukastenShinNipponParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                    : TomeyDeviceParser.IsParserMode(parserMode)
                        ? _xdtBaukastenTomeyParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                        : TomeyEmDeviceParser.IsParserMode(parserMode)
                            ? _xdtBaukastenTomeyEmParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                            : CanonZeissVisionixDeviceParser.IsParserMode(parserMode)
                                ? _xdtBaukastenCanonZeissVisionixParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                                : ZeissIolMaster700DeviceParser.IsParserMode(parserMode)
                                    ? _xdtBaukastenZeissIolMaster700Parser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath)
                                    : _xdtBaukastenDeviceParser.ParseFile(_xdtBaukastenState.DeviceInput.SourcePath);
            if (result.HasErrors)
            {
                return false;
            }

            measurements = result.Measurements;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return false;
        }
    }

    private static string DisplayPlaceholderValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 32 ? trimmed : trimmed[..29] + "...";
    }

    private void XdtBaukastenPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { Tag: string token } button || string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        PushXdtBaukastenUndoState();
        if (button.DataContext is XdtBaukastenPlaceholder { Category: "Ausgabe an Gerät" }
            && _xdtBaukastenState.IsBidirectionalDevice
            && _xdtBaukastenState.CurrentRuleDirection != XdtBaukastenRuleDirection.DeviceOutput)
        {
            _xdtBaukastenState.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
            RefreshXdtBaukastenRuleDirectionUi();
            RefreshXdtBaukastenRuleGrid();
            if (_xdtBaukastenExportRules.Count > 0)
            {
                XdtBaukastenExportRulesGrid.SelectedItem = _xdtBaukastenExportRules[0];
            }
        }

        var textBox = XdtBaukastenDraftOutputTemplateTextBox;
        var caret = textBox.CaretIndex;
        textBox.Text = textBox.Text.Insert(caret, token);
        textBox.CaretIndex = caret + token.Length;
        textBox.Focus();
        if (TryApplyXdtBaukastenDraftRule(updateStatus: false))
        {
            XdtBaukastenDraftStatusText.Text = "Platzhalter eingefügt und Vorschau aktualisiert.";
            RefreshXdtBaukastenPreviewIfPossible();
        }
    }

    private void XdtBaukastenUndo_Click(object sender, RoutedEventArgs e)
    {
        if (!_xdtBaukastenUndoBuffer.TryPop(out var snapshot) || snapshot is null)
        {
            UpdateXdtBaukastenUndoButtonState();
            return;
        }

        _restoringXdtBaukastenUndo = true;
        try
        {
            _xdtBaukastenState.RestoreSnapshot(snapshot);
            SyncXdtBaukastenUiFromState();
            RefreshXdtBaukastenPreviewIfPossible();
            XdtBaukastenStatusText.Text = "Letzte Baukasten-Änderung rückgängig gemacht.";
        }
        finally
        {
            _restoringXdtBaukastenUndo = false;
            UpdateXdtBaukastenUndoButtonState();
        }
    }

    private void PushXdtBaukastenUndoState()
    {
        if (_restoringXdtBaukastenUndo || TabBaukastenUndoButton is null)
        {
            return;
        }

        _xdtBaukastenUndoBuffer.Push(_xdtBaukastenState.CreateSnapshot());
        UpdateXdtBaukastenUndoButtonState();
    }

    private void UpdateXdtBaukastenUndoButtonState()
    {
        if (TabBaukastenUndoButton is not null)
        {
            TabBaukastenUndoButton.IsEnabled = _xdtBaukastenUndoBuffer.CanUndo;
        }
    }

    private void SyncXdtBaukastenUiFromState()
    {
        _updatingXdtBaukastenSelection = true;
        try
        {
            SelectXdtBaukastenComboItem(XdtBaukastenAisProfileComboBox, _xdtBaukastenState.AisProfile?.Metadata.Id);
            SelectXdtBaukastenComboItem(XdtBaukastenDeviceProfileComboBox, _xdtBaukastenState.DeviceProfile?.Metadata.Id);
            SelectXdtBaukastenComboItem(XdtBaukastenExportProfileComboBox, _xdtBaukastenState.SourceExportProfile?.Metadata.Id);
        }
        finally
        {
            _updatingXdtBaukastenSelection = false;
        }

        UpdateXdtBaukastenDeviceIdentity(_xdtBaukastenState.DeviceProfile);
        XdtBaukastenLoadAisOrSerialButton.Content = _xdtBaukastenState.PrimaryInputButtonText;
        XdtBaukastenPrimaryRawGroupBox.Header = _xdtBaukastenState.PrimaryRawInputTitle;
        XdtBaukastenAisRawTextBox.Text = _xdtBaukastenState.AisInput?.RawText ?? string.Empty;
        XdtBaukastenDeviceRawTextBox.Text = _xdtBaukastenState.DeviceInput?.RawText ?? string.Empty;
        XdtBaukastenAttachmentStatusText.Text = _xdtBaukastenState.AttachmentInput is null
            ? "Kein Dateianhang geladen."
            : $"Dateianhang geladen: {_xdtBaukastenState.AttachmentInput.DisplayName} ({_xdtBaukastenState.AttachmentInput.SourcePath})";

        RefreshXdtBaukastenRuleDirectionUi();
        RefreshXdtBaukastenRuleGrid();
        _xdtBaukastenSelectedRuleId = null;
        ClearXdtBaukastenRuleDraft();
        UpdateXdtBaukastenResultView();
        UpdateXdtBaukastenPlaceholders();
    }

    private static void SelectXdtBaukastenComboItem(System.Windows.Controls.ComboBox comboBox, string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            comboBox.SelectedItem = null;
            return;
        }

        foreach (var item in comboBox.Items)
        {
            var metadata = item switch
            {
                AisProfile aisProfile => aisProfile.Metadata,
                DeviceProfileDefinition deviceProfile => deviceProfile.Metadata,
                ExportProfileDefinition exportProfile => exportProfile.Metadata,
                _ => null
            };

            if (metadata is not null && string.Equals(metadata.Id, profileId, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
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

    private void RecordScanMonitoringEvents(InterfaceProfileDefinition? profile, string profileName, AutoImportScanResult result)
    {
        TryPlayNotificationSoundForDeviceFiles(result);
        var includeAttachmentDeviceFiles = profile?.FolderOptions.IsAttachmentOnlyMode == true;
        var allowAisOnlyManualSelection = profile?.FolderOptions.IsAttachmentOnlyMode == true
            && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;

        if (result.AisFilesDetected > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                MonitoringActivityEventKeyBuilder.CreateAisDetectedKey(result.Queue),
                $"{profileName}: AIS-Datei erkannt ({result.AisFilesDetected}).");
        }

        if (result.DeviceFilesDetected > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                MonitoringActivityEventKeyBuilder.CreateDeviceDetectedKey(result.Queue, includeAttachmentDeviceFiles),
                $"{profileName}: Gerätedatei erkannt ({result.DeviceFilesDetected}).");
        }

        if (result.ReadyPairs > 0)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                MonitoringActivityEventKeyBuilder.CreateReadyPairKey(result.Queue, includeAttachmentDeviceFiles, allowAisOnlyManualSelection),
                allowAisOnlyManualSelection
                    ? $"{profileName}: AIS-Datei bereit für manuelle Dokumentübergabe."
                    : $"{profileName}: AIS-/Geräte-Paar vollständig ({result.ReadyPairs}).");
        }

        foreach (var message in result.Messages)
        {
            AppendMonitoringEvent(
                result.InterfaceProfileId,
                $"scan-message:{message}",
                $"{profileName}: {message}",
                IsInformationalScanMessage(message)
                    ? InterfaceMonitoringEventSeverity.Info
                    : InterfaceMonitoringEventSeverity.Warning);
        }
    }

    private static bool IsInformationalScanMessage(string message)
    {
        return message.StartsWith("RS232-Profil:", StringComparison.OrdinalIgnoreCase);
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
        TryAutoDetachMonitoringCardForActivity(entry);
        TryUpdateAutoRedockForActivity(entry);
    }

    private void AppendProfileMessage(string message)
    {
        if (ProfileManagementStatusText is not null)
        {
            ProfileManagementStatusText.Text = message;
        }

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

    private sealed class PendingDocumentAttachmentConfirmation
    {
        public PendingDocumentAttachmentConfirmation(DocumentAttachmentDocumentationWindow window)
        {
            Window = window;
        }

        public DocumentAttachmentDocumentationWindow Window { get; }

        public bool IsTransferConfirmed { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsCompleting { get; set; }

        public IReadOnlyDictionary<string, string> FileDescriptions { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<AttachmentImportFileCandidate> SelectedCandidates { get; set; } =
            Array.Empty<AttachmentImportFileCandidate>();
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



