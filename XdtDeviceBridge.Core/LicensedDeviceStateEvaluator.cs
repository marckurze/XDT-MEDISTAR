namespace XdtDeviceBridge.Core;

public sealed class LicensedDeviceStateEvaluator
{
    public IReadOnlyList<LicensedDeviceState> Evaluate(
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        LicenseInfo? license,
        DateTime nowUtc)
    {
        return Evaluate(interfaceProfiles, license, Array.Empty<LicensedDeviceGracePeriod>(), nowUtc);
    }

    public IReadOnlyList<LicensedDeviceState> Evaluate(
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        LicenseInfo? license,
        IReadOnlyList<LicensedDeviceGracePeriod> gracePeriods,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfiles);
        ArgumentNullException.ThrowIfNull(gracePeriods);

        var licensedDeviceCount = Math.Max(0, license?.LicensedDeviceCount ?? 0);
        var coveredLicenseRequiredProfiles = 0;
        var states = new List<LicensedDeviceState>();
        var gracePeriodsByProfileId = gracePeriods
            .Where(gracePeriod => !string.IsNullOrWhiteSpace(gracePeriod.InterfaceProfileId))
            .GroupBy(gracePeriod => gracePeriod.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var profile in interfaceProfiles)
        {
            var isLicenseRelevant = profile.IsActive && profile.IsLicenseRequired;
            var isCovered = false;
            var isInGracePeriod = false;
            DateTime? gracePeriodStartedAt = null;
            DateTime? gracePeriodEndsAt = null;
            string statusMessage;

            if (!profile.IsLicenseRequired)
            {
                statusMessage = "Nicht lizenzpflichtig.";
            }
            else if (!profile.IsActive)
            {
                statusMessage = "Lizenzpflichtig, aber nicht aktiv - zählt aktuell nicht.";
            }
            else if (license is null)
            {
                statusMessage = "Nicht lizenziert: keine Lizenz vorhanden.";
                ApplyGracePeriodState(
                    profile,
                    gracePeriodsByProfileId,
                    nowUtc,
                    ref isInGracePeriod,
                    ref gracePeriodStartedAt,
                    ref gracePeriodEndsAt,
                    ref statusMessage);
            }
            else if (coveredLicenseRequiredProfiles < licensedDeviceCount)
            {
                coveredLicenseRequiredProfiles++;
                isCovered = true;
                statusMessage = "Durch Lizenz gedeckt.";
            }
            else
            {
                coveredLicenseRequiredProfiles++;
                statusMessage = "Nicht durch Lizenz gedeckt: lizenzierte Anzahl ueberschritten.";
                ApplyGracePeriodState(
                    profile,
                    gracePeriodsByProfileId,
                    nowUtc,
                    ref isInGracePeriod,
                    ref gracePeriodStartedAt,
                    ref gracePeriodEndsAt,
                    ref statusMessage);
            }

            states.Add(new LicensedDeviceState(
                InterfaceProfileId: profile.Metadata.Id,
                DisplayName: profile.Metadata.Name,
                IsActive: profile.IsActive,
                IsLicenseRequired: profile.IsLicenseRequired,
                IsCoveredByLicense: isLicenseRelevant && isCovered,
                IsInGracePeriod: isInGracePeriod,
                GracePeriodStartedAt: gracePeriodStartedAt,
                GracePeriodEndsAt: gracePeriodEndsAt,
                StatusMessage: statusMessage));
        }

        return states;
    }

    private static void ApplyGracePeriodState(
        InterfaceProfileDefinition profile,
        IReadOnlyDictionary<string, LicensedDeviceGracePeriod> gracePeriodsByProfileId,
        DateTime nowUtc,
        ref bool isInGracePeriod,
        ref DateTime? gracePeriodStartedAt,
        ref DateTime? gracePeriodEndsAt,
        ref string statusMessage)
    {
        if (!gracePeriodsByProfileId.TryGetValue(profile.Metadata.Id, out var gracePeriod))
        {
            return;
        }

        gracePeriodStartedAt = gracePeriod.StartedAtUtc;
        gracePeriodEndsAt = gracePeriod.EndsAtUtc;

        if (nowUtc <= gracePeriod.EndsAtUtc)
        {
            isInGracePeriod = true;
            statusMessage = $"Nicht lizenziert, aber in Karenzzeit bis {gracePeriod.EndsAtUtc:dd.MM.yyyy}.";
            return;
        }

        statusMessage = "Nicht durch Lizenz gedeckt. Karenzzeit abgelaufen.";
    }
}
