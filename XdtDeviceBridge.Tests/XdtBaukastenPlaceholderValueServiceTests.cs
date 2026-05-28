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
    public void CreateDevicePlaceholders_ShouldNotBindLm7ValuesToCl300Profile()
    {
        var cl300Profile = DefaultDeviceProfileDefinitions.CreateTopconCl300Default();
        var lm7Measurements = ParseFixture("Nidek", "LM7", "NIDEK LM7.xml");

        var placeholders = _service.CreateDevicePlaceholders(cl300Profile, lm7Measurements);

        Assert.False(_service.IsCompatibleWithDeviceProfile(cl300Profile, lm7Measurements));
        Assert.DoesNotContain(placeholders, placeholder => placeholder.ExampleValue is "NIDEK" or "LM-7" or "+6.25");
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

    private IReadOnlyList<MeasurementValue> ParseFixture(params string[] pathParts)
    {
        var path = Path.Combine(new[] { AppContext.BaseDirectory, "TestData", "Devices" }.Concat(pathParts).ToArray());
        var result = _parser.ParseFile(path);
        Assert.Empty(result.Issues);
        return result.Measurements;
    }
}
