namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileAutoRedockService
{
    public static readonly TimeSpan DefaultRestlaufzeit = TimeSpan.FromSeconds(5);

    private readonly Dictionary<string, InterfaceProfileAutoRedockState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _redockDelay;

    public InterfaceProfileAutoRedockService(TimeSpan? redockDelay = null)
    {
        _redockDelay = redockDelay ?? DefaultRestlaufzeit;
    }

    public bool HasPendingCountdowns => _states.Values.Any(state => state.IsCountdownRunning);

    public InterfaceProfileAutoRedockState GetOrCreate(string interfaceProfileId)
    {
        var normalizedId = NormalizeId(interfaceProfileId);
        if (_states.TryGetValue(normalizedId, out var state))
        {
            return state;
        }

        state = new InterfaceProfileAutoRedockState(normalizedId);
        _states[normalizedId] = state;
        return state;
    }

    public InterfaceProfileAutoRedockDecision MarkAutoDetached(
        string interfaceProfileId,
        InterfaceProfileFloatingWindowState floatingState,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(floatingState);

        var state = GetOrCreate(interfaceProfileId) with
        {
            IsAutoDetached = floatingState.IsDetached
        };
        _states[state.InterfaceProfileId] = state;

        if (state.IsTerminalCompleted && CanStartCountdown(state, floatingState))
        {
            return StartCountdown(state, now);
        }

        return Empty();
    }

    public InterfaceProfileAutoRedockDecision RecordMonitoringEvent(
        InterfaceMonitoringEventEntry entry,
        InterfaceProfileFloatingWindowState floatingState)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(floatingState);

        if (!IsRelevantForProfile(entry, floatingState.InterfaceProfileId))
        {
            return Empty();
        }

        var state = GetOrCreate(entry.ScopeId);
        if (IsTerminalActivity(entry))
        {
            state = state with
            {
                IsOperationActive = false,
                IsTerminalCompleted = true
            };
            _states[state.InterfaceProfileId] = state;

            if (CanStartCountdown(state, floatingState))
            {
                return StartCountdown(state, entry.Timestamp) with
                {
                    IsTerminalActivity = true
                };
            }

            var cancelled = state.IsCountdownRunning;
            state = state with
            {
                IsCountdownRunning = false,
                RedockDueAt = null
            };
            _states[state.InterfaceProfileId] = state;
            return new InterfaceProfileAutoRedockDecision(
                IsOpenActivity: false,
                IsTerminalActivity: true,
                DidStartCountdown: false,
                DidCancelCountdown: cancelled,
                ShouldRedockNow: false);
        }

        if (IsOpenActivity(entry))
        {
            var cancelled = state.IsCountdownRunning;
            state = state with
            {
                IsOperationActive = true,
                IsTerminalCompleted = false,
                IsCountdownRunning = false,
                RedockDueAt = null
            };
            _states[state.InterfaceProfileId] = state;
            return new InterfaceProfileAutoRedockDecision(
                IsOpenActivity: true,
                IsTerminalActivity: false,
                DidStartCountdown: false,
                DidCancelCountdown: cancelled,
                ShouldRedockNow: false);
        }

        return Empty();
    }

    public InterfaceProfileAutoRedockDecision NotifyPinnedChanged(
        string interfaceProfileId,
        bool isPinned,
        InterfaceProfileFloatingWindowState floatingState,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(floatingState);

        var state = GetOrCreate(interfaceProfileId);
        if (isPinned)
        {
            var cancelled = state.IsCountdownRunning;
            state = state with
            {
                IsCountdownRunning = false,
                RedockDueAt = null
            };
            _states[state.InterfaceProfileId] = state;
            return new InterfaceProfileAutoRedockDecision(
                IsOpenActivity: false,
                IsTerminalActivity: false,
                DidStartCountdown: false,
                DidCancelCountdown: cancelled,
                ShouldRedockNow: false);
        }

        if (state.IsTerminalCompleted && CanStartCountdown(state, floatingState))
        {
            return StartCountdown(state, now);
        }

        return Empty();
    }

    public void NotifyDocked(string interfaceProfileId)
    {
        var normalizedId = NormalizeId(interfaceProfileId);
        _states.Remove(normalizedId);
    }

    public InterfaceProfileAutoRedockDecision EvaluateDue(
        string interfaceProfileId,
        InterfaceProfileFloatingWindowState floatingState,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(floatingState);

        var state = GetOrCreate(interfaceProfileId);
        if (!state.IsCountdownRunning)
        {
            return Empty();
        }

        if (!floatingState.IsDetached || floatingState.IsPinned || !state.IsAutoDetached)
        {
            var cancelled = state.IsCountdownRunning;
            state = state with
            {
                IsCountdownRunning = false,
                RedockDueAt = null
            };
            _states[state.InterfaceProfileId] = state;
            return new InterfaceProfileAutoRedockDecision(
                IsOpenActivity: false,
                IsTerminalActivity: false,
                DidStartCountdown: false,
                DidCancelCountdown: cancelled,
                ShouldRedockNow: false);
        }

        if (state.RedockDueAt is null || now < state.RedockDueAt.Value)
        {
            return Empty();
        }

        state = state with
        {
            IsCountdownRunning = false,
            RedockDueAt = null
        };
        _states[state.InterfaceProfileId] = state;
        return new InterfaceProfileAutoRedockDecision(
            IsOpenActivity: false,
            IsTerminalActivity: true,
            DidStartCountdown: false,
            DidCancelCountdown: false,
            ShouldRedockNow: true);
    }

    private InterfaceProfileAutoRedockDecision StartCountdown(
        InterfaceProfileAutoRedockState state,
        DateTime now)
    {
        var dueAt = now + _redockDelay;
        state = state with
        {
            IsCountdownRunning = true,
            RedockDueAt = dueAt
        };
        _states[state.InterfaceProfileId] = state;
        return new InterfaceProfileAutoRedockDecision(
            IsOpenActivity: false,
            IsTerminalActivity: false,
            DidStartCountdown: true,
            DidCancelCountdown: false,
            ShouldRedockNow: false,
            RedockDueAt: dueAt);
    }

    private static bool CanStartCountdown(
        InterfaceProfileAutoRedockState state,
        InterfaceProfileFloatingWindowState floatingState)
    {
        return state.IsAutoDetached
            && floatingState.IsDetached
            && !floatingState.IsPinned;
    }

    private static bool IsRelevantForProfile(InterfaceMonitoringEventEntry entry, string interfaceProfileId)
    {
        return !string.IsNullOrWhiteSpace(entry.ScopeId)
            && !string.Equals(entry.ScopeId, "monitoring", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.ScopeId, interfaceProfileId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOpenActivity(InterfaceMonitoringEventEntry entry)
    {
        var eventKey = entry.EventKey.Trim();
        var message = entry.Message;
        if (string.Equals(eventKey, "scan-ais-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-device-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-ready-pair", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(eventKey, "package-state", StringComparison.OrdinalIgnoreCase)
            || eventKey.StartsWith("scan-message:", StringComparison.OrdinalIgnoreCase)
            || eventKey.StartsWith("pair:", StringComparison.OrdinalIgnoreCase))
        {
            return ContainsAny(message, "warte", "wartet", "warten", "noch nicht stabil", "nicht stabil", "Dateipaar vollständig", "Paar wird bereits verarbeitet", "Bereits in Verarbeitung", "ersetzt");
        }

        return false;
    }

    private static bool IsTerminalActivity(InterfaceMonitoringEventEntry entry)
    {
        var eventKey = entry.EventKey.Trim();
        var message = entry.Message;
        if (string.Equals(eventKey, "automatic-processing-export-profile-missing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(eventKey, "package-state", StringComparison.OrdinalIgnoreCase)
            && ContainsAny(message, "AIS-Datei abgelaufen", "keine Gerätedatei innerhalb der Wartezeit", "keine Geraetedatei innerhalb der Wartezeit"))
        {
            return true;
        }

        if (!eventKey.StartsWith("pair:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ContainsAny(
            message,
            "automatisch verarbeitet",
            "automatische Verarbeitung fehlgeschlagen",
            "Automatischer Fehler",
            "Timeout erreicht",
            "Verarbeitung blockiert",
            "Export ohne Anhang",
            "Fehlerablage fehlgeschlagen");
    }

    private static bool ContainsAny(string text, params string[] fragments)
    {
        return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static InterfaceProfileAutoRedockDecision Empty()
    {
        return new InterfaceProfileAutoRedockDecision(
            IsOpenActivity: false,
            IsTerminalActivity: false,
            DidStartCountdown: false,
            DidCancelCountdown: false,
            ShouldRedockNow: false);
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
