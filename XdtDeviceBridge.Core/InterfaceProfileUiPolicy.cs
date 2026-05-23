namespace XdtDeviceBridge.Core;

public static class InterfaceProfileUiPolicy
{
    private const string Cv5000InterfaceProfileId = "interface-medistar-topcon-cv5000-default";
    private const string Cv5000DeviceProfileId = "device-topcon-cv5000-default";

    public static bool ShouldShowDeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return IsCv5000(interfaceProfile, deviceProfile)
            || deviceProfile?.IsBidirectional == true
            || interfaceProfile?.DeviceOutput is not null;
    }

    public static bool ShouldShowAisAttachmentOptions(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return !IsCv5000(interfaceProfile, deviceProfile);
    }

    public static bool ShouldTriggerCv5000DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return IsCv5000(interfaceProfile, deviceProfile)
            && interfaceProfile?.DeviceOutput?.IsEnabled == true;
    }

    public static string? ValidateCv5000DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (!ShouldTriggerCv5000DeviceOutput(interfaceProfile, deviceProfile))
        {
            return "Ausgabe an Gerät ist für dieses Schnittstellenprofil nicht aktiv.";
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile?.DeviceOutput?.OutputFolder))
        {
            return "Ausgabeordner an Gerät fehlt.";
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile?.DeviceOutput?.FileNameTemplate))
        {
            return "Dateiname für Ausgabe an Gerät fehlt.";
        }

        return null;
    }

    public static bool IsCv5000(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return string.Equals(interfaceProfile?.Metadata.Id, Cv5000InterfaceProfileId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(interfaceProfile?.DeviceProfileId, Cv5000DeviceProfileId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(deviceProfile?.Metadata.Id, Cv5000DeviceProfileId, StringComparison.OrdinalIgnoreCase)
            || ContainsCv5000(deviceProfile?.Model)
            || ContainsCv5000(deviceProfile?.Metadata.Product);
    }

    private static bool ContainsCv5000(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("CV-5000", StringComparison.OrdinalIgnoreCase)
                || value.Contains("CV5000", StringComparison.OrdinalIgnoreCase));
    }
}
