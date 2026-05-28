using System.Text;
using System.Xml.Linq;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class NidekRt6100ProfileTests
{
    private readonly XmlDeviceParser _xmlParser = new();
    private readonly MedistarHistoricalMeasurementParser _historyParser = new();
    private readonly NidekRt6100InputXmlWriter _writer = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void HistoricalParser_DefaultRt6100SelectionShouldUseNewestExportableV0V1Only()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));

        var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);

        Assert.Equal(2, selected.Count);
        Assert.Equal(
            new[]
            {
                AisHistoricalMeasurementSourceKind.Lensmeter,
                AisHistoricalMeasurementSourceKind.Autorefraction
            },
            selected.Select(record => record.SourceKind));
        Assert.DoesNotContain(selected, record => record.SourceKind is
            AisHistoricalMeasurementSourceKind.Phoropter
            or AisHistoricalMeasurementSourceKind.Prescription
            or AisHistoricalMeasurementSourceKind.AutorefractionSubjective
            or AisHistoricalMeasurementSourceKind.Keratometry
            or AisHistoricalMeasurementSourceKind.Pachymetry
            or AisHistoricalMeasurementSourceKind.Tonometry);
        Assert.All(selected, record => Assert.True(record.IsExportableToCv5000));
    }

    [Fact]
    public void InputWriter_ShouldCreateRt6100XmlFromSelectedMedistarHistory()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);

        var result = _writer.BuildXml(
            new Cv5000ImportSelection(
                history.Patient,
                selected,
                "C:\\Temp",
                NidekRt6100InputXmlWriter.DefaultFileNameTemplate),
            new DateTimeOffset(2026, 5, 28, 10, 31, 35, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(
            Path.Combine("C:\\Temp", "RTImport_4701-1_20260528_103135.xml"),
            result.TargetPath);
        Assert.NotNull(result.XmlContent);

        var document = XDocument.Parse(result.XmlContent!);
        Assert.NotNull(document.Root);
        var root = document.Root!;
        Assert.Equal("Ophthalmology", root.Name.LocalName);
        var common = Assert.Single(root.Elements(), element => element.Name.LocalName == "Common");
        Assert.Equal("NIDEK", ChildValue(common, "Company"));
        Assert.Equal("RT-6100", ChildValue(common, "ModelName"));
        Assert.Equal("NIDEK_RT_V1.00", ChildValue(common, "Version"));
        Assert.Equal("2026.05.28", ChildValue(common, "Date"));
        Assert.Equal("10:31:35", ChildValue(common, "Time"));

        var patient = Child(common, "Patient")!;
        Assert.Equal("4701-1", ChildValue(patient, "No"));
        Assert.Equal("4701-1", ChildValue(patient, "ID"));
        Assert.Equal("Anna", ChildValue(patient, "FirstName"));
        Assert.Equal("Testfrau", ChildValue(patient, "LastName"));
        Assert.Equal("1955.06.12", ChildValue(patient, "DOB"));

        var measure = Assert.Single(root.Elements(), element => element.Name.LocalName == "Measure");
        Assert.Equal("RT", measure.Attributes().Single(attribute => attribute.Name.LocalName == "Type").Value);
        var corrected = Child(measure, "Phoropter")!
            .Elements()
            .Where(element => element.Name.LocalName == "Corrected")
            .ToArray();
        Assert.Equal(2, corrected.Length);
        Assert.Equal("LM_Base", corrected[0].Attributes().Single(attribute => attribute.Name.LocalName == "CorrectionType").Value);
        Assert.Equal("REF_Base", corrected[1].Attributes().Single(attribute => attribute.Name.LocalName == "CorrectionType").Value);
        Assert.DoesNotContain(result.XmlContent, "CorrectionType=\"Full\"", StringComparison.Ordinal);
        Assert.DoesNotContain(result.XmlContent, "CorrectionType=\"Best\"", StringComparison.Ordinal);

        var lensmeterRight = Child(corrected[0], "R")!;
        Assert.Equal("6.25", ChildValue(lensmeterRight, "Sphere"));
        Assert.Equal("-3.25", ChildValue(lensmeterRight, "Cylinder"));
        Assert.Equal("3", ChildValue(lensmeterRight, "Axis"));
    }

    [Fact]
    public void InputWriter_ShouldUseDeviceOutputConfigurationFromInterfaceProfile()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);
        var targetFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(targetFolder);
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default() with
        {
            DeviceOutput = new DeviceOutputConfiguration(
                IsEnabled: true,
                OutputFolder: targetFolder,
                FileNameTemplate: "RTImport_{PatientNumber}_{yyyyMMdd}_{HHmmss}",
                Format: NidekRt6100InputXmlWriter.DeviceOutputFormat)
        };

        var result = _writer.WriteFile(
            new Cv5000ImportSelection(history.Patient, selected, null, null),
            interfaceProfile,
            new DateTimeOffset(2026, 5, 28, 10, 31, 35, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(Path.Combine(targetFolder, "RTImport_4701-1_20260528_103135.xml"), result.TargetPath);
        Assert.True(File.Exists(result.TargetPath));
        var bytes = File.ReadAllBytes(result.TargetPath!);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xFE, bytes[1]);
    }

    [Fact]
    public void InputWriter_ShouldRejectMissingDeviceOutputTargetFolder()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultRt6100Selection(history.Records);
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default() with
        {
            DeviceOutput = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default().DeviceOutput! with
            {
                IsEnabled = true,
                OutputFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"))
            }
        };

        var result = _writer.WriteFile(
            new Cv5000ImportSelection(history.Patient, selected, null, null),
            interfaceProfile);

        Assert.False(result.Success);
        Assert.Equal("Ausgabeordner an RT-6100 existiert nicht.", result.ErrorMessage);
        Assert.False(File.Exists(result.TargetPath));
    }

    [Fact]
    public void ParseRt6100ReturnXml_ShouldCreateMedistarPhoropterLines()
    {
        var result = _xmlParser.ParseFile(WriteTempXml(CreateRt6100ReturnXml()));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "NIDEK");
        AssertMeasurement(result, "Common/ModelName", "RT-6100");
        AssertMeasurement(result, "Common/Version", "NIDEK_RT_V1.00");
        AssertMeasurement(result, "Common/Patient/No", "4711");
        AssertMeasurement(result, "Measure[@Type='RT']/Best/HeaderLine", "Phoropter finaler Verordnungswert");
        AssertMeasurement(result, "Measure[@Type='RT']/Best/R/MedistarLine", "R.:S=- 8.00 Z=- 1.25*165 A=+ 1.25 PD= 65 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='RT']/Best/L/MedistarLine", "L.:S=- 7.50 Z=- 0.50* 25 A=+ 1.25 PD= 33");
        AssertMeasurement(result, "Measure[@Type='RT']/Full/HeaderLine", "Phoropter Maximalwert (Vollkorrektion)");
        AssertMeasurement(result, "Measure[@Type='RT']/Full/R/MedistarLine", "R.:S=- 8.25 Z=- 1.50*170 A=+ 1.50 PD= 65.5 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='RT']/Full/L/MedistarLine", "L.:S=- 7.75 Z=- 0.75* 30 A=+ 1.50 PD= 33");
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.Contains("6330", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseRt6100ReturnXml_ShouldTolerateVersionWithSpace()
    {
        var result = _xmlParser.ParseFile(WriteTempXml(CreateRt6100ReturnXml(version: "NIDEK_RT V1.00")));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Measure[@Type='RT']/Best/HeaderLine", "Phoropter finaler Verordnungswert");
    }

    [Fact]
    public void ParseRt6100ReturnXml_ShouldRejectOtherNidekModels()
    {
        var result = _xmlParser.ParseFile(WriteTempXml(CreateRt6100ReturnXml(modelName: "LM-7")));

        Assert.Empty(result.Issues);
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RT']/Best/HeaderLine");
    }

    [Fact]
    public void ParseMalformedRt6100OcrXml_ShouldReturnClearIssue()
    {
        var path = WriteTempXml(
            """
            <?xml version="1.0" encoding="UTF-16"?>
            <Ophthalmology>
              <Common><Company>NIDEK</Company><ModelName>RT-6100</ModelName><Version>NIDEK_RT_V1.00</Version></Common>
              <Measure Type="RT"><Phoropter><Corrected CorrectionType="Full"><R><Sphere>-8.25</Sphere>
            """,
            Encoding.UTF8);

        var result = _xmlParser.ParseFile(path);

        Assert.Contains(result.Issues, issue => issue.Message.Contains("Failed to parse XML", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath.StartsWith("Measure[@Type='RT']/Full", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForRt6100Return_ShouldMapBestTo6228AndFullTo6227()
    {
        var content = BuildRt6100ExportContent(WriteTempXml(CreateRt6100ReturnXml()));

        Assert.Contains("8402Phoro", content, StringComparison.Ordinal);
        Assert.Contains("6228Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=- 8.00 Z=- 1.25*165 A=+ 1.25 PD= 65 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 7.50 Z=- 0.50* 25 A=+ 1.25 PD= 33", content, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", content, StringComparison.Ordinal);
        Assert.Contains("6227R.:S=- 8.25 Z=- 1.50*170 A=+ 1.50 PD= 65.5 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6227L.:S=- 7.75 Z=- 0.75* 30 A=+ 1.50 PD= 33", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228--", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForRt6100Return_WithOnlyFull_ShouldUseOnly6227()
    {
        var content = BuildRt6100ExportContent(WriteTempXml(CreateRt6100ReturnXml(includeBest: false)));

        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", content, StringComparison.Ordinal);
        Assert.Contains("6227R.:S=- 8.25 Z=- 1.50*170 A=+ 1.50 PD= 65.5 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6227L.:S=- 7.75 Z=- 0.75* 30 A=+ 1.50 PD= 33", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228R.:S=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForRt6100Return_WithOnlyBest_ShouldUseOnly6228()
    {
        var content = BuildRt6100ExportContent(WriteTempXml(CreateRt6100ReturnXml(includeFull: false)));

        Assert.Contains("6228Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=- 8.00 Z=- 1.25*165 A=+ 1.25 PD= 65 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 7.50 Z=- 0.50* 25 A=+ 1.25 PD= 33", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter Maximalwert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227R.:S=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForRt6100Return_WithoutBestOrFull_ShouldFailInsteadOfAisOnlyExport()
    {
        var measurements = _xmlParser.ParseFile(WriteTempXml(CreateRt6100ReturnXml(includeBest: false, includeFull: false))).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default());

        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, rules);

        Assert.True(mappingResult.HasErrors);
        Assert.Contains(mappingResult.Issues, issue => issue.Message == "No exportable device measurements were found.");
        Assert.DoesNotContain(mappingResult.Records, record => record.FieldCode is "6227" or "6228" or "6330");
    }

    [Fact]
    public void BuiltInNidekRt6100Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekRt6100Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default();

        Assert.Equal("NIDEK", deviceProfile.Manufacturer);
        Assert.Equal("RT-6100", deviceProfile.Model);
        Assert.Equal("Phoropter", deviceProfile.DeviceType);
        Assert.True(deviceProfile.IsBidirectional);
        Assert.Equal(DeviceConnectionKind.NetworkLan, deviceProfile.ConnectionKind);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RT']/Best/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RT']/Full/R/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-nidek-rt6100-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='RT']/Best/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='RT']/Full/R/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode == "6330");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6221" or "6220" or "6205" or "6302" or "6303" or "6304" or "6305");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-nidek-rt6100-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-rt6100-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.NotNull(interfaceProfile.DeviceOutput);
        Assert.False(interfaceProfile.DeviceOutput!.IsEnabled);
        Assert.Equal(string.Empty, interfaceProfile.DeviceOutput.OutputFolder);
        Assert.Equal(NidekRt6100InputXmlWriter.DefaultFileNameTemplate, interfaceProfile.DeviceOutput.FileNameTemplate);
        Assert.Equal(NidekRt6100InputXmlWriter.DeviceOutputFormat, interfaceProfile.DeviceOutput.Format);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    private string BuildRt6100ExportContent(string devicePath)
    {
        var measurements = _xmlParser.ParseFile(devicePath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default());
        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, rules);
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        return exportResult.Content;
    }

    private static string CreateRt6100ReturnXml(
        bool includeBest = true,
        bool includeFull = true,
        bool includeRight = true,
        bool includeLeft = true,
        string version = "NIDEK_RT_V1.00",
        string modelName = "RT-6100")
    {
        var best = includeBest
            ? CreateCorrectedXml("Best", "Final Prescription", "-8.00", "-1.25", "165", "+1.25", "32.00", "-7.50", "-0.50", "25", "+1.25", "33.00", "65.00", includeRight, includeLeft)
            : string.Empty;
        var full = includeFull
            ? CreateCorrectedXml("Full", "Full Correction", "-8.25", "-1.50", "170", "+1.50", "32.50", "-7.75", "-0.75", "30", "+1.50", "33.00", "65.50", includeRight, includeLeft)
            : string.Empty;

        return $$"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>{{modelName}}</ModelName>
                <Version>{{version}}</Version>
                <Date>2026.05.28</Date>
                <Time>12:34:56</Time>
                <Patient>
                  <No>4711</No>
                  <ID>4711</ID>
                  <FirstName>Test</FirstName>
                  <LastName>Person</LastName>
                  <DOB>2000.01.01</DOB>
                </Patient>
              </Common>
              <Measure Type="RT">
                <Phoropter>
                  <Corrected CorrectionType="LM_Base" Vision="Distant" Situation="Standard">
                    <DisplayName>Lensmeter</DisplayName>
                  </Corrected>
                  <Corrected CorrectionType="REF_Base" Vision="Distant" Situation="Standard">
                    <DisplayName>Autorefraction</DisplayName>
                  </Corrected>
            {{best}}
            {{full}}
                </Phoropter>
              </Measure>
            </Ophthalmology>
            """;
    }

    private static string CreateCorrectedXml(
        string correctionType,
        string displayName,
        string rightSphere,
        string rightCylinder,
        string rightAxis,
        string rightAdd,
        string rightPd,
        string leftSphere,
        string leftCylinder,
        string leftAxis,
        string leftAdd,
        string leftPd,
        string binocularPd,
        bool includeRight,
        bool includeLeft)
    {
        var right = includeRight
            ? $$"""
                    <R>
                      <Sphere>{{rightSphere}}</Sphere>
                      <Cylinder>{{rightCylinder}}</Cylinder>
                      <Axis>{{rightAxis}}</Axis>
                      <ADD>{{rightAdd}}</ADD>
                      <PD>{{rightPd}}</PD>
                      <PrismX base="In">1.00</PrismX>
                      <PrismY base="Up">0.50</PrismY>
                      <VA>1.0</VA>
                    </R>
              """
            : string.Empty;
        var left = includeLeft
            ? $$"""
                    <L>
                      <Sphere>{{leftSphere}}</Sphere>
                      <Cylinder>{{leftCylinder}}</Cylinder>
                      <Axis>{{leftAxis}}</Axis>
                      <ADD>{{leftAdd}}</ADD>
                      <PD>{{leftPd}}</PD>
                    </L>
              """
            : string.Empty;

        return $$"""
                  <Corrected CorrectionType="{{correctionType}}" Vision="Distant" Situation="Standard">
                    <DisplayName>{{displayName}}</DisplayName>
                    <VD>13.75</VD>
            {{right}}
            {{left}}
                    <B>
                      <PD>{{binocularPd}}</PD>
                    </B>
                  </Corrected>
            """;
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-RT6100",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01012000",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "Phoro");
    }

    private static XElement? Child(XElement element, string localName)
    {
        return element.Elements().FirstOrDefault(child => child.Name.LocalName == localName);
    }

    private static string? ChildValue(XElement element, string localName)
    {
        return Child(element, localName)?.Value;
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string value)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == value);
    }

    private static string GetCv5000FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Topcon", "CV5000", fileName);
    }

    private static string WriteTempXml(string content, Encoding? encoding = null)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "nidek-rt6100.xml");
        File.WriteAllText(path, content, encoding ?? Encoding.Unicode);
        return path;
    }
}
