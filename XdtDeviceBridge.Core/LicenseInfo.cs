namespace XdtDeviceBridge.Core;

public sealed record LicenseInfo(
    string LicenseId,
    string CustomerName,
    string CustomerNumber,
    string InstallationId,
    int LicensedDeviceCount,
    DateTime ValidFrom,
    DateTime ValidUntil,
    LicenseType LicenseType,
    string ProductCode,
    string MinimumAppVersion,
    DateTime IssuedAt,
    string Signature)
{
    public IReadOnlyList<string> Validate()
    {
        return LicenseInfoValidator.Validate(this);
    }
}
