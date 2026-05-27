namespace XdtDeviceBridge.Core;

public static class LicensePayloadValidator
{
    public static IReadOnlyList<string> Validate(LicensePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(payload.LicenseId))
        {
            issues.Add("LicenseId must not be empty.");
        }

        if (!string.Equals(payload.ProductCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
        {
            issues.Add($"ProductCode must be {XdtBoxLicenseConstants.ProductCode}.");
        }

        if (string.IsNullOrWhiteSpace(payload.LicenseeName))
        {
            issues.Add("LicenseeName must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.InstallationId))
        {
            issues.Add("InstallationId must not be empty.");
        }

        if (payload.MaxActiveDeviceConnections < 0)
        {
            issues.Add("MaxActiveDeviceConnections must not be negative.");
        }

        if (payload.ValidFromUtc == default)
        {
            issues.Add("ValidFromUtc must not be default.");
        }

        if (payload.ValidUntilUtc == default)
        {
            issues.Add("ValidUntilUtc must not be default.");
        }

        if (payload.ValidUntilUtc < payload.ValidFromUtc)
        {
            issues.Add("ValidUntilUtc must not be before ValidFromUtc.");
        }

        if (payload.GraceDays < 0)
        {
            issues.Add("GraceDays must not be negative.");
        }

        if (payload.IssuedAtUtc == default)
        {
            issues.Add("IssuedAtUtc must not be default.");
        }

        if (string.IsNullOrWhiteSpace(payload.Issuer))
        {
            issues.Add("Issuer must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.LicenseType))
        {
            issues.Add("LicenseType must not be empty.");
        }

        return issues;
    }
}
