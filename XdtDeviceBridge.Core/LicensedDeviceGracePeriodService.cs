namespace XdtDeviceBridge.Core;

public sealed class LicensedDeviceGracePeriodService
{
    private const string NewActiveInterfaceProfileReason = "New active license-required interface profile";

    public LicensedDeviceGracePeriodStore EnsureGracePeriodsForUncoveredDevices(
        IEnumerable<LicensedDeviceState> states,
        LicensedDeviceGracePeriodStore existingStore,
        DateTime nowUtc,
        int graceDays)
    {
        ArgumentNullException.ThrowIfNull(states);
        ArgumentNullException.ThrowIfNull(existingStore);

        if (nowUtc == default)
        {
            throw new ArgumentException("NowUtc must not be default.", nameof(nowUtc));
        }

        if (graceDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(graceDays), "Grace days must not be negative.");
        }

        var stateList = states.ToList();
        var existingProfileIds = stateList
            .Select(state => state.InterfaceProfileId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var gracePeriods = existingStore.GracePeriods
            .Where(gracePeriod => existingProfileIds.Contains(gracePeriod.InterfaceProfileId))
            .GroupBy(gracePeriod => gracePeriod.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        var profileIdsWithGracePeriod = gracePeriods
            .Select(gracePeriod => gracePeriod.InterfaceProfileId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var state in stateList)
        {
            if (!ShouldCreateGracePeriod(state) || profileIdsWithGracePeriod.Contains(state.InterfaceProfileId))
            {
                continue;
            }

            var gracePeriod = new LicensedDeviceGracePeriod(
                InterfaceProfileId: state.InterfaceProfileId,
                StartedAtUtc: nowUtc,
                EndsAtUtc: nowUtc.AddDays(graceDays),
                Reason: NewActiveInterfaceProfileReason);

            gracePeriods.Add(gracePeriod);
            profileIdsWithGracePeriod.Add(state.InterfaceProfileId);
        }

        return new LicensedDeviceGracePeriodStore(gracePeriods);
    }

    private static bool ShouldCreateGracePeriod(LicensedDeviceState state)
    {
        return state.IsActive
            && state.IsLicenseRequired
            && !state.IsCoveredByLicense;
    }
}
