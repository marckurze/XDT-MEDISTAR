using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconKr800SProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void ParseSerial0036_ShouldReadRefKmAndSbjMeasurements()
    {
        var result = _parser.ParseFile(GetKr800SFixturePath("M-Serial0036_20131206_213127_TOPCON_KR-800S_.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "KR-800S");
        AssertMeasurement(result, "Common/MachineNo", "01");
        AssertMeasurement(result, "Common/ROMVersion", "0.11.00");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2013-12-06");
        AssertMeasurement(result, "Common/Time", "21:31:27");
        AssertMeasurement(result, "Measure[@Type='REF']/VD", "13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Distance", "58.00");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Near", "58.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Sphere", "-5.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Cylinder", "0.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Axis", "0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Sphere", "-5.25");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Cylinder", "0.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Axis", "0");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Radius", "7.70");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Power", "43.75");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Axis", "180");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Radius", "7.69");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Power", "43.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/L/Sph", "-6.00");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/L", "0.7");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B", "58.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=- 5.50 Z=+ 0.00*  0 PD= 58 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=- 5.25 Z=+ 0.00*  0");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine1", "R: R1=7.70 43.75 *180 R2=7.70 43.75 *90 // L: R1=7.70 43.75 *180 R2=7.69 43.75 *90");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine2", "R: AV=7.70 43.75 CYL=+0.00 0 // L: AV=7.70 43.75 CYL=+0.00 0");
        AssertMeasurement(result, "Measure[@Type='SBJ']/MedistarLine1", "Subjektive Refraktion Full Correction FAR: L.:S=- 6.00 Z=+ 0.00* 93 VA=0.7 PD=58.5 VD=12.00");
        AssertMeasurement(result, "Measure[@Type='SBJ']/MedistarLine2", "Subjektive Refraktion Full Correction NEAR: L.:S=- 8.25 Z=+ 0.00* 93 VA=0.8 PD=58.5 VD=12.00");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/MedistarLine3");
    }

    [Fact]
    public void ParseSerial0426_ShouldReadRefKmAndFullCorrectionSbjMeasurements()
    {
        var result = _parser.ParseFile(GetKr800SFixturePath("M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "KR-800S");
        AssertMeasurement(result, "Common/ROMVersion", "1.04.00");
        AssertMeasurement(result, "Measure[@Type='REF']/VD", "13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/PD/Distance", "66.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Sphere", "3.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Cylinder", "-4.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/Median/Axis", "13");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Sphere", "3.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Cylinder", "-2.50");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/Median/Axis", "173");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Radius", "8.48");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/R1/Power", "39.75");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/R/Median/Cylinder/Power", "-3.75");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/L/Median/R2/Power", "43.00");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph", "3.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/R", "0.6");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/L", "1.0");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=+ 3.75 Z=- 4.00* 13 PD= 66 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=+ 3.75 Z=- 2.50*173");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine1", "R: R1=8.48 39.75 *11 R2=7.79 43.50 *101 // L: R1=8.35 40.50 *171 R2=7.87 43.00 *81");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine2", "R: AV=8.14 41.75 CYL=-3.75 11 // L: AV=8.11 41.75 CYL=-2.50 171");
        AssertMeasurement(result, "Measure[@Type='SBJ']/MedistarLine1", "Subjektive Refraktion Full Correction FAR: R.:S=+ 3.75 Z=- 4.00* 13 VA=0.6 / L.:S=+ 3.75 Z=- 2.50*173 VA=1.0 PD=66 VD=13.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/MedistarLine2", "Subjektive Refraktion Full Correction NEAR: R.:S=+ 5.50 Z=- 4.00* 13 / L.:S=+ 5.50 Z=- 2.50*173 PD=66 VD=13.75");
        Assert.DoesNotContain(result.Measurements, measurement =>
            measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal)
            && measurement.Value.Contains("Unaided", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Measurements, measurement =>
            measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal)
            && measurement.Value.Contains("ContrastVA", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Measurements, measurement =>
            measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal)
            && measurement.Value.Contains("GlareVA", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MedistarExportForSerial0036_ShouldUseRefKmAndConservativeSbjLines()
    {
        var result = MapWithKr800SExport(GetKr800SFixturePath("M-Serial0036_20131206_213127_TOPCON_KR-800S_.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Equal(
            new[]
            {
                "R.:S=- 5.50 Z=+ 0.00*  0 PD= 58 VD= 13.75",
                "L.:S=- 5.25 Z=+ 0.00*  0"
            },
            result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value).ToArray());
        Assert.Equal(2, result.Records.Count(record => record.FieldCode == "6221"));
        Assert.Contains(result.Records, record => record.FieldCode == "6227" && (record.Value ?? string.Empty).Contains("L.:S=- 6.00", StringComparison.Ordinal));
        Assert.Contains(result.Records, record => record.FieldCode == "6227" && (record.Value ?? string.Empty).Contains("L.:S=- 8.25", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Records, record => (record.Value ?? string.Empty).Contains("R.:S= Z=", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Records, record => record.FieldCode is "6205" or "6220" or "6302" or "6303" or "6304" or "6305");
    }

    [Fact]
    public void MedistarExportForSerial0426_ShouldUseRefKmSbjAndAisExaminationType()
    {
        var mappingResult = MapWithKr800SExport(GetKr800SFixturePath("M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml"));
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402KR800S", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 3.75 Z=- 4.00* 13 PD= 66 VD= 13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 3.75 Z=- 2.50*173", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=8.48 39.75 *11 R2=7.79 43.50 *101 // L: R1=8.35 40.50 *171 R2=7.87 43.00 *81", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6221R: AV=8.14 41.75 CYL=-3.75 11 // L: AV=8.11 41.75 CYL=-2.50 171", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6227Subjektive Refraktion Full Correction FAR: R.:S=+ 3.75 Z=- 4.00* 13 VA=0.6 / L.:S=+ 3.75 Z=- 2.50*173 VA=1.0 PD=66 VD=13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6227Subjektive Refraktion Full Correction NEAR: R.:S=+ 5.50 Z=- 4.00* 13 / L.:S=+ 5.50 Z=- 2.50*173 PD=66 VD=13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ParserMeasurements_ShouldExposePathsUsedByKr800SExportProfile()
    {
        var parseResult = _parser.ParseFile(GetKr800SFixturePath("M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml"));
        var sourcePaths = parseResult.Measurements
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.Ordinal);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();

        foreach (var rule in exportProfile.Rules.Where(rule => rule.TargetFieldCode is "6228" or "6221" or "6227"))
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.SourcePath));
            Assert.StartsWith("Device.", rule.SourcePath, StringComparison.Ordinal);
            if (rule.SourcePath!.Contains("MedistarLine3", StringComparison.Ordinal)
                || rule.SourcePath.Contains("MedistarLine4", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.Contains(rule.SourcePath[7..], sourcePaths);
        }
    }

    [Fact]
    public void BuiltInTopconKr800SProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal("KR-800S", deviceProfile.Model);
        Assert.Contains("Subjective", deviceProfile.DeviceType);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/MedistarLine1");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-kr800-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6221" && rule.SourcePath == "Device.Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/MedistarLine1");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6205" or "6220" or "6302" or "6303" or "6304" or "6305");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-kr800-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-kr800-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void NonKr800STopconXml_ShouldNotCreateKr800SMedistarLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-800</ModelName>
              </Common>
              <Measure type="REF">
                <REF>
                  <R>
                    <Median>
                      <Sphere>+1.25</Sphere>
                      <Cylinder>-0.50</Cylinder>
                      <Axis>12</Axis>
                    </Median>
                  </R>
                </REF>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        Assert.DoesNotContain(parseResult.Measurements, measurement => measurement.SourcePath.Contains("MedistarLine", StringComparison.Ordinal));
    }

    private MappingResult MapWithKr800SExport(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-KR800S",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "KR800S");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetKr800SFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "KR800S", fileName);
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-kr800s.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
