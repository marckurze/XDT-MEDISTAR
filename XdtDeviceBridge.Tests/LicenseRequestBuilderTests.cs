using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseRequestBuilderTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicenseRequestBuilder _builder = new();

    [Fact]
    public void Build_ShouldUseInstallationIdFromInstallationInfo()
    {
        var request = BuildDefaultRequest();

        Assert.Equal("installation-1", request.InstallationId);
    }

    [Fact]
    public void Build_ShouldUseMachineNameAndUserNameFromInstallationInfo()
    {
        var request = BuildDefaultRequest();

        Assert.Equal("TEST-MACHINE", request.MachineName);
        Assert.Equal("test-user", request.UserName);
    }

    [Fact]
    public void Build_ShouldUseTerminalServerFlagFromInstallationInfo()
    {
        var installation = CreateInstallationInfo() with { IsTerminalServer = true };

        var request = _builder.Build(
            installation,
            CreateInterfaceProfiles(),
            "XDT-DEVICE-BRIDGE",
            "1.0.0",
            CreatedAtUtc);

        Assert.True(request.IsTerminalServer);
    }

    [Fact]
    public void Build_ShouldSetProductCodeAndAppVersion()
    {
        var request = BuildDefaultRequest();

        Assert.Equal("XDT-DEVICE-BRIDGE", request.ProductCode);
        Assert.Equal("1.0.0", request.AppVersion);
    }

    [Fact]
    public void Build_ShouldCreateRequestId()
    {
        var request = BuildDefaultRequest();

        Assert.False(string.IsNullOrWhiteSpace(request.RequestId));
    }

    [Fact]
    public void Build_ShouldCountOnlyActiveLicenseRequiredProfiles()
    {
        var request = BuildDefaultRequest();

        Assert.Equal(1, request.ActiveLicensedDeviceCount);
    }

    [Fact]
    public void Build_ShouldIncludeInactiveLicenseRequiredProfilesWithoutCountingThemAsActive()
    {
        var request = BuildDefaultRequest();

        Assert.Equal(2, request.Devices.Count);
        Assert.Contains(request.Devices, device => device.Id == "interface-inactive" && !device.IsActive);
        Assert.Equal(1, request.ActiveLicensedDeviceCount);
    }

    [Fact]
    public void Build_ShouldIgnoreProfilesThatAreNotLicenseRequired()
    {
        var request = BuildDefaultRequest();

        Assert.DoesNotContain(request.Devices, device => device.Id == "interface-not-licensed");
    }

    [Fact]
    public void Build_ShouldThrowArgumentNullExceptionForNullInstallation()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.Build(
            null!,
            CreateInterfaceProfiles(),
            "XDT-DEVICE-BRIDGE",
            "1.0.0",
            CreatedAtUtc));
    }

    [Fact]
    public void Build_ShouldThrowArgumentNullExceptionForNullInterfaceProfiles()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.Build(
            CreateInstallationInfo(),
            null!,
            "XDT-DEVICE-BRIDGE",
            "1.0.0",
            CreatedAtUtc));
    }

    [Fact]
    public void Build_ShouldThrowArgumentExceptionForEmptyProductCode()
    {
        var exception = Assert.Throws<ArgumentException>(() => _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            "",
            "1.0.0",
            CreatedAtUtc));

        Assert.Contains("Product code must not be empty.", exception.Message);
    }

    [Fact]
    public void Build_ShouldThrowArgumentExceptionForEmptyAppVersion()
    {
        var exception = Assert.Throws<ArgumentException>(() => _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            "XDT-DEVICE-BRIDGE",
            " ",
            CreatedAtUtc));

        Assert.Contains("App version must not be empty.", exception.Message);
    }

    [Fact]
    public void Build_ShouldThrowArgumentExceptionForDefaultCreatedAtUtc()
    {
        var exception = Assert.Throws<ArgumentException>(() => _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            "XDT-DEVICE-BRIDGE",
            "1.0.0",
            default));

        Assert.Contains("CreatedAt must not be default.", exception.Message);
    }

    private LicenseRequest BuildDefaultRequest()
    {
        return _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            "XDT-DEVICE-BRIDGE",
            "1.0.0",
            CreatedAtUtc);
    }

    private static InstallationInfo CreateInstallationInfo()
    {
        return new InstallationInfo(
            InstallationId: "installation-1",
            MachineName: "TEST-MACHINE",
            UserName: "test-user",
            IsTerminalServer: false,
            CreatedAt: CreatedAtUtc.AddDays(-1));
    }

    private static IReadOnlyList<InterfaceProfileDefinition> CreateInterfaceProfiles()
    {
        return new[]
        {
            CreateInterfaceProfile("interface-active", "Active Device", isActive: true, isLicenseRequired: true),
            CreateInterfaceProfile("interface-inactive", "Inactive Device", isActive: false, isLicenseRequired: true),
            CreateInterfaceProfile("interface-not-licensed", "Not Licensed Device", isActive: true, isLicenseRequired: false)
        };
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string id,
        string name,
        bool isActive,
        bool isLicenseRequired)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name),
            IsActive = isActive,
            IsLicenseRequired = isLicenseRequired
        };
    }

    private static ProfileMetadata CreateMetadata(string id, string name)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
