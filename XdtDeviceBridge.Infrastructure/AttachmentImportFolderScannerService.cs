using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentImportFolderScannerService : IAttachmentImportFolderScannerService
{
    private static readonly StringComparer FilePathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static readonly IReadOnlySet<string> SupportedExtensions = new HashSet<string>(
        new[]
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".tif",
            ".tiff",
            ".dcm",
            ".txt"
        },
        StringComparer.OrdinalIgnoreCase);

    private readonly FolderSafetyValidator _folderSafetyValidator;

    public AttachmentImportFolderScannerService()
        : this(new FolderSafetyValidator())
    {
    }

    public AttachmentImportFolderScannerService(FolderSafetyValidator folderSafetyValidator)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
    }

    public AttachmentImportFolderScanResult Scan(InterfaceFolderOptions folderOptions)
    {
        ArgumentNullException.ThrowIfNull(folderOptions);

        return Scan(folderOptions.AttachmentImportFolder);
    }

    public AttachmentImportFolderScanResult Scan(string attachmentImportFolder)
    {
        if (string.IsNullOrWhiteSpace(attachmentImportFolder))
        {
            return Fail(attachmentImportFolder, "XDT-Anhang Importordner ist nicht konfiguriert.");
        }

        var safetyResult = _folderSafetyValidator.ValidateFolderForCleanup(attachmentImportFolder);
        var safetyError = safetyResult.Issues.FirstOrDefault(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error);
        if (safetyError is not null)
        {
            return Fail(attachmentImportFolder, $"XDT-Anhang Importordner ist unsicher: {safetyError.Message}");
        }

        if (!Directory.Exists(attachmentImportFolder))
        {
            return Fail(attachmentImportFolder, "XDT-Anhang Importordner existiert nicht.");
        }

        try
        {
            var directoryInfo = new DirectoryInfo(attachmentImportFolder);
            var candidates = directoryInfo
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Select(CreateCandidate)
                .OrderBy(candidate => candidate.LastWriteTimeUtc)
                .ThenBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.FullPath, FilePathComparer)
                .ToList();

            return new AttachmentImportFolderScanResult(
                Success: true,
                ScannedFolder: attachmentImportFolder,
                Candidates: candidates,
                ErrorMessage: null);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException
            or IOException
            or ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return Fail(attachmentImportFolder, $"XDT-Anhang Importordner konnte nicht gelesen werden: {ex.Message}");
        }
    }

    private static AttachmentImportFileCandidate CreateCandidate(FileInfo file)
    {
        var extension = file.Extension.ToLowerInvariant();
        var isSupported = SupportedExtensions.Contains(extension);

        return new AttachmentImportFileCandidate(
            FileName: file.Name,
            Extension: extension,
            FullPath: file.FullName,
            SizeBytes: file.Length,
            LastWriteTimeUtc: file.LastWriteTimeUtc,
            IsSupported: isSupported,
            StableStatus: "Nicht geprüft.",
            ErrorMessage: isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.");
    }

    private static AttachmentImportFolderScanResult Fail(string? folder, string message)
    {
        return new AttachmentImportFolderScanResult(
            Success: false,
            ScannedFolder: folder ?? string.Empty,
            Candidates: Array.Empty<AttachmentImportFileCandidate>(),
            ErrorMessage: message);
    }
}
