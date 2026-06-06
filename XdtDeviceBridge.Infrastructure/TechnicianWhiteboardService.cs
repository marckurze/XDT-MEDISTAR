using System.Text;
using System.Text.Json;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TechnicianWhiteboardService
{
    public const string FolderName = "technician-notes";
    public const string ImagesFolderName = "images";
    public const string StateFileName = "whiteboard-state.json";

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public string GetWhiteboardFolder(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return Path.Combine(paths.BaseFolder, FolderName);
    }

    public string GetImagesFolder(AppDataPaths paths)
    {
        return Path.Combine(GetWhiteboardFolder(paths), ImagesFolderName);
    }

    public string GetStateFilePath(AppDataPaths paths)
    {
        return Path.Combine(GetWhiteboardFolder(paths), StateFileName);
    }

    public TechnicianWhiteboardState LoadOrEmpty(AppDataPaths paths)
    {
        var filePath = GetStateFilePath(paths);
        if (!File.Exists(filePath))
        {
            return TechnicianWhiteboardState.Empty;
        }

        var json = File.ReadAllText(filePath, Utf8NoBom);
        return JsonSerializer.Deserialize<TechnicianWhiteboardState>(json, Options)
            ?? TechnicianWhiteboardState.Empty;
    }

    public void Save(AppDataPaths paths, TechnicianWhiteboardState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var filePath = GetStateFilePath(paths);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? GetWhiteboardFolder(paths));
        File.WriteAllText(filePath, JsonSerializer.Serialize(state, Options), Utf8NoBom);
    }

    public string CopyImageIntoWhiteboard(AppDataPaths paths, string sourceImagePath)
    {
        if (string.IsNullOrWhiteSpace(sourceImagePath))
        {
            throw new ArgumentException("Image path must not be empty.", nameof(sourceImagePath));
        }

        if (!File.Exists(sourceImagePath))
        {
            throw new FileNotFoundException("Image file not found.", sourceImagePath);
        }

        var extension = Path.GetExtension(sourceImagePath);
        if (!IsSupportedImageExtension(extension))
        {
            throw new InvalidOperationException("Unterstützt werden PNG, JPG und JPEG.");
        }

        var imagesFolder = GetImagesFolder(paths);
        Directory.CreateDirectory(imagesFolder);
        var targetPath = Path.Combine(imagesFolder, $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        File.Copy(sourceImagePath, targetPath, overwrite: false);
        return targetPath;
    }

    private static bool IsSupportedImageExtension(string extension)
    {
        return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase);
    }
}
