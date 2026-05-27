using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseV1DeviceConnectionCounterTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CountActiveDeviceConnections_ShouldCountOnlyActiveInterfaceProfiles()
    {
        var profiles = new[]
        {
            CreateProfile("interface-active-lan", isActive: true, serialSettings: null),
            CreateProfile("interface-inactive-lan", isActive: false, serialSettings: null),
            CreateProfile("interface-active-serial", isActive: true, SerialCommunicationSettings.Default)
        };

        var count = LicenseV1DeviceConnectionCounter.CountActiveDeviceConnections(profiles);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CountActiveDeviceConnections_ShouldNotTreatSerialRs232AsSeparateModuleLimit()
    {
        var profiles = new[]
        {
            CreateProfile("interface-lan", isActive: true, serialSettings: null),
            CreateProfile("interface-serial", isActive: true, SerialCommunicationSettings.Default)
        };

        var count = LicenseV1DeviceConnectionCounter.CountActiveDeviceConnections(profiles);

        Assert.Equal(2, count);
    }

    private static InterfaceProfileDefinition CreateProfile(
        string id,
        bool isActive,
        SerialCommunicationSettings? serialSettings)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = new ProfileMetadata(
                Id: id,
                Name: id,
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: null,
                Vendor: null,
                Product: null,
                Version: "1.0.0",
                CreatedAt: new DateTimeOffset(NowUtc),
                UpdatedAt: new DateTimeOffset(NowUtc),
                CreatedBy: "XdtDeviceBridge",
                IsBuiltIn: false,
                IsUserDefined: true),
            IsActive = isActive,
            SerialSettings = serialSettings
        };
    }
}
