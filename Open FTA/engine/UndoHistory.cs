/// <summary>
/// Undo and redo of changes of the fault tree. Every step keeps independent copies of all events,
/// so changes of event properties (name, value, gate, ...) can be undone, not only added or deleted events.
/// Call <see cref="Record(FTAlogic)"/> just before a change.
/// </summary>
public sealed class UndoHistory
{
    /// <summary>Copy of the whole tree at one moment.</summary>
    public sealed class State
    {
        internal State(List<FTAitem> items, Guid topEventGuid)
        {
            Items = items;
            TopEventGuid = topEventGuid;
        }

        internal List<FTAitem> Items { get; }
        internal Guid TopEventGuid { get; }
    }

    private readonly Stack<State> _undo = new();
    private readonly Stack<State> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Copy of the current tree; record it later with <see cref="Record(State)"/> if the tree changed.</summary>
    public static State Capture(FTAlogic logic) =>
        new(logic.FTAStructure.Values.Select(e => e.Clone()).ToList(), logic.TopEventGuid);

    /// <summary>Saves the current tree as an undo step and clears the redo steps.</summary>
    public void Record(FTAlogic logic) => Record(Capture(logic));

    /// <summary>Saves a tree captured before a change as an undo step and clears the redo steps.</summary>
    public void Record(State state)
    {
        _undo.Push(state);
        _redo.Clear();
    }

    public bool Undo(FTAlogic logic) => Move(logic, _undo, _redo);

    public bool Redo(FTAlogic logic) => Move(logic, _redo, _undo);

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    private static bool Move(FTAlogic logic, Stack<State> from, Stack<State> to)
    {
        if (from.Count == 0)
            return false;
        to.Push(Capture(logic));
        Restore(logic, from.Pop());
        return true;
    }

    private static void Restore(FTAlogic logic, State state)
    {
        var selected = logic.SelectedEvents.Select(e => e.GuidCode).ToHashSet();

        // The same dictionary is kept: the drawing engine refers to it.
        logic.FTAStructure.Clear();
        foreach (FTAitem item in state.Items)
        {
            FTAitem copy = item.Clone();   // the state itself stays unchanged
            logic.FTAStructure[copy.GuidCode] = copy;
        }
        logic.TopEventGuid = state.TopEventGuid;
        logic.FindAllChilren();
        logic.AssignLevelsToAllEvents();

        // Selection and highlighting referred to the replaced events.
        logic.SelectedEvents.Clear();
        foreach (FTAitem item in logic.FTAStructure.Values)
        {
            item.IsSelected = selected.Contains(item.GuidCode);
            if (item.IsSelected)
                logic.SelectedEvents.Add(item);
        }
        logic.HighlightedEvents.Clear();
        logic.MyDrawingEngine.TopEvent = logic.GetItem(logic.TopEventGuid);
    }
}
