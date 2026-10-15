namespace OpenFTA.Tests.Helpers;

/// <summary>
/// Normalizes an MCS expression like "B*A + C" into a sorted list of cut sets ["A*B", "C"],
/// so that tests do not depend on the order of terms or factors.
/// </summary>
public static class CutSets
{
    public static List<string> Parse(string expression) =>
        expression
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(term => string.Join("*",
                term.Split('*', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(f => f.ToUpperInvariant())
                    .OrderBy(f => f, StringComparer.Ordinal)))
            .Distinct()
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToList();
}
