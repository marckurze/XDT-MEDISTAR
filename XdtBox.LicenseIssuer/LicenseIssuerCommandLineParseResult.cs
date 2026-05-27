namespace XdtBox.LicenseIssuer;

public sealed record LicenseIssuerCommandLineParseResult(
    LicenseIssuerOptions? Options,
    bool ShowHelp,
    IReadOnlyList<string> Errors);
