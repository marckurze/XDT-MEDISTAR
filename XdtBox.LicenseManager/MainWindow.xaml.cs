using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
    private readonly LicenseManagerCustomerRepository _customerRepository = new();
    private readonly LicenseRequestFileRepository _requestRepository = new();
    private readonly LicenseManagerCustomerPdfExporter _customerPdfExporter = new();
    private readonly LicenseManagerBackupService _backupService = new();
    private readonly LicenseIssuerService _issuerService = new();
    private readonly LicenseManagerCustomerMergeService _customerMergeService = new();
    private readonly ObservableCollection<RequestDeviceRow> _requestDeviceRows = new();
    private readonly ObservableCollection<HistoryRow> _historyRows = new();
    private readonly ObservableCollection<IssuedLicenseDeviceRecord> _historyDeviceRows = new();
    private readonly ObservableCollection<CustomerRow> _customerRows = new();
    private readonly HashSet<string> _markedCustomerIds = new(StringComparer.Ordinal);

    private readonly LicenseManagerPaths _paths;
    private LicenseManagerSettings _settings;
    private LicenseRequest? _currentRequest;
    private string? _currentRequestFile;
    private bool _currentRequestUsesSeparateCustomerRecord;
    private IReadOnlyList<IssuedLicenseRecord> _historyRecords = Array.Empty<IssuedLicenseRecord>();
    private IReadOnlyList<LicenseManagerCustomerRecord> _customerRecords = Array.Empty<LicenseManagerCustomerRecord>();
    private bool _updatingPaymentMethod;

    public MainWindow()
    {
        InitializeComponent();
        Title = $"XDTBox Lizenzmanager {GetApplicationVersionText()}";

        _paths = _pathProvider.GetDefaultPaths();
        _settings = _settingsRepository.LoadOrDefault(_paths.SettingsFile, _paths.BaseFolder);

        RequestDevicesGrid.ItemsSource = _requestDeviceRows;
        HistoryGrid.ItemsSource = _historyRows;
        HistoryDevicesGrid.ItemsSource = _historyDeviceRows;
        CustomersGrid.ItemsSource = _customerRows;

        InitializeDefaults();
        LoadHistory();
        LoadCustomers();
    }

    private void InitializeDefaults()
    {
        ValidFromDatePicker.SelectedDate = DateTime.Today;
        ValidUntilDatePicker.SelectedDate = XdtBoxLicenseConstants.UnlimitedValidUntilUtc.Date;
        SetPaymentMethod(LicenseManagerPaymentMethod.BankTransfer);
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
        var priceText = _settings.PricePerDeviceNet.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));
        CustomerPricePerDeviceTextBox.Text = priceText;
        SettingsPricePerDeviceTextBox.Text = priceText;
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
                throw new InvalidOperationException("Lizenzanfrage ist ungültig: " + string.Join("; ", issues));
            }

            if (!string.Equals(request.ProductCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Lizenzanfrage ist nicht für {XdtBoxLicenseConstants.ProductCode} ausgestellt.");
            }

            _currentRequest = request;
            _currentRequestFile = dialog.FileName;
            _currentRequestUsesSeparateCustomerRecord = false;
            ShowRequest(request, dialog.FileName);
            UpsertCustomerFromRequest(request);
            CreateLicenseStatusText.Text = "Lizenzanfrage geladen.";
        }
        catch (Exception ex)
        {
            ShowError($"Lizenzanfrage konnte nicht gelesen werden: {ex.Message}");
        }
    }

    private void ShowRequest(LicenseRequest request, string filePath)
    {
        var billableDevices = request.Devices
            .Where(device => device.IsActive && device.IsLicenseRequired)
            .ToArray();
        var requestedActiveConnections = request.Devices.Count > 0
            ? billableDevices.Length
            : request.ActiveLicensedDeviceCount;

        RequestFileTextBox.Text = filePath;
        InstallationIdTextBox.Text = request.InstallationId;
        RequestDateTextBlock.Text = request.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
        RequestAppVersionTextBlock.Text = request.AppVersion;
        RequestedActiveConnectionsTextBlock.Text = requestedActiveConnections.ToString(CultureInfo.InvariantCulture);

        var customer = request.Customer ?? LicenseRequestCustomer.Empty;
        CustomerNameTextBox.Text = customer.CustomerName;
        StreetTextBox.Text = customer.Street;
        PostalCodeTextBox.Text = customer.PostalCode;
        CityTextBox.Text = customer.City;
        PhoneTextBox.Text = customer.Phone;
        EmailTextBox.Text = customer.Email ?? string.Empty;
        ContactPersonTextBox.Text = customer.ContactPerson ?? string.Empty;
        InvoiceEmailTextBox.Text = customer.InvoiceEmail ?? string.Empty;
        CustomerIbanTextBox.Text = customer.Iban ?? string.Empty;
        CustomerBicTextBox.Text = customer.Bic ?? string.Empty;
        CustomerAccountHolderTextBox.Text = customer.AccountHolder ?? string.Empty;
        SetPaymentMethod(customer.SepaDirectDebitConsent && !customer.AlwaysInvoice
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer);
        CustomerNumberTextBox.Text = customer.CustomerNumber ?? string.Empty;
        LicenseeTextBox.Text = string.IsNullOrWhiteSpace(customer.CustomerName) ? request.MachineName : customer.CustomerName;
        MaxActiveConnectionsTextBox.Text = Math.Max(requestedActiveConnections, 1).ToString(CultureInfo.InvariantCulture);

        _requestDeviceRows.Clear();
        RequestDevicesHintTextBlock.Text = request.Devices.Count == 0
            ? "Geräteliste in alter Anfrage nicht enthalten. Lizenzpflichtig bleibt die angeforderte Geräteanzahl."
            : "Es werden nur aktive lizenzpflichtige Geräteanbindungen angezeigt.";
        var index = 1;
        foreach (var device in billableDevices)
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
            CommitRequestDeviceEdits();
            var options = CreateIssuerOptions();
            var result = _issuerService.CreateLicense(options);
            var record = CreateHistoryRecord(result, options);
            _historyRecords = _historyRepository.Add(_paths.HistoryFile, record);
            UpsertCustomerAfterLicense(record);
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
            ?? throw new InvalidOperationException("Gültig ab fehlt.");
        _ = ValidUntilDatePicker.SelectedDate;

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
            ValidUntilUtc: XdtBoxLicenseConstants.UnlimitedValidUntilUtc,
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
        var devices = _requestDeviceRows.Count > 0
            ? _requestDeviceRows
                .Where(row => row.IsActive && row.IsLicenseRequired)
                .Select(row => row.ToIssuedLicenseDeviceRecord())
                .ToArray()
            : _historyDeviceRows.ToArray();

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
            Devices: devices,
            MachineName: _currentRequest?.MachineName,
            InvoiceEmail: customer.InvoiceEmail,
            Iban: customer.Iban,
            Bic: customer.Bic,
            AccountHolder: customer.AccountHolder,
            SepaDirectDebitConsent: customer.SepaDirectDebitConsent,
            AlwaysInvoice: customer.AlwaysInvoice);
    }

    private LicenseRequestCustomer ReadCustomerFromUi()
    {
        var paymentMethod = GetSelectedPaymentMethod();
        ValidatePaymentMethod(
            paymentMethod,
            NormalizeOptional(CustomerIbanTextBox.Text),
            NormalizeOptional(CustomerAccountHolderTextBox.Text));

        return new LicenseRequestCustomer(
            CustomerName: CustomerNameTextBox.Text.Trim(),
            Street: StreetTextBox.Text.Trim(),
            PostalCode: PostalCodeTextBox.Text.Trim(),
            City: CityTextBox.Text.Trim(),
            Phone: PhoneTextBox.Text.Trim(),
            Email: NormalizeOptional(EmailTextBox.Text),
            ContactPerson: NormalizeOptional(ContactPersonTextBox.Text),
            Iban: NormalizeOptional(CustomerIbanTextBox.Text),
            Bic: NormalizeOptional(CustomerBicTextBox.Text),
            AccountHolder: NormalizeOptional(CustomerAccountHolderTextBox.Text),
            SepaDirectDebitConsent: paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit,
            AlwaysInvoice: paymentMethod == LicenseManagerPaymentMethod.BankTransfer,
            InvoiceEmail: NormalizeOptional(InvoiceEmailTextBox.Text),
            CustomerNumber: NormalizeOptional(CustomerNumberTextBox.Text));
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
        _currentRequestUsesSeparateCustomerRecord = false;
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
        InvoiceEmailTextBox.Text = string.Empty;
        CustomerIbanTextBox.Text = string.Empty;
        CustomerBicTextBox.Text = string.Empty;
        CustomerAccountHolderTextBox.Text = string.Empty;
        SetPaymentMethod(LicenseManagerPaymentMethod.BankTransfer);
        LicenseeTextBox.Text = string.Empty;
        CustomerNumberTextBox.Text = string.Empty;
        MaxActiveConnectionsTextBox.Text = string.Empty;
        NotesTextBox.Text = string.Empty;
        OutputFileTextBox.Text = string.Empty;
        _requestDeviceRows.Clear();
        RequestDevicesHintTextBlock.Text = "Es werden nur aktive lizenzpflichtige Geräteanbindungen angezeigt.";
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

    private void LoadCustomers()
    {
        try
        {
            _customerRecords = _customerRepository.LoadOrEmpty(_paths.CustomersFile);
            PruneMarkedCustomerIds();
            RefreshCustomerRows();
            CustomersStatusText.Text = $"{_customerRecords.Count} Kunde(n) geladen.";
        }
        catch (Exception ex)
        {
            _customerRecords = Array.Empty<LicenseManagerCustomerRecord>();
            PruneMarkedCustomerIds();
            RefreshCustomerRows();
            CustomersStatusText.Text = $"Kundenliste konnte nicht geladen werden: {ex.Message}";
        }
    }

    private void UpsertCustomerFromRequest(LicenseRequest request)
    {
        try
        {
            var incoming = LicenseManagerCustomerRecord.FromRequest(request);
            var existingByCustomerNumber = FindCustomerByCustomerNumber(incoming.CustomerNumber);
            if (existingByCustomerNumber is not null)
            {
                var confirmation = MessageBox.Show(
                    this,
                    $"Es gibt bereits den Kunden \"{existingByCustomerNumber.CustomerName}\" mit der Kundennummer {incoming.CustomerNumber}. Soll die Lizenzanfrage diesem bestehenden Kunden zugeordnet werden?",
                    "Kundennummer bereits vorhanden",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (confirmation == MessageBoxResult.Cancel)
                {
                    CustomersStatusText.Text = "Kundenzuordnung aus Lizenzanfrage abgebrochen.";
                    return;
                }

                _customerRecords = confirmation == MessageBoxResult.Yes
                    ? _customerRepository.Upsert(_paths.CustomersFile, incoming)
                    : _customerRepository.UpsertByInstallation(_paths.CustomersFile, incoming);
                _currentRequestUsesSeparateCustomerRecord = confirmation == MessageBoxResult.No;
                RefreshCustomerRows();
                CustomersStatusText.Text = confirmation == MessageBoxResult.Yes
                    ? $"Lizenzanfrage wurde dem bestehenden Kunden \"{existingByCustomerNumber.CustomerName}\" zugeordnet."
                    : "Lizenzanfrage wurde als separater Kundensatz gespeichert.";
                return;
            }

            _customerRecords = _customerRepository.Upsert(_paths.CustomersFile, incoming);
            RefreshCustomerRows();
            CustomersStatusText.Text = "Kunde aus Lizenzanfrage angelegt oder aktualisiert.";
        }
        catch (Exception ex)
        {
            CustomersStatusText.Text = $"Kunde konnte nicht gespeichert werden: {ex.Message}";
        }
    }

    private LicenseManagerCustomerRecord? FindCustomerByCustomerNumber(string? customerNumber)
    {
        if (string.IsNullOrWhiteSpace(customerNumber))
        {
            return null;
        }

        return _customerRecords.FirstOrDefault(customer =>
            string.Equals(customer.CustomerNumber, customerNumber, StringComparison.OrdinalIgnoreCase));
    }

    private void UpsertCustomerAfterLicense(IssuedLicenseRecord record)
    {
        var customer = _currentRequest is null
            ? CustomerFromCurrentUi(record.InstallationId)
            : LicenseManagerCustomerRecord.FromRequest(_currentRequest);
        customer = customer with
        {
            CustomerNumber = record.CustomerNumber,
            CustomerName = record.CustomerName,
            Street = record.Street,
            PostalCode = record.PostalCode,
            City = record.City,
            Phone = record.Phone,
            Email = record.Email,
            ContactPerson = record.ContactPerson,
            InvoiceEmail = record.InvoiceEmail,
            Iban = record.Iban,
            Bic = record.Bic,
            AccountHolder = record.AccountHolder,
            SepaDirectDebitConsent = record.SepaDirectDebitConsent,
            AlwaysInvoice = record.AlwaysInvoice
        };

        _customerRecords = _currentRequestUsesSeparateCustomerRecord
            ? _customerRepository.UpsertLicenseByInstallation(_paths.CustomersFile, customer, record)
            : _customerRepository.UpsertLicense(_paths.CustomersFile, customer, record);
        RefreshCustomerRows();
        CustomersStatusText.Text = "Kunde und Lizenzhistorie aktualisiert.";
    }

    private LicenseManagerCustomerRecord CustomerFromCurrentUi(string installationId)
    {
        var customer = ReadCustomerFromUi();
        return new LicenseManagerCustomerRecord(
            Id: Guid.NewGuid().ToString("N"),
            CustomerNumber: customer.CustomerNumber,
            CustomerName: customer.CustomerName,
            Street: customer.Street,
            PostalCode: customer.PostalCode,
            City: customer.City,
            Phone: customer.Phone,
            Email: customer.Email,
            ContactPerson: customer.ContactPerson,
            InvoiceEmail: customer.InvoiceEmail,
            Iban: customer.Iban,
            Bic: customer.Bic,
            AccountHolder: customer.AccountHolder,
            SepaDirectDebitConsent: customer.SepaDirectDebitConsent,
            AlwaysInvoice: customer.AlwaysInvoice,
            InstallationId: installationId,
            MachineName: _currentRequest?.MachineName,
            ActiveLicensedDeviceCount: int.TryParse(MaxActiveConnectionsTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0,
            Devices: _requestDeviceRows
                .Where(row => row.IsActive && row.IsLicenseRequired)
                .Select(row => row.ToIssuedLicenseDeviceRecord())
                .ToArray(),
            UpdatedAtUtc: DateTime.UtcNow);
    }

    private void RefreshCustomerRows()
    {
        var query = CustomerSearchTextBox.Text.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _customerRecords
            : _customerRecords.Where(customer => ContainsIgnoreCase(customer.CustomerName, query)
                || ContainsIgnoreCase(customer.CustomerNumber, query)
                || ContainsIgnoreCase(customer.ContactPerson, query)
                || ContainsIgnoreCase(customer.InvoiceEmail, query)
                || ContainsIgnoreCase(FormatPaymentMethod(customer.PaymentMethod), query)
                || customer.EffectiveInstallations.Any(installation => ContainsIgnoreCase(installation.InstallationId, query)));

        _customerRows.Clear();
        foreach (var customer in filtered.OrderBy(customer => customer.CustomerName, StringComparer.CurrentCultureIgnoreCase))
        {
            _customerRows.Add(new CustomerRow(
                customer,
                _settings.PricePerDeviceNet,
                _markedCustomerIds.Contains(customer.Id),
                SetCustomerMarked));
        }

        UpdateMergeCustomersButtonState();
        UpdateSelectedCustomerPdfButtonState();
        UpdateCustomerMonthlyTotal();
    }

    private void SetCustomerMarked(string customerId, bool isMarked)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return;
        }

        if (isMarked)
        {
            _markedCustomerIds.Add(customerId);
        }
        else
        {
            _markedCustomerIds.Remove(customerId);
        }

        UpdateMergeCustomersButtonState();
    }

    private void PruneMarkedCustomerIds()
    {
        var existingIds = _customerRecords
            .Select(customer => customer.Id)
            .ToHashSet(StringComparer.Ordinal);
        _markedCustomerIds.RemoveWhere(id => !existingIds.Contains(id));
    }

    private void UpdateMergeCustomersButtonState()
    {
        MergeCustomersButton.IsEnabled = _markedCustomerIds.Count >= 2;
    }

    private void UpdateCustomerMonthlyTotal()
    {
        var totalDevices = _customerRecords.Sum(customer => customer.BillableDeviceCount);
        var total = LicenseManagerCostCalculator.CalculateNetTotal(totalDevices, _settings.PricePerDeviceNet);
        var culture = CultureInfo.GetCultureInfo("de-DE");
        CustomerMonthlyTotalTextBlock.Text =
            $"Gesamtsumme monatlicher Lizenzen: {total.ToString("N2", culture)} EUR netto " +
            $"({totalDevices} aktive lizenzierte Geräteanbindung(en) × {_settings.PricePerDeviceNet.ToString("N2", culture)} EUR).";
    }

    private void CustomerSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshCustomerRows();
    }

    private void SaveCustomerPrice_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var price = ParsePrice(CustomerPricePerDeviceTextBox.Text);
            _settings = _settings with { PricePerDeviceNet = price };
            _settingsRepository.Save(_paths.SettingsFile, _settings);
            SettingsPricePerDeviceTextBox.Text = price.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));
            RefreshCustomerRows();
            CustomersStatusText.Text = "Preis gespeichert.";
        }
        catch (Exception ex)
        {
            ShowError($"Preis konnte nicht gespeichert werden: {ex.Message}");
        }
    }

    private void OpenCustomer_Click(object sender, RoutedEventArgs e)
    {
        OpenSelectedCustomer();
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedCustomerPdfButtonState();
    }

    private void CustomersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenSelectedCustomer();
    }

    private void UpdateSelectedCustomerPdfButtonState()
    {
        ExportSelectedCustomerPdfButton.IsEnabled = GetSelectedCustomer() is not null;
    }

    private void OpenSelectedCustomer()
    {
        var selected = GetSelectedCustomer();
        if (selected is null)
        {
            CustomersStatusText.Text = "Bitte zuerst einen Kunden auswählen.";
            return;
        }

        var window = new CustomerDetailWindow(selected, _historyRecords, _settings.PricePerDeviceNet)
        {
            Owner = this
        };

        if (window.ShowDialog() == true)
        {
            _customerRecords = _customerRepository.Upsert(_paths.CustomersFile, window.Customer);
            RefreshCustomerRows();
            CustomersStatusText.Text = "Kundendetails gespeichert.";
        }
    }

    private void MergeCustomers_Click(object sender, RoutedEventArgs e)
    {
        if (_markedCustomerIds.Count < 2)
        {
            CustomersStatusText.Text = "Bitte mindestens zwei Kunden markieren.";
            return;
        }

        var selectedCustomers = _customerRecords
            .Where(customer => _markedCustomerIds.Contains(customer.Id))
            .ToArray();
        if (selectedCustomers.Length < 2)
        {
            CustomersStatusText.Text = "Bitte mindestens zwei Kunden markieren.";
            PruneMarkedCustomerIds();
            RefreshCustomerRows();
            return;
        }

        var dialog = new CustomerMergeWindow(selectedCustomers)
        {
            Owner = this
        };
        if (dialog.ShowDialog() != true || dialog.MergeSelection is null)
        {
            return;
        }

        try
        {
            var selectedIds = selectedCustomers.Select(customer => customer.Id).ToArray();
            var preview = _customerMergeService.Merge(_customerRecords, selectedIds, dialog.MergeSelection);
            var historyCount = CountHistoryEntriesForCustomers(selectedCustomers);
            var confirmation = MessageBox.Show(
                this,
                CreateMergeConfirmationText(preview, historyCount),
                "Kunden zusammenführen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                CustomersStatusText.Text = "Kundenzusammenführung abgebrochen.";
                return;
            }

            _customerRepository.Save(_paths.CustomersFile, preview.Customers);
            _customerRecords = preview.Customers;
            foreach (var removedId in preview.RemovedCustomerIds)
            {
                _markedCustomerIds.Remove(removedId);
            }

            _markedCustomerIds.Remove(preview.TargetCustomer.Id);
            PruneMarkedCustomerIds();
            RefreshCustomerRows();
            CustomersStatusText.Text =
                $"{preview.MergedCustomerCount} Kunden wurden in \"{preview.TargetCustomer.CustomerName}\" zusammengeführt.";
        }
        catch (Exception ex)
        {
            ShowError($"Kunden konnten nicht zusammengeführt werden: {ex.Message}");
        }
    }

    private int CountHistoryEntriesForCustomers(IReadOnlyList<LicenseManagerCustomerRecord> customers)
    {
        var installationIds = customers
            .SelectMany(customer => customer.EffectiveInstallations)
            .Select(installation => installation.InstallationId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var customerNumbers = customers
            .Select(customer => customer.CustomerNumber)
            .Where(number => !string.IsNullOrWhiteSpace(number))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _historyRecords.Count(record =>
            installationIds.Contains(record.InstallationId)
            || (!string.IsNullOrWhiteSpace(record.CustomerNumber) && customerNumbers.Contains(record.CustomerNumber)));
    }

    private static string CreateMergeConfirmationText(LicenseManagerCustomerMergeResult preview, int historyCount)
    {
        return
            $"Zielkunde: {preview.TargetCustomer.CustomerName}{Environment.NewLine}" +
            $"Zusammenzuführende Kunden: {preview.MergedCustomerCount}{Environment.NewLine}" +
            $"Übernommene Installationen: {preview.InstallationCount}{Environment.NewLine}" +
            $"Übernommene aktive Anbindungen: {preview.ActiveLicensedDeviceCount}{Environment.NewLine}" +
            $"Übernommene Historieneinträge: {historyCount}{Environment.NewLine}{Environment.NewLine}" +
            "Die Quellkunden werden nach dem Zusammenführen aus der Kundenliste entfernt. Fortfahren?";
    }

    private void ExportCustomersPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Kundenliste als PDF exportieren",
            Filter = "PDF (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
            FileName = $"xdtbox-kunden-lizenzuebersicht-{DateTime.Today:yyyyMMdd}.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _customerPdfExporter.ExportCustomers(dialog.FileName, _customerRecords, _settings.PricePerDeviceNet, DateTime.Now);
            CustomersStatusText.Text = $"PDF exportiert: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            ShowError($"PDF konnte nicht exportiert werden: {ex.Message}");
        }
    }

    private void ExportSelectedCustomerPdf_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedCustomer();
        if (selected is null)
        {
            CustomersStatusText.Text = "Bitte zuerst einen Kunden auswählen.";
            return;
        }

        var now = DateTime.Now;
        var dialog = new SaveFileDialog
        {
            Title = "Kunden-PDF exportieren",
            Filter = "PDF (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
            FileName = LicenseManagerCustomerPdfExporter.CreateSuggestedCustomerPdfFileName(selected, now),
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _customerPdfExporter.ExportCustomer(dialog.FileName, selected, _historyRecords, _settings.PricePerDeviceNet, now);
            CustomersStatusText.Text = $"Kunden-PDF exportiert: {dialog.FileName}";
            MessageBox.Show(this, "Kunden-PDF wurde exportiert.", "PDF exportieren", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError($"Kunden-PDF konnte nicht exportiert werden: {ex.Message}");
        }
    }

    private void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "XDTBox Lizenzmanager-Sicherung erstellen",
            Filter = "XDTBox Lizenzmanager-Sicherung (*.xdtbox-licensemanager-backup)|*.xdtbox-licensemanager-backup|Alle Dateien (*.*)|*.*",
            InitialDirectory = Directory.Exists(_paths.BackupFolder) ? _paths.BackupFolder : _paths.BaseFolder,
            FileName = $"xdtbox-licensemanager-backup-{DateTime.Today:yyyyMMdd}.xdtbox-licensemanager-backup",
            DefaultExt = ".xdtbox-licensemanager-backup",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            _backupService.CreateBackup(dialog.FileName, _customerRecords, _settings, _historyRecords);
            BackupStatusText.Text = $"Sicherung erstellt: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            ShowError($"Sicherung konnte nicht erstellt werden: {ex.Message}");
        }
    }

    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "XDTBox Lizenzmanager-Sicherung wiederherstellen",
            Filter = "XDTBox Lizenzmanager-Sicherung (*.xdtbox-licensemanager-backup)|*.xdtbox-licensemanager-backup|Alle Dateien (*.*)|*.*",
            InitialDirectory = Directory.Exists(_paths.BackupFolder) ? _paths.BackupFolder : _paths.BaseFolder,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "Die vorhandenen Lizenzmanager-Daten werden durch die Sicherung ersetzt. Fortfahren?",
            "XDTBox Lizenzverwaltung",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            _backupService.RestoreBackup(dialog.FileName, _paths);
            _settings = _settingsRepository.LoadOrDefault(_paths.SettingsFile, _paths.BaseFolder);
            InitializeDefaults();
            LoadHistory();
            LoadCustomers();
            BackupStatusText.Text = "Sicherung wiederhergestellt.";
        }
        catch (Exception ex)
        {
            ShowError($"Sicherung konnte nicht wiederhergestellt werden: {ex.Message}");
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

        ShowHistoryDetails(GetSelectedHistoryRecord());
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
        ShowHistoryDetails(GetSelectedHistoryRecord());
    }

    private void ShowHistoryDetails(IssuedLicenseRecord? selected)
    {
        _historyDeviceRows.Clear();
        DeleteHistoryEntryButton.IsEnabled = selected is not null;

        if (selected is null)
        {
            SetHistoryDetailText("Keine Lizenz ausgewählt.");
            return;
        }

        HistoryDetailCustomerText.Text = selected.CustomerName;
        HistoryDetailCustomerNumberText.Text = selected.CustomerNumber ?? string.Empty;
        HistoryDetailPaymentText.Text = FormatPaymentMethod(selected.SepaDirectDebitConsent && !selected.AlwaysInvoice
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer);
        HistoryDetailAddressText.Text = selected.Street;
        HistoryDetailCityText.Text = $"{selected.PostalCode} {selected.City}".Trim();
        HistoryDetailContactText.Text = selected.ContactPerson ?? string.Empty;
        HistoryDetailPhoneText.Text = selected.Phone;
        HistoryDetailEmailText.Text = selected.Email ?? string.Empty;
        HistoryDetailInvoiceEmailText.Text = selected.InvoiceEmail ?? string.Empty;
        HistoryDetailInstallationText.Text = selected.InstallationId;
        HistoryDetailMachineText.Text = selected.MachineName ?? string.Empty;
        HistoryDetailDeviceCountText.Text = selected.MaxActiveDeviceConnections.ToString(CultureInfo.InvariantCulture);
        var licenseFile = string.IsNullOrWhiteSpace(selected.OutputFilePath)
            ? string.Empty
            : Path.GetFileName(selected.OutputFilePath);
        HistoryDetailLicenseText.Text = string.IsNullOrWhiteSpace(licenseFile)
            ? selected.LicenseId
            : $"{selected.LicenseId} / {licenseFile}";
        HistoryDetailIssuedText.Text = selected.IssuedAtUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        HistoryDetailValidUntilText.Text = FormatValidity(selected.ValidUntilUtc);

        foreach (var device in selected.Devices)
        {
            _historyDeviceRows.Add(device);
        }
    }

    private void SetHistoryDetailText(string value)
    {
        HistoryDetailCustomerText.Text = value;
        HistoryDetailCustomerNumberText.Text = string.Empty;
        HistoryDetailPaymentText.Text = string.Empty;
        HistoryDetailAddressText.Text = string.Empty;
        HistoryDetailCityText.Text = string.Empty;
        HistoryDetailContactText.Text = string.Empty;
        HistoryDetailPhoneText.Text = string.Empty;
        HistoryDetailEmailText.Text = string.Empty;
        HistoryDetailInvoiceEmailText.Text = string.Empty;
        HistoryDetailInstallationText.Text = string.Empty;
        HistoryDetailMachineText.Text = string.Empty;
        HistoryDetailDeviceCountText.Text = string.Empty;
        HistoryDetailLicenseText.Text = string.Empty;
        HistoryDetailIssuedText.Text = string.Empty;
        HistoryDetailValidUntilText.Text = string.Empty;
    }

    private void DeleteHistoryEntry_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedHistoryRecord();
        if (selected is null)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "Möchten Sie diesen Eintrag wirklich aus der lokalen Lizenzmanager-Historie entfernen?",
            "Eintrag entfernen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            HistoryStatusText.Text = "Entfernen abgebrochen.";
            return;
        }

        try
        {
            _historyRecords = _historyRepository.Remove(_paths.HistoryFile, selected);
            ReconcileCustomersAfterHistoryDeletion(selected);
            RefreshHistoryRows();
            ShowHistoryDetails(null);
            HistoryStatusText.Text = "Historieneintrag entfernt. Lizenzdateien, Private Keys und Kundenstammdaten wurden nicht gelöscht.";
        }
        catch (Exception ex)
        {
            ShowError($"Historieneintrag konnte nicht entfernt werden: {ex.Message}");
        }
    }

    private void ReconcileCustomersAfterHistoryDeletion(IssuedLicenseRecord deletedRecord)
    {
        var customers = _customerRecords.ToList();
        var changed = false;

        for (var i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            var installations = customer.EffectiveInstallations.ToList();
            var installationIndex = installations.FindIndex(installation =>
                string.Equals(installation.InstallationId, deletedRecord.InstallationId, StringComparison.OrdinalIgnoreCase));
            if (installationIndex < 0)
            {
                continue;
            }

            var remainingLatestLicense = _historyRecords
                .Where(record => string.Equals(record.InstallationId, deletedRecord.InstallationId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.IssuedAtUtc)
                .FirstOrDefault();

            if (remainingLatestLicense is null)
            {
                installations.RemoveAt(installationIndex);
            }
            else
            {
                installations[installationIndex] = LicenseManagerInstallationRecord.FromLicense(remainingLatestLicense);
            }

            customers[i] = (customer with
            {
                Installations = installations.ToArray(),
                UpdatedAtUtc = DateTime.UtcNow
            }).WithNormalizedInstallations();
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        _customerRepository.Save(_paths.CustomersFile, customers);
        _customerRecords = customers;
        RefreshCustomerRows();
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
        _currentRequestUsesSeparateCustomerRecord = false;
        RequestFileTextBox.Text = string.Empty;
        InstallationIdTextBox.Text = includeInstallation ? selected.InstallationId : string.Empty;
        CustomerNameTextBox.Text = selected.CustomerName;
        StreetTextBox.Text = selected.Street;
        PostalCodeTextBox.Text = selected.PostalCode;
        CityTextBox.Text = selected.City;
        PhoneTextBox.Text = selected.Phone;
        EmailTextBox.Text = selected.Email ?? string.Empty;
        ContactPersonTextBox.Text = selected.ContactPerson ?? string.Empty;
        InvoiceEmailTextBox.Text = selected.InvoiceEmail ?? string.Empty;
        CustomerIbanTextBox.Text = selected.Iban ?? string.Empty;
        CustomerBicTextBox.Text = selected.Bic ?? string.Empty;
        CustomerAccountHolderTextBox.Text = selected.AccountHolder ?? string.Empty;
        SetPaymentMethod(selected.SepaDirectDebitConsent && !selected.AlwaysInvoice
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer);
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
            CustomerPricePerDeviceTextBox.Text = _settings.PricePerDeviceNet.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));
            RefreshCustomerRows();
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
            Directory.CreateDirectory(_paths.BackupFolder);
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

        var pricePerDeviceNet = ParsePrice(SettingsPricePerDeviceTextBox.Text);

        return new LicenseManagerSettings(
            DefaultOutputFolder: SettingsOutputFolderTextBox.Text.Trim(),
            DefaultRequestFolder: SettingsRequestFolderTextBox.Text.Trim(),
            DefaultKeyFolder: SettingsKeyFolderTextBox.Text.Trim(),
            PrivateKeyPath: NormalizeOptional(SettingsPrivateKeyPathTextBox.Text),
            KeyId: SettingsKeyIdTextBox.Text.Trim(),
            DefaultIssuer: SettingsIssuerTextBox.Text.Trim(),
            DefaultGraceDays: graceDays,
            PricePerDeviceNet: pricePerDeviceNet);
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

    private LicenseManagerCustomerRecord? GetSelectedCustomer()
    {
        return (CustomersGrid.SelectedItem as CustomerRow)?.Customer;
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

    private void PaymentMethodCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingPaymentMethod)
        {
            return;
        }

        if (ReferenceEquals(sender, SepaConsentCheckBox))
        {
            SetPaymentMethod(LicenseManagerPaymentMethod.SepaDirectDebit);
            return;
        }

        SetPaymentMethod(LicenseManagerPaymentMethod.BankTransfer);
    }

    private void PaymentMethodCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_updatingPaymentMethod)
        {
            return;
        }

        if (SepaConsentCheckBox.IsChecked != true && AlwaysInvoiceCheckBox.IsChecked != true)
        {
            SetPaymentMethod(LicenseManagerPaymentMethod.BankTransfer);
        }
    }

    private void SetPaymentMethod(LicenseManagerPaymentMethod method)
    {
        _updatingPaymentMethod = true;
        try
        {
            SepaConsentCheckBox.IsChecked = method == LicenseManagerPaymentMethod.SepaDirectDebit;
            AlwaysInvoiceCheckBox.IsChecked = method == LicenseManagerPaymentMethod.BankTransfer;
        }
        finally
        {
            _updatingPaymentMethod = false;
        }
    }

    private LicenseManagerPaymentMethod GetSelectedPaymentMethod()
    {
        return SepaConsentCheckBox.IsChecked == true
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer;
    }

    private static void ValidatePaymentMethod(LicenseManagerPaymentMethod paymentMethod, string? iban, string? accountHolder)
    {
        if (paymentMethod != LicenseManagerPaymentMethod.SepaDirectDebit)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(iban) || string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new InvalidOperationException("SEPA-Lastschrift benötigt IBAN und Kontoinhaber.");
        }
    }

    private static string FormatPaymentMethod(LicenseManagerPaymentMethod paymentMethod)
    {
        return paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit
            ? "SEPA-Lastschrift"
            : "Banküberweisung";
    }

    private static string GetApplicationVersionText()
    {
        var assembly = typeof(MainWindow).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return string.IsNullOrWhiteSpace(informationalVersion)
            ? assembly.GetName().Version?.ToString() ?? "unbekannt"
            : informationalVersion;
    }

    private static string FormatValidity(DateTime? validUntilUtc)
    {
        return XdtBoxLicenseConstants.IsUnlimitedValidUntil(validUntilUtc)
            ? "unbefristet"
            : validUntilUtc?.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static string? NormalizeOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal ParsePrice(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        var culture = CultureInfo.GetCultureInfo("de-DE");
        if (decimal.TryParse(value.Trim(), NumberStyles.Number, culture, out var price)
            || decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out price))
        {
            if (price < 0)
            {
                throw new InvalidOperationException("Preis darf nicht negativ sein.");
            }

            return price;
        }

        throw new InvalidOperationException("Preis pro Geräteanbindung ist keine gültige Zahl.");
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

    private void CommitRequestDeviceEdits()
    {
        RequestDevicesGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        RequestDevicesGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
    }

    private sealed class RequestDeviceRow
    {
        public RequestDeviceRow(
            int index,
            string displayName,
            string deviceDisplayName,
            string interfaceProfileId,
            string deviceProfileId,
            DeviceConnectionKind connectionKind,
            bool isActive,
            bool isLicenseRequired,
            string? location)
        {
            Index = index;
            DisplayName = displayName;
            DeviceDisplayName = deviceDisplayName;
            InterfaceProfileId = interfaceProfileId;
            DeviceProfileId = deviceProfileId;
            ConnectionKind = connectionKind;
            IsActive = isActive;
            IsLicenseRequired = isLicenseRequired;
            Location = location ?? string.Empty;
        }

        public int Index { get; }
        public string DisplayName { get; }
        public string DeviceDisplayName { get; }
        public string InterfaceProfileId { get; }
        public string DeviceProfileId { get; }
        public DeviceConnectionKind ConnectionKind { get; }
        public bool IsActive { get; }
        public bool IsLicenseRequired { get; }
        public string Location { get; set; }

        public static RequestDeviceRow FromRequestDevice(int index, LicenseRequestDevice device)
        {
            return new RequestDeviceRow(
                index,
                string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                string.IsNullOrWhiteSpace(device.InterfaceProfileId) ? device.ProfileId : device.InterfaceProfileId,
                device.DeviceProfileId,
                device.ConnectionKind,
                device.IsActive,
                device.IsLicenseRequired,
                device.Location);
        }

        public static RequestDeviceRow FromIssuedDevice(int index, IssuedLicenseDeviceRecord device)
        {
            return new RequestDeviceRow(
                index,
                device.DisplayName,
                device.DeviceDisplayName,
                device.InterfaceProfileId,
                device.DeviceProfileId,
                device.ConnectionKind,
                true,
                true,
                device.Location);
        }

        public IssuedLicenseDeviceRecord ToIssuedLicenseDeviceRecord()
        {
            return new IssuedLicenseDeviceRecord(
                DisplayName,
                DeviceDisplayName,
                InterfaceProfileId,
                DeviceProfileId,
                ConnectionKind,
                NormalizeOptional(Location));
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
        public string ValidUntilDisplay => FormatValidity(Record.ValidUntilUtc);
        public string LicenseType => Record.LicenseType;
        public string IssuedAtDisplay => Record.IssuedAtUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
    }

    private sealed class CustomerRow : INotifyPropertyChanged
    {
        private readonly decimal _pricePerDeviceNet;
        private readonly Action<string, bool> _markChanged;
        private bool _isMarked;

        public CustomerRow(
            LicenseManagerCustomerRecord customer,
            decimal pricePerDeviceNet,
            bool isMarked,
            Action<string, bool> markChanged)
        {
            Customer = customer;
            _pricePerDeviceNet = pricePerDeviceNet;
            _isMarked = isMarked;
            _markChanged = markChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public LicenseManagerCustomerRecord Customer { get; }
        public bool IsMarked
        {
            get => _isMarked;
            set
            {
                if (_isMarked == value)
                {
                    return;
                }

                _isMarked = value;
                _markChanged(Customer.Id, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMarked)));
            }
        }

        public string CustomerNumber => Customer.CustomerNumber ?? string.Empty;
        public string CustomerName => Customer.CustomerName;
        public string ContactPerson => Customer.ContactPerson ?? string.Empty;
        public string InvoiceEmail => Customer.InvoiceEmail ?? Customer.Email ?? string.Empty;
        public string PaymentMethodDisplay => FormatPaymentMethod(Customer.PaymentMethod);
        public string Iban => Customer.Iban ?? string.Empty;
        public string Bic => Customer.Bic ?? string.Empty;
        public string AccountHolder => Customer.AccountHolder ?? string.Empty;
        public int ActiveInstallationCount => Customer.ActiveInstallationCount;
        public int ActiveLicensedDeviceCount => Customer.BillableDeviceCount;
        public string PricePerDeviceNetDisplay => _pricePerDeviceNet.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR";
        public string TotalNetDisplay => LicenseManagerCostCalculator
            .CalculateNetTotal(Customer.BillableDeviceCount, _pricePerDeviceNet)
            .ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR";
        public string LastLicenseIssuedDisplay => Customer.EffectiveLastLicenseIssuedAtUtc?.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture) ?? string.Empty;
        public string LicenseValidUntilDisplay => FormatValidity(Customer.EffectiveLicenseValidUntilUtc);
        public string InstallationIdsDisplay => string.Join("; ", Customer.EffectiveInstallations.Select(FormatInstallation));

        private static string FormatInstallation(LicenseManagerInstallationRecord installation)
        {
            return installation.IsActive ? installation.InstallationId : installation.InstallationId + " (storniert)";
        }
    }
}
