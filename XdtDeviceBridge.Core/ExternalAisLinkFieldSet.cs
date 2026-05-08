namespace XdtDeviceBridge.Core;

public sealed record ExternalAisLinkFieldSet(
    string DocumentName,
    string FileFormat,
    string Description,
    string FullPath)
{
    public bool HasDocumentName => !string.IsNullOrWhiteSpace(DocumentName);

    public bool HasFileFormat => !string.IsNullOrWhiteSpace(FileFormat);

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool HasFullPath => !string.IsNullOrWhiteSpace(FullPath);
}
