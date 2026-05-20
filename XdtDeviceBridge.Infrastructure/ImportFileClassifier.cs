namespace XdtDeviceBridge.Infrastructure;

public sealed class ImportFileClassifier
{
    public ImportFileClassificationResult Classify(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var (kind, message) = extension switch
        {
            ".gdt" => (ImportFileKind.AisGdt, "AIS-GDT-Datei erkannt."),
            ".xdt" => (ImportFileKind.AisXdt, "AIS-XDT-Datei erkannt."),
            ".xml" => (ImportFileKind.DeviceXml, "XML-Gerätedatei erkannt."),
            ".txt" => (ImportFileKind.DeviceText, "Text-Gerätedatei erkannt."),
            ".csv" => (ImportFileKind.DeviceCsv, "CSV-Gerätedatei erkannt."),
            ".jpg" or ".jpeg" or ".png" => (ImportFileKind.AttachmentImage, "Bild-Anhang erkannt."),
            ".pdf" => (ImportFileKind.AttachmentPdf, "PDF-Anhang erkannt."),
            ".tif" or ".tiff" or ".dcm" or ".mp4" or ".mp3" or ".wav" => (ImportFileKind.AttachmentFile, "Anhangdatei erkannt."),
            _ => (ImportFileKind.Unknown, "Dateityp anhand der Endung nicht bekannt.")
        };

        return new ImportFileClassificationResult(
            FilePath: filePath,
            FileName: fileName,
            Extension: extension,
            Kind: kind,
            Message: message);
    }
}
