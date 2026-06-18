using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenPlaceholderValueServiceTests
{
    private readonly XmlDeviceParser _parser = new();
    private readonly XdtBaukastenPlaceholderValueService _service = new();

    [Fact]
    public void CreateDevicePlaceholders_ShouldShowLm7MeasurementValuesWithoutProfileSwitch()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();
        var measurements = ParseFixture("Nidek", "LM7", "NIDEK LM7.xml");

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        var rightSphere = Assert.Single(placeholders, placeholder => placeholder.DisplayName == "R Sphere");
        Assert.Equal("+6.25", rightSphere.ExampleValue);
        Assert.Equal("{Device.Measure[@Type='LM']/LM/R/Sphere}", rightSphere.Token);
        Assert.DoesNotContain(placeholders, placeholder => placeholder.DisplayName == "R Sphere" && placeholder.ExampleValue == "-");
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldShowLoadedFileValuesWhenProfileMismatchesInWorkbench()
    {
        var cl300Profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var lm7Measurements = ParseFixture("Nidek", "LM7", "NIDEK LM7.xml");

        var placeholders = _service.CreateDevicePlaceholders(cl300Profile, lm7Measurements);

        Assert.False(_service.IsCompatibleWithDeviceProfile(cl300Profile, lm7Measurements));
        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@type='LM']/LM/R/Sphare}"
            && placeholder.ExampleValue == "+6.25");
        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@type='LM']/LM/L/ADD}"
            && placeholder.ExampleValue == "+1.50");
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldBindCl300ValuesToCl300Profile()
    {
        var cl300Profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var cl300Measurements = ParseFixture("Topcon", "CL300", "M-Serial0001_20120101_000000_TOPCON_CL-300_00.xml");

        var placeholders = _service.CreateDevicePlaceholders(cl300Profile, cl300Measurements);

        var rightSphere = Assert.Single(placeholders, placeholder => placeholder.DisplayName == "R Sphere");
        Assert.Equal("-0.25", rightSphere.ExampleValue);
        Assert.True(_service.IsCompatibleWithDeviceProfile(cl300Profile, cl300Measurements));
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldKeepAr360ReferenceValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekAr360Default();
        var measurements = ParseFixture("Nidek", "AR360", "AR360.xml");

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        Assert.Equal("+2.00", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "R Sphere").ExampleValue);
        Assert.Equal("-1.25", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "R Cylinder").ExampleValue);
        Assert.Equal("172", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "R Axis").ExampleValue);
        Assert.True(_service.IsCompatibleWithDeviceProfile(profile, measurements));
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldBindCv5000AliasValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconCv5000Default();
        var measurements = ParseFixture("Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml");

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        Assert.True(_service.IsCompatibleWithDeviceProfile(profile, measurements));
        Assert.Equal("CV-5000", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "ModelName").ExampleValue);
        Assert.Contains(placeholders, placeholder =>
            placeholder.DisplayName == "Prescription R MEDISTAR-Zeile"
            && placeholder.ExampleValue != "-");
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldExposeKr800SVisualAcuityValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateTopconKr800Default();
        var measurements = ParseFixture("Topcon", "KR800S", "M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml");

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        Assert.True(_service.IsCompatibleWithDeviceProfile(profile, measurements));
        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/R}"
            && placeholder.ExampleValue == "0.6");
        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@Type='SBJ']/RefractionTest/Type[@No='1']/ExamDistance[@No='1']/VA/L}"
            && placeholder.ExampleValue == "1.0");
        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@Type='REF']/REF/R/MedistarLineWithVisualAcuity}");
    }

    [Theory]
    [InlineData("AR360", "Nidek", "AR360", "AR360.xml")]
    [InlineData("LM7", "Nidek", "LM7", "NIDEK LM7.xml")]
    [InlineData("NT530P", "Nidek", "NT530P", "NIDEK_NT530P.xml")]
    [InlineData("KR800S", "Topcon", "KR800S", "M-Serial0426_20241126_145500_TOPCON_KR-800S_4871341.xml")]
    [InlineData("CV5000", "Topcon", "CV5000", "M-Serial1234_20130625_170509656_TOPCON_CV-5000_10111.xml")]
    public void CreateDevicePlaceholders_ShouldExposeAllParsedMeasurementValuesForRepresentativeXmlDevices(
        string profileKey,
        string manufacturer,
        string deviceFolder,
        string fileName)
    {
        var profile = CreateRepresentativeProfile(profileKey);
        var measurements = ParseFixture(manufacturer, deviceFolder, fileName);

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        AssertAllParsedMeasurementTokensAreOffered(measurements, placeholders);
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldShowRt3100PracticeCaptureValues()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault();
        var measurements = new NidekRtSerialPhoropterParser()
            .ParseDeviceText(Encoding.ASCII.GetString(LoadHexFixture("rt3100-final-prescription-practice-capture-202606xx.hex")))
            .Measurements;

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        Assert.Equal("-1.50", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R Sphere").ExampleValue);
        Assert.Equal("-0.75", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R Cylinder").ExampleValue);
        Assert.Equal("180", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R Axis").ExampleValue);
        Assert.Equal("+0.75", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R ADD").ExampleValue);
        Assert.Equal("-1.50", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final L Sphere").ExampleValue);
        Assert.Equal("-1.50", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final L Cylinder").ExampleValue);
        Assert.Equal("175", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final L Axis").ExampleValue);
        Assert.Equal("+1.25", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final L ADD").ExampleValue);
        Assert.Equal("64.0", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R PD").ExampleValue);
        Assert.Equal("40", Assert.Single(placeholders, placeholder => placeholder.DisplayName == "Final R WD").ExampleValue);
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldExposeAllParsedMeasurementValuesForRtSerialPracticeCapture()
    {
        var profile = DefaultDeviceProfileDefinitions.CreateNidekRt3100SerialDefault();
        var measurements = new NidekRtSerialPhoropterParser()
            .ParseDeviceText(Encoding.ASCII.GetString(LoadHexFixture("rt3100-final-prescription-practice-capture-202606xx.hex")))
            .Measurements;

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        AssertAllParsedMeasurementTokensAreOffered(measurements, placeholders);
    }

    [Fact]
    public void CreateDevicePlaceholders_ShouldShowParsedValuesForUserDefinedDraftProfile()
    {
        var profile = CreateUserDefinedDraftProfile();
        var measurements = ParseFixture("Nidek", "LM7", "NIDEK LM7.xml");

        var placeholders = _service.CreateDevicePlaceholders(profile, measurements);

        Assert.Contains(placeholders, placeholder =>
            placeholder.Token == "{Device.Measure[@type='LM']/LM/R/Sphare}"
            && placeholder.ExampleValue == "+6.25");
        Assert.DoesNotContain(placeholders.Take(6), placeholder => placeholder.Token.Contains("/@", StringComparison.Ordinal));
    }

    private IReadOnlyList<MeasurementValue> ParseFixture(params string[] pathParts)
    {
        var path = Path.Combine(new[] { AppContext.BaseDirectory, "TestData", "Devices" }.Concat(pathParts).ToArray());
        var result = _parser.ParseFile(path);
        Assert.Empty(result.Issues);
        return result.Measurements;
    }

    private static byte[] LoadHexFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "Devices", "Nidek", "RS232", fileName);
        return File.ReadAllText(path, Encoding.UTF8)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => Convert.ToByte(token, 16))
            .ToArray();
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

    private static DeviceProfileDefinition CreateRepresentativeProfile(string profileKey)
    {
        return profileKey switch
        {
            "AR360" => DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
            "LM7" => DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            "NT530P" => DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
            "KR800S" => DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
            "CV5000" => DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKey), profileKey, "Unknown representative device profile.")
        };
    }

    private static void AssertAllParsedMeasurementTokensAreOffered(
        IReadOnlyList<MeasurementValue> measurements,
        IReadOnlyList<XdtBaukastenPlaceholder> placeholders)
    {
        var expectedTokens = measurements
            .Where(measurement => IsDeviceMeasurementPlaceholderCandidate(measurement.SourcePath))
            .Where(measurement => !string.IsNullOrWhiteSpace(measurement.Value))
            .Select(measurement => "{Device." + measurement.SourcePath + "}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.NotEmpty(expectedTokens);
        Assert.All(expectedTokens, token =>
            Assert.Contains(placeholders, placeholder => string.Equals(placeholder.Token, token, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool IsDeviceMeasurementPlaceholderCandidate(string sourcePath)
    {
        return !sourcePath.StartsWith("Common/", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "Company", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "ModelName", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "MachineNo", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "ROMVersion", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "Version", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "Date", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sourcePath, "Time", StringComparison.OrdinalIgnoreCase);
    }
}
