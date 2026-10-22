using System.Globalization;

namespace OpenFTA.Tests;

/// <summary>Numbers typed by the user must be read the same way on Slovak, English and German Windows.</summary>
public class NumberParsingTests
{
    public static IEnumerable<object[]> Cultures() => new[]
    {
        new object[] { "sk-SK" },
        new object[] { "en-US" },
        new object[] { "de-DE" },
    };

    private static void InCulture(string culture, Action action)
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        try { action(); }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Decimal_comma_and_point_are_accepted(string culture) => InCulture(culture, () =>
    {
        var cases = new (string Text, double Expected)[]
        {
            ("0,001", 0.001),
            ("0.001", 0.001),
            ("1E-4", 1e-4),
            ("1,142E-4", 1.142e-4),
            ("1.142e-4", 1.142e-4),
            (" 2 ", 2),
            ("-0,5", -0.5),
            ("876", 876),
            ("1 000", 1000),
        };
        foreach (var (text, expected) in cases)
        {
            Assert.True(NumberParsing.TryParse(text, out double value), $"'{text}' was not accepted in {culture}");
            Assert.Equal(expected, value, 12);
        }
    });

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Invalid_or_ambiguous_text_is_rejected(string culture) => InCulture(culture, () =>
    {
        foreach (string text in new[] { "", "   ", "abc", "1,000.5", "1.000,5", "1.2.3", "1,2,3", "NaN", "Infinity" })
            Assert.False(NumberParsing.TryParse(text, out _), $"'{text}' should be rejected in {culture}");
    });

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Value_shown_by_the_editor_is_read_back(string culture) => InCulture(culture, () =>
    {
        // The editor shows Value.ToString() in the current culture
        foreach (double v in new[] { 0.001, 1.142e-4, 2.5, 1234.5678, 3e-12 })
        {
            Assert.True(NumberParsing.TryParse(v.ToString(), out double back));
            Assert.Equal(v, back);
        }
    });
}
