using System;
using System.Linq;
using Core;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Acceptance tests for <see cref="ScanProtocol"/> and its validation logic.
/// </summary>
public class ScanProtocolTests
{
    // -----------------------------------------------------------------------
    // AC-1: A fully valid protocol produces no validation problems.
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_Ac1_ValidProtocolReportsNoProblems()
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: 1.0);

        var problems = protocol.Validate();

        Assert.Empty(problems);
    }

    // -----------------------------------------------------------------------
    // AC-2: A tube voltage not in the allowed set is reported with field name
    //       and value.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(110)]
    [InlineData(130)]
    [InlineData(160)]
    public void Validate_Ac2_InvalidTubeVoltageReportsOneProblemNamingFieldAndValue(int invalidKv)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: invalidKv,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: 1.0);

        var problems = protocol.Validate();

        Assert.Single(problems);
        var problem = problems[0];
        Assert.Contains("TubeVoltageKv", problem.FieldName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(invalidKv.ToString(), problem.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(70)]
    [InlineData(80)]
    [InlineData(100)]
    [InlineData(120)]
    [InlineData(140)]
    [InlineData(150)]
    public void Validate_Ac2_AllAllowedTubeVoltagesProduceNoTubeVoltageProblem(int validKv)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: validKv,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: 1.0);

        var problems = protocol.Validate();

        Assert.DoesNotContain(problems, p =>
            p.FieldName.Contains("TubeVoltageKv", StringComparison.OrdinalIgnoreCase));
    }

    // -----------------------------------------------------------------------
    // AC-3: Out-of-range tube current-time product is reported with field name
    //       and allowed range.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0.9)]   // just below minimum
    [InlineData(0.0)]
    [InlineData(1000.1)] // just above maximum
    [InlineData(1500.0)]
    public void Validate_Ac3_TubeCurrentTimeProductOutOfRangeReportsProblemWithFieldAndRange(double invalidMas)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: invalidMas,
            sliceThicknessMm: 5.0,
            pitch: 1.0);

        var problems = protocol.Validate();

        var problem = Assert.Single(problems);
        Assert.Contains("TubeCurrentTimeMas", problem.FieldName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1", problem.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1000", problem.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(500.0)]
    [InlineData(1000.0)]
    public void Validate_Ac3_TubeCurrentTimeProductAtBoundaryOrInsideRangeProducesNoRelatedProblem(double validMas)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: validMas,
            sliceThicknessMm: 5.0,
            pitch: 1.0);

        var problems = protocol.Validate();

        Assert.DoesNotContain(problems, p =>
            p.FieldName.Contains("TubeCurrentTimeMas", StringComparison.OrdinalIgnoreCase));
    }

    // -----------------------------------------------------------------------
    // AC-3: Out-of-range slice thickness is reported with field name and
    //       allowed range.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0.4)]   // just below minimum
    [InlineData(0.0)]
    [InlineData(10.1)]  // just above maximum
    [InlineData(20.0)]
    public void Validate_Ac3_SliceThicknessOutOfRangeReportsProblemWithFieldAndRange(double invalidMm)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: invalidMm,
            pitch: 1.0);

        var problems = protocol.Validate();

        var problem = Assert.Single(problems);
        Assert.Contains("SliceThicknessMm", problem.FieldName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0.5", problem.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("10", problem.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void Validate_Ac3_SliceThicknessAtBoundaryOrInsideRangeProducesNoRelatedProblem(double validMm)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: validMm,
            pitch: 1.0);

        var problems = protocol.Validate();

        Assert.DoesNotContain(problems, p =>
            p.FieldName.Contains("SliceThicknessMm", StringComparison.OrdinalIgnoreCase));
    }

    // -----------------------------------------------------------------------
    // AC-3: Out-of-range pitch is reported with field name and allowed range.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0.4)]   // just below minimum
    [InlineData(0.0)]
    [InlineData(1.6)]   // just above maximum
    [InlineData(3.0)]
    public void Validate_Ac3_PitchOutOfRangeReportsProblemWithFieldAndRange(double invalidPitch)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: invalidPitch);

        var problems = protocol.Validate();

        var problem = Assert.Single(problems);
        Assert.Contains("Pitch", problem.FieldName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0.5", problem.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1.5", problem.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    public void Validate_Ac3_PitchAtBoundaryOrInsideRangeProducesNoRelatedProblem(double validPitch)
    {
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 120,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: validPitch);

        var problems = protocol.Validate();

        Assert.DoesNotContain(problems, p =>
            p.FieldName.Contains("Pitch", StringComparison.OrdinalIgnoreCase));
    }

    // -----------------------------------------------------------------------
    // AC-4: All violations in a single protocol are reported, not only the
    //       first.
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_Ac4_MultipleViolationsAllReported()
    {
        // Invalid voltage (90 kV), out-of-range mAs (0), thickness (0.1), pitch (2.0)
        var protocol = new ScanProtocol(
            name: "BAD_PROTOCOL",
            tubeVoltageKv: 90,
            tubeCurrentTimeMas: 0.0,
            sliceThicknessMm: 0.1,
            pitch: 2.0);

        var problems = protocol.Validate();

        Assert.Equal(4, problems.Count);

        Assert.Contains(problems, p =>
            p.FieldName.Contains("TubeVoltageKv", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(problems, p =>
            p.FieldName.Contains("TubeCurrentTimeMas", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(problems, p =>
            p.FieldName.Contains("SliceThicknessMm", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(problems, p =>
            p.FieldName.Contains("Pitch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Ac4_TwoViolationsProduceTwoProblems()
    {
        // Invalid voltage and out-of-range pitch; other fields valid
        var protocol = new ScanProtocol(
            name: "HEAD_NC_5mm",
            tubeVoltageKv: 55,
            tubeCurrentTimeMas: 250.0,
            sliceThicknessMm: 5.0,
            pitch: 0.1);

        var problems = protocol.Validate();

        Assert.Equal(2, problems.Count);
    }

    // -----------------------------------------------------------------------
    // AC-5: A blank protocol name raises ArgumentException at construction.
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_Ac5_BlankNameThrowsArgumentException(string blankName)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ScanProtocol(
                name: blankName,
                tubeVoltageKv: 120,
                tubeCurrentTimeMas: 250.0,
                sliceThicknessMm: 5.0,
                pitch: 1.0));

        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_Ac5_NullNameThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ScanProtocol(
                name: null!,
                tubeVoltageKv: 120,
                tubeCurrentTimeMas: 250.0,
                sliceThicknessMm: 5.0,
                pitch: 1.0));
    }

    [Fact]
    public void Constructor_Ac5_NonBlankNameDoesNotThrow()
    {
        var ex = Record.Exception(() =>
            new ScanProtocol(
                name: "CHEST_HR",
                tubeVoltageKv: 120,
                tubeCurrentTimeMas: 250.0,
                sliceThicknessMm: 5.0,
                pitch: 1.0));

        Assert.Null(ex);
    }
}
