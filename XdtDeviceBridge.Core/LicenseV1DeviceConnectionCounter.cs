namespace XdtDeviceBridge.Core;

public static class LicenseV1DeviceConnectionCounter
{
    public static int CountActiveDeviceConnections(IEnumerable<InterfaceProfileDefinition> interfaceProfiles)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfiles);

        return interfaceProfiles.Count(profile => profile.IsActive);
    }
}
