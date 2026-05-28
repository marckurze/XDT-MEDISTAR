using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenDeviceCompatibilityServiceTests
{
    private readonly XdtBaukastenDeviceCompatibilityService _service = new();
    private readonly XmlDeviceParser _parser = new();

    public static IEnumerable<object[]> KnownDeviceFixtures()
    {
        yield return new object[] { "NIDEK ARK1S", DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(), RootFixture("nidek-ark1s-sample.xml") };
        yield return new object[] { "NIDEK AR360", DefaultDeviceProfileDefinitions.CreateNidekAr360Default(), DeviceFixture("Nidek", "AR360", "AR360.xml") };
        yield return new object[] { "NIDEK LM7", DefaultDeviceProfileDefinitions.CreateNidekLm7Default(), DeviceFixture("Nidek", "LM7", "NIDEK LM7.xml") };
        yield return new object[] { "NIDEK NT530P", DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(), DeviceFixture("Nidek", "NT530P", "NIDEK_NT530P.xml") };
        yield return new object[] { "NIDEK RT-6100", DefaultDeviceProfileDefinitions.CreateNidekRt6100Default(), WriteTempXml("nidek-rt6100.xml", CreateRt6100ReturnXml(), Encoding.Unicode) };
        yield return new object[] { "TOPCON CL-300", DefaultDeviceProfileDefinitions.CreateTopconCl300Default(), DeviceFixture("Topcon", "CL300", "M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml") };
        yield return new object[] { "TOPCON KR800S", DefaultDeviceProfileDefinitions.CreateTopconKr800Default(), DeviceFixture("Topcon", "KR800S", "M-Serial0036_20131206_213127_TOPCON_KR-800S_.xml") };
        yield return new object[] { "TOPCON KR-1", DefaultDeviceProfileDefinitions.CreateTopconKr1Default(), DeviceFixture("Topcon", "KR1", "M-Serial0001_20190411_120732_TOPCON_KR-1_4430227.xml") };
        yield return new object[] { "TOPCON TRK-2P", DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault(), DeviceFixture("Topcon", "TRK2P", "M-Serial0001_20190411_113829_TOPCON_TRK-2P_5270367.xml") };
        yield return new object[] { "TOPCON CT-1P", DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault(), DeviceFixture("Topcon", "CT1P", "M-Serial0214_20130620_152002_TOPCON_CT-1P_2630218.xml") };
        yield return new object[] { "TOPCON CT-800A", DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault(), DeviceFixture("Topcon", "CT800A", "M-Serial0069_20190411_170716_TOPCON_CT-800A_AA3100481.xml") };
        yield return new object[] { "TOPCON CV-5000", DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(), DeviceFixture("Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml") };
        yield return new object[] { "TOPCON Solos", DefaultDeviceProfileDefinitions.CreateTopconSolosDefault(), DeviceFixture("Topcon", "Solos", "SolosExportSample.xml") };
    }

    [Theory]
    [MemberData(nameof(KnownDeviceFixtures))]
    public void Evaluate_ShouldAcceptKnownDeviceFixtures(string deviceName, DeviceProfileDefinition profile, string fixturePath)
    {
        var result = _service.Evaluate(profile, fixturePath);

        Assert.True(result.IsCompatible, $"{deviceName}: {result.Message}");
        Assert.NotEmpty(result.Measurements);
    }

    [Theory]
    [MemberData(nameof(KnownDeviceFixtures))]
    public void CreateDevicePlaceholders_ShouldExposeValuesForKnownDeviceFixtures(string deviceName, DeviceProfileDefinition profile, string fixturePath)
    {
        var result = _service.Evaluate(profile, fixturePath);
        var placeholderService = new XdtBaukastenPlaceholderValueService(_service);

        var placeholders = placeholderService.CreateDevicePlaceholders(profile, result.Measurements);

        Assert.True(result.IsCompatible, $"{deviceName}: {result.Message}");
        Assert.Contains(placeholders, placeholder => placeholder.ExampleValue != "-");
    }

    [Fact]
    public void Evaluate_ShouldTreatCv5000AliasAsCompatible()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        var measurements = Parse(DeviceFixture("Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml"));

        var result = _service.Evaluate(profile, measurements);

        Assert.True(result.IsCompatible, result.Message);
    }

    [Fact]
    public void EvaluateForWorkbench_ShouldWarnButAllowPreviewForBuiltInModelMismatch()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var measurements = Parse(DeviceFixture("Nidek", "LM7", "NIDEK LM7.xml"));

        var result = _service.EvaluateForWorkbench(profile, measurements);

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.ModelMismatchWarning, result.Status);
        Assert.False(result.IsCompatible);
        Assert.True(result.AllowsPreview);
        Assert.True(result.IsWarning);
        Assert.Equal("NIDEK", result.DetectedCompany);
        Assert.Equal("LM-7", result.DetectedModelName);
        Assert.Contains("Baukasten-Vorschau wird trotzdem erzeugt", result.Message);
    }

    [Fact]
    public void Evaluate_ShouldAllowUserDefinedProfileToUseParsedDeviceFile()
    {
        var profile = CreateUserDefinedDraftProfile();
        var measurements = Parse(DeviceFixture("Nidek", "LM7", "NIDEK LM7.xml"));

        var result = _service.EvaluateForWorkbench(profile, measurements);

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.UnknownButParseable, result.Status);
        Assert.False(result.IsCompatible);
        Assert.True(result.AllowsPreview);
    }

    [Fact]
    public void EvaluateForWorkbench_ShouldReportNoExportableValuesForEmptyParsedData()
    {
        var result = _service.EvaluateForWorkbench(
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            Array.Empty<MeasurementValue>());

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.NoExportableValues, result.Status);
        Assert.False(result.AllowsPreview);
        Assert.Contains("keine exportierbaren Werte", result.Message);
    }

    [Fact]
    public void EvaluateForWorkbench_ShouldReportNotReadableFile()
    {
        var result = _service.EvaluateForWorkbench(
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            Path.Combine(Path.GetTempPath(), "missing-xdtbox-device-file.xml"));

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.NotReadable, result.Status);
        Assert.False(result.AllowsPreview);
    }

    [Fact]
    public void EvaluateForProduction_ShouldStayStrictForModelMismatch()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var measurements = Parse(DeviceFixture("Nidek", "LM7", "NIDEK LM7.xml"));

        var result = _service.EvaluateForProduction(profile, measurements);

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.Incompatible, result.Status);
        Assert.False(result.IsCompatible);
        Assert.False(result.AllowsPreview);
    }

    [Fact]
    public void Evaluate_ShouldReportMalformedXml()
    {
        var path = WriteTempXml("malformed.xml", "<Ophthalmology><Common><ModelName>CV-5000</ModelName></Common>", Encoding.UTF8);

        var result = _service.Evaluate(DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(), path);

        Assert.Equal(XdtBaukastenDeviceCompatibilityStatus.Malformed, result.Status);
        Assert.Contains("konnte nicht", result.Message);
    }

    private IReadOnlyList<MeasurementValue> Parse(string path)
    {
        var result = _parser.ParseFile(path);
        Assert.Empty(result.Issues);
        return result.Measurements;
    }

    private static string RootFixture(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    private static string DeviceFixture(params string[] pathParts)
    {
        return Path.Combine(new[] { AppContext.BaseDirectory, "TestData", "Devices" }.Concat(pathParts).ToArray());
    }

    private static string WriteTempXml(string fileName, string content, Encoding encoding)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, fileName);
        File.WriteAllText(path, content, encoding);
        return path;
    }

    private static string CreateRt6100ReturnXml()
    {
        return """
            <?xml version="1.0" encoding="UTF-16"?>
            <Ophthalmology>
              <Common>
                <Company>NIDEK</Company>
                <ModelName>RT-6100</ModelName>
                <Version>NIDEK_RT_V1.00</Version>
                <Date>2026.05.28</Date>
                <Time>12:34:56</Time>
              </Common>
              <Measure Type="RT">
                <Phoropter>
                  <Corrected CorrectionType="Best" Vision="Distant" Situation="Standard">
                    <R><Sphere>-8.00</Sphere><Cylinder>-1.25</Cylinder><Axis>165</Axis></R>
                    <L><Sphere>-7.50</Sphere><Cylinder>-0.50</Cylinder><Axis>25</Axis></L>
                  </Corrected>
                  <Corrected CorrectionType="Full" Vision="Distant" Situation="Standard">
                    <R><Sphere>-8.25</Sphere><Cylinder>-1.50</Cylinder><Axis>170</Axis></R>
                    <L><Sphere>-7.75</Sphere><Cylinder>-0.75</Cylinder><Axis>30</Axis></L>
                  </Corrected>
                </Phoropter>
              </Measure>
            </Ophthalmology>
            """;
    }

    private static DeviceProfileDefinition CreateUserDefinedDraftProfile()
    {
        var timestamp = DateTimeOffset.UtcNow;
        return new DeviceProfileDefinition(
            new ProfileMetadata(
                "device-userdefined-draft",
                "Eigenes Gerät",
                ProfileKind.DeviceProfile,
                "UserDefined draft device profile for Baukasten tests.",
                "Praxis",
                "Eigenes Gerät",
                "1.0.0",
                timestamp,
                timestamp,
                "Test",
                IsBuiltIn: false,
                IsUserDefined: true),
            "Unbekannt",
            "Eigenes Gerät",
            "Testgerät",
            "Xml",
            Array.Empty<DeviceMeasurementDefinition>(),
            Array.Empty<string>(),
            CanContainMultipleExaminationTypes: true);
    }
}
