using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class DefaultDeviceProfilesTests
{
    [Fact]
    public void CreateNidekArk1sDefault_ShouldCreateStandardProfile()
    {
        var profile = DefaultDeviceProfiles.CreateNidekArk1sDefault();

        Assert.NotNull(profile);
        Assert.Equal("NIDEK ARK1S", profile.Name);
        Assert.Equal(DeviceParserMode.Xml, profile.DeviceParserMode);
        Assert.Equal("Windows-1252", profile.OutputEncoding);
        Assert.Equal(10, profile.AssignmentWindowMinutes);
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainPatientRules()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;

        Assert.Contains(rules, r => r.SourcePath == "AIS.PatientNumber" && r.TargetFieldCode == "3000");
        Assert.Contains(rules, r => r.SourcePath == "AIS.LastName" && r.TargetFieldCode == "3101");
        Assert.Contains(rules, r => r.SourcePath == "AIS.FirstName" && r.TargetFieldCode == "3102");
        Assert.Contains(rules, r => r.SourcePath == "AIS.BirthDate" && r.TargetFieldCode == "3103");
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainMedistarControlRules()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;

        Assert.Contains(rules, r =>
            r.TargetFieldCode == "8000"
            && r.OutputTemplate == "6310"
            && r.IsEnabled);

        Assert.Contains(rules, r =>
            r.TargetFieldCode == "8402"
            && r.SourcePath == "AIS.ExaminationType"
            && r.IsEnabled);
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainTwoMedistarResultLines()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;
        var resultLines = rules.Where(r => r.TargetFieldCode == "6228" && r.IsEnabled).ToList();

        Assert.Equal(2, resultLines.Count);
        Assert.Contains(resultLines, r => r.OutputTemplate.Contains("R.:S="));
        Assert.Contains(resultLines, r => r.OutputTemplate.Contains("L.:S="));
        Assert.All(resultLines, r => Assert.Contains("PD=", r.OutputTemplate));
    }

    [Fact]
    public void CreateNidekArk1sDefault_SortOrderShouldBeUniqueAndAscending()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;
        var orders = rules.Select(r => r.SortOrder).ToList();

        Assert.Equal(orders.Count, orders.Distinct().Count());
        Assert.True(orders.SequenceEqual(orders.OrderBy(x => x)));
    }
}
