using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InstallationInfoProvider
{
    private const string FileName = "installation.json";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public InstallationInfo GetOrCreate(string storageFolder)
    {
        if (string.IsNullOrWhiteSpace(storageFolder))
        {
            throw new ArgumentException("Storage folder must not be empty.", nameof(storageFolder));
        }

        Directory.CreateDirectory(storageFolder);

        var filePath = Path.Combine(storageFolder, FileName);
        if (File.Exists(filePath))
        {
            return Load(filePath);
        }

        var installationInfo = new InstallationInfo(
            InstallationId: Guid.NewGuid().ToString(),
            MachineName: Environment.MachineName,
            UserName: Environment.UserName,
            IsTerminalServer: false,
            CreatedAt: DateTime.UtcNow);

        Save(filePath, installationInfo);

        return installationInfo;
    }

    private static InstallationInfo Load(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<InstallationInfo>(json, Options)
                ?? throw new InvalidOperationException("Installation info file did not contain valid installation data.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid installation info JSON: {ex.Message}", ex);
        }
    }

    private static void Save(string filePath, InstallationInfo installationInfo)
    {
        var json = JsonSerializer.Serialize(installationInfo, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }
}
