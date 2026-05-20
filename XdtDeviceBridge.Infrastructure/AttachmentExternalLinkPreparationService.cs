using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentExternalLinkPreparationService : IAttachmentExternalLinkPreparationService
{
    private readonly AttachmentFileNameBuilder _fileNameBuilder;
    private readonly IAttachmentTransferService _transferService;
    private readonly ExternalAisLinkFieldBuilder _linkFieldBuilder;
    private readonly ExternalAisLinkXdtFieldAdapter _fieldAdapter;

    public AttachmentExternalLinkPreparationService()
        : this(
            new AttachmentFileNameBuilder(),
            new AttachmentTransferService(),
            new ExternalAisLinkFieldBuilder(),
            new ExternalAisLinkXdtFieldAdapter())
    {
    }

    public AttachmentExternalLinkPreparationService(
        AttachmentFileNameBuilder fileNameBuilder,
        IAttachmentTransferService transferService,
        ExternalAisLinkFieldBuilder linkFieldBuilder,
        ExternalAisLinkXdtFieldAdapter fieldAdapter)
    {
        _fileNameBuilder = fileNameBuilder ?? throw new ArgumentNullException(nameof(fileNameBuilder));
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _linkFieldBuilder = linkFieldBuilder ?? throw new ArgumentNullException(nameof(linkFieldBuilder));
        _fieldAdapter = fieldAdapter ?? throw new ArgumentNullException(nameof(fieldAdapter));
    }

    public AttachmentExternalLinkPreparationResult Prepare(AttachmentExternalLinkPreparationRequest? request)
    {
        if (request is null)
        {
            return Fail(string.Empty, AttachmentTransferMode.Move, "Attachment preparation request must not be null.");
        }

        if (request.FolderOptions is null)
        {
            return Fail(request.SourceAttachmentPath, AttachmentTransferMode.Move, "Interface folder options must not be null.");
        }

        var transferMode = request.FolderOptions.AttachmentTransferMode;
        if (string.IsNullOrWhiteSpace(request.SourceAttachmentPath))
        {
            return Fail(request.SourceAttachmentPath, transferMode, "Attachment source path must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.FolderOptions.AttachmentExportFolder))
        {
            return Fail(request.SourceAttachmentPath, transferMode, "XDT attachment export folder must not be empty.");
        }

        if (request.IsSourceStable == false)
        {
            return Fail(request.SourceAttachmentPath, transferMode, "XDT attachment source file is not stable yet.");
        }

        var originalExtension = string.IsNullOrWhiteSpace(request.OriginalExtension)
            ? Path.GetExtension(request.SourceAttachmentPath)
            : request.OriginalExtension;
        var desiredFileName = _fileNameBuilder.Build(
            request.FolderOptions.AttachmentFileNameTemplate,
            request.Patient,
            request.ProcessingTimestamp,
            originalExtension);

        var transferResult = _transferService.Transfer(
            request.SourceAttachmentPath,
            request.FolderOptions.AttachmentExportFolder,
            desiredFileName,
            transferMode);
        if (!transferResult.Success)
        {
            return Fail(request.SourceAttachmentPath, transferMode, transferResult.ErrorMessage ?? "Attachment transfer failed.");
        }

        if (string.IsNullOrWhiteSpace(transferResult.TargetPath))
        {
            return Fail(request.SourceAttachmentPath, transferMode, "Attachment transfer did not return a final target path.");
        }

        var finalExtension = Path.GetExtension(transferResult.TargetPath);
        var linkOptions = string.IsNullOrWhiteSpace(request.DescriptionOverride)
            ? request.FolderOptions
            : request.FolderOptions with
            {
                AttachmentExternalLinkDescription = NormalizeDescription(request.DescriptionOverride)
            };
        var linkFieldResult = _linkFieldBuilder.Build(
            linkOptions,
            transferResult.TargetPath,
            string.IsNullOrWhiteSpace(finalExtension) ? originalExtension : finalExtension);
        if (!linkFieldResult.Success || linkFieldResult.FieldSet is null)
        {
            return Fail(request.SourceAttachmentPath, transferMode, linkFieldResult.ErrorMessage ?? "External AIS link fields could not be built.");
        }

        var adapterResult = _fieldAdapter.Adapt(linkFieldResult.FieldSet);
        if (!adapterResult.Success)
        {
            return Fail(request.SourceAttachmentPath, transferMode, adapterResult.ErrorMessage ?? "External AIS link fields could not be adapted to XDT fields.");
        }

        return new AttachmentExternalLinkPreparationResult(
            Success: true,
            SourcePath: request.SourceAttachmentPath,
            TargetPath: transferResult.TargetPath,
            TargetFileName: transferResult.FileName,
            TransferMode: transferMode,
            ExternalAisLinkFieldSet: linkFieldResult.FieldSet,
            ExportFields: adapterResult.Fields,
            ErrorMessage: null);
    }

    private static string NormalizeDescription(string value)
    {
        return string.Join(
            " / ",
            value
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static AttachmentExternalLinkPreparationResult Fail(
        string? sourcePath,
        AttachmentTransferMode transferMode,
        string message)
    {
        return new AttachmentExternalLinkPreparationResult(
            Success: false,
            SourcePath: sourcePath ?? string.Empty,
            TargetPath: null,
            TargetFileName: null,
            TransferMode: transferMode,
            ExternalAisLinkFieldSet: null,
            ExportFields: Array.Empty<ExportFieldRecord>(),
            ErrorMessage: message);
    }
}
