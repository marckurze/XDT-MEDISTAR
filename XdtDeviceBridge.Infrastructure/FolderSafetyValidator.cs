using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class FolderSafetyValidator
{
    public FolderSafetyValidationResult ValidateFolderForCleanup(string folderPath)
    {
        var issues = new List<FolderSafetyValidationIssue>();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            AddError(issues, "Folder path must not be empty.", folderPath);
            return new FolderSafetyValidationResult(issues);
        }

        if (!Path.IsPathFullyQualified(folderPath))
        {
            AddError(issues, "Folder path must be fully qualified.", folderPath);
            return new FolderSafetyValidationResult(issues);
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(folderPath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            AddError(issues, $"Folder path is invalid: {ex.Message}", folderPath);
            return new FolderSafetyValidationResult(issues);
        }

        var normalizedPath = NormalizeForComparison(fullPath);
        var normalizedRoot = NormalizeForComparison(Path.GetPathRoot(fullPath) ?? string.Empty);

        if (string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            AddError(issues, "Folder path must not be a drive or share root.", fullPath);
        }

        AddProtectedFolderError(issues, fullPath, Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Folder path must not be the Windows system folder.");
        AddProtectedFolderError(issues, fullPath, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Folder path must not be Program Files.");
        AddProtectedFolderError(issues, fullPath, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Folder path must not be Program Files.");

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile)
            && string.Equals(normalizedPath, NormalizeForComparison(userProfile), StringComparison.OrdinalIgnoreCase))
        {
            AddError(issues, "Folder path must not be the user profile root.", fullPath);
        }

        if (!issues.Any(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error)
            && !Directory.Exists(fullPath))
        {
            issues.Add(new FolderSafetyValidationIssue(
                FolderSafetyValidationIssueSeverity.Warning,
                "Folder path does not exist.",
                fullPath));
        }

        return new FolderSafetyValidationResult(issues);
    }

    public FolderSafetyValidationResult ValidateInterfaceFolderOptions(InterfaceFolderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var issues = new List<FolderSafetyValidationIssue>();

        if (options.ClearAisImportFolderBeforeProcessing)
        {
            issues.AddRange(ValidateFolderForCleanup(options.AisImportFolder).Issues);
        }

        if (options.ClearDeviceImportFolderBeforeProcessing)
        {
            issues.AddRange(ValidateFolderForCleanup(options.DeviceImportFolder).Issues);
        }

        if (options.ClearExportFolderAfterSuccessfulTransfer)
        {
            issues.AddRange(ValidateFolderForCleanup(options.ExportFolder).Issues);
        }

        if (!string.IsNullOrWhiteSpace(options.AttachmentImportFolder))
        {
            issues.AddRange(ValidateFolderForCleanup(options.AttachmentImportFolder).Issues
                .Where(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error));
        }

        if (!string.IsNullOrWhiteSpace(options.AttachmentExportFolder))
        {
            issues.AddRange(ValidateFolderForCleanup(options.AttachmentExportFolder).Issues
                .Where(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error));
        }

        return new FolderSafetyValidationResult(issues);
    }

    private static void AddProtectedFolderError(
        List<FolderSafetyValidationIssue> issues,
        string fullPath,
        string protectedFolder,
        string message)
    {
        if (string.IsNullOrWhiteSpace(protectedFolder))
        {
            return;
        }

        if (IsSameOrChildPath(fullPath, protectedFolder))
        {
            AddError(issues, message, fullPath);
        }
    }

    private static bool IsSameOrChildPath(string path, string parentPath)
    {
        var normalizedPath = NormalizeForComparison(path);
        var normalizedParent = NormalizeForComparison(parentPath);

        return string.Equals(normalizedPath, normalizedParent, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedParent + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(
                normalizedParent + Path.AltDirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeForComparison(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static void AddError(List<FolderSafetyValidationIssue> issues, string message, string path)
    {
        issues.Add(new FolderSafetyValidationIssue(
            FolderSafetyValidationIssueSeverity.Error,
            message,
            path));
    }
}
