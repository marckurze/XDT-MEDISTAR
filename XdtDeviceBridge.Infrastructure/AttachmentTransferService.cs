using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentTransferService : IAttachmentTransferService
{
    private readonly AttachmentFileNameBuilder _fileNameBuilder;
    private readonly FolderSafetyValidator _folderSafetyValidator;

    public AttachmentTransferService()
        : this(new AttachmentFileNameBuilder(), new FolderSafetyValidator())
    {
    }

    public AttachmentTransferService(
        AttachmentFileNameBuilder fileNameBuilder,
        FolderSafetyValidator folderSafetyValidator)
    {
        _fileNameBuilder = fileNameBuilder ?? throw new ArgumentNullException(nameof(fileNameBuilder));
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
    }

    public AttachmentTransferResult Transfer(
        string sourceAttachmentPath,
        string targetFolder,
        string desiredFileName,
        AttachmentTransferMode transferMode)
    {
        if (!Enum.IsDefined(transferMode))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment transfer mode is invalid.");
        }

        if (string.IsNullOrWhiteSpace(sourceAttachmentPath))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment source path must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(targetFolder))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment target folder must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(desiredFileName))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment target file name must not be empty.");
        }

        if (Directory.Exists(sourceAttachmentPath))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment source path must be a file, not a folder.");
        }

        if (!File.Exists(sourceAttachmentPath))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment source file does not exist.");
        }

        var safetyResult = _folderSafetyValidator.ValidateFolderForCleanup(targetFolder);
        var safetyError = safetyResult.Issues.FirstOrDefault(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error);
        if (safetyError is not null)
        {
            return Fail(sourceAttachmentPath, transferMode, $"Attachment target folder is unsafe: {safetyError.Message}");
        }

        if (!Directory.Exists(targetFolder))
        {
            return Fail(sourceAttachmentPath, transferMode, "Attachment target folder does not exist.");
        }

        try
        {
            var targetFileName = _fileNameBuilder.BuildUniqueFileName(targetFolder, desiredFileName);
            var targetPath = Path.Combine(targetFolder, targetFileName);

            if (transferMode == AttachmentTransferMode.Move)
            {
                File.Move(sourceAttachmentPath, targetPath);
                return new AttachmentTransferResult(
                    Success: true,
                    SourcePath: sourceAttachmentPath,
                    TargetPath: targetPath,
                    FileName: targetFileName,
                    TransferMode: transferMode,
                    ErrorMessage: null,
                    WasCopied: false,
                    WasMoved: true);
            }

            File.Copy(sourceAttachmentPath, targetPath, overwrite: false);
            return new AttachmentTransferResult(
                Success: true,
                SourcePath: sourceAttachmentPath,
                TargetPath: targetPath,
                FileName: targetFileName,
                TransferMode: transferMode,
                ErrorMessage: null,
                WasCopied: true,
                WasMoved: false);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return Fail(sourceAttachmentPath, transferMode, $"Attachment transfer failed: {ex.Message}");
        }
    }

    private static AttachmentTransferResult Fail(
        string? sourcePath,
        AttachmentTransferMode transferMode,
        string message)
    {
        return new AttachmentTransferResult(
            Success: false,
            SourcePath: sourcePath ?? string.Empty,
            TargetPath: null,
            FileName: null,
            TransferMode: transferMode,
            ErrorMessage: message,
            WasCopied: false,
            WasMoved: false);
    }
}
