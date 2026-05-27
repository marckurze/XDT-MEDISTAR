using Microsoft.Win32;
using System.Runtime.Versioning;

namespace XdtDeviceBridge.Infrastructure;

public sealed class SerialPortDiscoveryService : ISerialPortDiscoveryService
{
    private const string SerialCommRegistryPath = @"HARDWARE\DEVICEMAP\SERIALCOMM";

    public IReadOnlyList<string> GetAvailablePortNames()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Array.Empty<string>();
        }

        return GetAvailablePortNamesWindows();
    }

    [SupportedOSPlatform("windows")]
    private static IReadOnlyList<string> GetAvailablePortNamesWindows()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(SerialCommRegistryPath);
            if (key is null)
            {
                return Array.Empty<string>();
            }

            return key.GetValueNames()
                .Select(name => key.GetValue(name) as string)
                .Where(portName => !string.IsNullOrWhiteSpace(portName))
                .Select(portName => portName!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(CreateSortKey, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static string CreateSortKey(string portName)
    {
        if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(portName[3..], out var portNumber))
        {
            return $"COM{portNumber:D5}";
        }

        return portName;
    }
}
