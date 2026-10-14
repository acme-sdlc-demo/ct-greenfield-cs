namespace Core;

/// <summary>Greets people by name.</summary>
public static class Greeting
{
    /// <summary>A greeting for <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">The name is empty or whitespace.</exception>
    public static string Greet(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return $"Hello, {name}";
    }
}
