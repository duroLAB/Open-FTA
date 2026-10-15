using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>
/// Importance measures on TOP = A OR B with P(A) = 0.1, P(B) = 0.2 (exact calculation).
/// P(TOP) = 0.28, P(TOP | A=1) = 1, P(TOP | A=0) = 0.2.
/// </summary>
public class ImportanceMeasureTests : SettingsTestBase
{
    private static (FTAlogic Logic, FTAitem A) BuildTree(ValueTypes typeA = ValueTypes.P, double valueA = 0.1)
    {
        UseExactCalculation();
        var b = new TreeBuilder(Gates.OR);
        var a = b.AddBasic(b.Top, "A", valueA, typeA);
        b.AddBasic(b.Top, "B", 0.2);
        var t = b.Build();
        t.ComputeTree();
        return (t, a);
    }

    [Fact]
    public void BIM()
    {
        var (t, a) = BuildTree();
        Approx.Equal(0.8, t.ComputeBIM(a));               // 1 - 0.2
    }

    [Fact]
    public void CIM()
    {
        var (t, a) = BuildTree();
        Approx.Equal(0.8 * 0.1 / 0.28, t.ComputeCIM(a));  // BIM * P(A) / P(TOP)
    }

    [Fact]
    public void RAW()
    {
        var (t, a) = BuildTree();
        Approx.Equal(1 / 0.28, t.ComputeRAW(a));          // P(TOP|A=1) / P(TOP)
    }

    [Fact]
    public void RRW()
    {
        var (t, a) = BuildTree();
        Approx.Equal(0.28 / 0.2, t.ComputeRRW(a));        // P(TOP) / P(TOP|A=0)
    }

    [Fact]
    public void FV()
    {
        var (t, a) = BuildTree();
        Approx.Equal((0.28 - 0.2) / 0.28, t.ComputeFV(a));
    }

    [Fact]
    public void Event_value_is_restored_after_computation()
    {
        var (t, a) = BuildTree();
        t.ComputeBIM(a);
        t.ComputeCIM(a);
        t.ComputeRAW(a);
        t.ComputeRRW(a);
        t.ComputeFV(a);
        Assert.Equal(0.1, a.Value);
    }

    [Fact(Skip = "Bug A2: tree is not recomputed after BIM, top event stays in the 'A works' state (0.2)")]
    public void Top_event_is_restored_after_BIM()
    {
        var (t, a) = BuildTree();
        t.ComputeBIM(a);
        Approx.Equal(0.28, t.Top().Value);
    }

    [Fact(Skip = "Bug A2: for a reliability (R) event Value=1 means 'works', so BIM has the wrong sign")]
    public void BIM_for_reliability_event()
    {
        var (t, a) = BuildTree(ValueTypes.R, 0.9);         // R = 0.9 -> P = 0.1
        Approx.Equal(0.8, t.ComputeBIM(a));
    }
}
