using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public interface IAttachmentImportFolderScannerService
{
    AttachmentImportFolderScanResult Scan(InterfaceFolderOptions folderOptions);

    AttachmentImportFolderScanResult Scan(string attachmentImportFolder);
}
