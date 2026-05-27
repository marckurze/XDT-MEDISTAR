namespace XdtDeviceBridge.Core;

public sealed record LicenseEnvelope(
    string PayloadBase64Url,
    string SignatureBase64Url,
    string Algorithm,
    string KeyId,
    string FormatVersion)
{
    public IReadOnlyList<string> Validate()
    {
        return LicenseEnvelopeValidator.Validate(this);
    }
}
