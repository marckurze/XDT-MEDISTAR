using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconKr1ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void ParseFixture_ShouldRecognizeTopconKr1RefXml()
    {
        var result = _parser.ParseFile(GetKr1FixturePath());

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "KR-1");
        AssertMeasurement(result, "Common/MachineNo", "02");
        AssertMeasurement(result, "Common/ROMVersion", "2.01.02");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2019-04-11");
        AssertMeasurement(result, "Common/Time", "12:07:32");
        AssertMeasurement(result, "Common/Patient/No.", "0001");
        AssertMeasurement(result, "Common/Patient/ID", "0001");
        AssertMeasurement(result, "Measure[@Type='REF']/@type", "REF");
    }

    [Fact]
    public void ParseFixture_ShouldReadRefMedianValuesAndPreparedMedistarLines()
    {
        var result = _parser.ParseFile(GetKr1FixturePath());

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Measure[@Type='REF']/VD", "13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/DiopterStep", "0.25");
        AssertMeasurement(result, "Measure[@Type='REF']/AxisStep", "1");
        AssertMeasurement(result, "Measure[@Type='REF']/CylinderMode", "-");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Distance", "68.00");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Near", "68.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Sphere", "0.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Cylinder", "-0.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Axis", "165");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/SE", "0.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Sphere", "0.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Cylinder", "0.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Axis", "0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/SE", "0.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=+ 0.75 Z=- 0.50*165 PD= 68 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=+ 0.50 Z=+ 0.00*  0");
    }

    [Fact]
    public void MedistarExportForFixture_ShouldUseRef6228OnlyAndAisExaminationType()
    {
        var result = MapWithKr1Export(GetKr1FixturePath());
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402KR1", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 0.75 Z=- 0.50*165 PD= 68 VD= 13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 0.50 Z=+ 0.00*  0", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("R.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("L.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Records, record => (record.Value ?? string.Empty).Contains("SE", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            new[]
            {
                "R.:S=+ 0.75 Z=- 0.50*165 PD= 68 VD= 13.75",
                "L.:S=+ 0.50 Z=+ 0.00*  0"
            },
            result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value).ToArray());
    }

    [Theory]
    [InlineData("R", "R.:S=+ 1.25 Z=- 0.50* 12 PD= 65 VD= 13.75")]
    [InlineData("L", "L.:S=- 2.00 Z=+ 0.00*  0")]
    public void PartialEyeFixture_ShouldExportOnlyExistingEye(string eye, string expectedLine)
    {
        var otherEye = eye == "R" ? "L" : "R";
        var path = WriteTempXml($"""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-1</ModelName>
              </Common>
              <Measure type="REF">
                <VD>13.75</VD>
                <REF>
                  <{eye}>
                    <Median>
                      <Sphere>{(eye == "R" ? "1.25" : "-2.00")}</Sphere>
                      <Cylinder>{(eye == "R" ? "-0.50" : "0.0")}</Cylinder>
                      <Axis>{(eye == "R" ? "12" : "0")}</Axis>
                    </Median>
                  </{eye}>
                </REF>
                <PD><Distance>65.00</Distance></PD>
              </Measure>
            </Ophthalmology>
            """);

        var result = MapWithKr1Export(path);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && record.Value == expectedLine);
        Assert.DoesNotContain(result.Records, record => (record.Value ?? string.Empty).StartsWith($"{otherEye}.:", StringComparison.Ordinal));
    }

    [Fact]
    public void ListOnlyRefFixture_ShouldNotUseListValuesWhenMedianIsMissing()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-1</ModelName>
              </Common>
              <Measure type="REF">
                <REF>
                  <R>
                    <List No="1">
                      <Sphere>4.00</Sphere>
                      <Cylinder>-1.00</Cylinder>
                      <Axis>99</Axis>
                    </List>
                  </R>
                </REF>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);
        var mappingResult = MapWithKr1Export(path);

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Measure[@Type='REF']/REF/R/List[@No='1']/Sphere", "4.00");
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.True(mappingResult.HasErrors);
        Assert.Contains(mappingResult.Issues, issue => issue.Message.Contains("No exportable device measurements", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(mappingResult.Records, record => record.FieldCode == "6228");
    }

    [Fact]
    public void EmptyRefFixture_ShouldFailInsteadOfProducingAisOnlyExport()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-1</ModelName>
              </Common>
              <Measure type="REF">
                <REF>
                  <R><Median></Median></R>
                </REF>
              </Measure>
            </Ophthalmology>
            """);

        var result = MapWithKr1Export(path);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("No exportable device measurements", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6228");
    }

    [Fact]
    public void BuiltInTopconKr1Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconKr1Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconKr1Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr1Default();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("KR-1", deviceProfile.Model);
        Assert.Contains("Keratorefraktometer", deviceProfile.DeviceType);
        Assert.False(deviceProfile.IsBidirectional);
        Assert.Contains("REF", deviceProfile.SupportedExaminationTypes);
        Assert.Contains("Keratometer-Kandidat", deviceProfile.SupportedExaminationTypes);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.DoesNotContain(deviceProfile.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='KM']/", StringComparison.Ordinal));
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-kr1-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6221" or "6227" or "6205" or "6220" or "6330");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-kr1-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-kr1-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.Null(interfaceProfile.DeviceOutput);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private MappingResult MapWithKr1Export(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconKr1Default());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-KR1",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "KR1");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetKr1FixturePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Devices",
            "Topcon",
            "KR1",
            "M-Serial0001_20190411_120732_TOPCON_KR-1_4430227.xml");
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-kr1.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
