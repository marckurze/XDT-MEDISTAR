using System.Globalization;
using System.Text;

namespace XdtDeviceBridge.Infrastructure;

public sealed class NidekRs232FrameReader
{
    private static readonly Encoding Ascii = Encoding.ASCII;

    public NidekRs232FrameReadResult Read(
        byte[] bytes,
        NidekRs232CommunicationMode mode = NidekRs232CommunicationMode.Unknown)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var frames = new List<NidekRs232Frame>();
        var warnings = new List<string>();
        var noise = new List<byte>();
        var partial = Array.Empty<byte>();
        var index = 0;

        while (index < bytes.Length)
        {
            var sohIndex = Array.IndexOf(bytes, NidekRs232ControlBytes.SOH, index);
            if (sohIndex < 0)
            {
                noise.AddRange(bytes[index..]);
                break;
            }

            if (sohIndex > index)
            {
                noise.AddRange(bytes[index..sohIndex]);
            }

            var eotIndex = Array.IndexOf(bytes, NidekRs232ControlBytes.EOT, sohIndex + 1);
            if (eotIndex < 0)
            {
                partial = bytes[sohIndex..];
                warnings.Add("Unvollständiger NIDEK-RS232-Frame: EOT fehlt.");
                break;
            }

            var checksumStart = eotIndex + 1;
            var afterChecksum = checksumStart;
            string? checksumText = null;
            bool? checksumValid = null;
            var hasChecksum = HasFourAsciiHexBytes(bytes, checksumStart);
            if (hasChecksum)
            {
                checksumText = Ascii.GetString(bytes, checksumStart, 4);
                checksumValid = VerifyChecksum(bytes, sohIndex, eotIndex, checksumText);
                afterChecksum += 4;
            }

            var hasTrailingCr = false;
            if (afterChecksum < bytes.Length && bytes[afterChecksum] == NidekRs232ControlBytes.CR)
            {
                hasTrailingCr = true;
                afterChecksum++;
            }

            if (afterChecksum < bytes.Length && bytes[afterChecksum] == NidekRs232ControlBytes.LF)
            {
                afterChecksum++;
            }

            var rawBytes = bytes[sohIndex..afterChecksum];
            frames.Add(CreateFrame(bytes, sohIndex, eotIndex, rawBytes, hasChecksum, checksumText, checksumValid, hasTrailingCr, mode));
            index = afterChecksum;
        }

        if (noise.Count > 0)
        {
            warnings.Add($"{noise.Count} Byte Noise vor oder zwischen NIDEK-Frames ignoriert.");
        }

        return new NidekRs232FrameReadResult(frames, noise.ToArray(), partial, warnings);
    }

    private static NidekRs232Frame CreateFrame(
        byte[] source,
        int sohIndex,
        int eotIndex,
        byte[] rawBytes,
        bool hasChecksum,
        string? checksumText,
        bool? checksumValid,
        bool hasTrailingCr,
        NidekRs232CommunicationMode mode)
    {
        var warnings = new List<string>();
        var stxIndex = Array.IndexOf(source, NidekRs232ControlBytes.STX, sohIndex + 1, eotIndex - sohIndex);
        if (stxIndex < 0)
        {
            warnings.Add("NIDEK-RS232-Frame enthält kein STX.");
            return new NidekRs232Frame(
                rawBytes,
                Ascii.GetString(rawBytes),
                NidekRs232FrameKind.Unknown,
                string.Empty,
                string.Empty,
                Array.Empty<string>(),
                hasChecksum,
                checksumText,
                checksumValid,
                hasTrailingCr,
                warnings);
        }

        var header = Ascii.GetString(source, sohIndex + 1, stxIndex - sohIndex - 1);
        var dataStart = stxIndex + 1;
        var dataBytes = source[dataStart..eotIndex];
        var segments = SplitSegments(dataBytes);
        if (segments.Count == 0)
        {
            warnings.Add("NIDEK-RS232-Frame enthält keine Datensegmente.");
        }

        if (mode == NidekRs232CommunicationMode.Ncp10 && !hasChecksum)
        {
            warnings.Add("NCP10-Frame enthält keine 4-stellige Checksumme.");
        }

        if (checksumValid == false)
        {
            warnings.Add("NIDEK-RS232-Checksumme ist ungültig.");
        }

        return new NidekRs232Frame(
            rawBytes,
            Ascii.GetString(rawBytes),
            DetermineKind(header),
            header,
            DetermineDeviceCode(header),
            segments,
            hasChecksum,
            checksumText,
            checksumValid,
            hasTrailingCr,
            warnings);
    }

    private static List<string> SplitSegments(byte[] dataBytes)
    {
        var segments = new List<string>();
        var segmentStart = 0;
        for (var i = 0; i < dataBytes.Length; i++)
        {
            if (dataBytes[i] != NidekRs232ControlBytes.ETB)
            {
                continue;
            }

            segments.Add(TrimSegmentTerminator(Ascii.GetString(dataBytes, segmentStart, i - segmentStart)));
            segmentStart = i + 1;
            if (segmentStart < dataBytes.Length && dataBytes[segmentStart] == NidekRs232ControlBytes.CR)
            {
                segmentStart++;
            }
        }

        if (segmentStart < dataBytes.Length)
        {
            segments.Add(TrimSegmentTerminator(Ascii.GetString(dataBytes, segmentStart, dataBytes.Length - segmentStart)));
        }

        return segments.Where(segment => segment.Length > 0).ToList();
    }

    private static string TrimSegmentTerminator(string segment)
    {
        return segment.TrimEnd('\r', '\n');
    }

    private static NidekRs232FrameKind DetermineKind(string header)
    {
        if (string.IsNullOrEmpty(header))
        {
            return NidekRs232FrameKind.Unknown;
        }

        return header[0] switch
        {
            'C' => NidekRs232FrameKind.Command,
            'D' => NidekRs232FrameKind.Data,
            'S' => NidekRs232FrameKind.Success,
            'E' => NidekRs232FrameKind.Error,
            _ => NidekRs232FrameKind.Unknown
        };
    }

    private static string DetermineDeviceCode(string header)
    {
        if (header.Length <= 1)
        {
            return string.Empty;
        }

        return header[1..];
    }

    private static bool HasFourAsciiHexBytes(byte[] bytes, int startIndex)
    {
        if (startIndex + 4 > bytes.Length)
        {
            return false;
        }

        return IsAsciiHex(bytes[startIndex])
            && IsAsciiHex(bytes[startIndex + 1])
            && IsAsciiHex(bytes[startIndex + 2])
            && IsAsciiHex(bytes[startIndex + 3]);
    }

    private static bool IsAsciiHex(byte value)
    {
        return value is >= (byte)'0' and <= (byte)'9'
            || value is >= (byte)'A' and <= (byte)'F'
            || value is >= (byte)'a' and <= (byte)'f';
    }

    public static string CalculateChecksumText(IReadOnlyList<byte> bytesFromSohThroughEot)
    {
        ArgumentNullException.ThrowIfNull(bytesFromSohThroughEot);

        var sum = 0;
        foreach (var value in bytesFromSohThroughEot)
        {
            sum = (sum + value) & 0xFFFF;
        }

        return sum.ToString("X4", CultureInfo.InvariantCulture);
    }

    private static bool VerifyChecksum(byte[] bytes, int sohIndex, int eotIndex, string checksumText)
    {
        var expected = CalculateChecksumText(bytes[sohIndex..(eotIndex + 1)]);
        return string.Equals(expected, checksumText, StringComparison.OrdinalIgnoreCase);
    }
}
