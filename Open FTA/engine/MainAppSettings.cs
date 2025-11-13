using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;



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
    [DisplayName("Event pen type")]
    public PenSettings ItemPen { get; set; } = new PenSettings();

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

    [Category("Sorting algorithm")]
    [DisplayName("Default sorting algorithm")]
    [Description("ALGOI - safe, fast, but not very effective, ALGOII-best results, but slower and less safe,ALGOII-left-aligned behaves the same as ALGOII, except that it aligns to the left")]
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

    // Cesta k súboru – vždy vedľa exe
    private static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.xml");

    // Uloženie do AppSettings.xml
    public void Save()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MainAppSettings));
            using (StreamWriter writer = new StreamWriter(FilePath))
            {
                serializer.Serialize(writer, this);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Chyba pri ukladaní nastavení: " + ex.Message);
        }
    }

    // Načítanie z AppSettings.xml
    public void Load()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MainAppSettings));
                using (StreamReader reader = new StreamReader(FilePath))
                {
                    var loaded = (MainAppSettings)serializer.Deserialize(reader);
                    if (loaded != null)
                    {
                        //Faktor = loaded.Faktor;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba pri načítaní nastavení, použijem default: " + ex.Message);
                // Ak sa načítanie nepodarí, zostanú default hodnoty
            }
        }
        // Ak súbor neexistuje, zostanú default hodnoty
    }
}


[Serializable]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class PenSettings
{
    // Farba pera
    [XmlIgnore] // Farbu budeme serializovať ako ARGB

    [Editor(typeof(System.Drawing.Design.ColorEditor), typeof(UITypeEditor))]
    public Color Color { get; set; } = Color.Blue;

    public float Width { get; set; } = 1.0f;

    public DashStyle DashStyle { get; set; } = DashStyle.Solid;

    [Browsable(false)]
    public Pen Pen => new Pen(Color, Width) { DashStyle = this.DashStyle };

    public override string ToString()
    {
        string s = $"Color: {Color}, Width: {Width}, DashStyle: {DashStyle}";
        return (s);
    }
}
