using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class TrayWindowStateServiceTests
{
    [Fact]
    public void MinimizeToTray_ShouldSetTrayState()
    {
        var service = new TrayWindowStateService();

        var decision = service.MinimizeToTray();

        Assert.True(service.IsInTray);
        Assert.False(service.IsExitRequested);
        Assert.True(decision.ShouldHideWindow);
        Assert.True(decision.ShouldShowHint);
    }

    [Fact]
    public void MinimizeToTray_ShouldShowHintOnlyOnce()
    {
        var service = new TrayWindowStateService();

        var first = service.MinimizeToTray();
        var second = service.MinimizeToTray();

        Assert.True(first.ShouldShowHint);
        Assert.False(second.ShouldShowHint);
        Assert.True(service.HasShownTrayHint);
    }

    [Fact]
    public void RestoreFromTray_ShouldSetVisibleState()
    {
        var service = new TrayWindowStateService();
        _ = service.MinimizeToTray();

        var decision = service.RestoreFromTray();

        Assert.False(service.IsInTray);
        Assert.True(decision.ShouldShowWindow);
        Assert.False(decision.ShouldHideWindow);
    }

    [Fact]
    public void CloseWithoutExitRequest_ShouldBeCancelled()
    {
        var service = new TrayWindowStateService();

        Assert.True(service.ShouldCancelClose());
    }

    [Fact]
    public void RequestExit_ShouldAllowRealClose()
    {
        var service = new TrayWindowStateService();
        _ = service.MinimizeToTray();

        var decision = service.RequestExit();

        Assert.True(service.IsExitRequested);
        Assert.False(service.IsInTray);
        Assert.True(decision.ShouldExit);
        Assert.False(service.ShouldCancelClose());
    }

    [Fact]
    public void TrayState_ShouldNotTouchFloatingWindowStates()
    {
        var trayService = new TrayWindowStateService();
        var floatingStateService = new InterfaceProfileFloatingWindowStateService();
        var ar360 = floatingStateService.Detach("interface-ar360");
        var ark1s = floatingStateService.SetPinned("interface-ark1s", true);

        _ = trayService.MinimizeToTray();
        _ = trayService.RestoreFromTray();
        _ = trayService.RequestExit();

        Assert.Equal(ar360, floatingStateService.GetOrCreate("interface-ar360"));
        Assert.Equal(ark1s, floatingStateService.GetOrCreate("interface-ark1s"));
    }
}
