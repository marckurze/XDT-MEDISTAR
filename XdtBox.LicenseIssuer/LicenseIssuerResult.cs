using XdtDeviceBridge.Core;

namespace XdtBox.LicenseIssuer;

public sealed record LicenseIssuerResult(
    LicensePayload Payload,
    LicenseEnvelope Envelope,
    string OutputFile);
