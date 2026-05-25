namespace XdtDeviceBridge.Core;

public sealed record InterfaceMonitoringCardDisplay(
    string InterfaceProfileId,
    string InterfaceProfileName,
    string AisName,
    string DeviceName,
    string ExportProfileName,
    string CurrentStatus,
    string StatusClass,
    int ScanIntervalSeconds,
    string ScanIntervalText,
    bool IsScanAnimationActive,
    string LastScanText,
    string AutomaticProcessingText,
    string PatientDisplayText,
    string AisFileName,
    string DeviceFileName,
    string AttachmentFileName,
    string ExportFileName,
    string LastSuccessfulExportText,
    string LastMessage,
    IReadOnlyList<ExpectedInputDisplayItem> ExpectedInputs,
    IReadOnlyList<InterfaceMonitoringDetailItem> FolderDetails,
    string AttachmentImportFolder,
    string AttachmentExportFolder,
    string AttachmentConfigurationStatus,
    bool IsDetailsExpanded = false,
    string DeviceType = "",
    string DeviceImagePath = "",
    bool UsesPilotDeviceVisual = false,
    string DeviceTypeDisplay = "")
{
    public bool HasDeviceImage => IsUsableDeviceImagePath(DeviceImagePath);

    public bool ShouldPulseStatusOrb => UsesPilotDeviceVisual && IsScanAnimationActive;

    public string EffectiveDeviceTypeDisplay => string.IsNullOrWhiteSpace(DeviceTypeDisplay)
        ? DeviceType
        : DeviceTypeDisplay;

    public InterfaceMonitoringCardDisplay WithPilotMonitoringActivity(bool isMonitoringActive)
    {
        var updated = this with
        {
            IsScanAnimationActive = isMonitoringActive
        };

        if (!updated.UsesPilotDeviceVisual)
        {
            return updated;
        }

        var idleStatus = isMonitoringActive ? "wartet" : "gestoppt";
        var idleStatusClass = isMonitoringActive ? "Waiting" : "Neutral";
        return updated with
        {
            ExpectedInputs = updated.ExpectedInputs
                .Select(input => IsPilotIdleInput(input)
                    ? input with
                    {
                        Status = idleStatus,
                        StatusClass = idleStatusClass
                    }
                    : input)
                .ToList()
        };
    }

    private static bool IsPilotIdleInput(ExpectedInputDisplayItem input)
    {
        return input.Key is "ais" or "device"
            && string.IsNullOrWhiteSpace(input.DisplayDetail)
            && input.Status is "gestoppt" or "wartet" or "erwartet";
    }

    private static bool IsUsableDeviceImagePath(string? deviceImagePath)
    {
        if (string.IsNullOrWhiteSpace(deviceImagePath))
        {
            return false;
        }

        var trimmed = deviceImagePath.Trim();
        if (trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            return !Path.IsPathFullyQualified(trimmed)
                || File.Exists(trimmed);
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
}
