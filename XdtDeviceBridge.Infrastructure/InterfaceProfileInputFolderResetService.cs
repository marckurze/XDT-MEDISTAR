using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileInputFolderResetService
{
    private readonly FolderSafetyValidator _folderSafetyValidator;
    private readonly Func<string, bool> _directoryExists;
    private readonly Func<string, IEnumerable<string>> _enumerateTopLevelFiles;
    private readonly Action<string> _deleteFile;

    public InterfaceProfileInputFolderResetService()
        : this(
            new FolderSafetyValidator(),
            Directory.Exists,
            folder => Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly),
            File.Delete)
    {
    }

    public InterfaceProfileInputFolderResetService(
        FolderSafetyValidator folderSafetyValidator,
        Func<string, bool> directoryExists,
        Func<string, IEnumerable<string>> enumerateTopLevelFiles,
        Action<string> deleteFile)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
        _directoryExists = directoryExists ?? throw new ArgumentNullException(nameof(directoryExists));
        _enumerateTopLevelFiles = enumerateTopLevelFiles ?? throw new ArgumentNullException(nameof(enumerateTopLevelFiles));
        _deleteFile = deleteFile ?? throw new ArgumentNullException(nameof(deleteFile));
    }

    public InterfaceProfileInputFolderResetResult ClearInputFolders(InterfaceProfileDefinition interfaceProfile)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);

        var messages = new List<string>();
        var processedFolders = new List<string>();
        var skippedFolders = new List<string>();
        var failedFiles = new List<string>();
        var inputFolders = GetInputFolders(interfaceProfile.FolderOptions);
        var protectedFolders = GetProtectedFolders(interfaceProfile.FolderOptions);
        var candidateFolders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var deletedFileCount = 0;
        var failedFileCount = 0;
        var missingFolderCount = 0;
        var skippedFolderCount = 0;

        foreach (var folder in inputFolders)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                continue;
            }

            var normalizedFolder = TryNormalizeFolder(folder, messages);
            if (normalizedFolder is null)
            {
                skippedFolderCount++;
                skippedFolders.Add(folder);
                continue;
            }

            if (protectedFolders.Contains(normalizedFolder))
            {
                skippedFolderCount++;
                skippedFolders.Add(normalizedFolder);
                messages.Add($"Geschützter Ordner wurde nicht geleert: {normalizedFolder}");
                continue;
            }

            candidateFolders.TryAdd(normalizedFolder, normalizedFolder);
        }

        foreach (var folder in candidateFolders.Values)
        {
            var validation = _folderSafetyValidator.ValidateFolderForCleanup(folder);
            var validationErrors = validation.Issues
                .Where(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error)
                .ToList();
            if (validationErrors.Count > 0)
            {
                skippedFolderCount++;
                skippedFolders.Add(folder);
                messages.Add($"Eingangsordner wurde nicht geleert: {validationErrors[0].Message} ({folder})");
                continue;
            }

            if (!_directoryExists(folder))
            {
                missingFolderCount++;
                messages.Add($"Eingangsordner nicht gefunden: {folder}");
                continue;
            }

            processedFolders.Add(folder);
            IReadOnlyList<string> files;
            try
            {
                files = _enumerateTopLevelFiles(folder).ToList();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                skippedFolderCount++;
                skippedFolders.Add(folder);
                messages.Add($"Eingangsordner konnte nicht gelesen werden: {folder} ({ex.Message})");
                continue;
            }

            foreach (var file in files)
            {
                try
                {
                    _deleteFile(file);
                    deletedFileCount++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
                {
                    failedFileCount++;
                    failedFiles.Add(file);
                    messages.Add($"Datei konnte nicht gelöscht werden: {file} ({ex.Message})");
                }
            }
        }

        return new InterfaceProfileInputFolderResetResult(
            CandidateFolderCount: candidateFolders.Count,
            ProcessedFolderCount: processedFolders.Count,
            DeletedFileCount: deletedFileCount,
            FailedFileCount: failedFileCount,
            MissingFolderCount: missingFolderCount,
            SkippedFolderCount: skippedFolderCount,
            ProcessedFolders: processedFolders,
            SkippedFolders: skippedFolders,
            FailedFiles: failedFiles,
            Messages: messages);
    }

    private static IReadOnlyList<string> GetInputFolders(InterfaceFolderOptions options)
    {
        return new[]
        {
            options.AisImportFolder,
            options.DeviceImportFolder,
            options.AttachmentImportFolder
        };
    }

    private static HashSet<string> GetProtectedFolders(InterfaceFolderOptions options)
    {
        var protectedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddProtectedFolder(protectedFolders, options.ExportFolder);
        AddProtectedFolder(protectedFolders, options.ArchiveFolder);
        AddProtectedFolder(protectedFolders, options.ErrorFolder);
        AddProtectedFolder(protectedFolders, options.AttachmentExportFolder);
        return protectedFolders;
    }

    private static void AddProtectedFolder(HashSet<string> protectedFolders, string folder)
    {
        var normalizedFolder = TryNormalizeFolder(folder, messages: null);
        if (normalizedFolder is not null)
        {
            protectedFolders.Add(normalizedFolder);
        }
    }

    private static string? TryNormalizeFolder(string folder, List<string>? messages)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(folder.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            messages?.Add($"Ordnerpfad ist ungültig: {folder} ({ex.Message})");
            return null;
        }
    }
}
