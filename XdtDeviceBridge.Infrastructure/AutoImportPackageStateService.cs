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
        var latestAisFingerprint = ImportFileFingerprint.Create(latestAisFile);
        if (!string.Equals(pendingAis.Fingerprint, latestAisFingerprint, StringComparison.OrdinalIgnoreCase)
            && latestAisFile.DetectedAtUtc >= pendingAis.File.DetectedAtUtc)
        {
            replaced.Add(pendingAis.File);
            messages.Add("Vorherige AIS-Datei wurde durch neuere AIS-Datei ersetzt.");
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

            messages.Add("Warte auf Gerätedatei.");
            return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, replaced.Count > 0
                ? AutoImportPackageStateReason.AisFileReplaced
                : AutoImportPackageStateReason.WaitingForDeviceFile);
        }

        var selectedAis = aisFiles.FirstOrDefault(file =>
                string.Equals(ImportFileFingerprint.Create(file), pendingAis.Fingerprint, StringComparison.OrdinalIgnoreCase))
            ?? latestAisFile;
        var selectedDevice = InterfaceProfileUiPolicy.IsCv5000(profile, deviceProfile: null)
            ? deviceFiles.FirstOrDefault(file => file.DetectedAtUtc >= selectedAis.DetectedAtUtc)
            : deviceFiles[0];
        if (selectedDevice is null)
        {
            messages.Add("Warte auf neue Phoropter-Rueckgabedatei.");
            return CreateResult(Array.Empty<PendingImportPair>(), messages, replaced, expired, AutoImportPackageStateReason.WaitingForDeviceFile);
        }

        var pair = new PendingImportPair(selectedAis, selectedDevice, IsReady: true);
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
        return new PendingAisState(latestAisFile, ImportFileFingerprint.Create(latestAisFile), nowUtc);
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

    private sealed record PendingAisState(PendingImportFile File, string Fingerprint, DateTime FirstSeenAtUtc);
}
