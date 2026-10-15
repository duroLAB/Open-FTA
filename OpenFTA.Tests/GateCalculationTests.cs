using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Gate-by-gate calculation (FTAlogic.ComputeTree) on small trees with hand-computed results.</summary>
public class GateCalculationTests : SettingsTestBase
{
    [Fact]
    public void OR_two_probabilities_exact()
    {
        UseExactCalculation();
        var t = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.2).Build();

        t.ComputeTree();

        Approx.Equal(0.28, t.Top().Value);              // 0.1 + 0.2 - 0.1*0.2
        Assert.Equal(ValueTypes.P, t.Top().ValueType);
    }

    [Fact]
    public void OR_two_probabilities_linear()
    {
        UseSimplifiedCalculation();
        var t = new TreeBuilder(Gates.OR).Basic("A", 0.1).Basic("B", 0.2).Build();

        t.ComputeTree();

        Approx.Equal(0.3, t.Top().Value);               // 0.1 + 0.2
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AND_two_probabilities(bool simplified)
    {
        if (simplified) UseSimplifiedCalculation(); else UseExactCalculation();
        var t = new TreeBuilder(Gates.AND).Basic("A", 0.1).Basic("B", 0.2).Build();

        t.ComputeTree();

        Approx.Equal(0.02, t.Top().Value);              // 0.1 * 0.2
    }

    [Fact]
    public void Nested_AND_under_OR_exact()
    {
        UseExactCalculation();
        var b = new TreeBuilder(Gates.OR);
        var ie = b.AddGate(b.Top, "IE1", Gates.AND);
        b.AddBasic(ie, "A", 0.1);
        b.AddBasic(ie, "B", 0.2);
        b.AddBasic(b.Top, "C", 0.05);
        var t = b.Build();

        t.ComputeTree();

        Approx.Equal(0.02, t.GetItems("IE1").Single().Value);
        Approx.Equal(0.069, t.Top().Value);             // 0.02 + 0.05 - 0.02*0.05
    }

    [Fact]
    public void OR_two_frequencies_exact_gives_sum_of_rates()
    {
        UseExactCalculation();
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F)
            .Basic("B", 0.2, ValueTypes.F)
            .Build();

        t.ComputeTree();

        // 1 - exp(-0.1)*exp(-0.2) converted back to a rate = 0.3 per year
        Approx.Equal(0.3, t.Top().Value);
        Assert.Equal(ValueTypes.F, t.Top().ValueType);
    }

    [Fact]
    public void OR_two_frequencies_simplified()
    {
        UseSimplifiedCalculation();
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F)
            .Basic("B", 0.2, ValueTypes.F)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.3, t.Top().Value);
        Assert.Equal(ValueTypes.F, t.Top().ValueType);
    }

    [Fact]
    public void AND_frequency_times_probability_simplified()
    {
        // Typical LOPA-like case: initiating event frequency x probability of failure on demand
        UseSimplifiedCalculation();
        var t = new TreeBuilder(Gates.AND)
            .Basic("IE", 0.1, ValueTypes.F)
            .Basic("PFD", 0.01)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.001, t.Top().Value);             // 0.1 /y * 0.01
        Assert.Equal(ValueTypes.F, t.Top().ValueType);
    }

    [Fact]
    public void Reliability_is_converted_to_probability()
    {
        UseExactCalculation();
        var t = new TreeBuilder(Gates.AND)
            .Basic("A", 0.9, ValueTypes.R)              // P = 0.1
            .Basic("B", 0.2)
            .Build();

        t.ComputeTree();

        // R is not treated as a probability, so the result is reported as a frequency:
        // -ln(1 - 0.1*0.2)
        Approx.Equal(-Math.Log(1 - 0.02), t.Top().Value);
    }

    [Fact]
    public void Repeated_event_gate_by_gate_is_not_exact_but_MCS_is()
    {
        // TOP = (A + B) * (A + C) = A + B*C   (A is a repeated event)
        UseExactCalculation();
        var b = new TreeBuilder(Gates.AND);
        var g1 = b.AddGate(b.Top, "G1", Gates.OR);
        var g2 = b.AddGate(b.Top, "G2", Gates.OR);
        b.AddBasic(g1, "A", 0.1);
        b.AddBasic(g1, "B", 0.2);
        b.AddBasic(g2, "A", 0.1);
        b.AddBasic(g2, "C", 0.3);
        var t = b.Build();

        t.ComputeTree();
        t.GenerateMCS(out _, out string simplified);
        t.SumChildren(t.McsTop(), t.MCSStructure);

        // Gate by gate treats both A as independent events: 0.28 * 0.37
        Approx.Equal(0.1036, t.Top().Value);
        // Exact: P(A + B*C) = 0.1 + 0.06 - 0.1*0.06
        Assert.Equal(new[] { "A", "B*C" }, CutSets.Parse(simplified));
        Approx.Equal(0.154, t.McsTop().Value);
    }
}
