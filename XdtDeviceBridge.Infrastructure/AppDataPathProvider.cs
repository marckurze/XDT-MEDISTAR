namespace XdtDeviceBridge.Infrastructure;

public sealed class AppDataPathProvider
{
    private const string ApplicationFolderName = "XdtDeviceBridge";

    public AppDataPaths GetDefaultUserPaths()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return GetPaths(Path.Combine(localAppData, ApplicationFolderName));
    }

    public AppDataPaths GetPaths(string baseFolder)
    {
        if (string.IsNullOrWhiteSpace(baseFolder))
        {
            throw new ArgumentException("Base folder must not be empty.", nameof(baseFolder));
        }

        var normalizedBaseFolder = Path.GetFullPath(baseFolder);
        var profilesFolder = Path.Combine(normalizedBaseFolder, "profiles");
        var templatesFolder = Path.Combine(normalizedBaseFolder, "templates");
        var licensesFolder = Path.Combine(normalizedBaseFolder, "licenses");
        var logsFolder = Path.Combine(normalizedBaseFolder, "logs");
        var licenseRequestsFolder = Path.Combine(normalizedBaseFolder, "license-requests");
        var templatePackagesFolder = Path.Combine(normalizedBaseFolder, "template-packages");

        return new AppDataPaths(
            BaseFolder: normalizedBaseFolder,
            ProfilesFolder: profilesFolder,
            TemplatesFolder: templatesFolder,
            LicensesFolder: licensesFolder,
            LogsFolder: logsFolder,
            InstallationInfoFile: Path.Combine(normalizedBaseFolder, "installation.json"),
            LicenseFile: Path.Combine(licensesFolder, "license.json"),
            LicenseRequestsFolder: licenseRequestsFolder,
            TemplatePackagesFolder: templatePackagesFolder);
    }
}
