namespace XdtDeviceBridge.Core;

public sealed record LicenseRequestCustomer(
    string CustomerName,
    string Street,
    string PostalCode,
    string City,
    string Phone,
    string? Email = null,
    string? ContactPerson = null)
{
    public static LicenseRequestCustomer Empty { get; } = new(
        CustomerName: string.Empty,
        Street: string.Empty,
        PostalCode: string.Empty,
        City: string.Empty,
        Phone: string.Empty);
}
