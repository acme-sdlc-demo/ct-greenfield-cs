using System;
using System.Collections.Generic;
using System.Globalization;

namespace Core;

/// <summary>
/// Represents a parsed CT protocol name in the
/// <c>&lt;REGION&gt;_&lt;CONTRAST&gt;_&lt;thickness&gt;mm</c> convention,
/// and provides methods to parse, format and validate protocol names.
/// </summary>
public sealed class ProtocolName
{
    private const string ExpectedForm = "<REGION>_<CONTRAST>_<thickness>mm";
    private const string MmSuffix = "mm";
    private const string ContrastNone = "NC";
    private const string ContrastWith = "C";

    /// <summary>Gets the anatomical body region of the protocol.</summary>
    public BodyRegion Region { get; }

    /// <summary>Gets whether intravenous contrast medium is used.</summary>
    public ContrastKind Contrast { get; }

    /// <summary>Gets the nominal slice thickness in millimetres.</summary>
    public decimal ThicknessMillimetres { get; }

    private ProtocolName(BodyRegion region, ContrastKind contrast, decimal thicknessMillimetres)
    {
        Region = region;
        Contrast = contrast;
        ThicknessMillimetres = thicknessMillimetres;
    }

    /// <summary>
    /// Parses a CT protocol name in the
    /// <c>&lt;REGION&gt;_&lt;CONTRAST&gt;_&lt;thickness&gt;mm</c> form.
    /// </summary>
    /// <param name="name">The protocol name string to parse.</param>
    /// <returns>A <see cref="ProtocolName"/> with the parsed values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="name"/> is not in the expected form, or names an unrecognised region.
    /// </exception>
    public static ProtocolName Parse(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        // Split into exactly three underscore-separated segments.
        string[] parts = name.Split('_');
        if (parts.Length != 3 || !parts[2].EndsWith(MmSuffix, StringComparison.Ordinal))
        {
            throw new FormatException(
                $"'{name}' is not in the expected {ExpectedForm} form.");
        }

        string regionStr = parts[0];
        string contrastStr = parts[1];
        string thicknessStr = parts[2].Substring(0, parts[2].Length - MmSuffix.Length);

        // Validate region.
        BodyRegion region = regionStr switch
        {
            "HEAD" => BodyRegion.Head,
            "NECK" => BodyRegion.Neck,
            "CHEST" => BodyRegion.Chest,
            "ABDOMEN" => BodyRegion.Abdomen,
            "PELVIS" => BodyRegion.Pelvis,
            "SPINE" => BodyRegion.Spine,
            _ => throw new FormatException(
                $"Unknown region '{regionStr}'; supported regions are HEAD, NECK, CHEST, ABDOMEN, PELVIS, SPINE."),
        };

        // Parse contrast.
        ContrastKind contrast = contrastStr switch
        {
            ContrastNone => ContrastKind.None,
            ContrastWith => ContrastKind.WithContrast,
            _ => throw new FormatException(
                $"Unknown contrast token '{contrastStr}'; expected 'NC' (none) or 'C' (with contrast)."),
        };

        // Parse thickness with invariant culture so '.' is always the decimal separator.
        if (!decimal.TryParse(thicknessStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal thickness))
        {
            throw new FormatException(
                $"'{name}' is not in the expected {ExpectedForm} form: thickness '{thicknessStr}' is not a valid decimal number.");
        }

        return new ProtocolName(region, contrast, thickness);
    }

    /// <summary>
    /// Formats a protocol name from its components.
    /// The result is guaranteed to round-trip through <see cref="Parse"/>.
    /// </summary>
    /// <param name="region">The body region.</param>
    /// <param name="contrast">The contrast kind.</param>
    /// <param name="thicknessMillimetres">The slice thickness in millimetres.</param>
    /// <returns>A protocol name string in <c>&lt;REGION&gt;_&lt;CONTRAST&gt;_&lt;thickness&gt;mm</c> form.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="region"/> or <paramref name="contrast"/> is not a defined enum value.
    /// </exception>
    public static string Format(BodyRegion region, ContrastKind contrast, decimal thicknessMillimetres)
    {
        string regionStr = region switch
        {
            BodyRegion.Head => "HEAD",
            BodyRegion.Neck => "NECK",
            BodyRegion.Chest => "CHEST",
            BodyRegion.Abdomen => "ABDOMEN",
            BodyRegion.Pelvis => "PELVIS",
            BodyRegion.Spine => "SPINE",
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unrecognised BodyRegion value."),
        };

        string contrastStr = contrast switch
        {
            ContrastKind.None => ContrastNone,
            ContrastKind.WithContrast => ContrastWith,
            _ => throw new ArgumentOutOfRangeException(nameof(contrast), contrast, "Unrecognised ContrastKind value."),
        };

        string thicknessStr = thicknessMillimetres.ToString(CultureInfo.InvariantCulture);

        return $"{regionStr}_{contrastStr}_{thicknessStr}{MmSuffix}";
    }

    /// <summary>
    /// Validates that the <see cref="ScanProtocol.Name"/>'s embedded thickness matches
    /// <see cref="ScanProtocol.SliceThicknessMillimetres"/>.
    /// Returns an empty list when the protocol name is consistent with its slice thickness.
    /// </summary>
    /// <param name="protocol">The scan protocol to validate.</param>
    /// <returns>
    /// A read-only list of problem description strings, one per detected inconsistency.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="protocol"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(ScanProtocol protocol)
    {
        ArgumentNullException.ThrowIfNull(protocol, nameof(protocol));

        var problems = new List<string>();

        ProtocolName parsed;
        try
        {
            parsed = Parse(protocol.Name);
        }
        catch (FormatException)
        {
            // If the name itself doesn't conform to the convention the thickness
            // comparison cannot be made; report nothing extra here.
            return problems;
        }

        if (parsed.ThicknessMillimetres != protocol.SliceThicknessMillimetres)
        {
            problems.Add(
                $"The name '{protocol.Name}' specifies a thickness of {parsed.ThicknessMillimetres} mm " +
                $"but the protocol's SliceThicknessMillimetres is {protocol.SliceThicknessMillimetres} mm.");
        }

        return problems;
    }
}
