namespace XdtDeviceBridge.Core;

public static class LicensedDeviceGracePeriodValidator
{
    public static IReadOnlyList<string> Validate(LicensedDeviceGracePeriod gracePeriod)
    {
        ArgumentNullException.ThrowIfNull(gracePeriod);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(gracePeriod.InterfaceProfileId))
        {
            issues.Add("InterfaceProfileId must not be empty.");
        }

        if (gracePeriod.StartedAtUtc == default)
        {
            issues.Add("StartedAtUtc must not be default.");
        }

        if (gracePeriod.EndsAtUtc == default)
        {
            issues.Add("EndsAtUtc must not be default.");
        }

        if (gracePeriod.EndsAtUtc < gracePeriod.StartedAtUtc)
        {
            issues.Add("EndsAtUtc must not be before StartedAtUtc.");
        }

        return issues;
    }

    public static IReadOnlyList<string> Validate(LicensedDeviceGracePeriodStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var issues = new List<string>();
        if (store.GracePeriods is null)
        {
            issues.Add("GracePeriods must not be null.");
            return issues;
        }

        issues.AddRange(store.GracePeriods.SelectMany(Validate));

        var duplicateIds = store.GracePeriods
            .Where(gracePeriod => !string.IsNullOrWhiteSpace(gracePeriod.InterfaceProfileId))
            .GroupBy(gracePeriod => gracePeriod.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var duplicateId in duplicateIds)
        {
            issues.Add($"Duplicate InterfaceProfileId in grace period store: {duplicateId}.");
        }

        return issues;
    }
}
