using System.Collections.Generic;

namespace Core;

/// <summary>
/// Represents a CT scan protocol with its acquisition parameters.
/// </summary>
public sealed class ScanProtocol
{
    private static readonly int[] AllowedTubeVoltagesKv = { 70, 80, 100, 120, 140, 150 };

    /// <summary>Gets the name of the protocol.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the slice thickness in millimetres as a decimal value.</summary>
    public decimal SliceThicknessMillimetres { get; init; }

    /// <summary>Gets the tube voltage in kilovolts.</summary>
    public int TubeVoltageKv { get; }

    /// <summary>Gets the tube current-time product in milliampere-seconds.</summary>
    public double TubeCurrentTimeMas { get; }

    /// <summary>Gets the slice thickness in millimetres.</summary>
    public double SliceThicknessMm { get; }

    /// <summary>Gets the helical pitch (dimensionless).</summary>
    public double Pitch { get; }

    /// <summary>
    /// Initialises a new <see cref="ScanProtocol"/> with default values.
    /// Use the object-initialiser syntax to set individual properties.
    /// </summary>
    public ScanProtocol() { }

    /// <summary>
    /// Initialises a new <see cref="ScanProtocol"/>.
    /// </summary>
    /// <param name="name">Protocol name; must not be null, empty or whitespace.</param>
    /// <param name="tubeVoltageKv">Tube voltage in kV.</param>
    /// <param name="tubeCurrentTimeMas">Tube current-time product in mAs.</param>
    /// <param name="sliceThicknessMm">Slice thickness in mm.</param>
    /// <param name="pitch">Helical pitch.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
    public ScanProtocol(
        string name,
        int tubeVoltageKv,
        double tubeCurrentTimeMas,
        double sliceThicknessMm,
        double pitch)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Protocol name must not be blank.", nameof(name));
        }

        Name = name;
        TubeVoltageKv = tubeVoltageKv;
        TubeCurrentTimeMas = tubeCurrentTimeMas;
        SliceThicknessMm = sliceThicknessMm;
        Pitch = pitch;
    }

    /// <summary>
    /// Validates the protocol parameters and returns all detected problems.
    /// Returns an empty list when the protocol is valid.
    /// This method has no side effects and requires no scanner or network access.
    /// </summary>
    /// <returns>
    /// A read-only list of <see cref="ValidationProblem"/> instances, one per detected issue.
    /// </returns>
    public IReadOnlyList<ValidationProblem> Validate()
    {
        var problems = new List<ValidationProblem>();

        if (System.Array.IndexOf(AllowedTubeVoltagesKv, TubeVoltageKv) < 0)
        {
            problems.Add(new ValidationProblem(
                FieldName: nameof(TubeVoltageKv),
                Description: $"TubeVoltageKv value {TubeVoltageKv} kV is not in the allowed set " +
                             $"(70, 80, 100, 120, 140, 150 kV)."));
        }

        if (TubeCurrentTimeMas < 1.0 || TubeCurrentTimeMas > 1000.0)
        {
            problems.Add(new ValidationProblem(
                FieldName: nameof(TubeCurrentTimeMas),
                Description: $"TubeCurrentTimeMas value {TubeCurrentTimeMas} mAs is outside " +
                             $"the allowed range 1 to 1000 mAs."));
        }

        if (SliceThicknessMm < 0.5 || SliceThicknessMm > 10.0)
        {
            problems.Add(new ValidationProblem(
                FieldName: nameof(SliceThicknessMm),
                Description: $"SliceThicknessMm value {SliceThicknessMm} mm is outside " +
                             $"the allowed range 0.5 to 10 mm."));
        }

        if (Pitch < 0.5 || Pitch > 1.5)
        {
            problems.Add(new ValidationProblem(
                FieldName: nameof(Pitch),
                Description: $"Pitch value {Pitch} is outside the allowed range 0.5 to 1.5."));
        }

        return problems;
    }
}
