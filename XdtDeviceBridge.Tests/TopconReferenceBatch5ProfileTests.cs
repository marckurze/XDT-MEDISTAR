using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class TopconReferenceBatch5ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void Cl300PdlFixture_ShouldUseCl300FamilyLensmeterParserAnd6228Export()
    {
        var result = _parser.ParseFile(GetTopconFixturePath("CL300PDL", "M-Serial0001_20260605_120000_TOPCON_CL-300PDL.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/ModelName", "CL-300PDL");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/R/MedistarLine", "R.:S=+ 1.25 Z=- 0.50* 32 A=+ 0.75 A2=+ 1.25 PD= 59");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/L/MedistarLine", "L.:S=+ 0.75 Z=- 0.25*142 A2=+ 0.25");

        var mapping = Map(result, DefaultExportProfileDefinitions.CreateMedistarTopconCl300PdlDefault(), "CL300PDL");

        Assert.False(mapping.HasErrors, string.Join(Environment.NewLine, mapping.Issues.Select(issue => issue.Message)));
        Assert.Equal(
            new[]
            {
                "R.:S=+ 1.25 Z=- 0.50* 32 A=+ 0.75 A2=+ 1.25 PD= 59",
                "L.:S=+ 0.75 Z=- 0.25*142 A2=+ 0.25"
            },
            mapping.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value).ToArray());
        Assert.DoesNotContain(mapping.Records, record => record.FieldCode is "6205" or "6220" or "6221" or "6227" or "6330");
    }

    [Fact]
    public void Rm800Fixture_ShouldUseTopconRefParserAnd6228Only()
    {
        var result = _parser.ParseFile(GetTopconFixturePath("RM800", "M-Serial0001_20260605_120000_TOPCON_RM-800.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/ModelName", "RM-800");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=- 3.75 Z=- 1.25* 98 PD= 66 VD= 12.00");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/L/MedistarLine", "L.:S=- 4.25 Z=- 2.00*105");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='KM']/KM/Medistar", StringComparison.Ordinal));

        var mapping = Map(result, DefaultExportProfileDefinitions.CreateMedistarTopconRm800Default(), "RM800");

        Assert.False(mapping.HasErrors, string.Join(Environment.NewLine, mapping.Issues.Select(issue => issue.Message)));
        Assert.Equal(2, mapping.Records.Count(record => record.FieldCode == "6228"));
        Assert.DoesNotContain(mapping.Records, record => record.FieldCode is "6205" or "6220" or "6221" or "6227" or "6330");
    }

    [Fact]
    public void Trk3OmniaFixture_ShouldUseTrk2PFamilyParserAndMessartFields()
    {
        var result = _parser.ParseFile(GetTopconFixturePath("TRK3Omnia", "M-Serial0001_20260605_120000_TOPCON_TRK-3.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/ModelName", "TRK-3");
        AssertMeasurement(result, "Measure[@Type='REF']/REF/R/MedistarLine", "R.:S=+ 0.25 Z=- 0.25*  2 PD= 68 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='KM']/KM/MedistarLine1", "R: R1=7.74 43.50 *3 R2=7.67 44.00 *93 // L: R1=7.72 43.75 *175 R2=7.65 44.25 *85");
        AssertMeasurement(result, "Measure[@Type='CCT']/Pachy/MedistarLine", "RA: 0.529   // LA: 0.525");
        AssertMeasurement(result, "Measure[@Type='TM']/Tono/TonoListLine", "R = 17 18 17 [17.5] // L = 17 16 16 [16.3] mmHg 12:00");

        var mapping = Map(result, DefaultExportProfileDefinitions.CreateMedistarTopconTrk3OmniaDefault(), "TRK3");

        Assert.False(mapping.HasErrors, string.Join(Environment.NewLine, mapping.Issues.Select(issue => issue.Message)));
        Assert.Equal(2, mapping.Records.Count(record => record.FieldCode == "6228"));
        Assert.Equal(2, mapping.Records.Count(record => record.FieldCode == "6221"));
        Assert.Equal(2, mapping.Records.Count(record => record.FieldCode == "6220"));
        Assert.Equal(4, mapping.Records.Count(record => record.FieldCode == "6205"));
        Assert.DoesNotContain(mapping.Records, record => record.FieldCode is "6227" or "6330");
    }

    [Fact]
    public void BuiltInTopconReferenceBatch5Profiles_ShouldBePresentAndValid()
    {
        AssertValidTopconCandidate(
            DefaultDeviceProfileDefinitions.CreateTopconCl300PdlDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCl300PdlDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300PdlDefault(),
            "CL-300PDL",
            "6228");
        AssertValidTopconCandidate(
            DefaultDeviceProfileDefinitions.CreateTopconRm800Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconRm800Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconRm800Default(),
            "RM-800",
            "6228");
        AssertValidTopconCandidate(
            DefaultDeviceProfileDefinitions.CreateTopconTrk3OmniaDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconTrk3OmniaDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk3OmniaDefault(),
            "TRK-3 Omnia",
            "6205");
    }

    [Theory]
    [InlineData("interface-medistar-topcon-cl300pdl-default", "package-medistar-topcon-cl-300pdl-v1")]
    [InlineData("interface-medistar-topcon-rm800-default", "package-medistar-topcon-rm-800-v1")]
    [InlineData("interface-medistar-topcon-trk3-omnia-default", "package-medistar-topcon-trk-3-omnia-v1")]
    public void TemplateSelection_ShouldCreateTopconReferenceBatch5Packages(string interfaceProfileId, string expectedPackageId)
    {
        var catalog = CreateCatalogWithReferenceBatch5TopconProfiles();
        var service = new TemplatePackageExportSelectionService();

        var result = service.CreateForInterfaceProfile(catalog, interfaceProfileId, new DateTimeOffset(2026, 6, 5, 12, 0, 0, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(expectedPackageId, result.Request!.Package.Metadata.Id);
        Assert.Contains(result.Request.Package.IncludedProfiles, profile => profile.Id == interfaceProfileId);
        Assert.Contains(result.Request.Package.IncludedProfiles, profile => profile.Id == "ais-medistar-default");
    }

    private MappingResult Map(DeviceParseResult parseResult, ExportProfileDefinition exportProfile, string examinationType)
    {
        var rules = _mappingAdapter.Adapt(exportProfile);
        return _mappingEngine.Map(CreatePatientData(examinationType), parseResult.Measurements, rules);
    }

    private static void AssertValidTopconCandidate(
        DeviceProfileDefinition deviceProfile,
        ExportProfileDefinition exportProfile,
        InterfaceProfileDefinition interfaceProfile,
        string model,
        string expectedFieldCode)
    {
        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Equal(model, deviceProfile.Model);
        Assert.True(deviceProfile.Metadata.IsBuiltIn);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal(deviceProfile.Metadata.Id, exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == expectedFieldCode);
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal(deviceProfile.Metadata.Id, interfaceProfile.DeviceProfileId);
        Assert.Equal(exportProfile.Metadata.Id, interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private static ProfileCatalog CreateCatalogWithReferenceBatch5TopconProfiles()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[]
            {
                DefaultDeviceProfileDefinitions.CreateTopconCl300PdlDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconRm800Default(),
                DefaultDeviceProfileDefinitions.CreateTopconTrk3OmniaDefault()
            },
            ExportProfiles: new[]
            {
                DefaultExportProfileDefinitions.CreateMedistarTopconCl300PdlDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconRm800Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconTrk3OmniaDefault()
            },
            InterfaceProfiles: new[]
            {
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300PdlDefault(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconRm800Default(),
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk3OmniaDefault()
            });
    }

    private static PatientData CreatePatientData(string examinationType)
    {
        return new PatientData(
            PatientNumber: "PAT-TOPCON-B5",
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

    private static string GetTopconFixturePath(string folder, string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", folder, fileName);
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string expectedValue)
    {
        Assert.Contains(result.Measurements, measurement =>
            measurement.SourcePath == sourcePath
            && measurement.Value == expectedValue);
    }
}
