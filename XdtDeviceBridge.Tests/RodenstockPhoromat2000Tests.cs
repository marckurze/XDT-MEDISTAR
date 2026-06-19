using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class RodenstockPhoromat2000Tests
{
    private readonly RodenstockPhoromat2000Parser _parser = new();
    private readonly RodenstockPhoromat2000OutputWriter _writer = new();
    private readonly MappingEngine _mappingEngine = new();
    private readonly ExportProfileMappingAdapter _mappingAdapter = new();

    [Fact]
    public void Parser_ShouldReadFnPdWdAndVaWithoutExportingVaAutomatically()
    {
        var result = _parser.ParseText(CreateReceiveFrame(endMarker: "\u0001*PC_RCV_E\u0004\r\n"));

        Assert.DoesNotContain(result.Issues, issue => issue.Severity == DeviceParseIssueSeverity.Error);
        AssertMeasurement(result, "Common/ModelName", "Phoromat 2000");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/SP", "+6.50");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/CY", "-3.00");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/AX", "172");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/AD", "+1.00");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/PH", "1.5 I");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/PV", "0.5 U");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/VA", "1.0");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/L/VA", "0.8");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/PD/R", "30");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/PD/L", "31");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/WD/MedistarLine", "WD= 40");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/MedistarLine", "R.:S=+ 6.50 Z=- 3.00*172 P= 1.5 I 0.5 U PD= 30 A=+ 1.00");
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/L/MedistarLine", "L.:S=+ 7.25 Z=- 2.75*168 P= 2.5 O 0.25 D PD= 31 A=+ 0.75");

        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarRodenstockPhoromat2000Default();
        var mappingResult = _mappingEngine.Map(CreatePatientData(), result.Measurements, _mappingAdapter.Adapt(exportProfile));
        var exportResult = new XdtExportBuilder().Build(mappingResult.Records);

        Assert.False(mappingResult.HasErrors, string.Join(Environment.NewLine, mappingResult.Issues.Select(issue => issue.Message)));
        Assert.Empty(exportResult.Issues);
        Assert.Contains("6228R.:S=+ 6.50 Z=- 3.00*172 P= 1.5 I 0.5 U PD= 30 A=+ 1.00", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6228L.:S=+ 7.25 Z=- 2.75*168 P= 2.5 O 0.25 D PD= 31 A=+ 0.75", exportResult.Content, StringComparison.Ordinal);
        Assert.Contains("6227WD= 40", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("VA=1.0", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("VA=0.8", exportResult.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("6330", exportResult.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Parser_ShouldTolerateLfOnlyReceiveEndMarker()
    {
        var result = _parser.ParseText(CreateReceiveFrame(endMarker: "\u0001*PC_RCV_E\u0004\n"));

        Assert.DoesNotContain(result.Issues, issue => issue.Severity == DeviceParseIssueSeverity.Error);
        AssertMeasurement(result, "Measure[@Type='PHOROMAT2000']/FN/R/MedistarLine", "R.:S=+ 6.50 Z=- 3.00*172 P= 1.5 I 0.5 U PD= 30 A=+ 1.00");
    }

    [Fact]
    public void OutputWriter_ShouldCreatePcSendFrameWithSupportedBlocksOnly()
    {
        var selected = new[]
        {
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Lensmeter, "V0", "+1.00", "-0.25", "11", "+1.25", "+1.50", "-0.50", "22", "+1.00", "62"),
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Autorefraction, "V1", "+2.00", "-1.25", "33", null, "+2.50", "-1.50", "44", null, null),
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Phoropter, "V2", "+6.50", "-3.00", "172", "+1.00", "+7.25", "-2.75", "168", "+0.75", "61")
        };

        var result = _writer.BuildFrame(
            CreatePatientData(),
            selected,
            new DateTimeOffset(2026, 6, 18, 10, 31, 35, TimeSpan.Zero));

        Assert.True(result.Success, result.ErrorMessage);
        Assert.StartsWith("<SOH>*PC_SND_S<EOT><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*Phoromat 2000|000000001|0<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*PD|62|62|<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*LM<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*AR<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*FN<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*SP|+7.25|+6.50|<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*CY|-2.75|-3.00|<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*AX|168|172|<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.Contains("<STX>*TIME|0026/06/18 10:31:35|<ETB><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.EndsWith("<SOH>*PC_SND_E<EOT><LF>", result.VisibleContent, StringComparison.Ordinal);
        Assert.DoesNotContain("*VA", result.VisibleContent, StringComparison.Ordinal);
        Assert.DoesNotContain("*V3", result.VisibleContent, StringComparison.Ordinal);
        Assert.DoesNotContain("*V4", result.VisibleContent, StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultProfiles_ShouldBeInactiveBidirectionalBetaChain()
    {
        var device = DefaultDeviceProfileDefinitions.CreateRodenstockPhoromat2000Default();
        var export = DefaultExportProfileDefinitions.CreateMedistarRodenstockPhoromat2000Default();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarRodenstockPhoromat2000Default();

        Assert.Equal("Rodenstock Phoromat 2000", device.Metadata.Name);
        Assert.Equal(RodenstockPhoromat2000Constants.ParserMode, device.ParserMode);
        Assert.True(device.IsBidirectional);
        Assert.Equal(DeviceConnectionKind.SerialRs232, device.ConnectionKind);
        Assert.Equal(9600, device.SerialSettings?.BaudRate);
        Assert.Equal(8, device.SerialSettings?.DataBits);
        Assert.Equal(SerialParitySetting.None, device.SerialSettings?.Parity);
        Assert.Equal(SerialStopBitsSetting.One, device.SerialSettings?.StopBits);
        Assert.Equal(SerialHandshakeSetting.None, device.SerialSettings?.Handshake);
        Assert.False(device.SerialSettings?.DtrEnable);
        Assert.False(device.SerialSettings?.RtsEnable);
        Assert.Equal(30000, device.SerialSettings?.ReadTimeoutMilliseconds);
        Assert.Equal("device-rodenstock-phoromat2000-default", export.SourceDeviceProfileId);
        Assert.Contains(export.Rules, rule => rule.TargetFieldCode == "6228" && rule.SourcePath == "Device.Measure[@Type='PHOROMAT2000']/FN/R/MedistarLine");
        Assert.Contains(export.Rules, rule => rule.TargetFieldCode == "6227" && rule.SourcePath == "Device.Measure[@Type='PHOROMAT2000']/WD/MedistarLine");
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.Equal(RodenstockPhoromat2000OutputWriter.DeviceOutputFormat, profile.DeviceOutput?.Format);
        Assert.Empty(DeviceProfileDefinitionValidator.Validate(device));
        Assert.Empty(ExportProfileDefinitionValidator.Validate(export));
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void BaukastenDeviceOutputPlaceholders_ShouldExposePhoromatValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateRodenstockPhoromat2000Default();
        var history = new[]
        {
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Lensmeter, "V0", "+1.00", "-0.25", "11", null, "+1.50", "-0.50", "22", null, "62"),
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Autorefraction, "V1", "+2.00", "-1.25", "33", null, "+2.50", "-1.50", "44", null, null),
            CreateHistoryRecord(AisHistoricalMeasurementSourceKind.Phoropter, "V2", "+6.50", "-3.00", "172", "+1.00", "+7.25", "-2.75", "168", "+0.75", "61")
        };

        var rules = XdtBaukastenDeviceOutputRuleService.CreateDefaultRules(profile);
        var placeholders = XdtBaukastenDeviceOutputRuleService.CreatePlaceholders(profile, CreatePatientData(), history);

        Assert.Contains(rules, rule => rule.TargetFieldCode == "Serial/FN/R/SP" && rule.SourcePath == "PhoromatInput.FN.R.SP");
        Assert.Contains(placeholders, placeholder => placeholder.Token == "{PhoromatInput.FN.R.SP}" && placeholder.ExampleValue == "+6.50");
        Assert.Contains(placeholders, placeholder => placeholder.Token == "{PhoromatInput.FN.L.AX}" && placeholder.ExampleValue == "168");
        Assert.Contains(placeholders, placeholder => placeholder.Token == "{PhoromatInput.PD}" && placeholder.ExampleValue == "62");
        Assert.True(placeholders.Count > 10);
    }

    private static string CreateReceiveFrame(string endMarker)
    {
        return "\u0001*PC_RCV_S\u0004\n"
            + "\u0002*Phoromat 2000|000000001|0\u0017\n"
            + "\u0002*FN\u0017\n"
            + "\u0002*SP|+7.25|+6.50|\u0017\n"
            + "\u0002*CY|-2.75|-3.00|\u0017\n"
            + "\u0002*AX|168|172|\u0017\n"
            + "\u0002*AD|+0.75|+1.00|\u0017\n"
            + "\u0002*PH|BO 2.50|BI 1.50|\u0017\n"
            + "\u0002*PV|BD 0.25|BU 0.50|\u0017\n"
            + "\u0002*VA|0.8|1.0|\u0017\n"
            + "\u0002*PD|31|30|\u0017\n"
            + "\u0002*WD|40|\u0017\n"
            + endMarker;
    }

    private static AisHistoricalMeasurementRecord CreateHistoryRecord(
        AisHistoricalMeasurementSourceKind sourceKind,
        string sourcePrefix,
        string rightSphere,
        string rightCylinder,
        string rightAxis,
        string? rightAdd,
        string leftSphere,
        string leftCylinder,
        string leftAxis,
        string? leftAdd,
        string? pd)
    {
        return new AisHistoricalMeasurementRecord(
            Date: new DateOnly(2026, 6, 18),
            SourcePrefix: sourcePrefix,
            SourceKind: sourceKind,
            Variant: null,
            OriginalLines: Array.Empty<string>(),
            RightEye: new AisHistoricalEyeRefraction(rightSphere, rightCylinder, rightAxis, rightAdd),
            LeftEye: new AisHistoricalEyeRefraction(leftSphere, leftCylinder, leftAxis, leftAdd),
            Pd: pd,
            Vd: null,
            IsExportableToCv5000: true,
            ParseWarnings: Array.Empty<string>());
    }

    private static PatientData CreatePatientData()
    {
        return new PatientData(
            PatientNumber: "4711",
            LastName: "Test",
            FirstName: "Person",
            BirthDate: "01011980",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: "MEDISTAR",
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: "Phoro");
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string expectedValue)
    {
        Assert.Contains(result.Measurements, measurement =>
            string.Equals(measurement.SourcePath, sourcePath, StringComparison.Ordinal)
            && string.Equals(measurement.Value, expectedValue, StringComparison.Ordinal));
    }
}
