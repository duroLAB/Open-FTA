/// <summary>Colours of the event types, shared by the event cards, gates and symbols.</summary>
public static class EventPalette
{
    public static readonly Color Top = Color.FromArgb(33, 52, 77);
    public static readonly Color Intermediate = Color.FromArgb(62, 92, 118);
    public static readonly Color Basic = Color.FromArgb(150, 72, 60);
    public static readonly Color House = Color.FromArgb(64, 120, 84);
    public static readonly Color Undeveloped = Color.FromArgb(112, 82, 140);

    /// <summary>Lines between events and gates.</summary>
    public static readonly Color Line = Color.FromArgb(74, 85, 99);

    /// <summary>Events of the highlighted minimal cut set.</summary>
    public static readonly Color Mcs = Color.FromArgb(47, 140, 60);

    /// <summary>Colour of an event: top event, intermediate, basic, house or undeveloped.</summary>
    public static Color ForEvent(FTAitem evt, bool isTopEvent) =>
        isTopEvent ? Top : evt.ItemType switch
        {
            2 => Basic,
            3 => House,
            4 => Undeveloped,
            _ => Intermediate,
        };

    /// <summary>Darker shade for outlines.</summary>
    public static Color Darker(Color c, float factor = 0.7f) =>
        Color.FromArgb(c.A, (int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor));
}
