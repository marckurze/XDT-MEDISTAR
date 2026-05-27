using XdtBox.LicenseIssuer;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseIssuerConsoleTests
{
    [Fact]
    public void Run_WithoutArguments_ShouldShowHelpAndPauseWhenRequested()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var pauseCalled = false;

        var exitCode = LicenseIssuerConsole.Run(
            Array.Empty<string>(),
            output,
            error,
            pauseOnNoArguments: true,
            pauseAction: () => pauseCalled = true);

        Assert.Equal(1, exitCode);
        Assert.True(pauseCalled);
        Assert.Contains("Fehler: Es wurden keine Parameter angegeben.", error.ToString());
        Assert.Contains("internes Herstellerwerkzeug", output.ToString());
        Assert.Contains("Drücken Sie eine Taste zum Schließen", output.ToString());
        Assert.Contains("--private-key", output.ToString());
    }

    [Fact]
    public void Run_WithoutArguments_ShouldNotPauseWhenDisabled()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var pauseCalled = false;

        var exitCode = LicenseIssuerConsole.Run(
            Array.Empty<string>(),
            output,
            error,
            pauseOnNoArguments: false,
            pauseAction: () => pauseCalled = true);

        Assert.Equal(1, exitCode);
        Assert.False(pauseCalled);
        Assert.Contains("Fehler: Es wurden keine Parameter angegeben.", error.ToString());
    }

    [Fact]
    public void Run_Help_ShouldShowHelpWithoutPause()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var pauseCalled = false;

        var exitCode = LicenseIssuerConsole.Run(
            new[] { "--help" },
            output,
            error,
            pauseOnNoArguments: true,
            pauseAction: () => pauseCalled = true);

        Assert.Equal(0, exitCode);
        Assert.False(pauseCalled);
        Assert.Contains("XdtBox.LicenseIssuer", output.ToString());
        Assert.DoesNotContain("Fehler:", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WithMissingRequiredParameters_ShouldShowClearError()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = LicenseIssuerConsole.Run(
            new[] { "--licensee", "Praxis Muster" },
            output,
            error,
            pauseOnNoArguments: true);

        Assert.Equal(2, exitCode);
        Assert.Contains("--max-active-device-connections ist erforderlich.", error.ToString());
        Assert.Contains("XdtBox.LicenseIssuer", error.ToString());
    }
}
