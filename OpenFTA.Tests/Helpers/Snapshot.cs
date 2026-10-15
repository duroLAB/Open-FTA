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

/// <summary>
/// Golden (snapshot) testing: on the first run the result is written to
/// OpenFTA.Tests/Snapshots/&lt;name&gt;.json; on later runs it is compared with it.
/// To accept an intentional change, delete the .json file and run the test again.
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
        string path = Path.Combine(Directory, Path.GetFileNameWithoutExtension(name) + ".json");

        if (!File.Exists(path))
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(path, JsonSerializer.Serialize(actual, Options));
            return true;
        }

        var expected = JsonSerializer.Deserialize<GoldenResult>(File.ReadAllText(path), Options)!;

        Approx.Equal(expected.TopValue, actual.TopValue);
        Assert.Equal(expected.TopValueType, actual.TopValueType);
        Approx.Equal(expected.McsTopValue, actual.McsTopValue);
        Assert.Equal(expected.CutSets, actual.CutSets);
        return false;
    }

    private static string FindSnapshotDirectory([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "Snapshots"));
}
