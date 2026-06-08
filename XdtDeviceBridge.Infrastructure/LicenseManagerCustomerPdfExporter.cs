using System.Globalization;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerCustomerPdfExporter
{
    private static readonly Encoding PdfEncoding = Encoding.ASCII;

    public void ExportCustomers(
        string filePath,
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customers);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = BuildLines(customers, pricePerDeviceNet, createdAt).ToArray();
        var document = BuildPdf(lines);
        File.WriteAllBytes(filePath, document);
    }

    private static IEnumerable<string> BuildLines(
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        yield return "XDTBox Kunden- und Lizenzuebersicht";
        yield return $"Erstellt am: {createdAt.ToString("dd.MM.yyyy HH:mm", culture)}";
        yield return $"Einzelpreis netto pro Geraeteanbindung: {pricePerDeviceNet.ToString("N2", culture)} EUR";
        yield return string.Empty;
        yield return "Kdnr | Praxis/Firma | Rechnungsmail | SEPA | IBAN | BIC | Kontoinhaber | Anzahl | Netto";

        foreach (var customer in customers.OrderBy(customer => customer.CustomerName, StringComparer.CurrentCultureIgnoreCase))
        {
            var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.ActiveLicensedDeviceCount, pricePerDeviceNet);
            yield return string.Join(" | ",
                Truncate(customer.CustomerNumber ?? "-", 12),
                Truncate(customer.CustomerName, 28),
                Truncate(customer.InvoiceEmail ?? customer.Email ?? "-", 26),
                customer.SepaDirectDebitConsent ? "Ja" : "Nein",
                Truncate(customer.Iban ?? "-", 24),
                Truncate(customer.Bic ?? "-", 12),
                Truncate(customer.AccountHolder ?? "-", 22),
                customer.ActiveLicensedDeviceCount.ToString(CultureInfo.InvariantCulture),
                total.ToString("N2", culture) + " EUR");
        }

        yield return string.Empty;
        yield return $"Gesamtanzahl Geraete: {customers.Sum(customer => customer.ActiveLicensedDeviceCount)}";
        yield return $"Gesamtkosten netto: {customers.Sum(customer => LicenseManagerCostCalculator.CalculateNetTotal(customer.ActiveLicensedDeviceCount, pricePerDeviceNet)).ToString("N2", culture)} EUR";
    }

    private static byte[] BuildPdf(IReadOnlyList<string> lines)
    {
        const int linesPerPage = 42;
        var pages = lines.Chunk(linesPerPage).ToArray();
        var objects = new List<string>();

        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
        var pageObjectIds = Enumerable.Range(0, pages.Length).Select(page => 3 + page * 2).ToArray();
        objects.Add($"<< /Type /Pages /Count {pages.Length} /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] >>");

        for (var pageIndex = 0; pageIndex < pages.Length; pageIndex++)
        {
            var pageObjectId = 3 + pageIndex * 2;
            var streamObjectId = pageObjectId + 1;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /Contents {streamObjectId} 0 R >>");
            objects.Add(CreateContentStream(pages[pageIndex]));
        }

        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(PdfEncoding.GetByteCount(builder.ToString()));
            builder.Append(index + 1).Append(" 0 obj\n");
            builder.Append(objects[index]).Append('\n');
            builder.Append("endobj\n");
        }

        var xrefOffset = PdfEncoding.GetByteCount(builder.ToString());
        builder.Append("xref\n");
        builder.Append("0 ").Append(objects.Count + 1).Append('\n');
        builder.Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }

        builder.Append("trailer\n");
        builder.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        builder.Append("startxref\n");
        builder.Append(xrefOffset.ToString(CultureInfo.InvariantCulture)).Append('\n');
        builder.Append("%%EOF\n");
        return PdfEncoding.GetBytes(builder.ToString());
    }

    private static string CreateContentStream(IReadOnlyList<string> lines)
    {
        var content = new StringBuilder();
        content.Append("BT\n");
        content.Append("/F1 10 Tf\n");
        content.Append("42 552 Td\n");
        foreach (var line in lines)
        {
            content.Append('(').Append(EscapePdfText(NormalizeAscii(line))).Append(") Tj\n");
            content.Append("0 -12 Td\n");
        }

        content.Append("ET\n");
        var payload = content.ToString();
        return $"<< /Length {PdfEncoding.GetByteCount(payload)} >>\nstream\n{payload}endstream";
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string NormalizeAscii(string value)
    {
        return value
            .Replace("ä", "ae", StringComparison.Ordinal)
            .Replace("ö", "oe", StringComparison.Ordinal)
            .Replace("ü", "ue", StringComparison.Ordinal)
            .Replace("Ä", "Ae", StringComparison.Ordinal)
            .Replace("Ö", "Oe", StringComparison.Ordinal)
            .Replace("Ü", "Ue", StringComparison.Ordinal)
            .Replace("ß", "ss", StringComparison.Ordinal)
            .Replace("€", "EUR", StringComparison.Ordinal);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..Math.Max(0, maxLength - 3)] + "...";
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
