using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileFloatingWindowStateServiceTests
{
    [Fact]
    public void GetOrCreate_ShouldStartDocked()
    {
        var service = new InterfaceProfileFloatingWindowStateService();

        var state = service.GetOrCreate("interface-ar360");

        Assert.False(state.IsDetached);
        Assert.False(state.IsPinned);
        Assert.False(state.IsPositionMemoryEnabled);
        Assert.Null(state.Bounds);
    }

    [Fact]
    public void Detach_ShouldSetDetachedOnlyForSelectedProfile()
    {
        var service = new InterfaceProfileFloatingWindowStateService();

        service.Detach("interface-ar360");

        Assert.True(service.GetOrCreate("interface-ar360").IsDetached);
        Assert.False(service.GetOrCreate("interface-ark1s").IsDetached);
    }

    [Fact]
    public void Dock_ShouldClearDetachedState()
    {
        var service = new InterfaceProfileFloatingWindowStateService();
        service.Detach("interface-ar360");

        var state = service.Dock("interface-ar360");

        Assert.False(state.IsDetached);
    }

    [Fact]
    public void SetPinned_ShouldSetPinnedOnlyForSelectedProfile()
    {
        var service = new InterfaceProfileFloatingWindowStateService();

        service.SetPinned("interface-ar360", true);

        Assert.True(service.GetOrCreate("interface-ar360").IsPinned);
        Assert.False(service.GetOrCreate("interface-ark1s").IsPinned);
    }

    [Fact]
    public void SetPositionMemoryEnabled_ShouldSetFlagOnlyForSelectedProfile()
    {
        var service = new InterfaceProfileFloatingWindowStateService();

        service.SetPositionMemoryEnabled("interface-ar360", true);

        Assert.True(service.GetOrCreate("interface-ar360").IsPositionMemoryEnabled);
        Assert.False(service.GetOrCreate("interface-ark1s").IsPositionMemoryEnabled);
    }

    [Fact]
    public void RememberPosition_ShouldStoreBoundsForCurrentSession()
    {
        var service = new InterfaceProfileFloatingWindowStateService();

        var state = service.RememberPosition("interface-ar360", 10, 20, 360, 240);

        Assert.True(state.IsPositionMemoryEnabled);
        Assert.Equal(new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240), state.Bounds);
    }

    [Fact]
    public void ClosingAsDock_ShouldRemainIndependentPerProfile()
    {
        var service = new InterfaceProfileFloatingWindowStateService();
        service.Detach("interface-ar360");
        service.Detach("interface-ark1s");

        service.Dock("interface-ar360");

        Assert.False(service.GetOrCreate("interface-ar360").IsDetached);
        Assert.True(service.GetOrCreate("interface-ark1s").IsDetached);
    }
}
