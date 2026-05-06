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
        var aisFiles = SortFiles(_filesByPath.Values.Where(IsStableAisFile)).ToList();
        var deviceFiles = SortFiles(_filesByPath.Values.Where(IsStableDeviceFile)).ToList();
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
            && (file.Kind == ImportFileKind.AisGdt || file.Kind == ImportFileKind.AisXdt);
    }

    private static bool IsStableDeviceFile(PendingImportFile file)
    {
        return file.Status == PendingImportFileStatus.Stable
            && (file.Kind == ImportFileKind.DeviceXml
                || file.Kind == ImportFileKind.DeviceText
                || file.Kind == ImportFileKind.DeviceCsv);
    }

    private static IOrderedEnumerable<PendingImportFile> SortFiles(IEnumerable<PendingImportFile> files)
    {
        return files
            .OrderBy(file => file.DetectedAtUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase);
    }
}
