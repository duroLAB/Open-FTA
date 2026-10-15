namespace OpenFTA.Tests.Helpers;

/// <summary>
/// Base class for tests that depend on MainAppSettings.Instance (a singleton).
/// xUnit creates a new instance per test, so the constructor resets the settings
/// before each test and Dispose resets them afterwards.
/// </summary>
public abstract class SettingsTestBase : IDisposable
{
    protected SettingsTestBase() => ResetToDefaults();

    public void Dispose() => ResetToDefaults();

    protected static MainAppSettings Settings => MainAppSettings.Instance;

    /// <summary>Same defaults as MainAppSettings: P ~ f*t and linear OR.</summary>
    protected static void ResetToDefaults()
    {
        Settings.SimplificationStrategy = true;
        Settings.SimplificationStrategyLinearOR = true;
        Settings.BaseTimeUnit = MainCompTimeUnit.Year;
        Settings.GateANDFrequencyHandling = GateFrequencyHandling.Allow;
        Settings.GateORFrequencyHandling = GateFrequencyHandling.Allow;
    }

    /// <summary>P = 1 - exp(-f*t) and P(A OR B) = PA + PB - PA*PB.</summary>
    protected static void UseExactCalculation()
    {
        Settings.SimplificationStrategy = false;
        Settings.SimplificationStrategyLinearOR = false;
    }

    /// <summary>P ~ f*t and P(A OR B) ~ PA + PB.</summary>
    protected static void UseSimplifiedCalculation()
    {
        Settings.SimplificationStrategy = true;
        Settings.SimplificationStrategyLinearOR = true;
    }
}
