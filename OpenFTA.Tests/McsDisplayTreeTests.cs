using System.Drawing.Imaging;
using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Drawing tree of minimal cut sets (tab "Minimal Cut Set Tree").</summary>
public class McsDisplayTreeTests : SettingsTestBase
{
    private static FTAlogic RepeatedEventTree(out FTAitem a)
    {
        // TOP = A * (B + C) = A*B + A*C  (A is in both cut sets)
        var b = new TreeBuilder(Gates.AND);
        a = b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();
        t.GenerateMCS(out _, out _);
        return t;
    }

    [Fact]
    public void Tree_has_top_cut_sets_and_event_occurrences()
    {
        var t = RepeatedEventTree(out var a);

        var tree = McsDisplayTree.Build(t);

        Assert.Equal(1 + 2 + 4, tree.Items.Count);
        var mcsNodes = tree.Root.Children.Select(g => tree.Items[g]).ToList();
        Assert.Equal(new[] { "MCS1", "MCS2" }, mcsNodes.Select(n => n.Name));
        Assert.All(mcsNodes, n => Assert.Equal(Gates.AND, n.Gate));
        Assert.All(mcsNodes, n => Assert.Equal(2, n.Children.Count));

        // A is drawn twice as two different nodes, both pointing to the original event
        var aNodes = tree.Items.Values.Where(i => i.Tag == "A").ToList();
        Assert.Equal(2, aNodes.Count);
        Assert.NotEqual(aNodes[0].GuidCode, aNodes[1].GuidCode);
        Assert.All(aNodes, n => Assert.Same(a, tree.Originals[n.GuidCode]));
        Assert.All(aNodes, n => Assert.False(t.FTAStructure.ContainsKey(n.GuidCode)));
    }

    [Fact]
    public void First_order_cut_set_has_no_gate()
    {
        // TOP = A + B*C: MCS1 = A (one event, no gate), MCS2 = B*C (AND)
        var b = new TreeBuilder(Gates.OR);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.AND);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();
        t.GenerateMCS(out _, out _);

        var tree = McsDisplayTree.Build(t);

        var gates = tree.Root.Children.Select(g => tree.Items[g]).ToDictionary(n => n.Name, n => n.Gate);
        Assert.Equal(Gates.NotSet, gates["MCS1"]);
        Assert.Equal(Gates.AND, gates["MCS2"]);
    }

    [Fact]
    public void Values_are_taken_from_the_MCS_calculation()
    {
        UseExactCalculation();
        var t = RepeatedEventTree(out _);

        var tree = McsDisplayTree.Build(t);

        Approx.Equal(t.McsTopValue, tree.Root.Value);
        Assert.Equal(t.McsTopValueType, tree.Root.ValueType);
        foreach (var (guid, mcs) in tree.CutSets)
        {
            Approx.Equal(mcs.Value, tree.Items[guid].Value);
            Assert.Equal(mcs.ValueType, tree.Items[guid].ValueType);
        }
    }

    [Theory]
    [MemberData(nameof(GoldenExampleTests.ExampleFiles), MemberType = typeof(GoldenExampleTests))]
    public void Example_tree_is_consistent_and_laid_out(string file)
    {
        var t = ExampleLoader.Load(file);
        t.ComputeTree();
        t.GenerateMCS(out _, out _);

        var tree = McsDisplayTree.Build(t);

        var nonEmpty = t.MinimalCutSets.Where(m => m.Events.Count > 0).ToList();
        Assert.Equal(1 + nonEmpty.Count + nonEmpty.Sum(m => m.Events.Count), tree.Items.Count);
        Assert.Equal(nonEmpty.Sum(m => m.Events.Count), tree.Originals.Count);

        // Parent / children links are consistent
        foreach (var item in tree.Items.Values)
            foreach (var child in item.Children)
                Assert.Equal(item.GuidCode, tree.Items[child].Parent);

        // No overlaps on any level
        int minGap = Constants.EventWidth + Constants.EventHorizontalSpacing;
        foreach (var level in tree.Items.Values.GroupBy(i => i.Y))
        {
            var xs = level.Select(i => i.X).OrderBy(x => x).ToList();
            for (int k = 0; k + 1 < xs.Count; k++)
                Assert.True(xs[k + 1] - xs[k] >= minGap, $"{file}: events at Y = {level.Key} overlap");
        }
    }

    [Theory]
    [MemberData(nameof(GoldenExampleTests.ExampleFiles), MemberType = typeof(GoldenExampleTests))]
    public void Example_tree_can_be_drawn_and_exported(string file)
    {
        var t = ExampleLoader.Load(file);
        t.ComputeTree();
        t.GenerateMCS(out _, out _);
        var tree = McsDisplayTree.Build(t);
        var engine = new DrawingEngine(t, tree.Items);

        // Highlight the first cut set as when it is selected in the tree view
        var first = t.MinimalCutSets.First(m => m.Events.Count > 0);
        t.HighlightedMCS = first.Name;
        t.HighlightedEvents.AddRange(first.Events);

        string path = Path.Combine(Path.GetTempPath(), $"openfta-mcs-{Guid.NewGuid():N}.png");
        try
        {
            engine.ExportToBMP(path, ImageFormat.Png);

            using var image = new Bitmap(path);
            int dark = 0;
            for (int y = 0; y < image.Height; y += 2)
                for (int x = 0; x < image.Width; x += 2)
                {
                    Color c = image.GetPixel(x, y);
                    if (c.R < 100 && c.G < 100 && c.B < 100)
                        dark++;
                }
            Assert.True(dark > 100, "Exported MCS tree is empty");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
