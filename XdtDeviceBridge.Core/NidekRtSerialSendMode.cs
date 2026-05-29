namespace XdtDeviceBridge.Core;

public enum NidekRtSerialSendMode
{
    RsSdHandshake,
    DirectWriterFrame,
    RsThenWriterWithoutSd
}

public static class NidekRtSerialSendModeInfo
{
    public const NidekRtSerialSendMode Default = NidekRtSerialSendMode.DirectWriterFrame;

    public static NidekRtSerialSendMode Resolve(NidekRtSerialSendMode? mode)
    {
        return mode ?? Default;
    }

    public static string ToDisplayName(NidekRtSerialSendMode mode)
    {
        return mode switch
        {
            NidekRtSerialSendMode.RsSdHandshake => "RS/SD-Handshake",
            NidekRtSerialSendMode.DirectWriterFrame => "Direkt Writer-Frame senden",
            NidekRtSerialSendMode.RsThenWriterWithoutSd => "RS senden, dann Writer ohne SD",
            _ => "Direkt Writer-Frame senden"
        };
    }
}
