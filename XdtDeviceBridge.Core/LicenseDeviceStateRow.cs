namespace XdtDeviceBridge.Core;

public sealed record LicenseDeviceStateRow(
    string InterfaceProfileId,
    string Name,
    string AktivText,
    string LizenzpflichtigText,
    string GedecktText,
    string KarenzText,
    string KarenzBisText,
    string KarenzVerbleibendText,
    string Status)
{
    public string Standort { get; set; } = string.Empty;

    public static LicenseDeviceStateRow FromState(
        LicensedDeviceState state,
        string? location = null,
        DateTime? nowUtc = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        var effectiveNowUtc = nowUtc ?? DateTime.UtcNow;

        return new LicenseDeviceStateRow(
            InterfaceProfileId: state.InterfaceProfileId,
            Name: state.DisplayName,
            AktivText: FormatBoolean(state.IsActive),
            LizenzpflichtigText: FormatBoolean(state.IsLicenseRequired),
            GedecktText: FormatBoolean(state.IsCoveredByLicense),
            KarenzText: FormatBoolean(state.IsInGracePeriod),
            KarenzBisText: state.GracePeriodEndsAt?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? string.Empty,
            KarenzVerbleibendText: FormatGraceRemaining(state, effectiveNowUtc),
            Status: state.StatusMessage)
        {
            Standort = location?.Trim() ?? string.Empty
        };
    }

    private static string FormatBoolean(bool value)
    {
        return value ? "Ja" : "Nein";
    }

    private static string FormatGraceRemaining(LicensedDeviceState state, DateTime nowUtc)
    {
        if (!state.IsActive || !state.IsLicenseRequired)
        {
            return string.Empty;
        }

        if (state.IsCoveredByLicense)
        {
            return "Lizenziert";
        }

        if (state.GracePeriodEndsAt is null)
        {
            return "Nicht gestartet";
        }

        var remaining = state.GracePeriodEndsAt.Value - nowUtc;
        if (remaining <= TimeSpan.Zero)
        {
            return "Abgelaufen";
        }

        var days = (int)Math.Floor(remaining.TotalDays);
        if (days > 0)
        {
            return days == 1
                ? "1 Tag"
                : $"{days} Tage";
        }

        var hours = (int)Math.Ceiling(remaining.TotalHours);
        return hours <= 1
            ? "unter 1 Stunde"
            : $"{hours} Stunden";
    }
}
