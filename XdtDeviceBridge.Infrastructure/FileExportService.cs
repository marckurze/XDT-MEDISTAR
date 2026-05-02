using System.Text;

namespace XdtDeviceBridge.Infrastructure;

public sealed class FileExportService
{
    public FileExportResult Export(string exportFolder, string fileName, string content, string encodingName)
    {
        var issues = new List<FileExportIssue>();

        if (string.IsNullOrWhiteSpace(exportFolder))
        {
            issues.Add(new FileExportIssue(FileExportIssueSeverity.Error, "Export folder is empty.", null));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            issues.Add(new FileExportIssue(FileExportIssueSeverity.Error, "File name is empty.", null));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            issues.Add(new FileExportIssue(FileExportIssueSeverity.Error, "Content is empty.", null));
        }

        if (issues.Any(i => i.Severity == FileExportIssueSeverity.Error))
        {
            return new FileExportResult(null, issues);
        }

        try
        {
            Directory.CreateDirectory(exportFolder);
            var encoding = ResolveEncoding(encodingName);
            var fullPath = GetNextAvailableFilePath(exportFolder, fileName);
            File.WriteAllText(fullPath, content, encoding);
            return new FileExportResult(fullPath, issues);
        }
        catch (Exception ex)
        {
            issues.Add(new FileExportIssue(FileExportIssueSeverity.Error, ex.Message, null));
            return new FileExportResult(null, issues);
        }
    }

    private static Encoding ResolveEncoding(string encodingName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (string.Equals(encodingName, "Windows-1252", StringComparison.OrdinalIgnoreCase)
            || string.Equals(encodingName, "cp1252", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding(1252);
        }

        if (string.Equals(encodingName, "UTF-8", StringComparison.OrdinalIgnoreCase)
            || string.Equals(encodingName, "utf8", StringComparison.OrdinalIgnoreCase))
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }

        return Encoding.GetEncoding(encodingName);
    }

    private static string GetNextAvailableFilePath(string exportFolder, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);

        var candidate = Path.Combine(exportFolder, fileName);
        var suffix = 0;

        while (File.Exists(candidate))
        {
            suffix++;
            var nextName = $"{baseName}_{suffix}{extension}";
            candidate = Path.Combine(exportFolder, nextName);
        }

        return candidate;
    }
}
