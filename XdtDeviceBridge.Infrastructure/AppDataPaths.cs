namespace XdtDeviceBridge.Infrastructure;

public sealed record AppDataPaths(
    string BaseFolder,
    string ProfilesFolder,
    string TemplatesFolder,
    string LicensesFolder,
    string LogsFolder,
    string InstallationInfoFile,
    string LicenseFile,
    string LicenseRequestsFolder,
    string TemplatePackagesFolder);
