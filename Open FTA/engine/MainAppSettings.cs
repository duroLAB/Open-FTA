using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Serialization;

public enum  TimeUnit
{
    Year,
    Day,
    Hour,
    Second
}

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

    [Category("Computation settings")]
    public TimeUnit BaseTimeUnit { get; set; } = TimeUnit.Year;

   
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
