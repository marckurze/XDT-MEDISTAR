using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekNt530PProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void XmlWithArbitraryUppercaseFileName_ShouldBeRecognizedAndParsed()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "beliebige-nt530p-messung.XML");
        File.Copy(GetNt530PFixturePath(), xmlPath);

        var classification = new ImportFileClassifier().Classify(xmlPath);
        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Equal(ImportFileKind.DeviceXml, classification.Kind);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Company", "NIDEK");
        AssertMeasurement(parseResult, "ModelName", "NT-530P");
        AssertMeasurement(parseResult, "R/NT/NTList[@No='1']/mmHg", "16");
    }

    [Theory]
    [InlineData("NTP_20260518120719RP1.jpg")]
    [InlineData("NTP_20260518120719LP1.jpeg")]
    public void JpgImages_ShouldNotBeClassifiedAsDeviceXml(string fileName)
    {
        var classification = new ImportFileClassifier().Classify(Path.Combine("C:\\Import", fileName));

        Assert.Equal(ImportFileKind.AttachmentImage, classification.Kind);
    }

    [Fact]
    public void ParseNt530PSample_ShouldReadRealTonometryPachymetryAndImageReferences()
    {
        var parseResult = _parser.ParseFile(GetNt530PFixturePath());

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Company", "NIDEK");
        AssertMeasurement(parseResult, "ModelName", "NT-530P");
        AssertMeasurement(parseResult, "ROMVersion", "1.11 /1.00");
        AssertMeasurement(parseResult, "Version", "1.00");
        AssertMeasurement(parseResult, "Date", "2026/05/18");
        AssertMeasurement(parseResult, "Time", "12:07:19");
        AssertMeasurement(parseResult, "Patient/No.", "8914");
        AssertMeasurement(parseResult, "Comment", "OCULUS  NT-530P");

        AssertMeasurement(parseResult, "R/NT/NTList[@No='1']/mmHg", "16");
        AssertMeasurement(parseResult, "R/NT/NTList[@No='1']/kPa", "2.1");
        AssertMeasurement(parseResult, "R/NT/NTList[@No='2']/mmHg", "20");
        AssertMeasurement(parseResult, "R/NT/NTList[@No='2']/kPa", "2.7");
        AssertMeasurement(parseResult, "R/NT/NTAverage/mmHg", "18.0");
        AssertMeasurement(parseResult, "R/NT/NTAverage/kPa", "2.40");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Param1", "550um");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Param2", "0.0400");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/CCT", "596um");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Measured/mmHg", "18.0");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Measured/kPa", "2.40");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Corrected/mmHg", "16.2");
        AssertMeasurement(parseResult, "R/NT/CorrectedIOP/Corrected/kPa", "2.15");
        AssertMeasurement(parseResult, "R/PACHY/PACHYList[@No='1']/Thickness", "596");
        AssertMeasurement(parseResult, "R/PACHY/PACHYAverage/Thickness", "596");
        AssertMeasurement(parseResult, "R/PACHY/PACHYImage", "NTP_              _20260518120719RP1.jpg");

        AssertMeasurement(parseResult, "L/NT/NTList[@No='1']/mmHg", "18");
        AssertMeasurement(parseResult, "L/NT/NTAverage/mmHg", "18.0");
        AssertMeasurement(parseResult, "L/NT/CorrectedIOP/Measured/mmHg", "18.0");
        AssertMeasurement(parseResult, "L/NT/CorrectedIOP/Corrected/mmHg", "16.2");
        AssertMeasurement(parseResult, "L/NT/CorrectedIOP/Param1", "550um");
        AssertMeasurement(parseResult, "L/NT/CorrectedIOP/Param2", "0.0400");
        AssertMeasurement(parseResult, "L/NT/CorrectedIOP/CCT", "596um");
        AssertMeasurement(parseResult, "L/PACHY/PACHYList[@No='1']/Thickness", "591");
        AssertMeasurement(parseResult, "L/PACHY/PACHYList[@No='2']/Thickness", "600");
        AssertMeasurement(parseResult, "L/PACHY/PACHYAverage/Thickness", "596");
        AssertMeasurement(parseResult, "L/PACHY/PACHYImage", "NTP_              _20260518120719LP1.jpg");
    }

    [Fact]
    public void ParserMeasurements_ShouldExposePachyAndTonoMedistarLines()
    {
        var parseResult = _parser.ParseFile(GetNt530PFixturePath());

        AssertMeasurement(parseResult, "Measure[@Type='NT530P']/Pachy/MedistarLine", "RA: 0.596   // LA: 0.596");
        var tonoLine = Assert.Single(
            parseResult.Measurements,
            measurement => measurement.SourcePath == "Measure[@Type='NT530P']/Tono/MedistarLine").Value;
        Assert.Contains("PR: 596 [596] µm", tonoLine, StringComparison.Ordinal);
        Assert.Contains("PL: 591 600 [596] µm", tonoLine, StringComparison.Ordinal);
        Assert.Contains("PR: Gemessen = 18.0 mmHg; Korrigiert = 16.2 mmHg", tonoLine, StringComparison.Ordinal);
        Assert.Contains("PL: Gemessen = 18.0 mmHg; Korrigiert = 16.2 mmHg", tonoLine, StringComparison.Ordinal);
        Assert.Contains("R = 16 20 [18.0] // L = 18 [18.0] mmHg", tonoLine, StringComparison.Ordinal);
        Assert.Contains("12:07", tonoLine, StringComparison.Ordinal);
        Assert.Contains("NT-530P Messung", tonoLine, StringComparison.Ordinal);
        Assert.DoesNotContain("Gemessen =  mmHg", tonoLine, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForNt530PSample_ShouldUseFields6220And6205()
    {
        var result = MapWithNt530PExport();

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var pachyRecord = Assert.Single(result.Records, record => record.FieldCode == "6220");
        var tonoRecord = Assert.Single(result.Records, record => record.FieldCode == "6205");
        Assert.Equal("RA: 0.596   // LA: 0.596", pachyRecord.Value);
        Assert.Contains("PR: 596 [596] µm", tonoRecord.Value, StringComparison.Ordinal);
        Assert.Contains("PL: 591 600 [596] µm", tonoRecord.Value, StringComparison.Ordinal);
        Assert.Contains("R = 16 20 [18.0] // L = 18 [18.0] mmHg", tonoRecord.Value, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Records, record => record.FieldCode == "6228");
    }

    [Fact]
    public void XdtExportForNt530PSample_ShouldUseAisExaminationTypeAndNo6228DeviceLine()
    {
        var mappingResult = MapWithNt530PExport();
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402NT530P", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6220RA: 0.596   // LA: 0.596", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6205PR: 596 [596] µm", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228PR:", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228R =", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInNt530PProfiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530PDefault();

        Assert.Equal("NIDEK", deviceProfile.Manufacturer);
        Assert.Equal("NT-530P", deviceProfile.Model);
        Assert.Contains("Tonometer", deviceProfile.DeviceType);
        Assert.Contains("Pachymeter", deviceProfile.DeviceType);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='NT530P']/Pachy/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='NT530P']/Tono/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-nidek-nt530p-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6220" && rule.SourcePath == "Device.Measure[@Type='NT530P']/Pachy/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6205" && rule.SourcePath == "Device.Measure[@Type='NT530P']/Tono/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode == "6228");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-nidek-nt530p-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-nt530p-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void PersistedLegacyNt530PBuiltInExport_ShouldBeRepairedBeforePreviewExport()
    {
        var paths = CreateAppDataPaths();
        var catalogService = new ProfileCatalogService();
        catalogService.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyNt530PExportProfile(isBuiltIn: true) },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        catalogService.EnsureDefaultProfiles(paths);
        var exportProfile = catalogService.Load(paths).ExportProfiles.Single(profile => profile.Metadata.Id == "export-medistar-nidek-nt530p-default");
        var measurements = _parser.ParseFile(GetNt530PFixturePath()).Measurements;
        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, _mappingAdapter.Adapt(exportProfile));

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Contains(mappingResult.Records, record => record.FieldCode == "6220" && record.Value == "RA: 0.596   // LA: 0.596");
        Assert.Contains(mappingResult.Records, record => record.FieldCode == "6205" && record.Value!.Contains("R = 16 20 [18.0] // L = 18 [18.0] mmHg", StringComparison.Ordinal));
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode == "6228");
        Assert.DoesNotContain(exportProfile.Rules, rule => ContainsLegacyNt530PPath(rule.SourcePath));
        Assert.DoesNotContain(exportProfile.Rules, rule => ContainsLegacyNt530PPath(rule.OutputTemplate));
    }

    private MappingResult MapWithNt530PExport()
    {
        var measurements = _parser.ParseFile(GetNt530PFixturePath()).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-NT530P",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "NT530P");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetNt530PFixturePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "NT530P", "NIDEK_NT530P.xml");
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static ExportProfileDefinition CreateLegacyNt530PExportProfile(bool isBuiltIn)
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault();
        return current with
        {
            Metadata = current.Metadata with
            {
                IsBuiltIn = isBuiltIn,
                IsUserDefined = !isBuiltIn
            },
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "LegacyPachymetryRight",
                        ExportRuleType.Template,
                        null,
                        "PR: {Device.Data/R/PACHY/PACHYList[@No='1']/Thickness:Pachy} [{Device.Data/R/PACHY/PACHYAverage/Thickness:Pachy}] µm",
                        7,
                        true,
                        "Legacy NT530P pachymetry template using field 6228 and Data-prefixed paths."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "LegacyTonometryBothEyes",
                        ExportRuleType.Template,
                        null,
                        "R = {Device.Data/R/NT/NTList[@No='1']/mmHg:Iop} [{Device.Data/R/NT/NTAverage/mmHg:Iop}] // L = {Device.Data/L/NT/NTList[@No='1']/mmHg:Iop} [{Device.Data/L/NT/NTAverage/mmHg:Iop}] mmHg",
                        8,
                        true,
                        "Legacy NT530P tonometry template using field 6228 and Data-prefixed paths.")
                })
                .ToArray()
        };
    }

    private static bool ContainsLegacyNt530PPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("Device.Data/", StringComparison.OrdinalIgnoreCase);
    }
}
