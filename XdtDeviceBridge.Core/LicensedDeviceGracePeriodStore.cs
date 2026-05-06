namespace XdtDeviceBridge.Core;

public sealed record LicensedDeviceGracePeriodStore(
    IReadOnlyList<LicensedDeviceGracePeriod> GracePeriods)
{
    public static LicensedDeviceGracePeriodStore Empty { get; } =
        new(Array.Empty<LicensedDeviceGracePeriod>());

    public IReadOnlyList<string> Validate()
    {
        return LicensedDeviceGracePeriodValidator.Validate(this);
    }
}
