using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;

public class MSC_Engine_v2
{
    FTAlogic EngineLogic;
    private List<bool[]> CombinationMatrix;
    private int NumberOftags;
    DataTable MCSCombinationResultsTable;
    DataTable MCSresultsTable;

    List<ulong> all;
    HashSet<ulong> remaining; 

    struct UniqueTagsStruct
    {
        public String Tag;
        public List<FTAitem> items;
    }

    public class CutSetsStruct
    {

        public bool IsMinimal;
        public List<FTAitem> items;
        public bool[] ItemStates;
        public String Name;
        public double Freq;
        public String result;
    }

    List<UniqueTagsStruct> UniqueTags;
    public List<CutSetsStruct> CutSets;

    public   MSC_Engine_v2(FTAlogic EngineLogicIN)
    {
        EngineLogic = EngineLogicIN;
        CombinationMatrix = new List<bool[]>();
        UniqueTags = new List<UniqueTagsStruct>();
        MCSCombinationResultsTable = new DataTable();
        MCSresultsTable = new DataTable();
        CutSets = new List<CutSetsStruct>();
        all = new List<ulong>();
        remaining = new HashSet<ulong>(all);
    }

    public void PerformMCS()
    {
        CombinationMatrix.Clear();
        UniqueTags.Clear();
        MCSCombinationResultsTable.Clear();
        GenrateUniqueTags();
       
        GenerateCombinationMatrix();

        remaining =  new HashSet<ulong>(all);

        TestAllCombinations();
        //EliminateSubSets();
        //PrintResults();
    }

    public void TestAllCombinations()
    {
        bool Solutionfound = false;

        while(remaining.Count > UniqueTags.Count*4)
        {
            int val = Random.Shared.Next(0,all.Count); // 5 až MAX

            TestOneExample(val);
            all.Clear();
            all.AddRange(remaining);
        }



        for (int i = 0; i < 100000; i++)
        {
            bool res = TestOneExample();
            all.Clear();
            all.AddRange(remaining);
            if (res == false)
            {
                Solutionfound = true;
                break;
                    }
        }

        int CutSetCount = 1;

        for (int i = 0; i < all.Count; i++)
        {


            CutSetsStruct CS = new CutSetsStruct();
            CS.IsMinimal = true;
            CS.ItemStates = UlongToBoolArray(all[i], UniqueTags.Count);

            CutSetCount += 1;
            CS.Name = "Cut set" + CutSetCount.ToString();


            CS.items = new List<FTAitem>();
            int j = 0;

            double Frek = 0;

            foreach (UniqueTagsStruct UTS in UniqueTags)
            {

                if (CS.ItemStates[j])
                {
                    CS.items.Add(UTS.items[0]);

                   /* if (Frek == 0)
                        Frek = UTS.items[0].Frequency;
                    else
                        Frek *= UTS.items[0].Frequency;*/

                    if (Frek == 0)
                        Frek = UTS.items[0].Value;
                    else
                        Frek *= UTS.items[0].Value;

                }
                j = j + 1;
            }

            CS.Freq = Frek;

            CutSets.Add(CS);
        }


    }

   
    public bool TestOneExample(int i)
    {
        var temp = UlongToBoolArray(all[i], UniqueTags.Count);
        ComputeTreeLogic(temp);
        if (EngineLogic.GetItem(EngineLogic.TopEventGuid).ItemState == true)
        {
            RemoveSupersets(all[i], UniqueTags.Count, remaining);
            return (true);
        }
        else
        {
            RemoveExact(all[i], remaining);
            return (true);
        }       
    }
    public bool TestOneExample()
            {
                bool res = false;

                for (int i = 0; i < all.Count; i++)
                {
                    var temp = UlongToBoolArray(all[i], UniqueTags.Count);

                    ComputeTreeLogic(temp);



                    if (EngineLogic.GetItem(EngineLogic.TopEventGuid).ItemState == true)
                    {

                        RemoveSupersets(all[i], UniqueTags.Count, remaining);
                        if (all.Count == remaining.Count)
                        {

                        }
                        else
                            return (true);
                    }
                    else
                    {
                        RemoveExact(all[i], remaining);
                        return (true);
                    }
                }

                return (false);

            }

