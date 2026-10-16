namespace Core;

/// <summary>
/// Describes a single validation problem found in a <see cref="ScanProtocol"/>.
/// </summary>
/// <param name="FieldName">The name of the field that has a problem.</param>
/// <param name="Description">A human-readable description of the problem.</param>
public sealed record ValidationProblem(string FieldName, string Description);
