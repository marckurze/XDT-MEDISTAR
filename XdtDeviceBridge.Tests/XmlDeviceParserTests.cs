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

        AssertMeasurement(result, "PD/PDList[@No='1']/FarPD", "62.0", null, "PDList");
        AssertMeasurement(result, "PD/PDList[@No='1']/NearPD", "59.0", null, "PDList");
    }

    [Fact]
    public void ParseFile_ShouldDistinguishArListByNoAttribute()
    {
        var parser = new XmlDeviceParser();
        var result = parser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));

        AssertMeasurement(result, "R/AR/ARList[@No='1']/Sphere", "-1.00", "R", "ARList");
    }

    [Fact]
    public void ParseFile_ShouldParseLm7LanXmlAttributesAndPdValues()
    {
        var parser = new XmlDeviceParser();
        var path = WriteTempXml("""
            <Ophthalmology>
              <Common>
                <Date>20260506</Date>
                <Time>150100</Time>
              </Common>
              <Measure Type="LM">
                <LM>
                  <R>
                    <Sphere unit="D">1.50</Sphere>
                    <Cylinder unit="D">-0.50</Cylinder>
                    <Axis unit="deg">128</Axis>
                    <PrismX unit="pri" base="out">0.75</PrismX>
                    <PrismY unit="pri" base="up">1.00</PrismY>
                    <ConfidenceIndex>9</ConfidenceIndex>
                  </R>
                  <L>
                    <Error>measurement failed</Error>
                  </L>
                </LM>
                <PD>
                  <Distance unit="mm">59.0</Distance>
                  <DistanceR unit="mm">29.5</DistanceR>
                  <DistanceL unit="mm">29.5</DistanceL>
                </PD>
              </Measure>
            </Ophthalmology>
            """);

        var result = parser.ParseFile(path);

        Assert.False(result.HasErrors);
        AssertMeasurement(result, "Common/Date", "20260506", null, null);
        AssertMeasurement(result, "Common/Time", "150100", null, null);
        AssertMeasurement(result, "Measure[@Type='LM']/@Type", "LM", null, null);
        AssertMeasurement(result, "Measure[@Type='LM']/LM/R/Sphere", "1.50", "R", "LM", "D");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/R/Sphere/@unit", "D", "R", "LM");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/R/PrismX/@base", "out", "R", "LM");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/R/PrismY/@base", "up", "R", "LM");
        AssertMeasurement(result, "Measure[@Type='LM']/PD/Distance", "59.0", null, "PD", "mm");
        AssertMeasurement(result, "Measure[@Type='LM']/PD/DistanceR", "29.5", null, "PD", "mm");
        AssertMeasurement(result, "Measure[@Type='LM']/PD/DistanceL", "29.5", null, "PD", "mm");
        AssertMeasurement(result, "Measure[@Type='LM']/LM/L/Error", "measurement failed", "L", "LM");
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

    private static void AssertMeasurement(
        DeviceParseResult result,
        string sourcePath,
        string expectedValue,
        string? eye,
        string? group,
        string? unit = null)
    {
        var measurement = Assert.Single(result.Measurements, m => m.SourcePath == sourcePath);
        Assert.Equal(expectedValue, measurement.Value);
        Assert.Equal(eye, measurement.Eye);
        Assert.Equal(group, measurement.Group);
        if (unit is not null)
        {
            Assert.Equal(unit, measurement.Unit);
        }
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
