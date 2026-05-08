namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentImportFolderDiagnosticResult(
    bool Success,
    string Message,
    string InterfaceProfileName,
    string ImportFolder,
    string ExportFolder,
    IReadOnlyList<AttachmentImportCandidateDisplayRow> Candidates);
