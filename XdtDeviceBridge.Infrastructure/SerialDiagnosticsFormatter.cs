using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public static class SerialDiagnosticsFormatter
{
    public static string FormatSettings(SerialCommunicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var portName = string.IsNullOrWhiteSpace(settings.PortName) ? "COM-Port fehlt" : settings.PortName;
        return string.Join(
            ", ",
            portName,
            $"{settings.BaudRate} Baud",
            $"{settings.DataBits} Datenbits",
            $"Parität {settings.Parity}",
            $"Stoppbits {settings.StopBits}",
            $"Handshake {settings.Handshake}",
            $"DTR {(settings.DtrEnable ? "aktiv" : "aus")}",
            $"RTS {(settings.RtsEnable ? "aktiv" : "aus")}",
            $"ReadTimeout {settings.ReadTimeoutMilliseconds} ms",
            $"WriteTimeout {settings.WriteTimeoutMilliseconds} ms");
    }

    public static string ToHexDump(IReadOnlyList<byte> bytes)
    {
        return bytes.Count == 0
            ? string.Empty
            : string.Join(" ", bytes.Select(value => value.ToString("X2")));
    }

    public static string FormatModemStatus(SerialModemStatus? status)
    {
        if (status is null)
        {
            return "CTS nicht verfügbar, DSR nicht verfügbar, DCD nicht verfügbar, RI nicht verfügbar";
        }

        return string.Join(
            ", ",
            $"CTS {FormatSignal(status.Cts)}",
            $"DSR {FormatSignal(status.Dsr)}",
            $"DCD {FormatSignal(status.CarrierDetect)}",
            $"RI {FormatSignal(status.RingIndicator)}");
    }

    public static string ToVisibleControlText(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(bytes.Count);
        foreach (var value in bytes)
        {
            builder.Append(value switch
            {
                0x01 => "<SOH>",
                0x02 => "<STX>",
                0x04 => "<EOT>",
                0x0A => "<LF>",
                0x0D => "<CR>",
                0x17 => "<ETB>",
                >= 0x20 and <= 0x7E => ((char)value).ToString(),
                _ => $"<0x{value:X2}>"
            });
        }

        return builder.ToString();
    }

    public static string CreateNoRtDataTroubleshooting(string deviceDisplayName)
    {
        var device = string.IsNullOrWhiteSpace(deviceDisplayName) ? "RT-Phoropter" : deviceDisplayName.Trim();
        return $"""
            Keine Daten vom {device} empfangen.
            Bitte prüfen:
            1. Ist der richtige COM-Port gewählt?
            2. Stimmen Baudrate, Datenbits, Parität und Stopbits?
            3. Ist am {device} der PC-Port auf PC gestellt?
            4. Ist Communication Sequence Type1 oder Type2 passend eingestellt?
            5. Sind DTR/RTS/Handshake korrekt gesetzt?
            6. Ist der Port durch ein anderes Programm belegt?
            7. Wurde am Phoropter PRINT/SEND ausgelöst?
            """;
    }

    private static string FormatSignal(bool? value)
    {
        return value switch
        {
            true => "aktiv",
            false => "aus",
            _ => "nicht verfügbar"
        };
    }
}
