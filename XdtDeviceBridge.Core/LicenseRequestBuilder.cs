namespace XdtDeviceBridge.Core;

public sealed class LicenseRequestBuilder
{
    public LicenseRequest Build(
        InstallationInfo installation,
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        string productCode,
        string appVersion,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(installation);
        ArgumentNullException.ThrowIfNull(interfaceProfiles);

        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code must not be empty.", nameof(productCode));
        }

        if (string.IsNullOrWhiteSpace(appVersion))
        {
            throw new ArgumentException("App version must not be empty.", nameof(appVersion));
        }

        if (createdAtUtc == default)
        {
            throw new ArgumentException("CreatedAt must not be default.", nameof(createdAtUtc));
        }

        var devices = interfaceProfiles
            .Where(profile => profile.IsLicenseRequired)
            .Select(profile => new LicenseRequestDevice(
                Id: profile.Metadata.Id,
                Name: profile.Metadata.Name,
                Manufacturer: string.Empty,
                Model: string.Empty,
                ProfileId: profile.Metadata.Id,
                IsActive: profile.IsActive,
                IsLicenseRequired: profile.IsLicenseRequired))
            .ToArray();

        var activeLicensedDeviceCount = devices.Count(device => device.IsActive && device.IsLicenseRequired);

        return new LicenseRequest(
            RequestId: Guid.NewGuid().ToString(),
            InstallationId: installation.InstallationId,
            MachineName: installation.MachineName,
            UserName: installation.UserName,
            IsTerminalServer: installation.IsTerminalServer,
            ProductCode: productCode,
            AppVersion: appVersion,
            ActiveLicensedDeviceCount: activeLicensedDeviceCount,
            Devices: devices,
            CreatedAt: createdAtUtc);
    }

    public LicenseRequest Build(
        InstallationInfo installation,
        IEnumerable<InterfaceProfileDefinition> interfaceProfiles,
        IEnumerable<DeviceProfileDefinition> deviceProfiles,
        LicenseRequestCustomer? customer,
        string productCode,
        string appVersion,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(installation);
        ArgumentNullException.ThrowIfNull(interfaceProfiles);
        ArgumentNullException.ThrowIfNull(deviceProfiles);

        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code must not be empty.", nameof(productCode));
        }

        if (string.IsNullOrWhiteSpace(appVersion))
        {
            throw new ArgumentException("App version must not be empty.", nameof(appVersion));
        }

        if (createdAtUtc == default)
        {
            throw new ArgumentException("CreatedAt must not be default.", nameof(createdAtUtc));
        }

        var deviceProfilesById = deviceProfiles.ToDictionary(
            profile => profile.Metadata.Id,
            profile => profile,
            StringComparer.OrdinalIgnoreCase);

        var devices = interfaceProfiles
            .Where(profile => profile.IsLicenseRequired)
            .Select(profile =>
            {
                deviceProfilesById.TryGetValue(profile.DeviceProfileId, out var deviceProfile);
                var deviceDisplayName = deviceProfile?.Metadata.Name ?? profile.DeviceProfileId;

                return new LicenseRequestDevice(
                    Id: profile.Metadata.Id,
                    Name: profile.Metadata.Name,
                    Manufacturer: deviceProfile?.Manufacturer ?? string.Empty,
                    Model: deviceProfile?.Model ?? string.Empty,
                    ProfileId: profile.Metadata.Id,
                    IsActive: profile.IsActive,
                    IsLicenseRequired: profile.IsLicenseRequired,
                    InterfaceProfileId: profile.Metadata.Id,
                    DisplayName: profile.Metadata.Name,
                    DeviceProfileId: profile.DeviceProfileId,
                    DeviceDisplayName: deviceDisplayName,
                    ConnectionKind: deviceProfile?.ConnectionKind ?? DeviceConnectionKind.NetworkLan);
            })
            .ToArray();

        var activeLicensedDeviceCount = devices.Count(device => device.IsActive && device.IsLicenseRequired);

        return new LicenseRequest(
            RequestId: Guid.NewGuid().ToString(),
            InstallationId: installation.InstallationId,
            MachineName: installation.MachineName,
            UserName: installation.UserName,
            IsTerminalServer: installation.IsTerminalServer,
            ProductCode: productCode,
            AppVersion: appVersion,
            ActiveLicensedDeviceCount: activeLicensedDeviceCount,
            Devices: devices,
            CreatedAt: createdAtUtc,
            Customer: customer);
    }
}
