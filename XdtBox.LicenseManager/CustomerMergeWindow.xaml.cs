using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseManager;

public partial class CustomerMergeWindow : Window
{
    private readonly IReadOnlyList<LicenseManagerCustomerRecord> _customers;
    private readonly ObservableCollection<CustomerChoice> _targetChoices = new();
    private readonly ObservableCollection<MergeFieldRow> _fieldRows = new();

    public CustomerMergeWindow(IReadOnlyList<LicenseManagerCustomerRecord> customers)
    {
        InitializeComponent();

        _customers = (customers ?? throw new ArgumentNullException(nameof(customers)))
            .Select(customer => customer.WithNormalizedInstallations())
            .ToArray();
        if (_customers.Count < 2)
        {
            throw new ArgumentException("Mindestens zwei Kunden sind fuer die Zusammenfuehrung erforderlich.", nameof(customers));
        }

        TargetCustomerComboBox.ItemsSource = _targetChoices;
        MergeFieldsGrid.ItemsSource = _fieldRows;

        foreach (var customer in _customers)
        {
            _targetChoices.Add(new CustomerChoice(customer));
        }

        TargetCustomerComboBox.SelectedIndex = 0;
        RefreshFieldRows();
    }

    public LicenseManagerCustomerMergeSelection? MergeSelection { get; private set; }

