using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


public class FTAlogic
{
    public Dictionary<Guid, FTAitem> FTAStructure;
    public Guid TopEventGuid;
    public Dictionary<int, String> GatesList;
    public Dictionary<int, String> EventsList;
    public Dictionary<Guid, FTAitem> MCSStructure { get; private set; }
    public List<FTAitem> SelectedEvents { get; set; } = new List<FTAitem>();
    public List<FTAitem> CopiedEvents { get; set; } = new List<FTAitem>();
    public bool IsHidden { get; set; } = false;

    public List<FTAitem> ConvertStructureToList()
    {
        return FTAStructure.Values.ToList();
    }


    public FTAlogic()
    {
        FTAStructure = new Dictionary<Guid, FTAitem>();
        MCSStructure = new Dictionary<Guid, FTAitem>();

        //Create Top Event
        FTAitem FI = new FTAitem();
        FI.Name = "Top Event ";
        FI.ItemType = 0;
        FI.GateType = 1;
        FI.X = 200;
        FI.Y = 100;
        TopEventGuid = FI.GuidCode;
        AddItem(FI);

        GatesList = new Dictionary<int, string>();
        EventsList = new Dictionary<int, string>();

        GenerateGatesList();
        GenerateEvetsList();
    }


    public void CopySelectedEvents()
    {
         CopiedEvents.Clear();

        if ( SelectedEvents.Count == 0)
        {
            MessageBox.Show("No events selected to copy.", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var evt in  SelectedEvents.OrderBy(item => item.level))
        {
             CopiedEvents.Add(evt);
        }

        MessageBox.Show("Selected events have been copied.", "Copy");
        
    }

    public void PasteCopiedEvents()
    {
        if (CopiedEvents.Count == 0)
        {
            MessageBox.Show("No events have been copied.", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedEvents.Count != 1)
        {
            MessageBox.Show("Please select exactly one target event to paste into.", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        FTAitem targetEvent = SelectedEvents[0];

        if (targetEvent.ItemType == 2)
        {
            MessageBox.Show("Cannot paste into a basic event.", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
       // SaveStateForUndo();
        List<Guid> copiedIDs = CopiedEvents.Select(evt => evt.GuidCode).ToList();
        Dictionary<Guid, Guid> cloneMapping = new Dictionary<Guid, Guid>();
        List<FTAitem> clonedEvents = new List<FTAitem>();

        foreach (FTAitem original in CopiedEvents)
        {
            Guid newParent = copiedIDs.Contains(original.Parent) ? original.Parent : targetEvent.GuidCode;
            Guid newGuid = AddNewEvent(newParent, original.Name, original.ItemType, original.GateType, original.Frequency, original.X, original.Y, original.Tag, original.level);
            cloneMapping[original.GuidCode] = newGuid;
            clonedEvents.Add(GetItem(newGuid));
            if (newParent == targetEvent.GuidCode)
            {
                if (!targetEvent.Children.Contains(newGuid))
                    targetEvent.Children.Add(newGuid);
            }
        }

        foreach (FTAitem original in CopiedEvents)
        {
            if (copiedIDs.Contains(original.Parent))
            {
                Guid newParentGuid = cloneMapping[original.Parent];
                Guid newEventGuid = cloneMapping[original.GuidCode];
                FTAitem newEvent = GetItem(newEventGuid);
                newEvent.Parent = newParentGuid;
                FTAitem newParentEvent = GetItem(newParentGuid);
                if (!newParentEvent.Children.Contains(newEventGuid))
                {
                    newParentEvent.Children.Add(newEventGuid);
                }
            }
        }

        MessageBox.Show("Paste operation completed.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);

        FindAllChilren();
   
        
        AssignLevelsToAllEvents();

        
    }
    public  void DeleteSelectedEvents()
    {
        if (SelectedEvents.Count == 0)
        {
            MessageBox.Show("No events selected to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedEvents.Any(evt => evt.GuidCode == TopEventGuid))
        {
            MessageBox.Show("Cannot delete the top event. You may edit it instead.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult result = MessageBox.Show("Are you sure you want to delete the selected events?",
                                                "Confirm Deletion",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {

            return;
        }
       
        foreach (var evt in SelectedEvents)
        {
            RemoveItem(evt.GuidCode);
        }
        SelectedEvents.Clear();

        AssignLevelsToAllEvents();
        
    }

    private void GenerateGatesList()
    {
        //https://relyence.com/2019/12/04/fault-tree-gates-events-explained/
        GatesList.Add(-1, "Not set");
        GatesList.Add(1, "OR");
        GatesList.Add(2, "AND");
        /*GatesList.Add(3, "NOT");
        GatesList.Add(4, "NAND");
        GatesList.Add(5, "NOR");
        GatesList.Add(6, "XOR");
        GatesList.Add(7, "Inhibit");
        GatesList.Add(8, "Priority AND");
        GatesList.Add(9, "Voting");*/
    }

    private void GenerateEvetsList()
    {
        EventsList.Add(-1, "Not set");
        EventsList.Add(1, "Intermediate");
        EventsList.Add(2, "Basic");
        /* EventsList.Add(3, "House");
         EventsList.Add(4, "Undeveloped");
         EventsList.Add(5, "Conditioning");*/
    }


    public void FindAllChilren()
    {
        foreach (var Event in FTAStructure)
        {
            Event.Value.Children.Clear();
        }
        foreach (var Event in FTAStructure)
        {
            Guid Parent = Event.Value.Parent;
            FTAitem fi = GetItem(Event.Value.Parent);
            if (fi != null)
                fi.Children.Add(Event.Value.GuidCode);
        }
    }

    //Methode for assigning levels to all events. Top event level=0.
    public void AssignLevelsToAllEvents()
    {
        var topEvents = FTAStructure.Values.Where(e => e.Parent == Guid.Empty || !FTAStructure.ContainsKey(e.Parent)).ToList();
        foreach (var top in topEvents)
        {
            AssignLevelsRecursive(top, 0);
        }
    }

    private void AssignLevelsRecursive(FTAitem item, int currentLevel)
    {
        item.level = currentLevel;
        var children = FTAStructure.Values.Where(e => e.Parent == item.GuidCode).ToList();
        foreach (var child in children)
        {
            AssignLevelsRecursive(child, currentLevel + 1);
        }
    }

    //Methode for adding new event to structure
    public Guid AddNewEvent(Guid Parent, string Name, int ItemType, int GateType, double Freq, int x = -1, int y = -1, string Tag = "", int level = 0)
    {
        FTAitem FI = new FTAitem();
        FI.Name = Name;
        FI.ItemType = ItemType;
        FI.GateType = GateType;
        FI.Parent = Parent;
        FI.Frequency = Freq;
        FI.Tag = Tag;
        FI.level = level;

        var ParentEvent = GetItem(Parent);

        if (x != -1 && y != -1)
        {
            FI.X = x;
            FI.Y = y;
        }
        else
        {

            if (ParentEvent.Children.Count == 0)
            {
                FI.X = ParentEvent.X;
                FI.Y = ParentEvent.Y + 150;
            }
            else
            {
                FI.X = GetItem(ParentEvent.Children[ParentEvent.Children.Count - 1]).X + Constants.EventWidth;
                FI.Y = ParentEvent.Y+150;
            }
        }
        AddItem(FI);
        return (FI.GuidCode);
    }

    // Methode for adding item to structure
    public void AddItem(FTAitem item)
    {
        if (!FTAStructure.ContainsKey(item.GuidCode))
        {
            FTAStructure[item.GuidCode] = item;
        }
    }

    // Methode for obtaining item from structure based od guid
    public FTAitem GetItem(Guid guid)
    {
        return FTAStructure.TryGetValue(guid, out FTAitem item) ? item : null;
    }

    // Methode for obtaining item from structure based od guid string
    public FTAitem GetItem(String guidString)
    {

        Guid guid;
        bool isValid = Guid.TryParse(guidString, out guid);
        if (isValid)
            return FTAStructure.TryGetValue(guid, out FTAitem item) ? item : null;
        else
            return (null);
    }

    public List<FTAitem> GetItems(String Tag)
    {
        List<FTAitem> l = new List<FTAitem>();

        foreach (FTAitem item in FTAStructure.Values)
        {
            if (item.Tag != null)
                if (item.Tag.Equals(Tag))
                    l.Add(item);
        }
        return (l);

    }
    // Methode for removing event with its children
    public void RemoveItem(Guid guid)
    {
        if (FTAStructure.ContainsKey(guid))
        {
            // Remove child
            RemoveChildren(guid);
            // Remove event
            FTAStructure.Remove(guid);
        }
    }

    private void RemoveChildren(Guid parentGuid)
    {
        for (int i = 0; i < FTAStructure[parentGuid].Children.Count; i++)
        {

            RemoveItem(FTAStructure[parentGuid].Children[i]);
        }
    }

    //Methode for computing freqvencies 
    public void ComputeTree()
    {
        foreach (var item in FTAStructure.Values)
        {
            if (item.ItemType < 2)
                item.Frequency = -1;
        }
        FTAitem topEvent = GetItem(TopEventGuid);
        if (topEvent == null)
            return;
        int maxIterations = 10000;
        int iteration = 0;
        bool updated = false;

        do
        {
            iteration++;
            updated = false;

            foreach (var evt in FTAStructure.Values)
            {
                if (evt.ItemType >= 2)
                    continue;

                if (evt.Children.Count == 0)
                    continue;
                bool allChildrenComputed = true;
                foreach (var childGuid in evt.Children)
                {
                    FTAitem child = GetItem(childGuid);
                    if (child == null || child.Frequency < 0)
                    {
                        allChildrenComputed = false;
                        break;
                    }
                }
                if (allChildrenComputed)
                {
                    double newFreq = 0;
                    int gate = evt.GateType;
                    for (int i = 0; i < evt.Children.Count; i++)
                    {
                        double childFreq = GetItem(evt.Children[i]).Frequency;
                        if (i == 0)
                            newFreq = childFreq;
                        else
                        {
                            // GateType == 1: OR gate 
                            // GateType == 2: AND gate
                            if (gate == 1)
                                newFreq += childFreq;
                            else if (gate == 2)
                                newFreq *= childFreq;
                        }
                    }
                    if (evt.Frequency != newFreq)
                    {
                        evt.Frequency = newFreq;
                        updated = true;
                    }
                }
            }
        }
        while (topEvent.Frequency < 0 && updated && iteration < maxIterations);
    }

    public void ComputeBasicEventFrequencyBounds(double deviationPercent)
    {
        foreach (FTAitem evt in FTAStructure.Values)
        {
            if (evt.ItemType == 2)
            {
                if (double.IsNaN(evt.LowerBoundFrequency) || double.IsNaN(evt.UpperBoundFrequency))
                {
                    evt.LowerBoundFrequency = evt.Frequency * (1 - deviationPercent / 100.0);
                    evt.UpperBoundFrequency = evt.Frequency * (1 + deviationPercent / 100.0);
                }
            }
        }
    }

    // Computes the Birnbaum Importance Measure (BIM) 
    public double ComputeBIM(FTAitem ftaEvent)
    {

        double originalFrequency = ftaEvent.Frequency;

        // Event failed  
        ftaEvent.Frequency = 1;
        ComputeTree();
        double topFreqWhenFailed = GetItem(TopEventGuid).Frequency;
        if (topFreqWhenFailed > 1)
        {
            topFreqWhenFailed = 1;
        }

        // Event didnt failed
        ftaEvent.Frequency = 0;
        ComputeTree();
        double topFreqWhenWorks = GetItem(TopEventGuid).Frequency;


        ftaEvent.Frequency = originalFrequency;

        // Calculate BIM
        double bim = topFreqWhenFailed - topFreqWhenWorks;
        return bim;
    }
    // Computes the Critical Importance Measure (CIM)
    public double ComputeCIM(FTAitem ftaEvent)
    {
        double pEventNormal = ftaEvent.Frequency;

        // Event failed 
        ftaEvent.Frequency = 0;
        ComputeTree();
        double pTopWhenWorks = GetItem(TopEventGuid).Frequency;

        // Event didnt failed
        ftaEvent.Frequency = 1;
        ComputeTree();
        double pTopWhenFails = GetItem(TopEventGuid).Frequency;

        // Event works normaly
        ftaEvent.Frequency = pEventNormal;
        ComputeTree();
        double pTopNormal = GetItem(TopEventGuid).Frequency;

        // CIM = P(top event when event works) - P(top event when event fails) * (pEventNormal / pTopNormal)
        double cim = (pTopWhenFails - pTopWhenWorks) * (pEventNormal / pTopNormal);

        ftaEvent.Frequency = pEventNormal;

        return cim;
    }


    public double ComputeRAW(FTAitem ftaEvent)
    {
        double originalFrequency = ftaEvent.Frequency;

        // Event failed
        ftaEvent.Frequency = 1;
        ComputeTree();
        double topProbWhenFails = GetItem(TopEventGuid).Frequency;

        // Event works normaly
        ftaEvent.Frequency = originalFrequency;
        ComputeTree();
        double topProbNormal = GetItem(TopEventGuid).Frequency;

        if (topProbNormal == 0)
            return double.PositiveInfinity;

        double raw = topProbWhenFails / topProbNormal;
        return raw;
    }


    public double ComputeRRW(FTAitem ftaEvent)
    {
        double originalFrequency = ftaEvent.Frequency;

        // Event failed
        ftaEvent.Frequency = originalFrequency;
        ComputeTree();
        double topProbNormal = GetItem(TopEventGuid).Frequency;

        // Event didnt failed
        ftaEvent.Frequency = 0;
        ComputeTree();
        double topProbWhenWorks = GetItem(TopEventGuid).Frequency;

        ftaEvent.Frequency = originalFrequency;

        if (topProbWhenWorks == 0)
            return double.PositiveInfinity;

        double rrw = topProbNormal / topProbWhenWorks;
        return rrw;
    }

    public double ComputeFV(FTAitem ftaEvent)
    {
        double originalFrequency = ftaEvent.Frequency;

        // Event works normaly
        ftaEvent.Frequency = originalFrequency;
        ComputeTree();
        double topProbNormal = GetItem(TopEventGuid).Frequency;

        // Event didnt failed
        ftaEvent.Frequency = 0;
        ComputeTree();
        double topProbWhenWorks = GetItem(TopEventGuid).Frequency;

        ftaEvent.Frequency = originalFrequency;

        if (topProbNormal == 0)
            return double.PositiveInfinity;

        double fv = (topProbNormal - topProbWhenWorks) / topProbNormal;
        return fv;
    }


    public void SumChildren(FTAitem parent)

    {

        for (int i = 0; i < parent.Children.Count; i++)
        {
            SumChildren(GetItem(parent.Children[i]));

            int gate = parent.GateType;
            double f = GetItem(parent.Children[i]).Frequency;

            if (i == 0)
                parent.Frequency = f;
            else
            {
                ///AND
                if (gate == 1)
                    parent.Frequency = parent.Frequency + f;
                //OR
                if (gate == 2)
                    parent.Frequency = parent.Frequency * f;
            }
        }
    }

    //Generate equation for MCS
    public string GenerateEquation()
    {
        //Call Top event
        var topEvent = GetItem(TopEventGuid);
        //Generate equation of the tree
        return GenerateEquationForEvent(topEvent);
    }

    //Generate equation for MCS
    private string GenerateEquationForEvent(FTAitem currentEvent)
    {
        //If event have no child (basic events)  return their TAG value
        if (currentEvent.Children.Count == 0)
        {
            return currentEvent.Tag;
        }

        //If event have child, create list childEquations
        List<string> childEquations = new List<string>();

        // If event have child. return next children and call GenerateEquationForEvent methode again until basic event is found.
        foreach (var childGuid in currentEvent.Children)
        {
            var childEvent = GetItem(childGuid);
            childEquations.Add(GenerateEquationForEvent(childEvent));
        }
        // We check the gate type for the current event to decide how to combine child equations.
        if (currentEvent.GateType == 1) // OR gate
        {
            return "(" + string.Join(" + ", childEquations) + ")";
        }
        else if (currentEvent.GateType == 2) // AND gate
        {
            return "(" + string.Join(" * ", childEquations) + ")";
        }

        return "";
    }

    public void GenerateMCS()
    {
        // Set default frequency for basic events if not set.
        foreach (var item in FTAStructure.Values)
        {
            if (item.ItemType == 2 && item.Frequency == 0)
            {
                item.Frequency = 0.001;
            }
        }

        MCSStructure.Clear();

        // 1) Generate the MCS equation from the fault tree.
        string equation = GenerateEquation();



        // 2) Simplify the equation using BooleanAlgebra.
        string simplifiedEquation = MCSEngine_Exp.ProcessExpression(equation);

        string message = "Generated Tree Expression:\n" + equation +
                         "\n\nSimplified Expression:\n" + simplifiedEquation;
        MessageBox.Show(message, "MCS Equation Overview", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // 3) Create the top event for MCS with an OR gate.
        FTAitem topEvent = GetItem(TopEventGuid);
        FTAitem mcsTopEvent = new FTAitem
        {
            Name = topEvent.Name,
            ItemType = topEvent.ItemType,
            GateType = 1, // OR gate for the top event.
            X = 200,
            Y = 100,
            Frequency = topEvent.Frequency
        };
        MCSStructure[mcsTopEvent.GuidCode] = mcsTopEvent;

        // 4) Attach MCS terms directly to the top event .
        ParseMCSIntoStructure(simplifiedEquation, mcsTopEvent.GuidCode);
    }

    private void ParseMCSIntoStructure(string equation, Guid parentGuid)
    {
        // Split the equation by the OR operator '+'
        string[] terms = equation.Split('+');
        int termNumber = 1; // Start numbering intermediate(MCS) events from 1

        foreach (string term in terms)
        {
            string trimmedTerm = term.Trim();
            // Determine gate type for intermediate event:
            // If term contains '*', it is a multi-component term => use AND gate (GateType = 2)
            // Otherwise, for a single component, set GateType to -1 (Not set)
            int gateTypeForIntermediate = trimmedTerm.Contains("*") ? 2 : -1;

            // Create a new intermediate event with name "MCS" followed by termNumber.
            Guid intermediateGuid = AddIntermediateEvent("MCS" + termNumber, gateTypeForIntermediate, parentGuid);
            termNumber++;

            if (trimmedTerm.Contains("*"))
            {
                // Multi-component term: split the term by '*' to get individual factors.
                string[] factors = trimmedTerm.Split('*');
                foreach (string factor in factors)
                {
                    string tag = factor.Trim();
                    // Find the original event by its tag.
                    var originalEvent = FTAStructure.Values.FirstOrDefault(e => string.Equals(e.Tag, tag, StringComparison.OrdinalIgnoreCase));
                    if (originalEvent != null)
                    {
                        // Create a copy of the original event for the MCS structure.
                        FTAitem newItem = new FTAitem
                        {
                            Name = originalEvent.Name,
                            Tag = originalEvent.Tag,
                            ItemType = originalEvent.ItemType,
                            GateType = originalEvent.GateType,
                            Frequency = originalEvent.Frequency,
                            Parent = intermediateGuid,
                            X = originalEvent.X,
                            Y = originalEvent.Y
                        };
                        MCSStructure[newItem.GuidCode] = newItem;
                        // Attach the basic event as a child of the AND intermediate event.
                        MCSStructure[intermediateGuid].Children.Add(newItem.GuidCode);
                    }
                }
            }
            else
            {
                // Single-component term: attach the literal event directly.
                string tag = trimmedTerm;
                var originalEvent = FTAStructure.Values.FirstOrDefault(e => string.Equals(e.Tag, tag, StringComparison.OrdinalIgnoreCase));
                if (originalEvent != null)
                {
                    FTAitem newItem = new FTAitem
                    {
                        Name = originalEvent.Name,
                        Tag = originalEvent.Tag,
                        ItemType = originalEvent.ItemType,
                        GateType = originalEvent.GateType,
                        Frequency = originalEvent.Frequency,
                        Parent = intermediateGuid,
                        X = originalEvent.X,
                        Y = originalEvent.Y
                    };
                    MCSStructure[newItem.GuidCode] = newItem;
                    MCSStructure[intermediateGuid].Children.Add(newItem.GuidCode);
                }
            }
            // Attach the intermediate event directly to the top event.
            if (MCSStructure.ContainsKey(parentGuid))
            {
                MCSStructure[parentGuid].Children.Add(intermediateGuid);
            }
        }
    }

    private Guid AddIntermediateEvent(string name, int gateType, Guid parentGuid)
    {
        var intermediateEvent = new FTAitem
        {
            Name = name,
            ItemType = 1, // Intermediate event
            GateType = gateType, // AND or OR gate
            Parent = parentGuid,
            X = 0,
            Y = 0
        };

        MCSStructure[intermediateEvent.GuidCode] = intermediateEvent;
        return intermediateEvent.GuidCode;
    }

    public void DebugMCSStructure()
    {
        Debug.WriteLine("MCS Structure Content:");
        foreach (var item in MCSStructure.Values)
        {
            Debug.WriteLine("Node Details:");
            Debug.WriteLine($"  Name: {item.Name}");
            Debug.WriteLine($"  Tag: {item.Tag}");
            Debug.WriteLine($"  Parent: {item.Parent}");
            Debug.WriteLine($"  ItemType: {item.ItemType}");
            Debug.WriteLine($"  GateType: {item.GateType}");
            Debug.WriteLine($"  Frequency: {item.Frequency}");
            Debug.WriteLine($"  X: {item.X}, Y: {item.Y}");
            Debug.WriteLine("  Children:");
            foreach (var child in item.Children)
            {
                Debug.WriteLine($"    Child GUID: {child}");
            }
        }
    }
}

public class FTAitem
{

    public string Name;
    public string Description;
    public Guid GuidCode;
    public Guid Parent;
    public int ItemType; // ItemType - 1- basic Event, 2- intermediate Event
    public int GateType; // GateType - 1-AND 2-OR
    public string Tag;
    public double Frequency;
    public List<Guid> Children;
    public int level { get; set; }
    public double X1;
    public double Y1;
    public int X;
    public int Y;
    public bool ItemState;
    public bool IsHidden;
    public bool IsSelected;
    public double LowerBoundFrequency { get; set; }
    public double UpperBoundFrequency { get; set; }

    public FTAitem()
    {
        // Set uniqe guid 
        GuidCode = Guid.NewGuid();
        Children = new List<Guid>();
        {
            LowerBoundFrequency = double.NaN;
            UpperBoundFrequency = double.NaN;
        }
    }

    public void Method()
    {
        throw new System.NotImplementedException();
    }

    public void Method1()
    {
        throw new System.NotImplementedException();
    }

    public void Method2()
    {
        throw new System.NotImplementedException();
    }
}

public static class Constants
{
    public const int EventWidth = 140;
    public const int EventHeight = 90;
}





