using System.Drawing.Drawing2D;
using OpenFTA.Tests.Helpers;

namespace OpenFTA.Tests;

/// <summary>Settings are saved to JSON and loaded back (B5).</summary>
public class MainAppSettingsTests : SettingsTestBase
{
    private static string TempFile() => Path.Combine(Path.GetTempPath(), $"openfta-settings-{Guid.NewGuid():N}.json");

    [Fact]
    public void Saved_settings_are_loaded_back()
    {
        var s = Settings;
        var original = (s.DisplayedMetric, s.FrequencyDisplayMode, s.EventWidth, s.Technicalgates,
                        s.ItemPen.Color, s.ItemPen.Width, s.ItemPen.DashStyle);
        string path = TempFile();
        try
        {
            s.SortingAlgoVersion = SortingStrategy.ALGOIII;
            s.SimplificationStrategy = false;
            s.BaseTimeUnit = MainCompTimeUnit.Hour;
            s.DisplayedMetric = DisplayMetricType.FV;
            s.FrequencyDisplayMode = FrequencyDisplayMode.allEvents;
            s.EventWidth = 177;
            s.Technicalgates = true;
            s.ItemPen.Color = Color.FromArgb(200, 10, 20, 30);
            s.ItemPen.Width = 2.5f;
            s.ItemPen.DashStyle = DashStyle.Dash;
            s.Save(path);

            // back to other values, then load
            ResetToDefaults();
            s.DisplayedMetric = DisplayMetricType.None;
            s.FrequencyDisplayMode = FrequencyDisplayMode.alwaysHidden;
            s.EventWidth = 140;
            s.Technicalgates = false;
            s.ItemPen = new PenSettings();
            s.Load(path);

            Assert.Equal(SortingStrategy.ALGOIII, s.SortingAlgoVersion);
            Assert.False(s.SimplificationStrategy);
            Assert.Equal(MainCompTimeUnit.Hour, s.BaseTimeUnit);
            Assert.Equal(DisplayMetricType.FV, s.DisplayedMetric);
            Assert.Equal(FrequencyDisplayMode.allEvents, s.FrequencyDisplayMode);
            Assert.Equal(177, s.EventWidth);
            Assert.True(s.Technicalgates);
            Assert.Equal(Color.FromArgb(200, 10, 20, 30).ToArgb(), s.ItemPen.Color.ToArgb());
            Assert.Equal(2.5f, s.ItemPen.Width);
            Assert.Equal(DashStyle.Dash, s.ItemPen.DashStyle);

            // enums are stored by name, so the file stays readable
            Assert.Contains("\"ALGOIII\"", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
            (s.DisplayedMetric, s.FrequencyDisplayMode, s.EventWidth, s.Technicalgates) =
                (original.DisplayedMetric, original.FrequencyDisplayMode, original.EventWidth, original.Technicalgates);
            s.ItemPen = new PenSettings { Color = original.Color, Width = original.Width, DashStyle = original.DashStyle };
        }
    }

    [Fact]
    public void Invalid_values_keep_defaults_and_valid_ones_are_loaded()
    {
        string path = TempFile();
        try
        {
            File.WriteAllText(path, """
                {
                  "SortingAlgoVersion": "ALGOIII",
                  "BaseTimeUnit": "NoSuchUnit",
                  "SimplificationStrategy": "not a bool",
                  "UnknownSetting": 42
                }
                """);

            Settings.Load(path);

            Assert.Equal(SortingStrategy.ALGOIII, Settings.SortingAlgoVersion);
            Assert.Equal(MainCompTimeUnit.Year, Settings.BaseTimeUnit);   // default kept
            Assert.True(Settings.SimplificationStrategy);                  // default kept
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Corrupted_or_missing_file_does_not_throw()
    {
        string path = TempFile();
        try
        {
            Settings.Load(path);                       // missing
            File.WriteAllText(path, "{ this is not json");
            Settings.Load(path);                       // corrupted
            Assert.Equal(MainCompTimeUnit.Year, Settings.BaseTimeUnit);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
