using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using XdtBox.LicenseIssuer;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtBox.LicenseManager;

public partial class MainWindow : Window
{
    private readonly LicenseManagerPathProvider _pathProvider = new();
    private readonly LicenseManagerSettingsRepository _settingsRepository = new();
    private readonly IssuedLicenseHistoryRepository _historyRepository = new();
    private readonly LicenseRequestFileRepository _requestRepository = new();
    private readonly LicenseIssuerService _issuerService = new();
    private readonly ObservableCollection<RequestDeviceRow> _requestDeviceRows = new();
    private readonly ObservableCollection<HistoryRow> _historyRows = new();
    private readonly ObservableCollection<IssuedLicenseDeviceRecord> _historyDeviceRows = new();

    private readonly LicenseManagerPaths _paths;
    private LicenseManagerSettings _settings;
    private LicenseRequest? _currentRequest;
    private string? _currentRequestFile;
    private IReadOnlyList<IssuedLicenseRecord> _historyRecords = Array.Empty<IssuedLicenseRecord>();

    public MainWindow()
    {
        InitializeComponent();

        _paths = _pathProvider.GetDefaultPaths();
        _settings = _settingsRepository.LoadOrDefault(_paths.SettingsFile, _paths.BaseFolder);

        RequestDevicesGrid.ItemsSource = _requestDeviceRows;
        HistoryGrid.ItemsSource = _historyRows;
        HistoryDevicesGrid.ItemsSource = _historyDeviceRows;

        InitializeDefaults();
        LoadHistory();
    }

    private void InitializeDefaults()
    {
        ValidFromDatePicker.SelectedDate = DateTime.Today;
        ValidUntilDatePicker.SelectedDate = DateTime.Today.AddYears(1);
        GraceDaysTextBox.Text = _settings.DefaultGraceDays.ToString(CultureInfo.InvariantCulture);
        KeyIdTextBox.Text = _settings.KeyId;
        PrivateKeyPathTextBox.Text = _settings.PrivateKeyPath ?? string.Empty;
        SettingsOutputFolderTextBox.Text = _settings.DefaultOutputFolder;
        SettingsRequestFolderTextBox.Text = _settings.DefaultRequestFolder;
        SettingsKeyFolderTextBox.Text = _settings.DefaultKeyFolder;
        SettingsPrivateKeyPathTextBox.Text = _settings.PrivateKeyPath ?? string.Empty;
        SettingsKeyIdTextBox.Text = _settings.KeyId;
        SettingsIssuerTextBox.Text = _settings.DefaultIssuer;
        SettingsGraceDaysTextBox.Text = _settings.DefaultGraceDays.ToString(CultureInfo.InvariantCulture);
        SuggestedOutputFileIfEmpty();
    }

