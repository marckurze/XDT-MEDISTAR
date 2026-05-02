using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class CorePipelineEndToEndTests
{
    [Fact]
    public void EndToEnd_ShouldParseMapAndBuildExportContent()
    {
        var gdtParser = new GdtParser();
        var xmlParser = new XmlDeviceParser();
        var patientMapper = new PatientDataMapper();
        var mappingEngine = new MappingEngine();
        var exportBuilder = new XdtExportBuilder();

        var gdtResult = gdtParser.ParseFile(GetFilePath("sample-gdt-utf8.gdt"));
        Assert.False(gdtResult.HasErrors);

        var patientData = patientMapper.Map(gdtResult.Records);
        Assert.Equal("PAT-100", patientData.PatientNumber);
        Assert.Equal("Müller", patientData.LastName);
        Assert.Equal("Jörg", patientData.FirstName);

        var deviceResult = xmlParser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));
        Assert.False(deviceResult.HasErrors);

        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "R/AR/ARMedian/Sphere" && m.Value == "-1.25");
        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "L/AR/ARMedian/Sphere" && m.Value == "-1.00");
        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "PD/PDList/FarPD" && m.Value == "62.0");

        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();

        var mappingResult = mappingEngine.Map(patientData, deviceResult.Measurements, profile.MappingRules);
        Assert.False(mappingResult.HasErrors);

        var exportResult = exportBuilder.Build(mappingResult.Records);
        Assert.False(exportResult.HasErrors);
        Assert.False(string.IsNullOrWhiteSpace(exportResult.Content));
        Assert.Contains("\r\n", exportResult.Content);

        Assert.Contains("3000PAT-100", exportResult.Content);
        Assert.Contains("3101Müller", exportResult.Content);
        Assert.Contains("3102Jörg", exportResult.Content);
        Assert.Contains("9001-1.25", exportResult.Content);
        Assert.Contains("9002-0.50", exportResult.Content);
        Assert.Contains("9003090", exportResult.Content);
        Assert.Contains("9004-1.50", exportResult.Content);
        Assert.Contains("9011-1.00", exportResult.Content);
        Assert.Contains("9012-0.25", exportResult.Content);
        Assert.Contains("9013085", exportResult.Content);
        Assert.Contains("9014-1.12", exportResult.Content);
        Assert.Contains("902162.0", exportResult.Content);
        Assert.Contains("902259.0", exportResult.Content);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }
}
