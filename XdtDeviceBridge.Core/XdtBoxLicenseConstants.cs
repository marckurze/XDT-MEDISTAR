namespace XdtDeviceBridge.Core;

public static class XdtBoxLicenseConstants
{
    public const string ProductCode = "XDTBOX";
    public const int DefaultGraceDays = 7;
    public const string DefaultIssuer = "Technik-Apparat";
    public static readonly DateTime UnlimitedValidUntilUtc = new(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public const string InvalidSignatureMessage = "Lizenzdatei ist ungültig oder wurde verändert.";
    public const string HardwareMigrationWarningText = "Achtung: Bei Hardwaretausch bitte neue Lizenz anfordern, Karenzzeit 7 Tage ab Umzug der Hardware.";

    public static string CreateSuccessfulLicenseImportMessage(int maxActiveDeviceConnections)
    {
        var deviceText = maxActiveDeviceConnections == 1 ? "Gerät" : "Geräte";
        return $"Wir haben {maxActiveDeviceConnections} {deviceText} für Sie lizenziert. Vielen Dank. Ihr XDTBox Team.";
    }

    public static bool IsUnlimitedValidUntil(DateTime? validUntilUtc)
    {
        return validUntilUtc.HasValue
            && validUntilUtc.Value.ToUniversalTime().Date >= UnlimitedValidUntilUtc.Date;
    }
}
