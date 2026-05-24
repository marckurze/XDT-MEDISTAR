using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AutoImportPackageStateService
{
    private readonly Dictionary<string, PendingAisState> _pendingAisByProfileId = new(StringComparer.OrdinalIgnoreCase);

    public AutoImportPackageEvaluationResult Evaluate(
        InterfaceProfileDefinition profile,
        PendingImportQueue queue,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(queue);

        var aisFiles = queue.GetAll()
            .Where(IsStableAisFile)
            .OrderBy(file => file.DetectedAtUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var isManualDocumentSelection = profile.FolderOptions.IsAttachmentOnlyMode
            && profile.FolderOptions.AttachmentOnlySourceMode == AttachmentOnlySourceMode.ManualUserSelection;
        var deviceFiles = queue.GetAll()
            .Where(file => IsStableDeviceFile(file, profile.FolderOptions.IsAttachmentOnlyMode))
            .OrderBy(file => file.DetectedAtUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var messages = new List<string>();
        var replaced = new List<PendingImportFile>();
        var expired = new List<PendingImportFile>();

        if (aisFiles.Count == 0)
        {
            _pendingAisByProfileId.Remove(profile.Metadata.Id);
            if (deviceFiles.Count > 0)
            {
                messages.Add("Warte auf AIS-Datei.");
                return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, AutoImportPackageStateReason.WaitingForAisFile);
            }

            return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, AutoImportPackageStateReason.NoImportFiles);
        }

        var latestAisFile = aisFiles[^1];
        var pendingAis = GetOrCreatePendingAis(profile, latestAisFile, nowUtc);
        if (!IsSameQueuedFile(pendingAis.File, latestAisFile))
        {
            pendingAis = CreatePendingAisState(latestAisFile, nowUtc);
            _pendingAisByProfileId[profile.Metadata.Id] = pendingAis;
        }

        if (isManualDocumentSelection)
        {
            var manualPair = queue
                .FindReadyPairs(includeAttachmentDeviceFiles: true, allowAisOnlyManualSelection: true)
                .First(pair => string.Equals(pair.AisFile.FilePath, latestAisFile.FilePath, StringComparison.OrdinalIgnoreCase));
            messages.Add("AIS-Datei bereit; wartet auf manuelle Dokumentauswahl.");
            return CreateResult(new[] { manualPair }, messages, replaced, expired, AutoImportPackageStateReason.ReadyForProcessing);
        }

        if (deviceFiles.Count == 0)
        {
            if (HasDeviceWaitTimedOut(profile, pendingAis, nowUtc))
            {
                expired.Add(pendingAis.File);
                messages.Add("AIS-Datei abgelaufen: keine Gerätedatei innerhalb der Wartezeit gefunden.");
                _pendingAisByProfileId.Remove(profile.Metadata.Id);
                return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, AutoImportPackageStateReason.AisFileExpired);
            }

            messages.Add(pendingAis.IsCv5000DeviceOutputPhaseStarted
                ? "Warte auf Phoropter-Rueckgabedatei."
                : "Warte auf Gerätedatei.");
            return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, AutoImportPackageStateReason.WaitingForDeviceFile);
        }

        var selectedAis = latestAisFile;
        var isCv5000 = InterfaceProfileUiPolicy.IsCv5000(profile, deviceProfile: null);
        var selectedDevice = deviceFiles[^1];

        var pair = new PendingImportPair(selectedAis, selectedDevice, IsReady: true);
        if (isCv5000 && pendingAis.IsCv5000DeviceOutputPhaseStarted)
        {
            messages.Add("CV-5000-Phoropter-Rueckgabe liegt stabil vor; Export wird gestartet.");
        }

        messages.Add("AIS-/Geräte-Dateipaar vollständig.");

        return CreateResult(new[] { pair }, messages, replaced, expired, AutoImportPackageStateReason.ReadyForProcessing);
    }

    public void ResetProfile(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        _pendingAisByProfileId.Remove(interfaceProfileId.Trim());
    }

    public void MarkCv5000WaitingForDeviceResult(
        string interfaceProfileId,
        PendingImportFile aisFile,
        PendingImportQueue queue,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(aisFile);
        ArgumentNullException.ThrowIfNull(queue);

        _pendingAisByProfileId[interfaceProfileId.Trim()] = new PendingAisState(
            aisFile,
            nowUtc,
            IsCv5000DeviceOutputPhaseStarted: true);
    }

    private PendingAisState GetOrCreatePendingAis(
        InterfaceProfileDefinition profile,
        PendingImportFile latestAisFile,
        DateTime nowUtc)
    {
        if (!_pendingAisByProfileId.TryGetValue(profile.Metadata.Id, out var pendingAis))
        {
            pendingAis = CreatePendingAisState(latestAisFile, nowUtc);
            _pendingAisByProfileId[profile.Metadata.Id] = pendingAis;
        }

        return pendingAis;
    }

    private static PendingAisState CreatePendingAisState(PendingImportFile latestAisFile, DateTime nowUtc)
    {
        return new PendingAisState(
            latestAisFile,
            nowUtc,
            IsCv5000DeviceOutputPhaseStarted: false);
    }

    private static bool HasDeviceWaitTimedOut(
        InterfaceProfileDefinition profile,
        PendingAisState pendingAis,
        DateTime nowUtc)
    {
        var timeout = TimeSpan.FromMinutes(Math.Max(0, profile.FolderOptions.DeviceFileWaitTimeoutMinutes));
        return nowUtc - pendingAis.FirstSeenAtUtc >= timeout;
    }

    private static AutoImportPackageEvaluationResult CreateResult(
        IReadOnlyList<PendingImportPair> readyPairs,
        IReadOnlyList<string> messages,
        IReadOnlyList<PendingImportFile> replacedAisFiles,
        IReadOnlyList<PendingImportFile> expiredAisFiles,
        AutoImportPackageStateReason reason)
    {
        return new AutoImportPackageEvaluationResult(
            ReadyPairs: readyPairs,
            Messages: messages,
            ReplacedAisFiles: replacedAisFiles,
            ExpiredAisFiles: expiredAisFiles,
            Reason: reason);
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

    private static bool IsSameQueuedFile(PendingImportFile left, PendingImportFile right)
    {
        return string.Equals(left.FilePath, right.FilePath, StringComparison.OrdinalIgnoreCase)
            && left.DetectedAtUtc == right.DetectedAtUtc
            && left.StableAtUtc == right.StableAtUtc;
    }

    private sealed record PendingAisState(
        PendingImportFile File,
        DateTime FirstSeenAtUtc,
        bool IsCv5000DeviceOutputPhaseStarted);
}
