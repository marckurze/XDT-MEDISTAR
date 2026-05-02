using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class XdtExportBuilder
{
    private static readonly Regex FieldCodePattern = new("^\\d{4}$", RegexOptions.Compiled);

    public XdtExportResult Build(IEnumerable<ExportFieldRecord> records)
    {
        var issues = new List<XdtExportIssue>();
        var lines = new List<string>();

        foreach (var record in records.OrderBy(r => r.SortOrder))
        {
            if (string.IsNullOrWhiteSpace(record.FieldCode))
            {
                issues.Add(new XdtExportIssue(XdtExportIssueSeverity.Error, "FieldCode is empty.", record.FieldCode, record.Value));
                continue;
            }

            if (!FieldCodePattern.IsMatch(record.FieldCode))
            {
                issues.Add(new XdtExportIssue(XdtExportIssueSeverity.Error, "FieldCode must be 4 numeric characters.", record.FieldCode, record.Value));
                continue;
            }

            var value = record.Value ?? string.Empty;

            // Length rule for v1 includes CR/LF: 3 (LLL) + 4 (FFFF) + value length + 2 (CRLF)
            var declaredLength = 3 + 4 + value.Length + 2;
            var lengthPrefix = declaredLength.ToString("D3");
            lines.Add($"{lengthPrefix}{record.FieldCode}{value}\r\n");
        }

        return new XdtExportResult(string.Concat(lines), issues);
    }

    public byte[] BuildBytesWindows1252(IEnumerable<ExportFieldRecord> records)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var result = Build(records);
        var encoding = Encoding.GetEncoding(1252);
        return encoding.GetBytes(result.Content);
    }
}
