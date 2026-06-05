namespace XdtDeviceBridge.Core;

public static class InterfaceProfileUiPolicy
{
    private const string Cv5000InterfaceProfileId = "interface-medistar-topcon-cv5000-default";
    private const string Cv5000DeviceProfileId = "device-topcon-cv5000-default";
    private const string NidekRt6100InterfaceProfileId = "interface-medistar-nidek-rt6100-default";
    private const string NidekRt6100DeviceProfileId = "device-nidek-rt6100-default";
    private static readonly string[] NidekRtSerialInterfaceProfileIds =
    {
        "interface-medistar-nidek-rt2100-serial-default",
        "interface-medistar-nidek-rt3100-serial-default",
        "interface-medistar-nidek-rt5100-serial-default"
    };
    private static readonly string[] NidekRtSerialDeviceProfileIds =
    {
        "device-nidek-rt2100-serial-default",
        "device-nidek-rt3100-serial-default",
        "device-nidek-rt5100-serial-default"
    };
    public const double PilotMonitoringCardWidth = 576;
    public const double PilotFloatingWindowMinWidth = 672;
    public const double PilotFloatingWindowDefaultWidth = 744;
    public const double MonitoringInputBadgeMinWidth = 120;
    public const double MonitoringInputBadgeMaxWidth = 176;
    public const double FloatingInputBadgeMinWidth = 128;
    public const double FloatingInputBadgeMaxWidth = 188;
    public const string BuiltInDeviceImageRoot = "pack://application:,,,/Assets/Devices/";
    public const string TopconCv5000DeviceImagePath = BuiltInDeviceImageRoot + "Topcon_CV5000_freigestellt.png";
    public const string NidekArk1sDeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-ark1s-default.png";
    public const string NidekAr360DeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-ar360-default.png";
    public const string NidekLm7DeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-lm7-default.png";
    public const string NidekNt530PDeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-nt530p-default.png";
    public const string NidekRt6100DeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-rt6100-default.png";
    public const string NidekRt2100SerialDeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-rt2100-serial-default.png";
    public const string NidekRt3100SerialDeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-rt3100-serial-default.png";
    public const string NidekRt5100SerialDeviceImagePath = BuiltInDeviceImageRoot + "device-nidek-rt5100-serial-default.png";
    public const string HuvitzHrk8000ADeviceImagePath = BuiltInDeviceImageRoot + "device-huvitz-hrk8000a-default.png";
    public const string HuvitzHrk9000ADeviceImagePath = BuiltInDeviceImageRoot + "device-huvitz-hrk9000a-default.png";
    public const string HuvitzHnt1PDeviceImagePath = BuiltInDeviceImageRoot + "device-huvitz-hnt1p-default.png";
    public const string HuvitzHtr1ADeviceImagePath = BuiltInDeviceImageRoot + "device-huvitz-htr1a-default.png";
    public const string ShinNipponTextDeviceImagePath = BuiltInDeviceImageRoot + "device-document-attachment-default.png";
    public const string TomeyCf2000DeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-cf2000-default.png";
    public const string TomeyTl2000CDeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-tl2000c-default.png";
    public const string TomeyTl6000DeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-tl6000-default.png";
    public const string TomeyTl7000DeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-tl7000-default.png";
    public const string TomeyMr6000DeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-mr6000-default.png";
    public const string TomeyTop1000DeviceImagePath = BuiltInDeviceImageRoot + "device-tomey-top1000-default.png";
    public const string TomeyEm3000DeviceImagePath = TomeyTop1000DeviceImagePath;
    public const string TomeyEm4000DeviceImagePath = TomeyTop1000DeviceImagePath;
    public const string TopconCl300DeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-cl300-default.png";
    public const string TopconSolosDeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-solos-default.png";
    public const string TopconKr800DeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-kr800-default.png";
    public const string TopconKr1DeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-kr1-default.png";
    public const string TopconTrk2PDeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-trk2p-default.png";
    public const string TopconCt1PDeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-ct1p-default.png";
    public const string TopconCt800ADeviceImagePath = BuiltInDeviceImageRoot + "device-topcon-ct800a-default.png";
    public const string DocumentAttachmentDeviceImagePath = BuiltInDeviceImageRoot + "device-document-attachment-default.png";
    public const string ManualDocumentSelectionDeviceImagePath = BuiltInDeviceImageRoot + "device-manual-document-selection-default.png";
    private static readonly IReadOnlyDictionary<string, string> BuiltInDeviceImagePathsByDeviceProfileId =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["device-nidek-ark1s-default"] = NidekArk1sDeviceImagePath,
            ["device-nidek-ar360-default"] = NidekAr360DeviceImagePath,
            ["device-nidek-lm7-default"] = NidekLm7DeviceImagePath,
            ["device-nidek-nt530p-default"] = NidekNt530PDeviceImagePath,
            ["device-nidek-rt6100-default"] = NidekRt6100DeviceImagePath,
            ["device-nidek-rt2100-serial-default"] = NidekRt2100SerialDeviceImagePath,
            ["device-nidek-rt3100-serial-default"] = NidekRt3100SerialDeviceImagePath,
            ["device-nidek-rt5100-serial-default"] = NidekRt5100SerialDeviceImagePath,
            ["device-huvitz-hrk8000a-default"] = HuvitzHrk8000ADeviceImagePath,
            ["device-huvitz-hrk9000a-default"] = HuvitzHrk9000ADeviceImagePath,
            ["device-huvitz-hnt1p-default"] = HuvitzHnt1PDeviceImagePath,
            ["device-huvitz-htr1a-default"] = HuvitzHtr1ADeviceImagePath,
            ["device-shin-nippon-accuref-r800-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-accuref-k900-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-dl1000-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-dl800-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-dl900-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-nct200-default"] = ShinNipponTextDeviceImagePath,
            ["device-shin-nippon-slm4000-default"] = ShinNipponTextDeviceImagePath,
            ["device-tomey-cf2000-default"] = TomeyCf2000DeviceImagePath,
            ["device-tomey-tl2000c-default"] = TomeyTl2000CDeviceImagePath,
            ["device-tomey-tl6000-default"] = TomeyTl6000DeviceImagePath,
            ["device-tomey-tl7000-default"] = TomeyTl7000DeviceImagePath,
            ["device-tomey-mr6000-default"] = TomeyMr6000DeviceImagePath,
            ["device-tomey-top1000-default"] = TomeyTop1000DeviceImagePath,
            ["device-tomey-em3000-default"] = TomeyEm3000DeviceImagePath,
            ["device-tomey-em4000-default"] = TomeyEm4000DeviceImagePath,
            ["device-topcon-cl300-default"] = TopconCl300DeviceImagePath,
            ["device-topcon-solos-default"] = TopconSolosDeviceImagePath,
            ["device-topcon-kr800-default"] = TopconKr800DeviceImagePath,
            ["device-topcon-kr1-default"] = TopconKr1DeviceImagePath,
            ["device-topcon-trk2p-default"] = TopconTrk2PDeviceImagePath,
            ["device-topcon-ct1p-default"] = TopconCt1PDeviceImagePath,
            ["device-topcon-ct800a-default"] = TopconCt800ADeviceImagePath,
            ["device-topcon-cv5000-default"] = TopconCv5000DeviceImagePath,
            ["device-document-attachment-default"] = DocumentAttachmentDeviceImagePath,
            ["device-manual-document-selection-default"] = ManualDocumentSelectionDeviceImagePath
        };

    public static bool ShouldShowDeviceOutput(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (deviceProfile?.ConnectionKind == DeviceConnectionKind.SerialRs232)
        {
            return false;
        }

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

    public static bool ShouldTriggerNidekRtSerialPhoropterWorkflow(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        if (!IsNidekRtSerialPhoropter(interfaceProfile, deviceProfile))
        {
            return false;
        }

        return interfaceProfile?.SerialSettings?.IsBidirectional == true
            || deviceProfile?.SerialSettings?.IsBidirectional == true
            || deviceProfile?.IsBidirectional == true;
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

    public static string GetBuiltInDeviceImagePathForDeviceProfileId(string? deviceProfileId)
    {
        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            return string.Empty;
        }

        return BuiltInDeviceImagePathsByDeviceProfileId.TryGetValue(deviceProfileId.Trim(), out var imagePath)
            ? imagePath
            : string.Empty;
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
        var deviceProfilePath = GetBuiltInDeviceImagePathForDeviceProfileId(deviceProfile?.Metadata.Id);
        if (!string.IsNullOrWhiteSpace(deviceProfilePath))
        {
            return deviceProfilePath;
        }

        var interfaceDeviceProfilePath = GetBuiltInDeviceImagePathForDeviceProfileId(interfaceProfile?.DeviceProfileId);
        if (!string.IsNullOrWhiteSpace(interfaceDeviceProfilePath))
        {
            return interfaceDeviceProfilePath;
        }

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

    public static bool IsNidekRtSerialPhoropter(
        InterfaceProfileDefinition? interfaceProfile,
        DeviceProfileDefinition? deviceProfile)
    {
        return ContainsId(NidekRtSerialInterfaceProfileIds, interfaceProfile?.Metadata.Id)
            || ContainsId(NidekRtSerialDeviceProfileIds, interfaceProfile?.DeviceProfileId)
            || ContainsId(NidekRtSerialDeviceProfileIds, deviceProfile?.Metadata.Id)
            || (deviceProfile?.ConnectionKind == DeviceConnectionKind.SerialRs232
                && ContainsNidekRtSerialModel(deviceProfile.Model, deviceProfile.Metadata.Product));
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

    private static bool ContainsNidekRtSerialModel(params string?[] values)
    {
        return values.Any(value => !string.IsNullOrWhiteSpace(value)
            && (ContainsAny(value, "RT-2100", "RT2100")
                || ContainsAny(value, "RT-3100", "RT3100")
                || ContainsAny(value, "RT-5100", "RT5100")));
    }

    private static bool ContainsId(IReadOnlyList<string> ids, string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && ids.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        return !string.IsNullOrWhiteSpace(value)
            && needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }
}
