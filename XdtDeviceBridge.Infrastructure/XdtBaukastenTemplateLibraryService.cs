using System.Text.Json;
using System.Text.Json.Serialization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record XdtBaukastenTemplate(
    string Id,
    string Name,
    string? Description,
    DateTimeOffset SavedAt,
    string? SavedBy,
    string AisProfileId,
    string DeviceProfileId,
    string ExportProfileId,
    IReadOnlyList<ExportRuleDefinition> AisExportRules,
    IReadOnlyList<ExportRuleDefinition> DeviceOutputRules);

public sealed class XdtBaukastenTemplateLibraryService
{
    private const string FileExtension = ".xdtbaukasten.template.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string GetTemplateFolder(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return paths.TemplatePackagesFolder;
    }

    public IReadOnlyList<string> ListTemplateFiles(AppDataPaths paths)
    {
        var folder = GetTemplateFolder(paths);
        if (!Directory.Exists(folder))
        {
            return Array.Empty<string>();
        }

        return Directory
            .EnumerateFiles(folder, $"*{FileExtension}", SearchOption.TopDirectoryOnly)
            .OrderBy(file => Path.GetFileName(file), StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public string CreateDefaultFilePath(AppDataPaths paths, string templateName)
    {
        var folder = GetTemplateFolder(paths);
        Directory.CreateDirectory(folder);

        var fileName = TemplatePackageExportSelectionService.CreateSafeTemplatePackageFileName(templateName)
            .Replace(".templatepackage.zip", FileExtension, StringComparison.OrdinalIgnoreCase);
        return Path.Combine(folder, fileName);
    }

    public void Save(string filePath, XdtBaukastenTemplate template, bool overwriteExisting)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Template file path must not be empty.", nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(template);

        var fullPath = Path.GetFullPath(filePath);
        var folder = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!overwriteExisting && File.Exists(fullPath))
        {
            throw new InvalidOperationException($"Baukasten-Template existiert bereits und wird nicht überschrieben: {fullPath}");
        }

        var json = JsonSerializer.Serialize(template, SerializerOptions);
        File.WriteAllText(fullPath, json);
    }

    public XdtBaukastenTemplate Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Template file path must not be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var template = JsonSerializer.Deserialize<XdtBaukastenTemplate>(json, SerializerOptions);
        if (template is null)
        {
            throw new InvalidOperationException("Baukasten-Template konnte nicht gelesen werden.");
        }

        Validate(template);
        return template;
    }

    public static void Validate(XdtBaukastenTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(template.Id))
        {
            issues.Add("Template-ID fehlt.");
        }

        if (string.IsNullOrWhiteSpace(template.Name))
        {
            issues.Add("Template-Name fehlt.");
        }

        if (string.IsNullOrWhiteSpace(template.AisProfileId))
        {
            issues.Add("AIS-Profil-ID fehlt.");
        }

        if (string.IsNullOrWhiteSpace(template.DeviceProfileId))
        {
            issues.Add("Geräteprofil-ID fehlt.");
        }

        if (string.IsNullOrWhiteSpace(template.ExportProfileId))
        {
            issues.Add("Exportprofil-ID fehlt.");
        }

        if (template.AisExportRules is null)
        {
            issues.Add("AIS-Exportregeln fehlen.");
        }

        if (template.DeviceOutputRules is null)
        {
            issues.Add("Geräteausgabe-Regeln fehlen.");
        }

        if (issues.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, issues));
        }
    }
}
