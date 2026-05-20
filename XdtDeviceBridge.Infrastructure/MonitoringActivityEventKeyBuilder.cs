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
        return CreateDeviceDetectedKey(queue, includeAttachmentDeviceFiles: false);
    }

    public static string CreateDeviceDetectedKey(PendingImportQueue queue, bool includeAttachmentDeviceFiles)
    {
        ArgumentNullException.ThrowIfNull(queue);

        return CreateFileKey(ScanDeviceDetected, queue.GetAll().Where(file => IsStableDeviceFile(file, includeAttachmentDeviceFiles)));
    }

    public static string CreateReadyPairKey(PendingImportQueue queue)
    {
        return CreateReadyPairKey(queue, includeAttachmentDeviceFiles: false);
    }

    public static string CreateReadyPairKey(PendingImportQueue queue, bool includeAttachmentDeviceFiles)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var pairKeys = queue.FindReadyPairs(includeAttachmentDeviceFiles)
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
            && file.Kind.IsAisImportFile();
    }

    private static bool IsStableDeviceFile(PendingImportFile file, bool includeAttachmentDeviceFiles)
    {
        return file.Status == PendingImportFileStatus.Stable
            && file.Kind.IsDeviceImportFile(includeAttachmentDeviceFiles);
    }
}
