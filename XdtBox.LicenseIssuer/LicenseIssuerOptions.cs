namespace XdtBox.LicenseIssuer;

public sealed record LicenseIssuerOptions(
    string? RequestFile,
    string? InstallationId,
    string LicenseeName,
    string? CustomerNumber,
    int MaxActiveDeviceConnections,
    DateTime ValidFromUtc,
    DateTime ValidUntilUtc,
    int GraceDays,
    string LicenseType,
    string Issuer,
    string ProductCode,
    string? Notes,
    string KeyId,
    string PrivateKeyPath,
    string OutputFile);
