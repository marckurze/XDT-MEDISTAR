using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileFloatingWindowStateRepositoryTests
{
    private readonly AppDataPathProvider _pathProvider = new();
    private readonly InterfaceProfileFloatingWindowStateRepository _repository = new();

    [Fact]
    public void Load_ShouldReturnEmpty_WhenNoStateFileExists()
    {
        var paths = CreateTempPaths();

        var states = _repository.Load(paths);

        Assert.Empty(states);
    }

    [Fact]
    public void SaveAndLoad_ShouldPersistMemoryEnabledState()
    {
        var paths = CreateTempPaths();
        var expected = new InterfaceProfileFloatingWindowState(
            "interface-ar360",
            IsDetached: true,
            IsPinned: true,
            IsPositionMemoryEnabled: true,
            Bounds: new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240));

        _repository.Save(paths, new[] { expected });
        var loaded = _repository.Load(paths);

        Assert.Equal(expected, Assert.Single(loaded));
    }

    [Fact]
    public void Save_ShouldNotPersistMemoryDisabledState()
    {
        var paths = CreateTempPaths();
        var state = new InterfaceProfileFloatingWindowState(
            "interface-ar360",
            IsDetached: true,
            IsPinned: true,
            IsPositionMemoryEnabled: false,
            Bounds: new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240));

        _repository.Save(paths, new[] { state });
        var loaded = _repository.Load(paths);

        Assert.Empty(loaded);
    }

    [Fact]
    public void SaveAndLoad_ShouldKeepProfilesIndependent()
    {
        var paths = CreateTempPaths();
        var ar360 = new InterfaceProfileFloatingWindowState(
            "interface-ar360",
            IsDetached: true,
            IsPinned: true,
            IsPositionMemoryEnabled: true,
            Bounds: new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240));
        var ark1s = new InterfaceProfileFloatingWindowState(
            "interface-ark1s",
            IsDetached: false,
            IsPinned: false,
            IsPositionMemoryEnabled: true,
            Bounds: new InterfaceProfileFloatingWindowBounds(30, 40, 420, 260));

        _repository.Save(paths, new[] { ar360, ark1s });
        var loaded = _repository.Load(paths);

        Assert.Contains(loaded, state => state.InterfaceProfileId == "interface-ar360" && state.IsDetached && state.IsPinned);
        Assert.Contains(loaded, state => state.InterfaceProfileId == "interface-ark1s" && !state.IsDetached && !state.IsPinned);
    }

    [Fact]
    public void SaveAndLoad_ShouldPersistPinnedStatePerProfile()
    {
        var paths = CreateTempPaths();
        var ar360 = new InterfaceProfileFloatingWindowState(
            "interface-ar360",
            IsDetached: true,
            IsPinned: true,
            IsPositionMemoryEnabled: true,
            Bounds: new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240));
        var ark1s = new InterfaceProfileFloatingWindowState(
            "interface-ark1s",
            IsDetached: true,
            IsPinned: false,
            IsPositionMemoryEnabled: true,
            Bounds: new InterfaceProfileFloatingWindowBounds(30, 40, 420, 260));

        _repository.Save(paths, new[] { ar360, ark1s });
        var loaded = _repository.Load(paths);

        Assert.Contains(loaded, state => state.InterfaceProfileId == "interface-ar360" && state.IsPinned);
        Assert.Contains(loaded, state => state.InterfaceProfileId == "interface-ark1s" && !state.IsPinned);
    }

    private AppDataPaths CreateTempPaths()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "XdtDeviceBridge-FloatingStateTests", Guid.NewGuid().ToString("N"));
        return _pathProvider.GetPaths(tempRoot);
    }
}
