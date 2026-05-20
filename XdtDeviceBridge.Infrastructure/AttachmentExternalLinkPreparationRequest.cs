using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentExternalLinkPreparationRequest(
    InterfaceFolderOptions FolderOptions,
    string SourceAttachmentPath,
    PatientData? Patient,
    DateTime ProcessingTimestamp,
    string? OriginalExtension = null,
    bool? IsSourceStable = null,
    string? DescriptionOverride = null);
