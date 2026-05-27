namespace XdtDeviceBridge.Core;

public sealed record LicenseManagerSettings(
    string DefaultOutputFolder,
    string DefaultRequestFolder,
    string DefaultKeyFolder,
    string? PrivateKeyPath,
    string KeyId,
    string DefaultIssuer,
    int DefaultGraceDays)
{
    public static LicenseManagerSettings CreateDefault(string baseFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseFolder);

        return new LicenseManagerSettings(
            DefaultOutputFolder: Path.Combine(baseFolder, "licenses"),
            DefaultRequestFolder: Path.Combine(baseFolder, "requests"),
            DefaultKeyFolder: Path.Combine(baseFolder, "keys"),
            PrivateKeyPath: null,
            KeyId: "xdtbox-prod-2026-01",
            DefaultIssuer: "Technik-Apparat",
            DefaultGraceDays: 7);
    }
}
