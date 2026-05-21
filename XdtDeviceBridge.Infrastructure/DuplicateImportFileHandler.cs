using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class DuplicateImportFileHandler
{
    private readonly ProcessedFileArchiveService _archiveService;
    private readonly FailedFileCopyService _failedFileCopyService;

    public DuplicateImportFileHandler()
        : this(new ProcessedFileArchiveService(), new FailedFileCopyService())
    {
    }

    public DuplicateImportFileHandler(ProcessedFileArchiveService archiveService)
        : this(archiveService, new FailedFileCopyService())
    {
    }

    public DuplicateImportFileHandler(
        ProcessedFileArchiveService archiveService,
        FailedFileCopyService failedFileCopyService)
    {
        _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
        _failedFileCopyService = failedFileCopyService ?? throw new ArgumentNullException(nameof(failedFileCopyService));
    }

    public DuplicateImportFileHandlingResult HandleAlreadyProcessedPair(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(pair);

        var messages = new List<string>
        {
            "Paar wurde bereits verarbeitet und nicht erneut exportiert."
        };

        if (interfaceProfile.FolderOptions.ArchiveProcessedFiles)
        {
            return ArchiveAlreadyProcessedPair(interfaceProfile, pair, timestamp, messages);
        }

        if (interfaceProfile.FolderOptions.MoveFailedFilesToErrorFolder
            && !string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ErrorFolder))
        {
            return MoveAlreadyProcessedPairToErrorFolder(interfaceProfile, pair, timestamp, messages);
        }

        if (interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing
            || interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing)
        {
            return RemoveAlreadyProcessedPair(pair, messages);
        }

        messages.Add("Keine sichere Nachlaufregel aktiv: Archivierung ist deaktiviert, kein Fehlerordner ist konfiguriert und Entfernen ist deaktiviert.");
        return new DuplicateImportFileHandlingResult(
            Status: "Bereits verarbeitet - keine sichere Nachlaufregel",
            Messages: messages,
            ArchiveResult: null);
    }

    private DuplicateImportFileHandlingResult ArchiveAlreadyProcessedPair(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime timestamp,
        List<string> messages)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.ArchiveFolder))
        {
            messages.Add("Archivierung bereits verarbeiteter Dateien fehlgeschlagen: Archivordner fehlt.");
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - Archivierung fehlgeschlagen",
                Messages: messages,
                ArchiveResult: new ProcessedFileArchiveResult(
                    ArchivedFiles: Array.Empty<string>(),
                    Issues: new[] { "Archivordner fehlt." },
                    HasErrors: true));
        }

        var archivedFiles = new List<string>();
        var issues = new List<string>();
        ArchiveKnownFile(interfaceProfile, pair.AisFile.FilePath, "AIS", timestamp.ToUniversalTime(), archivedFiles, issues);
        ArchiveKnownFile(interfaceProfile, pair.DeviceFile.FilePath, "Device", timestamp.ToUniversalTime(), archivedFiles, issues);

        var archiveResult = new ProcessedFileArchiveResult(
            ArchivedFiles: archivedFiles,
            Issues: issues,
            HasErrors: issues.Count > 0);
        if (archiveResult.HasErrors)
        {
            messages.Add("Archivierung bereits verarbeiteter Dateien fehlgeschlagen:");
            messages.AddRange(archiveResult.Issues);
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - Archivierung fehlgeschlagen",
                Messages: messages,
                ArchiveResult: archiveResult);
        }

        messages.Add("Bereits verarbeitete Importdateien wurden ins Archiv verschoben.");
        messages.AddRange(archiveResult.ArchivedFiles);
        return new DuplicateImportFileHandlingResult(
            Status: "Bereits verarbeitet - ins Archiv verschoben",
            Messages: messages,
            ArchiveResult: archiveResult);
    }

    private DuplicateImportFileHandlingResult MoveAlreadyProcessedPairToErrorFolder(
        InterfaceProfileDefinition interfaceProfile,
        PendingImportPair pair,
        DateTime timestamp,
        List<string> messages)
    {
        try
        {
            var result = _failedFileCopyService.MoveFailedFiles(
                interfaceProfile.FolderOptions.ErrorFolder,
                interfaceProfile.Metadata.Name,
                pair.AisFile.FilePath,
                pair.DeviceFile.FilePath,
                timestamp.ToUniversalTime(),
                "Bereits verarbeitet und nicht erneut exportiert.");

            if (result.HasErrors)
            {
                messages.Add("Fehlerablage bereits verarbeiteter Dateien mit Problemen abgeschlossen.");
                messages.AddRange(result.Issues);
                return new DuplicateImportFileHandlingResult(
                    Status: "Bereits verarbeitet - Fehlerablage fehlgeschlagen",
                    Messages: messages,
                    ArchiveResult: new ProcessedFileArchiveResult(
                        ArchivedFiles: result.CopiedFiles,
                        Issues: result.Issues,
                        HasErrors: true));
            }

            messages.Add("Bereits verarbeitete Importdateien wurden in den Fehlerordner verschoben.");
            messages.AddRange(result.CopiedFiles);
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - in Fehlerordner verschoben",
                Messages: messages,
                ArchiveResult: new ProcessedFileArchiveResult(
                    ArchivedFiles: result.CopiedFiles,
                    Issues: Array.Empty<string>(),
                    HasErrors: false));
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            messages.Add($"Fehlerablage bereits verarbeiteter Dateien fehlgeschlagen: {ex.Message}");
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - Fehlerablage fehlgeschlagen",
                Messages: messages,
                ArchiveResult: new ProcessedFileArchiveResult(
                    ArchivedFiles: Array.Empty<string>(),
                    Issues: new[] { ex.Message },
                    HasErrors: true));
        }
    }

    private static DuplicateImportFileHandlingResult RemoveAlreadyProcessedPair(PendingImportPair pair, List<string> messages)
    {
        var removedFiles = new List<string>();
        var issues = new List<string>();
        RemoveKnownFile(pair.AisFile.FilePath, removedFiles, issues);
        RemoveKnownFile(pair.DeviceFile.FilePath, removedFiles, issues);
        if (issues.Count > 0)
        {
            messages.Add("Entfernen bereits verarbeiteter Importdateien mit Fehlern abgeschlossen.");
            messages.AddRange(issues);
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - Entfernen fehlgeschlagen",
                Messages: messages,
                ArchiveResult: new ProcessedFileArchiveResult(
                    ArchivedFiles: removedFiles,
                    Issues: issues,
                    HasErrors: true));
        }

        messages.Add("Bereits verarbeitete Importdateien wurden aus dem Importordner entfernt.");
        messages.AddRange(removedFiles);
        return new DuplicateImportFileHandlingResult(
            Status: "Bereits verarbeitet - aus Importordner entfernt",
            Messages: messages,
            ArchiveResult: new ProcessedFileArchiveResult(
                ArchivedFiles: removedFiles,
                Issues: Array.Empty<string>(),
                HasErrors: false));
    }

    private void ArchiveKnownFile(
        InterfaceProfileDefinition interfaceProfile,
        string sourceFilePath,
        string category,
        DateTime timestampUtc,
        List<string> archivedFiles,
        List<string> issues)
    {
        try
        {
            var result = _archiveService.ArchiveProcessedFile(
                interfaceProfile.FolderOptions.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                sourceFilePath,
                category,
                timestampUtc,
                moveFile: true);
            archivedFiles.AddRange(result.ArchivedFiles);
            issues.AddRange(result.Issues);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            issues.Add(ex.Message);
        }
    }

    private static void RemoveKnownFile(string sourceFilePath, List<string> removedFiles, List<string> issues)
    {
        if (!File.Exists(sourceFilePath))
        {
            issues.Add($"Quelldatei fehlt: {sourceFilePath}");
            return;
        }

        try
        {
            File.Delete(sourceFilePath);
            removedFiles.Add(sourceFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            issues.Add($"Entfernen fehlgeschlagen für {sourceFilePath}: {ex.Message}");
        }
    }
}
