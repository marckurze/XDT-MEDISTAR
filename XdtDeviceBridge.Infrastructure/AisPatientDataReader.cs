using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AisPatientDataReader : IAisPatientDataReader
{
    private readonly GdtParser _gdtParser = new();
    private readonly PatientDataMapper _patientDataMapper = new();

    public AisPatientDataReadResult Read(string aisFilePath)
    {
        if (string.IsNullOrWhiteSpace(aisFilePath))
        {
            return new AisPatientDataReadResult(false, null, "AIS-Dateipfad fehlt.");
        }

        try
        {
            var parseResult = _gdtParser.ParseFile(aisFilePath);
            if (parseResult.HasErrors)
            {
                var details = parseResult.Issues
                    .Where(issue => issue.Severity == GdtParseIssueSeverity.Error)
                    .Take(5)
                    .Select(issue =>
                    {
                        var rawLine = string.IsNullOrWhiteSpace(issue.RawLine)
                            ? string.Empty
                            : $" Inhalt: {TrimForDiagnostic(issue.RawLine)}";
                        return $"Zeile {issue.LineNumber}: {issue.Message}{rawLine}";
                    });
                return new AisPatientDataReadResult(
                    false,
                    null,
                    "AIS-Datei konnte wegen GDT-/XDT-Parserfehlern nicht gelesen werden: "
                    + string.Join("; ", details));
            }

            return new AisPatientDataReadResult(true, _patientDataMapper.Map(parseResult.Records), null);
        }
        catch (Exception ex)
        {
            return new AisPatientDataReadResult(false, null, CreateReadExceptionMessage(ex, aisFilePath));
        }
    }

    private static string TrimForDiagnostic(string value)
    {
        var normalized = value.Trim();
        return normalized.Length <= 120 ? normalized : normalized[..117] + "...";
    }

    private static string CreateReadExceptionMessage(Exception exception, string aisFilePath)
    {
        return exception switch
        {
            FileNotFoundException => $"AIS-Datei nicht gefunden: {aisFilePath}",
            UnauthorizedAccessException => $"Zugriff auf AIS-Datei verweigert: {exception.Message}",
            IOException => $"AIS-Datei konnte wegen Datei-/IO-Fehler nicht gelesen werden: {exception.Message}",
            ArgumentException or NotSupportedException => $"AIS-Dateipfad ist ungueltig: {exception.Message}",
            _ => $"AIS-Datei konnte nicht gelesen werden: {exception.Message}"
        };
    }
}
