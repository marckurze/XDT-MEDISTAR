using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public static class NidekRs232ControlBytes
{
    public const byte SOH = 0x01;
    public const byte STX = 0x02;
    public const byte EOT = 0x04;
    public const byte CR = 0x0D;
    public const byte LF = 0x0A;
    public const byte ETB = 0x17;
}

public enum NidekRs232CommunicationMode
{
    Unknown,
    Nidek,
    Ncp10,
    Ncp20Prepared,
    PcPrepared
}

public enum NidekRs232FrameKind
{
    Command,
    Data,
    Success,
    Error,
    Unknown
}

public enum NidekRs232Eye
{
    Unknown,
    Single,
    Right,
    Left
}

public sealed record NidekRs232Frame(
    byte[] RawBytes,
    string RawAscii,
    NidekRs232FrameKind Kind,
    string Header,
    string DeviceCode,
    IReadOnlyList<string> Segments,
    bool HasChecksum,
    string? ChecksumText,
    bool? ChecksumValid,
    bool HasTrailingCr,
    IReadOnlyList<string> Warnings);

public sealed record NidekRs232FrameReadResult(
    IReadOnlyList<NidekRs232Frame> Frames,
    byte[] NoiseBytes,
    byte[] PartialBytes,
    IReadOnlyList<string> Warnings)
{
    public bool HasPartialFrame => PartialBytes.Length > 0;
}

public sealed record NidekRs232Segment(
    int Index,
    string RawText,
    string SegmentCode);

public sealed record NidekRs232RefractionMeasurement(
    NidekRs232Eye Eye,
    decimal? Sphere,
    decimal? Cylinder,
    int? Axis,
    decimal? Add,
    decimal? NearSphere,
    decimal? PdTotal,
    decimal? PdRight,
    decimal? PdLeft,
    string SourceSegment);

public sealed record NidekRs232TonometryMeasurement(
    NidekRs232Eye Eye,
    int? Count,
    IReadOnlyList<decimal> ValuesMmHg,
    IReadOnlyList<decimal> ValuesKpa,
    decimal? AverageMmHg,
    decimal? AverageKpa,
    string SourceSegment);

public sealed record NidekRs232PachymetryMeasurement(
    NidekRs232Eye Eye,
    int? Count,
    IReadOnlyList<int> ValuesMicrometer,
    int? AverageMicrometer,
    string SourceSegment);

public sealed record NidekRs232ErrorSegment(
    string Code,
    string RawText,
    NidekRs232Eye Eye = NidekRs232Eye.Unknown);

public sealed record NidekRs232MedistarCandidate(
    string FieldCode,
    string Description,
    string PreviewText);

public sealed record NidekRs232ParsedPayload(
    string DeviceFamily,
    string DeviceCode,
    string? Manufacturer,
    string? Model,
    string? PatientOrPrintNumber,
    string? PatientId,
    DateTime? MeasurementDateTime,
    IReadOnlyList<NidekRs232Segment> RawSegments,
    IReadOnlyList<NidekRs232RefractionMeasurement> RefractionMeasurements,
    IReadOnlyList<NidekRs232TonometryMeasurement> TonometryMeasurements,
    IReadOnlyList<NidekRs232PachymetryMeasurement> PachymetryMeasurements,
    IReadOnlyList<NidekRs232ErrorSegment> Errors,
    IReadOnlyList<NidekRs232MedistarCandidate> MedistarCandidates,
    IReadOnlyList<string> Warnings);

public static class NidekRs232CommunicationPresets
{
    public static SerialCommunicationSettings CreateNtPreset(string? portName = null)
    {
        return SerialCommunicationSettings.Default with
        {
            PortName = portName,
            BaudRate = 9600,
            DataBits = 8,
            StopBits = SerialStopBitsSetting.One,
            Parity = SerialParitySetting.Odd,
            Handshake = SerialHandshakeSetting.None,
            LineTerminator = SerialLineTerminatorSetting.CR
        };
    }

    public static SerialCommunicationSettings CreateLm7Preset(string? portName = null)
    {
        return SerialCommunicationSettings.Default with
        {
            PortName = portName,
            BaudRate = 9600,
            DataBits = 8,
            StopBits = SerialStopBitsSetting.One,
            Parity = SerialParitySetting.None,
            Handshake = SerialHandshakeSetting.None,
            LineTerminator = SerialLineTerminatorSetting.CR
        };
    }

    public static SerialCommunicationSettings CreateNcp10ListenPreset(string? portName = null)
    {
        return CreateNtPreset(portName) with
        {
            IsBidirectional = false,
            Handshake = SerialHandshakeSetting.None
        };
    }

    public static SerialCommunicationSettings CreateRt2100Type1Preset(string? portName = null)
    {
        return CreateRtType1Preset(portName);
    }

    public static SerialCommunicationSettings CreateRt3100Type1Preset(string? portName = null)
    {
        return CreateRtType1Preset(portName);
    }

    public static SerialCommunicationSettings CreateRt3100Type2Preset(string? portName = null)
    {
        return CreateRtType2Preset(portName);
    }

    public static SerialCommunicationSettings CreateRt5100Type1Preset(string? portName = null)
    {
        return CreateRtType1Preset(portName);
    }

    public static SerialCommunicationSettings CreateRt5100Type2Preset(string? portName = null)
    {
        return CreateRtType2Preset(portName);
    }

    private static SerialCommunicationSettings CreateRtType1Preset(string? portName)
    {
        return SerialCommunicationSettings.Default with
        {
            PortName = portName,
            BaudRate = 2400,
            DataBits = 7,
            StopBits = SerialStopBitsSetting.Two,
            Parity = SerialParitySetting.Even,
            Handshake = SerialHandshakeSetting.None,
            DtrEnable = true,
            RtsEnable = true,
            IsBidirectional = true,
            LineTerminator = SerialLineTerminatorSetting.CR
        };
    }

    private static SerialCommunicationSettings CreateRtType2Preset(string? portName)
    {
        return SerialCommunicationSettings.Default with
        {
            PortName = portName,
            BaudRate = 9600,
            DataBits = 8,
            StopBits = SerialStopBitsSetting.One,
            Parity = SerialParitySetting.Odd,
            Handshake = SerialHandshakeSetting.None,
            DtrEnable = true,
            RtsEnable = true,
            IsBidirectional = true,
            LineTerminator = SerialLineTerminatorSetting.CR
        };
    }
}
