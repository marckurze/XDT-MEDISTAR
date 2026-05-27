namespace XdtDeviceBridge.Core;

public sealed class LicensePublicKeyProvider : ILicensePublicKeyProvider
{
    public const string DevelopmentKeyId = "xdtbox-dev-rsa-pss-2026-01";

    private const string DevelopmentPublicKeySubjectPublicKeyInfoBase64 =
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkSgJi3SXsVlmdoiuoKn5jJvB/pLCApjoBKYxCPV/O6DwB7qX2KHOsUPIEV1sg6CEp63O8JmPY+PsI4ME/+oQ00cFXccgPYgSxqU22xWmqTlsmTNRzUj88CKAkH8LzE3Baj9OU6vnYBks2Xk4tW4HzI3YpktZpqloFj9mwyDnVor835cU2XXRHt8QxHq8aMuY58PvikqekvBJ9ijVKMfrYs369BksoEAEcPJmKgAWN5Meq8FG6no242z5qZy2tPnwumCVZO+nMV6lzzf1wW5ZIkMVcYNqGa7KSpxEqxIkL+WfxkmF82lioLIB907lDeILKJekaiQqKbngp+n84iQ/jQIDAQAB";

    private readonly IReadOnlyDictionary<string, byte[]> _publicKeysByKeyId;

    public LicensePublicKeyProvider()
        : this(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DevelopmentKeyId] = DevelopmentPublicKeySubjectPublicKeyInfoBase64
        })
    {
    }

    public LicensePublicKeyProvider(IReadOnlyDictionary<string, string> publicKeysByKeyIdBase64)
    {
        ArgumentNullException.ThrowIfNull(publicKeysByKeyIdBase64);

        _publicKeysByKeyId = publicKeysByKeyIdBase64.ToDictionary(
            pair => pair.Key,
            pair => Convert.FromBase64String(pair.Value),
            StringComparer.Ordinal);
    }

    public bool TryGetPublicKey(string keyId, out byte[] publicKeySubjectPublicKeyInfo)
    {
        publicKeySubjectPublicKeyInfo = Array.Empty<byte>();
        if (string.IsNullOrWhiteSpace(keyId)
            || !_publicKeysByKeyId.TryGetValue(keyId, out var publicKey))
        {
            return false;
        }

        publicKeySubjectPublicKeyInfo = publicKey.ToArray();
        return true;
    }
}
