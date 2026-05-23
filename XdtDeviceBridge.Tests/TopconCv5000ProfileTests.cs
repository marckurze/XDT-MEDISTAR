using System.Xml.Linq;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class TopconCv5000ProfileTests
{
    private readonly XmlDeviceParser _xmlParser = new();
    private readonly MedistarHistoricalMeasurementParser _historyParser = new();
    private readonly TopconCv5000ImportXmlWriter _writer = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void HistoricalParser_ShouldReadPatientAndMedistarCardMeasurements()
    {
        var result = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));

        Assert.Equal("4701-1", result.Patient.PatientNumber);
        Assert.Equal("Testfrau", result.Patient.LastName);
        Assert.Equal("Anna", result.Patient.FirstName);
        Assert.Equal("12061955", result.Patient.BirthDate);
        Assert.Equal("Phoro", result.Patient.ExaminationType);
        Assert.Contains(result.Records, record => record.SourcePrefix == "V0" && record.SourceKind == AisHistoricalMeasurementSourceKind.Lensmeter);
        Assert.Contains(result.Records, record => record.SourcePrefix == "V1" && record.SourceKind == AisHistoricalMeasurementSourceKind.Autorefraction);
        Assert.Contains(result.Records, record => record.SourcePrefix == "V2" && record.SourceKind == AisHistoricalMeasurementSourceKind.Phoropter);
        Assert.Contains(result.Records, record => record.SourcePrefix == "V3" && record.SourceKind == AisHistoricalMeasurementSourceKind.Prescription);
        Assert.Contains(result.Records, record => record.SourcePrefix == "V7" && record.SourceKind == AisHistoricalMeasurementSourceKind.Keratometry);
        Assert.Contains(result.Records, record => record.SourcePrefix == "P" && record.SourceKind == AisHistoricalMeasurementSourceKind.Pachymetry);
        Assert.Contains(result.Records, record => record.SourcePrefix == "Y" && record.SourceKind == AisHistoricalMeasurementSourceKind.Tonometry);

        var latestLensmeter = Assert.Single(result.Records, record =>
            record.SourcePrefix == "V0"
            && record.Date == new DateOnly(2026, 5, 18));
        Assert.Equal("+6.25", latestLensmeter.RightEye?.Sphere);
        Assert.Equal("-3.25", latestLensmeter.RightEye?.Cylinder);
        Assert.Equal("3", latestLensmeter.RightEye?.Axis);
        Assert.Equal("+6.50", latestLensmeter.LeftEye?.Sphere);
        Assert.Equal("-2.75", latestLensmeter.LeftEye?.Cylinder);
        Assert.Equal("170", latestLensmeter.LeftEye?.Axis);
        Assert.True(latestLensmeter.IsExportableToCv5000);

        var defaultSelection = _historyParser.CreateDefaultCv5000Selection(result.Records);
        Assert.Equal(
            new[]
            {
                AisHistoricalMeasurementSourceKind.Lensmeter,
                AisHistoricalMeasurementSourceKind.Autorefraction,
                AisHistoricalMeasurementSourceKind.Phoropter
            },
            defaultSelection.Select(record => record.SourceKind));
        Assert.All(defaultSelection, record => Assert.True(record.IsExportableToCv5000));
    }

    [Fact]
    public void ImportWriter_ShouldCreateCv5000SbjXmlFromSelectedMedistarHistory()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);

        var result = _writer.BuildXml(
            new Cv5000ImportSelection(history.Patient, selected, "C:\\Temp", "CVImport.xml"),
            new DateTimeOffset(2026, 5, 23, 10, 31, 35, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(Path.Combine("C:\\Temp", "CVImport.xml"), result.TargetPath);
        Assert.NotNull(result.XmlContent);

        var document = XDocument.Parse(result.XmlContent!);
        Assert.NotNull(document.Root);
        var root = document.Root!;
        Assert.Equal("Ophthalmology", root.Name.LocalName);
        var common = Assert.Single(root.Elements(), element => element.Name.LocalName == "Common");
        Assert.Equal("Topcon Europe Medical B.V.", ChildValue(common, "Company"));
        Assert.Equal("IMAGEnet i-base", ChildValue(common, "ModelName"));
        Assert.Equal("4701-1", ChildValue(Child(common, "Patient")!, "No."));
        Assert.Equal("Testfrau", ChildValue(Child(common, "Patient")!, "LastName"));

        var measure = Assert.Single(root.Elements(), element => element.Name.LocalName == "Measure");
        Assert.Equal("SBJ", measure.Attributes().Single(attribute => attribute.Name.LocalName == "type").Value);
        var types = Child(measure, "RefractionTest")!.Elements().Where(element => element.Name.LocalName == "Type").ToArray();
        Assert.Equal(3, types.Length);
        Assert.Equal("Lensmeter", ChildValue(types[0], "TypeName"));
        Assert.Equal("Autorefraction", ChildValue(types[1], "TypeName"));
        Assert.Equal("Previous Phoropter", ChildValue(types[2], "TypeName"));
        Assert.All(types, type => Assert.Single(type.Elements(), element => element.Name.LocalName == "ExamDistance"));
        Assert.DoesNotContain("ExamDistance No=\"2\"", result.XmlContent, StringComparison.Ordinal);

        var firstRefraction = Child(Child(types[0], "ExamDistance")!, "RefractionData")!;
        Assert.Equal("6.25", ChildValue(Child(firstRefraction, "R")!, "Sph"));
        Assert.Equal("-3.25", ChildValue(Child(firstRefraction, "R")!, "Cyl"));
        Assert.Equal("3", ChildValue(Child(firstRefraction, "R")!, "Axis"));
        Assert.Equal("6.50", ChildValue(Child(firstRefraction, "L")!, "Sph"));
        Assert.Equal("-2.75", ChildValue(Child(firstRefraction, "L")!, "Cyl"));
        Assert.Equal("170", ChildValue(Child(firstRefraction, "L")!, "Axis"));

        var phoropterPd = Child(Child(types[2], "ExamDistance")!, "PD")!;
        Assert.Equal("62.00", ChildValue(phoropterPd, "B"));
    }

    [Fact]
    public void HistoricalParser_DefaultSelectionShouldUseNewestExportableV0V1V2Only()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));

        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);

        Assert.Equal(3, selected.Count);
        Assert.Equal(
            new[]
            {
                AisHistoricalMeasurementSourceKind.Lensmeter,
                AisHistoricalMeasurementSourceKind.Autorefraction,
                AisHistoricalMeasurementSourceKind.Phoropter
            },
            selected.Select(record => record.SourceKind));
        Assert.DoesNotContain(selected, record => record.SourceKind is
            AisHistoricalMeasurementSourceKind.Prescription
            or AisHistoricalMeasurementSourceKind.AutorefractionSubjective
            or AisHistoricalMeasurementSourceKind.Keratometry
            or AisHistoricalMeasurementSourceKind.Pachymetry
            or AisHistoricalMeasurementSourceKind.Tonometry);
        Assert.All(selected, record => Assert.True(record.IsExportableToCv5000));
    }

    [Fact]
    public void ImportWriter_ShouldRejectEmptyCv5000Selection()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));

        var result = _writer.BuildXml(new Cv5000ImportSelection(
            history.Patient,
            Array.Empty<AisHistoricalMeasurementRecord>(),
            "C:\\Temp",
            "CVImport.xml"));

        Assert.False(result.Success);
        Assert.Equal("Keine exportierbaren refraktiven Messdatensätze für den CV-5000-Import ausgewählt.", result.ErrorMessage);
        Assert.Null(result.XmlContent);
    }

    [Fact]
    public void ParseCv5000ReturnXml_ShouldCreateMedistarPhoropterLines()
    {
        var result = _xmlParser.ParseFile(GetCv5000FixturePath("M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml"));

        Assert.Empty(result.Issues);
        AssertMeasurement(result, "Common/Company", "TOPCON");
        AssertMeasurement(result, "Common/ModelName", "CV-5000");
        AssertMeasurement(result, "Common/MachineNo", "10111");
        AssertMeasurement(result, "Common/Version", "1.2");
        AssertMeasurement(result, "Common/Date", "2013-06-25");
        AssertMeasurement(result, "Common/Time", "17:05:09.656");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/TypeName", "Prescription");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='2']/TypeName", "Full Correction");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Sph", "1.25");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Cyl", "-2.00");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/R/Axis", "7");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/RefractionData/VD", "13.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/R", "29.50");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/L", "29.50");
        AssertMeasurement(result, "Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/PD/B", "59.00");
        AssertMeasurement(result, "Measure[@Type='SBJ']/Prescription/HeaderLine", "Phoropter finaler Verordnungswert");
        AssertMeasurement(result, "Measure[@Type='SBJ']/Prescription/R/MedistarLine", "R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/Prescription/L/MedistarLine", "L.:S=+ 1.25 Z=- 2.00*  7");
        AssertMeasurement(result, "Measure[@Type='SBJ']/FullCorrection/HeaderLine", "Phoropter Maximalwert (Vollkorrektion)");
        AssertMeasurement(result, "Measure[@Type='SBJ']/FullCorrection/R/MedistarLine", "R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75");
        AssertMeasurement(result, "Measure[@Type='SBJ']/FullCorrection/L/MedistarLine", "L.:S=+ 1.25 Z=- 2.00*  7");
        Assert.DoesNotContain(result.Measurements, measurement =>
            measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExportForCv5000Return_ShouldSeparatePrescriptionAndFullCorrection()
    {
        var measurements = _xmlParser.ParseFile(GetCv5000FixturePath("M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml")).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, rules);
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("8402Phoro", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter finaler Verordnungswert", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 1.25 Z=- 2.00*  7", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6330R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6330L.:S=+ 1.25 Z=- 2.00*  7", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228--", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6205", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6220", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6221", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6302", exportResult.Content, StringComparison.Ordinal);
        Assert.Equal(2, mappingResult.Records.Count(record => record.FieldCode == "6227"));
        Assert.Equal(2, mappingResult.Records.Count(record => record.FieldCode == "6228"));
        Assert.Equal(2, mappingResult.Records.Count(record => record.FieldCode == "6330"));
    }

    [Fact]
    public void MedistarExportForCv5000Return_WithOnlyPrescription_ShouldOmitFullCorrectionBlock()
    {
        var content = BuildCv5000ExportContent(WriteTempXml(CreateCv5000ReturnXml("Prescription")));

        Assert.Contains("6227Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 1.25 Z=- 2.00*  7", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter Maximalwert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForCv5000Return_WithOnlyFullCorrection_ShouldUse6330AndOmitPrescriptionBlock()
    {
        var content = BuildCv5000ExportContent(WriteTempXml(CreateCv5000ReturnXml("Full Correction")));

        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", content, StringComparison.Ordinal);
        Assert.Contains("6330R.:S=+ 1.25 Z=- 2.00*  7 PD= 59 VD= 13.75", content, StringComparison.Ordinal);
        Assert.Contains("6330L.:S=+ 1.25 Z=- 2.00*  7", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228R.:S=", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForCv5000Return_WithOneEye_ShouldOutputOnlyPresentEye()
    {
        var content = BuildCv5000ExportContent(WriteTempXml(CreateCv5000ReturnXml("Prescription", includeRight: false)));

        Assert.Contains("6227Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 1.25 Z=- 2.00*  7", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228R.:S=", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExportForCv5000Return_WithoutExportableValues_ShouldFailInsteadOfAisOnlyExport()
    {
        var path = WriteTempXml(CreateCv5000ReturnXml("Prescription", includeRight: false, includeLeft: false));
        var measurements = _xmlParser.ParseFile(path).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());

        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, rules);

        Assert.True(mappingResult.HasErrors);
        Assert.Contains(mappingResult.Issues, issue => issue.Message == "No exportable device measurements were found.");
        Assert.DoesNotContain(mappingResult.Records, record => record.FieldCode is "6227" or "6228" or "6330");
    }

    [Fact]
    public void BuiltInTopconCv5000Profiles_ShouldBePresentAndValid()
    {
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default();
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default();

        Assert.Equal("TOPCON", deviceProfile.Manufacturer);
        Assert.Contains("CV-5000", deviceProfile.Model);
        Assert.Equal("Phoropter", deviceProfile.DeviceType);
        Assert.True(deviceProfile.IsBidirectional);
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/Prescription/HeaderLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/Prescription/R/MedistarLine");
        Assert.Contains(deviceProfile.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(deviceProfile));

        Assert.Equal("device-topcon-cv5000-default", exportProfile.SourceDeviceProfileId);
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='SBJ']/Prescription/HeaderLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='SBJ']/Prescription/R/MedistarLine");
        Assert.Contains(exportProfile.Rules, rule => rule.TargetFieldCode == "6330" && rule.SourcePath == "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine");
        Assert.DoesNotContain(exportProfile.Rules, rule => rule.TargetFieldCode is "6221" or "6220" or "6205" or "6302" or "6303" or "6304" or "6305");
        Assert.Empty(ExportProfileDefinitionValidator.Validate(exportProfile));

        Assert.Equal("device-topcon-cv5000-default", interfaceProfile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-cv5000-default", interfaceProfile.ExportProfileId);
        Assert.False(interfaceProfile.IsActive);
        Assert.True(interfaceProfile.Metadata.IsBuiltIn);
        Assert.NotNull(interfaceProfile.DeviceOutput);
        Assert.False(interfaceProfile.DeviceOutput!.IsEnabled);
        Assert.Equal(string.Empty, interfaceProfile.DeviceOutput.OutputFolder);
        Assert.Equal("CVImport.xml", interfaceProfile.DeviceOutput.FileNameTemplate);
        Assert.Equal("TOPCON CV-5000 XML", interfaceProfile.DeviceOutput.Format);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
    }

    [Fact]
    public void ImportWriter_ShouldUseDeviceOutputConfigurationFromInterfaceProfile()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);
        var targetFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            DeviceOutput = new DeviceOutputConfiguration(
                IsEnabled: true,
                OutputFolder: targetFolder,
                FileNameTemplate: "CVImport.xml",
                Format: "TOPCON CV-5000 XML")
        };

        var result = _writer.WriteFile(
            new Cv5000ImportSelection(history.Patient, selected, null, null),
            interfaceProfile,
            new DateTimeOffset(2026, 5, 23, 10, 31, 35, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(Path.Combine(targetFolder, "CVImport.xml"), result.TargetPath);
        Assert.True(File.Exists(result.TargetPath));
    }

    [Fact]
    public void ImportWriter_ShouldRejectMissingDeviceOutputFolderFromInterfaceProfile()
    {
        var history = _historyParser.ParseFile(GetCv5000FixturePath("Patient_mit_Phoropter_Daten.XDT"));
        var selected = _historyParser.CreateDefaultCv5000Selection(history.Records);
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            DeviceOutput = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default().DeviceOutput! with
            {
                IsEnabled = true,
                OutputFolder = string.Empty
            }
        };

        var result = _writer.WriteFile(
            new Cv5000ImportSelection(history.Patient, selected, @"C:\Wrong", "Wrong.xml"),
            interfaceProfile);

        Assert.False(result.Success);
        Assert.Equal("Ausgabeordner an Gerät fehlt.", result.ErrorMessage);
        Assert.Null(result.TargetPath);
    }

    [Fact]
    public void NonCv5000TopconSbjXml_ShouldNotCreateCv5000MedistarLines()
    {
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>KR-800S</ModelName>
              </Common>
              <Measure type="SBJ">
                <RefractionTest>
                  <Type No="1">
                    <TypeName>Full Correction</TypeName>
                  </Type>
                </RefractionTest>
              </Measure>
            </Ophthalmology>
            """);

        var result = _xmlParser.ParseFile(path);

        Assert.Empty(result.Issues);
        Assert.DoesNotContain(result.Measurements, measurement => measurement.SourcePath == "Measure[@Type='SBJ']/Prescription/HeaderLine");
    }

    private string BuildCv5000ExportContent(string devicePath)
    {
        var measurements = _xmlParser.ParseFile(devicePath).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var mappingResult = _mappingEngine.Map(CreatePatientData(), measurements, rules);
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        return exportResult.Content;
    }

    private static string CreateCv5000ReturnXml(
        string typeName,
        bool includeRight = true,
        bool includeLeft = true)
    {
        var right = includeRight
            ? """
                        <R>
                          <Sph>1.25</Sph>
                          <Cyl>-2.00</Cyl>
                          <Axis>7</Axis>
                        </R>
              """
            : string.Empty;
        var left = includeLeft
            ? """
                        <L>
                          <Sph>1.25</Sph>
                          <Cyl>-2.00</Cyl>
                          <Axis>7</Axis>
                        </L>
              """
            : string.Empty;

        return $$"""
            <Ophthalmology>
              <Common>
                <Company>TOPCON</Company>
                <ModelName>CV-5000</ModelName>
                <Date>2013-06-25</Date>
                <Time>17:05:09.656</Time>
              </Common>
              <Measure type="SBJ">
                <RefractionTest>
                  <Type No="1">
                    <TypeName>{{typeName}}</TypeName>
                    <ExamDistance No="1">
                      <Distance unit="cm">500.000</Distance>
                      <RefractionData>
            {{right}}
            {{left}}
                        <VD>13.75</VD>
                      </RefractionData>
                      <PD>
                        <R>29.50</R>
                        <L>29.50</L>
                        <B>59.00</B>
                      </PD>
                    </ExamDistance>
                  </Type>
                </RefractionTest>
              </Measure>
            </Ophthalmology>
            """;
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "PAT-CV5000",
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

    private static string WriteTempXml(string content)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "topcon-cv5000.xml");
        File.WriteAllText(path, content);
        return path;
    }
}
