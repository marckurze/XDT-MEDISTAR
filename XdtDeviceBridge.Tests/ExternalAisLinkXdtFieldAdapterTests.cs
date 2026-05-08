using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ExternalAisLinkXdtFieldAdapterTests
{
    private readonly ExternalAisLinkXdtFieldAdapter _adapter = new();

    [Fact]
    public void Adapt_ShouldCreateDocumentNameField6302()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.Contains(result.Fields, field => field.FieldCode == "6302" && field.Value == "PDF-Befund");
    }

    [Fact]
    public void Adapt_ShouldCreateFileFormatField6303()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.Contains(result.Fields, field => field.FieldCode == "6303" && field.Value == "PDF");
    }

    [Fact]
    public void Adapt_ShouldCreateDescriptionField6304()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "Messprotokoll", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.Contains(result.Fields, field => field.FieldCode == "6304" && field.Value == "Messprotokoll");
    }

    [Fact]
    public void Adapt_ShouldCreateFullPathField6305()
    {
        var targetPath = @"C:\Xdt\Out\Patient.pdf";

        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", targetPath));

        Assert.True(result.Success);
        Assert.Contains(result.Fields, field => field.FieldCode == "6305" && field.Value == targetPath);
    }

    [Fact]
    public void Adapt_ShouldKeepStableFieldOrder()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "Messprotokoll", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.Equal(new[] { "6302", "6303", "6304", "6305" }, result.Fields.Select(field => field.FieldCode));
    }

    [Fact]
    public void Adapt_ShouldSkipEmptyDescriptionField6304()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.DoesNotContain(result.Fields, field => field.FieldCode == "6304");
    }

    [Fact]
    public void Adapt_ShouldSkipEmptyDocumentNameField6302()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("", "PDF", "", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.DoesNotContain(result.Fields, field => field.FieldCode == "6302");
    }

    [Fact]
    public void Adapt_ShouldSkipEmptyFileFormatField6303()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "", "", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.DoesNotContain(result.Fields, field => field.FieldCode == "6303");
    }

    [Fact]
    public void Adapt_ShouldRequireFullPath()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", ""));

        Assert.False(result.Success);
        Assert.Empty(result.Fields);
        Assert.Contains("full path", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Adapt_ShouldNotCreateXdtLengthPrefixes()
    {
        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "Messprotokoll", @"C:\Xdt\Out\Patient.pdf"));

        Assert.True(result.Success);
        Assert.Equal("6302", result.Fields[0].FieldCode);
        Assert.Equal("PDF-Befund", result.Fields[0].Value);
        Assert.DoesNotContain(result.Fields, field => field.Value?.StartsWith(field.FieldCode, StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Adapt_ShouldNotPerformFileOperations()
    {
        var targetPath = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "Patient.pdf");

        var result = Adapt(new ExternalAisLinkFieldSet("PDF-Befund", "PDF", "", targetPath));

        Assert.True(result.Success);
        Assert.False(File.Exists(targetPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(targetPath)!));
    }

    [Fact]
    public void AdaptedFields_ShouldBeAcceptedByXdtExportBuilder()
    {
        var adapterResult = Adapt(new ExternalAisLinkFieldSet("A", "PDF", "B", @"C:\Xdt\Out\Patient.pdf"));
        var exportBuilder = new XdtExportBuilder();

        var exportResult = exportBuilder.Build(adapterResult.Fields);

        Assert.True(adapterResult.Success);
        Assert.False(exportResult.HasErrors);
        Assert.Contains("0106302A\r\n", exportResult.Content);
        Assert.Contains("0126303PDF\r\n", exportResult.Content);
        Assert.Contains("0106304B\r\n", exportResult.Content);
        Assert.Contains("6305C:\\Xdt\\Out\\Patient.pdf\r\n", exportResult.Content);
    }

    private ExternalAisLinkXdtFieldAdapterResult Adapt(ExternalAisLinkFieldSet fieldSet)
    {
        return _adapter.Adapt(fieldSet);
    }
}
