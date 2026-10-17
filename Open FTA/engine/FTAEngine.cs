using Open_FTA.forms;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static Open_FTA.forms.ErrorDialog;

/// <summary>
/// TODO: Copy/Paste dialog cely strom alebo len 4as5
/// BUG: po vytvoreni reportu prestane fungovat oyna4ovanie MCS

/// </summary>

public class MessageItem
{
    public MessageType Type { get; set; }
    public string Text { get; set; }

    public MessageItem(MessageType type, string text)
    {
        Type = type;
        Text = text;
    }
}

public enum MainCompTimeUnit
{
    Year,
    Day,
    Hour,
    Second
}
public enum GateFrequencyHandling
{
    Allow,
    Disallow,
    AllowWithWarning
}

public enum ValueTypes { F, P, R, Lambda }
public enum Gates { NotSet, OR, AND }
public enum SortingStrategy { ALGOI, ALGOII_centered, ALGOII_align_left }
public enum DisplayMetricType { None, BIM, CIM, RAW, RRW, FV}

public enum FrequencyDisplayMode { topEventsOnly, allEvents, alwaysHidden }
public class FTAlogic
{
    public DrawingEngine MyDrawingEngine;

    public Dictionary<Guid, FTAitem> FTAStructure { get; set; } = new Dictionary<Guid, FTAitem>();

    public Dictionary<Guid, FTAitem> TempStructure { get; set; } = new Dictionary<Guid, FTAitem>();
    public Guid TopEventGuid;
    public Dictionary<int, String> GatesList { get; private set; } = new Dictionary<int, String>();
    public Dictionary<int, String> EventsList { get; private set; } = new Dictionary<int, String>();
    public Dictionary<int, String> MetricList { get; private set; } = new Dictionary<int, String>();
    //  public Dictionary<int, String> MetricUnitsList { get; private set; } = new Dictionary<int, String>();



    public Dictionary<int, string> MetricUnitsList = new()
    {
        { 0, "y⁻¹" },  // roky
        { 1, "d⁻¹" },  // dni
        { 2, "h⁻¹" },  // hodiny
        { 3, "s⁻¹" }   // sekundy
    };

    // Prevodné faktory z jednotky na 1/rok
    private static readonly Dictionary<int, double> TimeUnitFactors = new()
    {
        { 0, 1.0 },           // 1 rok = 1
        { 1, 365.0 },         // 1 deň = 1/365 roku
        { 2, 8760.0 },        // 1 hod = 1/8760 roku
        { 3, 31_536_000.0 }   // 1 sek = 1/31 536 000 roku
    };

  

    /// <summary>Minimal cut sets created by GenerateMCS, in the order of the simplified expression.</summary>
    public List<MinimalCutSet> MinimalCutSets { get; } = new List<MinimalCutSet>();

    /// <summary>Top event result evaluated from the minimal cut sets: 1 - prod(1 - P_MCS).</summary>
    public double McsTopValue { get; private set; }
    public ValueTypes McsTopValueType { get; private set; }
    public int McsTopValueUnit { get; private set; }
    public List<FTAitem> SelectedEvents { get; set; } = new List<FTAitem>();
    public List<FTAitem> HighlightedEvents { get; set; } = new List<FTAitem>();

    public String HighlightedMCS { get; set; }
    public List<FTAitem> CopiedEvents { get; set; } = new List<FTAitem>();
    public bool IsHidden { get; set; } = false;

    public StringBuilder html { get; set; } = new StringBuilder();

    public List<FTAitem> ConvertStructureToList()
    {
        return FTAStructure.Values.ToList();
    }


    public FTAlogic()
    {

        //Create Top Event
        CreateNewTopEvent();

        GenerateGatesList();
        GenerateEvetsList();
        GenerateMetrics();
        GenereteMetricUnitsList();

        MyDrawingEngine = new DrawingEngine(this, FTAStructure);
    }

    public bool HasEventHiddenChildren(FTAitem evn)
    {
        if(evn == null) return false;
        if(evn.IsHidden) return false;
        if(evn.Children.Count >0) 
            if(GetItem(evn.Children[0]).IsHidden) return true;

        return false;
    }

    public void CreateNewTopEvent()
    {
        FTAitem FI = new FTAitem();
        FI.Name = "Top Event";
        FI.Tag = "TE";
        FI.ItemType = 1;
        FI.Gate = Gates.OR;
        FI.X = 200;
        FI.Y = 100;
        TopEventGuid = FI.GuidCode;
        AddItem(FI);
    }

