using Core;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Acceptance tests for <see cref="ProtocolName"/> – CT naming convention parsing,
/// formatting, and slice-thickness validation.
/// </summary>
public class ProtocolNameTests
{
    // ------------------------------------------------------------------ AC-1

    [Fact]
    public void Parse_Ac1_HeadNc5mm_RegionIsHead()
    {
        var result = ProtocolName.Parse("HEAD_NC_5mm");

        Assert.Equal(BodyRegion.Head, result.Region);
    }

    [Fact]
    public void Parse_Ac1_HeadNc5mm_ContrastIsNone()
    {
        var result = ProtocolName.Parse("HEAD_NC_5mm");

        Assert.Equal(ContrastKind.None, result.Contrast);
    }

    [Fact]
    public void Parse_Ac1_HeadNc5mm_ThicknessIs5mm()
    {
        var result = ProtocolName.Parse("HEAD_NC_5mm");

        Assert.Equal(5m, result.ThicknessMillimetres);
    }

    // ------------------------------------------------------------------ AC-2

    [Fact]
    public void Parse_Ac2_ChestC1Point25mm_RegionIsChest()
    {
        var result = ProtocolName.Parse("CHEST_C_1.25mm");

        Assert.Equal(BodyRegion.Chest, result.Region);
    }

    [Fact]
    public void Parse_Ac2_ChestC1Point25mm_ContrastIsWithContrast()
    {
        var result = ProtocolName.Parse("CHEST_C_1.25mm");

        Assert.Equal(ContrastKind.WithContrast, result.Contrast);
    }

    [Fact]
    public void Parse_Ac2_ChestC1Point25mm_ThicknessIs1Point25mm()
    {
        var result = ProtocolName.Parse("CHEST_C_1.25mm");

        Assert.Equal(1.25m, result.ThicknessMillimetres);
    }

    // ------------------------------------------------------------------ AC-3

    [Fact]
    public void Parse_Ac3_UnrecognisedRegion_ThrowsFormatExceptionNamingRegion()
    {
        var ex = Assert.Throws<FormatException>(() => ProtocolName.Parse("BRAIN_NC_5mm"));

        Assert.Contains("BRAIN", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TORSO_NC_5mm", "TORSO")]
    [InlineData("LEG_C_2mm", "LEG")]
    [InlineData("KNEE_NC_1mm", "KNEE")]
    public void Parse_Ac3_VariousUnrecognisedRegions_ThrowsFormatExceptionNamingEachRegion(
        string input, string badRegion)
    {
        var ex = Assert.Throws<FormatException>(() => ProtocolName.Parse(input));

        Assert.Contains(badRegion, ex.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ AC-4

    [Theory]
    [InlineData("head 5mm")]
    [InlineData("HEAD-NC-5mm")]
    [InlineData("HEADNC5mm")]
    [InlineData("HEAD_NC_5")]
    [InlineData("")]
    public void Parse_Ac4_MalformedName_ThrowsFormatExceptionNamingInputAndExpectedForm(string input)
    {
        var ex = Assert.Throws<FormatException>(() => ProtocolName.Parse(input));

        // The exception must identify the bad input …
        Assert.Contains(input, ex.Message, StringComparison.Ordinal);
        // … and describe the expected form (the suffix "mm" is a stable part of the pattern).
        Assert.Contains("mm", ex.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ AC-5

    [Theory]
    [InlineData(BodyRegion.Head, ContrastKind.None, 5.0)]
    [InlineData(BodyRegion.Chest, ContrastKind.WithContrast, 1.25)]
    [InlineData(BodyRegion.Neck, ContrastKind.None, 2.5)]
    [InlineData(BodyRegion.Abdomen, ContrastKind.WithContrast, 0.625)]
    [InlineData(BodyRegion.Pelvis, ContrastKind.None, 3.0)]
    [InlineData(BodyRegion.Spine, ContrastKind.WithContrast, 1.0)]
    public void Format_Ac5_AllRegionsAndContrasts_FormattedNameRoundTripsToSameValues(
        BodyRegion region, ContrastKind contrast, double thicknessMm)
    {
        var thickness = (decimal)thicknessMm;

        var formatted = ProtocolName.Format(region, contrast, thickness);
        var parsed = ProtocolName.Parse(formatted);

        Assert.Equal(region, parsed.Region);
        Assert.Equal(contrast, parsed.Contrast);
        Assert.Equal(thickness, parsed.ThicknessMillimetres);
    }

    // ------------------------------------------------------------------ AC-6

    [Fact]
    public void Validate_Ac6_NameThicknessDiffersFromSliceThickness_ReturnsSingleProblemMentioningBoth()
    {
        // Name says 5 mm; scan protocol says 3 mm – they disagree.
        var protocol = new ScanProtocol
        {
            Name = "HEAD_NC_5mm",
            SliceThicknessMillimetres = 3m,
        };

        var problems = ProtocolName.Validate(protocol);

        Assert.Single(problems);
        Assert.Contains("5", problems[0], StringComparison.Ordinal);
        Assert.Contains("3", problems[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_Ac6_NameThicknessMatchesSliceThickness_ReturnsNoProblems()
    {
        // When the name and the protocol agree there should be no disagreement problem.
        var protocol = new ScanProtocol
        {
            Name = "HEAD_NC_5mm",
            SliceThicknessMillimetres = 5m,
        };

        var problems = ProtocolName.Validate(protocol);

        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_Ac6_ChestProtocolNameThicknessDiffersFromSliceThickness_ReturnsSingleProblem()
    {
        // A second body region to confirm the check is not region-specific.
        var protocol = new ScanProtocol
        {
            Name = "CHEST_C_1.25mm",
            SliceThicknessMillimetres = 2m,
        };

        var problems = ProtocolName.Validate(protocol);

        Assert.Single(problems);
        Assert.Contains("1.25", problems[0], StringComparison.Ordinal);
        Assert.Contains("2", problems[0], StringComparison.Ordinal);
    }
}
