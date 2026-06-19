using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebDataStore
{
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

    public string? ResolvePrivateKeyPath()
    {
        var configured = _options.CurrentValue.PrivateKeyPath;
        return string.IsNullOrWhiteSpace(configured) ? null : Path.GetFullPath(configured);
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

    private LicenseManagerPaths EnsureDataFolders()
    {
        var paths = Paths;
        Directory.CreateDirectory(paths.DataFolder);
        Directory.CreateDirectory(paths.LicensesFolder);
        Directory.CreateDirectory(paths.RequestsFolder);
        Directory.CreateDirectory(paths.BackupFolder);
        return paths;
    }

    private LicenseWebRuntimeInfo CreateRuntimeInfo(LicenseManagerPaths paths)
    {
        var privateKeyPath = ResolvePrivateKeyPath();
        return new LicenseWebRuntimeInfo(
            DataRoot: paths.BaseFolder,
            LicensesFolder: paths.LicensesFolder,
            RequestsFolder: paths.RequestsFolder,
            BackupFolder: paths.BackupFolder,
            PrivateKeyConfigured: !string.IsNullOrWhiteSpace(privateKeyPath),
            PrivateKeyFileExists: !string.IsNullOrWhiteSpace(privateKeyPath) && File.Exists(privateKeyPath));
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
        return fullPath;
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
