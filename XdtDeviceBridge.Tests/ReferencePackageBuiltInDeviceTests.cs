using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ReferencePackageBuiltInDeviceTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly HuvitzTextDeviceParser _huvitzParser = new();
    private readonly ShinNipponDeviceParser _shinNipponParser = new();
    private readonly TomeyDeviceParser _tomeyParser = new();
    private readonly TomeyEmDeviceParser _tomeyEmParser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Theory]
    [InlineData("ARK-510A", "device-nidek-ark510a-default", "export-medistar-nidek-ark510a-default")]
    [InlineData("ARK-560A", "device-nidek-ark560a-default", "export-medistar-nidek-ark560a-default")]
    public void NidekArk5xxProfiles_ShouldParseAndExportAutorefractorLines(string model, string deviceId, string exportId)
    {
        var path = WriteTempXml($$"""
            <Data>
              <Company>NIDEK</Company>
              <ModelName>{{model}}</ModelName>
              <VD>12.00</VD>
              <R>
                <AR>
                  <ARMedian>
                    <Sphere>0.25</Sphere>
                    <Cylinder>-0.75</Cylinder>
                    <Axis>141</Axis>
                  </ARMedian>
                </AR>
              </R>
              <L>
                <AR>
                  <ARMedian>
                    <Sphere>0.00</Sphere>
                    <Cylinder>0.00</Cylinder>
                    <Axis>0</Axis>
                  </ARMedian>
                </AR>
              </L>
              <PD>
                <PDList No="1">
                  <FarPD>59</FarPD>
                </PDList>
              </PD>
            </Data>
            """);

        var parseResult = _parser.ParseFile(path);
        var exportProfile = model == "ARK-510A"
            ? DefaultExportProfileDefinitions.CreateMedistarNidekArk510ADefault()
            : DefaultExportProfileDefinitions.CreateMedistarNidekArk560ADefault();
        var mappingResult = _mappingEngine.Map(CreatePatientData(model), parseResult.Measurements, _mappingAdapter.Adapt(exportProfile));
        var xdt = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "ModelName" && measurement.Value == model);
        Assert.Equal(deviceId, model == "ARK-510A"
            ? DefaultDeviceProfileDefinitions.CreateNidekArk510ADefault().Metadata.Id
            : DefaultDeviceProfileDefinitions.CreateNidekArk560ADefault().Metadata.Id);
        Assert.Equal(exportId, exportProfile.Metadata.Id);
        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(xdt.Issues);
        Assert.Contains("6228R.:S=+ 0.25 Z=- 0.75*141 PD= 59 VD= 12.00 mm", xdt.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 0.00 Z=+ 0.00*  0", xdt.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void NidekLm1800PProfile_ShouldUseLensmeterFallbackEyeBlocks()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>LM-1800P</ModelName>
              </Common>
              <Measure Type="LM">
                <LM>
                  <I>
                    <Sphare>6.25</Sphare>
                    <Cylinder>-3.25</Cylinder>
                    <Axis>3</Axis>
                  </I>
                  <S>
                    <Sphare>6.50</Sphare>
                    <Cylinder>-2.75</Cylinder>
                    <Axis>170</Axis>
                    <ADD>1.50</ADD>
                  </S>
                </LM>
                <PD>
                  <Distance>59</Distance>
                </PD>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekLm1800PDefault();
        var mappingResult = _mappingEngine.Map(CreatePatientData("LM1800P"), parseResult.Measurements, _mappingAdapter.Adapt(exportProfile));
        var resultLines = mappingResult.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/I/Sphere" && measurement.Value == "6.25");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine" && measurement.Value == "R.:S=+ 6.25 Z=- 3.25*  3 PD= 59");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/MedistarLine" && measurement.Value == "L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50");
        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Equal(
            new[]
            {
                "R.:S=+ 6.25 Z=- 3.25*  3 PD= 59",
                "L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50"
            },
            resultLines);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(DefaultDeviceProfileDefinitions.CreateNidekLm1800PDefault()));
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm1800PDefault()));
    }

    [Fact]
    public void TopconKr800Alias_ShouldCreateExistingKr800FamilyPreparedLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-800</ModelName>
              </Common>
              <Measure type="REF">
                <VD>13.75</VD>
                <REF>
                  <R>
                    <Median>
                      <Sphere>1.25</Sphere>
                      <Cylinder>-0.50</Cylinder>
                      <Axis>12</Axis>
                    </Median>
                  </R>
                </REF>
                <PD>
                  <Distance>60.00</Distance>
                </PD>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement =>
            measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine"
            && measurement.Value == "R.:S=+ 1.25 Z=- 0.50* 12 PD= 60 VD= 13.75");
    }

    [Fact]
    public void HuvitzHrkProfiles_ShouldParseAndExportRefAndKeratometryLines()
    {
        var parseResult = _huvitzParser.ParseFile(GetHuvitzFixturePath("HRK8000A", "HRK8000A_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarHuvitzHrk8000ADefault();
        var xdt = BuildXdt(CreatePatientData("HRK8000A"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "HRK-8000A");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/Sphere" && measurement.Value == "-0.75");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains("6228R.:S=- 0.75 Z=- 0.50* 25 PD= 66", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 0.25 Z=- 0.75*111 PD= 66", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.82 R2=7.7 *108 // L: R1=7.9 R2=7.81 * 33", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void HuvitzHnt1PProfile_ShouldParseAndExportTonometrieAndPachymetrieLines()
    {
        var parseResult = _huvitzParser.ParseFile(GetHuvitzFixturePath("HNT1P", "HNT1P_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarHuvitzHnt1PDefault();
        var xdt = BuildXdt(CreatePatientData("HNT1P"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "HNT-1P");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine" && measurement.Value == "R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/MedistarLine" && measurement.Value == "RA: 0.559 // LA: 0.560");
        Assert.Contains("6205R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg", xdt, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.559 // LA: 0.560", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void HuvitzHtr1AProfile_ShouldParseAndExportCombinedMeasurementsByMeasurementType()
    {
        var parseResult = _huvitzParser.ParseFile(GetHuvitzFixturePath("HTR1A", "HTR1A_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarHuvitzHtr1ADefault();
        var xdt = BuildXdt(CreatePatientData("HTR1A"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "HTR-1A");
        Assert.Contains("6228R.:S=+ 0.75 Z=- 1.25*110 PD= 63", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 0.25 Z=- 0.75* 77 PD= 63", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.82 R2=7.7 *108 // L: R1=7.9 R2=7.81 * 33", xdt, StringComparison.Ordinal);
        Assert.Contains("6205R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg", xdt, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.559 // LA: 0.560", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void ShinNipponAccurefR800Profile_ShouldParseAndExportAutorefractorLines()
    {
        var parseResult = _shinNipponParser.ParseFile(GetShinNipponFixturePath("AccurefR800", "AccurefR800_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefR800Default();
        var xdt = BuildXdt(CreatePatientData("AccurefR800"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "Accuref R-800");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/Sphere" && measurement.Value == "-3.75");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/L/MedistarLine");
        Assert.Contains("6228R.:S=- 3.75 Z=- 1.25* 98 PD= 66 VD= 12", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 4.25 Z=- 2.00*105 PD= 66 VD= 12", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void ShinNipponAccurefK900Profile_ShouldParseAndExportRefAndKeratometryLines()
    {
        var parseResult = _shinNipponParser.ParseFile(GetShinNipponFixturePath("AccurefK900", "AccurefK900_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefK900Default();
        var xdt = BuildXdt(CreatePatientData("AccurefK900"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "Accuref K-900");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains("6228R.:S=+ 0.25 Z=- 0.75*141 PD= 59 VD= 12", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 0.00 Z=+ 0.00*  0 PD= 59 VD= 12", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.67 *173 R2=7.57 * 83 // L: R1=7.68 *175 R2=7.52 * 85", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: CYL=-0.50 173 // L: CYL=-1.00 175", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("DL1000", "DL1000_reference_text.txt", "DL-1000", "6228R.:S=+ 6.25 Z=- 3.25*  3 P=0.75 OUT 1.00 UP PD= 59 A=+ 1.25", "6228L.:S=+ 6.50 Z=- 2.75*170 A=+ 0.25")]
    [InlineData("DL800", "DL800_reference_text.txt", "DL-800", "6228R.:S=+ 1.00 Z=- 0.25*103", "6228L.:S=+ 0.25 Z=- 1.25* 62")]
    [InlineData("DL900", "DL900_reference_text.txt", "DL-900", "6228R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP A=+ 0.25", "6228L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP A=+ 0.25")]
    [InlineData("SLM4000", "SLM4000_reference_text.txt", "SLM-4000", "6228R.:S=+ 0.75 Z=- 0.50* 25 PD= 63", "6228L.:S=+ 0.25 Z=- 0.75*111")]
    public void ShinNipponLensmeterProfiles_ShouldParseAndExportLensmeterLines(
        string familyFolder,
        string fileName,
        string model,
        string expectedRight,
        string expectedLeft)
    {
        var parseResult = _shinNipponParser.ParseFile(GetShinNipponFixturePath(familyFolder, fileName));
        var exportProfile = model switch
        {
            "DL-1000" => DefaultExportProfileDefinitions.CreateMedistarShinNipponDl1000Default(),
            "DL-800" => DefaultExportProfileDefinitions.CreateMedistarShinNipponDl800Default(),
            "DL-900" => DefaultExportProfileDefinitions.CreateMedistarShinNipponDl900Default(),
            _ => DefaultExportProfileDefinitions.CreateMedistarShinNipponSlm4000Default()
        };
        var xdt = BuildXdt(CreatePatientData(model), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == model);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(expectedRight, xdt, StringComparison.Ordinal);
        Assert.Contains(expectedLeft, xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void ShinNipponNct200Profile_ShouldParseAndExportTonometryLine()
    {
        var parseResult = _shinNipponParser.ParseFile(GetShinNipponFixturePath("NCT200", "NCT200_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarShinNipponNct200Default();
        var xdt = BuildXdt(CreatePatientData("NCT200"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "NCT-200");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine" && measurement.Value == "R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg");
        Assert.Contains("6205R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyCf2000Profile_ShouldParseAndExportLensmeterLines()
    {
        var parseResult = _tomeyParser.ParseFile(GetTomeyFixturePath("CF2000", "CF2000_reference_text.txt"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTomeyCf2000Default();
        var xdt = BuildXdt(CreatePatientData("CF2000"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "CF-2000");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/Sphere" && measurement.Value == "+6.50");
        Assert.Contains("6228R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP A=+ 0.25 A2=+ 1.25", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP A=+ 0.25", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TL2000C", "TL2000C_reference.csv", "TL-2000C", "6228R.:S=+ 1.00 Z=- 0.25*103 P=0.75 OUT 1.00 UP PD= 59 A=+ 0.25 A2=+ 1.25", "6228L.:S=+ 0.25 Z=- 1.25* 62 A=+ 0.25")]
    [InlineData("TL6000", "TL6000_reference.csv", "TL-6000", "6228R.:S=+ 6.50 Z=- 1.75*172 P=0.75 OUT 1.00 UP PD= 59", "6228L.:S=+ 6.00 Z=- 2.25*  2 P=0.50 OUT 1.50 UP")]
    [InlineData("TL7000", "TL7000_reference.csv", "TL-7000", "6228R.:S=+ 1.00 Z=- 0.25*103", "6228L.:S=+ 0.25 Z=- 1.25* 62")]
    public void TomeyTlProfiles_ShouldParseAndExportLensmeterLines(
        string familyFolder,
        string fileName,
        string model,
        string expectedRight,
        string expectedLeft)
    {
        var parseResult = _tomeyParser.ParseFile(GetTomeyFixturePath(familyFolder, fileName));
        var exportProfile = model switch
        {
            "TL-6000" => DefaultExportProfileDefinitions.CreateMedistarTomeyTl6000Default(),
            "TL-7000" => DefaultExportProfileDefinitions.CreateMedistarTomeyTl7000Default(),
            _ => DefaultExportProfileDefinitions.CreateMedistarTomeyTl2000CDefault()
        };
        var xdt = BuildXdt(CreatePatientData(model), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == model);
        Assert.Contains(expectedRight, xdt, StringComparison.Ordinal);
        Assert.Contains(expectedLeft, xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyMr6000Profile_ShouldParseAndExportCombinedMeasurementsByMeasurementType()
    {
        var parseResult = _tomeyParser.ParseFile(GetTomeyFixturePath("MR6000", "MR6000_reference.xml"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTomeyMr6000Default();
        var xdt = BuildXdt(CreatePatientData("MR6000"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "MR-6000");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/MedistarLine" && measurement.Value == "RA: 0.559 // LA: 0.560");
        Assert.Contains("6228R.:S=- 3.75 Z=- 1.25* 98 PD= 66 VD= 12.00", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 4.25 Z=- 2.00*105", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.67 44.00 *173 R2=7.57 44.50 * 83 // L: R1=7.68 44.00 *175 R2=7.52 45.00 * 85", xdt, StringComparison.Ordinal);
        Assert.Contains("6221R: AV=7.62 44.25 CYL=-0.50 173 // L: AV=7.60 44.50 CYL=-1.00 175", xdt, StringComparison.Ordinal);
        Assert.Contains("6205R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg", xdt, StringComparison.Ordinal);
        Assert.Contains("6205PR: Gemessen = 12.7 mmHg; Korrigiert = 12.3 mmHg; CCT = 559um", xdt, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.559 // LA: 0.560", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyTop1000Profile_ShouldParseAndExportTonometrieAndPachymetrieLines()
    {
        var parseResult = _tomeyParser.ParseFile(GetTomeyFixturePath("TOP1000", "TOP1000_reference.xml"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTomeyTop1000Default();
        var xdt = BuildXdt(CreatePatientData("TOP1000"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "TOP-1000");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='TM']/Tono/TonoListLine" && measurement.Value == "R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='CCT']/Pachy/MedistarLine" && measurement.Value == "RA: 0.559 // LA: 0.560");
        Assert.Contains("6205R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg", xdt, StringComparison.Ordinal);
        Assert.Contains("6205PR: Gemessen = 12.7 mmHg; Korrigiert = 12.3 mmHg; CCT = 559um", xdt, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.559 // LA: 0.560", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6228", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyEm3000Profile_ShouldParseAndExportEndothelialMeasurementsCommentsAndImageReferences()
    {
        var parseResult = _tomeyEmParser.ParseFile(GetTomeyFixturePath("EM3000", "EM3000_reference.csv"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTomeyEm3000Default();
        var xdt = BuildXdt(CreatePatientData("EM3000"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "EM-3000");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='EM']/Endothelium/R/Number" && measurement.Value == "2530");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='EM']/Endothelium/L/MedistarLine" && measurement.Value == "L: Anzahl = 2420; Dichte = 2390 mm2; Hornhautdicke = 529 um");
        Assert.Contains("6228R: Anzahl = 2530; Dichte = 2488 mm2; Hornhautdicke = 534 um", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L: Anzahl = 2420; Dichte = 2390 mm2; Hornhautdicke = 529 um", xdt, StringComparison.Ordinal);
        Assert.Contains("6227Kontrolle Endothelmessung", xdt, StringComparison.Ordinal);
        Assert.Contains("6302em3000-right.jpg", xdt, StringComparison.Ordinal);
        Assert.Contains("6302em3000-left.bmp", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyEm4000Profile_ShouldParseAndExportCellDensityCctCommentsAndImageReferences()
    {
        var parseResult = _tomeyEmParser.ParseFile(GetTomeyFixturePath("EM4000", "EM4000_reference.csv"));
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTomeyEm4000Default();
        var xdt = BuildXdt(CreatePatientData("EM4000"), parseResult, exportProfile);

        Assert.Empty(parseResult.Issues);
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Common/ModelName" && measurement.Value == "EM-4000");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='EM']/Endothelium/R/CellDensity" && measurement.Value == "2440");
        Assert.Contains(parseResult.Measurements, measurement => measurement.SourcePath == "Measure[@Type='EM']/Endothelium/L/CCT/MedistarLine" && measurement.Value == "L CCT = 527");
        Assert.Contains("6228R CD = 2440", xdt, StringComparison.Ordinal);
        Assert.Contains("6228R CCT = 531", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L CD = 2388", xdt, StringComparison.Ordinal);
        Assert.Contains("6228L CCT = 527", xdt, StringComparison.Ordinal);
        Assert.Contains("6227Kontrolle Zellmessung", xdt, StringComparison.Ordinal);
        Assert.Contains("6302em4000-right.jpg", xdt, StringComparison.Ordinal);
        Assert.Contains("6302em4000-left.jpg", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", xdt, StringComparison.Ordinal);
        Assert.DoesNotContain("--", xdt, StringComparison.Ordinal);
    }

    [Fact]
    public void NewReferenceBackedBuiltIns_ShouldBeValid()
    {
        var deviceProfiles = new[]
        {
            DefaultDeviceProfileDefinitions.CreateNidekArk510ADefault(),
            DefaultDeviceProfileDefinitions.CreateNidekArk560ADefault(),
            DefaultDeviceProfileDefinitions.CreateNidekLm1800PDefault(),
            DefaultDeviceProfileDefinitions.CreateShinNipponAccurefR800Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponAccurefK900Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponDl1000Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponDl800Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponDl900Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponNct200Default(),
            DefaultDeviceProfileDefinitions.CreateShinNipponSlm4000Default(),
            DefaultDeviceProfileDefinitions.CreateHuvitzHrk8000ADefault(),
            DefaultDeviceProfileDefinitions.CreateHuvitzHrk9000ADefault(),
            DefaultDeviceProfileDefinitions.CreateHuvitzHnt1PDefault(),
            DefaultDeviceProfileDefinitions.CreateHuvitzHtr1ADefault(),
            DefaultDeviceProfileDefinitions.CreateTomeyCf2000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyTl2000CDefault(),
            DefaultDeviceProfileDefinitions.CreateTomeyTl6000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyTl7000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyMr6000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyTop1000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyEm3000Default(),
            DefaultDeviceProfileDefinitions.CreateTomeyEm4000Default()
        };
        var exportProfiles = new[]
        {
            DefaultExportProfileDefinitions.CreateMedistarNidekArk510ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk560ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm1800PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefR800Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponDl1000Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponDl800Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponDl900Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponNct200Default(),
            DefaultExportProfileDefinitions.CreateMedistarShinNipponSlm4000Default(),
            DefaultExportProfileDefinitions.CreateMedistarHuvitzHrk8000ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarHuvitzHrk9000ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarHuvitzHnt1PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarHuvitzHtr1ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyCf2000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyTl6000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyTl7000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyMr6000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyTop1000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyEm3000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTomeyEm4000Default()
        };
        var interfaceProfiles = new[]
        {
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk510ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk560ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm1800PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponAccurefR800Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl1000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl800Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponDl900Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponNct200Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponSlm4000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHrk8000ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHrk9000ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHnt1PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHtr1ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyCf2000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl6000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl7000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyMr6000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTop1000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyEm3000Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTomeyEm4000Default()
        };

        Assert.All(deviceProfiles, profile => Assert.Empty(DeviceProfileDefinitionValidator.Validate(profile)));
        Assert.All(exportProfiles, profile => Assert.Empty(ExportProfileDefinitionValidator.Validate(profile)));
        Assert.All(interfaceProfiles, profile => Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile)));
        Assert.All(interfaceProfiles, profile => Assert.False(profile.IsActive));
    }

    [Fact]
    public void HuvitzTextPreview_ShouldAcceptNonXmlDeviceFileInBaukastenPreview()
    {
        var service = new BuilderManualProcessingPreviewService();
        var aisPath = WriteTempGdt();
        var devicePath = GetHuvitzFixturePath("HTR1A", "HTR1A_reference_text.txt");

        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarHuvitzHtr1ADefault(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateHuvitzHtr1ADefault(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarHuvitzHtr1ADefault(),
            AisFilePath: aisPath,
            DeviceFilePath: devicePath));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains("6228R.:S=+ 0.75 Z=- 1.25*110 PD= 63", result.ExportContent, StringComparison.Ordinal);
    }

    [Fact]
    public void ShinNipponTextPreview_ShouldAcceptNonXmlDeviceFileInBaukastenPreview()
    {
        var service = new BuilderManualProcessingPreviewService();
        var aisPath = WriteTempGdt();
        var devicePath = GetShinNipponFixturePath("AccurefK900", "AccurefK900_reference_text.txt");

        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateShinNipponAccurefK900Default(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarShinNipponAccurefK900Default(),
            AisFilePath: aisPath,
            DeviceFilePath: devicePath));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='REF']/REF/R/MedistarLine");
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='KM']/KM/MedistarLine1");
        Assert.Contains("6228R.:S=+ 0.25 Z=- 0.75*141 PD= 59 VD= 12", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6221R: R1=7.67 *173 R2=7.57 * 83 // L: R1=7.68 *175 R2=7.52 * 85", result.ExportContent, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyPreview_ShouldAcceptNonXmlDeviceFileInBaukastenPreview()
    {
        var service = new BuilderManualProcessingPreviewService();
        var aisPath = WriteTempGdt();
        var devicePath = GetTomeyFixturePath("TL2000C", "TL2000C_reference.csv");

        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateTomeyTl2000CDefault(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarTomeyTl2000CDefault(),
            AisFilePath: aisPath,
            DeviceFilePath: devicePath));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains("6228R.:S=+ 1.00 Z=- 0.25*103 P=0.75 OUT 1.00 UP PD= 59 A=+ 0.25 A2=+ 1.25", result.ExportContent, StringComparison.Ordinal);
    }

    [Fact]
    public void TomeyEmPreview_ShouldAcceptEndothelialCsvInBaukastenPreview()
    {
        var service = new BuilderManualProcessingPreviewService();
        var aisPath = WriteTempGdt();
        var devicePath = GetTomeyFixturePath("EM3000", "EM3000_reference.csv");

        var result = service.BuildPreview(new BuilderManualProcessingPreviewRequest(
            InterfaceProfile: DefaultInterfaceProfileDefinitions.CreateMedistarTomeyEm3000Default(),
            DeviceProfile: DefaultDeviceProfileDefinitions.CreateTomeyEm3000Default(),
            ExportProfile: DefaultExportProfileDefinitions.CreateMedistarTomeyEm3000Default(),
            AisFilePath: aisPath,
            DeviceFilePath: devicePath));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='EM']/Endothelium/R/MedistarLine");
        Assert.Contains("6228R: Anzahl = 2530; Dichte = 2488 mm2; Hornhautdicke = 534 um", result.ExportContent, StringComparison.Ordinal);
        Assert.Contains("6302em3000-right.jpg", result.ExportContent, StringComparison.Ordinal);
    }

    private static PatientData CreatePatientData(string examinationType)
    {
        return new PatientData(
            PatientNumber: "PAT-REF",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: examinationType);
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "reference-device.xml");
        File.WriteAllText(path, content);
        return path;
    }

    private string BuildXdt(PatientData patient, DeviceParseResult parseResult, ExportProfileDefinition exportProfile)
    {
        var mappingResult = _mappingEngine.Map(patient, parseResult.Measurements, _mappingAdapter.Adapt(exportProfile));
        var xdt = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(xdt.Issues);
        return xdt.Content;
    }

    private static string GetHuvitzFixturePath(string familyFolder, string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Huvitz", familyFolder, fileName);
    }

    private static string GetShinNipponFixturePath(string familyFolder, string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "ShinNippon", familyFolder, fileName);
    }

    private static string GetTomeyFixturePath(string familyFolder, string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Tomey", familyFolder, fileName);
    }

    private static string WriteTempGdt()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "patient.gdt");
        File.WriteAllText(path, """
            01380006302
            014810000304
            01092063
            014921802.10
            0143000PAT-REF
            0113101Test
            0133102Person
            015310301012000
            0128402HTR1A
            """);
        return path;
    }
}
