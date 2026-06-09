using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseManager;

public partial class CustomerDetailWindow : Window
{
    private readonly IReadOnlyList<IssuedLicenseRecord> _allHistory;
    private readonly decimal _pricePerDeviceNet;
    private readonly ObservableCollection<InstallationRow> _installationRows = new();
    private readonly ObservableCollection<CustomerDeviceRow> _deviceRows = new();
    private readonly ObservableCollection<CustomerHistoryRow> _historyRows = new();
    private bool _updatingPaymentMethod;

    public CustomerDetailWindow(
        LicenseManagerCustomerRecord customer,
        IReadOnlyList<IssuedLicenseRecord> history,
        decimal pricePerDeviceNet)
    {
        InitializeComponent();

        Customer = (customer ?? throw new ArgumentNullException(nameof(customer))).WithNormalizedInstallations();
        _allHistory = history ?? Array.Empty<IssuedLicenseRecord>();
        _pricePerDeviceNet = pricePerDeviceNet;

        InstallationsGrid.ItemsSource = _installationRows;
        DevicesGrid.ItemsSource = _deviceRows;
        HistoryGrid.ItemsSource = _historyRows;

        ShowCustomer(Customer);
    }

    public LicenseManagerCustomerRecord Customer { get; private set; }

    private void ShowCustomer(LicenseManagerCustomerRecord customer)
    {
        CustomerNumberTextBox.Text = customer.CustomerNumber ?? string.Empty;
        CustomerNameTextBox.Text = customer.CustomerName;
        StreetTextBox.Text = customer.Street;
        PostalCodeTextBox.Text = customer.PostalCode;
        CityTextBox.Text = customer.City;
        PhoneTextBox.Text = customer.Phone;
        EmailTextBox.Text = customer.Email ?? string.Empty;
        ContactPersonTextBox.Text = customer.ContactPerson ?? string.Empty;
        InvoiceEmailTextBox.Text = customer.InvoiceEmail ?? string.Empty;
        IbanTextBox.Text = customer.Iban ?? string.Empty;
        BicTextBox.Text = customer.Bic ?? string.Empty;
        AccountHolderTextBox.Text = customer.AccountHolder ?? string.Empty;
        SetPaymentMethod(customer.PaymentMethod);

        RefreshInstallations(customer);
        RefreshDevices(customer);
        RefreshHistory(customer);
        UpdateSummary(customer);
    }

    private void RefreshInstallations(LicenseManagerCustomerRecord customer)
    {
        _installationRows.Clear();
        foreach (var installation in customer.EffectiveInstallations
                     .OrderByDescending(installation => installation.IsActive)
                     .ThenByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue))
        {
            _installationRows.Add(new InstallationRow(installation));
        }

