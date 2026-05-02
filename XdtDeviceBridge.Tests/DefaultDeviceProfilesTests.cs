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
    public void CreateNidekArk1sDefault_ShouldContainRightArMedianRules()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;

        Assert.Contains(rules, r => r.SourcePath == "Device.R/AR/ARMedian/Sphere" && r.TargetFieldCode == "9001");
        Assert.Contains(rules, r => r.SourcePath == "Device.R/AR/ARMedian/Cylinder" && r.TargetFieldCode == "9002");
        Assert.Contains(rules, r => r.SourcePath == "Device.R/AR/ARMedian/Axis" && r.TargetFieldCode == "9003");
        Assert.Contains(rules, r => r.SourcePath == "Device.R/AR/ARMedian/SE" && r.TargetFieldCode == "9004");
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainLeftArMedianRules()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;

        Assert.Contains(rules, r => r.SourcePath == "Device.L/AR/ARMedian/Sphere" && r.TargetFieldCode == "9011");
        Assert.Contains(rules, r => r.SourcePath == "Device.L/AR/ARMedian/Cylinder" && r.TargetFieldCode == "9012");
        Assert.Contains(rules, r => r.SourcePath == "Device.L/AR/ARMedian/Axis" && r.TargetFieldCode == "9013");
        Assert.Contains(rules, r => r.SourcePath == "Device.L/AR/ARMedian/SE" && r.TargetFieldCode == "9014");
    }

    [Fact]
    public void CreateNidekArk1sDefault_ShouldContainPdRules()
    {
        var rules = DefaultDeviceProfiles.CreateNidekArk1sDefault().MappingRules;

        Assert.Contains(rules, r => r.SourcePath == "Device.PD/PDList/FarPD" && r.TargetFieldCode == "9021");
        Assert.Contains(rules, r => r.SourcePath == "Device.PD/PDList/NearPD" && r.TargetFieldCode == "9022");
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
