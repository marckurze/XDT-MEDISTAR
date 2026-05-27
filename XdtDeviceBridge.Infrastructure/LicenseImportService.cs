using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseImportService
{
    private const string ReadErrorMessage = "Lizenzdatei konnte nicht gelesen werden.";
    private const string ValidImportMessage = "Lizenz wurde erfolgreich geprüft und importiert.";
    private const string WrongInstallationMessage = "Diese Lizenz wurde für eine andere Installation ausgestellt. Bitte neue Lizenz anfordern.";
    private const string WrongProductMessage = "Diese Lizenz ist nicht für XDTBox ausgestellt.";
    private const string ExpiredMessage = "Lizenz ist abgelaufen.";

    private readonly LicenseEnvelopeReader _reader;
    private readonly ILicenseSignatureVerifier _signatureVerifier;
    private readonly LicenseV1PolicyEvaluator _policyEvaluator;

    public LicenseImportService()
        : this(new LicenseEnvelopeReader(), new LicenseSignatureVerifier(), new LicenseV1PolicyEvaluator())
    {
    }

    public LicenseImportService(
        LicenseEnvelopeReader reader,
        ILicenseSignatureVerifier signatureVerifier,
        LicenseV1PolicyEvaluator policyEvaluator)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _policyEvaluator = policyEvaluator ?? throw new ArgumentNullException(nameof(policyEvaluator));
    }

    public LicenseImportResult ImportFromFile(
        string filePath,
        InstallationInfo installation,
        int activeDeviceConnectionCount,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(installation);

        LicenseEnvelopeReadResult readResult;
        try
        {
            readResult = _reader.ReadFile(filePath);
        }
        catch
        {
            return new LicenseImportResult(
                CanPersistLicenseFile: false,
                Envelope: null,
                Payload: null,
                SignatureStatus: LicenseSignatureVerificationStatus.MalformedEnvelope,
                PolicyEvaluation: null,
                UserMessage: ReadErrorMessage);
        }

        return Import(
            readResult,
            installation,
            activeDeviceConnectionCount,
            nowUtc);
    }

    public LicenseImportResult Import(
        LicenseEnvelopeReadResult readResult,
        InstallationInfo installation,
        int activeDeviceConnectionCount,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(readResult);
        ArgumentNullException.ThrowIfNull(installation);

        if (!readResult.Success || readResult.Envelope is null || readResult.Payload is null)
        {
            return new LicenseImportResult(
                CanPersistLicenseFile: false,
                Envelope: readResult.Envelope,
                Payload: readResult.Payload,
                SignatureStatus: readResult.Status,
                PolicyEvaluation: null,
                UserMessage: CreateReadFailureMessage(readResult.Status));
        }

        var signatureResult = _signatureVerifier.Verify(
            readResult.PayloadBytes,
            readResult.SignatureBytes,
            readResult.Envelope.Algorithm,
            readResult.Envelope.KeyId);

        if (signatureResult.Status != LicenseSignatureVerificationStatus.Valid)
        {
            return new LicenseImportResult(
                CanPersistLicenseFile: false,
                Envelope: readResult.Envelope,
                Payload: readResult.Payload,
                SignatureStatus: signatureResult.Status,
                PolicyEvaluation: null,
                UserMessage: CreateSignatureFailureMessage(signatureResult.Status));
        }

        var policy = _policyEvaluator.Evaluate(
            readResult.Payload,
            installation,
            signatureResult.Status,
            activeDeviceConnectionCount,
            nowUtc);

        return new LicenseImportResult(
            CanPersistLicenseFile: CanPersist(policy),
            Envelope: readResult.Envelope,
            Payload: readResult.Payload,
            SignatureStatus: signatureResult.Status,
            PolicyEvaluation: policy,
            UserMessage: CreatePolicyMessage(policy));
    }

    private static string CreateReadFailureMessage(LicenseSignatureVerificationStatus status)
    {
        return status == LicenseSignatureVerificationStatus.MissingSignature
            ? XdtBoxLicenseConstants.InvalidSignatureMessage
            : ReadErrorMessage;
    }

    private static string CreateSignatureFailureMessage(LicenseSignatureVerificationStatus status)
    {
        return status is LicenseSignatureVerificationStatus.Invalid
            or LicenseSignatureVerificationStatus.MissingSignature
            ? XdtBoxLicenseConstants.InvalidSignatureMessage
            : ReadErrorMessage;
    }

    private static string CreatePolicyMessage(LicenseV1PolicyEvaluationResult policy)
    {
        return policy.Status switch
        {
            LicenseV1PolicyStatus.Valid => ValidImportMessage,
            LicenseV1PolicyStatus.WrongProduct => WrongProductMessage,
            LicenseV1PolicyStatus.InstallationMismatch => WrongInstallationMessage,
            LicenseV1PolicyStatus.ExpiredWithinGrace when policy.GraceEndsAtUtc is { } graceEndsAtUtc =>
                $"Lizenz ist abgelaufen. Karenzzeit aktiv bis {graceEndsAtUtc.ToLocalTime():d}.",
            LicenseV1PolicyStatus.ExpiredAfterGrace => ExpiredMessage,
            _ => policy.Message
        };
    }

    private static bool CanPersist(LicenseV1PolicyEvaluationResult policy)
    {
        return policy.Status is not LicenseV1PolicyStatus.WrongProduct
            and not LicenseV1PolicyStatus.InstallationMismatch;
    }
}
