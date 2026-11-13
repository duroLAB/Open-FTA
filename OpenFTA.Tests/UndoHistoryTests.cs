using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Undo and redo keep independent copies of the events, so property changes can be undone (B6).</summary>
public class UndoHistoryTests : SettingsTestBase
{
    private static FTAlogic Tree() => new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.2).Build();

    private static FTAitem A(FTAlogic t) => t.GetItems("A").Single();

    [Fact]
    public void Change_of_event_properties_is_undone_and_redone()
    {
        var t = Tree();
        var history = new UndoHistory();

        history.Record(t);
        A(t).Name = "Pump fails";
        A(t).Value = 0.5;
        t.Top().Gate = Gates.AND;

        Assert.True(history.Undo(t));
        Assert.Equal("A", A(t).Name);
        Assert.Equal(0.1, A(t).Value);
        Assert.Equal(Gates.OR, t.Top().Gate);

        Assert.True(history.Redo(t));
        Assert.Equal("Pump fails", A(t).Name);
        Assert.Equal(0.5, A(t).Value);
        Assert.Equal(Gates.AND, t.Top().Gate);
    }

    [Fact]
    public void Change_after_undo_does_not_change_the_saved_steps()
    {
        var t = Tree();
        var history = new UndoHistory();

        history.Record(t);
        A(t).Value = 0.5;
        history.Undo(t);
        A(t).Value = 0.9;          // changes the restored event, not the step for redo
        history.Redo(t);

        Assert.Equal(0.5, A(t).Value);
        history.Undo(t);
        Assert.Equal(0.9, A(t).Value);
    }

    [Fact]
    public void Deleted_event_is_restored_with_its_parent()
    {
        var t = Tree();
        var history = new UndoHistory();
        t.SelectedEvents.Add(A(t));

        history.Record(t);
        t.DeleteSelectedEvents();
        Assert.Empty(t.GetItems("A"));

        history.Undo(t);

        Assert.Equal(3, t.FTAStructure.Count);
        Assert.Contains(A(t).GuidCode, t.Top().Children);
    }

    [Fact]
    public void Selection_refers_to_the_restored_events()
    {
        var t = Tree();
        var history = new UndoHistory();
        history.Record(t);
        A(t).Value = 0.5;
        A(t).IsSelected = true;
        t.SelectedEvents.Add(A(t));

        history.Undo(t);

        Assert.Same(A(t), t.SelectedEvents.Single());
        Assert.True(A(t).IsSelected);
        Assert.Same(t.Top(), t.MyDrawingEngine.TopEvent);
    }

    [Fact]
    public void New_step_clears_redo()
    {
        var t = Tree();
        var history = new UndoHistory();
        history.Record(t);
        A(t).Value = 0.5;
        history.Undo(t);
        Assert.True(history.CanRedo);

        history.Record(t);

        Assert.False(history.CanRedo);
        Assert.False(history.Redo(t));
    }
}
