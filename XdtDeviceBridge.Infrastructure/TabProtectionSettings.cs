namespace XdtDeviceBridge.Infrastructure;

public sealed record TabProtectionSettings(
    bool IsEnabled,
    string? SaltBase64,
    string? PasswordHashBase64,
    int Iterations)
{
    public static TabProtectionSettings Disabled { get; } = new(false, null, null, 0);

    public bool HasPassword =>
        IsEnabled
        && !string.IsNullOrWhiteSpace(SaltBase64)
        && !string.IsNullOrWhiteSpace(PasswordHashBase64)
        && Iterations > 0;
}
