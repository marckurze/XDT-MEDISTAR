using System.Text;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class XdtBaukastenTextEncodingReaderTests
{
    [Fact]
    public void Decode_ShouldReadWindows1252UmlautsWithoutReplacementCharacters()
    {
        var bytes = Encoding.Latin1.GetBytes("0310103Geräteanbindung GA_XDT");

        var text = XdtBaukastenTextEncodingReader.Decode(bytes);

        Assert.Equal("0310103Geräteanbindung GA_XDT", text);
        Assert.DoesNotContain("�", text);
    }

    [Fact]
    public void Decode_ShouldKeepUtf8Umlauts()
    {
        var bytes = Encoding.UTF8.GetBytes("Geräteanbindung");

        var text = XdtBaukastenTextEncodingReader.Decode(bytes);

        Assert.Equal("Geräteanbindung", text);
    }

    [Fact]
    public void Decode_ShouldRespectUtf16LittleEndianBom()
    {
        var payload = Encoding.Unicode.GetBytes("Geräteanbindung");
        var bytes = new byte[payload.Length + 2];
        bytes[0] = 0xFF;
        bytes[1] = 0xFE;
        payload.CopyTo(bytes, 2);

        var text = XdtBaukastenTextEncodingReader.Decode(bytes);

        Assert.Equal("Geräteanbindung", text);
    }
}
