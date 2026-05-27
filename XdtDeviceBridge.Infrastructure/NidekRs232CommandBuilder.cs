using System.Text;

namespace XdtDeviceBridge.Infrastructure;

public sealed class NidekRs232CommandBuilder
{
    private static readonly Encoding Ascii = Encoding.ASCII;

    public byte[] BuildRsBroadcastCommand(bool appendCr = true)
    {
        return BuildRawCommand(string.Empty, "RS", appendCr);
    }

    public byte[] BuildLmSendDataCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "SD", appendCr);
    }

    public byte[] BuildLmReadCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "RD", appendCr);
    }

    public byte[] BuildLmReadAndSendCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "RSD", appendCr);
    }

    public byte[] BuildLmClearCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "CL", appendCr);
    }

    public byte[] BuildLmSetAbbeCommand(int abbeNumber, bool appendCr = true)
    {
        if (abbeNumber is < 0 or > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(abbeNumber), "AB-Wert muss zwischen 0 und 99 liegen.");
        }

        return BuildRawCommand("LM", $"AB{abbeNumber:00}", appendCr);
    }

    public byte[] BuildLmSelectLensCommand(char lens, bool appendCr = true)
    {
        var normalized = char.ToUpperInvariant(lens);
        if (normalized is not ('S' or 'R' or 'L'))
        {
            throw new ArgumentException("LM-Linsenauswahl muss S, R oder L sein.", nameof(lens));
        }

        return BuildRawCommand("LM", $"CH{normalized}", appendCr);
    }

    public byte[] BuildLmChangeMeasureScreenCommand(char screen, bool appendCr = true)
    {
        var normalized = char.ToUpperInvariant(screen);
        if (normalized is not ('A' or 'N' or 'P' or 'C'))
        {
            throw new ArgumentException("LM-Messbildschirm muss A, N, P oder C sein.", nameof(screen));
        }

        return BuildRawCommand("LM", $"MD{normalized}", appendCr);
    }

    public byte[] BuildLmUvReadCommand(char target, bool appendCr = true)
    {
        return BuildRawCommand("LM", $"URD{char.ToUpperInvariant(target)}", appendCr);
    }

    public byte[] BuildLmUvWithSendCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "URS", appendCr);
    }

    public byte[] BuildLmInitializeCommand(char mode, bool appendCr = true)
    {
        return BuildRawCommand("LM", $"IZ{char.ToUpperInvariant(mode)}", appendCr);
    }

    public byte[] BuildLmAlignmentInfoCommand(bool appendCr = true)
    {
        return BuildRawCommand("LM", "NSD", appendCr);
    }

    public byte[] BuildNtSendDataCommand(bool appendCr = true)
    {
        return BuildRawCommand("Nt", "SD", appendCr);
    }

    public byte[] BuildNtClearCommand(bool appendCr = true)
    {
        return BuildRawCommand("Nt", "CL", appendCr);
    }

    public byte[] BuildNtPachymetrySendDataCommand(bool appendCr = true)
    {
        return BuildRawCommand("PM", "SD", appendCr);
    }

    public byte[] BuildNtPachymetryClearCommand(bool appendCr = true)
    {
        return BuildRawCommand("PM", "CL", appendCr);
    }

    public byte[] BuildNtAndPachymetrySendDataCommand(bool appendCr = true)
    {
        return BuildRawCommand("NP", "SD", appendCr);
    }

    public byte[] BuildNtAndPachymetryClearCommand(bool appendCr = true)
    {
        return BuildRawCommand("NP", "CL", appendCr);
    }

    public byte[] BuildRawCommand(string deviceCode, string command, bool appendCr = true)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var bytes = new List<byte>
        {
            NidekRs232ControlBytes.SOH
        };
        bytes.AddRange(Ascii.GetBytes("C"));
        bytes.AddRange(Ascii.GetBytes(deviceCode ?? string.Empty));
        bytes.Add(NidekRs232ControlBytes.STX);
        bytes.AddRange(Ascii.GetBytes(command));
        bytes.Add(NidekRs232ControlBytes.ETB);
        bytes.Add(NidekRs232ControlBytes.EOT);
        if (appendCr)
        {
            bytes.Add(NidekRs232ControlBytes.CR);
        }

        return bytes.ToArray();
    }
}
