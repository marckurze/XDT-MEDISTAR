namespace XdtDeviceBridge.Core;

public static class DeviceProfileDefinitionValidator
{
    public static IReadOnlyList<string> Validate(DeviceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var issues = new List<string>();

        if (profile.Metadata is null)
        {
            issues.Add("Metadata must not be null.");
        }
        else if (profile.Metadata.ProfileKind != ProfileKind.DeviceProfile)
        {
            issues.Add("Metadata.ProfileKind must be DeviceProfile.");
        }

        if (string.IsNullOrWhiteSpace(profile.Manufacturer))
        {
            issues.Add("Manufacturer must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.Model))
        {
            issues.Add("Model must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.ParserMode))
        {
            issues.Add("ParserMode must not be empty.");
        }

        if (!Enum.IsDefined(profile.ConnectionKind))
        {
            issues.Add("ConnectionKind must be a valid value.");
        }

        if (profile.Measurements is null)
        {
            issues.Add("Measurements must not be null.");
        }
        else
        {
            foreach (var measurement in profile.Measurements)
            {
                if (string.IsNullOrWhiteSpace(measurement.Id))
                {
                    issues.Add("Measurement Id must not be empty.");
                }

                if (string.IsNullOrWhiteSpace(measurement.SourcePath))
                {
                    issues.Add("Measurement SourcePath must not be empty.");
                }
            }

            var duplicateIds = profile.Measurements
                .GroupBy(measurement => measurement.Id, StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
                .Select(group => group.Key);

            foreach (var duplicateId in duplicateIds)
            {
                issues.Add($"Measurements contains duplicate Id: {duplicateId}");
            }
        }

        return issues;
    }
}
