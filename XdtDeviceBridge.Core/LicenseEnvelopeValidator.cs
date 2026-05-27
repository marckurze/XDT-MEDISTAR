namespace XdtDeviceBridge.Core;

public static class LicenseEnvelopeValidator
{
    public static IReadOnlyList<string> Validate(LicenseEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(envelope.PayloadBase64Url))
        {
            issues.Add("PayloadBase64Url must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SignatureBase64Url))
        {
            issues.Add("SignatureBase64Url must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(envelope.Algorithm))
        {
            issues.Add("Algorithm must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(envelope.KeyId))
        {
            issues.Add("KeyId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(envelope.FormatVersion))
        {
            issues.Add("FormatVersion must not be empty.");
        }

        return issues;
    }
}
