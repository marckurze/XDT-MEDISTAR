namespace XdtDeviceBridge.Core;

public enum NidekRtSerialOutputFrameVariant
{
    FullSelectedData,
    ArOnly,
    LmOnly,
    LmOnlyWithoutAdd,
    ArOnlyWithoutId,
    LmOnlyWithoutId,
    FullWithoutId,
    MinimalRightOnly
}

public static class NidekRtSerialOutputFrameVariantInfo
{
    public const NidekRtSerialOutputFrameVariant Default = NidekRtSerialOutputFrameVariant.FullSelectedData;

    public static NidekRtSerialOutputFrameVariant Resolve(NidekRtSerialOutputFrameVariant? variant)
    {
        return variant is { } value && Enum.IsDefined(value)
            ? value
            : Default;
    }

    public static string ToDisplayName(NidekRtSerialOutputFrameVariant variant)
    {
        return variant switch
        {
            NidekRtSerialOutputFrameVariant.ArOnly => "Nur Autoref",
            NidekRtSerialOutputFrameVariant.LmOnly => "Nur Lensmeter",
            NidekRtSerialOutputFrameVariant.LmOnlyWithoutAdd => "Nur Lensmeter ohne ADD",
            NidekRtSerialOutputFrameVariant.ArOnlyWithoutId => "Nur Autoref ohne ID",
            NidekRtSerialOutputFrameVariant.LmOnlyWithoutId => "Nur Lensmeter ohne ID",
            NidekRtSerialOutputFrameVariant.FullWithoutId => "Alle ausgewählten Werte ohne ID",
            NidekRtSerialOutputFrameVariant.MinimalRightOnly => "Minimal rechts",
            _ => "Alle ausgewählten Werte"
        };
    }
}
