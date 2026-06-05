namespace XdtDeviceBridge.Infrastructure;

public sealed record LocalLicenseRemovalResult(
    bool RemovedSignedLicense,
    bool RemovedLegacyLicense)
{
    public bool RemovedAnyLicense => RemovedSignedLicense || RemovedLegacyLicense;
}

public sealed class LocalLicenseRemovalService
{
    private const string SignedLicenseFileName = "license.xdtboxlic";

    public LocalLicenseRemovalResult RemoveLocalLicense(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var signedLicenseFile = GetSignedLicenseFilePath(paths);
        var removedSignedLicense = DeleteIfExists(signedLicenseFile);
        var removedLegacyLicense = DeleteIfExists(paths.LicenseFile);

        return new LocalLicenseRemovalResult(removedSignedLicense, removedLegacyLicense);
    }

    public static string GetSignedLicenseFilePath(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return Path.Combine(paths.LicensesFolder, SignedLicenseFileName);
    }

    private static bool DeleteIfExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }
}
