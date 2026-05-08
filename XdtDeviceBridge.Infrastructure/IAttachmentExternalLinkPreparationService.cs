namespace XdtDeviceBridge.Infrastructure;

public interface IAttachmentExternalLinkPreparationService
{
    AttachmentExternalLinkPreparationResult Prepare(AttachmentExternalLinkPreparationRequest? request);
}
