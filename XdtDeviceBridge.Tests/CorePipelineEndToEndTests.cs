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
        Assert.Equal("ARK1S", patientData.ExaminationType);

        var deviceResult = xmlParser.ParseFile(GetFilePath("nidek-ark1s-sample.xml"));
        Assert.False(deviceResult.HasErrors);

        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "R/AR/ARMedian/Sphere" && m.Value == "-1.25");
        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "L/AR/ARMedian/Sphere" && m.Value == "-1.00");
        Assert.Contains(deviceResult.Measurements, m => m.SourcePath == "PD/PDList[@No='1']/FarPD" && m.Value == "62.0");

        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();

        var mappingResult = mappingEngine.Map(patientData, deviceResult.Measurements, profile.MappingRules);
        Assert.False(mappingResult.HasErrors);
        Assert.Contains(mappingResult.Records, r => r.FieldCode == "8000" && r.Value == "6310");
        Assert.Contains(mappingResult.Records, r => r.FieldCode == "8402" && r.Value == "ARK1S");

        var resultLines = mappingResult.Records.Where(r => r.FieldCode == "6228").ToList();
        Assert.Equal(2, resultLines.Count);
        Assert.Contains(resultLines, r => r.Value?.Contains("R.:S=") == true);
        Assert.Contains(resultLines, r => r.Value?.Contains("L.:S=") == true);
        Assert.All(resultLines, r => Assert.Contains("PD=62.0", r.Value));

        var exportResult = exportBuilder.Build(mappingResult.Records);
        Assert.False(exportResult.HasErrors);
        Assert.False(string.IsNullOrWhiteSpace(exportResult.Content));
        Assert.Contains("\r\n", exportResult.Content);

        Assert.Contains("80006310", exportResult.Content);
        Assert.Contains("3000PAT-100", exportResult.Content);
        Assert.Contains("3101Müller", exportResult.Content);
        Assert.Contains("3102Jörg", exportResult.Content);
        Assert.Contains("8402ARK1S", exportResult.Content);
        Assert.Contains("6228R.:S=", exportResult.Content);
        Assert.Contains("6228L.:S=", exportResult.Content);
        Assert.Contains("PD=62.0", exportResult.Content);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }
}
