namespace XdtDeviceBridge.Core;

public sealed class LicenseManagerCustomerMergeService
{
    public LicenseManagerCustomerMergeResult Merge(
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        IReadOnlyCollection<string> selectedCustomerIds,
        LicenseManagerCustomerMergeSelection selection)
    {
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(selectedCustomerIds);
        ArgumentNullException.ThrowIfNull(selection);

        var selectedIds = selectedCustomerIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        if (selectedIds.Count < 2)
        {
            throw new InvalidOperationException("Mindestens zwei Kunden muessen fuer die Zusammenfuehrung markiert sein.");
        }

        var selectedCustomers = customers
            .Where(customer => selectedIds.Contains(customer.Id))
            .Select(customer => customer.WithNormalizedInstallations())
            .ToArray();
        if (selectedCustomers.Length != selectedIds.Count)
        {
            throw new InvalidOperationException("Mindestens ein markierter Kunde wurde nicht gefunden.");
        }

        var target = selectedCustomers.FirstOrDefault(customer => customer.Id == selection.TargetCustomerId)
            ?? throw new InvalidOperationException("Der Zielkunde ist nicht Teil der markierten Kunden.");

        var mergedInstallations = selectedCustomers
            .SelectMany(customer => customer.EffectiveInstallations)
            .Where(installation => !string.IsNullOrWhiteSpace(installation.InstallationId))
            .GroupBy(installation => installation.InstallationId, StringComparer.OrdinalIgnoreCase)
            .Select(group => MergeInstallationGroup(group.ToArray()))
            .OrderByDescending(installation => installation.IsActive)
            .ThenByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
            .ThenBy(installation => installation.InstallationId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var paymentMethod = selection.PaymentMethod;
        var mergedTarget = (target with
        {
            CustomerNumber = Normalize(selection.CustomerNumber),
            CustomerName = selection.CustomerName.Trim(),
            ContactPerson = Normalize(selection.ContactPerson),
            Street = selection.Street.Trim(),
            PostalCode = selection.PostalCode.Trim(),
            City = selection.City.Trim(),
            Phone = selection.Phone.Trim(),
            Email = Normalize(selection.Email),
            InvoiceEmail = Normalize(selection.InvoiceEmail),
            Iban = Normalize(selection.Iban),
            Bic = Normalize(selection.Bic),
            AccountHolder = Normalize(selection.AccountHolder),
            SepaDirectDebitConsent = paymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit,
            AlwaysInvoice = paymentMethod == LicenseManagerPaymentMethod.BankTransfer,
            Installations = mergedInstallations,
            UpdatedAtUtc = DateTime.UtcNow
        }).WithNormalizedInstallations();

        var mergedCustomers = customers
            .Select(customer => customer.Id == target.Id ? mergedTarget : customer.WithNormalizedInstallations())
            .Where(customer => customer.Id == target.Id || !selectedIds.Contains(customer.Id))
            .ToArray();
        var removedIds = selectedIds
            .Where(id => id != target.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        return new LicenseManagerCustomerMergeResult(
            Customers: mergedCustomers,
            TargetCustomer: mergedTarget,
            RemovedCustomerIds: removedIds,
            MergedCustomerCount: selectedCustomers.Length,
            InstallationCount: mergedTarget.EffectiveInstallations.Count,
            ActiveInstallationCount: mergedTarget.ActiveInstallationCount,
            ActiveLicensedDeviceCount: mergedTarget.BillableDeviceCount);
    }

    private static LicenseManagerInstallationRecord MergeInstallationGroup(
        IReadOnlyList<LicenseManagerInstallationRecord> installations)
    {
        var latestLicense = installations
            .OrderByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
            .First();
        var latestStatus = installations
            .OrderByDescending(GetStatusTimestamp)
            .First();
        var devices = installations
            .SelectMany(installation => installation.Devices)
            .GroupBy(CreateDeviceKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => MergeDeviceGroup(group.ToArray()))
            .ToArray();
        var status = latestStatus.Status;
        var activeLicensedDeviceCount = status == LicenseManagerInstallationStatus.Active
            ? devices.Length > 0
                ? devices.Length
                : installations.Where(installation => installation.IsActive)
                    .Select(installation => installation.ActiveLicensedDeviceCount)
                    .DefaultIfEmpty(latestLicense.ActiveLicensedDeviceCount)
                    .Max()
            : installations.Select(installation => installation.ActiveLicensedDeviceCount).DefaultIfEmpty(0).Max();

        return latestLicense with
        {
            MachineName = installations
                .OrderByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
                .Select(installation => Normalize(installation.MachineName))
                .FirstOrDefault(value => value is not null),
            ActiveLicensedDeviceCount = activeLicensedDeviceCount,
            Devices = devices,
            LastLicenseIssuedAtUtc = installations
                .Select(installation => installation.LastLicenseIssuedAtUtc)
                .Where(value => value.HasValue)
                .DefaultIfEmpty(latestLicense.LastLicenseIssuedAtUtc)
                .Max(),
            LicenseValidUntilUtc = latestLicense.LicenseValidUntilUtc,
            LastLicenseFilePath = installations
                .OrderByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
                .Select(installation => Normalize(installation.LastLicenseFilePath))
                .FirstOrDefault(value => value is not null),
            Status = status,
            CancelledAtUtc = status == LicenseManagerInstallationStatus.Cancelled ? latestStatus.CancelledAtUtc : null,
            CancellationNote = status == LicenseManagerInstallationStatus.Cancelled ? latestStatus.CancellationNote : null
        };
    }

    private static DateTime GetStatusTimestamp(LicenseManagerInstallationRecord installation)
    {
        return installation.Status == LicenseManagerInstallationStatus.Cancelled
            ? installation.CancelledAtUtc ?? installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue
            : installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue;
    }

    private static IssuedLicenseDeviceRecord MergeDeviceGroup(IReadOnlyList<IssuedLicenseDeviceRecord> devices)
    {
        var preferred = devices.Last();
        var location = devices
            .Select(device => Normalize(device.Location))
            .LastOrDefault(value => value is not null);
        return preferred with { Location = location };
    }

    private static string CreateDeviceKey(IssuedLicenseDeviceRecord device)
    {
        return !string.IsNullOrWhiteSpace(device.InterfaceProfileId)
            ? device.InterfaceProfileId
            : $"{device.DisplayName}|{device.DeviceDisplayName}|{device.DeviceProfileId}|{device.ConnectionKind}";
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
