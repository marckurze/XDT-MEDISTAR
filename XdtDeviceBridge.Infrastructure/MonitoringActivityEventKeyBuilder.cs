namespace XdtDeviceBridge.Infrastructure;

public static class MonitoringActivityEventKeyBuilder
{
    public const string ScanAisDetected = "scan-ais-detected";
    public const string ScanDeviceDetected = "scan-device-detected";
    public const string ScanReadyPair = "scan-ready-pair";

    public static string CreateAisDetectedKey(PendingImportQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        return CreateFileKey(ScanAisDetected, queue.GetAll().Where(IsStableAisFile));
    }

    public static string CreateDeviceDetectedKey(PendingImportQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        return CreateFileKey(ScanDeviceDetected, queue.GetAll().Where(IsStableDeviceFile));
    }

    public static string CreateReadyPairKey(PendingImportQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var pairKeys = queue.FindReadyPairs()
            .Select(pair => string.Join(
                "+",
                ImportFileFingerprint.Create(pair.AisFile),
                ImportFileFingerprint.Create(pair.DeviceFile)));

        return CreateKey(ScanReadyPair, pairKeys);
    }

    private static string CreateFileKey(string baseKey, IEnumerable<PendingImportFile> files)
    {
        return CreateKey(baseKey, files.Select(ImportFileFingerprint.Create));
    }

    private static string CreateKey(string baseKey, IEnumerable<string> keyParts)
    {
        var parts = keyParts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();
        return parts.Length == 0
            ? baseKey
            : $"{baseKey}:{string.Join(";", parts)}";
    }

    private static bool IsStableAisFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind == ImportFileKind.AisGdt || file.Kind == ImportFileKind.AisXdt);
    }

    private static bool IsStableDeviceFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind == ImportFileKind.DeviceXml
                || file.Kind == ImportFileKind.DeviceText
                || file.Kind == ImportFileKind.DeviceCsv);
    }
}
