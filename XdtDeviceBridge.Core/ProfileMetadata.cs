namespace XdtDeviceBridge.Core;

public sealed record ProfileMetadata(
    string Id,
    string Name,
    ProfileKind ProfileKind,
    string? Description,
    string? Vendor,
    string? Product,
    string Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? CreatedBy,
    bool IsBuiltIn,
    bool IsUserDefined)
{
    public IReadOnlyList<string> Validate()
    {
        return ProfileMetadataValidator.Validate(this);
    }
}
