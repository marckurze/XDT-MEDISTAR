using XdtDeviceBridge.Core;

namespace XdtBox.LicenseWeb.Services;

public sealed record LicenseWebSnapshot(
    IReadOnlyList<LicenseManagerCustomerRecord> Customers,
    IReadOnlyList<IssuedLicenseRecord> History,
    LicenseManagerSettings Settings,
    LicenseWebRuntimeInfo Runtime);

public sealed record LicenseWebRuntimeInfo(
    string DataRoot,
    string LicensesFolder,
    string RequestsFolder,
    string BackupFolder,
    bool PrivateKeyConfigured,
    bool PrivateKeyFileExists);

public sealed record LicenseWebImportResult(
    LicenseRequest Request,
    string RequestFile,
    LicenseManagerCustomerRecord Customer,
    bool ExistingCustomerMatched,
    IReadOnlyList<string> Messages);

public sealed record LicenseWebCreateLicenseResult(
    string OutputFile,
    string FileName,
    IssuedLicenseRecord HistoryRecord);
