namespace XdtDeviceBridge.Infrastructure;

public sealed class PendingImportQueue
{
    private readonly Dictionary<string, PendingImportFile> _filesByPath = new(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdate(PendingImportFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (string.IsNullOrWhiteSpace(file.FilePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(file));
        }

        _filesByPath[file.FilePath] = file;
    }

    public IReadOnlyList<PendingImportFile> GetAll()
    {
        return SortFiles(_filesByPath.Values).ToList();
    }

    public IReadOnlyList<PendingImportFile> GetByKind(ImportFileKind kind)
    {
        return SortFiles(_filesByPath.Values.Where(file => file.Kind == kind)).ToList();
    }

    public IReadOnlyList<PendingImportPair> FindReadyPairs()
    {
        return FindReadyPairs(includeAttachmentDeviceFiles: false);
    }

    public IReadOnlyList<PendingImportPair> FindReadyPairs(bool includeAttachmentDeviceFiles)
    {
        return FindReadyPairs(includeAttachmentDeviceFiles, allowAisOnlyManualSelection: false);
    }

    public IReadOnlyList<PendingImportPair> FindReadyPairs(
        bool includeAttachmentDeviceFiles,
        bool allowAisOnlyManualSelection)
    {
        var aisFiles = SortFiles(_filesByPath.Values.Where(IsStableAisFile)).ToList();
        var deviceFiles = SortFiles(_filesByPath.Values.Where(file => IsStableDeviceFile(file, includeAttachmentDeviceFiles))).ToList();
        if (allowAisOnlyManualSelection && deviceFiles.Count == 0)
        {
            return aisFiles
                .Select(aisFile => new PendingImportPair(
                    AisFile: aisFile,
                    DeviceFile: CreateManualSelectionDevicePlaceholder(aisFile),
                    IsReady: true))
                .ToList();
        }

        var pairCount = Math.Min(aisFiles.Count, deviceFiles.Count);
        var pairs = new List<PendingImportPair>(pairCount);

        for (var index = 0; index < pairCount; index++)
        {
            pairs.Add(new PendingImportPair(
                AisFile: aisFiles[index],
                DeviceFile: deviceFiles[index],
                IsReady: true));
        }

        return pairs;
    }

    public bool Remove(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        return _filesByPath.Remove(filePath);
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

    private static PendingImportFile CreateManualSelectionDevicePlaceholder(PendingImportFile aisFile)
    {
        return new PendingImportFile(
            FilePath: $"manual-document-selection://{aisFile.FilePath}",
            FileName: "Manuelle Dokumentauswahl",
            Kind: ImportFileKind.AttachmentFile,
            Status: PendingImportFileStatus.Stable,
            DetectedAtUtc: aisFile.DetectedAtUtc,
            StableAtUtc: aisFile.StableAtUtc,
            Message: "Manuelle Dokumentauswahl wartet auf Dateien.");
    }

    private static IOrderedEnumerable<PendingImportFile> SortFiles(IEnumerable<PendingImportFile> files)
    {
        return files
            .OrderBy(file => file.DetectedAtUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase);
    }
}
