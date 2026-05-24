using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconCt800AProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Theory]
    [InlineData("M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml", "0069", "17:07:16")]
    [InlineData("M-Serial0070_20190411_171259_TOPCON_CT-800A_AA3100481.xml", "0070", "17:12:59")]
    [InlineData("M-Serial0071_20190411_175130_TOPCON_CT-800A_AA3100481.xml", "0071", "17:51:30")]
    public void ParseFixtures_ShouldRecognizeTopconCt800A(string fileName, string patientNo, string time)
    {
        var result = _parser.ParseFile(GetCt800AFixturePath(fileName));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "CT-800A");
        AssertMeasurement(result, "Common/MachineNo", "01");
        AssertMeasurement(result, "Common/ROMVersion", "1.00.07");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2019-04-11");
        AssertMeasurement(result, "Common/Time", time);
        AssertMeasurement(result, "Common/Patient/No.", patientNo);
        AssertMeasurement(result, "Common/Patient/ID", patientNo);
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/HeaderLine", "Tonometrie");
    }

    [Fact]
    public void ParseSerial0069_ShouldReadIopAndCompleteCorrectedIop()
    {
        var result = _parser.ParseFile(GetCt800AFixturePath("M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='1']/IOP_mmHg", "18.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/List[@No='2']/IOP_mmHg", "20.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/R/Average/IOP_mmHg", "19.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='1']/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/List[@No='2']/IOP_mmHg", "16.0");
        AssertMeasurement(result, "Measure[@Type='TM']/TM/L/Average/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param1", "0.545");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Param2", "0.050");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/CCT", "0.550");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Measured/IOP_mmHg", "19.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/R/Corrected/IOP_mmHg", "19.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/CCT", "0.550");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Measured/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/CorrectedIOP/Formula1[@No='1']/L/Corrected/IOP_mmHg", "17.0");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyRightLine", "PR: 550 [550] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/PachyLeftLine", "PL: 550 [550] µm");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/MeasuredRightLine", "PR: Gemessen = 19.0 mmHg; Korrigiert = 19.0 mmHg;");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/ParameterRightLine", "PR: Param1 = 545um; Param2 = 0.050; CCT = 550um");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/MeasuredLeftLine", "PL: Gemessen = 17.0 mmHg; Korrigiert = 17.0 mmHg;");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/ParameterLeftLine", "PL: Param1 = 545um; Param2 = 0.050; CCT = 550um");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 18 20 [19.0] // L = 17 16 [17.0] mmHg 17:07");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='CCT']/", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("M-Serial0070_20190411_171259_TOPCON_CT-800A_AA3100481.xml", "R = 18 18 [18.0] // L = 18 19 [19.0] mmHg 17:12")]
    [InlineData("M-Serial0071_20190411_175130_TOPCON_CT-800A_AA3100481.xml", "R = 22 22 [22.0] // L = 25 25 24 [25.0] mmHg 17:51")]
    public void ParseFixturesWithIncompleteCorrectedIop_ShouldOmitCorrectedIopOutput(string fileName, string expectedListLine)
    {
        var result = _parser.ParseFile(GetCt800AFixturePath(fileName));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", expectedListLine);
        Assert.Contains(result.Measurements, measurement => measurement.Value == "ERROR");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/PachyRightLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/PachyLeftLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/MeasuredRightLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/MeasuredLeftLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/ParameterRightLine");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/ParameterLeftLine");
    }

    [Fact]
    public void MedistarExportForSerial0069_ShouldUseTono6205WithCorrectedIop()
    {
        var result = MapWithCt800AExport(GetCt800AFixturePath("M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml"));
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402CT800A", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: 550 [550] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: 550 [550] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Gemessen = 19.0 mmHg; Korrigiert = 19.0 mmHg;", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: Param1 = 545um; Param2 = 0.050; CCT = 550um", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: Gemessen = 17.0 mmHg; Korrigiert = 17.0 mmHg;", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PL: Param1 = 545um; Param2 = 0.050; CCT = 550um", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205R = 18 20 [19.0] // L = 17 16 [17.0] mmHg 17:07", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(8, result.Records.Count(record => record.FieldCode == "6205"));
    }

    [Theory]
    [InlineData("M-Serial0070_20190411_171259_TOPCON_CT-800A_AA3100481.xml", "R = 18 18 [18.0] // L = 18 19 [19.0] mmHg 17:12")]
    [InlineData("M-Serial0071_20190411_175130_TOPCON_CT-800A_AA3100481.xml", "R = 22 22 [22.0] // L = 25 25 24 [25.0] mmHg 17:51")]
    public void MedistarExportForIncompleteCorrectedIopFixtures_ShouldUseTonoOnly(string fileName, string expectedList)
    {
        var result = MapWithCt800AExport(GetCt800AFixturePath(fileName));
        var exportResult = new XdtExportBuilder().Build(result.Records);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("6205Tonometrie", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains($"6205{expectedList}", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205PR:", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205PL:", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Gemessen =", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("CCT =", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6205"));
    }

    [Theory]
    [InlineData("R", "R = 21 22 [22.0] mmHg 09:30")]
    [InlineData("L", "L = 16 15 [16.0] mmHg 09:30")]
    public void PartialEyeFixture_ShouldExportOnlyExistingEye(string eye, string expectedList)
    {
        var otherEye = eye == "R" ? "L" : "R";
        var path = WriteTempXml($"""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>CT-800A</ModelName>
                <Time>09:30:00</Time>
              </Common>
              <Measure type="TM">
                <TM>
                  <{eye}>
                    <List No="1"><IOP_mmHg>{(eye == "R" ? "21.0" : "16.0")}</IOP_mmHg></List>
                    <List No="2"><IOP_mmHg>{(eye == "R" ? "22.0" : "15.0")}</IOP_mmHg></List>
                    <Average><IOP_mmHg>{(eye == "R" ? "22.0" : "16.0")}</IOP_mmHg></Average>
                  </{eye}>
                </TM>
              </Measure>
            </Ophthalmology>
            """);

        var result = MapWithCt800AExport(path);

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Records, record => record.FieldCode == "6205" && record.Value == expectedList);
        Assert.DoesNotContain(result.Records, record => (record.Value ?? string.Empty).StartsWith($"{otherEye} =", StringComparison.Ordinal));
    }

    [Fact]
    public void EmptyTmFixture_ShouldFailInsteadOfProducingAisOnlyExport()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>CT-800A</ModelName>
              </Common>
              <Measure type="TM">
                <TM>
                  <R><Average><IOP_mmHg></IOP_mmHg></Average></R>
                </TM>
              </Measure>
            </Ophthalmology>
            """);

        var result = MapWithCt800AExport(path);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("No exportable device measurements", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6205");
    }

    [Fact]
    public void BuiltInTopconCt800AProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCt800ADefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt800ADefault();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("CT-800A", deviceProfile.Model);
        Assert.Contains("Tonometer", deviceProfile.DeviceType);
        Assert.False(deviceProfile.IsBidirectional);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine");
        Assert.DoesNotContain(deviceProfile.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='CCT']/", StringComparison.Ordinal));
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-ct800a-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='TM']/Tono/TonoListLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6220" or "6228" or "6221" or "6227" or "6330");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-ct800a-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-ct800a-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.Null(interfaceProfile.DeviceOutput);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private MappingResult MapWithCt800AExport(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCt800ADefault());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-CT800A",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "CT800A");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetCt800AFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "CT800A", fileName);
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-ct800a.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
