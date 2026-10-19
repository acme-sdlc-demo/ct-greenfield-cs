namespace Core;

/// <summary>
/// Indicates whether a CT scan protocol uses intravenous contrast medium.
/// </summary>
public enum ContrastKind
{
    /// <summary>No contrast medium is administered.</summary>
    None,

    /// <summary>Intravenous contrast medium is administered.</summary>
    WithContrast,
}
