namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebOptions
{
    public const string SectionName = "LicenseWeb";

    public string? DataRoot { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string KeyId { get; set; } = XdtDeviceBridge.Core.LicensePublicKeyProvider.ProductionKeyId;
    public string Issuer { get; set; } = "XDTBox Lizenzmanager Web";
    public int GraceDays { get; set; } = 7;
    public AdminOptions Admin { get; set; } = new();

    public sealed class AdminOptions
    {
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
    }
}