    public void CopySelectedEventsOLD()
    {
        CopiedEvents.Clear();

        if (SelectedEvents.Count == 0)
        {
            MessageBox.Show("No events selected to copy.", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var evt in SelectedEvents.OrderBy(item => item.level))
        {
            CopiedEvents.Add(evt);
        }

        string json = JsonSerializer.Serialize(CopiedEvents, new JsonSerializerOptions
        {
            WriteIndented = true // pekné odsadenie pre ľudí
        });

        // uloženie do clipboardu
        Clipboard.SetText(json);

        MessageBox.Show("Selected events have been copied.", "Copy");

    }

    public void PasteCopiedEventsOLD()
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
            Guid newGuid = AddNewEvent(newParent, original.Name, original.ItemType, original.Gate, original.Frequency, original.X, original.Y, original.Tag, original.level);
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

    public void CopySelectedEvents()
    {
        try
        {

            foreach (var evt in SelectedEvents.OrderBy(item => item.level))
            {
                CopiedEvents.Add(evt);
            }
            string json = JsonSerializer.Serialize(CopiedEvents, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Clipboard.SetText(json);
            CopiedEvents.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error during copy operation: " + ex.Message, "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void PasteCopiedEvents()
    {
        try
        {

            if (SelectedEvents.Count != 1)
            {
                MessageBox.Show("Please select exactly one target event to paste into.", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FTAitem targetEvent = SelectedEvents[0];

            if (targetEvent.ItemType >= 2)
            {
                MessageBox.Show("Cannot paste into a basic event.", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            string jsonFromClipboard = Clipboard.GetText();
            List<FTAitem> CopiedEvents = JsonSerializer.Deserialize<List<FTAitem>>(jsonFromClipboard);

            List<Guid> copiedIDs = CopiedEvents.Select(evt => evt.GuidCode).ToList();
            Dictionary<Guid, Guid> cloneMapping = new Dictionary<Guid, Guid>();
            List<FTAitem> clonedEvents = new List<FTAitem>();

            foreach (FTAitem original in CopiedEvents)
            {
                FTAitem newItem = original.DeepCopyFrom(original);
                if (newItem.ItemType >= 1)
                    newItem.Tag = GetNextAvailableTag(newItem.ItemType);
                FTAStructure.Add(newItem.GuidCode, newItem);
                //   cloneMapping[original.GuidCode] =newItem.GuidCode;
                cloneMapping.Add(original.GuidCode, newItem.GuidCode);
                clonedEvents.Add(newItem);
            }


            SelectedEvents.Clear();
            foreach (FTAitem pasted in clonedEvents)
            {
                if (cloneMapping.ContainsKey(pasted.Parent))
                {
                    Guid newNguid = cloneMapping[pasted.Parent];
                    pasted.Parent = newNguid;
                }
                else
                {
                    pasted.Parent = targetEvent.GuidCode;

                }
                SelectedEvents.Add(pasted);

            }






            //MessageBox.Show("Paste operation completed.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);

            FindAllChilren();
            AssignLevelsToAllEvents();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error during paste operation: " + ex.Message, "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


    }


    public void DeleteSelectedEvents()
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
        // GatesList.Add(-1, "Not set");
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
        //    EventsList.Add(-1, "Not set");
        EventsList.Add(1, "Intermediate");
        EventsList.Add(2, "Basic");
        EventsList.Add(3, "House");
        EventsList.Add(4, "Undeveloped");
        //EventsList.Add(5, "Conditioning");
    }
    private void GenerateMetrics()
    {

        MetricList.Add(0, "Frequency(f)");
        MetricList.Add(1, "Probability(P)");
        MetricList.Add(2, "Reliability(R)");
        MetricList.Add(3, "Failure rate(λ)");
    }

    private void GenereteMetricUnitsList()
    {
        /* MetricUnitsList.Add(0,"y⁻¹");
         MetricUnitsList.Add(1,"d⁻¹");
         MetricUnitsList.Add(2,"h⁻¹");
         MetricUnitsList.Add(3,"s⁻¹");*/

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

    public void FindAllChilren(Dictionary<Guid, FTAitem> s)
    {
        foreach (var Event in s)
        {
            Event.Value.Children.Clear();
        }
        foreach (var Event in s)
        {
            Guid Parent = Event.Value.Parent;
            FTAitem fi = GetItem(Event.Value.Parent, s);
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
    public Guid AddNewEvent(Guid Parent, string Name, int ItemType, Gates Gate, double Freq, int x = -1, int y = -1, string Tag = "", int level = 0)
    {
        FTAitem FI = new FTAitem();
        FI.Name = Name;
        FI.ItemType = ItemType;
        FI.Gate = Gate;
        FI.Parent = Parent;
        FI.Frequency = Freq;
        //FI.Tag = Tag;
        if (Tag.Length < 2)
            FI.Tag = GetNextAvailableTag(ItemType);
        if (Name.Length < 2)
            FI.Name = "New - " + FI.Tag;
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
                FI.Y = ParentEvent.Y + 150;
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

    public FTAitem GetItem(Guid guid, Dictionary<Guid, FTAitem> str)
    {
        return str.TryGetValue(guid, out FTAitem item) ? item : null;
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

    public FTAitem GetItem(String guidString, Dictionary<Guid, FTAitem> str)
    {

        Guid guid;
        bool isValid = Guid.TryParse(guidString, out guid);
        if (isValid)
            return str.TryGetValue(guid, out FTAitem item) ? item : null;
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

    public void ComputeTree()
    {
        /*
       foreach (FTAitem item in FTAStructure.Values)
      {
          if (item.ItemType > 1)
                UserUnitsToFreq(item);

           if (item.ItemType < 2)
           {
               item.Frequency = -1;
               item.Value = -1;
               item.UserMetricType = -1;
               item.ValueUnit = -1;
           }
      }*/



        //ComputeTreeSimple();
        SumChildren(GetItem(TopEventGuid));


    }

    public void SumChildren(FTAitem parent, Dictionary<Guid, FTAitem> str)
    {
        bool AllProbabilities = true;
        var events = new List<FTAitem>();

        for (int i = 0; i < parent.Children.Count; i++)
        {
            FTAitem item = GetItem(parent.Children[i], str);

            SumChildren(item, str);


            if (i == 0)
            {
                events.Clear();
            }

            // P and R are probability-type values; F and lambda make the result a frequency
            if (item.ValueType != ValueTypes.P && item.ValueType != ValueTypes.R && AllProbabilities)
            {
                AllProbabilities = false;
            }

            events.Add(GetItem(parent.Children[i], str));

            if (i == parent.Children.Count - 1)
            {
                double SumResult = 0.0;

                SumResult = CalculateGate(events, parent.Gate);

                if (AllProbabilities)
                {
                    parent.Frequency = SumResult;
                    parent.ValueType = ValueTypes.P;
                    parent.Value = SumResult;
                    parent.ValueUnit = (int)MainAppSettings.Instance.BaseTimeUnit;
                }
                else
                {
                    parent.Frequency = ProbabilityToFrequency(SumResult);
                    parent.ValueType = ValueTypes.F;
                    parent.Value = ProbabilityToFrequency(SumResult);
                    parent.ValueUnit = (int)MainAppSettings.Instance.BaseTimeUnit;
                }
            }
        }
    }
    public void SumChildren(FTAitem parent)
    {
        SumChildren(parent, FTAStructure);
    }

    public static double CalculateGate(List<FTAitem> events, Gates gate) =>
        CalculateGate(events.Select(ConvertToProbability), gate);

    /// <summary>Combines probabilities of independent inputs with an OR / AND gate.</summary>
    public static double CalculateGate(IEnumerable<double> probabilities, Gates gate)
    {
        var probs = probabilities.ToList();

        if (gate == Gates.OR && MainAppSettings.Instance.SimplificationStrategyLinearOR)
        {
            // lineárna aproximácia OR brány
            return probs.Sum();
        }

        return gate switch
        {
            Gates.OR => probs.Aggregate((a, b) => a + b - a * b),
            Gates.AND => probs.Aggregate((a, b) => a * b),
            _ => throw new ArgumentException("Neznámy typ brány")
        };
    }

    /// <summary>
    /// Converts an event value to the probability of occurrence within one base time period
    /// (MainAppSettings.BaseTimeUnit).
    /// F and Lambda: exposure time t = one base period expressed in the event's time unit,
    /// t = Tbase / Tsource (e.g. event in 1/h, base year -> t = 8760 h).
    /// Simplified: P = f*t, exact: P = 1 - exp(-f*t). P and R do not depend on time units.
    /// </summary>
    private static double ConvertToProbability(FTAitem e)
    {
        // Importance measures temporarily force the event to occur (1) or not occur (0)
        if (e.ForcedProbability is double forced)
            return forced;

        return ConvertToProbability(e.Value, e.ValueType, e.ValueUnit);
    }

    private static double ConvertToProbability(double value, ValueTypes valueType, int valueUnit)
    {
        // Length of one unit in years: year = 1, day = 1/365, hour = 1/8760, second = 1/31 536 000
        double Tsource = 1.0 / TimeUnitFactors[valueUnit];
        double Tbase = 1.0 / TimeUnitFactors[(int)MainAppSettings.Instance.BaseTimeUnit];
        double t = Tbase / Tsource;

        return valueType switch
        {
            ValueTypes.F or ValueTypes.Lambda => MainAppSettings.Instance.SimplificationStrategy
                ? value * t
                : 1 - Math.Exp(-value * t),
            ValueTypes.P => value,
            ValueTypes.R => 1 - value,
            _ => throw new ArgumentException("Neznámy typ hodnoty")
        };
    }

    public static double ProbabilityToFrequency(double P)
    {

        double Tbase = 1.0 / TimeUnitFactors[(int)MainAppSettings.Instance.BaseTimeUnit];
        double res = 0;
        if (!MainAppSettings.Instance.SimplificationStrategy)
        {
            // res = -Math.Log(1 - P) / Tbase;
            res = -Math.Log(1 - P);
        }
        else
        {
            //res = P * Tbase / Tsource; 
            res = P;
        }

        return (res);
    }

    /*
    public void SumChildrenOld(FTAitem parent)
    {
        bool AllProbabilities = true;

        for (int i = 0; i < parent.Children.Count; i++)
        {
            SumChildren(GetItem(parent.Children[i]));

            FTAitem item = GetItem(parent.Children[i]);
            double f = item.Frequency;

            if(item.UserMetricType!=1&&(AllProbabilities))
            {
                AllProbabilities = false;
            }

            double P = ProbabilityFromFrequency(f);

            int gate = parent.Gate;
            if (i == 0)
            {
                if (gate == 2)
                    parent.Frequency = P;
                if (gate == 1)
                    // parent.Frequency = f;
                    parent.Frequency = 1 - P;
            }

            else
            {
                ///AND
                if (gate == 2)
                    parent.Frequency = parent.Frequency * P;
                //OR
                if (gate == 1)
                    //parent.Frequency = parent.Frequency * f;
                    parent.Frequency = parent.Frequency * (1 - P);

            }
            if (gate == 1 && i == parent.Children.Count-1)
                parent.Frequency = 1 - parent.Frequency;

            if (i == parent.Children.Count - 1)
            {
                parent.Frequency = FrequencyFromProbability(parent.Frequency);
               
                parent.UserMetricType = 0;
                parent.Value = parent.Frequency;
                if (AllProbabilities)
                {
                    parent.UserMetricType = 1;
                    parent.Value = ProbabilityFromFrequency(parent.Frequency);

                } 

                
               
             }

        }      

    }
    */
    /*
    public void ComputeTreeSimple()
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
                    int gate = evt.Gate;
                    bool AllProbabilities = true;
                    ///to test if all children are probabilities
                    for (int i = 0; i < evt.Children.Count; i++)
                    {
                        if (GetItem(evt.Children[i]).UserMetricType != 1)
                        { 
                            AllProbabilities = false;
                            break;
                        }
                    }

                    for (int i = 0; i < evt.Children.Count; i++)
                    {
                        double childFreq = GetItem(evt.Children[i]).Frequency;
                        if (i == 0)
                            newFreq = (childFreq);
                        else
                        {
                            // Gates == 1: OR gate 
                            // Gates == 2: AND gate
                            if (gate == 1)
                                newFreq += (childFreq);
                            else if (gate == 2)
                                newFreq *= (childFreq);
                        }
                    }
                    if(AllProbabilities)
                    {
                        evt.UserMetricType = 1;
                    }

                    if (evt.Frequency != newFreq)
                    {
                        evt.Frequency = (newFreq);
                        updated = true;
                    }
                }
            }
        }
        while (topEvent.Frequency < 0 && updated && iteration < maxIterations);
    }
    */
    public void UserUnitsToFreq(FTAitem evt)
    {
        if (evt.ValueType == ValueTypes.F)
        {
            evt.Frequency = evt.Value * TimeUnitFactors[evt.ValueUnit];
        }


        if (evt.ValueType == ValueTypes.P)
        {
            evt.Frequency = FrequencyFromProbability(evt.Value);
        }
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

    // ---------------------------------------------------------------------------------
    // Importance measures
    //
    // The measures use the top event result in its own quantity: frequency (per base time unit)
    // if the top event is a frequency, otherwise probability. No capping.
    // The examined basic event - all its instances with the same Tag (repeated event) -
    // is forced to P = 1 (event occurs) or P = 0 (event does not occur) through
    // FTAitem.ForcedProbability; the event's Value is never modified.
    // The tree is recomputed in its normal state at the end.
    //
    // Frequency events (F, lambda - initiating events) cannot "occur with certainty", so
    // BIM, CIM and RAW are not defined for them and return NaN (shown as n/a).
    // RRW and FV (event removed, P = 0) are defined for all events.
    // ---------------------------------------------------------------------------------

    private readonly record struct ImportanceInputs(double Top, double TopWhenFails, double TopWhenWorks, double PEvent);

    private static bool IsFrequencyEvent(FTAitem e) =>
        e.ValueType == ValueTypes.F || e.ValueType == ValueTypes.Lambda;

    /// <summary>All basic events that represent the same event (same Tag, case-insensitive).</summary>
    private List<FTAitem> GetEventInstances(FTAitem ftaEvent)
    {
        var instances = string.IsNullOrWhiteSpace(ftaEvent.Tag)
            ? new List<FTAitem>()
            : FTAStructure.Values
                .Where(i => i.ItemType >= 2 && string.Equals(i.Tag, ftaEvent.Tag, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (!instances.Contains(ftaEvent))
            instances.Add(ftaEvent);
        return instances;
    }

    /// <summary>Top event result (frequency or probability) with the event forced to the given probability (null = normal state).</summary>
    private double TopResultWith(List<FTAitem> instances, double? forcedProbability)
    {
        foreach (var item in instances)
            item.ForcedProbability = forcedProbability;
        try
        {
            ComputeTree();
            return GetItem(TopEventGuid).Value;
        }
        finally
        {
            foreach (var item in instances)
                item.ForcedProbability = null;
        }
    }

    private ImportanceInputs EvaluateImportanceInputs(FTAitem ftaEvent)
    {
        var instances = GetEventInstances(ftaEvent);

        double whenFails = TopResultWith(instances, 1.0);
        double whenWorks = TopResultWith(instances, 0.0);
        double top = TopResultWith(instances, null);   // last: leaves the tree in its normal state
        double pEvent = ConvertToProbability(ftaEvent);

        return new ImportanceInputs(top, whenFails, whenWorks, pEvent);
    }

    private void StoreMeasure(FTAitem ftaEvent, Action<FTAitem> store)
    {
        foreach (var item in GetEventInstances(ftaEvent))
            store(item);
    }

    /// <summary>Birnbaum importance: BIM = Top(event occurs) - Top(event does not occur). NaN for frequency events.</summary>
    public double ComputeBIM(FTAitem ftaEvent)
    {
        var x = EvaluateImportanceInputs(ftaEvent);
        double bim = IsFrequencyEvent(ftaEvent) ? double.NaN : x.TopWhenFails - x.TopWhenWorks;
        StoreMeasure(ftaEvent, i => i.BIM = bim);
        return bim;
    }

    /// <summary>Criticality importance: CIM = BIM * P(event) / Top. NaN for frequency events.</summary>
    public double ComputeCIM(FTAitem ftaEvent)
    {
        var x = EvaluateImportanceInputs(ftaEvent);
        double cim = IsFrequencyEvent(ftaEvent) ? double.NaN
                   : x.Top == 0 ? 0
                   : (x.TopWhenFails - x.TopWhenWorks) * x.PEvent / x.Top;
        StoreMeasure(ftaEvent, i => i.CIM = cim);
        return cim;
    }

    /// <summary>Risk achievement worth: RAW = Top(event occurs) / Top. NaN for frequency events.</summary>
    public double ComputeRAW(FTAitem ftaEvent)
    {
        var x = EvaluateImportanceInputs(ftaEvent);
        double raw = IsFrequencyEvent(ftaEvent) ? double.NaN
                   : x.Top == 0 ? double.PositiveInfinity
                   : x.TopWhenFails / x.Top;
        StoreMeasure(ftaEvent, i => i.RAW = raw);
        return raw;
    }

    /// <summary>Risk reduction worth: RRW = Top / Top(event does not occur).</summary>
    public double ComputeRRW(FTAitem ftaEvent)
    {
        var x = EvaluateImportanceInputs(ftaEvent);
        double rrw = x.TopWhenWorks == 0 ? double.PositiveInfinity : x.Top / x.TopWhenWorks;
        StoreMeasure(ftaEvent, i => i.RRW = rrw);
        return rrw;
    }

    /// <summary>Fussell-Vesely: FV = (Top - Top(event does not occur)) / Top.</summary>
    public double ComputeFV(FTAitem ftaEvent)
    {
        var x = EvaluateImportanceInputs(ftaEvent);
        double fv = x.Top == 0 ? 0 : (x.Top - x.TopWhenWorks) / x.Top;
        StoreMeasure(ftaEvent, i => i.FV = fv);
        return fv;
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
        if (currentEvent.Gate == Gates.OR) // OR gate
        {
            return "(" + string.Join(" + ", childEquations) + ")";
        }
        else if (currentEvent.Gate == Gates.AND) // AND gate
        {
            return "(" + string.Join(" * ", childEquations) + ")";
        }

        return "";
    }

    public void GenerateMCS(out string equation, out string simplifiedEquation)
    {


        // 1) Generate the MCS equation from the fault tree.
        equation = GenerateEquation();



        // 2) Simplify the equation using BooleanAlgebra.
        simplifiedEquation = MCSEngine_Exp.ProcessExpression(equation);

        string message = "Generated Tree Expression:\n" + equation +
                         "\n\nSimplified Expression:\n" + simplifiedEquation;
        // MessageBox.Show(message, "MCS Equation Overview", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // 3) List of cut sets referencing the original events
        BuildMinimalCutSets(simplifiedEquation);
        EvaluateMinimalCutSets();
    }

    /// <summary>Creates MinimalCutSets from a simplified expression like "A*B + C".</summary>
    private void BuildMinimalCutSets(string simplifiedEquation)
    {
        MinimalCutSets.Clear();

        int index = 1;
        foreach (string term in simplifiedEquation.Split('+'))
        {
            var events = new List<FTAitem>();
            foreach (string factor in term.Split('*'))
            {
                string tag = factor.Trim();
                var originalEvent = FTAStructure.Values.FirstOrDefault(e => string.Equals(e.Tag, tag, StringComparison.OrdinalIgnoreCase));
                if (originalEvent != null)
                    events.Add(originalEvent);
            }
            MinimalCutSets.Add(new MinimalCutSet(index++, events));
        }
    }

    private static bool IsProbabilityType(ValueTypes type) => type == ValueTypes.P || type == ValueTypes.R;

    /// <summary>
    /// Computes the value of each cut set (AND of its events), the MCS top event value
    /// (OR of the cut sets) and the contribution of each cut set.
    /// Same rules as gate-by-gate calculation: the result is a probability if all inputs
    /// are probabilities, otherwise a frequency per base time unit.
    /// </summary>
    public void EvaluateMinimalCutSets()
    {
        int baseUnit = (int)MainAppSettings.Instance.BaseTimeUnit;

        foreach (var mcs in MinimalCutSets)
        {
            if (mcs.Events.Count == 0)
            {
                mcs.Value = 0;
                mcs.ValueType = ValueTypes.F;
                mcs.ValueUnit = 0;
                continue;
            }

            bool allProbabilities = mcs.Events.All(e => IsProbabilityType(e.ValueType));
            double p = CalculateGate(mcs.Events.Select(ConvertToProbability), mcs.Gate);
            mcs.ValueType = allProbabilities ? ValueTypes.P : ValueTypes.F;
            mcs.Value = allProbabilities ? p : ProbabilityToFrequency(p);
            mcs.ValueUnit = baseUnit;
        }

        if (MinimalCutSets.Count == 0)
        {
            McsTopValue = 0;
            McsTopValueType = ValueTypes.F;
            McsTopValueUnit = 0;
            return;
        }

        bool topAllProbabilities = MinimalCutSets.All(m => IsProbabilityType(m.ValueType));
        double pTop = CalculateGate(
            MinimalCutSets.Select(m => ConvertToProbability(m.Value, m.ValueType, m.ValueUnit)), Gates.OR);
        McsTopValueType = topAllProbabilities ? ValueTypes.P : ValueTypes.F;
        McsTopValue = topAllProbabilities ? pTop : ProbabilityToFrequency(pTop);
        McsTopValueUnit = baseUnit;

        foreach (var mcs in MinimalCutSets)
            mcs.Contribution = mcs.Value / McsTopValue * 100;
    }

    public void GenerateHTMLreport()
    {

        html.Clear();
        // HTML dokument
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"sk\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>OpenFTA export</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body {");
        html.AppendLine("            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;");
        html.AppendLine("            background-color: #f4f6f8;");
        html.AppendLine("            margin: 40px;");
        html.AppendLine("            color: #333;");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        h1 {");
        html.AppendLine("            text-align: center;");
        html.AppendLine("            color: #2c3e50;");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        table {");
        html.AppendLine("            width: 100%;");
        html.AppendLine("            border-collapse: collapse;");
        html.AppendLine("            background-color: #fff;");
        html.AppendLine("            border-radius: 8px;");
        html.AppendLine("            overflow: hidden;");
        html.AppendLine("            box-shadow: 0 4px 8px rgba(0,0,0,0.05);");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        th, td {");
        //html.AppendLine("            padding: 16px;");
        html.AppendLine("            padding: 6px;");
        html.AppendLine("            text-align: left;");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        thead {");
        html.AppendLine("            background-color: #2c3e50;");
        html.AppendLine("            color: white;");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        tbody tr:hover {");
        html.AppendLine("            background-color: #f1f1f1;");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        tbody tr:nth-child(even) {");
        html.AppendLine("            background-color: #f9f9f9;");
        html.AppendLine("        }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("TOPEVENTHOLDER");

        GenerateReport_ListOfBE(html);

        GenerateReport_MCS(html);

        GenerateReport_ImportanceMeasure(html);

        SumChildren(GetItem(TopEventGuid));
        String temp = GenerateReport_TopEvent();

        html.Replace("TOPEVENTHOLDER", temp);

        /*  html.AppendLine("  <script>");
          html.AppendLine("    document.addEventListener('DOMContentLoaded', function () {");
          html.AppendLine("      const getCellValue = (tr, idx) => tr.children[idx].innerText || tr.children[idx].textContent;");
          html.AppendLine("      const comparer = (idx, asc) => (a, b) => {");
          html.AppendLine("        const v1 = getCellValue(asc ? a : b, idx);");
          html.AppendLine("        const v2 = getCellValue(asc ? b : a, idx);");
          html.AppendLine("        const f1 = parseFloat(v1), f2 = parseFloat(v2);");
          html.AppendLine("        return (!isNaN(f1) && !isNaN(f2)) ? f1 - f2 : v1.localeCompare(v2);");
          html.AppendLine("      };");
          html.AppendLine("      document.querySelectorAll('table.sortable th').forEach(th => {");
          html.AppendLine("        th.addEventListener('click', () => {");
          html.AppendLine("          const table = th.closest('table');");
          html.AppendLine("          const tbody = table.tBodies[0];");
          html.AppendLine("          const rows = Array.from(tbody.rows);");
          html.AppendLine("          const idx = Array.from(th.parentNode.children).indexOf(th);");
          html.AppendLine("          rows.sort(comparer(idx, th.asc = !th.asc));");
          html.AppendLine("          rows.forEach(row => tbody.appendChild(row));");
          html.AppendLine("        });");
          html.AppendLine("      });");
          html.AppendLine("    });");
          html.AppendLine("  </script>");*/
        html.AppendLine("  <script>");
        html.AppendLine("    document.addEventListener('DOMContentLoaded', function () {");
        html.AppendLine("      const getCellValue = (tr, idx) => {");
        html.AppendLine("        let v = tr.children[idx].innerText || tr.children[idx].textContent;");
        html.AppendLine("        if (!v) return NaN;");
        html.AppendLine("        v = v.trim();");
        html.AppendLine("        v = v.replace(/,/g, ''); // odstranenie tisicoviek");
        html.AppendLine("        const match = v.match(/[-+]?\\d*\\.?\\d+(?:[eE][-+]?\\d+)?/);");
        html.AppendLine("        if (match) return parseFloat(match[0]);");
        html.AppendLine("        return v;");
        html.AppendLine("      };");
        html.AppendLine("");
        html.AppendLine("      const comparer = (idx, asc) => (a, b) => {");
        html.AppendLine("        const v1 = getCellValue(a, idx);");
        html.AppendLine("        const v2 = getCellValue(b, idx);");
        html.AppendLine("        if (typeof v1 === 'number' && typeof v2 === 'number') {");
        html.AppendLine("          return asc ? v1 - v2 : v2 - v1;");
        html.AppendLine("        } else {");
        html.AppendLine("          return asc ? ('' + v1).localeCompare(v2) : ('' + v2).localeCompare(v1);");
        html.AppendLine("        };");
        html.AppendLine("      };");
        html.AppendLine("");
        html.AppendLine("      document.querySelectorAll('table.sortable th').forEach(th => {");
        html.AppendLine("        th.addEventListener('click', () => {");
        html.AppendLine("          const table = th.closest('table');");
        html.AppendLine("          const tbody = table.tBodies[0];");
        html.AppendLine("          const rows = Array.from(tbody.rows);");
        html.AppendLine("          const idx = Array.from(th.parentNode.children).indexOf(th);");
        html.AppendLine("          const asc = !(th.asc);");
        html.AppendLine("          th.asc = asc;");
        html.AppendLine("          rows.sort(comparer(idx, asc));");
        html.AppendLine("          rows.forEach(row => tbody.appendChild(row));");
        html.AppendLine("        });");
        html.AppendLine("      });");
        html.AppendLine("    });");
        html.AppendLine("  </script>;");




        html.AppendLine("</body>");
        html.AppendLine("</html>");


        /* string path = " .html";
         File.WriteAllText(path, html.ToString(), Encoding.UTF8);*/
    }

    public String GenerateReport_TopEvent()
    {
        String html = "";

        html= "    <h1>Top Event</h1>";

        FTAitem GbG = FTAStructure.First().Value;

        string tempTag = "";
        if(GbG.Tag!=null)
        if (GbG.Tag.Length > 0)
            tempTag = " (" + GbG.Tag + ")";
        html += "    <h2>"+GbG.Name+ tempTag+"</h2>";

        string freqText = McsTopValueType switch
        {
            ValueTypes.F => "f=",
            ValueTypes.P => "P=",
            ValueTypes.R => "R=",
            ValueTypes.Lambda => "λ=",
            _ => ""
        };

        freqText += (McsTopValue < 0.001) ? McsTopValue.ToString("0.0000E+0") : McsTopValue.ToString("F6");
        if (McsTopValueType == ValueTypes.F || McsTopValueType == ValueTypes.Lambda)
        {
            freqText += " " + MetricUnitsList[McsTopValueUnit];

        }

        String temp = "<h3> Minimal cut set result:"+ freqText + "</h3>";
        html += temp;


         

        freqText = GbG.ValueType switch
        {
            ValueTypes.F => "f=",
            ValueTypes.P => "P=",
            ValueTypes.R => "R=",
            ValueTypes.Lambda => "λ=",
            _ => ""
        };

        freqText += (GbG.Value < 0.001) ? GbG.Value.ToString("0.0000E+0") : GbG.Value.ToString("F6");
        if (GbG.ValueType == ValueTypes.F || GbG.ValueType == ValueTypes.Lambda)
        {
            freqText += " " + MetricUnitsList[GbG.ValueUnit];

        }

        temp = "<h3> Gate by Gate result:" + freqText + "</h3>";
        html += temp;

        return (html);
    }

    public void GenerateReport_ListOfBE(StringBuilder html)
    {
        String temp;
        html.AppendLine("    <h1>List of Basic Events</h1>");
        //   html.AppendLine("    <table>");
        html.AppendLine("<table class='sortable'>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>Tag</th>");
        html.AppendLine("                <th>Name</th>");
        html.AppendLine("                <th>Notes</th>");
        html.AppendLine("                <th>Metric</th>");
        html.AppendLine("                <th>Ref.</th>");
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");

        List<FTAitem> l = GenerateListOfBasicEvents();

        var bibliography = GenerateListofreferences();

        foreach (var Event in l)
        {


            temp = "<tr><td>";
            temp += Event.Tag;
            temp += "</td><td>" + Event.Name + "</td ><td>" + Event.Description + "</td ><td>";

            string freqText = Event.ValueType switch
            {
                ValueTypes.F => "f=",
                ValueTypes.P => "P=",
                ValueTypes.R => "R=",
                ValueTypes.Lambda => "λ=",
                _ => ""
            };
            freqText += (Event.Value < 0.001) ? Event.Value.ToString("0.0000E+0") : Event.Value.ToString("F6");
            if (Event.ValueType == ValueTypes.F || Event.ValueType == ValueTypes.Lambda)
            {
                freqText += " " + MetricUnitsList[Event.ValueUnit];

            }
            // freqText = Event.Frequency.ToString();
            temp += freqText;
            temp += "</td>";
            temp += "<td>" + Event.TempReference + "</td></tr>";

             

            html.AppendLine(temp);




        }


         

        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");

        temp = "\n <h2>References</h2>";
        temp += "<ul>";
        foreach (var line in bibliography)
            temp += "<li>" + line + "</li>";
        temp += "</ul>";

        html.AppendLine(temp);

    }

    public void GenerateReport_MCS(StringBuilder html)
    {
        string equation = "";
        string simplifiedEquation = "";

        GenerateMCS(out equation, out simplifiedEquation);

        String temp;
        html.AppendLine("<br>    <h1>Minimal cut sets</h1>");
        // html.AppendLine("    <table>");
        html.AppendLine("<table class='sortable'>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>Name</th>");
        html.AppendLine("               <th>Events</th>");
        html.AppendLine("                <th>Metric</th>");
         html.AppendLine("                <th>MCS Contribution</th>"); 
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");

        /*  int MSCcount = 1;
          for (int i = 0; i < mcs.CutSets.Count; i++)
          {
              if (mcs.CutSets[i].IsMinimal)
              {
                  temp = "<tr><td>";
                  temp += "MSC - " + MSCcount.ToString();
                  MSCcount = MSCcount + 1;
                  temp += "</td><td>";
                  for (int j = 0; j < mcs.CutSets[i].items.Count; j++)
                      temp += "{" + mcs.CutSets[i].items[j].Tag + "} - ";
                  temp = temp.Remove(temp.Length - 2);
                  temp += "</td><td>";
                  temp += mcs.CutSets[i].Freq.ToString();
                  temp += "</td></tr>";
                  html.AppendLine(temp);
              }
          }*/
        foreach (var mcs in MinimalCutSets)
        {
            if (mcs.Events.Count == 0)
                continue;

            temp = "<tr><td>" + mcs.Name + "</td><td>";
            foreach (var evt in mcs.Events)
                temp += "{" + evt.Name + "} - ";
            temp = temp.Remove(temp.Length - 2);
            temp += "</td><td>";

            string freqText = mcs.ValueType switch
            {
                ValueTypes.F => "f=",
                ValueTypes.P => "P=",
                ValueTypes.R => "R=",
                ValueTypes.Lambda => "λ=",
                _ => ""
            };
            freqText += (mcs.Value < 0.001) ? mcs.Value.ToString("0.0000E+0") : mcs.Value.ToString("F6");
            if (mcs.ValueType == ValueTypes.F || mcs.ValueType == ValueTypes.Lambda)
                freqText += " " + MetricUnitsList[mcs.ValueUnit];

            temp += freqText;
            temp += "</td>";

            double MCSEffect = mcs.Contribution;
            string MCSEffectString = (MCSEffect < 0.00001) ? MCSEffect.ToString("0.000E+0") : MCSEffect.ToString("F4");
            temp += "<td>" + MCSEffectString + " %</td>";
            temp += "</tr>";
            html.AppendLine(temp);
        }

        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");
    }

    public void GenerateReport_MCSv2(StringBuilder html)
    {
        // MCSEngine mcs = new MCSEngine(this);
        MSC_Engine_v2 mcs = new MSC_Engine_v2(this);
        mcs.PerformMCS();

        String temp;
        html.AppendLine("<br>    <h1>Minimal cut sets</h1>");
        // html.AppendLine("    <table>");
        html.AppendLine("<table class='sortable'>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>Name</th>");
        html.AppendLine("               <th>Events</th>");
        html.AppendLine("                <th>Metric</th>");
        /*   html.AppendLine("                <th>Metric</th>");*/
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");

        int MSCcount = 1;
        for (int i = 0; i < mcs.CutSets.Count; i++)
        {
            if (mcs.CutSets[i].IsMinimal)
            {
                temp = "<tr><td>";
                temp += "MSC - " + MSCcount.ToString();
                MSCcount = MSCcount + 1;
                temp += "</td><td>";
                for (int j = 0; j < mcs.CutSets[i].items.Count; j++)
                    temp += "{" + mcs.CutSets[i].items[j].Tag + "} - ";
                temp = temp.Remove(temp.Length - 2);
                temp += "</td><td>";
                temp += mcs.CutSets[i].Freq.ToString();
                temp += "</td></tr>";
                html.AppendLine(temp);
            }
        }


        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");
    }

    public void GenerateReport_ImportanceMeasure(StringBuilder html)
    {
        String temp;
        html.AppendLine("<br>    <h1>Importance Measures</h1>");
        html.AppendLine("<table class='sortable'>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>Name</th>");
        html.AppendLine("                <th>Tag</th>");
        html.AppendLine("                <th>BIM</th>");
        html.AppendLine("                <th>CIM</th>");
        html.AppendLine("                <th>RAW</th>");
        html.AppendLine("                <th>RRW</th>");
        html.AppendLine("                <th>FV</th>");

        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");


        foreach (FTAitem evt in FTAStructure.Values)
        {
            if (evt.ItemType >= 2)
            {
                html.AppendLine("            <tr>");
                double bimValue = ComputeBIM(evt);
                double cimValue = ComputeCIM(evt);
                double rawValue = ComputeRAW(evt);
                double rrwValue = ComputeRRW(evt);
                double fvValue = ComputeFV(evt);

                temp = "<td>" + evt.Name + "</td><td>" + evt.Tag + "</td>";
                temp += "<td>" + FormatMeasure(bimValue) + "</td>";
                temp += "<td>" + FormatMeasure(cimValue) + "</td>";
                temp += "<td>" + FormatMeasure(rawValue) + "</td>";
                temp += "<td>" + FormatMeasure(rrwValue) + "</td>";
                temp += "<td>" + FormatMeasure(fvValue) + "</td></tr>";

                html.AppendLine(temp);



            }
        }
        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");
    }

    private static string FormatMeasure(double value) =>
        double.IsNaN(value) ? "n/a"
        : double.IsPositiveInfinity(value) ? "∞"
        : Math.Round(value, 8).ToString();

    public List<FTAitem> GenerateListOfBasicEvents()
    {
        List<FTAitem> basicEvents = new List<FTAitem>();
        foreach (var item in FTAStructure.Values)
        {
            if (item.ItemType >= 2) // Basic event
            {
                basicEvents.Add(item);
            }
        }
        return basicEvents;
    }
    // --- From P, R, λ to  f (v 1/y) ---

    public List<string> GenerateListofreferences()
    {
        var filteredItems = FTAStructure.Values
    .Where(i => i.ItemType > 1 && !string.IsNullOrWhiteSpace(i.Reference))
    .ToList();

        // 1️⃣ Získanie unikátnych referencií
        var uniqueRefs = filteredItems
            .Select(i => i.Reference)
            .Distinct()
            .ToList();

        // 2️⃣ Priradenie čísla (TempReference) každému FTAitem
        var refIndex = uniqueRefs
            .Select((refText, index) => new { refText, index })
            .ToDictionary(x => x.refText, x => $"[{x.index + 1}]");

        foreach (var item in filteredItems)
        {
            if (refIndex.ContainsKey(item.Reference))
            {
                item.TempReference = refIndex[item.Reference];
            }
        }

        // 3️⃣ Vytvorenie číslovaného zoznamu literatúry
        var bibliography = uniqueRefs
            .Select((r, i) => $"[{i + 1}] {r}")
            .ToList();

        return bibliography;
         
    }

    public static double FrequencyFromProbability(double P)
    {
        if (P >= 1.0) throw new ArgumentException("Probability P must be < 1");
        if (P <= 0.0) return 0.0;
        return -Math.Log(1.0 - P);
    }

    public static double FrequencyFromReliability(double R)
    {
        if (R <= 0.0 || R > 1.0) throw new ArgumentException("R must be in (0,1]");
        return -Math.Log(R);
    }

    public static double FrequencyFromLambda(double lambda, int lambdaUnitIndex)
    {
        if (!TimeUnitFactors.ContainsKey(lambdaUnitIndex))
            throw new ArgumentException($"Unknown index: {lambdaUnitIndex}");

        double factor = TimeUnitFactors[lambdaUnitIndex];
        return lambda * factor; // to/year
    }

    // --- From f ( 1/rok) back ---

    public static double ProbabilityFromFrequency(double f)
    {
        return 1.0 - Math.Exp(-f);
    }

    public static double ReliabilityFromFrequency(double f)
    {
        return Math.Exp(-f);
    }

    public static double LambdaFromFrequency(double f, int targetUnitIndex)
    {
        if (!TimeUnitFactors.ContainsKey(targetUnitIndex))
            throw new ArgumentException($"Unknown index: {targetUnitIndex}");

        double factor = TimeUnitFactors[targetUnitIndex];
        return f / factor;
    }

    public void SelectChildren(FTAitem parent, bool all)
    {
        foreach (var childGuid in parent.Children)
        {
            var child = GetItem(childGuid);
            if (child != null)
            {
                child.IsSelected = true;
                SelectedEvents.Add(child);
                if (all)
                    SelectChildren(child, all);
            }
        }
    }

    private string GetNextAvailableTag(int itemType)
    {
        string prefix = GetPrefixForType(itemType);

        // pozbierame všetky obsadené čísla pre daný prefix
        var usedNumbers = FTAStructure.Values
            .Where(item => item.Tag != null && item.Tag.StartsWith(prefix))
            .Select(item =>
            {
                if (int.TryParse(item.Tag.Substring(prefix.Length), out int num))
                    return num;
                return (int?)null;
            })
            .Where(n => n.HasValue)
            .Select(n => n.Value)
            .OrderBy(n => n)
            .ToList();

        int next = 1;
        foreach (var n in usedNumbers)
        {
            if (n == next)
                next++;
            else if (n > next)
                break;
        }

        return $"{prefix}{next}";
    }

    private string GetPrefixForType(int itemType)
    {
        return itemType switch
        {
            1 => "IE",
            2 => "BE",
            _ => "XX" // fallback pre neznámy typ
        };
    }

    public string GetNextAvailableBETag()
    {
        var usedNumbers = FTAStructure.Values
            .Where(item => item.Tag != null && item.Tag.StartsWith("BE"))
            .Select(item =>
            {
                if (int.TryParse(item.Tag.Substring(2), out int num))
                    return num;
                return (int?)null;
            })
            .Where(num => num.HasValue)
            .Select(num => num.Value)
            .OrderBy(n => n)
            .ToList();

        int next = 1;
        foreach (int n in usedNumbers)
        {
            if (n == next)
                next++;
            else if (n > next)
                break;
        }

        return $"BE{next}";
    }


    List<MessageItem> ErrorMessages = new List<MessageItem>();
    public bool PerformFullTest()
    {
        ErrorMessages.Clear();


        bool res = true;

        if (!PerformTestHasbasicEvents())
        {
            res = false;
        }

        if(MainAppSettings.Instance.GateANDFrequencyHandling != GateFrequencyHandling.Allow)
        if (!PerformTestAND_2F())
        {
                if(MainAppSettings.Instance.GateANDFrequencyHandling == GateFrequencyHandling.Disallow)
            res = false;
        }

        if (MainAppSettings.Instance.GateORFrequencyHandling != GateFrequencyHandling.Allow)
            if (!PerformTestOR_FORP())
            {
                if (MainAppSettings.Instance.GateORFrequencyHandling == GateFrequencyHandling.Disallow)
                    res = false; 
            }

        if (ErrorMessages.Count>0) ErrorDialog.ShowMessages(ErrorMessages);

        return (res);
    }
    private bool PerformTestHasbasicEvents()
    {
        bool res = true;
        foreach (var item in FTAStructure.Values)
        {
            if (item.Children.Count == 0 && (item.ItemType == 1 || item.ItemType == 0))
            {

                string txt = "Event: " + item.Name + " (" + item.Tag + ") is an intermediate event and has no children.";
                ErrorMessages.Add(new MessageItem(MessageType.Error, txt));
                txt = "To fix it change event to basic event or add children events.";
                ErrorMessages.Add(new MessageItem(MessageType.Info, txt));
                res = false;
            }
        }
        return (res);
    }

    private bool PerformTestAND_2F()
    {
        bool res = true;
        foreach (var item in FTAStructure.Values)
        {
            int NumberOfChildrenWithFrequency = 0;

            foreach (var childGuid in item.Children)
            {
                if (FTAStructure.TryGetValue(childGuid, out FTAitem child))
                {
                    if (child.ValueType == ValueTypes.F || child.ValueType == ValueTypes.Lambda)
                    {
                        NumberOfChildrenWithFrequency = NumberOfChildrenWithFrequency + 1;
                    }
                }
            }
            if (item.Gate == Gates.AND && NumberOfChildrenWithFrequency >= 2)
            {
                if (MainAppSettings.Instance.GateANDFrequencyHandling == GateFrequencyHandling.Disallow)
                {
                    string txt = "ERROR - Event: " + item.Name + " (" + item.Tag + ") has an AND gate with " + NumberOfChildrenWithFrequency.ToString() + " frequency events (F or λ). This configuration is not permitted based on the current settings.";
                    ErrorMessages.Add(new MessageItem(MessageType.Error, txt));
                    txt = "Change one of the frequency events to a probability event. Or change the setting “AND gate with two frequency events” to “Allow” or “Allow with warning.";
                    ErrorMessages.Add(new MessageItem(MessageType.Info, txt));
                }
                else
                {
                    string txt = "WARNING - Event: " + item.Name + " (" + item.Tag + ") has an AND gate with " + NumberOfChildrenWithFrequency.ToString() + " frequency events (F or λ). This configuration may lead to incorrect probability results.";
                    ErrorMessages.Add(new MessageItem(MessageType.Warning, txt));
                    txt = "To improve model consistency, consider changing event types or adjusting the calculation settings";
                    ErrorMessages.Add(new MessageItem(MessageType.Info, txt));
                }
                res = false;
            }


        }
        return (res);
    }

    private bool PerformTestOR_FORP()
    {
        bool res = true;
        foreach (var item in FTAStructure.Values)
        {
            int NumberOfChildrenWithFrequency = 0;
            int NumberOfChildrenWithProbability = 0;

            foreach (var childGuid in item.Children)
            {
                if (FTAStructure.TryGetValue(childGuid, out FTAitem child))
                {
                    if (child.ValueType == ValueTypes.F || child.ValueType == ValueTypes.Lambda)
                    {
                        NumberOfChildrenWithFrequency = NumberOfChildrenWithFrequency + 1;
                    }

                    if (child.ValueType == ValueTypes.P || child.ValueType == ValueTypes.R)
                    {
                        NumberOfChildrenWithProbability = NumberOfChildrenWithProbability + 1;
                    }
                }
            }
            if (NumberOfChildrenWithProbability>0 && NumberOfChildrenWithFrequency > 0)
            {
                if (MainAppSettings.Instance.GateORFrequencyHandling == GateFrequencyHandling.Disallow)
                {
                    string txt = "ERROR - Event: " + item.Name + " (" + item.Tag + ") has an OR gate and contains a combination of a probability event and a frequency event. This configuration is not permitted based on the current settings.";
                    ErrorMessages.Add(new MessageItem(MessageType.Error, txt));
                    txt = "Change both events to the same type (either both as probability events or both as frequency events).\r\n Or change the setting “OR gate with mixed event types” to “Allow” or “Allow with warning.”";
                    ErrorMessages.Add(new MessageItem(MessageType.Info, txt));
                }
                if (MainAppSettings.Instance.GateORFrequencyHandling == GateFrequencyHandling.AllowWithWarning)
                {
                    string txt = "WARNING - Event: " + item.Name + " (" + item.Tag + ") has an OR gate and contains a combination of a probability event and a frequency event. This configuration may lead to inconsistent or invalid probability results.";
                    ErrorMessages.Add(new MessageItem(MessageType.Warning, txt));
                    txt = "To improve model consistency, consider changing event types or adjusting the calculation settings";
                    ErrorMessages.Add(new MessageItem(MessageType.Info, txt));
                }

                res = false;
            }


        }
        return (res);
    }

    public bool IsAnyItemOverlapping()
    {
        var items = FTAStructure.Values.ToList();

        for (int i = 0; i < items.Count; i++)
        {
            var rect1 = items[i].rect;

            for (int j = i + 1; j < items.Count; j++)
            {
                var rect2 = items[j].rect;

                if (rect1.IntersectsWith(rect2) && items[i].IsHidden == false && items[j].IsHidden == false)
                    //if (rect1.IntersectsWith(rect2))
                    return true;
            }
        }

        return false;
    }


    bool TempTreeAlignSuccess; bool GlobalStop; public int TemActualX;

    public void ArrangeTree()
    {
        //ArrangeEventsAlgo1();
        ArrangeEventsAlgo0();

        if(MainAppSettings.Instance.SortingAlgoVersion==SortingStrategy.ALGOI)
            ArrangeEventsAlgo0();
        if (MainAppSettings.Instance.SortingAlgoVersion == SortingStrategy.ALGOII_centered || MainAppSettings.Instance.SortingAlgoVersion == SortingStrategy.ALGOII_align_left)
        {
            ArrangeEventsAlgo1();
            MyDrawingEngine.SetStructure(FTAStructure);
        }

        
    }

    public void ArrangeEventsAlgo1()
    {
        var backup = new Dictionary<Guid, FTAitem>(FTAStructure);
        foreach (FTAitem evnt in FTAStructure.Values)
        {
            if (evnt.IsHidden)
                FTAStructure.Remove(evnt.GuidCode);
        }
        FindAllChilren(FTAStructure);

        TemActualX = 0;
        PrepareTreeForAlign(FTAStructure.Values.First(), 0);


        AssignLevelsToAllEvents();
        SetDefaultYpositions(GetItem(TopEventGuid), GetItem(TopEventGuid).Y);
        AddParentToMiddle(GetItem(TopEventGuid));
        GlobalStop = false;


        for (int i = 0; i < 500; i++)
        {
            TempTreeAlignSuccess = false;
            MoveSubtreeToLeft(GetItem(TopEventGuid), 90);
            if (!TempTreeAlignSuccess || GlobalStop)
                break;
        }



        for (int i = 0; i < 500; i++)
        {
            TempTreeAlignSuccess = false;
            MoveSubtreeToLeft(GetItem(TopEventGuid), 50);
            if (!TempTreeAlignSuccess || GlobalStop)
                break;
        }

        for (int i = 0; i < 500; i++)
        {
            TempTreeAlignSuccess = false;
            MoveSubtreeToLeft(GetItem(TopEventGuid), 10);
            if (!TempTreeAlignSuccess || GlobalStop)
                break;
        }


        for (int i = 0; i < 500; i++)
        {
            TempTreeAlignSuccess = false;
            MoveSubtreeToLeft(GetItem(TopEventGuid), 1);
            if (!TempTreeAlignSuccess)
                break;
        }

        FTAStructure = new Dictionary<Guid, FTAitem>(backup);
        FindAllChilren(FTAStructure);

    }

    public void SetDefaultYpositions(FTAitem itm, int TopEventY)
    {
        for (int i = 0; i < itm.Children.Count; i++)
        {
            FTAitem c = GetItem(itm.Children[i]);
            c.Y = c.level * (int)(Constants.EventVerticalSpacing + Constants.EventHeight) + TopEventY;

            SetDefaultYpositions(c, TopEventY);
        }
        itm.Y = itm.level * (int)(Constants.EventVerticalSpacing + Constants.EventHeight) + TopEventY;
    }

    public void AddParentToMiddle(FTAitem itm)
    {
        int min = 111111111;
        int max = -111111111;
        for (int i = 0; i < itm.Children.Count; i++)
        {
            FTAitem c = GetItem(itm.Children[i]);
            AddParentToMiddle(c);


            if(MainAppSettings.Instance.SortingAlgoVersion==SortingStrategy.ALGOII_centered)
                itm.X = GetItem(itm.Children[0]).X + (int)Math.Round((GetItem(itm.Children[itm.Children.Count - 1]).X - GetItem(itm.Children[0]).X) / 2.0, MidpointRounding.ToPositiveInfinity);
            else
                itm.X = GetItem(itm.Children[0]).X;
        }

    }

    public void AddParentToMiddle()
    {
        AddParentToMiddle(GetItem(TopEventGuid));
    }
         
    public void MoveSubtreeToLeft(FTAitem fTAitem, int shift)
    {

        for (int i = 0; i < fTAitem.Children.Count; i++)
        {

            var child = GetItem(fTAitem.Children[i]);

            if ((i > 0))
            {


                OneStepLeft(child, shift);
                AddParentToMiddle();


                if (IsAnyItemOverlapping())
                {
                    OneStepLeft(child, -shift);
                    AddParentToMiddle();
                }
                else
                    TempTreeAlignSuccess = true;

                if (IsAnyItemOverlapping())
                {
                    OneStepLeft(child, -1);
                    AddParentToMiddle();
                }
            }
            AddParentToMiddle(fTAitem);
            MoveSubtreeToLeft(child, shift);

        }

    }

    public void OneStepLeft(FTAitem fTAitem, int shift)
    {


        foreach (var childGuid in fTAitem.Children)
        {
            var child = GetItem(childGuid);
            OneStepLeft(child, shift);
            child.X = child.X - shift;
        }

        fTAitem.X = fTAitem.X - shift;
    }

    public void OneStepBack(FTAitem fTAitem, int shift)
    {

        fTAitem.X = fTAitem.X + shift;
        foreach (var childGuid in fTAitem.Children)
        {
            var child = GetItem(childGuid);
            OneStepBack(child, shift);
            child.X = child.X + shift;
        }
    }
           
    public void PrepareTreeForAlign(FTAitem fTAitem, int actualx)
    {

        foreach (var childGuid in fTAitem.Children)
        {
            var child = GetItem(childGuid);
            PrepareTreeForAlign(child, actualx);

            child.X = TemActualX;
            // TemActualX = TemActualX + Constants.EventWidth+22;            
            TemActualX = TemActualX + Constants.EventWidth + Constants.EventHorizontalSpacing + 1;
        }
        fTAitem.X = TemActualX;


    }

    public void ArrangeEventsAlgo0()
    {
        FTAitem topEvent = FTAStructure.Values.FirstOrDefault(e => e.Parent == Guid.Empty);
        if (topEvent == null)
            return;

        // Set top event position
        topEvent.X = 0;
        topEvent.Y = 50;

        ArrangeChildren(topEvent);


    }
    private void ArrangeChildren(FTAitem parent)
    {
        int verticalSpacing = 180;
        int gap = 20;

        if (parent.Children == null || parent.Children.Count == 0)
            return;

        double allocatedWidth = ComputeSubtreeWidth(parent, gap);
        double startX = parent.X - allocatedWidth / 2;
        int childY = parent.Y + verticalSpacing;

        double currentX = startX;
        foreach (Guid childGuid in parent.Children)
        {
            if (FTAStructure.TryGetValue(childGuid, out FTAitem child))
            {
                double childWidth = ComputeSubtreeWidth(child, gap);
                child.X = (int)(currentX + childWidth / 2);
                child.Y = childY;
                currentX += childWidth + gap;
                ArrangeChildren(child);
            }
        }
    }
    private double ComputeSubtreeWidth(FTAitem node, int gap)
    {
        if (node.Children == null || node.Children.Count == 0)
            return Constants.EventWidth;

        double totalWidth = 0;
        foreach (Guid childGuid in node.Children)
        {
            if (FTAStructure.TryGetValue(childGuid, out FTAitem child))
            {
                totalWidth += ComputeSubtreeWidth(child, gap) + gap;
            }
        }
        totalWidth -= gap;
        return Math.Max(totalWidth, Constants.EventWidth);
    }
}

public static class Constants
{
    public static int EventWidth { get; set; } = 140;
    public static int EventHeight { get; set; } = 90;
    public static int EventHorizontalSpacing { get; set; } = 20;
    public static int EventVerticalSpacing { get; set; } = 80;
}





