using System.Globalization;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerCustomerPdfExporter
{
    private const double PageWidth = 842;
    private const double PageHeight = 595;
    private const double Margin = 28;
    private const double TableTop = 485;
    private const double RowHeight = 21;
    private static readonly Encoding PdfEncoding = Encoding.Latin1;
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
    private static readonly double[] ColumnWidths = { 38, 108, 118, 78, 100, 58, 82, 46, 36, 66, 56 };
    private static readonly string[] Headers =
    {
        "Kdnr",
        "Praxis/Firma",
        "Rechnungs-E-Mail",
        "Zahlungsart",
        "IBAN",
        "BIC",
        "Kontoinhaber",
        "Install.",
        "Geräte",
        "Netto",
        "Gültig bis"
    };

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

        var rows = customers
            .OrderBy(customer => customer.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .Select(customer => CreateRow(customer, pricePerDeviceNet))
            .ToArray();

        var document = BuildPdf(rows, customers, pricePerDeviceNet, createdAt);
        File.WriteAllBytes(filePath, document);
    }

    public void ExportCustomer(
        string filePath,
        LicenseManagerCustomerRecord customer,
        IReadOnlyList<IssuedLicenseRecord> history,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(history);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalizedCustomer = customer.WithNormalizedInstallations();
        var relatedHistory = FilterHistoryForCustomer(normalizedCustomer, history);
        var document = BuildCustomerPdf(normalizedCustomer, relatedHistory, pricePerDeviceNet, createdAt);
        File.WriteAllBytes(filePath, document);
    }

    public static string CreateSuggestedCustomerPdfFileName(LicenseManagerCustomerRecord customer, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var rawName = !string.IsNullOrWhiteSpace(customer.CustomerNumber)
            ? customer.CustomerNumber
            : customer.CustomerName;
        var safeName = CreateSafeFileNameSegment(rawName);
        return $"XDTBox_Kunde_{safeName}_Lizenz" + $"uebersicht_{createdAt:yyyyMMdd}.pdf";
    }

    private static PdfTableRow CreateRow(LicenseManagerCustomerRecord customer, decimal pricePerDeviceNet)
    {
        var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, pricePerDeviceNet);
        return new PdfTableRow(
            CustomerNumber: customer.CustomerNumber ?? "-",
            CustomerName: customer.CustomerName,
            InvoiceEmail: customer.InvoiceEmail ?? customer.Email ?? "-",
            PaymentMethod: FormatPaymentMethod(customer.PaymentMethod),
            Iban: customer.Iban ?? "-",
            Bic: customer.Bic ?? "-",
            AccountHolder: customer.AccountHolder ?? "-",
            Installations: customer.ActiveInstallationCount.ToString(CultureInfo.InvariantCulture),
            Devices: customer.BillableDeviceCount.ToString(CultureInfo.InvariantCulture),
            TotalNet: total.ToString("N2", GermanCulture) + " EUR",
            ValidUntil: FormatValidity(customer.EffectiveLicenseValidUntilUtc));
    }

    private static byte[] BuildPdf(
        IReadOnlyList<PdfTableRow> rows,
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        const int rowsPerPage = 18;
        var pages = rows.Count == 0
            ? new[] { Array.Empty<PdfTableRow>() }
            : rows.Chunk(rowsPerPage).ToArray();

        var streams = pages
            .Select((pageRows, pageIndex) => CreateContentStream(pageRows, pageIndex + 1, pages.Length, customers, pricePerDeviceNet, createdAt))
            .ToArray();

        return BuildPdfDocument(streams);
    }

    private static byte[] BuildCustomerPdf(
        LicenseManagerCustomerRecord customer,
        IReadOnlyList<IssuedLicenseRecord> history,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        var composer = new CustomerPdfComposer(customer, history, pricePerDeviceNet, createdAt);
        return BuildPdfDocument(composer.CreateContentStreams());
    }

    private static byte[] BuildPdfDocument(IReadOnlyList<string> contentStreams)
    {
        var pageCount = Math.Max(1, contentStreams.Count);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>"
        };

        var pageObjectIds = Enumerable.Range(0, pageCount).Select(page => 3 + page * 2).ToArray();
        objects.Add($"<< /Type /Pages /Count {pageCount} /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] >>");

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var pageObjectId = 3 + pageIndex * 2;
            var streamObjectId = pageObjectId + 1;
            objects.Add($"""
                << /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >> /F2 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >> >> >> /Contents {streamObjectId} 0 R >>
                """);
            objects.Add(contentStreams[pageIndex]);
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

    private static string CreateContentStream(
        IReadOnlyList<PdfTableRow> rows,
        int pageNumber,
        int pageCount,
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        decimal pricePerDeviceNet,
        DateTime createdAt)
    {
        var content = new StringBuilder();
        AppendRect(content, Margin, PageHeight - 94, PageWidth - Margin * 2, 46, "0.91 0.97 0.99", "0.73 0.86 0.93");
        AppendText(content, Margin + 16, PageHeight - 68, 18, "XDTBox Kunden- und Lizenzübersicht", bold: true);
        AppendText(content, PageWidth - 248, PageHeight - 62, 9, "Interne Hersteller-Lizenzverwaltung", bold: true);
        AppendText(content, Margin + 16, PageHeight - 86, 9, "Erstellt am: " + createdAt.ToString("dd.MM.yyyy HH:mm", GermanCulture));
        AppendText(content, 280, PageHeight - 86, 9, "Einzelpreis netto pro Geräteanbindung: " + pricePerDeviceNet.ToString("N2", GermanCulture) + " EUR");
        AppendText(content, PageWidth - 95, PageHeight - 86, 8, $"Seite {pageNumber}/{pageCount}");

        DrawTable(content, rows);
        DrawSummary(content, customers, pricePerDeviceNet);

        return WrapContentStream(content);
    }

    private static string WrapContentStream(StringBuilder content)
    {
        var payload = content.ToString();
        return $"<< /Length {PdfEncoding.GetByteCount(payload)} >>\nstream\n{payload}endstream";
    }

    private static void DrawTable(StringBuilder content, IReadOnlyList<PdfTableRow> rows)
    {
        var x = Margin;
        var tableWidth = ColumnWidths.Sum();

        AppendRect(content, x, TableTop, tableWidth, RowHeight, "0.88 0.95 0.98", "0.64 0.79 0.88");
        var cursor = x;
        for (var column = 0; column < Headers.Length; column++)
        {
            AppendText(content, cursor + 4, TableTop + 7, 7.5, Headers[column], bold: true);
            DrawLine(content, cursor, TableTop, cursor, TableTop + RowHeight, "0.64 0.79 0.88");
            cursor += ColumnWidths[column];
        }

        DrawLine(content, x + tableWidth, TableTop, x + tableWidth, TableTop + RowHeight, "0.64 0.79 0.88");
        DrawLine(content, x, TableTop, x + tableWidth, TableTop, "0.64 0.79 0.88");
        DrawLine(content, x, TableTop + RowHeight, x + tableWidth, TableTop + RowHeight, "0.64 0.79 0.88");

        var y = TableTop - RowHeight;
        for (var index = 0; index < rows.Count; index++)
        {
            var fill = index % 2 == 0 ? "1 1 1" : "0.96 0.99 1";
            AppendRect(content, x, y, tableWidth, RowHeight, fill, "0.78 0.87 0.92");
            DrawRow(content, rows[index], y);
            y -= RowHeight;
        }

        if (rows.Count == 0)
        {
            AppendRect(content, x, y, tableWidth, RowHeight, "1 1 1", "0.78 0.87 0.92");
            AppendText(content, x + 6, y + 7, 8, "Keine Kunden gespeichert.");
        }
    }

    private static void DrawRow(StringBuilder content, PdfTableRow row, double y)
    {
        var values = new[]
        {
            row.CustomerNumber,
            row.CustomerName,
            row.InvoiceEmail,
            row.PaymentMethod,
            row.Iban,
            row.Bic,
            row.AccountHolder,
            row.Installations,
            row.Devices,
            row.TotalNet,
            row.ValidUntil
        };

        var cursor = Margin;
        for (var column = 0; column < values.Length; column++)
        {
            var width = ColumnWidths[column];
            var rightAligned = column is 7 or 8 or 9;
            var value = Truncate(values[column], width, rightAligned ? 6.8 : 7.2);
            var textWidth = EstimateTextWidth(value, rightAligned ? 6.8 : 7.2);
            var textX = rightAligned ? cursor + width - textWidth - 4 : cursor + 4;
            AppendText(content, textX, y + 7, rightAligned ? 6.8 : 7.2, value);
            DrawLine(content, cursor, y, cursor, y + RowHeight, "0.78 0.87 0.92");
            cursor += width;
        }

        DrawLine(content, cursor, y, cursor, y + RowHeight, "0.78 0.87 0.92");
    }

    private static void DrawSummary(
        StringBuilder content,
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        decimal pricePerDeviceNet)
    {
        var totalDevices = customers.Sum(customer => customer.BillableDeviceCount);
        var totalCost = customers.Sum(customer => LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, pricePerDeviceNet));
        var y = 50;
        AppendRect(content, Margin, y, PageWidth - Margin * 2, 32, "0.91 0.98 0.94", "0.66 0.84 0.72");
        AppendText(content, Margin + 12, y + 19, 9, $"Summenzeile: Gesamtanzahl Geräte {totalDevices}", bold: true);
        AppendText(content, 360, y + 19, 9, "Gesamtkosten netto: " + totalCost.ToString("N2", GermanCulture) + " EUR", bold: true);
        AppendText(content, Margin + 12, y + 7, 7.5, "Stornierte Installationen werden in aktiven Geräte- und Kostensummen nicht berücksichtigt.");
    }

    private static void AppendText(StringBuilder content, double x, double y, double size, string text, bool bold = false)
    {
        content.Append("BT\n");
        content.Append(bold ? "/F2 " : "/F1 ");
        content.Append(size.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Tf\n");
        content.Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td\n");
        content.Append('(').Append(EscapePdfText(NormalizePdfText(text))).Append(") Tj\n");
        content.Append("ET\n");
    }

    private static void AppendRect(StringBuilder content, double x, double y, double width, double height, string fillRgb, string strokeRgb)
    {
        content.Append("q\n");
        content.Append(fillRgb).Append(" rg\n");
        content.Append(strokeRgb).Append(" RG\n");
        content.Append("0.6 w\n");
        content.Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(width.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(height.ToString("0.##", CultureInfo.InvariantCulture)).Append(" re B\n");
        content.Append("Q\n");
    }

    private static void DrawLine(StringBuilder content, double x1, double y1, double x2, double y2, string strokeRgb)
    {
        content.Append("q\n");
        content.Append(strokeRgb).Append(" RG\n");
        content.Append("0.45 w\n");
        content.Append(x1.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(y1.ToString("0.##", CultureInfo.InvariantCulture)).Append(" m ")
            .Append(x2.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(y2.ToString("0.##", CultureInfo.InvariantCulture)).Append(" l S\n");
        content.Append("Q\n");
    }

    private static string FormatPaymentMethod(LicenseManagerPaymentMethod paymentMethod)
    {
        return paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit
            ? "SEPA-Lastschrift"
            : "Banküberweisung";
    }

    private static string FormatValidity(DateTime? validUntilUtc)
    {
        return XdtBoxLicenseConstants.IsUnlimitedValidUntil(validUntilUtc)
            ? "unbefristet"
            : validUntilUtc?.ToLocalTime().ToString("dd.MM.yyyy", GermanCulture) ?? "unbefristet";
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string NormalizePdfText(string value)
    {
        return value
            .Replace("€", "EUR", StringComparison.Ordinal)
            .Replace("–", "-", StringComparison.Ordinal)
            .Replace("—", "-", StringComparison.Ordinal)
            .Replace("…", "...", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
    }

    private static string Truncate(string value, double columnWidth, double fontSize)
    {
        var normalized = NormalizePdfText(value);
        var maxCharacters = Math.Max(3, (int)Math.Floor(columnWidth / (fontSize * 0.55)));
        if (normalized.Length <= maxCharacters)
        {
            return normalized;
        }

        return normalized[..Math.Max(0, maxCharacters - 3)] + "...";
    }

    private static double EstimateTextWidth(string value, double fontSize)
    {
        return value.Length * fontSize * 0.48;
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }

    private static IReadOnlyList<IssuedLicenseRecord> FilterHistoryForCustomer(
        LicenseManagerCustomerRecord customer,
        IReadOnlyList<IssuedLicenseRecord> history)
    {
        var installationIds = customer.EffectiveInstallations
            .Select(installation => installation.InstallationId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return history
            .Where(record => installationIds.Contains(record.InstallationId)
                || (!string.IsNullOrWhiteSpace(customer.CustomerNumber)
                    && string.Equals(record.CustomerNumber, customer.CustomerNumber, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(record => record.IssuedAtUtc)
            .ToArray();
    }

    private static string CreateSafeFileNameSegment(string? value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in string.IsNullOrWhiteSpace(value) ? "Kunde" : value.Trim())
        {
            var replacement = invalid.Contains(character) || character is '<' or '>' or ':' or '"' or '/' or '\\' or '|' or '?' or '*'
                ? '_'
                : character;
            if (char.IsWhiteSpace(replacement))
            {
                replacement = '_';
            }

            if (replacement == '_')
            {
                if (previousWasSeparator)
                {
                    continue;
                }

                previousWasSeparator = true;
            }
            else
            {
                previousWasSeparator = false;
            }

            builder.Append(replacement);
        }

        var result = builder.ToString().Trim('_', '.');
        return string.IsNullOrWhiteSpace(result) ? "Kunde" : result;
    }

    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value?.ToLocalTime().ToString("dd.MM.yyyy HH:mm", GermanCulture) ?? "-";
    }

    private static string FormatDeviceCount(int count)
    {
        return count.ToString(CultureInfo.InvariantCulture);
    }

    private static string JoinDistinct(IEnumerable<string?> values)
    {
        var distinct = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        return distinct.Length == 0 ? "-" : string.Join("; ", distinct);
    }

    private static string ExtractAisSystem(string displayName)
    {
        var separator = displayName.IndexOf(" + ", StringComparison.Ordinal);
        return separator > 0 ? displayName[..separator] : "-";
    }

    private sealed class CustomerPdfComposer
    {
        private static readonly string[] InstallationHeaders =
        {
            "InstallationId",
            "Computername",
            "Gerätestandort",
            "Status",
            "Geräte",
            "Letzte Lizenz",
            "Gültig bis"
        };

        private static readonly double[] InstallationWidths = { 150, 115, 150, 86, 86, 100, 99 };

        private static readonly string[] DeviceHeaders =
        {
            "InstallationId",
            "Anbindung",
            "Gerät",
            "AIS-System",
            "Standort Gerät",
            "Status",
            "Monatlich netto"
        };

        private static readonly double[] DeviceWidths = { 105, 160, 120, 90, 135, 80, 96 };

        private static readonly string[] HistoryHeaders =
        {
            "Ausgestellt",
            "InstallationId",
            "Geräte",
            "Gerätestandorte",
            "Gültig bis",
            "Status",
            "Datei"
        };

        private static readonly double[] HistoryWidths = { 90, 145, 150, 120, 96, 64, 121 };

        private readonly LicenseManagerCustomerRecord _customer;
        private readonly IReadOnlyList<IssuedLicenseRecord> _history;
        private readonly decimal _pricePerDeviceNet;
        private readonly DateTime _createdAt;
        private readonly List<StringBuilder> _pages = new();
        private readonly Dictionary<string, LicenseManagerInstallationStatus> _installationStatuses;
        private StringBuilder _content = new();
        private double _cursorY;

        public CustomerPdfComposer(
            LicenseManagerCustomerRecord customer,
            IReadOnlyList<IssuedLicenseRecord> history,
            decimal pricePerDeviceNet,
            DateTime createdAt)
        {
            _customer = customer;
            _history = history;
            _pricePerDeviceNet = pricePerDeviceNet;
            _createdAt = createdAt;
            _installationStatuses = customer.EffectiveInstallations
                .Where(installation => !string.IsNullOrWhiteSpace(installation.InstallationId))
                .GroupBy(installation => installation.InstallationId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Status, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> CreateContentStreams()
        {
            StartPage();
            DrawCustomerData();
            DrawFinancialSummary();
            DrawInstallations();
            DrawActiveDevices();
            DrawHistory();

            var pageCount = _pages.Count;
            return _pages
                .Select((page, index) =>
                {
                    DrawFooter(page, index + 1, pageCount);
                    return WrapContentStream(page);
                })
                .ToArray();
        }

        private void StartPage()
        {
            _content = new StringBuilder();
            _pages.Add(_content);

            AppendRect(_content, Margin, PageHeight - 94, PageWidth - Margin * 2, 46, "0.91 0.97 0.99", "0.73 0.86 0.93");
            AppendText(_content, Margin + 16, PageHeight - 68, 18, "XDTBox Kunden-Lizenzübersicht", bold: true);
            AppendText(_content, PageWidth - 265, PageHeight - 62, 9, "Interne Hersteller-Lizenzverwaltung", bold: true);
            AppendText(_content, Margin + 16, PageHeight - 86, 9, "Kunde: " + _customer.CustomerName);
            AppendText(_content, 360, PageHeight - 86, 9, "Erstellt am: " + _createdAt.ToString("dd.MM.yyyy HH:mm", GermanCulture));
            _cursorY = PageHeight - 120;
        }

        private void DrawCustomerData()
        {
            var rows = new[]
            {
                new[] { "Kundennummer", FormatOptional(_customer.CustomerNumber), "Praxis/Firma", _customer.CustomerName },
                new[] { "Adresse", $"{_customer.Street}, {_customer.PostalCode} {_customer.City}", "Telefon", FormatOptional(_customer.Phone) },
                new[] { "E-Mail", FormatOptional(_customer.Email), "Ansprechpartner", FormatOptional(_customer.ContactPerson) },
                new[] { "Rechnungs-E-Mail", FormatOptional(_customer.InvoiceEmail), "Zahlungsart", FormatPaymentMethod(_customer.PaymentMethod) },
                new[] { "IBAN", FormatOptional(_customer.Iban), "BIC", FormatOptional(_customer.Bic) },
                new[] { "Kontoinhaber", FormatOptional(_customer.AccountHolder), "Aktualisiert", FormatDateTime(_customer.UpdatedAtUtc) }
            };

            DrawKeyValueTable("Kundendaten / Abrechnung", rows);
        }

        private void DrawFinancialSummary()
        {
            EnsureSpace(58);
            var activeDevices = _customer.BillableDeviceCount;
            var total = LicenseManagerCostCalculator.CalculateNetTotal(activeDevices, _pricePerDeviceNet);
            AppendRect(_content, Margin, _cursorY - 48, PageWidth - Margin * 2, 44, "0.91 0.98 0.94", "0.66 0.84 0.72");
            AppendText(_content, Margin + 12, _cursorY - 19, 10, "Monatliche Lizenzsumme", bold: true);
            AppendText(_content, 250, _cursorY - 19, 9, "Aktive Geräteanbindungen: " + FormatDeviceCount(activeDevices), bold: true);
            AppendText(_content, 460, _cursorY - 19, 9, "Einzelpreis netto: " + _pricePerDeviceNet.ToString("N2", GermanCulture) + " EUR");
            AppendText(_content, 650, _cursorY - 19, 9, "Monatliche Summe netto: " + total.ToString("N2", GermanCulture) + " EUR", bold: true);
            AppendText(_content, Margin + 12, _cursorY - 36, 7.5, "Stornierte Installationen werden in aktiven Geräte- und Kostensummen nicht berücksichtigt.");
            _cursorY -= 64;
        }

        private void DrawInstallations()
        {
            var rows = _customer.EffectiveInstallations
                .OrderByDescending(installation => installation.IsActive)
                .ThenBy(installation => installation.InstallationId, StringComparer.CurrentCultureIgnoreCase)
                .Select(installation => new[]
                {
                    installation.InstallationId,
                    FormatOptional(installation.MachineName),
                    JoinDistinct(installation.Devices.Select(device => device.Location)),
                    installation.IsActive ? "Aktiv" : "Storniert",
                    FormatDeviceCount(installation.BillableDeviceCount),
                    FormatDateTime(installation.LastLicenseIssuedAtUtc),
                    FormatValidity(installation.LicenseValidUntilUtc)
                })
                .ToArray();

            DrawTable("Installationen / Arbeitsplätze", InstallationHeaders, InstallationWidths, rows);
        }

        private void DrawActiveDevices()
        {
            var rows = _customer.ActiveInstallations
                .OrderBy(installation => installation.InstallationId, StringComparer.CurrentCultureIgnoreCase)
                .SelectMany(installation => CreateDeviceRows(installation))
                .ToArray();

            DrawTable("Aktive lizenzierte Geräteanbindungen", DeviceHeaders, DeviceWidths, rows);
        }

        private IEnumerable<string[]> CreateDeviceRows(LicenseManagerInstallationRecord installation)
        {
            if (installation.Devices.Count == 0 && installation.BillableDeviceCount > 0)
            {
                yield return new[]
                {
                    installation.InstallationId,
                    "Geräteanbindung ohne Detaildaten",
                    "-",
                    "-",
                    "-",
                    "Aktiv",
                    LicenseManagerCostCalculator.CalculateNetTotal(installation.BillableDeviceCount, _pricePerDeviceNet).ToString("N2", GermanCulture) + " EUR"
                };
                yield break;
            }

            foreach (var device in installation.Devices)
            {
                yield return new[]
                {
                    installation.InstallationId,
                    FormatOptional(device.DisplayName),
                    FormatOptional(device.DeviceDisplayName),
                    ExtractAisSystem(device.DisplayName),
                    FormatOptional(device.Location),
                    "Aktiv",
                    _pricePerDeviceNet.ToString("N2", GermanCulture) + " EUR"
                };
            }
        }

        private void DrawHistory()
        {
            var rows = _history
                .Select(record => new[]
                {
                    record.IssuedAtUtc.ToLocalTime().ToString("dd.MM.yyyy", GermanCulture),
                    record.InstallationId,
                    FormatHistoryDevices(record),
                    JoinDistinct(record.Devices.Select(device => device.Location)),
                    FormatValidity(record.ValidUntilUtc),
                    ResolveHistoryStatus(record.InstallationId),
                    Path.GetFileName(record.OutputFilePath)
                })
                .ToArray();

            DrawTable("Lizenzhistorie", HistoryHeaders, HistoryWidths, rows);
        }

        private string ResolveHistoryStatus(string installationId)
        {
            return _installationStatuses.TryGetValue(installationId, out var status)
                && status == LicenseManagerInstallationStatus.Cancelled
                    ? "Storniert"
                    : "Aktiv";
        }

        private static string FormatHistoryDevices(IssuedLicenseRecord record)
        {
            if (record.Devices.Count == 0)
            {
                return FormatDeviceCount(record.MaxActiveDeviceConnections);
            }

            return JoinDistinct(record.Devices.Select(device => device.DeviceDisplayName));
        }

        private void DrawKeyValueTable(string title, IReadOnlyList<string[]> rows)
        {
            AppendSectionTitle(title);
            var rowHeight = 17d;
            var height = rows.Count * rowHeight + 8;
            EnsureSpace(height + 8);
            var boxY = _cursorY - height;
            AppendRect(_content, Margin, boxY, PageWidth - Margin * 2, height, "1 1 1", "0.78 0.87 0.92");

            for (var index = 0; index < rows.Count; index++)
            {
                var y = _cursorY - 13 - index * rowHeight;
                var row = rows[index];
                AppendText(_content, Margin + 8, y, 7.5, row[0] + ":", bold: true);
                AppendText(_content, Margin + 105, y, 7.5, Truncate(row[1], 250, 7.5));
                AppendText(_content, Margin + 400, y, 7.5, row[2] + ":", bold: true);
                AppendText(_content, Margin + 505, y, 7.5, Truncate(row[3], 240, 7.5));
            }

            _cursorY = boxY - 16;
        }

        private void DrawTable(string title, string[] headers, double[] widths, IReadOnlyList<string[]> rows)
        {
            AppendSectionTitle(title);
            DrawTableHeader(headers, widths);

            var tableRows = rows.Count == 0
                ? new[] { new[] { "Keine Daten vorhanden." }.Concat(Enumerable.Repeat(string.Empty, headers.Length - 1)).ToArray() }
                : rows;

            for (var index = 0; index < tableRows.Count; index++)
            {
                if (_cursorY - RowHeight < 64)
                {
                    StartPage();
                    AppendSectionTitle(title + " (Fortsetzung)");
                    DrawTableHeader(headers, widths);
                }

                var fill = index % 2 == 0 ? "1 1 1" : "0.96 0.99 1";
                var y = _cursorY - RowHeight;
                AppendRect(_content, Margin, y, widths.Sum(), RowHeight, fill, "0.78 0.87 0.92");
                DrawTableRow(tableRows[index], widths, y);
                _cursorY = y;
            }

            _cursorY -= 14;
        }

        private void DrawTableHeader(string[] headers, double[] widths)
        {
            EnsureSpace(RowHeight * 2);
            var y = _cursorY - RowHeight;
            AppendRect(_content, Margin, y, widths.Sum(), RowHeight, "0.88 0.95 0.98", "0.64 0.79 0.88");
            DrawTableRow(headers, widths, y, bold: true);
            _cursorY = y;
        }

        private void DrawTableRow(IReadOnlyList<string> values, double[] widths, double y, bool bold = false)
        {
            var cursor = Margin;
            for (var column = 0; column < widths.Length; column++)
            {
                var width = widths[column];
                var value = column < values.Count ? values[column] : string.Empty;
                var fontSize = bold ? 7.2 : 6.9;
                var rightAligned = value.EndsWith(" EUR", StringComparison.Ordinal)
                    || int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
                var truncated = Truncate(value, width, fontSize);
                var textWidth = EstimateTextWidth(truncated, fontSize);
                var textX = rightAligned ? cursor + width - textWidth - 4 : cursor + 4;
                AppendText(_content, textX, y + 7, fontSize, truncated, bold);
                DrawLine(_content, cursor, y, cursor, y + RowHeight, "0.78 0.87 0.92");
                cursor += width;
            }

            DrawLine(_content, cursor, y, cursor, y + RowHeight, "0.78 0.87 0.92");
        }

        private void AppendSectionTitle(string title)
        {
            EnsureSpace(28);
            AppendText(_content, Margin, _cursorY, 10.5, title, bold: true);
            _cursorY -= 18;
        }

        private void EnsureSpace(double height)
        {
            if (_cursorY - height < 64)
            {
                StartPage();
            }
        }

        private static void DrawFooter(StringBuilder content, int pageNumber, int pageCount)
        {
            DrawLine(content, Margin, 42, PageWidth - Margin, 42, "0.78 0.87 0.92");
            AppendText(content, Margin, 28, 7.5, "XDTBox Lizenzmanager - Kunden-PDF");
            AppendText(content, PageWidth - 88, 28, 7.5, $"Seite {pageNumber}/{pageCount}");
        }
    }

    private sealed record PdfTableRow(
        string CustomerNumber,
        string CustomerName,
        string InvoiceEmail,
        string PaymentMethod,
        string Iban,
        string Bic,
        string AccountHolder,
        string Installations,
        string Devices,
        string TotalNet,
        string ValidUntil);
}
