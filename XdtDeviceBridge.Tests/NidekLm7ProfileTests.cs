using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekLm7ProfileTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void XmlWithArbitraryUppercaseFileName_ShouldBeRecognizedAndParsed()
    {
        var tempFolder = CreateTempFolder();
        var xmlPath = Path.Combine(tempFolder, "beliebige-lm7-messung.XML");
        File.Copy(GetLm7FixturePath("NIDEK LM7.xml"), xmlPath);

        var classification = new ImportFileClassifier().Classify(xmlPath);
        var parseResult = _parser.ParseFile(xmlPath);

        Assert.Equal(ImportFileKind.DeviceXml, classification.Kind);
        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "NIDEK");
        AssertMeasurement(parseResult, "Common/ModelName", "LM-7");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "6.25");
    }

    [Fact]
    public void Stylesheet_ShouldNotBeClassifiedAsDeviceFile()
    {
        var classification = new ImportFileClassifier().Classify(GetLm7FixturePath("NIDEK_LM_Stylesheet.xsl"));

        Assert.Equal(ImportFileKind.Unknown, classification.Kind);
    }

    [Fact]
    public void ParseLm7Sample_ShouldReadRealMeasurementsAndAliases()
    {
        var parseResult = _parser.ParseFile(GetLm7FixturePath("NIDEK LM7.xml"));

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Common/Company", "NIDEK");
        AssertMeasurement(parseResult, "Common/ModelName", "LM-7");
        AssertMeasurement(parseResult, "Common/Version", "NIDEK_V1.00");
        AssertMeasurement(parseResult, "Common/Date", "2026-05-18");
        AssertMeasurement(parseResult, "Common/Time", "13:07:57");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/MeasureMode", "AutoProgressive");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/CylinderMode", "-");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/PrismMode", "xy");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/AddMode", "add");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphare", "6.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "6.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Cylinder", "-3.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Axis", "3");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Sphere", "6.50");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Cylinder", "-2.75");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/Axis", "170");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/ADD", "1.50");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/NearSphare", "8.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/L/NearSphere", "8.00");
    }

    [Fact]
    public void ParseCanonicalSphereNames_ShouldAlsoExposeLegacySphareAliases()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>LM-7P</ModelName>
              </Common>
              <Measure Type="LM">
                <LM>
                  <R>
                    <Sphere unit="D">1.25</Sphere>
                    <Cylinder unit="D">-0.50</Cylinder>
                    <Axis unit="deg">12</Axis>
                    <NearSphere unit="D">2.00</NearSphere>
                    <NearSphere2 unit="D">2.25</NearSphere2>
                  </R>
                </LM>
              </Measure>
            </Ophthalmology>
            """);

        var parseResult = _parser.ParseFile(path);

        Assert.Empty(parseResult.Issues);
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphere", "1.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/Sphare", "1.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/NearSphere", "2.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/NearSphare", "2.00");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/NearSphere2", "2.25");
        AssertMeasurement(parseResult, "Measure[@Type='LM']/LM/R/NearSphare2", "2.25");
    }

    [Fact]
    public void MedistarExportForLm7Sample_ShouldUseLensmeterFormatAndOmitMissingOptionals()
    {
        var result = MapWithLm7Export(GetLm7FixturePath("NIDEK LM7.xml"));

        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        var resultLines = result.Records.Where(record => record.FieldCode == "6228").Select(record => record.Value ?? string.Empty).ToArray();
        Assert.Equal(
            new[]
            {
                "R.:S=+ 6.25 Z=- 3.25*  3",
                "L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50"
            },
            resultLines);
        Assert.DoesNotContain(resultLines, line => line.Contains("PD=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("P=", StringComparison.Ordinal));
        Assert.DoesNotContain(resultLines, line => line.Contains("A2=", StringComparison.Ordinal));
    }

    [Fact]
    public void ParserMeasurements_ShouldExposePathsUsedByLm7ExportProfile()
    {
        var parseResult = _parser.ParseFile(GetLm7FixturePath("NIDEK LM7.xml"));
        var sourcePaths = parseResult.Measurements
            .Select(measurement => measurement.SourcePath)
            .ToHashSet(StringComparer.Ordinal);
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();

        foreach (var rule in exportProfile.Rules.Where(rule => rule.TargetFieldCode == "6228"))
        {
            Assert.False(string.IsNullOrWhiteSpace(rule.SourcePath));
            Assert.StartsWith("Device.", rule.SourcePath, StringComparison.Ordinal);
            Assert.Contains(rule.SourcePath![7..], sourcePaths);
        }
    }

    [Fact]
    public void XdtExportForLm7Sample_ShouldUseAisExaminationType()
    {
        var mappingResult = MapWithLm7Export(GetLm7FixturePath("NIDEK LM7.xml"));

        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402LM7", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 6.25 Z=- 3.25*  3", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("MEDISTAR Eintrag", exportResult.Content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("- 6.25 Z=- 3.25* 32", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void PersistedLegacyLm7BuiltInExport_ShouldBeRepairedBeforePreviewExport()
    {
        var paths = CreateAppDataPaths();
        var catalogService = new ProfileCatalogService();
        catalogService.Save(paths, new ProfileCatalog(
            AisProfiles: Array.Empty<AisProfile>(),
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: new[] { CreateLegacyLm7ExportProfile() },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        catalogService.EnsureDefaultProfiles(paths);
        var exportProfile = catalogService.Load(paths).ExportProfiles.Single(profile => profile.Metadata.Id == "export-medistar-nidek-lm7-default");
        var measurements = _parser.ParseFile(GetLm7FixturePath("NIDEK LM7.xml")).Measurements;
        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, _mappingAdapter.Adapt(exportProfile));
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402LM7", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 6.25 Z=- 3.25*  3", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("R.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("L.:S= Z=*", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("P=              PD=", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("P=", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("PD=", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInLm7Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default();

        Assert.Equal("NIDEK", deviceProfile.Manufacturer);
        Assert.Equal("LM7", deviceProfile.Model);
        Assert.Equal("Lensmeter", deviceProfile.DeviceType);
        Assert.True(deviceProfile.Metadata.IsBuiltIn);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/Sphere" && measurement.IsRequired);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/L/ADD");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-nidek-lm7-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.SourcePath == "Device.Measure[@Type='LM']/LM/L/MedistarLine");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-nidek-lm7-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-lm7-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private MappingResult MapWithLm7Export(string xmlPath)
    {
        var measurements = _parser.ParseFile(xmlPath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());

        return _mappingEngine.Map(CreatePatientData(), measurements, rules);
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-LM7",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "LM7");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetLm7FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "LM7", fileName);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string WriteTempXml(string content)
    {
        var path = Path.Combine(CreateTempFolder(), "lm7-canonical.xml");
        File.WriteAllText(path, content);
        return path;
    }

    private static ExportProfileDefinition CreateLegacyLm7ExportProfile()
    {
        var current = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        return current with
        {
            Rules = current.Rules.Take(6)
                .Concat(new[]
                {
                    new ExportRuleDefinition(
                        "7",
                        "6228",
                        "LensmeterResultRight",
                        ExportRuleType.Template,
                        null,
                        "R.:S={Device.R/LM/Median/Sphere:Diopter} Z={Device.R/LM/Median/Cylinder:Diopter}*{Device.R/LM/Median/Axis:Axis} P={Device.R/LM/Median/PrismHorizontal:Prism} {Device.R/LM/Median/PrismHorizontalBase:Raw} {Device.R/LM/Median/PrismVertical:Prism} {Device.R/LM/Median/PrismVerticalBase:Raw}              PD={Device.PD/Distance:Pd}",
                        7,
                        true,
                        "Legacy LM7 right lensmeter template with unresolved median paths."),
                    new ExportRuleDefinition(
                        "8",
                        "6228",
                        "LensmeterResultLeft",
                        ExportRuleType.Template,
                        null,
                        "L.:S={Device.L/LM/Median/Sphere:Diopter} Z={Device.L/LM/Median/Cylinder:Diopter}*{Device.L/LM/Median/Axis:Axis} P={Device.L/LM/Median/PrismHorizontal:Prism} {Device.L/LM/Median/PrismHorizontalBase:Raw} {Device.L/LM/Median/PrismVertical:Prism} {Device.L/LM/Median/PrismVerticalBase:Raw}",
                        8,
                        true,
                        "Legacy LM7 left lensmeter template with unresolved median paths.")
                })
                .ToArray()
        };
    }
}