    private void TargetCustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshFieldRows();
    }

    private void RefreshFieldRows()
    {
        if (TargetCustomerComboBox.SelectedItem is not CustomerChoice targetChoice)
        {
            return;
        }

        var target = targetChoice.Customer;
        _fieldRows.Clear();
        _fieldRows.Add(CreateTextRow("CustomerNumber", "Kundennummer", customer => customer.CustomerNumber, target.CustomerNumber));
        _fieldRows.Add(CreateTextRow("CustomerName", "Kunde / Praxis / Firma", customer => customer.CustomerName, target.CustomerName));
        _fieldRows.Add(CreateTextRow("ContactPerson", "Ansprechpartner", customer => customer.ContactPerson, target.ContactPerson));
        _fieldRows.Add(CreateTextRow("Street", "Straße / Anschrift", customer => customer.Street, target.Street));
        _fieldRows.Add(CreateTextRow("PostalCode", "PLZ", customer => customer.PostalCode, target.PostalCode));
        _fieldRows.Add(CreateTextRow("City", "Ort", customer => customer.City, target.City));
        _fieldRows.Add(CreateTextRow("Phone", "Telefon", customer => customer.Phone, target.Phone));
        _fieldRows.Add(CreateTextRow("Email", "E-Mail", customer => customer.Email, target.Email));
        _fieldRows.Add(CreateTextRow("InvoiceEmail", "Rechnungs-E-Mail", customer => customer.InvoiceEmail, target.InvoiceEmail));
        _fieldRows.Add(CreatePaymentRow(target.PaymentMethod));
        _fieldRows.Add(CreateTextRow("Iban", "IBAN", customer => customer.Iban, target.Iban));
        _fieldRows.Add(CreateTextRow("Bic", "BIC", customer => customer.Bic, target.Bic));
        _fieldRows.Add(CreateTextRow("AccountHolder", "Kontoinhaber", customer => customer.AccountHolder, target.AccountHolder));

        var installationCount = _customers.SelectMany(customer => customer.EffectiveInstallations)
            .Select(installation => installation.InstallationId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var activeDeviceCount = _customers.Sum(customer => customer.BillableDeviceCount);
        MergeSummaryTextBlock.Text =
            $"{_customers.Count.ToString(CultureInfo.CurrentCulture)} Kunden markiert. " +
            $"{installationCount.ToString(CultureInfo.CurrentCulture)} Installation(en) und " +
            $"{activeDeviceCount.ToString(CultureInfo.CurrentCulture)} aktive lizenzierte Anbindung(en) werden in den Zielkunden uebernommen.";
    }

    private MergeFieldRow CreateTextRow(
        string key,
        string label,
        Func<LicenseManagerCustomerRecord, string?> selector,
        string? targetValue)
    {
        var options = CreateValueOptions(selector).ToArray();
        var selected = options.FirstOrDefault(option => string.Equals(option.Value, Normalize(targetValue), StringComparison.Ordinal))
            ?? options.First();
        return new MergeFieldRow(key, label, options, selected);
    }

    private MergeFieldRow CreatePaymentRow(LicenseManagerPaymentMethod targetValue)
    {
        var options = _customers
            .Select(customer => new MergeValueOption(
                Value: customer.PaymentMethod.ToString(),
                DisplayText: FormatPaymentMethod(customer.PaymentMethod),
                PaymentMethod: customer.PaymentMethod))
            .GroupBy(option => option.Value, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var selected = options.FirstOrDefault(option => option.PaymentMethod == targetValue)
            ?? options.First();
        return new MergeFieldRow("PaymentMethod", "Zahlungsart", options, selected);
    }

    private IEnumerable<MergeValueOption> CreateValueOptions(Func<LicenseManagerCustomerRecord, string?> selector)
    {
        return _customers
            .Select(customer => Normalize(selector(customer)))
            .Distinct(StringComparer.Ordinal)
            .Select(value => new MergeValueOption(value, string.IsNullOrWhiteSpace(value) ? "(leer)" : value, null));
    }

    private void Merge_Click(object sender, RoutedEventArgs e)
    {
        if (TargetCustomerComboBox.SelectedItem is not CustomerChoice targetChoice)
        {
            MessageBox.Show(this, "Bitte einen Zielkunden auswählen.", "Kunden zusammenführen", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var customerName = GetValue("CustomerName");
        if (string.IsNullOrWhiteSpace(customerName))
        {
            MessageBox.Show(this, "Bitte einen Kundennamen auswählen.", "Kunden zusammenführen", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MergeSelection = new LicenseManagerCustomerMergeSelection(
            TargetCustomerId: targetChoice.Customer.Id,
            CustomerNumber: GetValue("CustomerNumber"),
            CustomerName: customerName,
            ContactPerson: GetValue("ContactPerson"),
            Street: GetValue("Street") ?? string.Empty,
            PostalCode: GetValue("PostalCode") ?? string.Empty,
            City: GetValue("City") ?? string.Empty,
            Phone: GetValue("Phone") ?? string.Empty,
            Email: GetValue("Email"),
            InvoiceEmail: GetValue("InvoiceEmail"),
            PaymentMethod: GetPaymentMethod(),
            Iban: GetValue("Iban"),
            Bic: GetValue("Bic"),
            AccountHolder: GetValue("AccountHolder"));

        DialogResult = true;
        Close();
    }

    private string? GetValue(string key)
    {
        return _fieldRows.First(row => row.Key == key).SelectedOption.Value;
    }

    private LicenseManagerPaymentMethod GetPaymentMethod()
    {
        return _fieldRows.First(row => row.Key == "PaymentMethod").SelectedOption.PaymentMethod
            ?? LicenseManagerPaymentMethod.BankTransfer;
    }

    private static string FormatPaymentMethod(LicenseManagerPaymentMethod paymentMethod)
    {
        return paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit
            ? "SEPA-Lastschrift"
            : "Banküberweisung";
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed class CustomerChoice
    {
        public CustomerChoice(LicenseManagerCustomerRecord customer)
        {
            Customer = customer;
        }

        public LicenseManagerCustomerRecord Customer { get; }

        public string DisplayName
        {
            get
            {
                var number = string.IsNullOrWhiteSpace(Customer.CustomerNumber)
                    ? string.Empty
                    : Customer.CustomerNumber + " - ";
                return number + Customer.CustomerName;
            }
        }
    }

    private sealed class MergeFieldRow
    {
        public MergeFieldRow(
            string key,
            string label,
            IReadOnlyList<MergeValueOption> options,
            MergeValueOption selectedOption)
        {
            Key = key;
            Label = label;
            Options = options;
            SelectedOption = selectedOption;
        }

        public string Key { get; }

        public string Label { get; }

        public IReadOnlyList<MergeValueOption> Options { get; }

        public MergeValueOption SelectedOption { get; set; }
    }

    private sealed record MergeValueOption(
        string? Value,
        string DisplayText,
        LicenseManagerPaymentMethod? PaymentMethod);
}
