using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class SerialDeviceCommunicationService : ISerialDeviceCommunicationService
{
    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint OpenExisting = 3;

    public Task<SerialCommunicationSessionResult> ListenAsync(
        SerialCommunicationSettings settings,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var startedAt = DateTimeOffset.UtcNow;
        var validationIssue = ValidateSettings(settings, requirePortName: true);
        if (validationIssue is not null)
        {
            return Task.FromResult(CreateListenFailure(settings, startedAt, validationIssue));
        }

        if (duration <= TimeSpan.Zero)
        {
            return Task.FromResult(CreateListenFailure(settings, startedAt, "Die Mitschnittdauer muss größer als 0 sein."));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(CreateListenFailure(settings, startedAt, "Vorgang wurde abgebrochen."));
        }

        return Task.Run(() => ListenCore(settings, duration, startedAt, cancellationToken), CancellationToken.None);
    }

    public Task<SerialCommunicationWriteResult> WriteAsync(
        SerialCommunicationSettings settings,
        string command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validationIssue = ValidateSettings(settings, requirePortName: true);
        if (validationIssue is not null)
        {
            return Task.FromResult(new SerialCommunicationWriteResult(false, validationIssue, settings.PortName ?? string.Empty, 0));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(new SerialCommunicationWriteResult(false, "Vorgang wurde abgebrochen.", settings.PortName!, 0));
        }

        return Task.Run(() =>
        {
            try
            {
                using var handle = OpenConfiguredPort(settings);
                using var stream = new FileStream(handle, FileAccess.ReadWrite, bufferSize: 4096, isAsync: false);
                var text = (command ?? string.Empty) + CreateTerminator(settings.LineTerminator);
                var bytes = Encoding.ASCII.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();

                return new SerialCommunicationWriteResult(true, null, settings.PortName!, bytes.Length);
            }
            catch (Exception ex) when (IsExpectedSerialException(ex))
            {
                return new SerialCommunicationWriteResult(false, CreateFriendlyErrorMessage(settings.PortName!, ex), settings.PortName!, 0);
            }
        }, CancellationToken.None);
    }

    public Task<SerialCommunicationExchangeResult> ExchangeAsync(
        SerialCommunicationSettings settings,
        SerialCommunicationExchangeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(request);

        var validationIssue = ValidateSettings(settings, requirePortName: true)
            ?? ValidateExchangeRequest(request);
        if (validationIssue is not null)
        {
            return Task.FromResult(new SerialCommunicationExchangeResult(
                Success: false,
                ErrorMessage: validationIssue,
                PortName: settings.PortName ?? string.Empty,
                BytesWritten: 0,
                HandshakeBytes: Array.Empty<byte>(),
                ReceivedBytes: Array.Empty<byte>(),
                RawText: string.Empty,
                HexDump: string.Empty,
                Messages: new[] { validationIssue }));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(new SerialCommunicationExchangeResult(
                Success: false,
                ErrorMessage: "Vorgang wurde abgebrochen.",
                PortName: settings.PortName!,
                BytesWritten: 0,
                HandshakeBytes: Array.Empty<byte>(),
                ReceivedBytes: Array.Empty<byte>(),
                RawText: string.Empty,
                HexDump: string.Empty,
                Messages: new[] { "Vorgang wurde abgebrochen." }));
        }

        return Task.Run(() => ExchangeCore(settings, request, cancellationToken), CancellationToken.None);
    }

    public static string? ValidateSettings(SerialCommunicationSettings settings, bool requirePortName)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (requirePortName && string.IsNullOrWhiteSpace(settings.PortName))
        {
            return "Bitte einen COM-Port auswählen.";
        }

        if (settings.BaudRate <= 0)
        {
            return "Baudrate muss größer als 0 sein.";
        }

        if (settings.DataBits is < 5 or > 8)
        {
            return "Datenbits müssen zwischen 5 und 8 liegen.";
        }

        if (!Enum.IsDefined(settings.StopBits))
        {
            return "Stoppbits enthalten einen ungültigen Wert.";
        }

        if (!Enum.IsDefined(settings.Parity))
        {
            return "Parität enthält einen ungültigen Wert.";
        }

        if (!Enum.IsDefined(settings.Handshake))
        {
            return "Flusskontrolle enthält einen ungültigen Wert.";
        }

        if (!Enum.IsDefined(settings.LineTerminator))
        {
            return "Zeilenende enthält einen ungültigen Wert.";
        }

        if (settings.ReadTimeoutMilliseconds < 0)
        {
            return "ReadTimeout darf nicht negativ sein.";
        }

        if (settings.WriteTimeoutMilliseconds < 0)
        {
            return "WriteTimeout darf nicht negativ sein.";
        }

        return null;
    }

    private static SerialCommunicationSessionResult ListenCore(
        SerialCommunicationSettings settings,
        TimeSpan duration,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            using var handle = OpenConfiguredPort(settings);
            using var stream = new FileStream(handle, FileAccess.ReadWrite, bufferSize: 4096, isAsync: false);
            using var memory = new MemoryStream();
            var buffer = new byte[4096];
            var endAt = DateTime.UtcNow + duration;

            while (DateTime.UtcNow < endAt && !cancellationToken.IsCancellationRequested)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    memory.Write(buffer, 0, read);
                    continue;
                }

                Thread.Sleep(25);
            }

            var bytes = memory.ToArray();
            var rawText = DecodeText(bytes);
            var finishedAt = DateTimeOffset.UtcNow;
            if (bytes.Length == 0 && cancellationToken.IsCancellationRequested)
            {
                return new SerialCommunicationSessionResult(
                    false,
                    "Vorgang wurde abgebrochen.",
                    settings.PortName!,
                    startedAt,
                    finishedAt,
                    0,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>());
            }

            if (bytes.Length == 0)
            {
                return new SerialCommunicationSessionResult(
                    false,
                    "Keine Daten empfangen. Bitte Gerät, Kabel, COM-Port und Kommunikationsparameter prüfen.",
                    settings.PortName!,
                    startedAt,
                    finishedAt,
                    0,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>());
            }

            return new SerialCommunicationSessionResult(
                true,
                null,
                settings.PortName!,
                startedAt,
                finishedAt,
                bytes.Length,
                rawText,
                CreateHexDump(bytes),
                SplitLines(rawText));
        }
        catch (Exception ex) when (IsExpectedSerialException(ex))
        {
            return CreateListenFailure(settings, startedAt, CreateFriendlyErrorMessage(settings.PortName!, ex));
        }
    }

    private static SerialCommunicationExchangeResult ExchangeCore(
        SerialCommunicationSettings settings,
        SerialCommunicationExchangeRequest request,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>
        {
            $"COM-Einstellungen: {SerialDiagnosticsFormatter.FormatSettings(settings)}"
        };

        try
        {
            using var handle = OpenConfiguredPort(settings);
            using var stream = new FileStream(handle, FileAccess.ReadWrite, bufferSize: 4096, isAsync: false);
            messages.Add($"COM-Port geöffnet: {settings.PortName}.");

            var bytesWritten = 0;
            if (request.RequestBytes.Length > 0)
            {
                messages.Add($"RS-Anforderung gesendet: {SerialDiagnosticsFormatter.ToVisibleControlText(request.RequestBytes)}");
                messages.Add($"RS-Hexdump: {SerialDiagnosticsFormatter.ToHexDump(request.RequestBytes)}");
                stream.Write(request.RequestBytes, 0, request.RequestBytes.Length);
                stream.Flush();
                bytesWritten += request.RequestBytes.Length;
            }

            var handshakeBytes = Array.Empty<byte>();
            if (request.ExpectedHandshakeBytes.Length > 0)
            {
                messages.Add($"Warte auf SD-Bestätigung, Marker: {SerialDiagnosticsFormatter.ToVisibleControlText(request.ExpectedHandshakeBytes)}.");
                handshakeBytes = ReadUntil(
                    stream,
                    buffer => ContainsSequence(buffer, request.ExpectedHandshakeBytes),
                    request.HandshakeTimeout,
                    request.MaxReceiveBytes,
                    stableAfterMatch: TimeSpan.Zero,
                    cancellationToken);

                if (!ContainsSequence(handshakeBytes, request.ExpectedHandshakeBytes))
                {
                    var message = "Keine RT-Antwort auf RS-Anforderung empfangen oder SD-Bestätigung fehlt.";
                    messages.Add(message);
                    if (handshakeBytes.Length == 0)
                    {
                        messages.Add("Keine Bytes empfangen.");
                        messages.Add("Empfangene SD-Antwort: keine Bytes.");
                    }
                    else
                    {
                        messages.Add("Bytes empfangen, aber keine SD-Bestätigung erkannt.");
                        messages.Add($"Empfangene SD-Antwort: {SerialDiagnosticsFormatter.ToVisibleControlText(handshakeBytes)}");
                        messages.Add($"SD-Hexdump: {CreateHexDump(handshakeBytes)}");
                    }

                    return new SerialCommunicationExchangeResult(
                        Success: false,
                        ErrorMessage: message,
                        PortName: settings.PortName!,
                        BytesWritten: bytesWritten,
                        HandshakeBytes: handshakeBytes,
                        ReceivedBytes: Array.Empty<byte>(),
                        RawText: string.Empty,
                        HexDump: CreateHexDump(handshakeBytes),
                        Messages: messages);
                }

                messages.Add($"SD-Bestätigung empfangen: {SerialDiagnosticsFormatter.ToVisibleControlText(handshakeBytes)}");
                messages.Add($"SD-Hexdump: {CreateHexDump(handshakeBytes)}");
            }

            if (request.PayloadBytes.Length > 0)
            {
                messages.Add($"Sendeframe gesendet: {request.PayloadBytes.Length} Bytes.");
                messages.Add($"Sendeframe sichtbar: {SerialDiagnosticsFormatter.ToVisibleControlText(request.PayloadBytes)}");
                messages.Add($"Sendeframe-Hexdump: {CreateHexDump(request.PayloadBytes)}");
                stream.Write(request.PayloadBytes, 0, request.PayloadBytes.Length);
                stream.Flush();
                bytesWritten += request.PayloadBytes.Length;
            }

            messages.Add($"Warte auf Rückgabe bis EOT 0x{request.EndOfTransmissionByte:X2}, Timeout {request.ReceiveTimeout.TotalSeconds:0} s.");
            var receivedBytes = ReadUntil(
                stream,
                buffer => buffer.Contains(request.EndOfTransmissionByte),
                request.ReceiveTimeout,
                request.MaxReceiveBytes,
                request.StableAfterEndOfTransmission,
                cancellationToken);

            if (receivedBytes.Length == 0)
            {
                var message = "Keine Rückgabe vom Phoropter empfangen.";
                messages.Add(message);
                return new SerialCommunicationExchangeResult(
                    Success: false,
                    ErrorMessage: message,
                    PortName: settings.PortName!,
                    BytesWritten: bytesWritten,
                    HandshakeBytes: handshakeBytes,
                    ReceivedBytes: Array.Empty<byte>(),
                    RawText: string.Empty,
                    HexDump: string.Empty,
                    Messages: messages);
            }

            if (!receivedBytes.Contains(request.EndOfTransmissionByte))
            {
                var message = "Rückgabe vom Phoropter war unvollständig: EOT wurde nicht empfangen.";
                messages.Add(message);
                messages.Add($"Empfangene Rückgabe sichtbar: {SerialDiagnosticsFormatter.ToVisibleControlText(receivedBytes)}");
                messages.Add($"Empfangene Rückgabe Hexdump: {CreateHexDump(receivedBytes)}");
                return new SerialCommunicationExchangeResult(
                    Success: false,
                    ErrorMessage: message,
                    PortName: settings.PortName!,
                    BytesWritten: bytesWritten,
                    HandshakeBytes: handshakeBytes,
                    ReceivedBytes: receivedBytes,
                    RawText: DecodeText(receivedBytes),
                    HexDump: CreateHexDump(receivedBytes),
                    Messages: messages);
            }

            messages.Add("EOT empfangen; Stabilitätswartezeit abgeschlossen.");
            messages.Add($"Rückgabe vollständig: {receivedBytes.Length} Bytes.");
            messages.Add($"Empfangene Rückgabe sichtbar: {SerialDiagnosticsFormatter.ToVisibleControlText(receivedBytes)}");
            messages.Add($"Empfangene Rückgabe Hexdump: {CreateHexDump(receivedBytes)}");
            return new SerialCommunicationExchangeResult(
                Success: true,
                ErrorMessage: null,
                PortName: settings.PortName!,
                BytesWritten: bytesWritten,
                HandshakeBytes: handshakeBytes,
                ReceivedBytes: receivedBytes,
                RawText: DecodeText(receivedBytes),
                HexDump: CreateHexDump(receivedBytes),
                Messages: messages);
        }
        catch (Exception ex) when (IsExpectedSerialException(ex))
        {
            var message = CreateFriendlyErrorMessage(settings.PortName!, ex);
            messages.Add(message);
            return new SerialCommunicationExchangeResult(
                Success: false,
                ErrorMessage: message,
                PortName: settings.PortName!,
                BytesWritten: 0,
                HandshakeBytes: Array.Empty<byte>(),
                ReceivedBytes: Array.Empty<byte>(),
                RawText: string.Empty,
                HexDump: string.Empty,
                Messages: messages);
        }
    }

    private static string? ValidateExchangeRequest(SerialCommunicationExchangeRequest request)
    {
        if (request.HandshakeTimeout <= TimeSpan.Zero)
        {
            return "Handshake-Timeout muss größer als 0 sein.";
        }

        if (request.ReceiveTimeout <= TimeSpan.Zero)
        {
            return "Empfangs-Timeout muss größer als 0 sein.";
        }

        if (request.StableAfterEndOfTransmission < TimeSpan.Zero)
        {
            return "Stabilitätswartezeit darf nicht negativ sein.";
        }

        if (request.MaxReceiveBytes <= 0)
        {
            return "Maximale Empfangsgröße muss größer als 0 sein.";
        }

        return null;
    }

    private static byte[] ReadUntil(
        FileStream stream,
        Func<byte[], bool> isComplete,
        TimeSpan timeout,
        int maxReceiveBytes,
        TimeSpan stableAfterMatch,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        var buffer = new byte[4096];
        var endAt = DateTime.UtcNow + timeout;
        DateTime? completedAt = null;

        while (DateTime.UtcNow < endAt && !cancellationToken.IsCancellationRequested)
        {
            var read = stream.Read(buffer, 0, Math.Min(buffer.Length, Math.Max(1, maxReceiveBytes - (int)memory.Length)));
            if (read > 0)
            {
                memory.Write(buffer, 0, read);
                if (memory.Length >= maxReceiveBytes)
                {
                    break;
                }

                var snapshot = memory.ToArray();
                if (isComplete(snapshot))
                {
                    completedAt = DateTime.UtcNow;
                    if (stableAfterMatch == TimeSpan.Zero)
                    {
                        break;
                    }
                }
            }
            else if (completedAt is not null && DateTime.UtcNow - completedAt >= stableAfterMatch)
            {
                break;
            }
            else
            {
                Thread.Sleep(25);
            }
        }

        return memory.ToArray();
    }

    private static SafeFileHandle OpenConfiguredPort(SerialCommunicationSettings settings)
    {
        var handle = CreateFile(
            @"\\.\" + settings.PortName,
            GenericRead | GenericWrite,
            shareMode: 0,
            securityAttributes: IntPtr.Zero,
            creationDisposition: OpenExisting,
            flagsAndAttributes: 0,
            templateFile: IntPtr.Zero);

        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            throw new IOException(new Win32Exception(error).Message, new Win32Exception(error));
        }

        ConfigurePort(handle, settings);
        return handle;
    }

    private static void ConfigurePort(SafeFileHandle handle, SerialCommunicationSettings settings)
    {
        var dcb = new Dcb
        {
            DCBlength = (uint)Marshal.SizeOf<Dcb>()
        };

        if (!GetCommState(handle, ref dcb))
        {
            ThrowLastWin32("COM-Port-Status konnte nicht gelesen werden.");
        }

        dcb.BaudRate = (uint)settings.BaudRate;
        dcb.ByteSize = (byte)settings.DataBits;
        dcb.Parity = MapParity(settings.Parity);
        dcb.StopBits = MapStopBits(settings.StopBits);
        dcb.Flags = ConfigureDcbFlags(dcb.Flags, settings);

        if (!SetCommState(handle, ref dcb))
        {
            ThrowLastWin32("COM-Port-Parameter konnten nicht gesetzt werden.");
        }

        var timeouts = new CommTimeouts
        {
            ReadIntervalTimeout = 50,
            ReadTotalTimeoutConstant = (uint)Math.Max(settings.ReadTimeoutMilliseconds, 1),
            ReadTotalTimeoutMultiplier = 0,
            WriteTotalTimeoutConstant = (uint)Math.Max(settings.WriteTimeoutMilliseconds, 1),
            WriteTotalTimeoutMultiplier = 0
        };

        if (!SetCommTimeouts(handle, ref timeouts))
        {
            ThrowLastWin32("COM-Port-Timeouts konnten nicht gesetzt werden.");
        }
    }

    private static uint ConfigureDcbFlags(uint flags, SerialCommunicationSettings settings)
    {
        const uint binary = 0x00000001;
        const uint parity = 0x00000002;
        const uint outxCtsFlow = 0x00000004;
        const uint outX = 0x00000100;
        const uint inX = 0x00000200;
        const uint dtrControlMask = 0x00000030;
        const uint dtrControlEnable = 0x00000010;
        const uint rtsControlMask = 0x00003000;
        const uint rtsControlEnable = 0x00001000;
        const uint rtsControlHandshake = 0x00002000;

        flags |= binary;
        flags = settings.Parity == SerialParitySetting.None
            ? flags & ~parity
            : flags | parity;

        flags &= ~(outxCtsFlow | outX | inX | dtrControlMask | rtsControlMask);
        if (settings.DtrEnable)
        {
            flags |= dtrControlEnable;
        }

        return settings.Handshake switch
        {
            SerialHandshakeSetting.RequestToSend => flags | outxCtsFlow | rtsControlHandshake,
            SerialHandshakeSetting.XOnXOff => flags | outX | inX | (settings.RtsEnable ? rtsControlEnable : 0),
            SerialHandshakeSetting.RequestToSendXOnXOff => flags | outxCtsFlow | outX | inX | rtsControlHandshake,
            _ => settings.RtsEnable ? flags | rtsControlEnable : flags
        };
    }

    private static byte MapParity(SerialParitySetting parity)
    {
        return parity switch
        {
            SerialParitySetting.Odd => 1,
            SerialParitySetting.Even => 2,
            SerialParitySetting.Mark => 3,
            SerialParitySetting.Space => 4,
            _ => 0
        };
    }

    private static byte MapStopBits(SerialStopBitsSetting stopBits)
    {
        return stopBits switch
        {
            SerialStopBitsSetting.OnePointFive => 1,
            SerialStopBitsSetting.Two => 2,
            _ => 0
        };
    }

    private static string CreateTerminator(SerialLineTerminatorSetting terminator)
    {
        return terminator switch
        {
            SerialLineTerminatorSetting.CR => "\r",
            SerialLineTerminatorSetting.LF => "\n",
            SerialLineTerminatorSetting.CRLF => "\r\n",
            _ => string.Empty
        };
    }

    private static SerialCommunicationSessionResult CreateListenFailure(
        SerialCommunicationSettings settings,
        DateTimeOffset startedAt,
        string message)
    {
        return new SerialCommunicationSessionResult(
            false,
            message,
            settings.PortName ?? string.Empty,
            startedAt,
            DateTimeOffset.UtcNow,
            0,
            string.Empty,
            string.Empty,
            Array.Empty<string>());
    }

    private static string DecodeText(byte[] bytes)
    {
        return bytes.Length == 0 ? string.Empty : Encoding.ASCII.GetString(bytes);
    }

    private static string CreateHexDump(byte[] bytes)
    {
        return bytes.Length == 0
            ? string.Empty
            : string.Join(" ", bytes.Select(value => value.ToString("X2")));
    }

    private static IReadOnlyList<string> SplitLines(string rawText)
    {
        return rawText
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static bool ContainsSequence(byte[] buffer, byte[] expected)
    {
        if (expected.Length == 0)
        {
            return true;
        }

        if (buffer.Length < expected.Length)
        {
            return false;
        }

        for (var index = 0; index <= buffer.Length - expected.Length; index++)
        {
            var matches = true;
            for (var expectedIndex = 0; expectedIndex < expected.Length; expectedIndex++)
            {
                if (buffer[index + expectedIndex] != expected[expectedIndex])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExpectedSerialException(Exception ex)
    {
        return ex is IOException or UnauthorizedAccessException or ArgumentException or ObjectDisposedException
            || ex.InnerException is Win32Exception;
    }

    private static string CreateFriendlyErrorMessage(string portName, Exception ex)
    {
        var message = ex.InnerException is Win32Exception win32Exception
            ? win32Exception.Message
            : ex.Message;
        return $"COM-Port {portName} konnte nicht verwendet werden: {message}";
    }

    private static void ThrowLastWin32(string prefix)
    {
        var error = Marshal.GetLastWin32Error();
        throw new IOException($"{prefix} {new Win32Exception(error).Message}", new Win32Exception(error));
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetCommState(SafeFileHandle fileHandle, ref Dcb dcb);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetCommState(SafeFileHandle fileHandle, ref Dcb dcb);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetCommTimeouts(SafeFileHandle fileHandle, ref CommTimeouts timeouts);

    [StructLayout(LayoutKind.Sequential)]
    private struct Dcb
    {
        public uint DCBlength;
        public uint BaudRate;
        public uint Flags;
        public ushort wReserved;
        public ushort XonLim;
        public ushort XoffLim;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public sbyte XonChar;
        public sbyte XoffChar;
        public sbyte ErrorChar;
        public sbyte EofChar;
        public sbyte EvtChar;
        public ushort wReserved1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CommTimeouts
    {
        public uint ReadIntervalTimeout;
        public uint ReadTotalTimeoutMultiplier;
        public uint ReadTotalTimeoutConstant;
        public uint WriteTotalTimeoutMultiplier;
        public uint WriteTotalTimeoutConstant;
    }
}
