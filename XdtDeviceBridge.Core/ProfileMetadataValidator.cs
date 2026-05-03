namespace XdtDeviceBridge.Core;

public static class ProfileMetadataValidator
{
    public static IReadOnlyList<string> Validate(ProfileMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(metadata.Id))
        {
            issues.Add("Id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadata.Name))
        {
            issues.Add("Name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadata.Version))
        {
            issues.Add("Version must not be empty.");
        }

        if (metadata.CreatedAt == default)
        {
            issues.Add("CreatedAt must not be default.");
        }

        return issues;
    }
}
