namespace XdtDeviceBridge.Core;

public sealed record TemplatePackage(
    ProfileMetadata Metadata,
    IReadOnlyList<ProfileMetadata> IncludedProfiles,
    string PackageFormatVersion,
    DateTime CreatedAt,
    string? CreatedBy,
    string? Description)
{
    public IReadOnlyList<string> Validate()
    {
        return TemplatePackageValidator.Validate(this);
    }
}
