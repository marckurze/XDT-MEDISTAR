using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenPreviewServiceTests
{
    [Fact]
    public void BuildPreview_ShouldSeparateAisViewAndDiagnostics()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path);
        var devicePath = CopyLm7Fixture(temp.Path);
        var state = CreateLm7State(aisPath, devicePath);
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("R.:S=+ 6.25 Z=- 3.25*  3", result.Output.AisView);
        Assert.DoesNotContain("Patient:", result.Output.AisView);
        Assert.DoesNotContain("Untersuchungsart:", result.Output.AisView);
        Assert.DoesNotContain("Karteikartenansicht", result.Output.AisView);
        Assert.DoesNotContain("8402:", result.Output.AisView);
        Assert.DoesNotContain("6228:", result.Output.AisView);
        Assert.DoesNotContain("Measure[@Type='LM']", result.Output.AisView);
        Assert.Contains("Messwerte erkannt:", result.Output.Diagnostics);
        Assert.Contains("Measure[@Type='LM']", result.Output.Diagnostics);
        Assert.Contains("6228", result.Output.RawXdt);
    }

    [Fact]
    public void BuildPreview_ShouldRenderLiteralRuleWithoutSourcePath()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path);
        var devicePath = CopyLm7Fixture(temp.Path);
        var state = CreateLm7State(aisPath, devicePath);
        state.AddWorkingRule(new ExportRuleDefinition(
            "literal-note",
            "6228",
            "Feste Überschrift",
            ExportRuleType.Template,
            null,
            "Phoropter finaler Verordnungswert",
            999,
            true,
            null));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.Output.RawXdt);
        Assert.Contains("Phoropter finaler Verordnungswert", result.Output.AisView);
    }

    [Fact]
    public void BuildPreview_ShouldKeepAr360BaukastenReferenceWorking()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path, "AR360");
        var devicePath = CopyFixture(temp.Path, "Devices", "Nidek", "AR360", "AR360.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228", result.Output.RawXdt);
        Assert.Contains("R.:", result.Output.AisView);
        Assert.DoesNotContain("passt nicht", string.Join(Environment.NewLine, result.Messages), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPreview_ShouldWarnInsteadOfBlockingOnWorkbenchModelMismatch()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path);
        var devicePath = CopyLm7Fixture(temp.Path);
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCl300Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("Baukasten-Vorschau wird trotzdem erzeugt", string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("R.:S=+ 6.25 Z=- 3.25*  3", result.Output.AisView);
        Assert.Contains("Status: ModelMismatchWarning", result.Output.Diagnostics);
        Assert.Contains("Profil: TOPCON CL300", result.Output.Diagnostics);
        Assert.Contains("Datei-ModelName: LM-7", result.Output.Diagnostics);
        Assert.Contains("Baukastenmodus: Modellabweichungen blockieren nicht", result.Output.Diagnostics);
    }

    [Fact]
    public void BuildPreview_ShouldCreateCv5000PhoropterPreview()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.Output.RawXdt);
        Assert.Contains("6227Phoropter Maximalwert", result.Output.RawXdt);
        Assert.DoesNotContain("6330", result.Output.RawXdt);
        Assert.Contains("Phoropter finaler Verordnungswert", result.Output.AisView);
        Assert.DoesNotContain("6228:", result.Output.AisView);
        Assert.Contains("<Ophthalmology", result.Output.DeviceOutput);
    }

    [Fact]
    public void BuildPreview_ShouldApplyCv5000DeviceOutputWorkingRules()
    {
        using var temp = new TempFolder();
        var aisPath = WriteHistoricalGdt(temp.Path);
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var idRule = state.WorkingDeviceOutputRules.Single(rule => rule.TargetFieldCode == "Common/Patient/ID") with
        {
            OutputTemplate = "BAUKASTEN-ID"
        };
        Assert.True(state.UpdateWorkingRule(idRule));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("BAUKASTEN-ID", result.Output.DeviceOutput);
        Assert.DoesNotContain("<nsCommon:ID>4701-1</nsCommon:ID>", result.Output.DeviceOutput);
    }

    [Fact]
    public void BuildPreview_ShouldApplyCv5000DeviceOutputPlaceholderRuleChanges()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var idRule = state.WorkingDeviceOutputRules.Single(rule => rule.TargetFieldCode == "Common/Patient/ID") with
        {
            SourcePath = null,
            OutputTemplate = "{CV5000Input.PatientNumber}-BAUKASTEN"
        };
        Assert.True(state.UpdateWorkingRule(idRule));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("<nsCommon:ID>4701-1-BAUKASTEN</nsCommon:ID>", result.Output.DeviceOutput);
    }

    [Fact]
    public void BuildPreview_ShouldCreateLineNumberedDocumentsWithoutChangingPlainText()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.NotNull(result.Output.Documents);
        var rawDocument = result.Output.Documents![XdtBaukastenResultView.RawXdt];
        var aisDocument = result.Output.Documents![XdtBaukastenResultView.AisView];
        var deviceDocument = result.Output.Documents![XdtBaukastenResultView.DeviceOutput];
        Assert.Equal(result.Output.RawXdt, rawDocument.PlainText);
        Assert.Equal(result.Output.AisView, aisDocument.PlainText);
        Assert.Equal(result.Output.DeviceOutput, deviceDocument.PlainText);
        Assert.Equal(1, rawDocument.Lines.First().LineNumber);
        Assert.Equal(1, aisDocument.Lines.First().LineNumber);
        Assert.Equal(1, deviceDocument.Lines.First().LineNumber);
        Assert.False(rawDocument.PlainText.StartsWith("1 ", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildPreview_ShouldLinkRulesToPreviewLines()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        var rawDocument = result.Output.Documents![XdtBaukastenResultView.RawXdt];
        var aisDocument = result.Output.Documents![XdtBaukastenResultView.AisView];
        var deviceDocument = result.Output.Documents![XdtBaukastenResultView.DeviceOutput];
        Assert.Contains(rawDocument.Lines, line => line.Text.Contains("6228Phoropter finaler Verordnungswert", StringComparison.Ordinal) && line.RuleName == "PrescriptionHeader");
        Assert.Contains(aisDocument.Lines, line => line.Text.Contains("Phoropter finaler Verordnungswert", StringComparison.Ordinal) && line.RuleName == "PrescriptionHeader");
        Assert.Contains(deviceDocument.Lines, line => line.Text.Contains("<nsCommon:ID>", StringComparison.Ordinal) && line.RuleId == "cv5000-patient-id");
    }

    [Fact]
    public void BuildPreview_ShouldApplyRt6100DeviceOutputWorkingRules()
    {
        using var temp = new TempFolder();
        var aisPath = CopyFixture(temp.Path, "Devices", "Topcon", "CV5000", "Patient_mit_Phoropter_Daten.XDT");
        var devicePath = WriteTempXml(temp.Path, BuilderManualProcessingPreviewServiceTests_CreateRt6100ReturnXml());
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekRt6100Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt6100Default());
        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var idRule = state.WorkingDeviceOutputRules.Single(rule => rule.TargetFieldCode == "Common/Patient/ID") with
        {
            OutputTemplate = "RT-BAUKASTEN-ID"
        };
        Assert.True(state.UpdateWorkingRule(idRule));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt6100Default());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("<ID>RT-BAUKASTEN-ID</ID>", result.Output.DeviceOutput);
    }

    [Fact]
    public void BuildPreview_ShouldCreateNidekRtSerialDeviceOutputPreviewWithoutWritingProductiveFile()
    {
        using var temp = new TempFolder();
        var aisPath = WriteHistoricalGdt(temp.Path);
        var devicePath = WriteText(temp.Path, "rt3100.txt", CreateRtSerialFrame());
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());
        state.SetRuleDirection(XdtBaukastenRuleDirection.DeviceOutput);
        var patientRule = state.WorkingDeviceOutputRules.Single(rule => rule.TargetFieldCode == "Serial/ID") with
        {
            OutputTemplate = "RTSERIAL-TEST-ID"
        };
        Assert.True(state.UpdateWorkingRule(patientRule));
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("<SH>", result.Output.DeviceOutput);
        Assert.Contains("DRL<SX>", result.Output.DeviceOutput);
        Assert.Contains("Hexdump:", result.Output.DeviceOutput);
        Assert.Contains("RTSERIAL-TEST-ID", result.Output.DeviceOutput);
        Assert.Contains("NIDEK RT-2100/3100/5100 RS232 ist vorbereitet", result.Output.DeviceOutput);
        Assert.False(File.Exists(Path.Combine(temp.Path, NidekRtSerialPhoropterOutputWriter.DefaultFileNameTemplate)));
    }

    [Fact]
    public void BuildPreview_ShouldUseRt3100PracticeCaptureForFinalPrescription()
    {
        using var temp = new TempFolder();
        var aisPath = WriteGdt(temp.Path, "Phoro");
        var devicePath = WriteBytes(
            temp.Path,
            "rt3100-practice.bin",
            LoadHexFixture("rt3100-final-prescription-practice-capture-202606xx.hex"));
        var state = CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());
        var service = new XdtBaukastenPreviewService();

        var result = service.BuildPreview(state, DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Messages));
        Assert.Contains("6228Phoropter finaler Verordnungswert", result.Output.RawXdt);
        Assert.Contains("6228R.:S=- 1.50 Z=- 0.75*180 A=+ 0.75 PD= 64", result.Output.RawXdt);
        Assert.Contains("6228L.:S=- 1.50 Z=- 1.50*175 A=+ 1.25 PD= 64", result.Output.RawXdt);
        Assert.DoesNotContain("6227", result.Output.RawXdt);
        Assert.DoesNotContain("6330", result.Output.RawXdt);
        Assert.Contains("Phoropter finaler Verordnungswert", result.Output.AisView);
        Assert.Contains("R.:S=- 1.50 Z=- 0.75*180 A=+ 0.75 PD= 64", result.Output.AisView);
        Assert.Contains("L.:S=- 1.50 Z=- 1.50*175 A=+ 1.25 PD= 64", result.Output.AisView);
        Assert.Contains("RT-3100", result.Output.Diagnostics);
        Assert.Contains("Measure[@Type='RTSERIAL']/Source: RT", result.Output.Diagnostics);
        Assert.Contains("Measure[@Type='RTSERIAL']/Final/R/VA: 0.1", result.Output.Diagnostics);
        Assert.Contains("Measure[@Type='RTSERIAL']/Final/R/WorkingDistance: 40", result.Output.Diagnostics);
        Assert.False(File.Exists(Path.Combine(temp.Path, NidekRtSerialPhoropterOutputWriter.DefaultFileNameTemplate)));
    }

    private static XdtBaukastenState CreateLm7State(string aisPath, string devicePath)
    {
        return CreateState(
            aisPath,
            devicePath,
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default());
    }

    private static XdtBaukastenState CreateState(
        string aisPath,
        string devicePath,
        DeviceProfileDefinition deviceProfile,
        ExportProfileDefinition exportProfile)
    {
        var state = new XdtBaukastenState();
        state.SetAisProfile(DefaultAisProfiles.CreateMedistarDefault());
        state.SetDeviceProfile(deviceProfile);
        state.SetExportProfile(exportProfile);
        state.SetAisInput(new XdtBaukastenLoadedInput(aisPath, Path.GetFileName(aisPath), File.ReadAllText(aisPath, Encoding.UTF8)));
        state.SetDeviceInput(new XdtBaukastenLoadedInput(devicePath, Path.GetFileName(devicePath), File.ReadAllText(devicePath, Encoding.UTF8)));
        return state;
    }

    private static string WriteGdt(string folder, string examType = "LM7")
    {
        var path = Path.Combine(folder, "patient.gdt");
        File.WriteAllText(path, string.Concat(
            BuildGdtLine("3000", "4701-1"),
            BuildGdtLine("3101", "Testfrau"),
            BuildGdtLine("3102", "Anna"),
            BuildGdtLine("3103", "12061955"),
            BuildGdtLine("8402", examType)), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string WriteHistoricalGdt(string folder)
    {
        var path = Path.Combine(folder, "patient-history.gdt");
        File.WriteAllText(path, string.Concat(
            BuildGdtLine("3000", "4701-1"),
            BuildGdtLine("3101", "Testfrau"),
            BuildGdtLine("3102", "Anna"),
            BuildGdtLine("3103", "12061955"),
            BuildGdtLine("8402", "Phoro"),
            BuildGdtLine("9999", "18.05.2026 V0 L.:S=+ 6.50 Z=- 2.75*170 A=+ 1.50"),
            BuildGdtLine("9999", "18.05.2026 V0 R.:S=+ 6.25 Z=- 3.25*  3"),
            BuildGdtLine("9999", "18.05.2026 V1 L.:S=+ 1.50 Z=- 0.75* 90"),
            BuildGdtLine("9999", "18.05.2026 V1 R.:S=+ 1.00 Z=- 1.25*  7")), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string BuildGdtLine(string fieldCode, string value)
    {
        var length = 3 + fieldCode.Length + value.Length;
        return $"{length:D3}{fieldCode}{value}\n";
    }

    private static string CopyLm7Fixture(string folder)
    {
        return CopyFixture(folder, "Devices", "Nidek", "LM7", "NIDEK LM7.xml");
    }

    private static string CopyFixture(string folder, params string[] pathParts)
    {
        var source = Path.Combine(new[] { AppContext.BaseDirectory, "TestData" }.Concat(pathParts).ToArray());
        var target = Path.Combine(folder, Path.GetFileName(source));
        File.Copy(source, target);
        return target;
    }

    private static string WriteTempXml(string folder, string xml)
    {
        var path = Path.Combine(folder, "rt6100.xml");
        File.WriteAllText(path, xml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string WriteText(string folder, string fileName, string text)
    {
        var path = Path.Combine(folder, fileName);
        File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static string WriteBytes(string folder, string fileName, byte[] bytes)
    {
        var path = Path.Combine(folder, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static byte[] LoadHexFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "RS232", fileName);
        return File.ReadAllText(path, Encoding.UTF8)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => Convert.ToByte(token, 16))
            .ToArray();
    }

    private static string CreateRtSerialFrame()
    {
        var builder = new StringBuilder();
        builder.Append((char)NidekRtSerialControlChars.SH);
        foreach (var line in new[]
                 {
                     "NIDEK_RT-3100",
                     "ID0000004711",
                     "DAY2026/05/29_02",
                     "@",
                     "RT",
                     "RF+02.25-00.50008",
                     "LF+02.00-00.25085",
                     "PD0060"
                 })
        {
            if (!line.StartsWith("NIDEK", StringComparison.Ordinal))
            {
                builder.Append((char)NidekRtSerialControlChars.SX);
            }

            builder.Append(line);
            builder.Append((char)NidekRtSerialControlChars.CR);
        }

        builder.Append((char)NidekRtSerialControlChars.EB);
        builder.Append((char)NidekRtSerialControlChars.ET);
        return builder.ToString();
    }

    private static string BuilderManualProcessingPreviewServiceTests_CreateRt6100ReturnXml()
    {
        return """
<?xml version="1.0" encoding="UTF-8"?>
<Ophthalmology>
  <Common>
    <Company>NIDEK</Company>
    <ModelName>RT-6100</ModelName>
    <Version>NIDEK_RT_V1.00</Version>
    <Date>2026.05.28</Date>
    <Time>12:00:00</Time>
    <Patient>
      <No>4701-1</No>
      <ID>4701-1</ID>
      <FirstName>Anna</FirstName>
      <LastName>Testfrau</LastName>
      <DOB>1955.06.12</DOB>
    </Patient>
  </Common>
  <Measure Type="RT">
    <Phoropter>
      <Corrected CorrectionType="Full" Vision="Distant" Situation="Standard">
        <DisplayName>Full Correction</DisplayName>
        <R>
          <Sphere>-8.25</Sphere>
          <Cylinder>-1.50</Cylinder>
          <Axis>170</Axis>
          <ADD>1.50</ADD>
          <PD>32.5</PD>
        </R>
        <L>
          <Sphere>-7.75</Sphere>
          <Cylinder>-0.75</Cylinder>
          <Axis>30</Axis>
          <ADD>1.50</ADD>
          <PD>33.0</PD>
        </L>
        <B>
          <PD>65.5</PD>
        </B>
      </Corrected>
      <Corrected CorrectionType="Best" Vision="Distant" Situation="Standard">
        <DisplayName>Best</DisplayName>
        <R>
          <Sphere>-8.00</Sphere>
          <Cylinder>-1.25</Cylinder>
          <Axis>168</Axis>
        </R>
        <L>
          <Sphere>-7.50</Sphere>
          <Cylinder>-0.50</Cylinder>
          <Axis>28</Axis>
        </L>
      </Corrected>
    </Phoropter>
  </Measure>
</Ophthalmology>
""";
    }

    private sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
