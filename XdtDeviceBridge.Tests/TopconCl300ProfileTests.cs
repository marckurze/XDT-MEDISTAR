using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconCl300ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void XmlWithArbitraryUppercaseFileName_ShouldBeRecognizedAndParsed()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "topcon-cl300-live.XML");
        File.Copy(GetCl300FixturePath("M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml"), xmlPath);

        var classification = new ImportFileClassifier().Classify(xmlPath);
        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Equal(ImportFileKind.DeviceXml, classification.Kind);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "TOPCON");
        AssertMeasurement(parseResult, "Common/ModelName", "CL-300");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "-0.25");
    }

    [Fact]
    public void ParseSerial0001_ShouldReadNamespaceMeasurementsAndOmitEmptyOptionals()
    {
        var parseResult = _parser.ParseFile(GetCl300FixturePath("M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "TOPCON");
        AssertMeasurement(parseResult, "Common/ModelName", "CL-300");
        AssertMeasurement(parseResult, "Common/MachineNo", "00");
        AssertMeasurement(parseResult, "Common/ROMVersion", "1.02.02");
        AssertMeasurement(parseResult, "Common/Version", "1.2");
        AssertMeasurement(parseResult, "Common/Date", "2012-01-01");
        AssertMeasurement(parseResult, "Common/Time", "12:34:56");
        AssertMeasurement(parseResult, "Common/Patient/No.", "0001");
        AssertMeasurement(parseResult, "Common/Patient/ID", "0001");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/DiopterStep", "0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/AxisStep", "1");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PrismStep", "0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/CylinderMode", "mix");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LensType", "glass");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/Wavelength", "e");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "-0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Cylinder", "-1.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Axis", "91");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/H", "+0.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/V", "-0.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Sphere", "+0.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Cylinder", "-1.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Axis", "87");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=- 0.25 Z=- 1.00* 91");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/MedistarLine", "L.:S=+ 0.00 Z=- 1.00* 87");
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/Add1");
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/PD/B/Distance");
    }

    [Fact]
    public void ParseSerial1521_ShouldReadAddPrismAndPdMeasurements()
    {
        var parseResult = _parser.ParseFile(GetCl300FixturePath("M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "TOPCON");
        AssertMeasurement(parseResult, "Common/ModelName", "CL-300");
        AssertMeasurement(parseResult, "Common/ROMVersion", "1.06.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/CylinderMode", "-");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "+3.75");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Cylinder", "-3.50");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Axis", "9");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Add1", "+2.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/H", "+0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/V", "-0.75");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Sphere", "+3.50");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Cylinder", "-3.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Axis", "178");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Add1", "+2.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/H", "-0.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/V", "-1.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/B/Distance", "55.0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/R/Distance", "25.0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/L/Distance", "30.0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/MedistarLine", "L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00");
    }

    [Fact]
    public void MedistarExportForSerial0001_ShouldUseLensmeterFormatAndOmitMissingOptionals()
    {
        var result = MapWithCl300Export(GetCl300FixturePath("M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=- 0.25 Z=- 1.00* 91",
                "L.:S=+ 0.00 Z=- 1.00* 87"
            },
            resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("A=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("A2=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("PD=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("P=", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForSerial1521_ShouldIncludeAddPdAndSignedPrismComponents()
    {
        var result = MapWithCl300Export(GetCl300FixturePath("M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55",
                "L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00"
            },
            resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("A2=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("P=H=-0.00", StringComparison.Ordinal));
    }

    [Fact]
    public void XdtExportForSerial1521_ShouldUseAisExaminationType()
    {
        var mappingResult = MapWithCl300Export(GetCl300FixturePath("M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml"));

        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402CL300", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 3.75 Z=- 3.50*  9 A=+ 2.25 P=H=+0.25 V=-0.75 PD= 55", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 3.50 Z=- 3.00*178 A=+ 2.25 P=V=-1.00", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ParserMeasurements_ShouldExposePathsUsedByCl300ExportProfile()
    {
        var parseResult = _parser.ParseFile(GetCl300FixturePath("M-Serial1521_20120101_000000_TOPCON_CL-300_00.xml"));
        var sourcePaths = parseResult.Measurements
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.Ordinal);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();

        foreach (var rule in exportProfile.Rules.Where(rule => rule.TargetFieldCode == "6228"))
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.SourcePath));
            Assert.StartsWith("Device.", rule.SourcePath, StringComparison.Ordinal);
            Assert.Contains(rule.SourcePath![7..], sourcePaths);
        }
    }

    [Fact]
    public void BuiltInTopconCl300Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300Default();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("CL-300", deviceProfile.Model);
        Assert.Equal("Lensmeter", deviceProfile.DeviceType);
        Assert.True(deviceProfile.Metadata.IsBuiltIn);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-cl300-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6205" or "6220");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-cl300-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-cl300-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void NonCl300TopconLmXml_ShouldNotCreateMedistarLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>CL-200</ModelName>
              </Common>
              <Measure Type="LM">
                <LM>
                  <R>
                    <Sphere>+1.25</Sphere>
                    <Cylinder>-0.50</Cylinder>
                    <Axis>12</Axis>
                  </R>
                </LM>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath.EndsWith("/MedistarLine", StringComparison.Ordinal));
    }

    private MappingResult MapWithCl300Export(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-CL300",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "CL300");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetCl300FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "CL300", fileName);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string WriteTempXml(string content)
    {
        var path = Path.Combine(CreateTempFolder(), "topcon-cl300.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
