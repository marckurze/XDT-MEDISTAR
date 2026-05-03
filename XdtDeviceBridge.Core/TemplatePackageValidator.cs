namespace XdtDeviceBridge.Core;

public static class TemplatePackageValidator
{
    public static IReadOnlyList<string> Validate(TemplatePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        var issues = new List<string>();

        if (package.Metadata is null)
        {
            issues.Add("Metadata must not be null.");
        }
        else if (package.Metadata.ProfileKind != ProfileKind.TemplatePackage)
        {
            issues.Add("Metadata.ProfileKind must be TemplatePackage.");
        }

        if (string.IsNullOrWhiteSpace(package.PackageFormatVersion))
        {
            issues.Add("PackageFormatVersion must not be empty.");
        }

        if (package.CreatedAt == default)
        {
            issues.Add("CreatedAt must not be default.");
        }

        if (package.IncludedProfiles is null)
        {
            issues.Add("IncludedProfiles must not be null.");
        }
        else
        {
            var duplicateIds = package.IncludedProfiles
                .Where(profile => profile is not null)
                .GroupBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            foreach (var duplicateId in duplicateIds)
            {
                issues.Add($"IncludedProfiles contains duplicate Id: {duplicateId}");
            }
        }

        return issues;
    }
}
