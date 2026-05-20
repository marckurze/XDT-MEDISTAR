using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ImportFileClassifierTests
{
    private readonly ImportFileClassifier _classifier = new();

    [Fact]
    public void Classify_ShouldThrowArgumentExceptionForEmptyPath()
    {
        Assert.Throws<ArgumentException>(() => _classifier.Classify(string.Empty));
    }

    [Theory]
    [InlineData("patient.gdt", ImportFileKind.AisGdt)]
    [InlineData("patient.GDT", ImportFileKind.AisGdt)]
    [InlineData("patient.xdt", ImportFileKind.AisXdt)]
    [InlineData("device.xml", ImportFileKind.DeviceXml)]
    [InlineData("device.txt", ImportFileKind.DeviceText)]
    [InlineData("device.csv", ImportFileKind.DeviceCsv)]
    [InlineData("image.jpg", ImportFileKind.AttachmentImage)]
    [InlineData("image.jpeg", ImportFileKind.AttachmentImage)]
    [InlineData("image.png", ImportFileKind.AttachmentImage)]
    [InlineData("document.pdf", ImportFileKind.AttachmentPdf)]
    [InlineData("scan.tif", ImportFileKind.AttachmentFile)]
    [InlineData("scan.tiff", ImportFileKind.AttachmentFile)]
    [InlineData("dicom.dcm", ImportFileKind.AttachmentFile)]
    [InlineData("video.mp4", ImportFileKind.AttachmentFile)]
    [InlineData("audio.mp3", ImportFileKind.AttachmentFile)]
    [InlineData("signal.wav", ImportFileKind.AttachmentFile)]
    [InlineData("NIDEK_LM_Stylesheet.xsl", ImportFileKind.Unknown)]
    [InlineData("unknown.dat", ImportFileKind.Unknown)]
    public void Classify_ShouldMapExtensionToExpectedKind(string fileName, ImportFileKind expectedKind)
    {
        var result = _classifier.Classify(Path.Combine("C:\\Import", fileName));

        Assert.Equal(expectedKind, result.Kind);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public void Classify_ShouldSetFileNameAndLowerCaseExtension()
    {
        var result = _classifier.Classify("C:\\Import\\SAMPLE.XML");

        Assert.Equal("SAMPLE.XML", result.FileName);
        Assert.Equal(".xml", result.Extension);
        Assert.Equal("C:\\Import\\SAMPLE.XML", result.FilePath);
    }
}
