namespace XdtDeviceBridge.Core;

public static class LicenseInfoValidator
{
    public static IReadOnlyList<string> Validate(LicenseInfo licenseInfo)
    {
        ArgumentNullException.ThrowIfNull(licenseInfo);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(licenseInfo.LicenseId))
        {
            issues.Add("LicenseId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(licenseInfo.InstallationId))
        {
            issues.Add("InstallationId must not be empty.");
        }

        if (licenseInfo.LicensedDeviceCount < 0)
        {
            issues.Add("LicensedDeviceCount must not be negative.");
        }

        if (licenseInfo.ValidUntil < licenseInfo.ValidFrom)
        {
            issues.Add("ValidUntil must not be before ValidFrom.");
        }

        if (string.IsNullOrWhiteSpace(licenseInfo.ProductCode))
        {
            issues.Add("ProductCode must not be empty.");
        }

        if (licenseInfo.LicenseType != LicenseType.Trial && string.IsNullOrWhiteSpace(licenseInfo.Signature))
        {
            issues.Add("Signature must not be empty for productive licenses.");
        }

        return issues;
    }
}
