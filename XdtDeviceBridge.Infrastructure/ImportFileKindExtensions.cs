namespace XdtDeviceBridge.Infrastructure;

public static class ImportFileKindExtensions
{
    public static bool IsAisImportFile(this ImportFileKind kind)
    {
        return kind is ImportFileKind.AisGdt or ImportFileKind.AisXdt;
    }

    public static bool IsMeasurementDeviceFile(this ImportFileKind kind)
    {
        return kind is ImportFileKind.DeviceXml
            or ImportFileKind.DeviceText
            or ImportFileKind.DeviceCsv;
    }

    public static bool IsAttachmentImportFile(this ImportFileKind kind)
    {
        return kind is ImportFileKind.AttachmentImage
            or ImportFileKind.AttachmentPdf
            or ImportFileKind.AttachmentFile;
    }

    public static bool IsDeviceImportFile(this ImportFileKind kind, bool includeAttachmentFiles)
    {
        return kind.IsMeasurementDeviceFile()
            || (includeAttachmentFiles && kind.IsAttachmentImportFile());
    }
}
