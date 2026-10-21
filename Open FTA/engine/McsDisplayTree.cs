/// <summary>
/// Tree of minimal cut sets for drawing only (calculations use FTAlogic.MinimalCutSets):
///
///     top event (OR)
///       ├─ MCS1 (AND)  ── events of MCS1
///       ├─ MCS2 (AND)  ── events of MCS2
///       └─ ...
///
/// A basic event that is part of several cut sets is drawn in each of them as a separate node
/// (own GUID); <see cref="Originals"/> maps every such node to the original event of the fault tree.
/// Values and importance measures are copied, so the drawing engine can show them.
/// The tree is laid out with <see cref="TidyTreeLayout"/> (ALGO III).
/// </summary>
public sealed class McsDisplayTree
{
    private McsDisplayTree(FTAitem root)
    {
        Root = root;
        Add(root, null);
    }

    /// <summary>All display nodes (structure for DrawingEngine).</summary>
    public Dictionary<Guid, FTAitem> Items { get; } = new Dictionary<Guid, FTAitem>();

    /// <summary>Display node of the top event.</summary>
    public FTAitem Root { get; }

    /// <summary>Display node of an event occurrence -> original basic event in the fault tree.</summary>
    public Dictionary<Guid, FTAitem> Originals { get; } = new Dictionary<Guid, FTAitem>();

    /// <summary>Display node of a cut set -> the cut set.</summary>
    public Dictionary<Guid, MinimalCutSet> CutSets { get; } = new Dictionary<Guid, MinimalCutSet>();

    /// <summary>Builds and lays out the display tree from the current FTAlogic.MinimalCutSets.</summary>
    public static McsDisplayTree Build(FTAlogic logic)
    {
        FTAitem? top = logic.GetItem(logic.TopEventGuid);
        var root = new FTAitem
        {
            Name = top?.Name ?? "Top Event",
            Tag = top?.Tag ?? "TE",
            Description = top?.Description ?? "",
            ItemType = 1,
            Gate = Gates.OR,
            Value = logic.McsTopValue,
            Frequency = logic.McsTopValue,
            ValueType = logic.McsTopValueType,
            ValueUnit = logic.McsTopValueUnit,
        };
        var tree = new McsDisplayTree(root);

        foreach (var mcs in logic.MinimalCutSets)
        {
            if (mcs.Events.Count == 0)
                continue;

            var mcsNode = new FTAitem
            {
                Name = mcs.Name,
                Tag = mcs.Name,
                Description = string.Join(" * ", mcs.Events.Select(e => e.Tag)),
                ItemType = 1,
                // A first-order cut set (one event) is drawn without a gate, only a connecting line
                Gate = mcs.Events.Count > 1 ? Gates.AND : Gates.NotSet,
                Value = mcs.Value,
                Frequency = mcs.Value,
                ValueType = mcs.ValueType,
                ValueUnit = mcs.ValueUnit,
            };
            tree.Add(mcsNode, root);
            tree.CutSets[mcsNode.GuidCode] = mcs;

            foreach (var evt in mcs.Events)
            {
                var occurrence = new FTAitem
                {
                    Name = evt.Name,
                    Tag = evt.Tag,
                    Description = evt.Description,
                    Reference = evt.Reference,
                    ItemType = evt.ItemType,
                    Gate = evt.Gate,
                    Value = evt.Value,
                    Frequency = evt.Frequency,
                    ValueType = evt.ValueType,
                    ValueUnit = evt.ValueUnit,
                    BIM = evt.BIM,
                    CIM = evt.CIM,
                    RAW = evt.RAW,
                    RRW = evt.RRW,
                    FV = evt.FV,
                };
                tree.Add(occurrence, mcsNode);
                tree.Originals[occurrence.GuidCode] = evt;
            }
        }

        TidyTreeLayout.Apply(tree.Items, root);
        return tree;
    }

    private void Add(FTAitem item, FTAitem? parent)
    {
        item.Parent = parent?.GuidCode ?? Guid.Empty;
        item.level = parent == null ? 0 : parent.level + 1;
        Items[item.GuidCode] = item;
        parent?.Children.Add(item.GuidCode);
    }
}
