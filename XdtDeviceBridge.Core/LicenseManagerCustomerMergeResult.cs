namespace XdtDeviceBridge.Core;

public sealed record LicenseManagerCustomerMergeResult(
    IReadOnlyList<LicenseManagerCustomerRecord> Customers,
    LicenseManagerCustomerRecord TargetCustomer,
    IReadOnlyList<string> RemovedCustomerIds,
    int MergedCustomerCount,
    int InstallationCount,
    int ActiveInstallationCount,
    int ActiveLicensedDeviceCount);