        InstallationsGrid.SelectedIndex = _installationRows.Count > 0 ? 0 : -1;
        UpdateCancelButtonState();
    }

    private void RefreshDevices(LicenseManagerCustomerRecord customer)
    {
        _deviceRows.Clear();
        foreach (var device in customer.EffectiveDevices)
        {
            _deviceRows.Add(CustomerDeviceRow.FromDevice(device));
        }
    }

    private void RefreshHistory(LicenseManagerCustomerRecord customer)
    {
        var installationIds = customer.EffectiveInstallations
            .Select(installation => installation.InstallationId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _historyRows.Clear();
        foreach (var record in _allHistory
                     .Where(record => installationIds.Contains(record.InstallationId)
                         || (!string.IsNullOrWhiteSpace(customer.CustomerNumber)
                             && string.Equals(record.CustomerNumber, customer.CustomerNumber, StringComparison.OrdinalIgnoreCase)))
                     .OrderByDescending(record => record.IssuedAtUtc))
        {
            _historyRows.Add(new CustomerHistoryRow(record));
        }
    }

    private void UpdateSummary(LicenseManagerCustomerRecord customer)
    {
        var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, _pricePerDeviceNet);
        SummaryTextBlock.Text =
            $"{customer.ActiveInstallationCount} aktive Installation(en), {customer.BillableDeviceCount} lizenzierte Geräteanbindung(en), netto {total.ToString("N2", CultureInfo.GetCultureInfo("de-DE"))} EUR.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var paymentMethod = GetSelectedPaymentMethod();
        ValidatePaymentMethod(paymentMethod, Normalize(IbanTextBox.Text), Normalize(AccountHolderTextBox.Text));
        CommitDeviceEdits();

        var installations = ApplyEditedDeviceLocations(Customer.EffectiveInstallations);

        Customer = (Customer with
        {
            CustomerNumber = Normalize(CustomerNumberTextBox.Text),
            CustomerName = CustomerNameTextBox.Text.Trim(),
            Street = StreetTextBox.Text.Trim(),
            PostalCode = PostalCodeTextBox.Text.Trim(),
            City = CityTextBox.Text.Trim(),
            Phone = PhoneTextBox.Text.Trim(),
            Email = Normalize(EmailTextBox.Text),
            ContactPerson = Normalize(ContactPersonTextBox.Text),
            InvoiceEmail = Normalize(InvoiceEmailTextBox.Text),
            Iban = Normalize(IbanTextBox.Text),
            Bic = Normalize(BicTextBox.Text),
            AccountHolder = Normalize(AccountHolderTextBox.Text),
            SepaDirectDebitConsent = paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit,
            AlwaysInvoice = paymentMethod == LicenseManagerPaymentMethod.BankTransfer,
            Installations = installations,
            UpdatedAtUtc = DateTime.UtcNow
        }).WithNormalizedInstallations();

        DialogResult = true;
        Close();
    }

    private void CancelInstallation_Click(object sender, RoutedEventArgs e)
    {
        if (InstallationsGrid.SelectedItem is not InstallationRow row)
        {
            return;
        }

        if (!row.IsActive)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "Diese Installation wird in der lokalen Lizenzmanager-Übersicht als storniert markiert. Fortfahren?",
            "Lizenz als storniert markieren",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        Customer = Customer.CancelInstallation(
            row.InstallationId,
            DateTime.UtcNow,
            "Manuell im XDTBox Lizenzmanager storniert");

        ShowCustomer(Customer);
    }

    private void InstallationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCancelButtonState();
    }

    private void UpdateCancelButtonState()
    {
        CancelInstallationButton.IsEnabled = InstallationsGrid.SelectedItem is InstallationRow { IsActive: true };
    }

    private void PaymentMethodCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_updatingPaymentMethod)
        {
            return;
        }

        SetPaymentMethod(ReferenceEquals(sender, SepaCheckBox)
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer);
    }

    private void PaymentMethodCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_updatingPaymentMethod)
        {
            return;
        }

        if (SepaCheckBox.IsChecked != true && AlwaysInvoiceCheckBox.IsChecked != true)
        {
            SetPaymentMethod(LicenseManagerPaymentMethod.BankTransfer);
        }
    }

    private void SetPaymentMethod(LicenseManagerPaymentMethod method)
    {
        _updatingPaymentMethod = true;
        try
        {
            SepaCheckBox.IsChecked = method == LicenseManagerPaymentMethod.SepaDirectDebit;
            AlwaysInvoiceCheckBox.IsChecked = method == LicenseManagerPaymentMethod.BankTransfer;
        }
        finally
        {
            _updatingPaymentMethod = false;
        }
    }

    private LicenseManagerPaymentMethod GetSelectedPaymentMethod()
    {
        return SepaCheckBox.IsChecked == true
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

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void CommitDeviceEdits()
    {
        DevicesGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        DevicesGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
    }

    private IReadOnlyList<LicenseManagerInstallationRecord> ApplyEditedDeviceLocations(
        IReadOnlyList<LicenseManagerInstallationRecord> installations)
    {
        var locationsByKey = _deviceRows
            .GroupBy(CreateDeviceKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => Normalize(group.Last().Location),
                StringComparer.OrdinalIgnoreCase);

        return installations
            .Select(installation => installation with
            {
                Devices = installation.Devices
                    .Select(device =>
                    {
                        var key = CreateDeviceKey(device);
                        return locationsByKey.TryGetValue(key, out var location)
                            ? device with { Location = location }
                            : device;
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static string CreateDeviceKey(CustomerDeviceRow row)
    {
        return CreateDeviceKey(row.InterfaceProfileId, row.DisplayName, row.DeviceDisplayName);
    }

    private static string CreateDeviceKey(IssuedLicenseDeviceRecord device)
    {
        return CreateDeviceKey(device.InterfaceProfileId, device.DisplayName, device.DeviceDisplayName);
    }

    private static string CreateDeviceKey(string interfaceProfileId, string displayName, string deviceDisplayName)
    {
        return !string.IsNullOrWhiteSpace(interfaceProfileId)
            ? interfaceProfileId
            : $"{displayName}|{deviceDisplayName}";
    }

    private static string FormatValidity(DateTime? validUntilUtc)
    {
        return XdtBoxLicenseConstants.IsUnlimitedValidUntil(validUntilUtc)
            ? "unbefristet"
            : validUntilUtc?.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private sealed class InstallationRow
    {
        public InstallationRow(LicenseManagerInstallationRecord installation)
        {
            Installation = installation;
        }

        public LicenseManagerInstallationRecord Installation { get; }
        public string InstallationId => Installation.InstallationId;
        public bool IsActive => Installation.IsActive;
        public int BillableDeviceCount => Installation.BillableDeviceCount;
        public string StatusDisplay => Installation.IsActive ? "Aktiv" : "Storniert";
        public string ValidUntilDisplay => FormatValidity(Installation.LicenseValidUntilUtc);
    }

    private sealed class CustomerHistoryRow
    {
        private readonly IssuedLicenseRecord _record;

        public CustomerHistoryRow(IssuedLicenseRecord record)
        {
            _record = record;
        }

        public string IssuedAtDisplay => _record.IssuedAtUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        public string InstallationId => _record.InstallationId;
        public string ValidUntilDisplay => FormatValidity(_record.ValidUntilUtc);
        public int DeviceCount => _record.MaxActiveDeviceConnections;
        public string FileName => Path.GetFileName(_record.OutputFilePath);
    }

    private sealed class CustomerDeviceRow
    {
        private CustomerDeviceRow(IssuedLicenseDeviceRecord device)
        {
            DisplayName = device.DisplayName;
            DeviceDisplayName = device.DeviceDisplayName;
            InterfaceProfileId = device.InterfaceProfileId;
            ConnectionKind = device.ConnectionKind;
            Location = device.Location ?? string.Empty;
        }

        public string DisplayName { get; }
        public string DeviceDisplayName { get; }
        public string InterfaceProfileId { get; }
        public DeviceConnectionKind ConnectionKind { get; }
        public string Location { get; set; }

        public static CustomerDeviceRow FromDevice(IssuedLicenseDeviceRecord device)
        {
            return new CustomerDeviceRow(device);
        }
    }
}
