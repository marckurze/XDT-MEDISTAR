namespace XdtDeviceBridge.Core;

public sealed record LicensePayload(
    string LicenseId,
    string ProductCode,
    string LicenseeName,
    string? CustomerNumber,
    string InstallationId,
    int MaxActiveDeviceConnections,
    DateTime ValidFromUtc,
    DateTime ValidUntilUtc,
    int GraceDays,
    DateTime IssuedAtUtc,
    string Issuer,
    string LicenseType,
    string? Notes)
{
    public IReadOnlyList<string> Validate()
    {
        return LicensePayloadValidator.Validate(this);
    }
}
