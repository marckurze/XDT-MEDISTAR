using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebDataStore
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string[] PublicPathSegments =
    [
        "wwwroot",
        "htdocs",
        "public_html",
        "assets",
        "download",
        "downloads"
    ];

    private readonly IWebHostEnvironment _environment;
    private readonly IOptionsMonitor<LicenseWebOptions> _options;
    private readonly LicenseManagerPathProvider _pathProvider = new();
    private readonly LicenseManagerCustomerRepository _customerRepository = new();
    private readonly IssuedLicenseHistoryRepository _historyRepository = new();
    private readonly LicenseManagerSettingsRepository _settingsRepository = new();
    private readonly LicenseManagerBackupService _backupService = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public LicenseWebDataStore(IWebHostEnvironment environment, IOptionsMonitor<LicenseWebOptions> options)
    {
        _environment = environment;
        _options = options;
    }

    public LicenseManagerPaths Paths => _pathProvider.GetPaths(ResolveDataRoot());

    public async Task<LicenseWebSnapshot> LoadSnapshotAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var settings = CreateEffectiveSettings(_settingsRepository.LoadOrDefault(paths.SettingsFile, paths.BaseFolder));
            return new LicenseWebSnapshot(
                _customerRepository.LoadOrEmpty(paths.CustomersFile),
                _historyRepository.LoadOrEmpty(paths.HistoryFile),
                settings,
                CreateRuntimeInfo(paths));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveSettingsAsync(LicenseManagerSettings settings)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            _settingsRepository.Save(paths.SettingsFile, settings);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseManagerCustomerRecord> UpdateCustomerAsync(string customerId, LicenseWebCustomerUpdate update)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentNullException.ThrowIfNull(update);

        if (string.IsNullOrWhiteSpace(update.CustomerName))
        {
            throw new InvalidOperationException("Bitte einen Kundennamen angeben.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var customers = _customerRepository.LoadOrEmpty(paths.CustomersFile).ToList();
            var index = customers.FindIndex(customer => string.Equals(customer.Id, customerId, StringComparison.Ordinal));
            if (index < 0)
            {
                throw new InvalidOperationException("Kunde wurde nicht gefunden.");
            }

            var existing = customers[index];
            var originalInstallationIds = existing.EffectiveInstallations
                .Select(installation => installation.InstallationId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var originalCustomerNumber = existing.CustomerNumber;
            var locationsByKey = update.DeviceLocations
                .Where(item => !string.IsNullOrWhiteSpace(item.Key))
                .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => Normalize(group.Last().Location),
                    StringComparer.OrdinalIgnoreCase);
            var installations = existing.EffectiveInstallations
                .Select(installation => installation with
                {
                    Devices = installation.Devices
                        .Select(device =>
                        {
                            var key = CreateDeviceLocationKey(installation.InstallationId, device);
                            return locationsByKey.TryGetValue(key, out var location)
                                ? device with { Location = location }
                                : device;
                        })
                        .ToArray()
                })
                .ToArray();
            var sepa = update.PaymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit;
            var edited = (existing with
            {
                CustomerNumber = Normalize(update.CustomerNumber),
                CustomerName = update.CustomerName.Trim(),
                Street = update.Street.Trim(),
                PostalCode = update.PostalCode.Trim(),
                City = update.City.Trim(),
                Phone = update.Phone.Trim(),
                Email = Normalize(update.Email),
                ContactPerson = Normalize(update.ContactPerson),
                InvoiceEmail = Normalize(update.InvoiceEmail),
                Iban = Normalize(update.Iban),
                Bic = Normalize(update.Bic),
                AccountHolder = Normalize(update.AccountHolder),
                SepaDirectDebitConsent = sepa,
                AlwaysInvoice = !sepa,
                Installations = installations,
                UpdatedAtUtc = DateTime.UtcNow
            }).WithNormalizedInstallations();

            customers[index] = edited;
            _customerRepository.Save(paths.CustomersFile, customers);

            var history = _historyRepository.LoadOrEmpty(paths.HistoryFile)
                .Select(record => MatchesCustomerHistory(record, originalInstallationIds, originalCustomerNumber)
                    ? UpdateHistoryRecord(record, edited, locationsByKey)
                    : record)
                .ToArray();
            _historyRepository.Save(paths.HistoryFile, history);
            return edited;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseManagerCustomerRecord> DeleteCustomerAsync(string customerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var customers = _customerRepository.LoadOrEmpty(paths.CustomersFile).ToList();
            var index = customers.FindIndex(customer => string.Equals(customer.Id, customerId, StringComparison.Ordinal));
            if (index < 0)
            {
                throw new InvalidOperationException("Kunde wurde nicht gefunden.");
            }

            var customer = customers[index];
            var installationIds = customer.EffectiveInstallations
                .Select(installation => installation.InstallationId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            customers.RemoveAt(index);
            _customerRepository.Save(paths.CustomersFile, customers);

            var history = _historyRepository.LoadOrEmpty(paths.HistoryFile)
                .Where(record => !MatchesCustomerHistory(record, installationIds, customer.CustomerNumber))
                .ToArray();
            _historyRepository.Save(paths.HistoryFile, history);
            return customer;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseManagerCustomerMergeResult> MergeCustomersAsync(IReadOnlyCollection<string> selectedCustomerIds)
    {
        ArgumentNullException.ThrowIfNull(selectedCustomerIds);

        var selectedIds = selectedCustomerIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (selectedIds.Length < 2)
        {
            throw new InvalidOperationException("Bitte mindestens zwei Kunden markieren.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var customers = _customerRepository.LoadOrEmpty(paths.CustomersFile);
            var target = selectedIds
                .Select(id => customers.FirstOrDefault(customer => string.Equals(customer.Id, id, StringComparison.Ordinal)))
                .FirstOrDefault(customer => customer is not null)
                ?? throw new InvalidOperationException("Der Zielkunde wurde nicht gefunden.");
            var selection = new LicenseManagerCustomerMergeSelection(
                TargetCustomerId: target.Id,
                CustomerNumber: target.CustomerNumber,
                CustomerName: target.CustomerName,
                ContactPerson: target.ContactPerson,
                Street: target.Street,
                PostalCode: target.PostalCode,
                City: target.City,
                Phone: target.Phone,
                Email: target.Email,
                InvoiceEmail: target.InvoiceEmail,
                PaymentMethod: target.PaymentMethod,
                Iban: target.Iban,
                Bic: target.Bic,
                AccountHolder: target.AccountHolder);
            return _customerRepository.MergeCustomers(paths.CustomersFile, selectedIds, selection);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseWebImportResult> ImportRequestAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Die hochgeladene Lizenzanfrage ist leer.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var requestFile = Path.Combine(paths.RequestsFolder, CreateSafeRequestFileName(file.FileName));
            await using (var stream = File.Create(requestFile))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            var request = new LicenseRequestFileRepository().Load(requestFile);
            var issues = request.Validate();
            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Lizenzanfrage ist ungueltig: " + string.Join("; ", issues));
            }

            var incoming = LicenseManagerCustomerRecord.FromRequest(request);
            var existing = FindMatchingCustomer(_customerRepository.LoadOrEmpty(paths.CustomersFile), incoming);
            var customers = !string.IsNullOrWhiteSpace(incoming.CustomerNumber)
                ? _customerRepository.Upsert(paths.CustomersFile, incoming)
                : _customerRepository.UpsertByInstallation(paths.CustomersFile, incoming);
            var updated = FindMatchingCustomer(customers, incoming) ?? incoming;
            var messages = new List<string>
            {
                existing is null ? "Neuer Kunde wurde aus der Lizenzanfrage angelegt." : "Lizenzanfrage wurde einem bestehenden Kunden zugeordnet.",
                $"{incoming.BillableDeviceCount} aktive lizenzpflichtige Geraeteanbindung(en) wurden uebernommen."
            };

            return new LicenseWebImportResult(request, requestFile, updated, existing is not null, messages);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<IssuedLicenseRecord>> AddHistoryAsync(IssuedLicenseRecord record, LicenseManagerCustomerRecord customer)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var history = _historyRepository.Add(paths.HistoryFile, record);
            _ = string.IsNullOrWhiteSpace(customer.CustomerNumber)
                ? _customerRepository.UpsertLicenseByInstallation(paths.CustomersFile, customer, record)
                : _customerRepository.UpsertLicense(paths.CustomersFile, customer, record);
            return history;
        }
        finally
        {
            _gate.Release();
        }
    }

    public LicenseWebAdminCredential? LoadAdminCredentialOverride()
    {
        try
        {
            var filePath = GetAdminCredentialsFile();
            if (!File.Exists(filePath))
            {
                return null;
            }

            var credential = JsonSerializer.Deserialize<LicenseWebAdminCredential>(
                File.ReadAllText(filePath, Utf8NoBom),
                JsonOptions);
            return IsValidAdminCredential(credential) ? credential : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveAdminCredentialOverrideAsync(LicenseWebAdminCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);
        if (!IsValidAdminCredential(credential))
        {
            throw new InvalidOperationException("Admin-Zugang ist unvollstaendig.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var filePath = GetAdminCredentialsFile();
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(
                filePath,
                JsonSerializer.Serialize(credential, JsonOptions),
                Utf8NoBom).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<byte[]> CreateBackupAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var filePath = Path.Combine(paths.BackupFolder, $"xdtbox-licenseweb-backup-{DateTime.Today:yyyyMMdd-HHmmss}.xdtbox-licensemanager-backup");
            _backupService.CreateBackup(
                filePath,
                _customerRepository.LoadOrEmpty(paths.CustomersFile),
                _settingsRepository.LoadOrDefault(paths.SettingsFile, paths.BaseFolder),
                _historyRepository.LoadOrEmpty(paths.HistoryFile));
            return await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RestoreBackupAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Die hochgeladene Sicherung ist leer.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var restoreFile = Path.Combine(paths.BackupFolder, "restore-upload.xdtbox-licensemanager-backup");
            await using (var stream = File.Create(restoreFile))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            _backupService.RestoreBackup(restoreFile, paths);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseWebCustomerMigrationResult> ImportSingleCustomerFromBackupAsync(
        IFormFile file,
        string customerNumber)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Die hochgeladene Sicherung ist leer.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var paths = EnsureDataFolders();
            var importFile = Path.Combine(paths.BackupFolder, CreateSafeRequestFileName(file.FileName));
            await using (var stream = File.Create(importFile))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
            }

            var backup = _backupService.ReadBackup(importFile);
            return ImportSingleCustomerCore(paths, backup.Customers, backup.History, customerNumber);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<LicenseWebCustomerMigrationResult> ImportSingleCustomerFromDataRootAsync(
        string sourceDataRoot,
        string customerNumber)
    {
        if (string.IsNullOrWhiteSpace(sourceDataRoot))
        {
            throw new InvalidOperationException("Bitte einen Quell-Datenroot angeben.");
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var targetPaths = EnsureDataFolders();
            var sourcePaths = _pathProvider.GetPaths(Path.GetFullPath(sourceDataRoot));
            if (string.Equals(
                    NormalizeFolder(sourcePaths.BaseFolder),
                    NormalizeFolder(targetPaths.BaseFolder),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Quell- und Ziel-Datenroot duerfen nicht identisch sein.");
            }

            var sourceCustomers = _customerRepository.LoadOrEmpty(sourcePaths.CustomersFile);
            var sourceHistory = _historyRepository.LoadOrEmpty(sourcePaths.HistoryFile);
            return ImportSingleCustomerCore(targetPaths, sourceCustomers, sourceHistory, customerNumber);
        }
        finally
        {
            _gate.Release();
        }
    }

    public string GetPrivateKeyPathForSigning()
    {
        var configured = _options.CurrentValue.PrivateKeyPath;
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("Lizenz-Erstellung ist deaktiviert, weil kein serverseitiger Private Key konfiguriert ist.");
        }

        var privateKeyPath = Path.GetFullPath(configured);
        if (!IsServerSideSecretPathSafe(privateKeyPath))
        {
            throw new InvalidOperationException("Lizenz-Erstellung ist deaktiviert, weil der Private-Key-Pfad unsicher konfiguriert ist.");
        }

        if (!File.Exists(privateKeyPath) || !CanReadFile(privateKeyPath))
        {
            throw new InvalidOperationException("Lizenz-Erstellung ist deaktiviert, weil der serverseitige Private Key nicht lesbar ist.");
        }

        return privateKeyPath;
    }

    public async Task<IReadOnlyList<LicenseWebDiagnosticItem>> CreateDiagnosticsAsync(bool isHttps, bool adminConfigured)
    {
        var snapshot = await LoadSnapshotAsync().ConfigureAwait(false);
        var runtime = snapshot.Runtime;
        return new[]
        {
            new LicenseWebDiagnosticItem(
                "Umgebung",
                true,
                runtime.EnvironmentLabel,
                string.IsNullOrWhiteSpace(runtime.PublicBaseUrl) ? "Keine oeffentliche Basis-URL konfiguriert." : "Oeffentliche Basis-URL ist konfiguriert."),
            new LicenseWebDiagnosticItem(
                "DataRoot",
                runtime.DataRootSafe && runtime.DataRootReadable && runtime.DataRootWritable,
                runtime.DataRootSafe ? "sicher" : "unsicher",
                runtime.DataRootConfigured ? "DataRoot ist explizit konfiguriert." : "DataRoot nutzt den lokalen Standardpfad."),
            new LicenseWebDiagnosticItem(
                "DataRoot Zugriff",
                runtime.DataRootReadable && runtime.DataRootWritable,
                runtime.DataRootReadable && runtime.DataRootWritable ? "lesbar/schreibbar" : "nicht vollstaendig nutzbar",
                "Kunden, Historie, Requests, Lizenzen und Backups werden serverseitig gespeichert."),
            new LicenseWebDiagnosticItem(
                "Private Key",
                runtime.LicenseSignatureAvailable,
                runtime.PrivateKeyStatus,
                "Der Private Key wird nur serverseitig gelesen und nicht in Backups geschrieben."),
            new LicenseWebDiagnosticItem(
                "Private-Key-Pfad",
                !runtime.PrivateKeyConfigured || runtime.PrivateKeyPathSafe,
                runtime.PrivateKeyPathSafe ? "sicher" : "unsicher",
                "Der Pfad darf nicht in Webroot, Publish-Verzeichnis oder oeffentlichen Download-/Asset-Ordnern liegen."),
            new LicenseWebDiagnosticItem(
                "Backup",
                runtime.BackupAvailable,
                runtime.BackupAvailable ? "verfuegbar" : "nicht verfuegbar",
                "Backups enthalten Kunden, Einstellungen und Historie, aber keine Private Keys."),
            new LicenseWebDiagnosticItem(
                "Admin-Zugang",
                adminConfigured,
                adminConfigured ? "konfiguriert" : "nicht konfiguriert",
                "Produktivstart ist nur mit konfiguriertem Hash-Admin erlaubt."),
            new LicenseWebDiagnosticItem(
                "HTTPS",
                isHttps,
                isHttps ? "aktiv" : "nicht erkannt",
                "Bei Reverse Proxy/IIS muss X-Forwarded-Proto korrekt durchgereicht werden.")
        };
    }

    private LicenseManagerSettings CreateEffectiveSettings(LicenseManagerSettings settings)
    {
        var options = _options.CurrentValue;
        return settings with
        {
            PrivateKeyPath = null,
            KeyId = string.IsNullOrWhiteSpace(options.KeyId) ? settings.KeyId : options.KeyId,
            DefaultIssuer = string.IsNullOrWhiteSpace(options.Issuer) ? settings.DefaultIssuer : options.Issuer,
            DefaultGraceDays = options.GraceDays < 0 ? settings.DefaultGraceDays : options.GraceDays
        };
    }

    public static string CreateDeviceLocationKey(string installationId, IssuedLicenseDeviceRecord device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return string.Join(
            "|",
            installationId,
            device.InterfaceProfileId,
            device.DeviceProfileId,
            device.DisplayName,
            device.DeviceDisplayName,
            device.ConnectionKind);
    }

    private LicenseManagerPaths EnsureDataFolders()
    {
        var paths = Paths;
        Directory.CreateDirectory(paths.DataFolder);
        Directory.CreateDirectory(paths.LicensesFolder);
        Directory.CreateDirectory(paths.RequestsFolder);
        Directory.CreateDirectory(paths.BackupFolder);
        return paths;
    }

    private string GetAdminCredentialsFile()
    {
        var paths = EnsureDataFolders();
        return Path.Combine(paths.DataFolder, "license-web-admin.json");
    }

    private LicenseWebRuntimeInfo CreateRuntimeInfo(LicenseManagerPaths paths)
    {
        var options = _options.CurrentValue;
        var privateKeyConfigured = !string.IsNullOrWhiteSpace(options.PrivateKeyPath);
        var privateKeyPath = privateKeyConfigured ? Path.GetFullPath(options.PrivateKeyPath!) : null;
        var privateKeyPathSafe = string.IsNullOrWhiteSpace(privateKeyPath) || IsServerSideSecretPathSafe(privateKeyPath);
        var privateKeyFileExists = privateKeyPathSafe && !string.IsNullOrWhiteSpace(privateKeyPath) && File.Exists(privateKeyPath);
        var privateKeyReadable = privateKeyFileExists && CanReadFile(privateKeyPath!);
        var licenseSignatureAvailable = privateKeyConfigured && privateKeyPathSafe && privateKeyFileExists && privateKeyReadable;
        var privateKeyStatus = !privateKeyConfigured
            ? "nicht konfiguriert"
            : !privateKeyPathSafe
                ? "unsicherer Pfad"
                : privateKeyReadable
                    ? "konfiguriert"
                    : privateKeyFileExists ? "nicht lesbar" : "Datei nicht gefunden";

        return new LicenseWebRuntimeInfo(
            DataRoot: paths.BaseFolder,
            LicensesFolder: paths.LicensesFolder,
            RequestsFolder: paths.RequestsFolder,
            BackupFolder: paths.BackupFolder,
            EnvironmentLabel: string.IsNullOrWhiteSpace(options.EnvironmentLabel) ? _environment.EnvironmentName : options.EnvironmentLabel,
            PublicBaseUrl: string.IsNullOrWhiteSpace(options.PublicBaseUrl) ? null : options.PublicBaseUrl.Trim(),
            DataRootConfigured: !string.IsNullOrWhiteSpace(options.DataRoot),
            DataRootSafe: IsDataRootSafe(paths.BaseFolder),
            DataRootReadable: CanReadDirectory(paths.BaseFolder),
            DataRootWritable: CanWriteDirectory(paths.BaseFolder),
            PrivateKeyConfigured: privateKeyConfigured,
            PrivateKeyPathSafe: privateKeyPathSafe,
            PrivateKeyFileExists: privateKeyFileExists,
            PrivateKeyReadable: privateKeyReadable,
            PrivateKeyStatus: privateKeyStatus,
            LicenseSignatureAvailable: licenseSignatureAvailable,
            BackupAvailable: Directory.Exists(paths.BackupFolder) && CanWriteDirectory(paths.BackupFolder));
    }

    private string ResolveDataRoot()
    {
        var configured = _options.CurrentValue.DataRoot;
        var dataRoot = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XDTBox", "LicenseWeb")
            : configured;
        var fullPath = Path.GetFullPath(dataRoot);
        EnsureOutside(fullPath, Path.Combine(_environment.ContentRootPath, "wwwroot"), "wwwroot");
        EnsureOutside(fullPath, _environment.ContentRootPath, "Projektverzeichnis");
        EnsureNoPublicSegment(fullPath, "DataRoot");
        return fullPath;
    }

    private bool IsDataRootSafe(string path)
    {
        return !IsSubPath(path, Path.Combine(_environment.ContentRootPath, "wwwroot"))
            && !IsSubPath(path, _environment.ContentRootPath)
            && !HasPublicPathSegment(path);
    }

    private bool IsServerSideSecretPathSafe(string path)
    {
        return !IsSubPath(path, Path.Combine(_environment.ContentRootPath, "wwwroot"))
            && !IsSubPath(path, _environment.ContentRootPath)
            && !HasPublicPathSegment(path);
    }

    private static void EnsureOutside(string path, string restrictedRoot, string label)
    {
        var fullRoot = Path.GetFullPath(restrictedRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Der LicenseWeb DataRoot darf nicht im {label} liegen.");
        }
    }

    private static void EnsureNoPublicSegment(string path, string label)
    {
        if (HasPublicPathSegment(path))
        {
            throw new InvalidOperationException($"Der LicenseWeb {label} darf nicht in oeffentlichen Download- oder Asset-Ordnern liegen.");
        }
    }

    private static bool IsSubPath(string path, string root)
    {
        var fullRoot = NormalizeFolder(root) + Path.DirectorySeparatorChar;
        var fullPath = NormalizeFolder(path) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeFolder(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool HasPublicPathSegment(string path)
    {
        var segments = Path.GetFullPath(path)
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment => PublicPathSegments.Contains(segment, StringComparer.OrdinalIgnoreCase));
    }

    private LicenseWebCustomerMigrationResult ImportSingleCustomerCore(
        LicenseManagerPaths targetPaths,
        IReadOnlyList<LicenseManagerCustomerRecord> sourceCustomers,
        IReadOnlyList<IssuedLicenseRecord> sourceHistory,
        string customerNumber)
    {
        var normalizedNumber = NormalizeCustomerNumber(customerNumber);
        var matches = sourceCustomers
            .Where(customer => string.Equals(customer.CustomerNumber, normalizedNumber, StringComparison.OrdinalIgnoreCase))
            .Select(customer => customer.WithNormalizedInstallations())
            .ToArray();
        if (matches.Length == 0)
        {
            throw new InvalidOperationException($"Kundennummer {normalizedNumber} wurde in der Quelle nicht gefunden.");
        }

        if (matches.Length > 1)
        {
            throw new InvalidOperationException($"Kundennummer {normalizedNumber} ist in der Quelle mehrfach vorhanden. Bitte zuerst im Lizenzmanager bereinigen.");
        }

        var incoming = matches[0];
        var installationIds = incoming.EffectiveInstallations
            .Select(installation => installation.InstallationId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var relevantHistory = sourceHistory
            .Where(record =>
                string.Equals(record.CustomerNumber, normalizedNumber, StringComparison.OrdinalIgnoreCase)
                || installationIds.Contains(record.InstallationId))
            .ToArray();

        var targetCustomers = _customerRepository.LoadOrEmpty(targetPaths.CustomersFile).ToList();
        var targetIndex = targetCustomers.FindIndex(customer =>
            string.Equals(customer.CustomerNumber, normalizedNumber, StringComparison.OrdinalIgnoreCase));
        var existingCustomerMatched = targetIndex >= 0;
        var merged = existingCustomerMatched
            ? targetCustomers[targetIndex].MergeImported(incoming)
            : incoming;
        if (existingCustomerMatched)
        {
            targetCustomers[targetIndex] = merged;
        }
        else
        {
            targetCustomers.Add(merged);
        }

        _customerRepository.Save(targetPaths.CustomersFile, targetCustomers);

        var targetHistory = _historyRepository.LoadOrEmpty(targetPaths.HistoryFile).ToList();
        var importedHistoryCount = 0;
        foreach (var record in relevantHistory)
        {
            if (targetHistory.Any(existing => IsSameHistoryEntry(existing, record)))
            {
                continue;
            }

            targetHistory.Add(record);
            importedHistoryCount++;
        }

        _historyRepository.Save(targetPaths.HistoryFile, targetHistory);

        var messages = new List<string>
        {
            existingCustomerMatched
                ? $"Kunde {normalizedNumber} wurde mit vorhandenen Webdaten zusammengefuehrt."
                : $"Kunde {normalizedNumber} wurde in den Web-Lizenzmanager uebernommen.",
            $"{importedHistoryCount} neue Historieneintraege wurden uebernommen.",
            "Installationen, Geraete und Standorte bleiben Bestandteil des Kundendatensatzes."
        };

        return new LicenseWebCustomerMigrationResult(
            merged,
            importedHistoryCount,
            merged.ActiveInstallationCount,
            merged.BillableDeviceCount,
            messages);
    }

    private static bool IsSameHistoryEntry(IssuedLicenseRecord left, IssuedLicenseRecord right)
    {
        return string.Equals(left.LicenseId, right.LicenseId, StringComparison.Ordinal)
            && left.IssuedAtUtc == right.IssuedAtUtc
            && string.Equals(left.InstallationId, right.InstallationId, StringComparison.Ordinal);
    }

    private static IssuedLicenseRecord UpdateHistoryRecord(
        IssuedLicenseRecord record,
        LicenseManagerCustomerRecord customer,
        IReadOnlyDictionary<string, string?> locationsByKey)
    {
        var devices = record.Devices
            .Select(device =>
            {
                var key = CreateDeviceLocationKey(record.InstallationId, device);
                return locationsByKey.TryGetValue(key, out var location)
                    ? device with { Location = location }
                    : device;
            })
            .ToArray();
        return record with
        {
            LicenseeName = customer.CustomerName,
            CustomerNumber = customer.CustomerNumber,
            CustomerName = customer.CustomerName,
            Street = customer.Street,
            PostalCode = customer.PostalCode,
            City = customer.City,
            Phone = customer.Phone,
            Email = customer.Email,
            ContactPerson = customer.ContactPerson,
            InvoiceEmail = customer.InvoiceEmail,
            Iban = customer.Iban,
            Bic = customer.Bic,
            AccountHolder = customer.AccountHolder,
            SepaDirectDebitConsent = customer.SepaDirectDebitConsent,
            AlwaysInvoice = customer.AlwaysInvoice,
            Devices = devices
        };
    }

    private static bool MatchesCustomerHistory(
        IssuedLicenseRecord record,
        IReadOnlySet<string> installationIds,
        string? customerNumber)
    {
        return installationIds.Contains(record.InstallationId)
            || (!string.IsNullOrWhiteSpace(customerNumber)
                && string.Equals(record.CustomerNumber, customerNumber, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidAdminCredential(LicenseWebAdminCredential? credential)
    {
        return credential is not null
            && !string.IsNullOrWhiteSpace(credential.Username)
            && !string.IsNullOrWhiteSpace(credential.PasswordHash)
            && !string.IsNullOrWhiteSpace(credential.PasswordSalt);
    }

    private static string NormalizeCustomerNumber(string customerNumber)
    {
        if (string.IsNullOrWhiteSpace(customerNumber))
        {
            throw new InvalidOperationException("Bitte eine Kundennummer angeben.");
        }

        return customerNumber.Trim();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool CanReadDirectory(string path)
    {
        try
        {
            _ = Directory.Exists(path);
            return Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static bool CanWriteDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            var probe = Path.Combine(path, $".xdtbox-write-test-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool CanReadFile(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.CanRead;
        }
        catch
        {
            return false;
        }
    }

    private static LicenseManagerCustomerRecord? FindMatchingCustomer(
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        LicenseManagerCustomerRecord incoming)
    {
        return customers.FirstOrDefault(customer =>
            (!string.IsNullOrWhiteSpace(incoming.CustomerNumber)
                && string.Equals(customer.CustomerNumber, incoming.CustomerNumber, StringComparison.OrdinalIgnoreCase))
            || customer.EffectiveInstallations.Any(installation =>
                string.Equals(installation.InstallationId, incoming.InstallationId, StringComparison.OrdinalIgnoreCase)));
    }

    private static string CreateSafeRequestFileName(string fileName)
    {
        var safeName = string.Join("_", (Path.GetFileName(fileName) ?? "license-request.json")
            .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{(string.IsNullOrWhiteSpace(safeName) ? "license-request.json" : safeName)}";
    }
}
