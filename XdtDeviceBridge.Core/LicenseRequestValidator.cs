namespace XdtDeviceBridge.Core;

public static class LicenseRequestValidator
{
    public static IReadOnlyList<string> Validate(LicenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            issues.Add("RequestId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.InstallationId))
        {
            issues.Add("InstallationId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            issues.Add("ProductCode must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.AppVersion))
        {
            issues.Add("AppVersion must not be empty.");
        }

        if (request.ActiveLicensedDeviceCount < 0)
        {
            issues.Add("ActiveLicensedDeviceCount must not be negative.");
        }

        if (request.CreatedAt == default)
        {
            issues.Add("CreatedAt must not be default.");
        }

        if (request.Devices is null)
        {
            issues.Add("Devices must not be null.");
        }
        else
        {
            foreach (var device in request.Devices.Where(device => device.IsActive && device.IsLicenseRequired))
            {
                if (string.IsNullOrWhiteSpace(device.Id))
                {
                    issues.Add("Active license-required devices must have an Id.");
                }

                if (string.IsNullOrWhiteSpace(device.Name))
                {
                    issues.Add("Active license-required devices must have a Name.");
                }
            }
        }

        return issues;
    }
}
