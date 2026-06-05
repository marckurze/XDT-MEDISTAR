using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ReferencePackageBuiltInDeviceTests
{
    private readonly XmlDeviceParser _parser = new();
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
    public void NewReferenceBackedBuiltIns_ShouldBeValid()
    {
        var deviceProfiles = new[]
        {
            DefaultDeviceProfileDefinitions.CreateNidekArk510ADefault(),
            DefaultDeviceProfileDefinitions.CreateNidekArk560ADefault(),
            DefaultDeviceProfileDefinitions.CreateNidekLm1800PDefault()
        };
        var exportProfiles = new[]
        {
            DefaultExportProfileDefinitions.CreateMedistarNidekArk510ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekArk560ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm1800PDefault()
        };
        var interfaceProfiles = new[]
        {
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk510ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk560ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm1800PDefault()
        };

        Assert.All(deviceProfiles, profile => Assert.Empty(DeviceProfileDefinitionValidator.Validate(profile)));
        Assert.All(exportProfiles, profile => Assert.Empty(ExportProfileDefinitionValidator.Validate(profile)));
        Assert.All(interfaceProfiles, profile => Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile)));
        Assert.All(interfaceProfiles, profile => Assert.False(profile.IsActive));
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
}
