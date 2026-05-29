using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekRtSerialPhoropterTests
{
    private readonly NidekRtSerialPhoropterParser _parser = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();
    private readonly NidekRtSerialPhoropterOutputWriter _writer = new();

    [Fact]
    public void Parser_ShouldParseRt3100ExpandedHeaderSubjectiveAndFinal()
    {
        var result = _parser.ParseDeviceText(CreateRt3100Frame(), "rt3100.txt");

        Assert.DoesNotContain(result.Issues, issue => issue.Severity == DeviceParseIssueSeverity.Error);
        AssertMeasurement(result, "Common/ModelName", "RT-3100");
        AssertMeasurement(result, "Common/Patient/ID", "0000004711");
        AssertMeasurement(result, "Common/Date", "2026-05-29");
        AssertMeasurement(result, "Common/SystemNo", "02");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Subjective/HeaderLine", "Phoropter Maximalwert (Vollkorrektion)");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Subjective/R/MedistarLine", "R.:S=+ 1.25 Z=- 2.00*  7 A=+ 1.50 PD= 61");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Subjective/L/MedistarLine", "L.:S=+ 1.50 Z=- 1.25* 90 A=+ 1.25 PD= 61");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/HeaderLine", "Phoropter finaler Verordnungswert");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/MedistarLine", "R.:S=+ 2.25 Z=- 0.50*  8 PD= 60");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/L/MedistarLine", "L.:S=+ 2.00 Z=- 0.25* 85 PD= 60");
    }

    [Fact]
    public void Parser_ShouldParseRt3100PracticeCaptureLineBasedStxFrame()
    {
        var bytes = LoadHexFixture("rt3100-final-prescription-practice-capture-202606xx.hex");

        var parsed = _parser.Parse(bytes);
        var result = _parser.ParseDeviceText(Encoding.ASCII.GetString(bytes), "rt3100-practice.bin");

        Assert.Equal(NidekRtSerialPhoropterModel.Rt3100, parsed.Model);
        Assert.Equal(new DateOnly(2002, 6, 16), parsed.MeasurementDate);
        Assert.Null(parsed.PatientId);
        Assert.Contains(NidekRtSerialDataSource.Refractor, parsed.Sources);
        var right = Assert.Single(parsed.Refractions, refraction => refraction.Kind == NidekRtSerialRefractionKind.Final && refraction.Eye == "R");
        var left = Assert.Single(parsed.Refractions, refraction => refraction.Kind == NidekRtSerialRefractionKind.Final && refraction.Eye == "L");
        Assert.Equal(-1.50m, right.Sphere);
        Assert.Equal(-0.75m, right.Cylinder);
        Assert.Equal(180, right.Axis);
        Assert.Equal(0.75m, right.Add);
        Assert.Equal(0.1m, right.VisualAcuity);
        Assert.Equal(64.0m, right.Pd);
        Assert.Equal(40m, right.WorkingDistance);
        Assert.Equal(-1.50m, left.Sphere);
        Assert.Equal(-1.50m, left.Cylinder);
        Assert.Equal(175, left.Axis);
        Assert.Equal(1.25m, left.Add);
        Assert.Equal(1.25m, left.VisualAcuity);
        Assert.Equal(64.0m, left.Pd);
        Assert.Equal(40m, left.WorkingDistance);
        Assert.DoesNotContain(result.Issues, issue => issue.Severity == DeviceParseIssueSeverity.Error);
        AssertMeasurement(result, "Common/ModelName", "RT-3100");
        AssertMeasurement(result, "Common/Date", "2002-06-16");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Source", "RT");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/Sphere", "-1.50");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/Cylinder", "-0.75");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/Axis", "180");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/ADD", "+0.75");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/VA", "0.1");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/PD", "64.0");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/R/WorkingDistance", "40");
        AssertMeasurement(result, "Measure[@Type='RTSERIAL']/Final/HeaderLine", "Phoropter finaler Verordnungswert");
    }

    [Theory]
    [InlineData(NidekRtSerialPhoropterModel.Rt2100, "NIDEK RT-2100 0000004711 DAY2026/05/29", "RT-2100")]
    [InlineData(NidekRtSerialPhoropterModel.Rt3100, "NIDEK_RT-3100", "RT-3100")]
    [InlineData(NidekRtSerialPhoropterModel.Rt5100, "NIDEK_RT-5100", "RT-5100")]
    public void Parser_ShouldRecognizeRtFamilyHeaders(
        NidekRtSerialPhoropterModel expectedModel,
        string header,
        string expectedModelName)
    {
        var parsed = _parser.Parse(BuildFrame(header, "RT", "RF+01.00-01.00010"));
        var result = _parser.ParseDeviceText(BuildFrame(header, "RT", "RF+01.00-01.00010"));

        Assert.Equal(expectedModel, parsed.Model);
        AssertMeasurement(result, "Common/ModelName", expectedModelName);
    }

    [Fact]
    public void Parser_ShouldWarnForIncompleteDataWithoutExportableRefraction()
    {
        var result = _parser.ParseDeviceText(BuildFrame("NIDEK_RT-3100", "RT", "KM"));

        Assert.Contains(result.Issues, issue =>
            issue.Severity == DeviceParseIssueSeverity.Warning
            && issue.Message.Contains("Keine Refraktionswerte", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Measurements, measurement =>
            measurement.SourcePath.Contains("/Final/", StringComparison.Ordinal)
            || measurement.SourcePath.Contains("/Subjective/", StringComparison.Ordinal));
    }

    [Fact]
    public void MedistarExport_ShouldMapFinalTo6228AndSubjectiveTo6227()
    {
        var content = BuildExportContent(CreateRt3100Frame(), DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());

        Assert.Contains("8402Phoro", content, StringComparison.Ordinal);
        Assert.Contains("6228Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=+ 2.25 Z=- 0.50*  8 PD= 60", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 2.00 Z=- 0.25* 85 PD= 60", content, StringComparison.Ordinal);
        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", content, StringComparison.Ordinal);
        Assert.Contains("6227R.:S=+ 1.25 Z=- 2.00*  7 A=+ 1.50 PD= 61", content, StringComparison.Ordinal);
        Assert.Contains("6227L.:S=+ 1.50 Z=- 1.25* 90 A=+ 1.25 PD= 61", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228--", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExport_WithOnlyFinal_ShouldUseOnly6228()
    {
        var content = BuildExportContent(
            BuildFrame("NIDEK_RT-5100", "RT", "RF+02.25-00.50008", "LF+02.00-00.25085", "PD0060"),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt5100SerialDefault());

        Assert.Contains("6228Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter Maximalwert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227R.:S=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExport_WithRt3100PracticeCapture_ShouldUseOnlyFinal6228()
    {
        var content = BuildExportContent(
            Encoding.ASCII.GetString(LoadHexFixture("rt3100-final-prescription-practice-capture-202606xx.hex")),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());

        Assert.Contains("8402Phoro", content, StringComparison.Ordinal);
        Assert.Contains("6228Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.Contains("6228R.:S=- 1.50 Z=- 0.75*180 A=+ 0.75 PD= 64", content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=- 1.50 Z=- 1.50*175 A=+ 1.25 PD= 64", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6227", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
        Assert.DoesNotContain("--", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExport_WithOnlySubjective_ShouldUseOnly6227()
    {
        var content = BuildExportContent(
            BuildFrame("NIDEK RT-2100 0000004711 DAY2026/05/29", "RT", "Rf+01.25-02.00007", "Lf+01.50-01.25090", "pD0061"),
            DefaultExportProfileDefinitions.CreateMedistarNidekRt2100SerialDefault());

        Assert.Contains("6227Phoropter Maximalwert (Vollkorrektion)", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Phoropter finaler Verordnungswert", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6228R.:S=", content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", content, StringComparison.Ordinal);
    }

    [Fact]
    public void MedistarExport_WithoutFinalOrSubjective_ShouldFailInsteadOfAisOnlyExport()
    {
        var measurements = _parser.ParseDeviceText(BuildFrame("NIDEK_RT-3100", "LM", "R+01.00-01.00010")).Measurements;
        var rules = _mappingAdapter.Adapt(DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault());

        var mapping = _mappingEngine.Map(CreatePatientData(), measurements, rules);

        Assert.True(mapping.HasErrors);
        Assert.Contains(mapping.Issues, issue => issue.Message == "No exportable device measurements were found.");
        Assert.DoesNotContain(mapping.Records, record => record.FieldCode is "6227" or "6228" or "6330");
    }

    [Fact]
    public void OutputWriter_ShouldBuildRt2100FrameWithoutIdBlock()
    {
        var history = CreateHistoricalRecords();

        var result = _writer.BuildFrame(CreatePatientData(), history, NidekRtSerialPhoropterModel.Rt2100);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(NidekRtSerialControlChars.SH, result.Bytes.First());
        Assert.Equal(NidekRtSerialControlChars.ET, result.Bytes.Last());
        Assert.DoesNotContain("DRL", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("DRM<SX>OR+01.00-01.25007", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("DLM<SX>*R+06.25-03.25003", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("ALM<SX>RA+01.50", result.VisibleContent, StringComparison.Ordinal);
    }

    [Fact]
    public void OutputWriter_ShouldBuildRt3100FrameWithIdBlock()
    {
        var history = CreateHistoricalRecords();

        var result = _writer.BuildFrame(CreatePatientData(), history, NidekRtSerialPhoropterModel.Rt3100);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.Contains("<SH>DRL<SX>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("DRM<SX>OR+01.00-01.25007", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("*L+06.50-02.75170", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<ET>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Equal(NidekRtSerialControlChars.SH, result.Bytes.First());
        Assert.Equal(NidekRtSerialControlChars.ET, result.Bytes.Last());
        Assert.Contains("01", result.HexDump, StringComparison.Ordinal);
        Assert.Contains("04", result.HexDump, StringComparison.Ordinal);
    }

    [Fact]
    public void BuiltInProfiles_ShouldBePresentForRtSerialFamily()
    {
        var profiles = new[]
        {
            (Device: DefaultDeviceProfileDefinitions.CreateNidekRt2100SerialDefault(), Export: DefaultExportProfileDefinitions.CreateMedistarNidekRt2100SerialDefault(), Interface: DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt2100SerialDefault()),
            (Device: DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault(), Export: DefaultExportProfileDefinitions.CreateMedistarNidekRt3100SerialDefault(), Interface: DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault()),
            (Device: DefaultDeviceProfileDefinitions.CreateNidekRt5100SerialDefault(), Export: DefaultExportProfileDefinitions.CreateMedistarNidekRt5100SerialDefault(), Interface: DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt5100SerialDefault())
        };

        foreach (var (device, export, interfaceProfile) in profiles)
        {
            Assert.Equal("NIDEK", device.Manufacturer);
            Assert.Equal("Phoropter", device.DeviceType);
            Assert.True(device.IsBidirectional);
            Assert.Equal(DeviceConnectionKind.SerialRs232, device.ConnectionKind);
            Assert.Equal(NidekRtSerialPhoropterConstants.ParserMode, device.ParserMode);
            Assert.NotNull(device.SerialSettings);
            Assert.Equal(2400, device.SerialSettings!.BaudRate);
            Assert.Equal(7, device.SerialSettings.DataBits);
            Assert.Equal(SerialParitySetting.Even, device.SerialSettings.Parity);
            Assert.Equal(SerialStopBitsSetting.Two, device.SerialSettings.StopBits);
            Assert.Contains(device.Measurements, measurement => measurement.SourcePath == "Measure[@Type='RTSERIAL']/Final/R/MedistarLine");
            Assert.Empty(DeviceProfileDefinitionValidator.Validate(device));

            Assert.Equal(device.Metadata.Id, export.SourceDeviceProfileId);
            Assert.Contains(export.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='RTSERIAL']/Final/HeaderLine");
            Assert.Contains(export.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='RTSERIAL']/Subjective/R/MedistarLine");
            Assert.DoesNotContain(export.Rules, rule => rule.TargetFieldCode == "6330");
            Assert.Empty(ExportProfileDefinitionValidator.Validate(export));

            Assert.Equal(device.Metadata.Id, interfaceProfile.DeviceProfileId);
            Assert.Equal(export.Metadata.Id, interfaceProfile.ExportProfileId);
            Assert.False(interfaceProfile.IsActive);
            Assert.NotNull(interfaceProfile.DeviceOutput);
            Assert.False(interfaceProfile.DeviceOutput!.IsEnabled);
            Assert.Equal(NidekRtSerialPhoropterOutputWriter.DeviceOutputFormat, interfaceProfile.DeviceOutput.Format);
            Assert.Equal(NidekRtSerialPhoropterOutputWriter.DefaultFileNameTemplate, interfaceProfile.DeviceOutput.FileNameTemplate);
            Assert.Empty(InterfaceProfileDefinitionValidator.Validate(interfaceProfile));
        }
    }

    private string BuildExportContent(string frame, ExportProfileDefinition exportProfile)
    {
        var measurements = _parser.ParseDeviceText(frame).Measurements;
        var rules = _mappingAdapter.Adapt(exportProfile);
        var mapping = _mappingEngine.Map(CreatePatientData(), measurements, rules);
        var export = new XdtExportBuilder().Build(mapping.Records);

        Assert.False(mapping.HasErrors, string.Join(Environment.NewLine, mapping.Issues.Select(issue => issue.Message)));
        Assert.Empty(export.Issues);
        return export.Content;
    }

    private static string CreateRt3100Frame()
    {
        return BuildFrame(
            "NIDEK_RT-3100",
            "ID0000004711",
            "DAY2026/05/29_02",
            "@",
            "RT",
            "Rf+01.25-02.00007",
            "Lf+01.50-01.25090",
            "Ra+01.50",
            "La+01.25",
            "pD0061",
            "RF+02.25-00.50008",
            "LF+02.00-00.25085",
            "PD0060");
    }

    private static byte[] LoadHexFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "RS232", fileName);
        return File.ReadAllText(path, Encoding.UTF8)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => Convert.ToByte(token, 16))
            .ToArray();
    }

    private static string BuildFrame(params string[] lines)
    {
        var builder = new StringBuilder();
        builder.Append((char)NidekRtSerialControlChars.SH);
        for (var index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                builder.Append((char)NidekRtSerialControlChars.SX);
            }

            builder.Append(lines[index]);
            builder.Append((char)NidekRtSerialControlChars.CR);
        }

        builder.Append((char)NidekRtSerialControlChars.EB);
        builder.Append((char)NidekRtSerialControlChars.ET);
        return builder.ToString();
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "4701-1",
            LastName: "Testfrau",
            FirstName: "Anna",
            BirthDate: "12061955",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: "XDT",
            GdtVersion: "2.10",
            ExaminationType: "Phoro");
    }

    private static IReadOnlyList<AisHistoricalMeasurementRecord> CreateHistoricalRecords()
    {
        return new[]
        {
            new AisHistoricalMeasurementRecord(
                new DateOnly(2026, 5, 29),
                "V0",
                AisHistoricalMeasurementSourceKind.Lensmeter,
                null,
                Array.Empty<string>(),
                new AisHistoricalEyeRefraction("+6.25", "-3.25", "3", "+1.50"),
                new AisHistoricalEyeRefraction("+6.50", "-2.75", "170", "+1.50"),
                "65",
                null,
                true,
                Array.Empty<string>()),
            new AisHistoricalMeasurementRecord(
                new DateOnly(2026, 5, 29),
                "V1",
                AisHistoricalMeasurementSourceKind.Autorefraction,
                null,
                Array.Empty<string>(),
                new AisHistoricalEyeRefraction("+1.00", "-1.25", "7", null),
                new AisHistoricalEyeRefraction("+1.50", "-0.75", "90", null),
                "64",
                null,
                true,
                Array.Empty<string>())
        };
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string expectedValue)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && measurement.Value == expectedValue);
    }
}
