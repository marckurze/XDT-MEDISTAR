namespace XdtDeviceBridge.Core;

public sealed class LicensePublicKeyProvider : ILicensePublicKeyProvider
{
    public const string ProductionKeyId = "xdtbox-prod-2026-01";
    public const string DevelopmentKeyId = "xdtbox-dev-rsa-pss-2026-01";

    private const string ProductionPublicKeySubjectPublicKeyInfoBase64 =
        "MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEA8R/0KHlWs4G8+Pj/XDI6G2myVNDSL4ig+S8fjPlv4f1iLbAr96CeeL6FgibElPz4oXQp4L7cS2BLI26J6Tsg+/sZI36jMJCvJkcAX8Xq/6P83DlEamyyDeovRqTb+Mwe/6Bgu7VMod/Y49QVxbacICdIvHBnA9DSv506HGy0rE/MwKKmVR63SjGz4nUCYEGAd5mbOA8jYgVoqZDP1lKNSCNRxf23S5Po6/I4CiSVqiySokTNtdnoF7ZkQJrcg83zAviszXbVvIRjrYSvgrtDfAHhm3X5Ik61vTNO+mKCG8HilDCo0EmifbFa04Qw/g8gWwgmx2kYSAeBbeQtKDYz1pxQ/F4gu5GQet6l93QtSNXezCOHAHs2ET/Zv60afjQldMAQVw/6TN2miLsndRk1LT70O7Dtp0Ln5Vs/KYHxr8qpvaUColoaGoGmAYcJm82gjq7kGgqUc+aUc93l17TjRuSYeD7xnc8fDq1tKLH5F/0tw6HoMhCUHPgDlYk7M+KPAgMBAAE=";

    private const string DevelopmentPublicKeySubjectPublicKeyInfoBase64 =
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkSgJi3SXsVlmdoiuoKn5jJvB/pLCApjoBKYxCPV/O6DwB7qX2KHOsUPIEV1sg6CEp63O8JmPY+PsI4ME/+oQ00cFXccgPYgSxqU22xWmqTlsmTNRzUj88CKAkH8LzE3Baj9OU6vnYBks2Xk4tW4HzI3YpktZpqloFj9mwyDnVor835cU2XXRHt8QxHq8aMuY58PvikqekvBJ9ijVKMfrYs369BksoEAEcPJmKgAWN5Meq8FG6no242z5qZy2tPnwumCVZO+nMV6lzzf1wW5ZIkMVcYNqGa7KSpxEqxIkL+WfxkmF82lioLIB907lDeILKJekaiQqKbngp+n84iQ/jQIDAQAB";

    private readonly IReadOnlyDictionary<string, byte[]> _publicKeysByKeyId;

    public LicensePublicKeyProvider()
        : this(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ProductionKeyId] = ProductionPublicKeySubjectPublicKeyInfoBase64,
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
