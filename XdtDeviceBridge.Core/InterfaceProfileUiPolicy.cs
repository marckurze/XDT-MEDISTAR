namespace XdtDeviceBridge.Core;

public static class InterfaceProfileUiPolicy
{
    private const string Cv5000InterfaceProfileId = "interface-medistar-topcon-cv5000-default";
    private const string Cv5000DeviceProfileId = "device-topcon-cv5000-default";
    private const string NidekRt6100InterfaceProfileId = "interface-medistar-nidek-rt6100-default";
    private const string NidekRt6100DeviceProfileId = "device-nidek-rt6100-default";
    public const double PilotMonitoringCardWidth = 576;
    public const double PilotFloatingWindowMinWidth = 672;
    public const double PilotFloatingWindowDefaultWidth = 744;
    public const double MonitoringInputBadgeMinWidth = 120;
    public const double MonitoringInputBadgeMaxWidth = 176;
    public const double FloatingInputBadgeMinWidth = 128;
    public const double FloatingInputBadgeMaxWidth = 188;
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
        return !IsCv5000(interfaceProfile, deviceProfile)
            && !IsNidekRt6100(interfaceProfile, deviceProfile);
    }

    public static bool ShouldTriggerCv5000DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return IsCv5000(interfaceProfile, deviceProfile)
            && interfaceProfile?.DeviceOutput?.IsEnabled == true;
    }

    public static bool ShouldTriggerNidekRt6100DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return IsNidekRt6100(interfaceProfile, deviceProfile)
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

    public static string GetMonitoringDeviceTypeDisplay(DeviceProfileDefinition? deviceProfile)
    {
        var deviceType = deviceProfile?.DeviceType?.Trim() ?? string.Empty;
        var model = deviceProfile?.Model?.Trim() ?? string.Empty;
        var product = deviceProfile?.Metadata.Product?.Trim() ?? string.Empty;

        if (ContainsAny(deviceType, "phoropter") || ContainsAny(model, "CV-5000", "CV5000"))
        {
            return "Phoropter";
        }

        if (ContainsAny(deviceType, "lensmeter"))
        {
            return "Lensmeter";
        }

        if (ContainsAny(deviceType, "document", "dokument"))
        {
            return "Dokumentgerät";
        }

        if (ContainsAny(model, "KR-1", "KR1", "KR-800", "KR800")
            || ContainsAny(product, "KR-1", "KR1", "KR-800", "KR800")
            || (ContainsAny(deviceType, "keratometer", "kerato")
                && ContainsAny(deviceType, "autorefractor", "autorefraktor", "refraktometer")))
        {
            return "Keratorefraktometer";
        }

        if (ContainsAny(deviceType, "tonometer"))
        {
            return "Tonometer";
        }

        if (ContainsAny(deviceType, "autorefractor", "autorefraktor", "refraktometer"))
        {
            return "Autorefraktor";
        }

        return string.IsNullOrWhiteSpace(deviceType)
            ? "Generisch"
            : deviceType;
    }

    public static bool ShouldUseTextAboveImageMonitoringLayout(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        var deviceTypeDisplay = GetMonitoringDeviceTypeDisplay(deviceProfile);
        var interfaceName = interfaceProfile?.Metadata.Name?.Trim() ?? string.Empty;
        var deviceName = deviceProfile?.Metadata.Name?.Trim() ?? string.Empty;

        return deviceTypeDisplay.Length > 22
            || interfaceName.Length > 58
            || deviceName.Length > 58;
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

    public static string? ValidateNidekRt6100DeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (!ShouldTriggerNidekRt6100DeviceOutput(interfaceProfile, deviceProfile))
        {
            return "Ausgabe an Gerät ist für dieses Schnittstellenprofil nicht aktiv.";
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile?.DeviceOutput?.OutputFolder))
        {
            return "Ausgabeordner an RT-6100 fehlt.";
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile?.DeviceOutput?.FileNameTemplate))
        {
            return "Dateiname für RT-6100-Importdatei fehlt.";
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

    public static bool IsNidekRt6100(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return string.Equals(interfaceProfile?.Metadata.Id, NidekRt6100InterfaceProfileId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(interfaceProfile?.DeviceProfileId, NidekRt6100DeviceProfileId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(deviceProfile?.Metadata.Id, NidekRt6100DeviceProfileId, StringComparison.OrdinalIgnoreCase)
            || ContainsRt6100(deviceProfile?.Model)
            || ContainsRt6100(deviceProfile?.Metadata.Product);
    }

    private static bool ContainsCv5000(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("CV-5000", StringComparison.OrdinalIgnoreCase)
                || value.Contains("CV5000", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsRt6100(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("RT-6100", StringComparison.OrdinalIgnoreCase)
                || value.Contains("RT6100", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        return !string.IsNullOrWhiteSpace(value)
            && needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }
}
