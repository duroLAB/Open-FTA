using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Minimal cut sets: Boolean simplification (MCSEngine_Exp) and MCS built from a tree.</summary>
public class McsTests : SettingsTestBase
{
    [Theory]
    [InlineData("(A*(B+C))", "A*B + A*C")]             // distribution
    [InlineData("(A+(A*B))", "A")]                     // absorption
    [InlineData("((A+B)*(A+C))", "A + B*C")]           // distribution + absorption
    [InlineData("((A+B)*(C+D))", "A*C + A*D + B*C + B*D")]
    [InlineData("(A*A)", "A")]                         // idempotence
    [InlineData("(A+B+A)", "A + B")]
    [InlineData("((A*B)+(A*B*C))", "A*B")]
    [InlineData("(A * (B + C))", "A*B + A*C")]         // spaces as produced by GenerateEquation
    public void Simplifies_to_minimal_cut_sets(string input, string expected)
    {
        string actual = MCSEngine_Exp.ProcessExpression(input);

        Assert.Equal(CutSets.Parse(expected), CutSets.Parse(actual));
    }

    [Fact]
    public void Equation_is_generated_from_tree()
    {
        var b = new TreeBuilder(Gates.OR);
        var ie = b.AddGate(b.Top, "IE1", Gates.AND);
        b.AddBasic(ie, "A", 0.1);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(b.Top, "C", 0.05);
        var t = b.Build();

        Assert.Equal("((A * B) + C)", t.GenerateEquation());
    }

    [Fact]
    public void MCS_of_tree_with_absorption()
    {
        // TOP = A + A*B = A
        var b = new TreeBuilder(Gates.OR);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.AND);
        b.AddBasic(ie, "A", 0.1);
        b.AddBasic(ie, "B", 0.2);
        var t = b.Build();

        t.GenerateMCS(out _, out string simplified);

        Assert.Equal(new[] { "A" }, CutSets.Parse(simplified));
    }

    [Fact]
    public void MCS_structure_has_one_intermediate_event_per_cut_set()
    {
        // TOP = A * (B + C) = A*B + A*C
        var b = new TreeBuilder(Gates.AND);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);

        var mcsEvents = t.McsTop().Children.Select(g => t.MCSStructure[g]).ToList();
        Assert.Equal(2, mcsEvents.Count);
        Assert.All(mcsEvents, m => Assert.Equal(Gates.AND, m.Gate));
        Assert.All(mcsEvents, m => Assert.Equal(2, m.Children.Count));
    }

    [Fact(Skip = "Bug A3: a basic event in several MCS shares one key in MCSStructure, Parent points to the last MCS")]
    public void Basic_event_in_several_MCS_has_correct_parent_in_each()
    {
        var b = new TreeBuilder(Gates.AND);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);

        foreach (Guid mcsGuid in t.McsTop().Children)
            foreach (Guid childGuid in t.MCSStructure[mcsGuid].Children)
                Assert.Equal(mcsGuid, t.MCSStructure[childGuid].Parent);
    }
}
