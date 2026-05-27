namespace XdtDeviceBridge.Core;

public sealed record LicenseRequest(
    string RequestId,
    string InstallationId,
    string MachineName,
    string UserName,
    bool IsTerminalServer,
    string ProductCode,
    string AppVersion,
    int ActiveLicensedDeviceCount,
    IReadOnlyList<LicenseRequestDevice> Devices,
    DateTime CreatedAt,
    LicenseRequestCustomer? Customer = null)
{
    public IReadOnlyList<string> Validate()
    {
        return LicenseRequestValidator.Validate(this);
    }
}
