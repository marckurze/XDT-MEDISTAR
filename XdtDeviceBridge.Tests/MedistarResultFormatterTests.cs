using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class MedistarResultFormatterTests
{
    private readonly MedistarResultFormatter _formatter = new();

    [Fact]
    public void FormatDiopter_ShouldFormatPositiveZero()
    {
        Assert.Equal("+ 0.00", _formatter.FormatDiopter("+0.00"));
    }

    [Fact]
    public void FormatDiopter_ShouldFormatNegativeValue()
    {
        Assert.Equal("- 0.25", _formatter.FormatDiopter("-0.25"));
    }

    [Fact]
    public void FormatDiopter_ShouldFormatPositiveValue()
    {
        Assert.Equal("+ 1.25", _formatter.FormatDiopter("+1.25"));
    }

    [Fact]
    public void FormatDiopter_ShouldReturnEmptyForNull()
    {
        Assert.Equal(string.Empty, _formatter.FormatDiopter(null));
    }

    [Fact]
    public void FormatDiopter_ShouldAddPlusSignWhenSignIsMissing()
    {
        // MEDISTAR card text should show the sign explicitly for neutral or positive refraction values.
        Assert.Equal("+ 0.00", _formatter.FormatDiopter("0.00"));
    }

    [Fact]
    public void FormatAxis_ShouldPadTwoDigitAxisToThreeCharacters()
    {
        Assert.Equal(" 49", _formatter.FormatAxis("49"));
    }

    [Fact]
    public void FormatAxis_ShouldPadSingleDigitAxisToThreeCharacters()
    {
        // Axis values are aligned to the same three-character width as 120.
        Assert.Equal("  9", _formatter.FormatAxis("9"));
    }

    [Fact]
    public void FormatAxis_ShouldKeepThreeDigitAxis()
    {
        Assert.Equal("120", _formatter.FormatAxis("120"));
    }

    [Fact]
    public void FormatPd_ShouldTrimValue()
    {
        Assert.Equal("61", _formatter.FormatPd(" 61 "));
    }
}