    public void ComputeTreeLogic(bool[] TagsLogic)
    {
        foreach (var Event in EngineLogic.FTAStructure)
        {
            Event.Value.ItemState = false;
        }

        int j = 0;
        foreach (UniqueTagsStruct UTS in UniqueTags)
        {
            for (int i = 0; i < UTS.items.Count; i++)
                UTS.items[i].ItemState = TagsLogic[j];

            j = j + 1;
        }

        GetChildrenLogic(EngineLogic.GetItem(EngineLogic.TopEventGuid));
    }

    public void GetChildrenLogic(FTAitem parent)
    {
        for (int i = 0; i < parent.Children.Count; i++)
        {
            GetChildrenLogic(EngineLogic.GetItem(parent.Children[i]));

            Gates gate = parent.Gate;
            bool f = EngineLogic.GetItem(parent.Children[i]).ItemState;

            if (i == 0)
                parent.ItemState = f;
            else
            {
                //AND
                if (gate == Gates.OR)
                    parent.ItemState = parent.ItemState | f;
                //OR
                if (gate == Gates.AND)
                    parent.ItemState = parent.ItemState & f;
            }
        }
    }
    public void GenrateUniqueTags()
    {
        HashSet<String> temp = new HashSet<string>();

        foreach (var Event in EngineLogic.FTAStructure)
        {
            if (Event.Value.ItemType >= 2)
                temp.Add(Event.Value.Tag);
        }

        NumberOftags = temp.Count();

        foreach (var t in temp)
        {
            var l = EngineLogic.GetItems(t);

            UniqueTagsStruct UTS;
            UTS.Tag = t;
            UTS.items = l;

            UniqueTags.Add(UTS);
        }
    }

    public void GenerateCombinationMatrix()
    {
        ulong max = 1UL << UniqueTags.Count();
        all = Enumerable.Range(0, (int)max).Select(i => (ulong)i).ToList();
    }

    static void RemoveSupersets(ulong item, int n, HashSet<ulong> remaining)
    {
        foreach (var superset in GenerateSupersets(item, n))
        {
            if (superset != item)
                remaining.Remove(superset);
        }
    }

    // Vygeneruj všetky supersety pre daný prvok
    static IEnumerable<ulong> GenerateSupersets(ulong item, int n)
    {
        ulong allBits = (1UL << n) - 1;
        ulong unsetBits = (~item) & allBits;

        List<int> unsetPositions = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if (((unsetBits >> i) & 1) == 1)
                unsetPositions.Add(i);
        }

        int total = 1 << unsetPositions.Count;

        for (int mask = 0; mask < total; mask++)
        {
            ulong superset = item;
            for (int j = 0; j < unsetPositions.Count; j++)
            {
                if (((mask >> j) & 1) == 1)
                    superset |= (1UL << unsetPositions[j]);
            }
            yield return superset;
        }
    }

    static ulong BoolArrayToUlong(bool[] bits)
    {
        ulong value = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
                value |= 1UL << (bits.Length - 1 - i); // zľava doprava
        }
        return value;
    }

    // Prevod: ulong -> bool[]
    static bool[] UlongToBoolArray(ulong value, int length)
    {
        var result = new bool[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = ((value >> (length - 1 - i)) & 1UL) == 1UL;
        }
        return result;
    }
    static void RemoveExact(ulong item, HashSet<ulong> remaining)
    {
        remaining.Remove(item);
    }

    static void RemoveExact(bool[] bits, HashSet<ulong> remaining)
    {
        ulong item = BoolArrayToUlong(bits);
        remaining.Remove(item);
    }

}

