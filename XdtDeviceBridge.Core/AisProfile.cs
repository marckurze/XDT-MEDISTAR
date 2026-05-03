namespace XdtDeviceBridge.Core;

public sealed record AisProfile(
    ProfileMetadata Metadata,
    string Name,
    string Vendor,
    string DefaultEncoding,
    IReadOnlyDictionary<string, string> RequiredStaticFields,
    IReadOnlyList<string> RequiredPatientFieldCodes,
    IReadOnlyList<string> SupportedOutputFieldCodes,
    bool SupportsResultTextField6228,
    bool SupportsCategoryValuePairs,
    bool RequiresExaminationType8402)
{
    public IReadOnlyList<string> Validate()
    {
        return AisProfileValidator.Validate(this);
    }
}
