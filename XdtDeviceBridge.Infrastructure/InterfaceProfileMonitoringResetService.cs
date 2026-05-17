namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileMonitoringResetService
{
    private readonly Dictionary<string, HashSet<string>> _ignoredFileKeysByProfileId = new(StringComparer.OrdinalIgnoreCase);

    public InterfaceProfileMonitoringResetResult Reset(
        string interfaceProfileId,
        PendingImportQueue? currentQueue,
        InterfaceProfileInputFolderResetResult? inputFolderResetResult = null)
    {
        var normalizedId = NormalizeId(interfaceProfileId);
        var ignoredFiles = _ignoredFileKeysByProfileId.GetValueOrDefault(normalizedId);
        if (ignoredFiles is null)
        {
            ignoredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _ignoredFileKeysByProfileId[normalizedId] = ignoredFiles;
        }

        var ignoredFileCount = 0;
        if (currentQueue is not null)
        {
            foreach (var file in currentQueue.GetAll())
            {
                if (ignoredFiles.Add(CreateFileKey(file)))
                {
                    ignoredFileCount++;
                }
            }
        }

        var messages = new List<string>
        {
            inputFolderResetResult?.SummaryMessage ?? "Vorgang zurückgesetzt."
        };

        if (inputFolderResetResult is not null)
        {
            messages.AddRange(inputFolderResetResult.Messages);
        }

        if (ignoredFileCount > 0
            && (inputFolderResetResult is null || inputFolderResetResult.FailedFileCount > 0))
        {
            messages.Add($"{ignoredFileCount} bekannte Datei(en) dieses Vorgangs werden bis zur Änderung ignoriert.");
        }

        return new InterfaceProfileMonitoringResetResult(
            normalizedId,
            ignoredFileCount,
            FileOperationsPerformed: inputFolderResetResult?.FileOperationsPerformed ?? false,
            InputFolderResetResult: inputFolderResetResult,
            Messages: messages);
    }

    public AutoImportScanResult Apply(AutoImportScanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var normalizedId = NormalizeId(result.InterfaceProfileId);
        if (!_ignoredFileKeysByProfileId.TryGetValue(normalizedId, out var ignoredFiles)
            || ignoredFiles.Count == 0)
        {
            return result;
        }

        var filteredQueue = new PendingImportQueue();
        foreach (var file in result.Queue.GetAll())
        {
            if (!ignoredFiles.Contains(CreateFileKey(file)))
            {
                filteredQueue.AddOrUpdate(file);
            }
        }

        var files = filteredQueue.GetAll();
        return result with
        {
            AisFilesDetected = files.Count(IsAisFile),
            DeviceFilesDetected = files.Count(IsDeviceFile),
            FilesQueued = files.Count,
            ReadyPairs = filteredQueue.FindReadyPairs().Count,
            Queue = filteredQueue
        };
    }

    public bool IsIgnored(string interfaceProfileId, PendingImportFile file)
    {
        var normalizedId = NormalizeId(interfaceProfileId);
        return _ignoredFileKeysByProfileId.TryGetValue(normalizedId, out var ignoredFiles)
            && ignoredFiles.Contains(CreateFileKey(file));
    }

    private static string NormalizeId(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            throw new ArgumentException("Schnittstellenprofil-ID fehlt.", nameof(interfaceProfileId));
        }

        return interfaceProfileId.Trim();
    }

    private static string CreateFileKey(PendingImportFile file)
    {
        return $"{file.FilePath.Trim()}|{file.DetectedAtUtc.ToUniversalTime().Ticks}";
    }

    private static bool IsAisFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind == ImportFileKind.AisGdt || file.Kind == ImportFileKind.AisXdt);
    }

    private static bool IsDeviceFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind == ImportFileKind.DeviceXml
                || file.Kind == ImportFileKind.DeviceText
                || file.Kind == ImportFileKind.DeviceCsv);
    }
}
