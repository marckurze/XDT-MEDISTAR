using System.Globalization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentCompletionService
{
    private readonly Dictionary<string, AttachmentCompletionState> _states = new(StringComparer.OrdinalIgnoreCase);

    public AttachmentCompletionDecision Decide(
        InterfaceProfileDefinition interfaceProfile,
        string activityKey,
        IReadOnlyList<AttachmentImportFileCandidate> candidates,
        DateTime timestampUtc)
    {
        ArgumentNullException.ThrowIfNull(interfaceProfile);
        ArgumentNullException.ThrowIfNull(candidates);

        if (!interfaceProfile.FolderOptions.IsAttachmentOnlyMode)
        {
            return Complete(
                AttachmentCompletionDecisionReason.NotAttachmentOnly,
                "Kein Dokumentgeraete-Modus.",
                candidates);
        }

        var supportedCandidates = candidates
            .Where(candidate => candidate.IsSupported)
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var stableCandidates = supportedCandidates
            .Where(candidate => candidate.IsStable)
            .ToList();

        if (supportedCandidates.Count == 0 || stableCandidates.Count == 0)
        {
            return Wait(
                AttachmentCompletionDecisionReason.NoStableFiles,
                "Dokumentgeraet: noch keine stabile Dokumentdatei gefunden.",
                stableCandidates);
        }

        if (stableCandidates.Count < supportedCandidates.Count)
        {
            return Wait(
                AttachmentCompletionDecisionReason.ContainsUnstableFiles,
                "Dokumentgeraet: mindestens eine Datei ist noch nicht stabil.",
                stableCandidates);
        }

        if (interfaceProfile.FolderOptions.AttachmentCompletionMode == AttachmentCompletionMode.ManualConfirmation)
        {
            _states[activityKey] = new AttachmentCompletionState(CreateFingerprint(stableCandidates), timestampUtc);
            return new AttachmentCompletionDecision(
                CanComplete: false,
                ShouldWait: true,
                RequiresManualConfirmation: true,
                Reason: AttachmentCompletionDecisionReason.ManualConfirmationRequired,
                Message: $"Dokumentgeraet: {stableCandidates.Count} Datei(en) bereit, wartet auf Benutzerbestaetigung.",
                SelectedCandidates: stableCandidates);
        }

        var quietPeriodSeconds = Math.Clamp(interfaceProfile.FolderOptions.AttachmentQuietPeriodSeconds, 1, 300);
        var quietPeriod = TimeSpan.FromSeconds(quietPeriodSeconds);
        var fingerprint = CreateFingerprint(stableCandidates);
        if (!_states.TryGetValue(activityKey, out var state))
        {
            _states[activityKey] = new AttachmentCompletionState(fingerprint, timestampUtc);
            return Wait(
                AttachmentCompletionDecisionReason.QuietPeriodStarted,
                $"Dokumentgeraet: Wartezeit nach letzter Datei gestartet ({quietPeriodSeconds} s).",
                stableCandidates,
                quietPeriod);
        }

        if (!string.Equals(state.Fingerprint, fingerprint, StringComparison.Ordinal))
        {
            _states[activityKey] = new AttachmentCompletionState(fingerprint, timestampUtc);
            return Wait(
                AttachmentCompletionDecisionReason.QuietPeriodRestarted,
                $"Dokumentgeraet: neue Datei erkannt, Wartezeit neu gestartet ({quietPeriodSeconds} s).",
                stableCandidates,
                quietPeriod);
        }

        var elapsed = timestampUtc.ToUniversalTime() - state.LastChangedAtUtc.ToUniversalTime();
        if (elapsed < quietPeriod)
        {
            return Wait(
                AttachmentCompletionDecisionReason.QuietPeriodWaiting,
                $"Dokumentgeraet: wartet auf weitere Dateien ({Math.Ceiling((quietPeriod - elapsed).TotalSeconds)} s).",
                stableCandidates,
                quietPeriod - elapsed);
        }

        return Complete(
            AttachmentCompletionDecisionReason.QuietPeriodComplete,
            $"Dokumentgeraet: Dateisammlung vollstaendig ({stableCandidates.Count} Datei(en)).",
            stableCandidates);
    }

    public void MarkCompleted(string activityKey)
    {
        if (string.IsNullOrWhiteSpace(activityKey))
        {
            return;
        }

        _states.Remove(activityKey);
    }

    public void ResetProfile(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        var prefix = $"{interfaceProfileId.Trim()}|";
        foreach (var key in _states.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList())
        {
            _states.Remove(key);
        }
    }

    private static AttachmentCompletionDecision Wait(
        AttachmentCompletionDecisionReason reason,
        string message,
        IReadOnlyList<AttachmentImportFileCandidate> candidates,
        TimeSpan? remainingQuietPeriod = null)
    {
        return new AttachmentCompletionDecision(
            CanComplete: false,
            ShouldWait: true,
            RequiresManualConfirmation: false,
            Reason: reason,
            Message: message,
            SelectedCandidates: candidates,
            RemainingQuietPeriod: remainingQuietPeriod);
    }

    private static AttachmentCompletionDecision Complete(
        AttachmentCompletionDecisionReason reason,
        string message,
        IReadOnlyList<AttachmentImportFileCandidate> candidates)
    {
        return new AttachmentCompletionDecision(
            CanComplete: true,
            ShouldWait: false,
            RequiresManualConfirmation: false,
            Reason: reason,
            Message: message,
            SelectedCandidates: candidates);
    }

    private static string CreateFingerprint(IReadOnlyList<AttachmentImportFileCandidate> candidates)
    {
        return string.Join(
            "||",
            candidates
                .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
                .Select(candidate => string.Join(
                    "|",
                    candidate.FullPath,
                    candidate.SizeBytes.ToString(CultureInfo.InvariantCulture),
                    candidate.LastWriteTimeUtc.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture))));
    }

    private sealed record AttachmentCompletionState(string Fingerprint, DateTime LastChangedAtUtc);
}
