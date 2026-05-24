using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconSolosProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void XmlWithArbitraryUppercaseFileName_ShouldBeRecognizedAndParsed()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "topcon-solos-live.XML");
        File.Copy(GetSolosFixturePath("SolosExportSample.xml"), xmlPath);

        var classification = new ImportFileClassifier().Classify(xmlPath);
        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Equal(ImportFileKind.DeviceXml, classification.Kind);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "TOPCON");
        AssertMeasurement(parseResult, "Common/ModelName", "SOLOS");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "-1.25");
    }

    [Fact]
    public void ParseSample_ShouldReadNamespaceMeasurementsAndOptionalLensmeterValues()
    {
        var parseResult = _parser.ParseFile(GetSolosFixturePath("SolosExportSample.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "TOPCON");
        AssertMeasurement(parseResult, "Common/ModelName", "SOLOS");
        AssertMeasurement(parseResult, "Common/MachineNo", "000000");
        AssertMeasurement(parseResult, "Common/ROMVersion", "0.0.1");
        AssertMeasurement(parseResult, "Common/Version", "TOPCON Lensmeter Output Data Common Specifications Rev0.1");
        AssertMeasurement(parseResult, "Common/Date", "2022-02-23");
        AssertMeasurement(parseResult, "Common/Time", "16:59:18");
        AssertMeasurement(parseResult, "Common/Patient/No.", "0090");
        AssertMeasurement(parseResult, "Common/Patient/ID", "0090");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/DiopterStep", "0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/AxisStep", "1");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PrismStep", "0.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/CylinderMode", "-");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LensType", "glass");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/Wavelength", "e");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "-1.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Cylinder", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Axis", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/H", "-0.5");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/V", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Sphere", "-1.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Cylinder", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Axis", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/H", "1");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/V", "0");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/B/Distance", "65.5");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/R/Distance", "32");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PD/L/Distance", "33.5");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=- 1.25 Z=+ 0.00*  0 P=H=-0.50 PD= 65.5");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/MedistarLine", "L.:S=- 1.25 Z=+ 0.00*  0 P=H=+1.00");
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/Add1");
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/Add2");
    }

    [Fact]
    public void MedistarExportForSample_ShouldUseLensmeter6228AndOmitEmptyOptionals()
    {
        var result = MapWithSolosExport(GetSolosFixturePath("SolosExportSample.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=- 1.25 Z=+ 0.00*  0 P=H=-0.50 PD= 65.5",
                "L.:S=- 1.25 Z=+ 0.00*  0 P=H=+1.00"
            },
            resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("A=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("A2=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("V=+0.00", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("V=-0.00", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Records, record => record.FieldCode is "6205" or "6220" or "6221" or "6227" or "6330");
    }

    [Fact]
    public void XdtExportForSample_ShouldUseAisExaminationTypeAndCalculatedLengthPrefixes()
    {
        var mappingResult = MapWithSolosExport(GetSolosFixturePath("SolosExportSample.xml"));

        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402SOLOS", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=- 1.25 Z=+ 0.00*  0 P=H=-0.50 PD= 65.5", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 1.25 Z=+ 0.00*  0 P=H=+1.00", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void OptionalSchemaFields_ShouldBeReadButTransmissionNotExportedToMedistar()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>SOLOS</ModelName>
              </Common>
              <Measure Type="LM">
                <LM>
                  <R>
                    <Sphere>+2.00</Sphere>
                    <Cylinder>-0.50</Cylinder>
                    <Axis>12</Axis>
                    <ADD>+1.25</ADD>
                    <ADD2>+2.00</ADD2>
                    <UVTransmittance>88</UVTransmittance>
                  </R>
                </LM>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);
        var result = MapWithSolosExport(path);

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/UVTransmittance", "88");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=+ 2.00 Z=- 0.50* 12 A=+ 1.25 A2=+ 2.00");
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(new[] { "R.:S=+ 2.00 Z=- 0.50* 12 A=+ 1.25 A2=+ 2.00" }, resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("UV", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParserMeasurements_ShouldExposePathsUsedBySolosExportProfile()
    {
        var parseResult = _parser.ParseFile(GetSolosFixturePath("SolosExportSample.xml"));
        var sourcePaths = parseResult.Measurements
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.Ordinal);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault();

        foreach (var rule in exportProfile.Rules.Where(rule => rule.TargetFieldCode == "6228"))
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.SourcePath));
            Assert.StartsWith("Device.", rule.SourcePath, StringComparison.Ordinal);
            Assert.Contains(rule.SourcePath![7..], sourcePaths);
        }
    }

    [Fact]
    public void BuiltInTopconSolosProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconSolosDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconSolosDefault();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("SOLOS", deviceProfile.Model);
        Assert.Equal("Lensmeter", deviceProfile.DeviceType);
        Assert.False(deviceProfile.IsBidirectional);
        Assert.True(deviceProfile.Metadata.IsBuiltIn);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/UVTransmittance");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-solos-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6205" or "6220" or "6221" or "6227" or "6330");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-solos-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-solos-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.Null(interfaceProfile.DeviceOutput);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private MappingResult MapWithSolosExport(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-SOLOS",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "SOLOS");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetSolosFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "Solos", fileName);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string WriteTempXml(string content)
    {
        var path = Path.Combine(CreateTempFolder(), "topcon-solos.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
