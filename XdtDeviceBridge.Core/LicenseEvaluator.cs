namespace XdtDeviceBridge.Core;

public sealed class LicenseEvaluator
{
    public LicenseEvaluationResult Evaluate(
        LicenseInfo? license,
        InstallationInfo installation,
        int activeLicensedDeviceCount,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(installation);

        if (license is null)
        {
            return CreateResult(
                LicenseStatus.NotLicensed,
                activeLicensedDeviceCount,
                licensedDeviceCount: 0,
                validUntil: null,
                "Keine Lizenz vorhanden.",
                canProcessFiles: false);
        }

        if (!string.Equals(license.InstallationId, installation.InstallationId, StringComparison.Ordinal))
        {
            return CreateResult(
                LicenseStatus.Invalid,
                activeLicensedDeviceCount,
                license.LicensedDeviceCount,
                license.ValidUntil,
                "Lizenz gehoert nicht zu dieser Installation.",
                canProcessFiles: false);
        }

        if (nowUtc < license.ValidFrom)
        {
            return CreateResult(
                LicenseStatus.Invalid,
                activeLicensedDeviceCount,
                license.LicensedDeviceCount,
                license.ValidUntil,
                "Lizenz ist noch nicht gueltig.",
                canProcessFiles: false);
        }

        if (nowUtc > license.ValidUntil)
        {
            return CreateResult(
                LicenseStatus.Expired,
                activeLicensedDeviceCount,
                license.LicensedDeviceCount,
                license.ValidUntil,
                "Lizenz ist abgelaufen.",
                canProcessFiles: false);
        }

        if (activeLicensedDeviceCount > license.LicensedDeviceCount)
        {
            return CreateResult(
                LicenseStatus.DeviceLimitExceeded,
                activeLicensedDeviceCount,
                license.LicensedDeviceCount,
                license.ValidUntil,
                "Geraeteanzahl ueberschreitet lizenzierte Anzahl.",
                canProcessFiles: false);
        }

        if (license.LicenseType == LicenseType.Trial)
        {
            return CreateResult(
                LicenseStatus.TrialActive,
                activeLicensedDeviceCount,
                license.LicensedDeviceCount,
                license.ValidUntil,
                "Testphase aktiv.",
                canProcessFiles: true);
        }

        return CreateResult(
            LicenseStatus.Active,
            activeLicensedDeviceCount,
            license.LicensedDeviceCount,
            license.ValidUntil,
            "Lizenz aktiv.",
            canProcessFiles: true);
    }

    private static LicenseEvaluationResult CreateResult(
        LicenseStatus status,
        int activeLicensedDeviceCount,
        int licensedDeviceCount,
        DateTime? validUntil,
        string message,
        bool canProcessFiles)
    {
        return new LicenseEvaluationResult(
            Status: status,
            ActiveLicensedDeviceCount: activeLicensedDeviceCount,
            LicensedDeviceCount: licensedDeviceCount,
            ValidUntil: validUntil,
            Messages: new[] { message },
            CanProcessFiles: canProcessFiles);
    }
}