    private void OpenRequest_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "XDTBox-Lizenzanfrage öffnen",
            Filter = "Lizenzanfrage (*.json)|*.json|Alle Dateien (*.*)|*.*",
            InitialDirectory = Directory.Exists(_settings.DefaultRequestFolder)
                ? _settings.DefaultRequestFolder
                : _paths.RequestsFolder,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var request = _requestRepository.Load(dialog.FileName);
            var issues = request.Validate();
            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Lizenzanfrage ist ungueltig: " + string.Join("; ", issues));
            }

            if (!string.Equals(request.ProductCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Lizenzanfrage ist nicht fuer {XdtBoxLicenseConstants.ProductCode} ausgestellt.");
            }

            _currentRequest = request;
            _currentRequestFile = dialog.FileName;
            ShowRequest(request, dialog.FileName);
            CreateLicenseStatusText.Text = "Lizenzanfrage geladen.";
        }
        catch (Exception ex)
        {
            ShowError($"Lizenzanfrage konnte nicht gelesen werden: {ex.Message}");
        }
    }

    private void ShowRequest(LicenseRequest request, string filePath)
    {
        RequestFileTextBox.Text = filePath;
        InstallationIdTextBox.Text = request.InstallationId;
        RequestDateTextBlock.Text = request.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
        RequestAppVersionTextBlock.Text = request.AppVersion;
        RequestedActiveConnectionsTextBlock.Text = request.ActiveLicensedDeviceCount.ToString(CultureInfo.InvariantCulture);

        var customer = request.Customer ?? LicenseRequestCustomer.Empty;
        CustomerNameTextBox.Text = customer.CustomerName;
        StreetTextBox.Text = customer.Street;
        PostalCodeTextBox.Text = customer.PostalCode;
        CityTextBox.Text = customer.City;
        PhoneTextBox.Text = customer.Phone;
        EmailTextBox.Text = customer.Email ?? string.Empty;
        ContactPersonTextBox.Text = customer.ContactPerson ?? string.Empty;
        LicenseeTextBox.Text = string.IsNullOrWhiteSpace(customer.CustomerName) ? request.MachineName : customer.CustomerName;
        MaxActiveConnectionsTextBox.Text = Math.Max(request.ActiveLicensedDeviceCount, 1).ToString(CultureInfo.InvariantCulture);

        _requestDeviceRows.Clear();
        var index = 1;
        foreach (var device in request.Devices)
        {
            _requestDeviceRows.Add(RequestDeviceRow.FromRequestDevice(index++, device));
        }

        SuggestedOutputFileIfEmpty();
    }

    private void BrowsePrivateKey_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Privaten RSA-Schlüssel auswählen",
            Filter = "PEM-Schlüssel (*.pem)|*.pem|Alle Dateien (*.*)|*.*",
            InitialDirectory = Directory.Exists(_settings.DefaultKeyFolder)
                ? _settings.DefaultKeyFolder
                : _paths.KeysFolder,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            PrivateKeyPathTextBox.Text = dialog.FileName;
        }
    }

    private void SuggestOutputFile_Click(object sender, RoutedEventArgs e)
    {
        OutputFileTextBox.Text = CreateSuggestedOutputFile();
    }

    private void CreateLicense_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var options = CreateIssuerOptions();
            var result = _issuerService.CreateLicense(options);
            var record = CreateHistoryRecord(result, options);
            _historyRecords = _historyRepository.Add(_paths.HistoryFile, record);
            RefreshHistoryRows();
            CreateLicenseStatusText.Text = $"Lizenz erzeugt: {result.OutputFile}";
            HistoryStatusText.Text = "Historie aktualisiert.";
        }
        catch (Exception ex)
        {
            ShowError($"Lizenz konnte nicht erzeugt werden: {ex.Message}");
        }
    }

    private LicenseIssuerOptions CreateIssuerOptions()
    {
        if (!int.TryParse(MaxActiveConnectionsTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxActiveConnections))
        {
            throw new InvalidOperationException("MaxActiveDeviceConnections ist keine Zahl.");
        }

        if (!int.TryParse(GraceDaysTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var graceDays))
        {
            throw new InvalidOperationException("Karenzzeit ist keine Zahl.");
        }

        var validFrom = ValidFromDatePicker.SelectedDate?.Date
            ?? throw new InvalidOperationException("Gueltig ab fehlt.");
        var validUntil = ValidUntilDatePicker.SelectedDate?.Date
            ?? throw new InvalidOperationException("Gueltig bis fehlt.");

        var outputFile = string.IsNullOrWhiteSpace(OutputFileTextBox.Text)
            ? CreateSuggestedOutputFile()
            : OutputFileTextBox.Text.Trim();
        OutputFileTextBox.Text = outputFile;

        return new LicenseIssuerOptions(
            RequestFile: string.IsNullOrWhiteSpace(_currentRequestFile) ? null : _currentRequestFile,
            InstallationId: string.IsNullOrWhiteSpace(_currentRequestFile) ? InstallationIdTextBox.Text.Trim() : null,
            LicenseeName: LicenseeTextBox.Text.Trim(),
            CustomerNumber: NormalizeOptional(CustomerNumberTextBox.Text),
            MaxActiveDeviceConnections: maxActiveConnections,
            ValidFromUtc: DateTime.SpecifyKind(validFrom, DateTimeKind.Utc),
            ValidUntilUtc: DateTime.SpecifyKind(validUntil, DateTimeKind.Utc),
            GraceDays: graceDays,
            LicenseType: GetSelectedLicenseType(),
            Issuer: _settings.DefaultIssuer,
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            Notes: NormalizeOptional(NotesTextBox.Text),
            KeyId: KeyIdTextBox.Text.Trim(),
            PrivateKeyPath: PrivateKeyPathTextBox.Text.Trim(),
            OutputFile: outputFile);
    }

    private IssuedLicenseRecord CreateHistoryRecord(LicenseIssuerResult result, LicenseIssuerOptions options)
    {
        var customer = ReadCustomerFromUi();
        var devices = _currentRequest?.Devices.Select(device => new IssuedLicenseDeviceRecord(
                DisplayName: string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                DeviceDisplayName: string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                InterfaceProfileId: string.IsNullOrWhiteSpace(device.InterfaceProfileId) ? device.ProfileId : device.InterfaceProfileId,
                DeviceProfileId: device.DeviceProfileId,
                ConnectionKind: device.ConnectionKind))
            .ToArray()
            ?? _historyDeviceRows.ToArray();

        return new IssuedLicenseRecord(
            LicenseId: result.Payload.LicenseId,
            IssuedAtUtc: result.Payload.IssuedAtUtc,
            LicenseeName: result.Payload.LicenseeName,
            CustomerNumber: result.Payload.CustomerNumber,
            CustomerName: customer.CustomerName,
            Street: customer.Street,
            PostalCode: customer.PostalCode,
            City: customer.City,
            Phone: customer.Phone,
            Email: customer.Email,
            ContactPerson: customer.ContactPerson,
            InstallationId: result.Payload.InstallationId,
            MaxActiveDeviceConnections: result.Payload.MaxActiveDeviceConnections,
            ValidFromUtc: result.Payload.ValidFromUtc,
            ValidUntilUtc: result.Payload.ValidUntilUtc,
            GraceDays: result.Payload.GraceDays,
            LicenseType: result.Payload.LicenseType,
            KeyId: options.KeyId,
            OutputFilePath: result.OutputFile,
            RequestFilePath: _currentRequestFile,
            Notes: result.Payload.Notes,
            Devices: devices);
    }

    private LicenseRequestCustomer ReadCustomerFromUi()
    {
        return new LicenseRequestCustomer(
            CustomerName: CustomerNameTextBox.Text.Trim(),
            Street: StreetTextBox.Text.Trim(),
            PostalCode: PostalCodeTextBox.Text.Trim(),
            City: CityTextBox.Text.Trim(),
            Phone: PhoneTextBox.Text.Trim(),
            Email: NormalizeOptional(EmailTextBox.Text),
            ContactPerson: NormalizeOptional(ContactPersonTextBox.Text));
    }

    private void OpenOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        var outputFile = string.IsNullOrWhiteSpace(OutputFileTextBox.Text)
            ? _settings.DefaultOutputFolder
            : OutputFileTextBox.Text.Trim();
        var folder = Directory.Exists(outputFile) ? outputFile : Path.GetDirectoryName(Path.GetFullPath(outputFile));
        OpenFolder(folder);
    }

    private void ResetInputs_Click(object sender, RoutedEventArgs e)
    {
        _currentRequest = null;
        _currentRequestFile = null;
        RequestFileTextBox.Text = string.Empty;
        InstallationIdTextBox.Text = string.Empty;
        RequestDateTextBlock.Text = string.Empty;
        RequestAppVersionTextBlock.Text = string.Empty;
        RequestedActiveConnectionsTextBlock.Text = string.Empty;
        CustomerNameTextBox.Text = string.Empty;
        StreetTextBox.Text = string.Empty;
        PostalCodeTextBox.Text = string.Empty;
        CityTextBox.Text = string.Empty;
        PhoneTextBox.Text = string.Empty;
        EmailTextBox.Text = string.Empty;
        ContactPersonTextBox.Text = string.Empty;
        LicenseeTextBox.Text = string.Empty;
        CustomerNumberTextBox.Text = string.Empty;
        MaxActiveConnectionsTextBox.Text = string.Empty;
        NotesTextBox.Text = string.Empty;
        OutputFileTextBox.Text = string.Empty;
        _requestDeviceRows.Clear();
        CreateLicenseStatusText.Text = "Eingaben zurückgesetzt.";
    }

    private void LoadHistory()
    {
        try
        {
            _historyRecords = _historyRepository.LoadOrEmpty(_paths.HistoryFile);
            RefreshHistoryRows();
            HistoryStatusText.Text = $"{_historyRecords.Count} Lizenzhistorien-Eintrag(e) geladen.";
        }
        catch (Exception ex)
        {
            _historyRecords = Array.Empty<IssuedLicenseRecord>();
            HistoryStatusText.Text = $"Historie konnte nicht geladen werden: {ex.Message}";
        }
    }

    private void RefreshHistoryRows()
    {
        var query = HistorySearchTextBox.Text.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _historyRecords
            : _historyRecords.Where(record => ContainsIgnoreCase(record.CustomerName, query)
                || ContainsIgnoreCase(record.CustomerNumber, query)
                || ContainsIgnoreCase(record.City, query)
                || ContainsIgnoreCase(record.InstallationId, query)
                || ContainsIgnoreCase(record.LicenseId, query));

        _historyRows.Clear();
        foreach (var record in filtered.OrderByDescending(record => record.IssuedAtUtc))
        {
            _historyRows.Add(new HistoryRow(record));
        }
    }

    private void HistorySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshHistoryRows();
    }

    private void ReloadHistory_Click(object sender, RoutedEventArgs e)
    {
        LoadHistory();
    }

    private void ExportHistory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Lizenzhistorie exportieren",
            Filter = "JSON (*.json)|*.json|Alle Dateien (*.*)|*.*",
            FileName = "xdtbox-license-history.json",
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
            var json = JsonSerializer.Serialize(_historyRecords, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);
            HistoryStatusText.Text = $"Historie exportiert: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            ShowError($"Historie konnte nicht exportiert werden: {ex.Message}");
        }
    }

    private void HistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        _historyDeviceRows.Clear();

        if (selected is null)
        {
            HistoryDetailsTextBlock.Text = "Keine Lizenz ausgewählt.";
            return;
        }

        HistoryDetailsTextBlock.Text =
            $"Kunde: {selected.CustomerName}{Environment.NewLine}" +
            $"Adresse: {selected.Street}, {selected.PostalCode} {selected.City}{Environment.NewLine}" +
            $"Kontakt: {selected.Phone} {selected.Email} {selected.ContactPerson}{Environment.NewLine}" +
            $"InstallationId: {selected.InstallationId}{Environment.NewLine}" +
            $"Lizenz: {selected.LicenseId}, {selected.MaxActiveDeviceConnections} Geräte, gültig bis {selected.ValidUntilUtc:yyyy-MM-dd}{Environment.NewLine}" +
            $"Ausgabe: {selected.OutputFilePath}";

        foreach (var device in selected.Devices)
        {
            _historyDeviceRows.Add(device);
        }
    }

    private void OpenSelectedLicenseFile_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        if (selected is null)
        {
            return;
        }

        OpenFileOrFolder(selected.OutputFilePath);
    }

    private void OpenSelectedLicenseFolder_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        if (selected is null)
        {
            return;
        }

        OpenFolder(Path.GetDirectoryName(Path.GetFullPath(selected.OutputFilePath)));
    }

    private void CreateNewForSelectedCustomer_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        if (selected is null)
        {
            return;
        }

        PopulateFromHistory(selected, includeInstallation: true);
        LicenseManagerTabs.SelectedIndex = 0;
        CreateLicenseStatusText.Text = "Kundendaten übernommen. Bitte neue Anfrage einlesen oder InstallationId prüfen.";
    }

    private void UseSelectedCustomerData_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        if (selected is null)
        {
            return;
        }

        PopulateFromHistory(selected, includeInstallation: false);
        LicenseManagerTabs.SelectedIndex = 0;
        CreateLicenseStatusText.Text = "Kundendaten übernommen.";
    }

    private void PopulateFromHistory(IssuedLicenseRecord selected, bool includeInstallation)
    {
        _currentRequest = null;
        _currentRequestFile = null;
        RequestFileTextBox.Text = string.Empty;
        InstallationIdTextBox.Text = includeInstallation ? selected.InstallationId : string.Empty;
        CustomerNameTextBox.Text = selected.CustomerName;
        StreetTextBox.Text = selected.Street;
        PostalCodeTextBox.Text = selected.PostalCode;
        CityTextBox.Text = selected.City;
        PhoneTextBox.Text = selected.Phone;
        EmailTextBox.Text = selected.Email ?? string.Empty;
        ContactPersonTextBox.Text = selected.ContactPerson ?? string.Empty;
        LicenseeTextBox.Text = selected.LicenseeName;
        CustomerNumberTextBox.Text = selected.CustomerNumber ?? string.Empty;
        MaxActiveConnectionsTextBox.Text = selected.MaxActiveDeviceConnections.ToString(CultureInfo.InvariantCulture);
        NotesTextBox.Text = selected.Notes ?? string.Empty;

        _requestDeviceRows.Clear();
        _historyDeviceRows.Clear();
        var index = 1;
        foreach (var device in selected.Devices)
        {
            _requestDeviceRows.Add(RequestDeviceRow.FromIssuedDevice(index++, device));
            _historyDeviceRows.Add(device);
        }

        OutputFileTextBox.Text = CreateSuggestedOutputFile();
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings = ReadSettingsFromUi();
            _settingsRepository.Save(_paths.SettingsFile, _settings);
            ApplySettingsToCreateTab();
            SettingsStatusText.Text = "Einstellungen gespeichert.";
        }
        catch (Exception ex)
        {
            ShowError($"Einstellungen konnten nicht gespeichert werden: {ex.Message}");
        }
    }

    private void CreateWorkingFolders_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings = ReadSettingsFromUi();
            Directory.CreateDirectory(_settings.DefaultOutputFolder);
            Directory.CreateDirectory(_settings.DefaultRequestFolder);
            Directory.CreateDirectory(_settings.DefaultKeyFolder);
            Directory.CreateDirectory(_paths.DataFolder);
            SettingsStatusText.Text = "Arbeitsordner angelegt.";
        }
        catch (Exception ex)
        {
            ShowError($"Arbeitsordner konnten nicht angelegt werden: {ex.Message}");
        }
    }

    private LicenseManagerSettings ReadSettingsFromUi()
    {
        if (!int.TryParse(SettingsGraceDaysTextBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var graceDays))
        {
            throw new InvalidOperationException("Standard-Karenzzeit ist keine Zahl.");
        }

        return new LicenseManagerSettings(
            DefaultOutputFolder: SettingsOutputFolderTextBox.Text.Trim(),
            DefaultRequestFolder: SettingsRequestFolderTextBox.Text.Trim(),
            DefaultKeyFolder: SettingsKeyFolderTextBox.Text.Trim(),
            PrivateKeyPath: NormalizeOptional(SettingsPrivateKeyPathTextBox.Text),
            KeyId: SettingsKeyIdTextBox.Text.Trim(),
            DefaultIssuer: SettingsIssuerTextBox.Text.Trim(),
            DefaultGraceDays: graceDays);
    }

    private void ApplySettingsToCreateTab()
    {
        if (string.IsNullOrWhiteSpace(PrivateKeyPathTextBox.Text))
        {
            PrivateKeyPathTextBox.Text = _settings.PrivateKeyPath ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(KeyIdTextBox.Text))
        {
            KeyIdTextBox.Text = _settings.KeyId;
        }

        if (string.IsNullOrWhiteSpace(GraceDaysTextBox.Text))
        {
            GraceDaysTextBox.Text = _settings.DefaultGraceDays.ToString(CultureInfo.InvariantCulture);
        }
    }

    private IssuedLicenseRecord? GetSelectedHistoryRecord()
    {
        return (HistoryGrid.SelectedItem as HistoryRow)?.Record;
    }

    private string CreateSuggestedOutputFile()
    {
        var rawName = string.IsNullOrWhiteSpace(LicenseeTextBox.Text)
            ? CustomerNameTextBox.Text
            : LicenseeTextBox.Text;
        var slug = CreateFileSlug(string.IsNullOrWhiteSpace(rawName) ? "xdtbox-lizenz" : rawName);
        return Path.Combine(_settings.DefaultOutputFolder, $"{slug}-{DateTime.Today:yyyyMMdd}.xdtboxlic");
    }

    private void SuggestedOutputFileIfEmpty()
    {
        if (string.IsNullOrWhiteSpace(OutputFileTextBox.Text))
        {
            OutputFileTextBox.Text = CreateSuggestedOutputFile();
        }
    }

    private string GetSelectedLicenseType()
    {
        return (LicenseTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Production";
    }

    private static string? NormalizeOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CreateFileSlug(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => invalidChars.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch)
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private static bool ContainsIgnoreCase(string? value, string query)
    {
        return value?.Contains(query, StringComparison.CurrentCultureIgnoreCase) == true;
    }

    private void OpenFileOrFolder(string path)
    {
        try
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                return;
            }

            ShowError($"Pfad nicht gefunden: {path}");
        }
        catch (Exception ex)
        {
            ShowError($"Pfad konnte nicht geöffnet werden: {ex.Message}");
        }
    }

    private void OpenFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            ShowError("Ordnerpfad fehlt.");
            return;
        }

        OpenFileOrFolder(folder);
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "XDTBox Lizenzverwaltung", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private sealed record RequestDeviceRow(
        int Index,
        string DisplayName,
        string DeviceDisplayName,
        DeviceConnectionKind ConnectionKind,
        bool IsActive)
    {
        public static RequestDeviceRow FromRequestDevice(int index, LicenseRequestDevice device)
        {
            return new RequestDeviceRow(
                index,
                string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                device.ConnectionKind,
                device.IsActive);
        }

        public static RequestDeviceRow FromIssuedDevice(int index, IssuedLicenseDeviceRecord device)
        {
            return new RequestDeviceRow(index, device.DisplayName, device.DeviceDisplayName, device.ConnectionKind, true);
        }
    }

    private sealed class HistoryRow
    {
        public HistoryRow(IssuedLicenseRecord record)
        {
            Record = record;
        }

        public IssuedLicenseRecord Record { get; }

        public string LicenseId => Record.LicenseId;
        public string CustomerName => Record.CustomerName;
        public string? CustomerNumber => Record.CustomerNumber;
        public string City => Record.City;
        public string Phone => Record.Phone;
        public string ShortInstallationId => Record.InstallationId.Length <= 14 ? Record.InstallationId : Record.InstallationId[..14] + "...";
        public int MaxActiveDeviceConnections => Record.MaxActiveDeviceConnections;
        public string ValidUntilDisplay => Record.ValidUntilUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        public string LicenseType => Record.LicenseType;
        public string IssuedAtDisplay => Record.IssuedAtUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
    }
}
