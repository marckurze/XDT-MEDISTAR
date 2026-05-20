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
            ".txt",
            ".xml",
            ".mp4",
            ".mp3",
            ".wav"
        },
        StringComparer.OrdinalIgnoreCase);

    private readonly FolderSafetyValidator _folderSafetyValidator;
    private readonly FileStabilityService _fileStabilityService;

    public AttachmentImportFolderScannerService()
        : this(new FolderSafetyValidator(), new FileStabilityService())
    {
    }

    public AttachmentImportFolderScannerService(
        FolderSafetyValidator folderSafetyValidator,
        FileStabilityService? fileStabilityService = null)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
        _fileStabilityService = fileStabilityService ?? new FileStabilityService();
    }

    public AttachmentImportFolderScanResult Scan(InterfaceFolderOptions folderOptions)
    {
        ArgumentNullException.ThrowIfNull(folderOptions);

        var stabilityDuration = TimeSpan.FromSeconds(Math.Max(0, folderOptions.AttachmentFileStabilityWaitSeconds));
        return Scan(folderOptions.AttachmentImportFolder, stabilityDuration);
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
                .Select(file => CreateCandidate(file, stabilityDuration: null))
                .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.LastWriteTimeUtc)
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

    private AttachmentImportFolderScanResult Scan(string attachmentImportFolder, TimeSpan stabilityDuration)
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
                .Select(file => CreateCandidate(file, stabilityDuration))
                .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.LastWriteTimeUtc)
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

    private AttachmentImportFileCandidate CreateCandidate(FileInfo file, TimeSpan? stabilityDuration)
    {
        var extension = file.Extension.ToLowerInvariant();
        var isSupported = SupportedExtensions.Contains(extension);
        var isStable = false;
        var stableStatus = "Nicht geprüft.";
        string? errorMessage = isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.";

        if (isSupported && stabilityDuration is not null)
        {
            try
            {
                var stabilityResult = _fileStabilityService
                    .CheckAsync(file.FullName, stabilityDuration.Value)
                    .GetAwaiter()
                    .GetResult();
                isStable = stabilityResult.IsStable;
                stableStatus = stabilityResult.IsStable
                    ? "Stabil."
                    : $"Noch nicht stabil: {stabilityResult.Message}";
                if (!stabilityResult.IsReadable)
                {
                    errorMessage = stabilityResult.Message;
                }
            }
            catch (Exception ex) when (ex is IOException
                or UnauthorizedAccessException
                or ArgumentException
                or NotSupportedException
                or PathTooLongException
                or InvalidOperationException)
            {
                stableStatus = $"Stabilitätsprüfung fehlgeschlagen: {ex.Message}";
                errorMessage = stableStatus;
            }
        }

        return new AttachmentImportFileCandidate(
            FileName: file.Name,
            Extension: extension,
            FullPath: file.FullName,
            SizeBytes: file.Length,
            LastWriteTimeUtc: file.LastWriteTimeUtc,
            IsSupported: isSupported,
            StableStatus: stableStatus,
            ErrorMessage: errorMessage,
            IsStable: isStable);
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
