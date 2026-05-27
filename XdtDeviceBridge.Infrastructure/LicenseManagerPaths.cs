namespace XdtDeviceBridge.Infrastructure;

public sealed record LicenseManagerPaths(
    string BaseFolder,
    string LicensesFolder,
    string RequestsFolder,
    string KeysFolder,
    string DataFolder,
    string HistoryFile,
    string SettingsFile);
