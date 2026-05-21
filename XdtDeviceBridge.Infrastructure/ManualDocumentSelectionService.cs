namespace XdtDeviceBridge.Infrastructure;

public sealed class ManualDocumentSelectionService
{
    private static readonly StringComparer FingerprintComparer = StringComparer.OrdinalIgnoreCase;

    public ManualDocumentSelectionResult AddFiles(
        IEnumerable<string> paths,
        IEnumerable<AttachmentImportFileCandidate>? existingFiles = null)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var knownFingerprints = new HashSet<string>(FingerprintComparer);
        if (existingFiles is not null)
        {
            foreach (var existingFile in existingFiles)
            {
                knownFingerprints.Add(AttachmentImportFileFingerprint.Create(existingFile));
            }
        }

        var acceptedFiles = new List<AttachmentImportFileCandidate>();
        var rejectedMessages = new List<string>();
        foreach (var rawPath in paths.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            var path = rawPath.Trim();
            if (Directory.Exists(path))
            {
                rejectedMessages.Add($"{Path.GetFileName(path)}: Ordner werden nicht übernommen.");
                continue;
            }

            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(path);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                rejectedMessages.Add($"{path}: Datei konnte nicht gelesen werden ({ex.Message}).");
                continue;
            }

            if (!fileInfo.Exists)
            {
                rejectedMessages.Add($"{path}: Datei wurde nicht gefunden.");
                continue;
            }

            var extension = SupportedAttachmentFileFormats.Normalize(fileInfo.Extension);
            if (!SupportedAttachmentFileFormats.IsSupported(extension))
            {
                rejectedMessages.Add($"{fileInfo.Name}: Dateityp wird für Dokumentanhänge nicht unterstützt.");
                continue;
            }

            var candidate = new AttachmentImportFileCandidate(
                FileName: fileInfo.Name,
                Extension: extension,
                FullPath: fileInfo.FullName,
                SizeBytes: fileInfo.Length,
                LastWriteTimeUtc: fileInfo.LastWriteTimeUtc,
                IsSupported: true,
                StableStatus: "Manuell ausgewählt.",
                ErrorMessage: null,
                IsStable: true);
            var fingerprint = AttachmentImportFileFingerprint.Create(candidate);
            if (!knownFingerprints.Add(fingerprint))
            {
                rejectedMessages.Add($"{fileInfo.Name}: Datei ist bereits in der Liste.");
                continue;
            }

            acceptedFiles.Add(candidate);
        }

        return new ManualDocumentSelectionResult(acceptedFiles, rejectedMessages);
    }
}
