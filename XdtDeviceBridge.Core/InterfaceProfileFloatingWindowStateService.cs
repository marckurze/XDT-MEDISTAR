namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileFloatingWindowStateService
{
    private readonly Dictionary<string, InterfaceProfileFloatingWindowState> _states = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<InterfaceProfileFloatingWindowState> GetAll()
    {
        return _states.Values
            .OrderBy(state => state.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void ReplaceAll(IEnumerable<InterfaceProfileFloatingWindowState> states)
    {
        ArgumentNullException.ThrowIfNull(states);

        _states.Clear();
        foreach (var state in states)
        {
            var normalizedId = NormalizeId(state.InterfaceProfileId);
            _states[normalizedId] = state with
            {
                InterfaceProfileId = normalizedId
            };
        }
    }

    public InterfaceProfileFloatingWindowState GetOrCreate(string interfaceProfileId)
    {
        var normalizedId = NormalizeId(interfaceProfileId);
        if (_states.TryGetValue(normalizedId, out var state))
        {
            return state;
        }

        state = new InterfaceProfileFloatingWindowState(normalizedId);
        _states[normalizedId] = state;
        return state;
    }

    public InterfaceProfileFloatingWindowState Detach(string interfaceProfileId)
    {
        return Update(interfaceProfileId, state => state with { IsDetached = true });
    }

    public InterfaceProfileFloatingWindowState Dock(string interfaceProfileId)
    {
        return Update(interfaceProfileId, state => state with { IsDetached = false });
    }

    public InterfaceProfileFloatingWindowState SetPinned(string interfaceProfileId, bool isPinned)
    {
        return Update(interfaceProfileId, state => state with { IsPinned = isPinned });
    }

    public InterfaceProfileFloatingWindowState SetPositionMemoryEnabled(string interfaceProfileId, bool isEnabled)
    {
        return Update(interfaceProfileId, state => state with { IsPositionMemoryEnabled = isEnabled });
    }

    public InterfaceProfileFloatingWindowState RememberPosition(
        string interfaceProfileId,
        double left,
        double top,
        double width,
        double height)
    {
        var bounds = new InterfaceProfileFloatingWindowBounds(left, top, width, height);
        return Update(interfaceProfileId, state => state with
        {
            IsPositionMemoryEnabled = true,
            Bounds = bounds
        });
    }

    private InterfaceProfileFloatingWindowState Update(
        string interfaceProfileId,
        Func<InterfaceProfileFloatingWindowState, InterfaceProfileFloatingWindowState> update)
    {
        var state = update(GetOrCreate(interfaceProfileId));
        _states[state.InterfaceProfileId] = state;
        return state;
    }

    private static string NormalizeId(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            throw new ArgumentException("Schnittstellenprofil-ID fehlt.", nameof(interfaceProfileId));
        }

        return interfaceProfileId.Trim();
    }
}
