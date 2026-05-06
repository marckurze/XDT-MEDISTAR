using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class DuplicateImportFileHandler
{
    private readonly ProcessedFileArchiveService _archiveService;

    public DuplicateImportFileHandler()
        : this(new ProcessedFileArchiveService())
    {
    }

    public DuplicateImportFileHandler(ProcessedFileArchiveService archiveService)
    {
        _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
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

        var shouldArchiveAisFile = interfaceProfile.FolderOptions.ClearAisImportFolderBeforeProcessing;
        var shouldArchiveDeviceFile = interfaceProfile.FolderOptions.ClearDeviceImportFolderBeforeProcessing;
        if (!shouldArchiveAisFile && !shouldArchiveDeviceFile)
        {
            messages.Add("Bereits verarbeitete Importdateien bleiben im Importordner, da Entfernen deaktiviert ist.");
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - bleibt im Importordner",
                Messages: messages,
                ArchiveResult: null);
        }

        if (!interfaceProfile.FolderOptions.ArchiveProcessedFiles)
        {
            messages.Add("Bereits verarbeitete Importdateien bleiben im Importordner, da Archivierung deaktiviert ist.");
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - bleibt im Importordner",
                Messages: messages,
                ArchiveResult: null);
        }

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

        var moveFiles = interfaceProfile.FolderOptions.ArchiveProcessedFileMode == ArchiveProcessedFileMode.Move;
        var archivedFiles = new List<string>();
        var issues = new List<string>();
        ArchiveSelectedFileIfEnabled(
            shouldArchiveAisFile,
            interfaceProfile,
            pair.AisFile.FilePath,
            "AIS",
            timestamp.ToUniversalTime(),
            moveFiles,
            archivedFiles,
            issues);
        ArchiveSelectedFileIfEnabled(
            shouldArchiveDeviceFile,
            interfaceProfile,
            pair.DeviceFile.FilePath,
            "Device",
            timestamp.ToUniversalTime(),
            moveFiles,
            archivedFiles,
            issues);

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

        if (moveFiles)
        {
            messages.Add("Bereits verarbeitete Importdateien wurden ins Archiv verschoben.");
            messages.AddRange(archiveResult.ArchivedFiles);
            return new DuplicateImportFileHandlingResult(
                Status: "Bereits verarbeitet - ins Archiv verschoben",
                Messages: messages,
                ArchiveResult: archiveResult);
        }

        messages.Add("Bereits verarbeitete Importdateien wurden ins Archiv kopiert.");
        messages.AddRange(archiveResult.ArchivedFiles);
        return new DuplicateImportFileHandlingResult(
            Status: "Bereits verarbeitet - ins Archiv kopiert",
            Messages: messages,
            ArchiveResult: archiveResult);
    }

    private void ArchiveSelectedFileIfEnabled(
        bool enabled,
        InterfaceProfileDefinition interfaceProfile,
        string sourceFilePath,
        string category,
        DateTime timestampUtc,
        bool moveFiles,
        List<string> archivedFiles,
        List<string> issues)
    {
        if (!enabled)
        {
            return;
        }

        try
        {
            var result = _archiveService.ArchiveProcessedFile(
                interfaceProfile.FolderOptions.ArchiveFolder,
                interfaceProfile.Metadata.Name,
                sourceFilePath,
                category,
                timestampUtc,
                moveFiles);
            archivedFiles.AddRange(result.ArchivedFiles);
            issues.AddRange(result.Issues);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            issues.Add(ex.Message);
        }
    }
}
