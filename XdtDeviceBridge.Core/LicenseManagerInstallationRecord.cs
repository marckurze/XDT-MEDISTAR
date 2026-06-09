using System.Text.Json.Serialization;

namespace XdtDeviceBridge.Core;

public enum LicenseManagerInstallationStatus
{
    Active = 0,
    Cancelled = 1
}

public sealed record LicenseManagerInstallationRecord(
    string InstallationId,
    string? MachineName,
    int ActiveLicensedDeviceCount,
    IReadOnlyList<IssuedLicenseDeviceRecord> Devices,
    DateTime? LastLicenseIssuedAtUtc = null,
    DateTime? LicenseValidUntilUtc = null,
    string? LastLicenseFilePath = null,
    LicenseManagerInstallationStatus Status = LicenseManagerInstallationStatus.Active,
    DateTime? CancelledAtUtc = null,
    string? CancellationNote = null)
{
    [JsonIgnore]
    public bool IsActive => Status == LicenseManagerInstallationStatus.Active;

    [JsonIgnore]
    public int BillableDeviceCount => IsActive ? ActiveLicensedDeviceCount : 0;

    public static LicenseManagerInstallationRecord FromRequest(LicenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var devices = CreateDeviceRecords(request.Devices);
        return new LicenseManagerInstallationRecord(
            InstallationId: request.InstallationId,
            MachineName: Normalize(request.MachineName),
            ActiveLicensedDeviceCount: devices.Count,
            Devices: devices);
    }

    public static LicenseManagerInstallationRecord FromLicense(IssuedLicenseRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new LicenseManagerInstallationRecord(
            InstallationId: record.InstallationId,
            MachineName: Normalize(record.MachineName),
            ActiveLicensedDeviceCount: record.Devices.Count > 0 ? record.Devices.Count : record.MaxActiveDeviceConnections,
            Devices: record.Devices,
            LastLicenseIssuedAtUtc: record.IssuedAtUtc,
            LicenseValidUntilUtc: record.ValidUntilUtc,
            LastLicenseFilePath: record.OutputFilePath);
    }

    public LicenseManagerInstallationRecord WithLicense(IssuedLicenseRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return this with
        {
            MachineName = Normalize(record.MachineName) ?? MachineName,
            ActiveLicensedDeviceCount = record.Devices.Count > 0 ? record.Devices.Count : record.MaxActiveDeviceConnections,
            Devices = record.Devices,
            LastLicenseIssuedAtUtc = record.IssuedAtUtc,
            LicenseValidUntilUtc = record.ValidUntilUtc,
            LastLicenseFilePath = record.OutputFilePath,
            Status = LicenseManagerInstallationStatus.Active,
            CancelledAtUtc = null,
            CancellationNote = null
        };
    }

    public LicenseManagerInstallationRecord Cancel(DateTime cancelledAtUtc, string? note)
    {
        return this with
        {
            Status = LicenseManagerInstallationStatus.Cancelled,
            CancelledAtUtc = cancelledAtUtc,
            CancellationNote = Normalize(note)
        };
    }

    internal static IReadOnlyList<IssuedLicenseDeviceRecord> CreateDeviceRecords(IEnumerable<LicenseRequestDevice> devices)
    {
        return devices
            .Where(device => device.IsActive && device.IsLicenseRequired)
            .Select(device => new IssuedLicenseDeviceRecord(
                DisplayName: string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                DeviceDisplayName: string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                InterfaceProfileId: string.IsNullOrWhiteSpace(device.InterfaceProfileId) ? device.ProfileId : device.InterfaceProfileId,
                DeviceProfileId: device.DeviceProfileId,
                ConnectionKind: device.ConnectionKind,
                Location: Normalize(device.Location)))
            .ToArray();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
