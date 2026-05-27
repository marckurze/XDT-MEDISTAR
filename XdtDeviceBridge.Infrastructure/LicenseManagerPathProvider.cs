namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerPathProvider
{
    public const string DefaultBaseFolder = @"C:\XDTBox\Lizenzaktivierung";

    public LicenseManagerPaths GetDefaultPaths()
    {
        return GetPaths(DefaultBaseFolder);
    }

    public LicenseManagerPaths GetPaths(string baseFolder)
    {
        if (string.IsNullOrWhiteSpace(baseFolder))
        {
            throw new ArgumentException("Base folder must not be empty.", nameof(baseFolder));
        }

        var normalizedBaseFolder = Path.GetFullPath(baseFolder);
        var dataFolder = Path.Combine(normalizedBaseFolder, "data");

        return new LicenseManagerPaths(
            BaseFolder: normalizedBaseFolder,
            LicensesFolder: Path.Combine(normalizedBaseFolder, "licenses"),
            RequestsFolder: Path.Combine(normalizedBaseFolder, "requests"),
            KeysFolder: Path.Combine(normalizedBaseFolder, "keys"),
            DataFolder: dataFolder,
            HistoryFile: Path.Combine(dataFolder, "license-history.json"),
            SettingsFile: Path.Combine(dataFolder, "license-manager-settings.json"));
    }
}
