using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class PlaceholderDisplayHelperTests
{
    [Theory]
    [InlineData("Device.R/AR/ARMedian/Sphere", "Diopter")]
    [InlineData("Device.R/AR/ARMedian/Axis", "Axis")]
    [InlineData("Device.R/NT/IOPAvg", "Iop")]
    [InlineData("Device.R/PACHY/PachyList[@No='1']/Value", "Pachy")]
    [InlineData("Device.L/CCT", "Pachy")]
    [InlineData("Device.R/PrismX", "Prism")]
    [InlineData("Device.R/KM/R1/Radius", "Keratometry")]
    [InlineData("Device.Time", "Time")]
    [InlineData("Device.ModelName", "Raw")]
    public void GetSuggestedFormat_ShouldReturnExpectedFormat(string placeholder, string expected)
    {
        Assert.Equal(expected, PlaceholderDisplayHelper.GetSuggestedFormat(placeholder));
    }

    [Theory]
    [InlineData("Device.R/AR/ARList[@No='1']/Cylinder", "Zylinder rechts Messung 1")]
    [InlineData("Device.R/AR/ARMedian/Sphere", "Sphäre rechts Berechnung / Median")]
    [InlineData("Device.CorrectedIOP", "korrigierter Augendruck")]
    [InlineData("Device.R/CorrectedIOP", "korrigierter Augendruck rechts")]
    [InlineData("Device.PACHYImage", "Pachymetrie-Bild")]
    [InlineData("Device.R/PACHY/PACHYImage", "Pachymetrie-Bild rechts")]
    [InlineData("Device.R/KM/R1/Radius", "Hornhautradius R1 rechts")]
    [InlineData("Device.R/KM/R1/Axis", "Achse Hornhautradius R1 rechts")]
    [InlineData("Device.R/KM/AV", "Keratometrie Durchschnitt rechts")]
    [InlineData("Device.L/KM/R1/Power", "Keratometrie Brechkraft R1 links")]
    [InlineData("Device.R/PrismX/@base", "Basisrichtung Prisma horizontal rechts")]
    [InlineData("Device.Date", "Datum")]
    [InlineData("Device.Time", "Uhrzeit")]
    [InlineData("Device.ModelName", "Modell")]
    public void GetDisplayName_ShouldReturnFriendlyMeasurementName(string placeholder, string expected)
    {
        Assert.Equal(expected, PlaceholderDisplayHelper.GetDisplayName(placeholder));
    }

    [Fact]
    public void GetDeviceSortOrder_ShouldPutRightMedianRefractionBeforeLeftListRefraction()
    {
        var rightMedian = PlaceholderDisplayHelper.GetDeviceSortOrder("R/AR/ARMedian/Sphere");
        var leftList = PlaceholderDisplayHelper.GetDeviceSortOrder("L/AR/ARList[@No='1']/Sphere");

        Assert.True(rightMedian < leftList);
    }
}
