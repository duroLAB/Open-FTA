using System.Globalization;

/// <summary>
/// Parses numbers typed by the user independently of the Windows regional format:
/// both a decimal comma and a decimal point are accepted ("0,001", "0.001", "1,5E-4", "1.5e-4").
/// Spaces are ignored. A text with both a comma and a point ("1,000.5") is ambiguous and rejected,
/// so a comma is never silently taken as a thousands separator.
/// </summary>
public static class NumberParsing
{
    public static bool TryParse(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string s = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());

        bool hasComma = s.Contains(',');
        bool hasPoint = s.Contains('.');
        if (hasComma && hasPoint)
            return false;
        if (hasComma)
            s = s.Replace(',', '.');

        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
               && double.IsFinite(value);
    }
}
