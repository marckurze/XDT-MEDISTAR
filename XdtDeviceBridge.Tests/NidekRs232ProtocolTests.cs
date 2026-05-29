using System.Globalization;
using System.Text;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class NidekRs232ProtocolTests
{
    [Fact]
    public void FrameReader_ShouldRecognizeBasicLmFrame()
    {
        var result = ReadFixture("lm-minimal.hex");

        var frame = Assert.Single(result.Frames);
        Assert.Equal(NidekRs232FrameKind.Data, frame.Kind);
        Assert.Equal("DLM", frame.Header);
        Assert.Equal("LM", frame.DeviceCode);
        Assert.Equal(new[] { "IDNIDEK/LM-7P", "  +01.00+00.00000" }, frame.Segments);
        Assert.False(frame.HasChecksum);
        Assert.True(frame.HasTrailingCr);
    }

    [Fact]
    public void FrameReader_ShouldTolerateCrAfterEtbAndSplitSegments()
    {
        var result = ReadFixture("lm-right-left.hex");

        var frame = Assert.Single(result.Frames);
        Assert.Equal(7, frame.Segments.Count);
        Assert.Contains(" R-01.25-00.75120", frame.Segments);
        Assert.Contains(" L-02.00-00.50180", frame.Segments);
        Assert.Contains("PD64.031.532.5", frame.Segments);
    }

    [Fact]
    public void FrameReader_ShouldTolerateCrLfAfterEot()
    {
        var bytes = ReadFixtureBytes("lm-minimal.hex").ToList();
        bytes.Add(NidekRs232ControlBytes.LF);

        var result = new NidekRs232FrameReader().Read(bytes.ToArray());

        var frame = Assert.Single(result.Frames);
        Assert.True(frame.HasTrailingCr);
        Assert.Empty(result.PartialBytes);
    }

    [Fact]
    public void FrameReader_ShouldIgnoreNoiseBeforeSoh()
    {
        var result = ReadFixture("noise-before-frame.hex");

        Assert.Equal(3, result.NoiseBytes.Length);
        Assert.Equal("XYZ", Encoding.ASCII.GetString(result.NoiseBytes));
        Assert.Single(result.Frames);
        Assert.Contains(result.Warnings, warning => warning.Contains("Noise", StringComparison.Ordinal));
    }

    [Fact]
    public void FrameReader_ShouldReadMultipleFramesFromOneBuffer()
    {
        var first = ReadFixtureBytes("lm-minimal.hex");
        var second = ReadFixtureBytes("nt-tonometry.hex");

        var result = new NidekRs232FrameReader().Read(first.Concat(second).ToArray());

        Assert.Equal(2, result.Frames.Count);
        Assert.Equal("DLM", result.Frames[0].Header);
        Assert.Equal("DNT", result.Frames[1].Header);
    }

    [Fact]
    public void FrameReader_ShouldReportIncompleteFrame()
    {
        var result = ReadFixture("incomplete-frame.hex");

        Assert.Empty(result.Frames);
        Assert.True(result.HasPartialFrame);
        Assert.Contains(result.Warnings, warning => warning.Contains("EOT fehlt", StringComparison.Ordinal));
    }

    [Fact]
    public void FrameReader_ShouldValidateNcp10Checksum()
    {
        var result = ReadFixture("ncp10-checksum-valid.hex", NidekRs232CommunicationMode.Ncp10);

        var frame = Assert.Single(result.Frames);
        Assert.True(frame.HasChecksum);
        Assert.Equal("096C", frame.ChecksumText);
        Assert.True(frame.ChecksumValid);
    }

    [Fact]
    public void FrameReader_ShouldReportInvalidChecksum()
    {
        var result = ReadFixture("ncp10-checksum-invalid.hex", NidekRs232CommunicationMode.Ncp10);

        var frame = Assert.Single(result.Frames);
        Assert.True(frame.HasChecksum);
        Assert.False(frame.ChecksumValid);
        Assert.Contains(frame.Warnings, warning => warning.Contains("Checksumme", StringComparison.Ordinal));
    }

    [Fact]
    public void FrameReader_ShouldAcceptChecksumWithoutTrailingCr()
    {
        var result = ReadFixture("ncp10-no-cr.hex", NidekRs232CommunicationMode.Ncp10);

        var frame = Assert.Single(result.Frames);
        Assert.True(frame.HasChecksum);
        Assert.True(frame.ChecksumValid);
        Assert.False(frame.HasTrailingCr);
    }

    [Fact]
    public void FrameReader_ShouldWarnWhenNcp10ChecksumIsMissing()
    {
        var result = ReadFixture("lm-minimal.hex", NidekRs232CommunicationMode.Ncp10);

        var frame = Assert.Single(result.Frames);
        Assert.False(frame.HasChecksum);
        Assert.Contains(frame.Warnings, warning => warning.Contains("NCP10", StringComparison.Ordinal));
    }

    [Fact]
    public void CommandBuilder_ShouldBuildLmCommands()
    {
        var builder = new NidekRs232CommandBuilder();

        Assert.Equal("01 43 4C 4D 02 53 44 17 04 0D", ToHex(builder.BuildLmSendDataCommand()));
        Assert.Equal("01 43 4C 4D 02 52 44 17 04 0D", ToHex(builder.BuildLmReadCommand()));
        Assert.Equal("01 43 4C 4D 02 52 53 44 17 04 0D", ToHex(builder.BuildLmReadAndSendCommand()));
        Assert.Equal("01 43 4C 4D 02 43 4C 17 04 0D", ToHex(builder.BuildLmClearCommand()));
    }

    [Fact]
    public void CommandBuilder_ShouldBuildNtCommands()
    {
        var builder = new NidekRs232CommandBuilder();

        Assert.Equal("01 43 4E 74 02 53 44 17 04 0D", ToHex(builder.BuildNtSendDataCommand()));
        Assert.Equal("01 43 4E 74 02 43 4C 17 04 0D", ToHex(builder.BuildNtClearCommand()));
        Assert.Equal("01 43 50 4D 02 53 44 17 04 0D", ToHex(builder.BuildNtPachymetrySendDataCommand()));
        Assert.Equal("01 43 4E 50 02 53 44 17 04 0D", ToHex(builder.BuildNtAndPachymetrySendDataCommand()));
    }

    [Fact]
    public void PayloadParser_ShouldParseLmRefractionAddAndPd()
    {
        var payload = ParseSinglePayload("lm-right-left.hex");

        Assert.Equal("LM", payload.DeviceFamily);
        Assert.Equal("NIDEK", payload.Manufacturer);
        Assert.Equal("LM-7P", payload.Model);
        Assert.Equal(new DateTime(2017, 2, 1, 15, 30, 0), payload.MeasurementDateTime);
        Assert.Equal(2, payload.RefractionMeasurements.Count);

        var right = Assert.Single(payload.RefractionMeasurements, measurement => measurement.Eye == NidekRs232Eye.Right);
        Assert.Equal(-1.25m, right.Sphere);
        Assert.Equal(-0.75m, right.Cylinder);
        Assert.Equal(120, right.Axis);
        Assert.Equal(2.00m, right.Add);
        Assert.Equal(64.0m, right.PdTotal);
        Assert.Equal(31.5m, right.PdRight);
        Assert.Equal(32.5m, right.PdLeft);

        var left = Assert.Single(payload.RefractionMeasurements, measurement => measurement.Eye == NidekRs232Eye.Left);
        Assert.Equal(-2.00m, left.Sphere);
        Assert.Equal(-0.50m, left.Cylinder);
        Assert.Equal(180, left.Axis);
        Assert.Equal(2.25m, left.Add);

        Assert.All(payload.MedistarCandidates, candidate => Assert.Equal("6228", candidate.FieldCode));
        Assert.DoesNotContain(payload.MedistarCandidates, candidate => candidate.FieldCode == "6205" || candidate.FieldCode == "6220");
    }

    [Fact]
    public void PayloadParser_ShouldParseNtTonometry()
    {
        var payload = ParseSinglePayload("nt-tonometry.hex");

        Assert.Equal("NT", payload.DeviceFamily);
        Assert.Equal("NT-530", payload.Model);
        Assert.Equal("0003", payload.PatientOrPrintNumber);
        Assert.Equal(new DateTime(2008, 11, 12, 17, 5, 0), payload.MeasurementDateTime);
        Assert.Equal(2, payload.TonometryMeasurements.Count);

        var right = Assert.Single(payload.TonometryMeasurements, measurement => measurement.Eye == NidekRs232Eye.Right);
        Assert.Equal(3, right.Count);
        Assert.Equal(new[] { 18.0m, 19.0m, 19.0m }, right.ValuesMmHg);
        Assert.Equal(18.7m, right.AverageMmHg);
        Assert.Equal(2.5m, right.AverageKpa);

        var left = Assert.Single(payload.TonometryMeasurements, measurement => measurement.Eye == NidekRs232Eye.Left);
        Assert.Equal(new[] { 13.0m, 12.0m, 14.0m }, left.ValuesMmHg);
        Assert.Equal(13.0m, left.AverageMmHg);

        Assert.All(payload.MedistarCandidates, candidate => Assert.Equal("6205", candidate.FieldCode));
        Assert.DoesNotContain(payload.MedistarCandidates, candidate => candidate.FieldCode == "6228" || candidate.FieldCode == "6220");
    }

    [Fact]
    public void PayloadParser_ShouldParsePmPachymetry()
    {
        var payload = ParseSinglePayload("pm-pachymetry.hex");

        Assert.Equal("PM", payload.DeviceFamily);
        Assert.Equal(2, payload.PachymetryMeasurements.Count);

        var right = Assert.Single(payload.PachymetryMeasurements, measurement => measurement.Eye == NidekRs232Eye.Right);
        Assert.Equal(3, right.Count);
        Assert.Equal(new[] { 519, 521, 520 }, right.ValuesMicrometer);
        Assert.Equal(520, right.AverageMicrometer);

        var left = Assert.Single(payload.PachymetryMeasurements, measurement => measurement.Eye == NidekRs232Eye.Left);
        Assert.Equal(new[] { 521, 524, 523 }, left.ValuesMicrometer);
        Assert.Equal(523, left.AverageMicrometer);

        Assert.All(payload.MedistarCandidates, candidate => Assert.Equal("6220", candidate.FieldCode));
        Assert.DoesNotContain(payload.MedistarCandidates, candidate => candidate.FieldCode == "6228" || candidate.FieldCode == "6205");
    }

    [Fact]
    public void PayloadParser_ShouldCaptureErrorCodesWithoutMeasurementExport()
    {
        var frame = BuildFrame("DNT", "IDNIDEK/NT-1", "R01 ERR AV", "L01 APL");
        var payload = new NidekRs232PayloadParser().Parse(frame);

        Assert.Contains(payload.Errors, error => error.Code == "ERR" && error.Eye == NidekRs232Eye.Right);
        Assert.Contains(payload.Errors, error => error.Code == "APL" && error.Eye == NidekRs232Eye.Left);
        Assert.Empty(payload.MedistarCandidates);
    }

    [Fact]
    public void PayloadParser_ShouldKeepUnknownSegmentsWithoutExport()
    {
        var frame = BuildFrame("DLM", "IDNIDEK/LM-7P", "UVR099", "PRAW");
        var payload = new NidekRs232PayloadParser().Parse(frame);

        Assert.Contains(payload.RawSegments, segment => segment.RawText == "UVR099");
        Assert.Empty(payload.MedistarCandidates);
    }

    [Fact]
    public void CommunicationPresets_ShouldExposeNidekDefaults()
    {
        var ntPreset = NidekRs232CommunicationPresets.CreateNtPreset("COM7");
        var lmPreset = NidekRs232CommunicationPresets.CreateLm7Preset();
        var rt2100Type1 = NidekRs232CommunicationPresets.CreateRt2100Type1Preset();
        var rt3100Type1 = NidekRs232CommunicationPresets.CreateRt3100Type1Preset();
        var rt3100Type2 = NidekRs232CommunicationPresets.CreateRt3100Type2Preset();
        var rt5100Type1 = NidekRs232CommunicationPresets.CreateRt5100Type1Preset();
        var rt5100Type2 = NidekRs232CommunicationPresets.CreateRt5100Type2Preset();

        Assert.Equal("COM7", ntPreset.PortName);
        Assert.Equal(9600, ntPreset.BaudRate);
        Assert.Equal(8, ntPreset.DataBits);
        Assert.Equal(SerialStopBitsSetting.One, ntPreset.StopBits);
        Assert.Equal(SerialParitySetting.Odd, ntPreset.Parity);
        Assert.Equal(SerialParitySetting.None, lmPreset.Parity);

        AssertRtType1Preset(rt2100Type1);
        AssertRtType1Preset(rt3100Type1);
        AssertRtType1Preset(rt5100Type1);
        AssertRtType2Preset(rt3100Type2);
        AssertRtType2Preset(rt5100Type2);
    }

    private static void AssertRtType1Preset(SerialCommunicationSettings preset)
    {
        Assert.Equal(2400, preset.BaudRate);
        Assert.Equal(7, preset.DataBits);
        Assert.Equal(SerialParitySetting.Even, preset.Parity);
        Assert.Equal(SerialStopBitsSetting.Two, preset.StopBits);
        Assert.Equal(SerialLineTerminatorSetting.CR, preset.LineTerminator);
        Assert.True(preset.IsBidirectional);
    }

    private static void AssertRtType2Preset(SerialCommunicationSettings preset)
    {
        Assert.Equal(9600, preset.BaudRate);
        Assert.Equal(8, preset.DataBits);
        Assert.Equal(SerialParitySetting.Odd, preset.Parity);
        Assert.Equal(SerialStopBitsSetting.One, preset.StopBits);
        Assert.Equal(SerialLineTerminatorSetting.CR, preset.LineTerminator);
        Assert.True(preset.IsBidirectional);
    }

    private static NidekRs232FrameReadResult ReadFixture(
        string fileName,
        NidekRs232CommunicationMode mode = NidekRs232CommunicationMode.Unknown)
    {
        return new NidekRs232FrameReader().Read(ReadFixtureBytes(fileName), mode);
    }

    private static NidekRs232ParsedPayload ParseSinglePayload(string fileName)
    {
        var frame = Assert.Single(ReadFixture(fileName).Frames);
        return new NidekRs232PayloadParser().Parse(frame);
    }

    private static byte[] ReadFixtureBytes(string fileName)
    {
        var path = FindWorkspaceFile(Path.Combine("TestData", "Devices", "Nidek", "RS232", fileName));
        return ParseHex(File.ReadAllText(path));
    }

    private static NidekRs232Frame BuildFrame(string header, params string[] segments)
    {
        var builder = new List<byte>
        {
            NidekRs232ControlBytes.SOH
        };
        builder.AddRange(Encoding.ASCII.GetBytes(header));
        builder.Add(NidekRs232ControlBytes.STX);
        foreach (var segment in segments)
        {
            builder.AddRange(Encoding.ASCII.GetBytes(segment));
            builder.Add(NidekRs232ControlBytes.ETB);
        }

        builder.Add(NidekRs232ControlBytes.EOT);
        var frame = Assert.Single(new NidekRs232FrameReader().Read(builder.ToArray()).Frames);
        return frame;
    }

    private static byte[] ParseHex(string text)
    {
        var compact = new string(text.Where(Uri.IsHexDigit).ToArray());
        if (compact.Length % 2 != 0)
        {
            throw new FormatException("Fixture hex has odd length.");
        }

        var bytes = new byte[compact.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(compact.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    private static string ToHex(byte[] bytes)
    {
        return string.Join(" ", bytes.Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
    }

    private static string FindWorkspaceFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var workspaceCandidate = Path.Combine(directory.FullName, "XdtDeviceBridge.Tests", relativePath);
            if (File.Exists(workspaceCandidate))
            {
                return workspaceCandidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Workspace file not found: {relativePath}");
    }
}
