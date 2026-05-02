using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class GdtParserTests
{
    [Fact]
    public void ParseFile_ShouldParseValidGdtLine()
    {
        var parser = new GdtParser();
        var path = GetFilePath("sample-gdt-utf8.gdt");

        var result = parser.ParseFile(path);

        var record = Assert.Single(result.Records.Where(r => r.FieldCode == "3000"));
        Assert.Equal("PAT-100", record.Value);
        Assert.Equal(1, record.LineNumber);
        Assert.Equal(13, record.DeclaredLength);
        Assert.Equal(13, record.ActualLength);
        Assert.True(record.IsLengthValid);
    }

    [Fact]
    public void ParseFile_ShouldParseAnsiFile()
    {
        var parser = new GdtParser();
        var path = GetFilePath("sample-gdt-ansi.gdt");

        var result = parser.ParseFile(path);

        Assert.Contains(result.Records, r => r.FieldCode == "3101" && r.Value == "Müller");
        Assert.Contains(result.Records, r => r.FieldCode == "3102" && r.Value == "Jörg");
    }

    [Fact]
    public void ParseFile_InvalidLength_ShouldCreateWarningAndKeepRecord()
    {
        var parser = new GdtParser();
        var path = WriteTempGdt("9993000PAT-100\n");

        var result = parser.ParseFile(path);

        Assert.Single(result.Records);
        Assert.Contains(result.Issues, i => i.Severity == GdtParseIssueSeverity.Warning && i.LineNumber == 1);
    }

    [Fact]
    public void ParseFile_TooShortLine_ShouldCreateError()
    {
        var parser = new GdtParser();
        var path = WriteTempGdt("12345\n");

        var result = parser.ParseFile(path);

        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, i => i.Severity == GdtParseIssueSeverity.Error && i.LineNumber == 1);
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void ParseFile_NonNumericLength_ShouldCreateError()
    {
        var parser = new GdtParser();
        var path = WriteTempGdt("ABC3000PAT-100\n");

        var result = parser.ParseFile(path);

        Assert.Empty(result.Records);
        Assert.Contains(result.Issues, i => i.Severity == GdtParseIssueSeverity.Error && i.LineNumber == 1);
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void PatientDataMapper_ShouldMapKnownFields()
    {
        var mapper = new PatientDataMapper();
        var records = new[]
        {
            new FieldRecord("3000", "PAT-100", 1, 13, 13, true),
            new FieldRecord("3101", "Müller", 2, 13, 13, true),
            new FieldRecord("3102", "Jörg", 3, 11, 11, true),
            new FieldRecord("3103", "31051980", 4, 15, 15, true),
            new FieldRecord("3106", "12345 Berlin", 5, 19, 19, true),
            new FieldRecord("3107", "Musterstraße 1", 6, 21, 21, true),
            new FieldRecord("3110", "M", 7, 8, 8, true),
            new FieldRecord("0102", "MEDISTAR", 8, 15, 15, true),
            new FieldRecord("0103", "NIDEK", 9, 12, 12, true),
            new FieldRecord("9218", "03.00", 10, 12, 12, true)
        };

        var patientData = mapper.Map(records);

        Assert.Equal("PAT-100", patientData.PatientNumber);
        Assert.Equal("Müller", patientData.LastName);
        Assert.Equal("Jörg", patientData.FirstName);
        Assert.Equal("31051980", patientData.BirthDate);
        Assert.Equal("12345 Berlin", patientData.PostalCodeCity);
        Assert.Equal("Musterstraße 1", patientData.Street);
        Assert.Equal("M", patientData.GenderCode);
        Assert.Equal("MEDISTAR", patientData.SourceSystem);
        Assert.Equal("NIDEK", patientData.TargetSystem);
        Assert.Equal("03.00", patientData.GdtVersion);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    private static string WriteTempGdt(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }
}
