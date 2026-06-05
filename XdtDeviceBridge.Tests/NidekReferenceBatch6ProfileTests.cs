using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekReferenceBatch6ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Theory]
    [InlineData("AR-1", "device-nidek-ar1-default", "export-medistar-nidek-ar1-default", "interface-medistar-nidek-ar1-default")]
    [InlineData("AR-1S", "device-nidek-ar1s-default", "export-medistar-nidek-ar1s-default", "interface-medistar-nidek-ar1s-default")]
    [InlineData("AR-310A", "device-nidek-ar310a-default", "export-medistar-nidek-ar310a-default", "interface-medistar-nidek-ar310a-default")]
    public void BuiltInArXmlVariants_ShouldBeAvailableAndValid(
        string model,
        string deviceProfileId,
        string exportProfileId,
        string interfaceProfileId)
    {
        var catalog = LoadDefaultCatalog();

        var deviceProfile = Assert.Single(catalog.DeviceProfiles, profile => profile.Metadata.Id == deviceProfileId);
        var exportProfile = Assert.Single(catalog.ExportProfiles, profile => profile.Metadata.Id == exportProfileId);
        var interfaceProfile = Assert.Single(catalog.InterfaceProfiles, profile => profile.Metadata.Id == interfaceProfileId);

        Assert.Equal("NIDEK", deviceProfile.Manufacturer);
        Assert.Equal(model, deviceProfile.Model);
        Assert.Contains("Autorefractor", deviceProfile.DeviceType, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));
        Assert.Equal(deviceProfileId, exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));
        Assert.Equal(deviceProfileId, interfaceProfile.DeviceProfileId);
        Assert.Equal(exportProfileId, interfaceProfile.ExportProfileId);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void Ar1SExport_ShouldMapAutorefTo6228AndSubjectiveTo6227()
    {
        var xmlPath = WriteTempXml("""
            <Data>
              <Company>NIDEK</Company>
              <ModelName>AR-1S</ModelName>
              <Date>2026-06-05</Date>
              <Time>12:00:00</Time>
              <VD>12.00</VD>
              <R>
                <AR><ARMedian><Sphere>+0.25</Sphere><Cylinder>-0.75</Cylinder><Axis>141</Axis></ARMedian></AR>
                <SR><Sphere>+2.25</Sphere><Cylinder>-1.50</Cylinder><Axis>93</Axis></SR>
              </R>
              <L>
                <AR><ARMedian><Sphere>+0.00</Sphere><Cylinder>-0.00</Cylinder><Axis>0</Axis></ARMedian></AR>
                <SR><Sphere>+2.25</Sphere><Cylinder>-2.00</Cylinder><Axis>58</Axis></SR>
              </L>
              <PD><PDList No="1"><FarPD>59</FarPD></PDList></PD>
            </Data>
            """);

        var result = Map(xmlPath, DefaultExportProfileDefinitions.CreateMedistarNidekAr1SDefault());

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && record.Value == "R.:S=+ 0.25 Z=- 0.75*141 PD= 59 VD= 12.00 mm");
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && record.Value == "L.:S=+ 0.00 Z=- 0.00*  0");
        Assert.Contains(result.Records, record => record.FieldCode == "6227" && record.Value == "R.:S=+ 2.25 Z=- 1.50* 93");
        Assert.Contains(result.Records, record => record.FieldCode == "6227" && record.Value == "L.:S=+ 2.25 Z=- 2.00* 58");
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6330");
    }

    [Fact]
    public void Lm1800PdExport_ShouldUseExistingLensmeterMedistarLineParser()
    {
        var xmlPath = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>LM-1800PD</ModelName>
              </Common>
              <Measure Type="LM">
                <LM>
                  <R><Sphere>+1.00</Sphere><Cylinder>-0.25</Cylinder><Axis>103</Axis></R>
                  <L><Sphere>+0.25</Sphere><Cylinder>-1.25</Cylinder><Axis>62</Axis><ADD>+0.25</ADD></L>
                </LM>
                <PD><Distance>59</Distance></PD>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(xmlPath);
        var result = Map(xmlPath, DefaultExportProfileDefinitions.CreateMedistarNidekLm1800PdDefault());

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/ModelName", "LM-1800PD");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=+ 1.00 Z=- 0.25*103 PD= 59");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/MedistarLine", "L.:S=+ 0.25 Z=- 1.25* 62 A=+ 0.25");
        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && record.Value == "R.:S=+ 1.00 Z=- 0.25*103 PD= 59");
        Assert.Contains(result.Records, record => record.FieldCode == "6228" && record.Value == "L.:S=+ 0.25 Z=- 1.25* 62 A=+ 0.25");
    }

    [Theory]
    [InlineData("NT-1", "device-nidek-nt1-default", "export-medistar-nidek-nt1-default", "interface-medistar-nidek-nt1-default")]
    [InlineData("NT-1E", "device-nidek-nt1e-default", "export-medistar-nidek-nt1e-default", "interface-medistar-nidek-nt1e-default")]
    [InlineData("NT-1P", "device-nidek-nt1p-default", "export-medistar-nidek-nt1p-default", "interface-medistar-nidek-nt1p-default")]
    [InlineData("NT-510", "device-nidek-nt510-default", "export-medistar-nidek-nt510-default", "interface-medistar-nidek-nt510-default")]
    [InlineData("NT-530", "device-nidek-nt530-default", "export-medistar-nidek-nt530-default", "interface-medistar-nidek-nt530-default")]
    public void BuiltInNtXmlVariants_ShouldReuseNt530PComputedMedistarLines(
        string model,
        string deviceProfileId,
        string exportProfileId,
        string interfaceProfileId)
    {
        var catalog = LoadDefaultCatalog();
        var xmlPath = WriteTempXml(CreateNtXml(model));
        var exportProfile = catalog.ExportProfiles.Single(profile => profile.Metadata.Id == exportProfileId);

        var parseResult = _parser.ParseFile(xmlPath);
        var result = Map(xmlPath, exportProfile);

        Assert.Contains(catalog.DeviceProfiles, profile => profile.Metadata.Id == deviceProfileId && profile.Model == model);
        Assert.Contains(catalog.InterfaceProfiles, profile => profile.Metadata.Id == interfaceProfileId && profile.ExportProfileId == exportProfileId);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "ModelName", model);
        AssertMeasurement(parseResult, "Measure[@Type='NT530P']/Pachy/MedistarLine", "RA: 0.559   // LA: 0.560");
        AssertMeasurement(parseResult, "Measure[@Type='NT530P']/Tono/TonoListLine", "CCT = 560um P  R = 12 11 15 [12.7] // L = 14 13 15 [14.0] mmHg 09:30");
        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Contains(result.Records, record => record.FieldCode == "6205" && record.Value == "Tonometrie");
        Assert.Contains(result.Records, record => record.FieldCode == "6220" && record.Value == "RA: 0.559   // LA: 0.560");
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6228");
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6330");
    }

    private MappingResult Map(string xmlPath, ExportProfileDefinition exportProfile)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        return _mappingEngine.Map(CreatePatientData(), measurements, _mappingAdapter.Adapt(exportProfile));
    }

    private static ProfileCatalog LoadDefaultCatalog()
    {
        var paths = CreateAppDataPaths();
        var catalogService = new ProfileCatalogService();
        catalogService.EnsureDefaultProfiles(paths);
        return catalogService.Load(paths);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-BATCH6",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "Batch6");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "device.xml");
        File.WriteAllText(path, content);
        return path;
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static string CreateNtXml(string model)
    {
        return $$"""
            <Data>
              <Company>NIDEK</Company>
              <ModelName>{{model}}</ModelName>
              <Date>2026/06/05</Date>
              <Time>09:30:00</Time>
              <R>
                <NT>
                  <NTList No="1"><mmHg>12</mmHg></NTList>
                  <NTList No="2"><mmHg>11</mmHg></NTList>
                  <NTList No="3"><mmHg>15</mmHg></NTList>
                  <NTAverage><mmHg>12.7</mmHg></NTAverage>
                  <CorrectedIOP>
                    <Measured><mmHg>12.7</mmHg></Measured>
                    <Corrected><mmHg>12.3</mmHg></Corrected>
                    <Param1>550um</Param1>
                    <Param2>0.0400</Param2>
                    <CCT>559um</CCT>
                  </CorrectedIOP>
                </NT>
                <PACHY>
                  <PACHYList No="1"><Thickness>559</Thickness></PACHYList>
                  <PACHYAverage><Thickness>559</Thickness></PACHYAverage>
                </PACHY>
              </R>
              <L>
                <NT>
                  <NTList No="1"><mmHg>14</mmHg></NTList>
                  <NTList No="2"><mmHg>13</mmHg></NTList>
                  <NTList No="3"><mmHg>15</mmHg></NTList>
                  <NTAverage><mmHg>14.0</mmHg></NTAverage>
                  <CorrectedIOP>
                    <Measured><mmHg>14.0</mmHg></Measured>
                    <Corrected><mmHg>13.6</mmHg></Corrected>
                    <Param1>550um</Param1>
                    <Param2>0.0400</Param2>
                    <CCT>560um</CCT>
                  </CorrectedIOP>
                </NT>
                <PACHY>
                  <PACHYList No="1"><Thickness>560</Thickness></PACHYList>
                  <PACHYAverage><Thickness>560</Thickness></PACHYAverage>
                </PACHY>
              </L>
            </Data>
            """;
    }
}
