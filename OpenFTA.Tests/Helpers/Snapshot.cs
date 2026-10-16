using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFTA.Tests.Helpers;

/// <summary>Results of one example that are stored as a golden snapshot.</summary>
public sealed record GoldenResult(
    double TopValue,
    string TopValueType,
    double McsTopValue,
    List<string> CutSets);

/// <summary>One minimal cut set as shown in the MCS table and report.</summary>
public sealed record McsDetail(
    string Name,
    List<string> Events,
    double Value,
    string ValueType,
    int ValueUnit,
    double Contribution);

/// <summary>
/// Golden (snapshot) testing: on the first run the result is written to
/// OpenFTA.Tests/Snapshots/&lt;file&gt;; on later runs it is compared with it.
/// To accept an intentional change, delete the snapshot file and run the test again.
/// </summary>
public static class Snapshot
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public static string Directory { get; } = FindSnapshotDirectory();

    /// <returns>true if the snapshot was newly created.</returns>
    public static bool Match(string name, GoldenResult actual)
    {
        if (!TryLoad(Path.GetFileNameWithoutExtension(name) + ".json", actual, out GoldenResult? expected))
            return true;

        Approx.Equal(expected!.TopValue, actual.TopValue);
        Assert.Equal(expected.TopValueType, actual.TopValueType);
        Approx.Equal(expected.McsTopValue, actual.McsTopValue);
        Assert.Equal(expected.CutSets, actual.CutSets);
        return false;
    }

    /// <returns>true if the snapshot was newly created.</returns>
    public static bool MatchMcs(string name, List<McsDetail> actual)
    {
        if (!TryLoad(Path.GetFileNameWithoutExtension(name) + ".mcs.json", actual, out List<McsDetail>? expected))
            return true;

        Assert.Equal(expected!.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            var a = actual[i];
            Assert.Equal(e.Name, a.Name);
            Assert.Equal(e.Events, a.Events);
            Approx.Equal(e.Value, a.Value);
            Assert.Equal(e.ValueType, a.ValueType);
            Assert.Equal(e.ValueUnit, a.ValueUnit);
            Approx.Equal(e.Contribution, a.Contribution);
        }
        return false;
    }

    /// <summary>Exact text comparison (line endings are normalized).</summary>
    /// <returns>true if the snapshot was newly created.</returns>
    public static bool MatchText(string fileName, string actual)
    {
        string path = Path.Combine(Directory, fileName);
        string normalized = actual.Replace("\r\n", "\n");

        if (!File.Exists(path))
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(path, normalized);
            return true;
        }

        string expected = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.Equal(expected, normalized);
        return false;
    }

    private static bool TryLoad<T>(string fileName, T actual, out T? expected)
    {
        string path = Path.Combine(Directory, fileName);
        if (!File.Exists(path))
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(path, JsonSerializer.Serialize(actual, Options));
            expected = default;
            return false;
        }

        expected = JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);
        Assert.NotNull(expected);
        return true;
    }

    private static string FindSnapshotDirectory([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "Snapshots"));
}
