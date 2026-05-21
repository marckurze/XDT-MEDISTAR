using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TerminalBlockedImportFileHandler
{
    private readonly FailedFileCopyService _failedFileCopyService;
    private readonly ProcessedFileArchiveService _processedFileArchiveService;

    public TerminalBlockedImportFileHandler()
        : this(new FailedFileCopyService(), new ProcessedFileArchiveService())
    {
    }

    public TerminalBlockedImportFileHandler(
        FailedFileCopyService failedFileCopyService,
        ProcessedFileArchiveService processedFileArchiveService)
    {
        _failedFileCopyService = failedFileCopyService ?? throw new ArgumentNullException(nameof(failedFileCopyService));
        _processedFileArchiveService = processedFileArchiveService ?? throw new ArgumentNullException(nameof(processedFileArchiveService));
    }

    public TerminalBlockedImportFileHandlingResult Handle(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime blockedAtUtc,
        string failureReason)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(pair);

        var messages = new List<string>();
        var handledFiles = new List<string>();
        var issues = new List<string>();

        if (TryMoveToErrorFolder(interfaceProfile, pair, blockedAtUtc, failureReason, messages, handledFiles, issues))
        {
            return new TerminalBlockedImportFileHandlingResult(handledFiles, messages.Concat(issues).ToList(), issues.Count > 0);
        }

        HandleArchiveFallback(interfaceProfile, pair, blockedAtUtc, messages, handledFiles, issues);
        if (handledFiles.Count == 0 && issues.Count == 0)
        {
            messages.Add("Terminal blockiertes Paket wurde intern abgeschlossen; bekannte Importdateien bleiben im Importordner, da keine sichere Entfernen-Konfiguration aktiv ist.");
        }

        return new TerminalBlockedImportFileHandlingResult(handledFiles, messages.Concat(issues).ToList(), issues.Count > 0);
    }

    private bool TryMoveToErrorFolder(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime blockedAtUtc,
        string failureReason,
        List<string> messages,
        List<string> handledFiles,
        List<string> issues)
    {
        if (!interfaceProfile.FolderOptions.MoveFailedFilesToErrorFolder)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ErrorFolder))
        {
            messages.Add("Fehlerordner ist nicht konfiguriert; versuche Archiv-/Entfernen-Nachlauf.");
            return false;
        }

        try
        {
            var result = _failedFileCopyService.MoveFailedFiles(
                interfaceProfile.FolderOptions.ErrorFolder,
                interfaceProfile.Metadata.Name,
                pair.AisFile.FilePath,
                pair.DeviceFile.FilePath,
                blockedAtUtc,
                failureReason);

            handledFiles.AddRange(result.CopiedFiles);
            issues.AddRange(result.Issues);
            messages.Add(result.HasErrors
                ? "Fehlerablage des blockierten Pakets mit Problemen abgeschlossen."
                : "Bekannte Importdateien des blockierten Pakets wurden in den Fehlerordner verschoben:");
            if (!result.HasErrors)
            {
                messages.AddRange(result.CopiedFiles);
            }

            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            issues.Add($"Fehlerablage des blockierten Pakets fehlgeschlagen: {ex.Message}");
            return true;
        }
    }

    private void HandleArchiveFallback(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime blockedAtUtc,
        List<string> messages,
        List<string> handledFiles,
        List<string> issues)
    {
        if (!interfaceProfile.FolderOptions.ArchiveProcessedFiles)
        {
            RemoveKnownFileIfEnabled(
                interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing,
                pair.AisFile.FilePath,
                handledFiles,
                issues);
            RemoveKnownFileIfEnabled(
                interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing,
                pair.DeviceFile.FilePath,
                handledFiles,
                issues);
            if (handledFiles.Count > 0)
            {
                messages.Add("Bekannte Importdateien des blockierten Pakets wurden aus dem Importordner entfernt:");
                messages.AddRange(handledFiles);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ArchiveFolder))
        {
            messages.Add("Archivordner ist nicht konfiguriert; bekannte Importdateien bleiben im Importordner.");
            return;
        }

        var moveFiles = interfaceProfile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move;
        ArchiveKnownFileIfEnabled(
            moveFiles || interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing,
            interfaceProfile,
            pair.AisFile.FilePath,
            "AIS",
            blockedAtUtc,
            moveFiles || interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing,
            handledFiles,
            issues);
        ArchiveKnownFileIfEnabled(
            moveFiles || interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing,
            interfaceProfile,
            pair.DeviceFile.FilePath,
            "Device",
            blockedAtUtc,
            moveFiles || interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing,
            handledFiles,
            issues);

        if (handledFiles.Count > 0)
        {
            var removedFromImportFolder = moveFiles
                || interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing
                || interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing;
            messages.Add(removedFromImportFolder
                ? "Bekannte Importdateien des blockierten Pakets wurden gemäß Profilregel ins Archiv verschoben:"
                : "Bekannte Importdateien des blockierten Pakets wurden ins Archiv kopiert; Originale bleiben erhalten:");
            messages.AddRange(handledFiles);
        }
    }

    private void ArchiveKnownFileIfEnabled(
        bool enabled,
        InterfaceProfileDefinition interfaceProfile,
        string sourceFilePath,
        string category,
        DateTime blockedAtUtc,
        bool moveFile,
        List<string> handledFiles,
        List<string> issues)
    {
        if (!enabled)
        {
            return;
        }

        try
        {
            var result = _processedFileArchiveService.ArchiveProcessedFile(
                interfaceProfile.FolderOptions.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                sourceFilePath,
                category,
                blockedAtUtc,
                moveFile);
            handledFiles.AddRange(result.ArchivedFiles);
            issues.AddRange(result.Issues);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            issues.Add($"Archivierung des blockierten Pakets fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }

    private static void RemoveKnownFileIfEnabled(
        bool enabled,
        string sourceFilePath,
        List<string> handledFiles,
        List<string> issues)
    {
        if (!enabled)
        {
            return;
        }

        if (!File.Exists(sourceFilePath))
        {
            issues.Add($"Quelldatei fehlt: {sourceFilePath}");
            return;
        }

        try
        {
            File.Delete(sourceFilePath);
            handledFiles.Add(sourceFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            issues.Add($"Entfernen des blockierten Pakets fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }
}
