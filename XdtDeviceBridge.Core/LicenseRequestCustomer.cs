namespace XdtDeviceBridge.Core;

public sealed record LicenseRequestCustomer(
    string CustomerName,
    string Street,
    string PostalCode,
    string City,
    string Phone,
    string? Email = null,
    string? ContactPerson = null,
    string? Iban = null,
    string? Bic = null,
    string? AccountHolder = null,
    bool SepaDirectDebitConsent = false,
    bool AlwaysInvoice = false,
    string? InvoiceEmail = null)
{
    public static LicenseRequestCustomer Empty { get; } = new(
        CustomerName: string.Empty,
        Street: string.Empty,
        PostalCode: string.Empty,
        City: string.Empty,
        Phone: string.Empty);
}
