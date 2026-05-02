namespace XdtDeviceBridge.Core;

public sealed record PatientData(
    string? PatientNumber,
    string? LastName,
    string? FirstName,
    string? BirthDate,
    string? PostalCodeCity,
    string? Street,
    string? GenderCode,
    string? SourceSystem,
    string? TargetSystem,
    string? GdtVersion);
