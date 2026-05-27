namespace XdtBox.LicenseIssuer;

public static class LicenseIssuerConsole
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        var parser = new LicenseIssuerCommandLineParser();
        var parseResult = parser.Parse(args);

        if (parseResult.ShowHelp)
        {
            WriteHelp(output);
            return 0;
        }

        if (parseResult.Errors.Count > 0 || parseResult.Options is null)
        {
            foreach (var parseError in parseResult.Errors)
            {
                error.WriteLine(parseError);
            }

            error.WriteLine();
            WriteHelp(error);
            return 2;
        }

        try
        {
            var result = new LicenseIssuerService().CreateLicense(parseResult.Options);
            output.WriteLine("Lizenz erfolgreich erzeugt.");
            output.WriteLine($"LicenseId: {result.Payload.LicenseId}");
            output.WriteLine($"LicenseeName: {result.Payload.LicenseeName}");
            output.WriteLine($"InstallationId: {result.Payload.InstallationId}");
            output.WriteLine($"MaxActiveDeviceConnections: {result.Payload.MaxActiveDeviceConnections}");
            output.WriteLine($"ValidFrom: {result.Payload.ValidFromUtc:yyyy-MM-dd}");
            output.WriteLine($"ValidUntil: {result.Payload.ValidUntilUtc:yyyy-MM-dd}");
            output.WriteLine($"GraceDays: {result.Payload.GraceDays}");
            output.WriteLine($"Output-Datei: {result.OutputFile}");
            return 0;
        }
        catch (LicenseIssuerException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            error.WriteLine($"Unerwarteter Fehler: {ex.Message}");
            return 1;
        }
    }

    public static void WriteHelp(TextWriter writer)
    {
        writer.WriteLine("XdtBox.LicenseIssuer - internes Herstellerwerkzeug fuer signierte XDTBox-Lizenzen");
        writer.WriteLine();
        writer.WriteLine("Pflichtparameter:");
        writer.WriteLine("  --request <license-request.json> oder --installation-id <id>");
        writer.WriteLine("  --licensee <Praxisname>");
        writer.WriteLine("  --max-active-device-connections <Anzahl>");
        writer.WriteLine("  --valid-from <yyyy-MM-dd>");
        writer.WriteLine("  --valid-until <yyyy-MM-dd>");
        writer.WriteLine("  --key-id <KeyId>");
        writer.WriteLine("  --private-key <private-key.pem>");
        writer.WriteLine("  --out <license.xdtboxlic>");
        writer.WriteLine();
        writer.WriteLine("Optionale Parameter:");
        writer.WriteLine("  --customer-number <Nummer>");
        writer.WriteLine("  --grace-days <Tage>             Default: 7");
        writer.WriteLine("  --license-type <Typ>            Default: Production");
        writer.WriteLine("  --issuer <Name>                 Default: Technik-Apparat");
        writer.WriteLine("  --product-code <Code>           Default: XDTBOX");
        writer.WriteLine("  --notes <Text>");
        writer.WriteLine();
        writer.WriteLine("Beispiel:");
        writer.WriteLine("  XdtBox.LicenseIssuer.exe --request \"C:\\XDTBox\\Lizenzaktivierung\\requests\\praxis.json\" --licensee \"Praxis Muster\" --customer-number \"K12345\" --max-active-device-connections 3 --valid-from \"2026-05-27\" --valid-until \"2027-05-27\" --key-id \"xdtbox-prod-2026-01\" --private-key \"C:\\XDTBox\\Lizenzaktivierung\\keys\\xdtbox_private.pem\" --out \"C:\\XDTBox\\Lizenzaktivierung\\licenses\\praxis-muster.xdtboxlic\"");
    }
}
