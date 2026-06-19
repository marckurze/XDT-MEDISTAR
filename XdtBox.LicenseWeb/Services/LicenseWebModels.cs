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
    string EnvironmentLabel,
    string? PublicBaseUrl,
    bool DataRootConfigured,
    bool DataRootSafe,
    bool DataRootReadable,
    bool DataRootWritable,
    bool PrivateKeyConfigured,
    bool PrivateKeyPathSafe,
    bool PrivateKeyFileExists,
    bool PrivateKeyReadable,
    string PrivateKeyStatus,
    bool LicenseSignatureAvailable,
    bool BackupAvailable);

public sealed record LicenseWebDiagnosticItem(
    string Name,
    bool IsOk,
    string Status,
    string Detail);

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

public sealed record LicenseWebCustomerMigrationResult(
    LicenseManagerCustomerRecord Customer,
    int ImportedHistoryCount,
    int ActiveInstallationCount,
    int ActiveDeviceCount,
    IReadOnlyList<string> Messages);

public sealed record LicenseWebCustomerUpdate(
    string? CustomerNumber,
    string CustomerName,
    string Street,
    string PostalCode,
    string City,
    string Phone,
    string? Email,
    string? ContactPerson,
    string? InvoiceEmail,
    string? Iban,
    string? Bic,
    string? AccountHolder,
    LicenseManagerPaymentMethod PaymentMethod,
    IReadOnlyList<LicenseWebCustomerDeviceLocationUpdate> DeviceLocations);

public sealed record LicenseWebCustomerDeviceLocationUpdate(
    string Key,
    string? Location);

public sealed record LicenseWebAdminCredential(
    string Username,
    string PasswordHash,
    string PasswordSalt,
    int PasswordIterations,
    DateTime UpdatedAtUtc);
