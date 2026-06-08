using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseManager;

public partial class CustomerDetailWindow : Window
{
    private readonly LicenseManagerCustomerRecord _original;
    private readonly decimal _pricePerDeviceNet;
    private readonly ObservableCollection<IssuedLicenseDeviceRecord> _deviceRows = new();
    private readonly ObservableCollection<CustomerHistoryRow> _historyRows = new();

    public CustomerDetailWindow(
        LicenseManagerCustomerRecord customer,
        IReadOnlyList<IssuedLicenseRecord> history,
        decimal pricePerDeviceNet)
    {
        InitializeComponent();

        _original = customer ?? throw new ArgumentNullException(nameof(customer));
        _pricePerDeviceNet = pricePerDeviceNet;
        Customer = customer;
        DevicesGrid.ItemsSource = _deviceRows;
        HistoryGrid.ItemsSource = _historyRows;

        ShowCustomer(customer);
        ShowHistory(history);
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
        SepaCheckBox.IsChecked = customer.SepaDirectDebitConsent;
        AlwaysInvoiceCheckBox.IsChecked = customer.AlwaysInvoice;
        InstallationIdTextBox.Text = customer.InstallationId;

        _deviceRows.Clear();
        foreach (var device in customer.Devices)
        {
            _deviceRows.Add(device);
        }

        var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.ActiveLicensedDeviceCount, _pricePerDeviceNet);
        SummaryTextBlock.Text = $"{customer.ActiveLicensedDeviceCount} lizenzierte Geraeteanbindung(en), monatlich netto {total.ToString("N2", CultureInfo.GetCultureInfo("de-DE"))} EUR.";
    }

    private void ShowHistory(IReadOnlyList<IssuedLicenseRecord> history)
    {
        _historyRows.Clear();
        foreach (var record in history
            .Where(record => string.Equals(record.InstallationId, _original.InstallationId, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(_original.CustomerNumber)
                    && string.Equals(record.CustomerNumber, _original.CustomerNumber, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(record => record.IssuedAtUtc))
        {
            _historyRows.Add(new CustomerHistoryRow(record));
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Customer = _original with
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
            SepaDirectDebitConsent = SepaCheckBox.IsChecked == true,
            AlwaysInvoice = AlwaysInvoiceCheckBox.IsChecked == true,
            UpdatedAtUtc = DateTime.UtcNow
        };
        DialogResult = true;
        Close();
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

    private sealed class CustomerHistoryRow
    {
        private readonly IssuedLicenseRecord _record;

        public CustomerHistoryRow(IssuedLicenseRecord record)
        {
            _record = record;
        }

        public string IssuedAtDisplay => _record.IssuedAtUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        public string ValidUntilDisplay => _record.ValidUntilUtc.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        public int DeviceCount => _record.MaxActiveDeviceConnections;
        public string FileName => Path.GetFileName(_record.OutputFilePath);
    }
}
