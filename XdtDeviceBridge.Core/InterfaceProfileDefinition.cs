namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileDefinition(
    ProfileMetadata Metadata,
    string AisProfileId,
    string DeviceProfileId,
    string ExportProfileId,
    InterfaceFolderOptions FolderOptions,
    bool IsActive,
    bool IsLicenseRequired,
    string? Description)
{
    public IReadOnlyList<string> Validate()
    {
        return InterfaceProfileDefinitionValidator.Validate(this);
    }
}
