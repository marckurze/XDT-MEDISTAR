using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class XmlDeviceParserTests
{
    [Fact]
    public void ParseFile_ShouldParseXmlSuccessfully()
    {
        var parser = new XmlDeviceParser();
        var path = GetFilePath("nidek-ark1s-sample.xml");

        var result = parser.ParseFile(path);

        Assert.NotEmpty(result.Measurements);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void ParseFile_ShouldFindRightArMedianValues()
    {
        var parser = new XmlDeviceParser();
        var result = parser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));

        AssertMeasurement(result, "R/AR/ARMedian/Sphere", "-1.25", "R", "ARMedian");
        AssertMeasurement(result, "R/AR/ARMedian/Cylinder", "-0.50", "R", "ARMedian");
        AssertMeasurement(result, "R/AR/ARMedian/Axis", "090", "R", "ARMedian");
        AssertMeasurement(result, "R/AR/ARMedian/SE", "-1.50", "R", "ARMedian");
    }

    [Fact]
    public void ParseFile_ShouldFindLeftArMedianValues()
    {
        var parser = new XmlDeviceParser();
        var result = parser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));

        AssertMeasurement(result, "L/AR/ARMedian/Sphere", "-1.00", "L", "ARMedian");
        AssertMeasurement(result, "L/AR/ARMedian/Cylinder", "-0.25", "L", "ARMedian");
        AssertMeasurement(result, "L/AR/ARMedian/Axis", "085", "L", "ARMedian");
        AssertMeasurement(result, "L/AR/ARMedian/SE", "-1.12", "L", "ARMedian");
    }

    [Fact]
    public void ParseFile_ShouldFindPdValues()
    {
        var parser = new XmlDeviceParser();
        var result = parser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));

        AssertMeasurement(result, "PD/PDList/FarPD", "62.0", null, "PDList");
        AssertMeasurement(result, "PD/PDList/NearPD", "59.0", null, "PDList");
    }

    [Fact]
    public void ParseFile_ShouldDistinguishArListByNoAttribute()
    {
        var parser = new XmlDeviceParser();
        var result = parser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));

        AssertMeasurement(result, "R/AR/ARList[@No='1']/Sphere", "-1.00", "R", "ARList");
    }

    [Fact]
    public void ParseFile_InvalidXml_ShouldCreateError()
    {
        var parser = new XmlDeviceParser();
        var path = WriteTempXml("<R><AR><ARMedian><Sphere>-1.25</Sphere></AR>");

        var result = parser.ParseFile(path);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Severity == DeviceParseIssueSeverity.Error);
    }

    private static void AssertMeasurement(DeviceParseResult result, string sourcePath, string expectedValue, string? eye, string? group)
    {
        var measurement = Assert.Single(result.Measurements.Where(m => m.SourcePath == sourcePath));
        Assert.Equal(expectedValue, measurement.Value);
        Assert.Equal(eye, measurement.Eye);
        Assert.Equal(group, measurement.Group);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    private static string WriteTempXml(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }
}
