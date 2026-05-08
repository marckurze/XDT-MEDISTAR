using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record AisPatientDataReadResult(
    bool Success,
    PatientData? Patient,
    string? ErrorMessage);
