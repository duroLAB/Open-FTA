/// <summary>
/// One minimal cut set: the smallest combination of basic events that causes the top event.
/// Events are references to the original basic events in FTAlogic.FTAStructure (no copies),
/// so one basic event can be part of any number of cut sets.
/// </summary>
public sealed class MinimalCutSet
{
    public MinimalCutSet(int index, List<FTAitem> events)
    {
        Index = index;
        Events = events;
    }

    /// <summary>1-based position in the simplified MCS expression. Stable identity of the cut set.</summary>
    public int Index { get; }

    public string Name => "MCS" + Index;

    /// <summary>Original basic events of the cut set.</summary>
    public IReadOnlyList<FTAitem> Events { get; }

    /// <summary>Gate that combines the events: AND for two or more events.</summary>
    public Gates Gate => Events.Count > 1 ? Gates.AND : Gates.OR;

    /// <summary>Probability (ValueType P) or frequency per base time unit (ValueType F).</summary>
    public double Value { get; internal set; }

    public ValueTypes ValueType { get; internal set; }

    public int ValueUnit { get; internal set; }

    /// <summary>Share of the cut set in the MCS top event result, in percent.</summary>
    public double Contribution { get; internal set; }

    public override string ToString() => $"{Name}: {string.Join("*", Events.Select(e => e.Tag))}";
}
