using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileActivationPreviewDisplayServiceTests
{
    private readonly InterfaceProfileActivationPreviewDisplayService _service = new();

    [Fact]
    public void CreateEmpty_ShouldShowNoSelectionHint()
    {
        var display = _service.CreateEmpty();

        Assert.Equal("Status: -", display.StatusText);
        Assert.Equal("Aktivierbar: -", display.CanActivateText);
        Assert.Contains("Bitte wählen", display.HintText);
        Assert.Empty(display.Rows);
    }

    [Fact]
    public void Create_ShouldCountBlockersWarningsAndInfos()
    {
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.Blocked,
            CanActivate: false,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Profil", "Info"),
                Check(InterfaceProfileActivationSeverity.Blocker, "Abhaengigkeiten", "Blocker"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenz", "Warnung")
            });

        var display = _service.Create(result);

        Assert.Equal("Status: Blockiert", display.StatusText);
        Assert.Equal("Aktivierbar: Nein", display.CanActivateText);
        Assert.Equal(1, display.BlockerCount);
        Assert.Equal(1, display.WarningCount);
        Assert.Equal(1, display.InfoCount);
        Assert.Equal("Blocker: 1 | Warnungen: 1 | Hinweise: 1", display.SummaryText);
    }

    [Fact]
    public void Create_ShouldOrderBlockersWarningsAndInfos()
    {
        var result = new InterfaceProfileActivationEvaluationResult(
            InterfaceProfileActivationStatus.ReadyWithWarnings,
            CanActivate: true,
            Checks: new[]
            {
                Check(InterfaceProfileActivationSeverity.Info, "Profil", "Info"),
                Check(InterfaceProfileActivationSeverity.Warning, "Lizenz", "Warnung"),
                Check(InterfaceProfileActivationSeverity.Blocker, "Ordner", "Blocker")
            });

        var display = _service.Create(result);

        Assert.Collection(
            display.Rows,
            row => Assert.Equal("BLOCKER", row.Severity),
            row => Assert.Equal("WARNUNG", row.Severity),
            row => Assert.Equal("INFO", row.Severity));
    }

    [Fact]
    public void CreateError_ShouldExposeFriendlyErrorDisplay()
    {
        var display = _service.CreateError("Technischer Fehler.");

        Assert.Equal("Status: Fehler", display.StatusText);
        Assert.Equal("Aktivierbar: Nein", display.CanActivateText);
        Assert.Equal(1, display.BlockerCount);
        var row = Assert.Single(display.Rows);
        Assert.Equal("BLOCKER", row.Severity);
        Assert.Contains("Technischer Fehler", row.Detail);
    }

    private static InterfaceProfileActivationCheckResult Check(
        InterfaceProfileActivationSeverity severity,
        string area,
        string message)
    {
        return new InterfaceProfileActivationCheckResult(
            Area: area,
            Code: message,
            Message: message,
            Severity: severity,
            Detail: $"{message} Detail");
    }
}
