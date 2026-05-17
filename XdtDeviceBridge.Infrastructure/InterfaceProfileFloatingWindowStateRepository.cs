using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileFloatingWindowStateRepository
{
    private const string UiFolderName = "ui";
    private const string FileName = "floating-interface-windows.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public IReadOnlyList<InterfaceProfileFloatingWindowState> Load(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var filePath = GetFilePath(paths);
        if (!File.Exists(filePath))
        {
            return Array.Empty<InterfaceProfileFloatingWindowState>();
        }

        var json = File.ReadAllText(filePath);
        var states = JsonSerializer.Deserialize<IReadOnlyList<InterfaceProfileFloatingWindowState>>(json, JsonOptions)
            ?? Array.Empty<InterfaceProfileFloatingWindowState>();
        return states
            .Where(IsPersistable)
            .OrderBy(state => state.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Save(AppDataPaths paths, IEnumerable<InterfaceProfileFloatingWindowState> states)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(states);

        var filePath = GetFilePath(paths);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var persistableStates = states
            .Where(IsPersistable)
            .OrderBy(state => state.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        File.WriteAllText(filePath, JsonSerializer.Serialize(persistableStates, JsonOptions));
    }

    private static bool IsPersistable(InterfaceProfileFloatingWindowState state)
    {
        return !string.IsNullOrWhiteSpace(state.InterfaceProfileId)
            && state.IsPositionMemoryEnabled;
    }

    private static string GetFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, UiFolderName, FileName);
    }
}
