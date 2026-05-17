namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileInputFolderResetResult(
    int CandidateFolderCount,
    int ProcessedFolderCount,
    int DeletedFileCount,
    int FailedFileCount,
    int MissingFolderCount,
    int SkippedFolderCount,
    IReadOnlyList<string> ProcessedFolders,
    IReadOnlyList<string> SkippedFolders,
    IReadOnlyList<string> FailedFiles,
    IReadOnlyList<string> Messages)
{
    public bool FileOperationsPerformed => DeletedFileCount > 0 || FailedFileCount > 0;

    public string SummaryMessage
    {
        get
        {
            if (DeletedFileCount > 0 && FailedFileCount > 0)
            {
                return $"Vorgang zurückgesetzt. {DeletedFileCount} Datei(en) gelöscht, {FailedFileCount} Datei(en) konnte(n) nicht gelöscht werden.";
            }

            if (DeletedFileCount > 0)
            {
                return $"Vorgang zurückgesetzt. Eingangsordner geleert: {DeletedFileCount} Datei(en) gelöscht.";
            }

            if (FailedFileCount > 0)
            {
                return $"Vorgang zurückgesetzt. {FailedFileCount} Datei(en) konnte(n) nicht gelöscht werden.";
            }

            return "Vorgang zurückgesetzt. Keine Dateien in den Eingangsordnern gefunden.";
        }
    }
}
