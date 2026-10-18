using System.Diagnostics;
using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>
/// ALGO III (tidy tree layout). Positions change on purpose compared to ALGO I / II,
/// so the tests check properties of the layout instead of exact coordinates.
/// </summary>
public class LayoutTests : SettingsTestBase
{
    private static int Distance => Constants.EventWidth + Constants.EventHorizontalSpacing + 1;
    private static int LevelHeight => Constants.EventHeight + Constants.EventVerticalSpacing;

    private static void Arrange(FTAlogic t)
    {
        Settings.SortingAlgoVersion = SortingStrategy.ALGOIII;
        t.ArrangeTree();
    }

    private static List<FTAitem> VisibleChildren(FTAlogic t, FTAitem item) =>
        item.Children.Select(g => t.GetItem(g)).Where(c => c != null && !c.IsHidden).ToList();

    /// <summary>No overlaps, order kept, parents centred, levels aligned, leftmost event at X = 0.</summary>
    private static void AssertTidy(FTAlogic t)
    {
        var top = t.Top();
        var levels = new Dictionary<int, List<FTAitem>>();
        var visible = new List<FTAitem>();

        void Walk(FTAitem item, int depth)
        {
            visible.Add(item);
            if (!levels.TryGetValue(depth, out var list))
                levels[depth] = list = new List<FTAitem>();
            list.Add(item);                                   // DFS order = left-to-right order
            foreach (var child in VisibleChildren(t, item))
                Walk(child, depth + 1);
        }
        Walk(top, 0);

        int minGap = Constants.EventWidth + Constants.EventHorizontalSpacing;
        foreach (var (depth, items) in levels)
        {
            Assert.All(items, i => Assert.Equal(top.Y + depth * LevelHeight, i.Y));
            for (int k = 0; k + 1 < items.Count; k++)
                Assert.True(items[k + 1].X - items[k].X >= minGap,
                    $"Level {depth}: {items[k].Tag} at {items[k].X} and {items[k + 1].Tag} at {items[k + 1].X} are too close");
        }

        foreach (var item in visible)
        {
            var kids = VisibleChildren(t, item);
            if (kids.Count == 0)
                continue;
            double middle = (kids[0].X + kids[^1].X) / 2.0;
            Assert.True(Math.Abs(item.X - middle) <= 1, $"{item.Tag} at {item.X} is not centred above its children ({middle})");
        }

        Assert.Equal(0, visible.Min(i => i.X));
        Assert.False(t.IsAnyItemOverlapping());
    }

    private static FTAlogic RandomTree(int size, Random rng)
    {
        var b = new TreeBuilder(Gates.OR);
        var nodes = new List<FTAitem> { b.Top };
        for (int i = 1; i < size; i++)
        {
            var parent = nodes[rng.Next(nodes.Count)];
            nodes.Add(b.AddGate(parent, "N" + i, Gates.OR));
        }
        return b.Build();
    }

    [Fact]
    public void Simple_fan_has_exact_positions()
    {
        var t = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.1).Basic("C", 0.1).Build();

        Arrange(t);

        var a = t.GetItems("A").Single();
        var b = t.GetItems("B").Single();
        var c = t.GetItems("C").Single();
        Assert.Equal(new[] { 0, Distance, 2 * Distance }, new[] { a.X, b.X, c.X });
        Assert.Equal(Distance, t.Top().X);                    // centred above B
        Assert.Equal(50, t.Top().Y);
        Assert.All(new[] { a, b, c }, e => Assert.Equal(50 + LevelHeight, e.Y));
    }

    [Theory]
    [MemberData(nameof(GoldenExampleTests.ExampleFiles), MemberType = typeof(GoldenExampleTests))]
    public void Examples_are_tidy(string file)
    {
        var t = ExampleLoader.Load(file);
        Arrange(t);
        AssertTidy(t);
    }

    [Fact]
    public void Random_trees_are_tidy()
    {
        var rng = new Random(12345);
        for (int i = 0; i < 300; i++)
        {
            var t = RandomTree(rng.Next(1, 150), rng);
            Arrange(t);
            AssertTidy(t);
        }
    }

    [Fact]
    public void Hidden_subtree_is_ignored_and_keeps_its_positions()
    {
        var rng = new Random(42);
        var t = RandomTree(80, rng);
        var collapsed = t.FTAStructure.Values.First(i => i.GuidCode != t.TopEventGuid && i.Children.Count > 0);

        var hidden = new List<FTAitem>();
        void Hide(FTAitem item)
        {
            foreach (var child in item.Children.Select(g => t.GetItem(g)))
            {
                child.IsHidden = true;
                hidden.Add(child);
                Hide(child);
            }
        }
        Hide(collapsed);
        var before = hidden.ToDictionary(h => h.GuidCode, h => (h.X, h.Y));

        Arrange(t);

        AssertTidy(t);
        Assert.All(hidden, h => Assert.Equal(before[h.GuidCode], (h.X, h.Y)));
    }

    [Fact]
    public void Large_tree_is_fast()
    {
        var t = RandomTree(2000, new Random(7));

        var sw = Stopwatch.StartNew();
        Arrange(t);
        sw.Stop();

        AssertTidy(t);
        Assert.True(sw.ElapsedMilliseconds < 1000, $"Layout of 2000 events took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    public void Whole_tree_is_drawn_after_sort_with_selected_event()
    {
        // The Sort button sets DrawingEngine.TopEvent to the selected event before ArrangeTree;
        // events are drawn recursively from TopEvent, so it must be the real top event afterwards.
        var t = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.1).Build();
        t.MyDrawingEngine.TopEvent = t.GetItems("A").Single();

        Arrange(t);

        Assert.Same(t.Top(), t.MyDrawingEngine.TopEvent);
    }
}
