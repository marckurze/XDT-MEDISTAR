using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseRequestTests
{
    private readonly LicenseRequestFileRepository _repository = new();

    [Fact]
    public void Validate_ShouldAcceptValidLicenseRequest()
    {
        var request = CreateLicenseRequest();

        var issues = LicenseRequestValidator.Validate(request);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeActiveLicensedDeviceCount()
    {
        var request = CreateLicenseRequest() with { ActiveLicensedDeviceCount = -1 };

        var issues = LicenseRequestValidator.Validate(request);

        Assert.Contains("ActiveLicensedDeviceCount must not be negative.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyInstallationId()
    {
        var request = CreateLicenseRequest() with { InstallationId = "" };

        var issues = LicenseRequestValidator.Validate(request);

        Assert.Contains("InstallationId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportActiveLicenseRequiredDeviceWithoutId()
    {
        var request = CreateLicenseRequest() with
        {
            Devices = new[]
            {
                CreateDevice() with { Id = "" }
            }
        };

        var issues = LicenseRequestValidator.Validate(request);

        Assert.Contains("Active license-required devices must have an Id.", issues);
    }

    [Fact]
    public void Validate_ShouldReportActiveLicenseRequiredDeviceWithoutName()
    {
        var request = CreateLicenseRequest() with
        {
            Devices = new[]
            {
                CreateDevice() with { Name = " " }
            }
        };

        var issues = LicenseRequestValidator.Validate(request);

        Assert.Contains("Active license-required devices must have a Name.", issues);
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundTripLicenseRequest()
    {
        var filePath = CreateTempFilePath("license-request.json");
        var request = CreateLicenseRequest();

        _repository.Save(filePath, request);
        var loaded = _repository.Load(filePath);

        Assert.Equal(request.RequestId, loaded.RequestId);
        Assert.Equal(request.InstallationId, loaded.InstallationId);
        Assert.Equal(request.ProductCode, loaded.ProductCode);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveDeviceList()
    {
        var filePath = CreateTempFilePath("license-request.json");
        var request = CreateLicenseRequest();

        _repository.Save(filePath, request);
        var loaded = _repository.Load(filePath);

        Assert.Equal(2, loaded.Devices.Count);
        Assert.Contains(loaded.Devices, device => device.Id == "device-1" && device.Name == "NIDEK ARK1S");
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveTerminalServerFlag()
    {
        var filePath = CreateTempFilePath("license-request.json");
        var request = CreateLicenseRequest() with { IsTerminalServer = true };

        _repository.Save(filePath, request);
        var loaded = _repository.Load(filePath);

        Assert.True(loaded.IsTerminalServer);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveCustomerData()
    {
        var filePath = CreateTempFilePath("license-request.json");
        var request = CreateLicenseRequest() with
        {
            Customer = new LicenseRequestCustomer(
                CustomerName: "Praxis Muster",
                Street: "Musterstraße 1",
                PostalCode: "12345",
                City: "Musterstadt",
                Phone: "01234",
                Email: "info@example.test",
                ContactPerson: "Frau Muster")
        };

        _repository.Save(filePath, request);
        var loaded = _repository.Load(filePath);

        Assert.NotNull(loaded.Customer);
        Assert.Equal("Praxis Muster", loaded.Customer.CustomerName);
        Assert.Equal("info@example.test", loaded.Customer.Email);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveDocumentaryDeviceConnectionData()
    {
        var filePath = CreateTempFilePath("license-request.json");
        var request = CreateLicenseRequest() with
        {
            Devices = new[]
            {
                CreateDevice() with
                {
                    InterfaceProfileId = "interface-medistar-device",
                    DisplayName = "MEDISTAR + Gerät",
                    DeviceProfileId = "device-test",
                    DeviceDisplayName = "Testgerät",
                    ConnectionKind = DeviceConnectionKind.SerialRs232
                }
            }
        };

        _repository.Save(filePath, request);
        var loaded = _repository.Load(filePath);

        var device = Assert.Single(loaded.Devices);
        Assert.Equal("interface-medistar-device", device.InterfaceProfileId);
        Assert.Equal("MEDISTAR + Gerät", device.DisplayName);
        Assert.Equal("device-test", device.DeviceProfileId);
        Assert.Equal("Testgerät", device.DeviceDisplayName);
        Assert.Equal(DeviceConnectionKind.SerialRs232, device.ConnectionKind);
    }

    [Fact]
    public void Load_ShouldReadLegacyRequestWithoutCustomerData()
    {
        var filePath = CreateTempFilePath("legacy-license-request.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, """
            {
              "RequestId": "request-legacy",
              "InstallationId": "installation-1",
              "MachineName": "TEST-MACHINE",
              "UserName": "test-user",
              "IsTerminalServer": false,
              "ProductCode": "XDTBOX",
              "AppVersion": "1.0.0",
              "ActiveLicensedDeviceCount": 1,
              "Devices": [
                {
                  "Id": "interface-1",
                  "Name": "MEDISTAR + Test",
                  "Manufacturer": "",
                  "Model": "",
                  "ProfileId": "interface-1",
                  "IsActive": true,
                  "IsLicenseRequired": true
                }
              ],
              "CreatedAt": "2026-05-03T00:00:00Z"
            }
            """);

        var loaded = _repository.Load(filePath);

        Assert.Null(loaded.Customer);
        Assert.Equal(DeviceConnectionKind.NetworkLan, loaded.Devices[0].ConnectionKind);
        Assert.Empty(loaded.Devices[0].DeviceProfileId);
    }

    [Fact]
    public void Load_ShouldThrowFileNotFoundExceptionForMissingFile()
    {
        var filePath = CreateTempFilePath("missing-license-request.json");

        var exception = Assert.Throws<FileNotFoundException>(() => _repository.Load(filePath));

        Assert.Contains("License request file not found:", exception.Message);
        Assert.Equal(filePath, exception.FileName);
    }

    [Fact]
    public void Load_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var filePath = CreateTempFilePath("invalid-license-request.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _repository.Load(filePath));

        Assert.Contains("Invalid license request JSON:", exception.Message);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }

    private static LicenseRequest CreateLicenseRequest()
    {
        return new LicenseRequest(
            RequestId: "request-1",
            InstallationId: "installation-1",
            MachineName: "TEST-MACHINE",
            UserName: "test-user",
            IsTerminalServer: false,
            ProductCode: "XDT-DEVICE-BRIDGE",
            AppVersion: "1.0.0",
            ActiveLicensedDeviceCount: 1,
            Devices: new[]
            {
                CreateDevice(),
                CreateDevice() with
                {
                    Id = "device-2",
                    Name = "Inactive Test Device",
                    IsActive = false,
                    IsLicenseRequired = true
                }
            },
            CreatedAt: new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc));
    }

    private static LicenseRequestDevice CreateDevice()
    {
        return new LicenseRequestDevice(
            Id: "device-1",
            Name: "NIDEK ARK1S",
            Manufacturer: "NIDEK",
            Model: "ARK1S",
            ProfileId: "device-nidek-ark1s-default",
            IsActive: true,
            IsLicenseRequired: true);
    }
}
