using System.Text;
using System.Text.RegularExpressions;

namespace XdtDeviceBridge.Core;

public sealed class GdtParser
{
    private static readonly Regex FieldCodePattern = new("^\\d{4}$", RegexOptions.Compiled);

    public GdtParseResult ParseFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be empty.", nameof(path));
        }

        var bytes = File.ReadAllBytes(path);
        var encoding = DetectEncoding(bytes);
        var content = encoding.GetString(bytes);

        var records = new List<FieldRecord>();
        var issues = new List<GdtParseIssue>();

        using var reader = new StringReader(content);
        var lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Length < 7)
            {
                issues.Add(new GdtParseIssue(lineNumber, GdtParseIssueSeverity.Error, "Line is shorter than 7 characters.", line));
                continue;
            }

            var declaredLengthRaw = line[..3];
            var fieldCode = line.Substring(3, 4);
            var value = line[7..];

            if (!int.TryParse(declaredLengthRaw, out var declaredLength))
            {
                issues.Add(new GdtParseIssue(lineNumber, GdtParseIssueSeverity.Error, "Declared length is not numeric.", line));
                continue;
            }

            if (!FieldCodePattern.IsMatch(fieldCode))
            {
                issues.Add(new GdtParseIssue(lineNumber, GdtParseIssueSeverity.Error, "Field code must contain exactly 4 digits.", line));
                continue;
            }

            var actualLength = line.Length;
            var isLengthValid = declaredLength == actualLength;

            if (!isLengthValid)
            {
                issues.Add(new GdtParseIssue(lineNumber, GdtParseIssueSeverity.Warning, "Declared length does not match actual line length.", line));
            }

            records.Add(new FieldRecord(
                FieldCode: fieldCode,
                Value: value,
                LineNumber: lineNumber,
                DeclaredLength: declaredLength,
                ActualLength: actualLength,
                IsLengthValid: isLengthValid));
        }

        return new GdtParseResult(records, issues);
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }

        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        try
        {
            utf8.GetString(bytes);
            return utf8;
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding("ISO-8859-1");
        }
    }
}
