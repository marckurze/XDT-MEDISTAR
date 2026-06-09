namespace XdtDeviceBridge.Core;

public sealed record LicenseDeviceLocationStore(
    IReadOnlyList<LicenseDeviceLocation> DeviceLocations)
{
    public static LicenseDeviceLocationStore Empty { get; } =
        new(Array.Empty<LicenseDeviceLocation>());

    public IReadOnlyDictionary<string, string?> ToDictionary()
    {
        return DeviceLocations
            .Where(location => !string.IsNullOrWhiteSpace(location.InterfaceProfileId))
            .GroupBy(location => location.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Location,
                StringComparer.OrdinalIgnoreCase);
    }
}
