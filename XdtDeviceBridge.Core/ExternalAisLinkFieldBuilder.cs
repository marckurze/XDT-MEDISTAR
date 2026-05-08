namespace XdtDeviceBridge.Core;

public sealed class ExternalAisLinkFieldBuilder
{
    private const string TargetFullPathPlaceholder = "{Attachment.TargetFullPath}";
    private const string FileNamePlaceholder = "{Attachment.FileName}";
    private const string ExtensionUpperPlaceholder = "{ExtensionUpper}";
    private const string ExtensionUpperWithoutDotPlaceholder = "{ExtensionUpperWithoutDot}";
    private const string OriginalExtensionPlaceholder = "{OriginalExtension}";

    public ExternalAisLinkFieldBuildResult Build(
        InterfaceFolderOptions? options,
        string? targetAttachmentFullPath,
        string? originalExtension = null)
    {
        if (options is null)
        {
            return Fail("Interface folder options must not be null.");
        }

        if (string.IsNullOrWhiteSpace(targetAttachmentFullPath))
        {
            return Fail("Attachment target full path must not be empty.");
        }

        var targetPath = targetAttachmentFullPath.Trim();
        if (!Path.IsPathFullyQualified(targetPath))
        {
            return Fail("Attachment target full path must be fully qualified.");
        }

        var fileName = Path.GetFileName(targetPath);
        var extension = NormalizeExtension(originalExtension, targetPath, withDot: true);
        var extensionUpper = extension.ToUpperInvariant();
        var extensionUpperWithoutDot = NormalizeExtension(originalExtension, targetPath, withDot: false);

        var documentName = ReplacePlaceholders(
            string.IsNullOrWhiteSpace(options.AttachmentExternalLinkDocumentName)
                ? "Datei"
                : options.AttachmentExternalLinkDocumentName,
            targetPath,
            fileName,
            extension,
            extensionUpper,
            extensionUpperWithoutDot);

        var fileFormatTemplate = string.IsNullOrWhiteSpace(options.AttachmentExternalLinkFileFormat)
            ? extensionUpperWithoutDot
            : options.AttachmentExternalLinkFileFormat;
        var fileFormat = ReplacePlaceholders(
            fileFormatTemplate,
            targetPath,
            fileName,
            extension,
            extensionUpper,
            extensionUpperWithoutDot);
        if (string.IsNullOrWhiteSpace(fileFormat))
        {
            fileFormat = extensionUpperWithoutDot;
        }

        var description = ReplacePlaceholders(
            options.AttachmentExternalLinkDescription ?? string.Empty,
            targetPath,
            fileName,
            extension,
            extensionUpper,
            extensionUpperWithoutDot);

        var fullPath = BuildFullPath(options.AttachmentExternalLinkPathTemplate, targetPath, fileName, extension, extensionUpper, extensionUpperWithoutDot);

        return new ExternalAisLinkFieldBuildResult(
            Success: true,
            FieldSet: new ExternalAisLinkFieldSet(
                DocumentName: documentName,
                FileFormat: fileFormat,
                Description: description,
                FullPath: fullPath),
            ErrorMessage: null);
    }

    private static string BuildFullPath(
        string? pathTemplate,
        string targetPath,
        string fileName,
        string originalExtension,
        string extensionUpper,
        string extensionUpperWithoutDot)
    {
        if (string.IsNullOrWhiteSpace(pathTemplate))
        {
            return targetPath;
        }

        if (!pathTemplate.Contains(TargetFullPathPlaceholder, StringComparison.Ordinal))
        {
            return targetPath;
        }

        var candidate = ReplacePlaceholders(pathTemplate, targetPath, fileName, originalExtension, extensionUpper, extensionUpperWithoutDot);
        return Path.IsPathFullyQualified(candidate) ? candidate : targetPath;
    }

    private static string ReplacePlaceholders(
        string template,
        string targetPath,
        string fileName,
        string originalExtension,
        string extensionUpper,
        string extensionUpperWithoutDot)
    {
        return template
            .Replace(TargetFullPathPlaceholder, targetPath, StringComparison.Ordinal)
            .Replace(FileNamePlaceholder, fileName, StringComparison.Ordinal)
            .Replace(ExtensionUpperWithoutDotPlaceholder, extensionUpperWithoutDot, StringComparison.Ordinal)
            .Replace(ExtensionUpperPlaceholder, extensionUpper, StringComparison.Ordinal)
            .Replace(OriginalExtensionPlaceholder, originalExtension, StringComparison.Ordinal)
            .Trim();
    }

    private static string NormalizeExtension(string? originalExtension, string targetPath, bool withDot)
    {
        var extension = string.IsNullOrWhiteSpace(originalExtension)
            ? Path.GetExtension(targetPath)
            : originalExtension.Trim();

        var pathExtension = Path.GetExtension(extension);
        if (!string.IsNullOrWhiteSpace(pathExtension))
        {
            extension = pathExtension;
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        if (!extension.StartsWith(".", StringComparison.Ordinal))
        {
            extension = "." + extension;
        }

        var extensionWithoutDot = extension.TrimStart('.').ToUpperInvariant();
        extensionWithoutDot = extensionWithoutDot switch
        {
            "JPEG" => "JPG",
            _ => extensionWithoutDot
        };

        return withDot ? "." + extensionWithoutDot.ToLowerInvariant() : extensionWithoutDot;
    }

    private static ExternalAisLinkFieldBuildResult Fail(string message)
    {
        return new ExternalAisLinkFieldBuildResult(
            Success: false,
            FieldSet: null,
            ErrorMessage: message);
    }
}
