using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
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

    [Category("Computation settings")]
    [DisplayName("Base time unit")]
    public MainCompTimeUnit BaseTimeUnit { get; set; } = MainCompTimeUnit.Year;

    [Category("Computation settings")]
    [DisplayName("Probability Computation settings")]
    [Description("If Probability Computation settings = true: " +
             "P(failure) = 1 − e^(−f⋅t)" +
             " Otherwise:" +
             "P ≈ f⋅t")]
    public bool SimplificationStrategy { get; set; } = false;
     
     

    [Category("General settings")]
    [DisplayName("Auto sort tree after paste")]
    public bool AutoSortTree { get; set; } = true;
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
