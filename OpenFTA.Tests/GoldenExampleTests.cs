using OpenFTA.Tests.Helpers;
using Xunit.Abstractions;

namespace OpenFTA.Tests;

/// <summary>
/// Golden tests: results of the example files are stored in Snapshots/*.json on the first run
/// and must not change afterwards (protection during refactoring).
/// The stored values are the CURRENT behaviour, not verified reference values.
/// When a bug fix changes a result on purpose, delete the .json file and run again.
/// </summary>
public class GoldenExampleTests : SettingsTestBase
{
    private readonly ITestOutputHelper _output;

    public GoldenExampleTests(ITestOutputHelper output) => _output = output;

    public static IEnumerable<object[]> ExampleFiles() =>
        Directory.GetFiles(ExampleLoader.ExamplesDirectory, "*.fta")
            .Select(Path.GetFileName)
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => new object[] { f! });

    [Fact]
    public void Examples_directory_contains_files()
    {
        Assert.NotEmpty(ExampleFiles());
    }

    [Theory]
    [MemberData(nameof(ExampleFiles))]
    public void Example_results_are_unchanged(string file)
    {
        var t = ExampleLoader.Load(file);

        t.ComputeTree();
        t.GenerateMCS(out string equation, out string simplified);
        var mcsTop = t.McsTop();
        t.SumChildren(mcsTop, t.MCSStructure);

        var actual = new GoldenResult(
            TopValue: t.Top().Value,
            TopValueType: t.Top().ValueType.ToString(),
            McsTopValue: mcsTop.Value,
            CutSets: CutSets.Parse(simplified));

        _output.WriteLine($"Equation:   {equation}");
        _output.WriteLine($"MCS:        {simplified}");
        _output.WriteLine($"Top (gate by gate): {actual.TopValue:R} ({actual.TopValueType})");
        _output.WriteLine($"Top (MCS):          {actual.McsTopValue:R}");

        if (Snapshot.Match(file, actual))
            _output.WriteLine($"Snapshot created in {Snapshot.Directory}");
    }
}
