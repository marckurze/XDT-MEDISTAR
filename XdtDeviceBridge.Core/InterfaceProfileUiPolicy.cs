namespace XdtDeviceBridge.Core;

public static class InterfaceProfileUiPolicy
{
    private const string Cv5000InterfaceProfileId = "interface-medistar-topcon-cv5000-default";
    private const string Cv5000DeviceProfileId = "device-topcon-cv5000-default";
    public const string BuiltInDeviceImageRoot = "pack://application:,,,/Assets/Devices/";
    public const string TopconCv5000DeviceImagePath = BuiltInDeviceImageRoot + "Topcon_CV5000_freigestellt.png";

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

    public static bool ShouldUsePilotMonitoringVisual(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return true;
    }

    public static string GetMonitoringDeviceImagePath(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (!string.IsNullOrWhiteSpace(deviceProfile?.DeviceImagePath))
        {
            return deviceProfile.DeviceImagePath.Trim();
        }

        return GetBuiltInDeviceImagePath(interfaceProfile, deviceProfile);
    }

    public static string GetMonitoringDeviceImagePath(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile,
        string? deviceImageOverridePath)
    {
        if (IsExistingImageOverride(deviceImageOverridePath))
        {
            return deviceImageOverridePath!.Trim();
        }

        return GetMonitoringDeviceImagePath(interfaceProfile, deviceProfile);
    }

    private static string GetBuiltInDeviceImagePath(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return IsCv5000(interfaceProfile, deviceProfile)
            ? TopconCv5000DeviceImagePath
            : string.Empty;
    }

    private static bool IsExistingImageOverride(string? deviceImageOverridePath)
    {
        if (string.IsNullOrWhiteSpace(deviceImageOverridePath))
        {
            return false;
        }

        var trimmed = deviceImageOverridePath.Trim();
        if (trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            return File.Exists(trimmed);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    public static double GetStatusOrbPulseDurationSeconds(int scanIntervalSeconds)
    {
        return Math.Clamp(Math.Max(1, scanIntervalSeconds) * 0.45, 0.65, 2.8);
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
