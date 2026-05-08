using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface IAttachmentTransferService
{
    AttachmentTransferResult Transfer(
        string sourceAttachmentPath,
        string targetFolder,
        string desiredFileName,
        AttachmentTransferMode transferMode);
}
