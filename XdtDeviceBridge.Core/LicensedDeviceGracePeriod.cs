namespace XdtDeviceBridge.Core;

public sealed record LicensedDeviceGracePeriod(
    string InterfaceProfileId,
    DateTime StartedAtUtc,
    DateTime EndsAtUtc,
    string Reason)
{
    public IReadOnlyList<string> Validate()
    {
        return LicensedDeviceGracePeriodValidator.Validate(this);
    }
}
