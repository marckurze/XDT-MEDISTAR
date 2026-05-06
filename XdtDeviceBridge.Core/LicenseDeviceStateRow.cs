namespace XdtDeviceBridge.Core;

public sealed record LicenseDeviceStateRow(
    string Name,
    string AktivText,
    string LizenzpflichtigText,
    string GedecktText,
    string KarenzText,
    string KarenzBisText,
    string Status)
{
    public static LicenseDeviceStateRow FromState(LicensedDeviceState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new LicenseDeviceStateRow(
            Name: state.DisplayName,
            AktivText: FormatBoolean(state.IsActive),
            LizenzpflichtigText: FormatBoolean(state.IsLicenseRequired),
            GedecktText: FormatBoolean(state.IsCoveredByLicense),
            KarenzText: FormatBoolean(state.IsInGracePeriod),
            KarenzBisText: state.GracePeriodEndsAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? string.Empty,
            Status: state.StatusMessage);
    }

    private static string FormatBoolean(bool value)
    {
        return value ? "Ja" : "Nein";
    }
}
