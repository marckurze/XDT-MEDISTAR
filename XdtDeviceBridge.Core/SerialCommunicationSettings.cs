namespace XdtDeviceBridge.Core;

public enum SerialStopBitsSetting
{
    One,
    OnePointFive,
    Two
}

public enum SerialParitySetting
{
    None,
    Odd,
    Even,
    Mark,
    Space
}

public enum SerialHandshakeSetting
{
    None,
    RequestToSend,
    XOnXOff,
    RequestToSendXOnXOff
}

public enum SerialLineTerminatorSetting
{
    None,
    CR,
    LF,
    CRLF
}

public sealed record SerialCommunicationSettings(
    string? PortName = null,
    int BaudRate = 9600,
    int DataBits = 8,
    SerialStopBitsSetting StopBits = SerialStopBitsSetting.One,
    SerialParitySetting Parity = SerialParitySetting.None,
    SerialHandshakeSetting Handshake = SerialHandshakeSetting.None,
    bool DtrEnable = false,
    bool RtsEnable = true,
    bool IsBidirectional = false,
    string? InitialCommand = null,
    SerialLineTerminatorSetting LineTerminator = SerialLineTerminatorSetting.CRLF,
    int ReadTimeoutMilliseconds = 1000,
    int WriteTimeoutMilliseconds = 1000)
{
    public static SerialCommunicationSettings Default { get; } = new();
}
