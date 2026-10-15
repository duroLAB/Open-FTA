namespace OpenFTA.Tests.Helpers;

/// <summary>
/// Builds small fault trees directly in code.
/// Events are created as FTAitem objects (not through FTAlogic.AddNewEvent,
/// which currently ignores the Tag argument).
/// </summary>
public sealed class TreeBuilder
{
    public FTAlogic Logic { get; } = new FTAlogic();

    public TreeBuilder(Gates topGate)
    {
        Top.Gate = topGate;
    }

    public FTAitem Top => Logic.GetItem(Logic.TopEventGuid);

    /// <summary>Adds a basic event directly under the top event (fluent).</summary>
    public TreeBuilder Basic(string tag, double value,
        ValueTypes type = ValueTypes.P, MainCompTimeUnit unit = MainCompTimeUnit.Year)
    {
        AddBasic(Top, tag, value, type, unit);
        return this;
    }

    public FTAitem AddBasic(FTAitem parent, string tag, double value,
        ValueTypes type = ValueTypes.P, MainCompTimeUnit unit = MainCompTimeUnit.Year)
    {
        var item = new FTAitem
        {
            Name = tag,
            Tag = tag,
            ItemType = 2,              // 2 = basic event
            Gate = Gates.NotSet,
            Parent = parent.GuidCode,
            Value = value,
            Frequency = value,
            ValueType = type,
            ValueUnit = (int)unit,
        };
        Logic.AddItem(item);
        parent.Children.Add(item.GuidCode);
        return item;
    }

    public FTAitem AddGate(FTAitem parent, string tag, Gates gate)
    {
        var item = new FTAitem
        {
            Name = tag,
            Tag = tag,
            ItemType = 1,              // 1 = intermediate event
            Gate = gate,
            Parent = parent.GuidCode,
        };
        Logic.AddItem(item);
        parent.Children.Add(item.GuidCode);
        return item;
    }

    public FTAlogic Build()
    {
        Logic.FindAllChilren();
        Logic.AssignLevelsToAllEvents();
        return Logic;
    }
}

public static class FtaLogicExtensions
{
    public static FTAitem Top(this FTAlogic logic) => logic.GetItem(logic.TopEventGuid);

    /// <summary>Top event of the MCS structure created by GenerateMCS.</summary>
    public static FTAitem McsTop(this FTAlogic logic) =>
        logic.MCSStructure.Values.Single(i => i.Parent == Guid.Empty);
}
