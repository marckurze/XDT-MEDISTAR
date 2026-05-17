using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileFloatingWindowRestoreGateTests
{
    [Fact]
    public void CanShowFloatingWindows_ShouldStartFalse()
    {
        var gate = new InterfaceProfileFloatingWindowRestoreGate();

        Assert.False(gate.CanShowFloatingWindows);
    }

    [Fact]
    public void MarkMainWindowReady_ShouldAllowInitialRestoreOnce()
    {
        var gate = new InterfaceProfileFloatingWindowRestoreGate();

        Assert.True(gate.MarkMainWindowReady());
        Assert.True(gate.CanShowFloatingWindows);
        Assert.False(gate.MarkMainWindowReady());
    }
}
