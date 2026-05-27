namespace XdtDeviceBridge.Core;

public sealed record LicenseSignatureVerificationResult(
    LicenseSignatureVerificationStatus Status,
    string Message)
{
    public bool IsValid => Status == LicenseSignatureVerificationStatus.Valid;
}
