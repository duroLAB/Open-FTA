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
    public void One_minimal_cut_set_per_term()
    {
        // TOP = A * (B + C) = A*B + A*C
        var b = new TreeBuilder(Gates.AND);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);

        Assert.Equal(new[] { "MCS1", "MCS2" }, t.MinimalCutSets.Select(m => m.Name));
        Assert.All(t.MinimalCutSets, m => Assert.Equal(Gates.AND, m.Gate));
        Assert.All(t.MinimalCutSets, m => Assert.Equal(2, m.Events.Count));
    }

    [Fact]
    public void Basic_event_in_several_MCS_references_the_original_event()
    {
        // A is in both cut sets (fix of bug A3: no copies sharing one key)
        var b = new TreeBuilder(Gates.AND);
        var a = b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);

        Assert.All(t.MinimalCutSets, m => Assert.Contains(a, m.Events));
        Assert.All(t.MinimalCutSets, m => Assert.All(m.Events, e => Assert.True(t.FTAStructure.ContainsKey(e.GuidCode))));
    }

    [Fact]
    public void MCS_values_and_contributions()
    {
        // TOP = A + B*C, exact: MCS1 = A = 0.1, MCS2 = B*C = 0.06, top = 1 - 0.9*0.94 = 0.154
        UseExactCalculation();
        var b = new TreeBuilder(Gates.OR);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.AND);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);

        var mcs1 = t.MinimalCutSets.Single(m => m.Name == "MCS1");
        var mcs2 = t.MinimalCutSets.Single(m => m.Name == "MCS2");
        Approx.Equal(0.1, mcs1.Value);
        Approx.Equal(0.06, mcs2.Value);
        Assert.Equal(ValueTypes.P, mcs1.ValueType);
        Approx.Equal(0.154, t.McsTopValue);
        Approx.Equal(0.1 / 0.154 * 100, mcs1.Contribution);
        Approx.Equal(0.06 / 0.154 * 100, mcs2.Contribution);
    }

    [Fact]
    public void MCS_identity_is_stable_when_generated_again()
    {
        // The report generates MCS again; the MCS list in the UI must still match
        var b = new TreeBuilder(Gates.AND);
        b.AddBasic(b.Top, "A", 0.1);
        var ie = b.AddGate(b.Top, "IE1", Gates.OR);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(ie, "C", 0.3);
        var t = b.Build();

        t.GenerateMCS(out _, out _);
        var first = t.MinimalCutSets.Select(m => m.ToString()).ToList();
        t.GenerateMCS(out _, out _);
        var second = t.MinimalCutSets.Select(m => m.ToString()).ToList();

        Assert.Equal(first, second);
    }
}
