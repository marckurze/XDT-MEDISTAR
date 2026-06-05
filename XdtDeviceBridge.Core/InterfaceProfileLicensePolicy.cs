namespace XdtDeviceBridge.Core;

public static class InterfaceProfileLicensePolicy
{
    public static bool IsLicenseRequired(InterfaceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return true;
    }

    public static bool IsActiveLicenseRelevant(InterfaceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.IsActive;
    }
}
