using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageExportSelectionService
{
    private readonly ProfileJsonSerializer _serializer;

    public TemplatePackageExportSelectionService()
        : this(new ProfileJsonSerializer())
    {
    }

    public TemplatePackageExportSelectionService(ProfileJsonSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public TemplatePackageExportSelectionResult CreateForInterfaceProfile(
        ProfileCatalog catalog,
        string interfaceProfileId,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return Fail("Bitte ein Schnittstellenprofil als Paketbasis auswählen.");
        }

        var interfaceProfile = catalog.InterfaceProfiles
            .FirstOrDefault(profile => string.Equals(profile.Metadata.Id, interfaceProfileId, StringComparison.OrdinalIgnoreCase));
        if (interfaceProfile is null)
        {
            return Fail($"Das Templatepaket kann nicht exportiert werden, weil das Schnittstellenprofil nicht gefunden wurde: {interfaceProfileId}.");
        }

        var aisProfile = catalog.AisProfiles
            .FirstOrDefault(profile => string.Equals(profile.Metadata.Id, interfaceProfile.AisProfileId, StringComparison.OrdinalIgnoreCase));
        if (aisProfile is null)
        {
            return Fail($"Das Templatepaket kann nicht exportiert werden, weil das referenzierte AIS-Profil nicht gefunden wurde: {interfaceProfile.AisProfileId}.");
        }

        var deviceProfile = catalog.DeviceProfiles
            .FirstOrDefault(profile => string.Equals(profile.Metadata.Id, interfaceProfile.DeviceProfileId, StringComparison.OrdinalIgnoreCase));
        if (deviceProfile is null)
        {
            return Fail($"Das Templatepaket kann nicht exportiert werden, weil das referenzierte Geräteprofil nicht gefunden wurde: {interfaceProfile.DeviceProfileId}.");
        }

        var exportProfile = catalog.ExportProfiles
            .FirstOrDefault(profile => string.Equals(profile.Metadata.Id, interfaceProfile.ExportProfileId, StringComparison.OrdinalIgnoreCase));
        if (exportProfile is null)
        {
            return Fail($"Das Templatepaket kann nicht exportiert werden, weil das referenzierte Exportprofil nicht gefunden wurde: {interfaceProfile.ExportProfileId}.");
        }

        var safetyIssues = FindSafetyIssues(aisProfile, deviceProfile, exportProfile, interfaceProfile).ToList();
        if (safetyIssues.Count > 0)
        {
            return new TemplatePackageExportSelectionResult(
                Success: false,
                Request: null,
                SuggestedFileName: "",
                Messages: safetyIssues,
                ErrorMessage: $"Templatepaket wurde nicht exportiert: {safetyIssues[0]}");
        }

        var slug = CreatePackageSlug(aisProfile.Metadata.Name, deviceProfile.Metadata.Name, interfaceProfile.Metadata.Name);
        var packageId = $"package-{slug}-v1";
        var packageName = $"{aisProfile.Metadata.Name} + {deviceProfile.Metadata.Name} Templatepaket V1";
        var createdBy = "XdtDeviceBridge";
        var package = new TemplatePackage(
            Metadata: new ProfileMetadata(
                Id: packageId,
                Name: packageName,
                ProfileKind: ProfileKind.TemplatePackage,
                Description: $"Selektives Templatepaket fuer {interfaceProfile.Metadata.Name}. Enthaelt Schnittstellenprofil und benoetigte Abhaengigkeiten.",
                Vendor: "XdtDeviceBridge",
                Product: $"{aisProfile.Metadata.Name}/{deviceProfile.Metadata.Name}",
                Version: "1.0.0",
                CreatedAt: createdAt,
                UpdatedAt: createdAt,
                CreatedBy: createdBy,
                IsBuiltIn: false,
                IsUserDefined: true),
            IncludedProfiles: new[]
            {
                aisProfile.Metadata,
                deviceProfile.Metadata,
                exportProfile.Metadata,
                interfaceProfile.Metadata
            },
            PackageFormatVersion: "1.0",
            CreatedAt: createdAt.UtcDateTime,
            CreatedBy: createdBy,
            Description: $"Selektives Templatepaket fuer {interfaceProfile.Metadata.Name}. Enthaelt keine automatisch ergaenzten Fremdprofile.");

        var request = new TemplatePackageExportRequest(
            Package: package,
            AisProfiles: new[] { aisProfile },
            DeviceProfiles: new[] { deviceProfile },
            ExportProfiles: new[] { exportProfile },
            InterfaceProfiles: new[] { interfaceProfile });

        return new TemplatePackageExportSelectionResult(
            Success: true,
            Request: request,
            SuggestedFileName: $"{slug}-v1.templatepackage.zip",
            Messages: new[]
            {
                $"Exportpaket enthält Schnittstellenprofil '{interfaceProfile.Metadata.Name}' und die benötigten AIS-, Geräte- und Exportprofile.",
                "Es wurden keine weiteren Profile aufgenommen."
            },
            ErrorMessage: null);
    }

    private IEnumerable<string> FindSafetyIssues(
        AisProfile aisProfile,
        DeviceProfileDefinition deviceProfile,
        ExportProfileDefinition exportProfile,
        InterfaceProfileDefinition interfaceProfile)
    {
        foreach (var issue in FindUnsafeFolderOptionIssues(interfaceProfile))
        {
            yield return issue;
        }

        var serializedProfiles = string.Join(
            Environment.NewLine,
            _serializer.SerializeAisProfile(aisProfile),
            _serializer.SerializeDeviceProfileDefinition(deviceProfile),
            _serializer.SerializeExportProfileDefinition(exportProfile),
            _serializer.SerializeInterfaceProfileDefinition(interfaceProfile));

        foreach (var marker in ForbiddenContentMarkers)
        {
            if (serializedProfiles.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                yield return $"Profilinhalt enthält einen nicht erlaubten Live-/Kundendaten-Marker: {marker}";
            }
        }
    }

    private static IEnumerable<string> FindUnsafeFolderOptionIssues(InterfaceProfileDefinition interfaceProfile)
    {
        var options = interfaceProfile.FolderOptions;
        var folders = new (string Label, string? Value)[]
        {
            ("AIS-Importordner", options.AisImportFolder),
            ("Geräte-Importordner", options.DeviceImportFolder),
            ("Exportordner", options.ExportFolder),
            ("Archivordner", options.ArchiveFolder),
            ("Fehlerordner", options.ErrorFolder),
            ("XDT-Anhang-Importordner", options.AttachmentImportFolder),
            ("XDT-Anhang-Exportordner", options.AttachmentExportFolder)
        };

        foreach (var (label, value) in folders)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (Path.IsPathFullyQualified(value) || value.StartsWith(@"\\", StringComparison.Ordinal))
            {
                yield return $"{label} enthält einen absoluten oder produktionsnahen Pfad und muss vor dem Paketexport entfernt werden.";
            }
        }
    }

    private static string CreatePackageSlug(string aisName, string deviceName, string fallbackName)
    {
        var source = $"{aisName}-{deviceName}";
        var slug = CreateSafeSlug(source);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = CreateSafeSlug(fallbackName);
        }

        return string.IsNullOrWhiteSpace(slug) ? "templatepaket" : slug;
    }

    public static string CreateSafeTemplatePackageFileName(string packageBaseName)
    {
        var slug = CreateSafeSlug(packageBaseName);
        return $"{(string.IsNullOrWhiteSpace(slug) ? "templatepaket" : slug)}.templatepackage.zip";
    }

    private static string CreateSafeSlug(string value)
    {
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static TemplatePackageExportSelectionResult Fail(string message)
    {
        return new TemplatePackageExportSelectionResult(
            Success: false,
            Request: null,
            SuggestedFileName: "",
            Messages: new[] { message },
            ErrorMessage: message);
    }

    private static readonly string[] ForbiddenContentMarkers =
    {
        @"C:\",
        @"C:\\",
        @"\\",
        "C:/",
        "C:\\MEDISTAR",
        "C:/MEDISTAR",
        "Praxis Dr.",
        "Kunde",
        "Mustermann",
        "M.Kurze",
        "MarcK",
        "11253",
        "4711"
    };
}
