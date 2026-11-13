using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>
/// The top event is the event with TopEventGuid, not the first event of the structure (B9),
/// and the tree is drawn from it after every layout (B14).
/// </summary>
public class TopEventTests : SettingsTestBase
{
    /// <summary>Tree whose top event is the last event of the structure (as in some loaded files).</summary>
    private static FTAlogic TreeWithTopEventLast()
    {
        var t = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.2).Build();
        FTAitem top = t.Top();
        top.Name = "Tank overflow";

        var items = t.FTAStructure.Values.OrderBy(e => e == top ? 1 : 0).ToList();
        t.FTAStructure.Clear();
        foreach (FTAitem item in items)
            t.FTAStructure[item.GuidCode] = item;
        t.FindAllChilren();

        Assert.NotSame(top, t.FTAStructure.Values.First());
        return t;
    }

    [Fact]
    public void Report_shows_the_top_event_even_if_it_is_not_first()
    {
        var t = TreeWithTopEventLast();

        string html = t.GenerateReport_TopEvent();

        Assert.Contains("Tank overflow", html);
        Assert.DoesNotContain("<h2>A", html);
    }

    [Fact]
    public void Drawing_starts_at_the_top_event_even_if_it_is_not_first()
    {
        var t = TreeWithTopEventLast();

        t.MyDrawingEngine.SetStructure(t.FTAStructure);

        Assert.Same(t.Top(), t.MyDrawingEngine.TopEvent);
    }

    [Theory]
    [InlineData(SortingStrategy.ALGOI)]
    [InlineData(SortingStrategy.ALGOII_centered)]
    [InlineData(SortingStrategy.ALGOII_align_left)]
    [InlineData(SortingStrategy.ALGOIII)]
    public void Whole_tree_is_drawn_after_sort_with_selected_event(SortingStrategy algorithm)
    {
        var t = TreeWithTopEventLast();
        t.MyDrawingEngine.TopEvent = t.GetItems("A").Single();   // as the Sort button did with a selected event
        Settings.SortingAlgoVersion = algorithm;

        t.ArrangeTree();

        Assert.Same(t.Top(), t.MyDrawingEngine.TopEvent);
        Assert.Equal(3, t.FTAStructure.Count);
    }
}
