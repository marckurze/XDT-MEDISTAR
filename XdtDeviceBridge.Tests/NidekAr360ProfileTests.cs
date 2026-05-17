using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekAr360ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void XmlWithArbitraryUppercaseFileName_ShouldBeRecognizedAndParsed()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "beliebiger-dateiname.XML");
        File.Copy(GetAr360FixturePath("AR360.xml"), xmlPath);

        var classification = new ImportFileClassifier().Classify(xmlPath);
        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Equal(ImportFileKind.DeviceXml, classification.Kind);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Company", "NIDEK");
        AssertMeasurement(parseResult, "ModelName", "AR-360A");
    }

    [Fact]
    public void ParseAr360Sample_ShouldReadArMedianVdAndFarPd()
    {
        var parseResult = _parser.ParseFile(GetAr360FixturePath("AR360.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Company", "NIDEK");
        AssertMeasurement(parseResult, "ModelName", "AR-360A");
        AssertMeasurement(parseResult, "VD", "12.00");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Sphere", "+2.00");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Cylinder", "-1.25");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Axis", "172");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Sphere", "+1.00");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Cylinder", "-0.75");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Axis", "170");
        AssertMeasurement(parseResult, "PD/PDList[@No='1']/FarPD", "60");

        AssertMeasurement(parseResult, "R/AR/ARList[@No='1']/Axis", "177");
    }

    [Fact]
    public void ParseCgu7LSample_ShouldReadNegativeValuesAxisZeroAndFarPd()
    {
        var parseResult = _parser.ParseFile(GetAr360FixturePath("CGU7L.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Company", "NIDEK");
        AssertMeasurement(parseResult, "ModelName", "AR-360A");
        AssertMeasurement(parseResult, "VD", "12.00");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Sphere", "-0.25");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Cylinder", "-0.00");
        AssertMeasurement(parseResult, "R/AR/ARMedian/Axis", "0");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Sphere", "-0.50");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Cylinder", "-0.25");
        AssertMeasurement(parseResult, "L/AR/ARMedian/Axis", "52");
        AssertMeasurement(parseResult, "PD/PDList[@No='1']/FarPD", "56");
    }

    [Fact]
    public void ParseUtf16Ar360Xml_ShouldReadMeasurements()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "UTF16-Messung.XML");
        var xml = File.ReadAllText(GetAr360FixturePath("AR360.xml"))
            .Replace("encoding=\"utf-8\"", "encoding=\"utf-16\"", StringComparison.OrdinalIgnoreCase);
        File.WriteAllText(xmlPath, xml, Encoding.Unicode);

        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "R/AR/ARMedian/Axis", "172");
        AssertMeasurement(parseResult, "VD", "12.00");
    }

    [Fact]
    public void MedistarExportForAr360Sample_ShouldUseArMedianAndTargetFormat()
    {
        var result = MapWithAr360Export(GetAr360FixturePath("AR360.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=+ 2.00 Z=- 1.25*172 PD= 60 VD= 12.00 mm",
                "L.:S=+ 1.00 Z=- 0.75*170"
            },
            resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("*177", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("+ 9.99", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("+ 8.88", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForCgu7LSample_ShouldFormatNegativeValuesAndAxisZero()
    {
        var result = MapWithAr360Export(GetAr360FixturePath("CGU7L.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=- 0.25 Z=- 0.00*0 PD= 56 VD= 12.00 mm",
                "L.:S=- 0.50 Z=- 0.25*52"
            },
            resultLines);
    }

    [Fact]
    public void XdtExportForAr360Sample_ShouldContainValidatedMedistarFields()
    {
        var mappingResult = MapWithAr360Export(GetAr360FixturePath("AR360.xml"));

        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.Empty(exportResult.Issues);
        Assert.Contains("0148402AR360\r\n", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("0536228R.:S=+ 2.00 Z=- 1.25*172 PD= 60 VD= 12.00 mm\r\n", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("0336228L.:S=+ 1.00 Z=- 0.75*170\r\n", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("*177", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportWithoutRightArMedianSphere_ShouldReportMissingSource()
    {
        var measurements = _parser.ParseFile(GetAr360FixturePath("AR360.xml")).Measurements
            .Where(measurement => !string.Equals(measurement.SourcePath, "R/AR/ARMedian/Sphere", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default());

        var result = _mappingEngine.Map(CreatePatientData(), measurements, rules);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("Device.R/AR/ARMedian/Sphere", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6228" && (record.Value ?? string.Empty).StartsWith("R.:", StringComparison.Ordinal));
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && (record.Value ?? string.Empty).StartsWith("L.:", StringComparison.Ordinal));
    }

    [Fact]
    public void BuiltInAr360Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekAr360Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default();

        Assert.Equal("NIDEK", deviceProfile.Manufacturer);
        Assert.Equal("AR-360A", deviceProfile.Model);
        Assert.Equal("Autorefractor", deviceProfile.DeviceType);
        Assert.True(deviceProfile.Metadata.IsBuiltIn);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "R/AR/ARMedian/Sphere" && measurement.IsRequired);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "PD/PDList[@No='1']/FarPD");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-nidek-ar360-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.OutputTemplate.Contains("VD= {Device.VD:Raw} mm", StringComparison.Ordinal));
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-nidek-ar360-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-ar360-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private MappingResult MapWithAr360Export(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-AR360",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "AR360");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase)
            && measurement.Value == value);
    }

    private static string GetAr360FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "AR360", fileName);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
