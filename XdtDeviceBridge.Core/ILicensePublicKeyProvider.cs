namespace XdtDeviceBridge.Core;

public interface ILicensePublicKeyProvider
{
    bool TryGetPublicKey(string keyId, out byte[] publicKeySubjectPublicKeyInfo);
}
