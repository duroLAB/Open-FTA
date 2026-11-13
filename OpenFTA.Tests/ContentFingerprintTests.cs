using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Unsaved changes: what changes the content of the tree and what does not.</summary>
public class ContentFingerprintTests : SettingsTestBase
{
    private static FTAlogic Tree()
    {
        var b = new TreeBuilder(Gates.OR);
        b.Basic("A", 0.1);
        FTAitem g = b.AddGate(b.Top, "G", Gates.AND);
        b.AddBasic(g, "B", 0.2, ValueTypes.F);
        b.AddBasic(g, "C", 0.3);
        return b.Build();
    }

    [Fact]
    public void Calculations_selection_and_view_are_not_changes()
    {
        var t = Tree();
        string saved = t.ContentFingerprint();

        t.ComputeTree();
        t.GetItems("A")[0].IsSelected = true;
        t.MyDrawingEngine.GlobalZoom = 2.5;
        t.MyDrawingEngine.offsetX = 100;

        Assert.Equal(saved, t.ContentFingerprint());
    }

    [Fact]
    public void Edits_are_changes()
    {
        var t = Tree();
        string saved = t.ContentFingerprint();

        FTAitem a = t.GetItems("A")[0];
        a.Name = "Pump fails";
        Assert.NotEqual(saved, t.ContentFingerprint());
        a.Name = "A";
        Assert.Equal(saved, t.ContentFingerprint());

        a.Value = 0.2;
        Assert.NotEqual(saved, t.ContentFingerprint());
        a.Value = 0.1;

        a.X += 10;
        Assert.NotEqual(saved, t.ContentFingerprint());
        a.X -= 10;

        t.RemoveItem(a.GuidCode);
        Assert.NotEqual(saved, t.ContentFingerprint());
    }
}
