using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconCt1PProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void ParseSerial0214_ShouldReadNamespaceTmAndOneSidedCorrectedIop()
    {
        var result = _parser.ParseFile(GetCt1PFixturePath());

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "CT-1P");
        AssertMeasurement(result, "Common/MachineNo", "01");
        AssertMeasurement(result, "Common/ROMVersion", "2.00.09");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2013-06-20");
        AssertMeasurement(result, "Common/Time", "15:20:02");
        AssertMeasurement(result, "Common/Patient/No.", "0214");
        AssertMeasurement(result, "Common/Patient/ID", "0214");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "13.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "12.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='3']/IOP_mmHg", "13.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='5']/IOP_mmHg", "26.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='4']/IOP_mmHg", "44.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='5']/IOP_mmHg", "44.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='6']/IOP_mmHg", "44.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "44.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param1", "0.545");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param2", "0.050");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/CCT", "0.767");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Measured/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Corrected/IOP_mmHg", "5.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param1", "0.545");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Param2", "0.050");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/HeaderLine", "Pachymetrie");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/MedistarLine", "RA: 0.767");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/HeaderLine", "Tonometrie");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyRightLine", "PR: 767 [767] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/MeasuredRightLine", "PR: Gemessen = 16.0 mmHg; Korrigiert = 5.0 mmHg;");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/ParameterRightLine", "PR: Param1 = 545um; Param2 = 0.050; CCT = 767um");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 13 12 13 26 [16.0] // L = 44 44 44 [44.0] mmHg 15:20");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/PachyLeftLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/MeasuredLeftLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/ParameterLeftLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='REF']/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='KM']/", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForSerial0214_ShouldUseTonoAndPachyOnly()
    {
        var result = MapWithCt1PExport(GetCt1PFixturePath());
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402CT1P", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220Pachymetrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.767", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("LA:", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: 767 [767] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Gemessen = 16.0 mmHg; Korrigiert = 5.0 mmHg;", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Param1 = 545um; Param2 = 0.050; CCT = 767um", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205R = 13 12 13 26 [16.0] // L = 44 44 44 [44.0] mmHg 15:20", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205PL:", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6220"));
        Assert.Equal(5, result.Records.Count(record => record.FieldCode == "6205"));
        Assert.DoesNotContain(result.Records, record => record.FieldCode is "6228" or "6221" or "6227");
    }

    [Fact]
    public void BuiltInTopconCt1PProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCt1PDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt1PDefault();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("CT-1P", deviceProfile.Model);
        Assert.Contains("Tonometer", deviceProfile.DeviceType);
        Assert.Contains("Pachymeter", deviceProfile.DeviceType);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-ct1p-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='CCT']/Pachy/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/ParameterRightLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/TonoListLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6228" or "6221" or "6227" or "6302" or "6303" or "6304" or "6305");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-ct1p-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-ct1p-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void NonCt1PTopconXml_ShouldNotCreateCt1PMedistarLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>CT-1</ModelName>
              </Common>
              <Measure type="TM">
                <TM>
                  <R>
                    <Average>
                      <IOP_mmHg>17.0</IOP_mmHg>
                    </Average>
                  </R>
                </TM>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='TM']/Tono/", StringComparison.Ordinal));
    }

    private MappingResult MapWithCt1PExport(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCt1PDefault());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-CT1P",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "CT1P");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetCt1PFixturePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Devices",
            "Topcon",
            "CT1P",
            "M-Serial0214_20130620_152002_TOPCON_CT-1P_2630218.xml");
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-ct1p.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
