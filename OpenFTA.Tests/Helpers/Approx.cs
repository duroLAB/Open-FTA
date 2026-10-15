namespace OpenFTA.Tests.Helpers;

public static class Approx
{
    /// <summary>Asserts that two doubles agree within a relative tolerance.</summary>
    public static void Equal(double expected, double actual, double relTol = 1e-9, double absTol = 1e-15)
    {
        if (double.IsNaN(expected) && double.IsNaN(actual)) return;
        if (double.IsInfinity(expected) || double.IsInfinity(actual))
        {
            Assert.Equal(expected, actual);
            return;
        }

        double diff = Math.Abs(expected - actual);
        double limit = Math.Max(absTol, relTol * Math.Abs(expected));
        Assert.True(diff <= limit,
            $"Expected {expected:R}, actual {actual:R} (difference {diff:E3}, allowed {limit:E3})");
    }
}
