using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Adding and deleting events keeps the structure consistent without FindAllChilren (B8, B13).</summary>
public class EventEditingTests : SettingsTestBase
{
    [Fact]
    public void New_event_is_added_to_the_children_of_its_parent()
    {
        var logic = new FTAlogic();
        FTAitem top = logic.GetItem(logic.TopEventGuid);

        Guid a = logic.AddNewEvent(top.GuidCode, "", 2, Gates.NotSet, 0);
        Guid b = logic.AddNewEvent(top.GuidCode, "", 2, Gates.NotSet, 0);

        Assert.Equal(new[] { a, b }, top.Children);
        Assert.NotEqual(logic.GetItem(a).X, logic.GetItem(b).X);   // placed side by side
    }

    [Fact]
    public void Given_tag_is_kept_and_missing_tag_is_generated()
    {
        var logic = new FTAlogic();

        FTAitem withTag = logic.GetItem(logic.AddNewEvent(logic.TopEventGuid, "Pump fails", 2, Gates.NotSet, 0, Tag: "P1"));
        FTAitem withoutTag = logic.GetItem(logic.AddNewEvent(logic.TopEventGuid, "", 2, Gates.NotSet, 0));

        Assert.Equal("P1", withTag.Tag);
        Assert.False(string.IsNullOrWhiteSpace(withoutTag.Tag));
        Assert.NotEqual("P1", withoutTag.Tag);
    }

    [Fact]
    public void Deleted_event_is_removed_from_the_children_of_its_parent()
    {
        var b = new TreeBuilder(Gates.OR);
        b.Basic("A", 0.1);
        FTAitem del = b.AddBasic(b.Top, "B", 0.2);
        var logic = b.Build();

        logic.RemoveItem(del.GuidCode);

        Assert.DoesNotContain(del.GuidCode, logic.Top().Children);
        Assert.Null(logic.GetItem(del.GuidCode));
    }

    [Fact]
    public void Deleting_a_gate_removes_all_its_descendants()
    {
        var b = new TreeBuilder(Gates.OR);
        FTAitem g1 = b.AddGate(b.Top, "G1", Gates.AND);
        FTAitem g2 = b.AddGate(g1, "G2", Gates.OR);
        b.AddBasic(g1, "A", 0.1);
        b.AddBasic(g2, "B", 0.2);
        b.Basic("C", 0.3);
        var logic = b.Build();

        // an event linked only by its Parent (missing in the Children list)
        var orphan = new FTAitem { Name = "D", Tag = "D", ItemType = 2, Parent = g2.GuidCode };
        logic.AddItem(orphan);

        logic.RemoveItem(g1.GuidCode);

        Assert.Equal(new[] { "C", "TE" }, logic.FTAStructure.Values.Select(e => e.Tag).OrderBy(t => t));
    }

    [Fact]
    public void Tree_can_be_computed_right_after_deleting_without_rebuilding_children()
    {
        UseSimplifiedCalculation();
        var b = new TreeBuilder(Gates.OR);
        b.Basic("A", 0.1);
        FTAitem g = b.AddGate(b.Top, "G", Gates.AND);
        b.AddBasic(g, "B", 0.2);
        var logic = b.Build();

        logic.RemoveItem(g.GuidCode);
        logic.ComputeTree();   // previously the deleted GUID stayed in Top.Children

        Approx.Equal(0.1, logic.Top().Value);
    }
}
