namespace XdtDeviceBridge.Core;

public sealed record LicenseManagerCustomerMergeSelection(
    string TargetCustomerId,
    string? CustomerNumber,
    string CustomerName,
    string? ContactPerson,
    string Street,
    string PostalCode,
    string City,
    string Phone,
    string? Email,
    string? InvoiceEmail,
    LicenseManagerPaymentMethod PaymentMethod,
    string? Iban,
    string? Bic,
    string? AccountHolder);
