using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

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
    public void Build_WithDeviceProfiles_ShouldIncludeCustomerData()
    {
        var customer = new LicenseRequestCustomer(
            CustomerName: "Praxis Muster",
            Street: "Musterstraße 1",
            PostalCode: "12345",
            City: "Musterstadt",
            Phone: "01234",
            Email: "info@example.test",
            ContactPerson: "Frau Muster");

        var request = _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            CreateDeviceProfiles(),
            customer,
            XdtBoxLicenseConstants.ProductCode,
            "1.0.0",
            CreatedAtUtc);

        Assert.NotNull(request.Customer);
        Assert.Equal("Praxis Muster", request.Customer.CustomerName);
        Assert.Equal("Musterstadt", request.Customer.City);
    }

    [Fact]
    public void Build_WithDeviceProfiles_ShouldDocumentDeviceConnectionNames()
    {
        var request = _builder.Build(
            CreateInstallationInfo(),
            CreateInterfaceProfiles(),
            CreateDeviceProfiles(),
            LicenseRequestCustomer.Empty,
            XdtBoxLicenseConstants.ProductCode,
            "1.0.0",
            CreatedAtUtc);

        var activeDevice = Assert.Single(request.Devices, device => device.Id == "interface-active");
        Assert.Equal("interface-active", activeDevice.InterfaceProfileId);
        Assert.Equal("Active Device", activeDevice.DisplayName);
        Assert.Equal("device-nidek-ark1s-default", activeDevice.DeviceProfileId);
        Assert.Equal("NIDEK ARK1S", activeDevice.DeviceDisplayName);
        Assert.Equal(DeviceConnectionKind.NetworkLan, activeDevice.ConnectionKind);
        Assert.Equal("NIDEK", activeDevice.Manufacturer);
        Assert.Equal("ARK1S", activeDevice.Model);
    }

    [Fact]
    public void Build_WithDeviceProfiles_ShouldKeepDeviceNamesNonBindingForLicensedCount()
    {
        var profiles = CreateInterfaceProfiles()
            .Select(profile => profile.Metadata.Id == "interface-active"
                ? profile with { Metadata = profile.Metadata with { Name = "Umbenannte Geräteanbindung" } }
                : profile)
            .ToArray();

        var request = _builder.Build(
            CreateInstallationInfo(),
            profiles,
            CreateDeviceProfiles(),
            LicenseRequestCustomer.Empty,
            XdtBoxLicenseConstants.ProductCode,
            "1.0.0",
            CreatedAtUtc);

        Assert.Equal(1, request.ActiveLicensedDeviceCount);
        Assert.Contains(request.Devices, device => device.DisplayName == "Umbenannte Geräteanbindung");
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

    private static IReadOnlyList<DeviceProfileDefinition> CreateDeviceProfiles()
    {
        return new[]
        {
            DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault()
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
