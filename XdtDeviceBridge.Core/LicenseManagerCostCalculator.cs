namespace XdtDeviceBridge.Core;

public static class LicenseManagerCostCalculator
{
    public static decimal CalculateNetTotal(int activeLicensedDeviceCount, decimal pricePerDeviceNet)
    {
        if (activeLicensedDeviceCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeLicensedDeviceCount));
        }

        if (pricePerDeviceNet < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pricePerDeviceNet));
        }

        return activeLicensedDeviceCount * pricePerDeviceNet;
    }
}
