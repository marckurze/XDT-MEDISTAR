using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AutoImportScannerService : IAutoImportScanner
{
    private readonly DirectorySnapshotService _directorySnapshotService;
    private readonly FileStabilityService _fileStabilityService;
    private readonly ImportFileClassifier _importFileClassifier;

    public AutoImportScannerService()
        : this(new DirectorySnapshotService(), new FileStabilityService(), new ImportFileClassifier())
    {
    }

    public AutoImportScannerService(
        DirectorySnapshotService directorySnapshotService,
        FileStabilityService fileStabilityService,
        ImportFileClassifier importFileClassifier)
    {
        _directorySnapshotService = directorySnapshotService;
        _fileStabilityService = fileStabilityService;
        _importFileClassifier = importFileClassifier;
    }

    public async Task<AutoImportScanResult> ScanOnceAsync(
        InterfaceProfileDefinition profile,
        TimeSpan stabilityDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (stabilityDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(stabilityDuration), "Stability duration must not be negative.");
        }

        var queue = new PendingImportQueue();
        var messages = new List<string>();
        var aisFilesDetected = 0;
        var deviceFilesDetected = 0;
        var filesQueued = 0;

        if (!profile.IsActive)
        {
            messages.Add("Schnittstellenprofil ist nicht aktiv.");
            return CreateResult(profile, aisFilesDetected, deviceFilesDetected, filesQueued, messages, queue);
        }

        if (string.IsNullOrWhiteSpace(profile.FolderOptions.AisImportFolder))
        {
            messages.Add("AIS-Importordner fehlt.");
        }
        else
        {
            var aisScan = await ScanFolderAsync(
                profile.FolderOptions.AisImportFolder,
                stabilityDuration,
                IsRelevantAisFile,
                queue,
                "AIS-Importordner",
                cancellationToken).ConfigureAwait(false);
            aisFilesDetected = aisScan.FilesDetected;
            filesQueued += aisScan.FilesQueued;
            messages.AddRange(aisScan.Messages);
        }

        var isManualDocumentSelection = profile.FolderOptions.IsAttachmentOnlyMode
            && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        if (isManualDocumentSelection)
        {
            messages.Add("Manuelle Dokumentübergabe: keine Geräte-Datei im Importordner erforderlich.");
        }
        else if (string.IsNullOrWhiteSpace(profile.FolderOptions.DeviceImportFolder))
        {
            messages.Add("Geräte-Importordner fehlt.");
        }
        else
        {
            var includeAttachmentDeviceFiles = profile.FolderOptions.IsAttachmentOnlyMode;
            var deviceScan = await ScanFolderAsync(
                profile.FolderOptions.DeviceImportFolder,
                stabilityDuration,
                kind => IsRelevantDeviceFile(kind, includeAttachmentDeviceFiles),
                queue,
                "Geräte-Importordner",
                cancellationToken).ConfigureAwait(false);
            deviceFilesDetected = deviceScan.FilesDetected;
            filesQueued += deviceScan.FilesQueued;
            messages.AddRange(deviceScan.Messages);
        }

        return CreateResult(profile, aisFilesDetected, deviceFilesDetected, filesQueued, messages, queue);
    }

    private async Task<FolderScanResult> ScanFolderAsync(
        string folderPath,
        TimeSpan stabilityDuration,
        Func<ImportFileKind, bool> isRelevantKind,
        PendingImportQueue queue,
        string folderLabel,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>();
        var snapshot = _directorySnapshotService.GetSnapshot(folderPath);
        if (!snapshot.Exists)
        {
            messages.Add($"{folderLabel} existiert nicht: {folderPath}");
            return new FolderScanResult(FilesDetected: 0, FilesQueued: 0, Messages: messages);
        }

        var filesDetected = 0;
        var filesQueued = 0;
        foreach (var file in snapshot.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var classification = _importFileClassifier.Classify(file.FilePath);
            if (!isRelevantKind(classification.Kind))
            {
                continue;
            }

            filesDetected++;
            var stabilityResult = await _fileStabilityService
                .CheckAsync(file.FilePath, stabilityDuration, cancellationToken)
                .ConfigureAwait(false);

            if (!stabilityResult.IsStable)
            {
                messages.Add($"{file.FileName}: {stabilityResult.Message}");
                continue;
            }

            var nowUtc = DateTime.UtcNow;
            queue.AddOrUpdate(new PendingImportFile(
                FilePath: classification.FilePath,
                FileName: classification.FileName,
                Kind: classification.Kind,
                Status: PendingImportFileStatus.Stable,
                DetectedAtUtc: file.LastWriteTimeUtc,
                StableAtUtc: nowUtc,
                Message: classification.Message));
            filesQueued++;
        }

        return new FolderScanResult(filesDetected, filesQueued, messages);
    }

    private static AutoImportScanResult CreateResult(
        InterfaceProfileDefinition profile,
        int aisFilesDetected,
        int deviceFilesDetected,
        int filesQueued,
        IReadOnlyList<string> messages,
        PendingImportQueue queue)
    {
        return new AutoImportScanResult(
            InterfaceProfileId: profile.Metadata.Id,
            AisFilesDetected: aisFilesDetected,
            DeviceFilesDetected: deviceFilesDetected,
            FilesQueued: filesQueued,
            ReadyPairs: queue.FindReadyPairs(
                profile.FolderOptions.IsAttachmentOnlyMode,
                profile.FolderOptions.IsAttachmentOnlyMode
                    && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection).Count,
            Messages: messages,
            Queue: queue);
    }

    private static bool IsRelevantAisFile(ImportFileKind kind)
    {
        return kind is ImportFileKind.AisGdt or ImportFileKind.AisXdt;
    }

    private static bool IsRelevantDeviceFile(ImportFileKind kind, bool includeAttachmentDeviceFiles)
    {
        return kind.IsDeviceImportFile(includeAttachmentDeviceFiles);
    }

    private sealed record FolderScanResult(
        int FilesDetected,
        int FilesQueued,
        IReadOnlyList<string> Messages);
}
