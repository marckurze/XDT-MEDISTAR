namespace XdtDeviceBridge.Core;

public static class AisProfileValidator
{
    public static IReadOnlyList<string> Validate(AisProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var issues = new List<string>();

        if (profile.Metadata is null)
        {
            issues.Add("Metadata must not be null.");
        }
        else if (profile.Metadata.ProfileKind != ProfileKind.AisProfile)
        {
            issues.Add("Metadata.ProfileKind must be AisProfile.");
        }

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            issues.Add("Name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.DefaultEncoding))
        {
            issues.Add("DefaultEncoding must not be empty.");
        }

        if (profile.RequiredPatientFieldCodes is null)
        {
            issues.Add("RequiredPatientFieldCodes must not be null.");
        }

        if (profile.SupportedOutputFieldCodes is null)
        {
            issues.Add("SupportedOutputFieldCodes must not be null.");
        }
        else
        {
            if (profile.RequiresExaminationType8402
                && !profile.SupportedOutputFieldCodes.Contains("8402", StringComparer.OrdinalIgnoreCase))
            {
                issues.Add("SupportedOutputFieldCodes must contain 8402 when RequiresExaminationType8402 is true.");
            }

            if (profile.SupportsResultTextField6228
                && !profile.SupportedOutputFieldCodes.Contains("6228", StringComparer.OrdinalIgnoreCase))
            {
                issues.Add("SupportedOutputFieldCodes must contain 6228 when SupportsResultTextField6228 is true.");
            }
        }

        return issues;
    }
}
