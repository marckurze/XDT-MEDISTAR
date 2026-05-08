using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentImportFolderDiagnosticService
{
    private readonly IAttachmentImportFolderScannerService _scannerService;

    public AttachmentImportFolderDiagnosticService()
        : this(new AttachmentImportFolderScannerService())
    {
    }

    public AttachmentImportFolderDiagnosticService(IAttachmentImportFolderScannerService scannerService)
    {
        _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
    }

    public AttachmentImportFolderDiagnosticResult Scan(InterfaceProfileDefinition? interfaceProfile)
    {
        if (interfaceProfile is null)
        {
            return Fail("Kein Schnittstellenprofil für den XDT-Anhang-Test ausgewählt.");
        }

        var importFolder = interfaceProfile.FolderOptions.AttachmentImportFolder;
        var exportFolder = interfaceProfile.FolderOptions.AttachmentExportFolder;
        if (string.IsNullOrWhiteSpace(importFolder))
        {
            return Fail(
                "XDT-Anhang Importordner ist nicht gesetzt.",
                interfaceProfile.Metadata.Name,
                importFolder,
                exportFolder);
        }

        var scanResult = _scannerService.Scan(interfaceProfile.FolderOptions);
        if (!scanResult.Success)
        {
            return new AttachmentImportFolderDiagnosticResult(
                Success: false,
                Message: scanResult.ErrorMessage ?? "XDT-Anhang Importordner konnte nicht eingelesen werden.",
                InterfaceProfileName: interfaceProfile.Metadata.Name,
                ImportFolder: importFolder,
                ExportFolder: exportFolder,
                Candidates: Array.Empty<AttachmentImportCandidateDisplayRow>());
        }

        var rows = scanResult.Candidates
            .Select(CreateDisplayRow)
            .ToList();

        return new AttachmentImportFolderDiagnosticResult(
            Success: true,
            Message: CreateStatusMessage(rows),
            InterfaceProfileName: interfaceProfile.Metadata.Name,
            ImportFolder: importFolder,
            ExportFolder: exportFolder,
            Candidates: rows);
    }

    private static AttachmentImportCandidateDisplayRow CreateDisplayRow(AttachmentImportFileCandidate candidate)
    {
        var status = candidate.IsSupported
            ? candidate.IsStable
                ? "Unterstützt, stabil"
                : $"Unterstützt, {candidate.StableStatus}"
            : $"Nicht unterstützt: {candidate.ErrorMessage ?? "Dateityp nicht unterstützt."}";

        return new AttachmentImportCandidateDisplayRow(
            FileName: candidate.FileName,
            Extension: candidate.Extension,
            FullPath: candidate.FullPath,
            SizeBytes: candidate.SizeBytes,
            LastWriteTimeUtc: candidate.LastWriteTimeUtc,
            IsSupported: candidate.IsSupported,
            IsStable: candidate.IsStable,
            Status: status);
    }

    private static string CreateStatusMessage(IReadOnlyList<AttachmentImportCandidateDisplayRow> rows)
    {
        if (rows.Count == 0)
        {
            return "Keine Dateien im XDT-Anhang Importordner gefunden.";
        }

        var supportedCount = rows.Count(row => row.IsSupported);
        if (supportedCount == 0)
        {
            return $"{rows.Count} Dateien gefunden, davon 0 unterstützt. Keine unterstützten XDT-Anhänge gefunden. Nicht unterstützte Dateien wurden nicht verarbeitet.";
        }

        var message = $"{rows.Count} Dateien gefunden, davon {supportedCount} unterstützt.";
        return supportedCount == rows.Count
            ? message
            : $"{message} Nicht unterstützte Dateien wurden nicht verarbeitet.";
    }

    private static AttachmentImportFolderDiagnosticResult Fail(
        string message,
        string interfaceProfileName = "",
        string importFolder = "",
        string exportFolder = "")
    {
        return new AttachmentImportFolderDiagnosticResult(
            Success: false,
            Message: message,
            InterfaceProfileName: interfaceProfileName,
            ImportFolder: importFolder,
            ExportFolder: exportFolder,
            Candidates: Array.Empty<AttachmentImportCandidateDisplayRow>());
    }
}
