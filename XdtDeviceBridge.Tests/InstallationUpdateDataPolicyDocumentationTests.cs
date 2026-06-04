namespace XdtDeviceBridge.Tests;

public sealed class InstallationUpdateDataPolicyDocumentationTests
{
    [Fact]
    public void InstallationPolicyDocument_ShouldDescribeCriticalCustomerDataRules()
    {
        var document = File.ReadAllText(FindWorkspaceFile("docs", "INSTALLATION_UPDATE_DATENPOLITIK.md"));

        Assert.Contains("Variante B", document);
        Assert.Contains(@"%LocalAppData%\XdtDeviceBridge", document);
        Assert.Contains(@"%ProgramData%\XDTBox", document);
        Assert.Contains("license.xdtboxlic", document);
        Assert.Contains("device-image-overrides.json", document);
        Assert.Contains("Baukasten-Templates", document);
        Assert.Contains("private Hersteller-Schluessel", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("externe Praxisordner", document, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("niemals blind", document, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UserFacingDocuments_ShouldReferenceInstallationPolicy()
    {
        var readme = File.ReadAllText(FindWorkspaceFile("README.md"));
        var architecture = File.ReadAllText(FindWorkspaceFile("docs", "ARCHITEKTUR.md"));
        var overview = File.ReadAllText(FindWorkspaceFile("docs", "PROJEKT_UEBERBLICK.md"));

        Assert.Contains("INSTALLATION_UPDATE_DATENPOLITIK.md", readme);
        Assert.Contains("INSTALLATION_UPDATE_DATENPOLITIK.md", architecture);
        Assert.Contains("INSTALLATION_UPDATE_DATENPOLITIK.md", overview);
        Assert.Contains("Kundendaten bleiben", readme);
        Assert.Contains("externe Praxisordner", architecture, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindWorkspaceFile(params string[] relativeSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {Path.Combine(relativeSegments)}");
    }
}
