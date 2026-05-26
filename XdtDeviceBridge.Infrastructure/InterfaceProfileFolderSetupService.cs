using System.Text.RegularExpressions;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileMainDefaultFolders(
    string AisImportFolder,
    string DeviceImportFolder,
    string ExportFolder,
    string ArchiveFolder,
    string ErrorFolder);

public sealed record InterfaceProfileAttachmentDefaultFolders(
    string AttachmentImportFolder,
    string AttachmentExportFolder);

public sealed record InterfaceProfileFolderCreationRequest(
    string Label,
    string Path);

public sealed record InterfaceProfileFolderCreationEntry(
    string Label,
    string Path,
    bool Success,
    bool AlreadyExisted,
    string? ErrorMessage);

public sealed record InterfaceProfileFolderCreationResult(
    IReadOnlyList<InterfaceProfileFolderCreationEntry> Entries)
{
    public bool Success => Entries.Count > 0 && Entries.All(entry => entry.Success);

    public bool HasCreatedOrExistingFolders => Entries.Any(entry => entry.Success);
}

public sealed class InterfaceProfileFolderSetupService
{
    public const string DefaultBaseFolder = @"C:\XDTBox";

    private static readonly Regex InvalidFolderNameCharacters = new("[^A-Za-z0-9]+", RegexOptions.Compiled);

    public string CreateDefaultFolderDeviceName(DeviceProfileDefinition? deviceProfile)
    {
        var source = FirstNonEmpty(
            deviceProfile?.Metadata.Product,
            deviceProfile?.Model,
            deviceProfile?.Metadata.Name,
            deviceProfile?.Metadata.Id);

        if (string.IsNullOrWhiteSpace(source))
        {
            return "Geraet";
        }

        var firstNamePart = source
            .Split(new[] { '/', '\\', ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? source;
        var normalized = InvalidFolderNameCharacters.Replace(firstNamePart, string.Empty).Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? "Geraet"
            : normalized;
    }

    public InterfaceProfileMainDefaultFolders CreateMainDefaultFolders(
        DeviceProfileDefinition? deviceProfile,
        string baseFolder = DefaultBaseFolder)
    {
        var deviceFolder = Path.Combine(baseFolder, CreateDefaultFolderDeviceName(deviceProfile));

        return new InterfaceProfileMainDefaultFolders(
            AisImportFolder: Path.Combine(deviceFolder, "Patient2Box"),
            DeviceImportFolder: Path.Combine(deviceFolder, "Device2Box"),
            ExportFolder: Path.Combine(deviceFolder, "Box2AIS"),
            ArchiveFolder: Path.Combine(deviceFolder, "Archiv"),
            ErrorFolder: Path.Combine(deviceFolder, "Fehler"));
    }

    public InterfaceProfileAttachmentDefaultFolders CreateAttachmentDefaultFolders(
        DeviceProfileDefinition? deviceProfile,
        string baseFolder = DefaultBaseFolder)
    {
        var deviceFolder = Path.Combine(baseFolder, CreateDefaultFolderDeviceName(deviceProfile));
        var attachmentFolder = Path.Combine(deviceFolder, "XDTAnhang");

        return new InterfaceProfileAttachmentDefaultFolders(
            AttachmentImportFolder: Path.Combine(attachmentFolder, "Device2Box"),
            AttachmentExportFolder: Path.Combine(attachmentFolder, "Attachment"));
    }

    public InterfaceProfileFolderCreationResult CreateDirectories(
        IEnumerable<InterfaceProfileFolderCreationRequest> requests)
    {
        var entries = new List<InterfaceProfileFolderCreationEntry>();

        foreach (var request in requests)
        {
            entries.Add(CreateDirectory(request));
        }

        return new InterfaceProfileFolderCreationResult(entries);
    }

    private static InterfaceProfileFolderCreationEntry CreateDirectory(InterfaceProfileFolderCreationRequest request)
    {
        var label = request.Label.Trim();
        var path = request.Path.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            return new InterfaceProfileFolderCreationEntry(
                label,
                path,
                Success: false,
                AlreadyExisted: false,
                ErrorMessage: "Pfad fehlt.");
        }

        if (ContainsInvalidPathCharacters(path))
        {
            return new InterfaceProfileFolderCreationEntry(
                label,
                path,
                Success: false,
                AlreadyExisted: false,
                ErrorMessage: "Pfad enthält ungültige Zeichen.");
        }

        try
        {
            var alreadyExisted = Directory.Exists(path);
            Directory.CreateDirectory(path);

            return new InterfaceProfileFolderCreationEntry(
                label,
                path,
                Success: true,
                AlreadyExisted: alreadyExisted,
                ErrorMessage: null);
        }
        catch (Exception ex) when (ex is ArgumentException
            or IOException
            or NotSupportedException
            or UnauthorizedAccessException
            or System.Security.SecurityException)
        {
            return new InterfaceProfileFolderCreationEntry(
                label,
                path,
                Success: false,
                AlreadyExisted: false,
                ErrorMessage: ex.Message);
        }
    }

    private static bool ContainsInvalidPathCharacters(string path)
    {
        return path.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }
}
