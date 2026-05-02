using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class XdtExportBuilderTests
{
    [Fact]
    public void Build_ShouldCreateSingleValidLine()
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[] { new ExportFieldRecord("3101", "Testfrau", 1) });

        // Decision: length includes CR/LF to align with v1 export rule (3+4+value+2).
        Assert.Equal("0173101Testfrau\r\n", result.Content);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Build_ShouldSortBySortOrder()
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[]
        {
            new ExportFieldRecord("3102", "B", 2),
            new ExportFieldRecord("3101", "A", 1)
        });

        Assert.StartsWith("0103101A\r\n", result.Content);
    }

    [Fact]
    public void Build_EmptyFieldCode_ShouldCreateError()
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[] { new ExportFieldRecord("", "X", 1) });

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Severity == XdtExportIssueSeverity.Error);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("31A1")]
    public void Build_InvalidFieldCode_ShouldCreateError(string code)
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[] { new ExportFieldRecord(code, "X", 1) });

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void Build_NullValue_ShouldBeTreatedAsEmptyString()
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[] { new ExportFieldRecord("3101", null, 1) });

        Assert.Equal("0093101\r\n", result.Content);
    }

    [Fact]
    public void Build_ShouldUseCrLf()
    {
        var builder = new XdtExportBuilder();
        var result = builder.Build(new[] { new ExportFieldRecord("3101", "X", 1) });

        Assert.Contains("\r\n", result.Content);
        Assert.DoesNotContain("\n\n", result.Content);
    }

    [Fact]
    public void BuildBytesWindows1252_ShouldEncodeUmlauts()
    {
        var builder = new XdtExportBuilder();
        var bytes = builder.BuildBytesWindows1252(new[] { new ExportFieldRecord("3101", "äöüß", 1) });

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = Encoding.GetEncoding(1252).GetString(bytes);
        Assert.Equal("0133101äöüß\r\n", text);
    }
}
