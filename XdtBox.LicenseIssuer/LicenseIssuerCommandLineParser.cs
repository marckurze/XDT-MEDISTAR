using System.Globalization;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseIssuer;

public sealed class LicenseIssuerCommandLineParser
{
    public LicenseIssuerCommandLineParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0 || args.Any(arg => arg is "--help" or "-h" or "/?"))
        {
            return new LicenseIssuerCommandLineParseResult(null, ShowHelp: true, Array.Empty<string>());
        }

        var errors = new List<string>();
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                errors.Add($"Unbekannter Parameter: {arg}");
                continue;
            }

            var name = arg[2..];
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add("Leerer Parametername.");
                continue;
            }

            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                errors.Add($"Parameter --{name} benoetigt einen Wert.");
                continue;
            }

            values[name] = args[++index];
        }

        var requestFile = GetOptional(values, "request");
        var installationId = GetOptional(values, "installation-id");
        if (!string.IsNullOrWhiteSpace(requestFile) && !string.IsNullOrWhiteSpace(installationId))
        {
            errors.Add("--installation-id darf nicht zusammen mit --request verwendet werden. Die InstallationId kommt dann aus der Lizenzanforderung.");
        }

        var licensee = Require(values, "licensee", errors);
        var keyId = Require(values, "key-id", errors);
        var privateKey = Require(values, "private-key", errors);
        var outputFile = Require(values, "out", errors);

        if (string.IsNullOrWhiteSpace(requestFile) && string.IsNullOrWhiteSpace(installationId))
        {
            errors.Add("Entweder --request oder --installation-id muss angegeben werden.");
        }

        var maxActiveDeviceConnections = ParseInt(
            Require(values, "max-active-device-connections", errors),
            "max-active-device-connections",
            errors);
        if (maxActiveDeviceConnections <= 0)
        {
            errors.Add("--max-active-device-connections muss groesser als 0 sein.");
        }

        var validFromUtc = ParseUtcDate(
            Require(values, "valid-from", errors),
            "valid-from",
            errors);
        var validUntilUtc = ParseUtcDate(
            Require(values, "valid-until", errors),
            "valid-until",
            errors);

        var graceDays = ParseOptionalInt(values, "grace-days", XdtBoxLicenseConstants.DefaultGraceDays, errors);
        if (graceDays < 0)
        {
            errors.Add("--grace-days darf nicht negativ sein.");
        }

        var productCode = GetOptional(values, "product-code") ?? XdtBoxLicenseConstants.ProductCode;
        var issuer = GetOptional(values, "issuer") ?? XdtBoxLicenseConstants.DefaultIssuer;
        var licenseType = GetOptional(values, "license-type") ?? "Production";
        var customerNumber = GetOptional(values, "customer-number");
        var notes = GetOptional(values, "notes");

        if (!string.Equals(productCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
        {
            errors.Add($"--product-code muss {XdtBoxLicenseConstants.ProductCode} sein.");
        }

        if (validFromUtc != default && validUntilUtc != default && validUntilUtc < validFromUtc)
        {
            errors.Add("--valid-until darf nicht vor --valid-from liegen.");
        }

        if (errors.Count > 0)
        {
            return new LicenseIssuerCommandLineParseResult(null, ShowHelp: false, errors);
        }

        var options = new LicenseIssuerOptions(
            RequestFile: requestFile,
            InstallationId: installationId,
            LicenseeName: licensee,
            CustomerNumber: customerNumber,
            MaxActiveDeviceConnections: maxActiveDeviceConnections,
            ValidFromUtc: validFromUtc,
            ValidUntilUtc: validUntilUtc,
            GraceDays: graceDays,
            LicenseType: licenseType,
            Issuer: issuer,
            ProductCode: productCode,
            Notes: notes,
            KeyId: keyId,
            PrivateKeyPath: privateKey,
            OutputFile: outputFile);

        return new LicenseIssuerCommandLineParseResult(options, ShowHelp: false, Array.Empty<string>());
    }

    private static string Require(IReadOnlyDictionary<string, string> values, string name, List<string> errors)
    {
        if (values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        errors.Add($"--{name} ist erforderlich.");
        return string.Empty;
    }

    private static string? GetOptional(IReadOnlyDictionary<string, string> values, string name)
    {
        return values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private static int ParseInt(string value, string name, List<string> errors)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"--{name} muss eine ganze Zahl sein.");
        }

        return 0;
    }

    private static int ParseOptionalInt(
        IReadOnlyDictionary<string, string> values,
        string name,
        int defaultValue,
        List<string> errors)
    {
        return values.TryGetValue(name, out var value)
            ? ParseInt(value, name, errors)
            : defaultValue;
    }

    private static DateTime ParseUtcDate(string value, string name, List<string> errors)
    {
        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed.Kind == DateTimeKind.Utc
                ? parsed
                : DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"--{name} muss ein gueltiges Datum sein, z. B. 2026-05-27.");
        }

        return default;
    }
}
