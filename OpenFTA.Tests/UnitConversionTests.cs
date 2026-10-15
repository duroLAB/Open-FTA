using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Conversion between event units (y, d, h, s) and the base time unit.</summary>
public class UnitConversionTests : SettingsTestBase
{
    [Fact]
    public void Lambda_per_hour_with_year_base_simplified()
    {
        UseSimplifiedCalculation();
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 1e-4, ValueTypes.Lambda, MainCompTimeUnit.Hour)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.876, t.Top().Value);             // 1e-4 /h * 8760 h/y
    }

    [Fact]
    public void Frequency_per_year_with_day_base_simplified()
    {
        UseSimplifiedCalculation();
        Settings.BaseTimeUnit = MainCompTimeUnit.Day;
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F, MainCompTimeUnit.Year)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.1 / 365.0, t.Top().Value);       // result in 1/day
    }

    [Fact]
    public void Frequency_per_year_with_year_base_exact()
    {
        UseExactCalculation();
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F, MainCompTimeUnit.Year)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.1, t.Top().Value);
    }

    [Fact]
    public void Lambda_per_hour_with_year_base_exact()
    {
        UseExactCalculation();
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 1e-4, ValueTypes.Lambda, MainCompTimeUnit.Hour)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.876, t.Top().Value);             // rate 0.876 /y
    }

    [Fact]
    public void Frequency_per_day_with_day_base_exact()
    {
        UseExactCalculation();
        Settings.BaseTimeUnit = MainCompTimeUnit.Day;
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F, MainCompTimeUnit.Day)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.1, t.Top().Value);               // 0.1 /d stays 0.1 /d
    }

    [Fact]
    public void Frequency_per_year_with_day_base_exact()
    {
        UseExactCalculation();
        Settings.BaseTimeUnit = MainCompTimeUnit.Day;
        var t = new TreeBuilder(Gates.OR)
            .Basic("A", 0.1, ValueTypes.F, MainCompTimeUnit.Year)
            .Build();

        t.ComputeTree();

        Approx.Equal(0.1 / 365.0, t.Top().Value);
    }
}
