using System.Text.Json;

namespace XdtDeviceBridge.Core;

public static class LicensePayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false
    };

    public static byte[] SerializeToUtf8Bytes(LicensePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return JsonSerializer.SerializeToUtf8Bytes(payload, Options);
    }

    public static LicensePayload? Deserialize(ReadOnlySpan<byte> payloadBytes)
    {
        return JsonSerializer.Deserialize<LicensePayload>(payloadBytes, Options);
    }
}
