namespace XdtDeviceBridge.Core;

public sealed class LicenseV1PolicyEvaluator
{
    public LicenseV1PolicyEvaluationResult Evaluate(
        LicensePayload? payload,
        InstallationInfo installation,
        LicenseSignatureVerificationStatus signatureStatus,
        int activeDeviceConnectionCount,
        DateTime nowUtc,
        DateTime? gracePeriodStartedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(installation);

        if (activeDeviceConnectionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeDeviceConnectionCount), "Active device connection count must not be negative.");
        }

        if (nowUtc == default)
        {
            throw new ArgumentException("NowUtc must not be default.", nameof(nowUtc));
        }

        if (signatureStatus is LicenseSignatureVerificationStatus.Invalid or LicenseSignatureVerificationStatus.NotChecked)
        {
            return Blocked(
                LicenseV1PolicyStatus.InvalidSignature,
                XdtBoxLicenseConstants.InvalidSignatureMessage);
        }

        if (payload is null)
        {
            return Blocked(
                LicenseV1PolicyStatus.MissingLicense,
                "Keine Lizenz vorhanden.");
        }

        if (!string.Equals(payload.InstallationId, installation.InstallationId, StringComparison.Ordinal))
        {
            return Blocked(
                LicenseV1PolicyStatus.InstallationMismatch,
                "Lizenz gehört nicht zu dieser Installation. Bitte neue Lizenzanforderung erzeugen.");
        }

        if (nowUtc < payload.ValidFromUtc)
        {
            return Blocked(
                LicenseV1PolicyStatus.NotYetValid,
                "Lizenz ist noch nicht gültig.");
        }

        if (nowUtc > payload.ValidUntilUtc)
        {
            return EvaluateGrace(
                LicenseV1PolicyStatus.ExpiredWithinGrace,
                LicenseV1PolicyStatus.ExpiredAfterGrace,
                payload,
                gracePeriodStartedAtUtc ?? payload.ValidUntilUtc,
                nowUtc,
                "Lizenz ist abgelaufen, aber noch in Karenzzeit.",
                "Lizenz ist abgelaufen und die Karenzzeit ist beendet.");
        }

        if (activeDeviceConnectionCount > payload.MaxActiveDeviceConnections)
        {
            return EvaluateGrace(
                LicenseV1PolicyStatus.DeviceLimitExceededWithinGrace,
                LicenseV1PolicyStatus.DeviceLimitExceededAfterGrace,
                payload,
                gracePeriodStartedAtUtc,
                nowUtc,
                "Anzahl aktiver Geräteanbindungen überschreitet die Lizenz, aber die Karenzzeit läuft.",
                "Anzahl aktiver Geräteanbindungen überschreitet die Lizenz und die Karenzzeit ist beendet.");
        }

        return new LicenseV1PolicyEvaluationResult(
            Status: LicenseV1PolicyStatus.Valid,
            CanStartDeviceConnections: true,
            IsWarningOnly: false,
            GraceEndsAtUtc: null,
            Message: "Lizenz gültig.");
    }

    private static LicenseV1PolicyEvaluationResult EvaluateGrace(
        LicenseV1PolicyStatus withinGraceStatus,
        LicenseV1PolicyStatus afterGraceStatus,
        LicensePayload payload,
        DateTime? gracePeriodStartedAtUtc,
        DateTime nowUtc,
        string withinGraceMessage,
        string afterGraceMessage)
    {
        if (gracePeriodStartedAtUtc is null)
        {
            return Blocked(afterGraceStatus, afterGraceMessage);
        }

        var graceEndsAtUtc = gracePeriodStartedAtUtc.Value.AddDays(payload.GraceDays);
        if (nowUtc <= graceEndsAtUtc)
        {
            return new LicenseV1PolicyEvaluationResult(
                Status: withinGraceStatus,
                CanStartDeviceConnections: true,
                IsWarningOnly: true,
                GraceEndsAtUtc: graceEndsAtUtc,
                Message: withinGraceMessage);
        }

        return new LicenseV1PolicyEvaluationResult(
            Status: afterGraceStatus,
            CanStartDeviceConnections: false,
            IsWarningOnly: false,
            GraceEndsAtUtc: graceEndsAtUtc,
            Message: afterGraceMessage);
    }

    private static LicenseV1PolicyEvaluationResult Blocked(
        LicenseV1PolicyStatus status,
        string message)
    {
        return new LicenseV1PolicyEvaluationResult(
            Status: status,
            CanStartDeviceConnections: false,
            IsWarningOnly: false,
            GraceEndsAtUtc: null,
            Message: message);
    }
}
