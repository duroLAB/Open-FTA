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

    [Fact]
    public void Top_event_is_restored_after_BIM()
    {
        var (t, a) = BuildTree();
        t.ComputeBIM(a);
        Approx.Equal(0.28, t.Top().Value);
    }

    [Fact]
    public void BIM_for_reliability_event()
    {
        var (t, a) = BuildTree(ValueTypes.R, 0.9);         // R = 0.9 -> P = 0.1
        Approx.Equal(0.8, t.ComputeBIM(a));
    }

    [Fact]
    public void Frequency_event_has_only_RRW_and_FV()
    {
        // A = 0.1 /y (initiating event), B: P = 0.2, exact; top is a frequency:
        // f(top) = -ln(0.8 * exp(-0.1)) = 0.1 - ln 0.8,  f(top | A removed) = -ln 0.8
        var (t, a) = BuildTree(ValueTypes.F, 0.1);
        double fTop = 0.1 - Math.Log(0.8);
        double fWorks = -Math.Log(0.8);

        Assert.True(double.IsNaN(t.ComputeBIM(a)));
        Assert.True(double.IsNaN(t.ComputeCIM(a)));
        Assert.True(double.IsNaN(t.ComputeRAW(a)));
        Approx.Equal(fTop / fWorks, t.ComputeRRW(a));
        Approx.Equal((fTop - fWorks) / fTop, t.ComputeFV(a));
    }

    [Fact]
    public void Repeated_event_is_forced_in_all_instances()
    {
        // TOP = (A + B) * (A + C) = A + B*C, BIM(A) = 1 - P(B*C) = 1 - 0.06
        UseExactCalculation();
        var b = new TreeBuilder(Gates.AND);
        var g1 = b.AddGate(b.Top, "G1", Gates.OR);
        var g2 = b.AddGate(b.Top, "G2", Gates.OR);
        var a1 = b.AddBasic(g1, "A", 0.1);
        b.AddBasic(g1, "B", 0.2);
        var a2 = b.AddBasic(g2, "A", 0.1);
        b.AddBasic(g2, "C", 0.3);
        var t = b.Build();

        Approx.Equal(0.94, t.ComputeBIM(a1));
        Approx.Equal(0.94, a1.BIM);
        Approx.Equal(0.94, a2.BIM);                       // stored on both instances
    }

    [Fact]
    public void Linear_OR_uses_rare_event_approximation()
    {
        UseSimplifiedCalculation();
        var b = new TreeBuilder(Gates.OR);
        var a = b.AddBasic(b.Top, "A", 0.1);
        b.AddBasic(b.Top, "B", 0.2);
        var t = b.Build();

        Approx.Equal(1.0, t.ComputeBIM(a));               // (1 + 0.2) - 0.2, no capping
        Approx.Equal(1.2 / 0.3, t.ComputeRAW(a));
    }

    [Fact]
    public void Initiating_event_and_protection_layer()
    {
        // LOPA-like: TOP = IE (0.1 /y) AND PFD (0.01) = 0.001 /y, simplified
        UseSimplifiedCalculation();
        var b = new TreeBuilder(Gates.AND);
        var ie = b.AddBasic(b.Top, "IE", 0.1, ValueTypes.F);
        var pfd = b.AddBasic(b.Top, "PFD", 0.01);
        var t = b.Build();

        Approx.Equal(0.1, t.ComputeBIM(pfd));             // 0.1 /y - 0 /y
        Approx.Equal(1.0, t.ComputeCIM(pfd));             // 0.1 * 0.01 / 0.001
        Approx.Equal(100.0, t.ComputeRAW(pfd));           // 0.1 / 0.001
        Approx.Equal(1.0, t.ComputeFV(pfd));
        Assert.Equal(double.PositiveInfinity, t.ComputeRRW(pfd));

        Assert.True(double.IsNaN(t.ComputeRAW(ie)));
        Approx.Equal(1.0, t.ComputeFV(ie));
        Approx.Equal(0.001, t.Top().Value);               // tree restored
    }
}
