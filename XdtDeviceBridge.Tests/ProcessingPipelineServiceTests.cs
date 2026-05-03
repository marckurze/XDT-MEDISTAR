using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ProcessingPipelineServiceTests
{
    [Fact]
    public void ProcessFiles_ShouldProduceExportContent()
    {
        var service = new ProcessingPipelineService();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();

        var result = service.ProcessFiles(GetFilePath("sample-gdt-utf8.gdt"), GetFilePath("nidek-ark1s-sample.xml"), profile);

        Assert.False(result.HasErrors);
        Assert.NotNull(result.Patient);
        Assert.NotEmpty(result.Measurements);
        Assert.NotEmpty(result.ExportRecords);
        Assert.False(string.IsNullOrWhiteSpace(result.ExportContent));

        Assert.Contains(result.ExportRecords, r => r.FieldCode == "8000" && r.Value == "6310");
        Assert.Contains(result.ExportRecords, r => r.FieldCode == "8402" && r.Value == "ARK1S");

        var resultLines = result.ExportRecords.Where(r => r.FieldCode == "6228").ToList();
        Assert.Equal(2, resultLines.Count);
        Assert.Contains(resultLines, r => r.Value?.Contains("R.:S=") == true);
        Assert.Contains(resultLines, r => r.Value?.Contains("L.:S=") == true);
        Assert.All(resultLines, r => Assert.Contains("PD=62.0", r.Value));

        Assert.Contains("3101Müller", result.ExportContent);
        Assert.Contains("3102Jörg", result.ExportContent);
        Assert.Contains("8402ARK1S", result.ExportContent);
        Assert.Contains("6228R.:S=", result.ExportContent);
        Assert.Contains("6228L.:S=", result.ExportContent);
        Assert.Contains("PD=62.0", result.ExportContent);
    }

    [Fact]
    public void ProcessFiles_InvalidGdt_ShouldReturnGdtParsingError()
    {
        var service = new ProcessingPipelineService();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
        var gdtPath = WriteTempFile("12\n", ".gdt");

        var result = service.ProcessFiles(gdtPath, GetFilePath("nidek-ark1s-sample.xml"), profile);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Stage == ProcessingStage.GdtParsing && i.Severity == ProcessingIssueSeverity.Error);
    }

    [Fact]
    public void ProcessFiles_InvalidXml_ShouldReturnDeviceParsingError()
    {
        var service = new ProcessingPipelineService();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();
        var xmlPath = WriteTempFile("<Root><R><AR></Root>", ".xml");

        var result = service.ProcessFiles(GetFilePath("sample-gdt-utf8.gdt"), xmlPath, profile);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Stage == ProcessingStage.DeviceParsing && i.Severity == ProcessingIssueSeverity.Error);
    }

    [Fact]
    public void ProcessFiles_UnsupportedMode_ShouldReturnError()
    {
        var service = new ProcessingPipelineService();
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault() with { DeviceParserMode = DeviceParserMode.Unknown };

        var result = service.ProcessFiles(GetFilePath("sample-gdt-utf8.gdt"), GetFilePath("nidek-ark1s-sample.xml"), profile);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Stage == ProcessingStage.DeviceParsing && i.Message == "Unsupported device parser mode");
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
    }

    private static string WriteTempFile(string content, string extension)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), extension);
        File.WriteAllText(path, content);
        return path;
    }
}
