using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;



[Serializable]
public class MainAppSettings
{
    private static MainAppSettings _instance;

    public static MainAppSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MainAppSettings();
                _instance.Load(); // načítanie pri prvom použití
            }
            return _instance;
        }
    }

    private MainAppSettings() { }

    [Category("Display settings")]
    [DisplayName("Technical gates")]
    public bool Technicalgates { get; set; } = false;

    [Category("Display settings")]
    [DisplayName("Event style")]
    [Description("Header - tag band coloured by event type, Minimal - colour stripe with tag and value labels, Clean - tinted tag and value rows")]
    public EventStyle EventStyle { get; set; } = EventStyle.Header;

    [Category("Display settings")]
    [DisplayName("Default event width")]
    [Description("Default event width in pixels")]
    public int EventWidth { get; set; } = 140;

    [Category("Display settings")]
    [DisplayName("Default event heigth")]
    [Description("Default event heigth in pixels")]
    public int EventHeight { get; set; } = 90;

    [Category("Display settings")]
    [DisplayName("Default event horizontal spacing")]
    [Description("Default event horizontal spacing in pixels")]
    public int EventHorizontalSpacing { get; set; } = 20;


    [Category("Display settings")]
    [DisplayName("Default event vertical spacing")]
    [Description("Default event vertical spacing in pixels")]
    public int EventVerticalSpacing { get; set; } = 80;


    [Category("Display settings")]
    [DisplayName("Displayed Metric")]
    [Description("Metric displayed in the tree")]
    public DisplayMetricType DisplayedMetric { get; set; } = DisplayMetricType.BIM;

    [Category("Display settings")]
    [DisplayName("Metric display")]
    [Description("How the Displayed Metric is shown on basic events: CardRow - in the bottom row of the event with a thin bar along its bottom edge, Badge - coloured label on the bottom right corner of the event")]
    public MetricDisplayStyle MetricDisplay { get; set; } = MetricDisplayStyle.CardRow;

    [Category("Display settings")]
    [DisplayName("Show event frequency")]
    [Description("Controls how event frequency is displayed: only for top events, for all events, or hidden")]
    public FrequencyDisplayMode FrequencyDisplayMode { get; set; } = FrequencyDisplayMode.alwaysHidden;


    [Category("Sorting algorithm")]
    [DisplayName("Default sorting algorithm")]
    [Description("ALGOI - safe, fast, but not very effective, ALGOII-best results, but slower and less safe,ALGOII-left-aligned behaves the same as ALGOII, except that it aligns to the left, ALGOIII - compact tidy tree (Reingold-Tilford/Walker), fast, parents centred, no overlaps")]
    public SortingStrategy SortingAlgoVersion { get; set; } = (SortingStrategy)1;

    [Category("Computation settings")]
    [DisplayName("Base time unit")]
    public MainCompTimeUnit BaseTimeUnit { get; set; } = MainCompTimeUnit.Year;

    [Category("Computation settings")]
    [DisplayName("AND gate with two frequency events")]
    [Description("Specifies whether an AND gate with two frequency events is allowed, disallowed, or allowed with a warning.")]
    public GateFrequencyHandling GateANDFrequencyHandling { get; set; }

    [Category("Computation settings")]
    [DisplayName("OR gate with mixed event types")]
    [Description("Specifies whether an OR gate containing both probability and frequency events is allowed, disallowed, or allowed with a warning.")]
    public GateFrequencyHandling GateORFrequencyHandling { get; set; }

    [Category("Computation settings")]
    [DisplayName("Simplified probability calculation")]
    [Description("If Simplified probability calculation is enabled: P ≈ f ⋅ t\nOtherwise: P(failure) = 1 − e^(−f ⋅ t)")]
    public bool SimplificationStrategy { get; set; } = true;
  
    [Category("Computation settings")]
    [DisplayName("Simplified OR gate")]
    [Description("If Simplified OR gate is enabled: P(A OR B) ≈ PA + PB\nOtherwise: P(A OR B) = PA + PB − PA×PB")]
    public bool SimplificationStrategyLinearOR { get; set; } = true;




    [Category("Auto sorting")]
    [DisplayName("Auto-sort tree after copy-paste")]
    [Description("Automatically sort tree after copy-paste operation")]
    public bool AutoSortTreeCopyPaste { get; set; } = true;

    [Category("Auto sorting")]
    [DisplayName("Auto-sort tree after adding/removing FTA item")]
    [Description("Automatically sort tree after adding or removing FTA item")]
    public bool AutoSortTreeAddRemove { get; set; } = true;
   
    [Category("Auto sorting")]
    [DisplayName("Auto-sort tree after collapse/expand")]
    [Description("Automatically sort tree after collapsing or expanding tree structure")]
    public bool AutoSortTreeCollapseExpand { get; set; } = true;

    // ---------------------------------------------------------------------------------
    // Persistence: %AppData%\OpenFTA\AppSettings.json
    // (not next to the exe - that folder is read-only when the app is installed in Program Files)
    // ---------------------------------------------------------------------------------

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenFTA", "AppSettings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Saves all settings. Errors are ignored (settings are not critical).</summary>
    public void Save() => Save(DefaultFilePath);

    public void Save(string filePath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Settings could not be saved: " + ex.Message);
        }
    }

    /// <summary>Loads saved settings into this instance; missing or invalid values keep their defaults.</summary>
    public void Load() => Load(DefaultFilePath);

    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(filePath));
            foreach (PropertyInfo property in typeof(MainAppSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;
                if (!doc.RootElement.TryGetProperty(property.Name, out JsonElement value))
                    continue;

                try
                {
                    object? loaded = value.Deserialize(property.PropertyType, JsonOptions);
                    if (loaded != null)
                        property.SetValue(this, loaded);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Setting {property.Name} could not be loaded, default is used: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Settings could not be loaded, defaults are used: " + ex.Message);
        }
    }
}
