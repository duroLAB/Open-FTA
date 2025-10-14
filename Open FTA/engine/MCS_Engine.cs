using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

using System.Collections;
using System.Text;
using System.Threading.Tasks;


class MCSEngine
{
    FTAlogic EngineLogic;
    private List<bool[]> CombinationMatrix;
    private int NumberOftags;
    DataTable MCSCombinationResultsTable;
    DataTable MCSresultsTable;


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

    public MCSEngine(FTAlogic EngineLogicIN)
    {
        EngineLogic = EngineLogicIN;
        CombinationMatrix = new List<bool[]>();
        UniqueTags = new List<UniqueTagsStruct>();
        MCSCombinationResultsTable = new DataTable();
        MCSresultsTable = new DataTable();
        CutSets = new List<CutSetsStruct>();
    }

    public void PerformMCS()
    {
        CombinationMatrix.Clear();
        UniqueTags.Clear();
        MCSCombinationResultsTable.Clear();
        GenrateUniqueTags();
        for (int i = 0; i < UniqueTags.Count; i++)
        {
            MCSCombinationResultsTable.Columns.Add(UniqueTags[i].Tag);
        }
        MCSCombinationResultsTable.Columns.Add("Top Event");
        MCSCombinationResultsTable.Columns.Add("Frequency");
        MCSCombinationResultsTable.Columns.Add("Result");

        GenerateCombinationMatrix();
        TestAllCombinations();
        EliminateSubSets();
        PrintResults();
    }

    public void TestAllCombinations()
    {
        int CutSetCount = 0;

        for (int i = 0; i < CombinationMatrix.Count; i++)
        {
            ComputeTreeLogic(CombinationMatrix[i]);

            DataRow r = MCSCombinationResultsTable.NewRow();

            MCSCombinationResultsTable.Rows.Add(r);

            for (int j = 0; j < UniqueTags.Count; j++)
                r[j] = CombinationMatrix[i][j] ? 1 : 0;

            r[UniqueTags.Count] = EngineLogic.GetItem(EngineLogic.TopEventGuid).ItemState;

            if (EngineLogic.GetItem(EngineLogic.TopEventGuid).ItemState == true)
            {
                r[UniqueTags.Count + 1] = "Cut Set";



                CutSetsStruct CS = new CutSetsStruct();
                CS.IsMinimal = true;
                CS.ItemStates = CombinationMatrix[i];

                CutSetCount += 1;
                CS.Name = "Cut set" + CutSetCount.ToString();


                CS.items = new List<FTAitem>();
                int j = 0;

                double Frek = 0;

                foreach (UniqueTagsStruct UTS in UniqueTags)
                {

                    if (CombinationMatrix[i][j])
                    {
                        CS.items.Add(UTS.items[0]);

                        if (Frek == 0)
                            Frek = UTS.items[0].Frequency;
                        else
                            Frek *= UTS.items[0].Frequency;

                    }
                    j = j + 1;
                }

                CS.Freq = Frek;

                CutSets.Add(CS);

                r[UniqueTags.Count + 1] = Frek.ToString();
                r[UniqueTags.Count + 2] = CS.Name;

            }

        }
    }

    public void PrintResults()
    {

        Form f = new Form();
        f.Text = "Explicit MCS results";
        f.Width = 1100;
        f.Height = 600;

        TabControl tabControl = new TabControl { Dock = DockStyle.Fill };
        tabControl.Parent = f;

        TabPage tabPage = new TabPage("Combination Matrix");
        tabControl.TabPages.Add(tabPage);

        TabPage tabPageCS = new TabPage("Cut Sets");
        tabControl.TabPages.Add(tabPageCS);


        DataGridView DGV = new DataGridView();
        DGV.Parent = tabPage;
        DGV.Dock = DockStyle.Fill;
        DGV.DataSource = MCSCombinationResultsTable;


        DataGridView DGV2 = new DataGridView();
        DGV2.Parent = tabPageCS;
        DGV2.Dock = DockStyle.Fill;
        DGV2.DataSource = MCSresultsTable;


        DGV2.ReadOnly = true;
        DGV2.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
        DGV2.AllowUserToAddRows = false;
        DGV2.RowHeadersVisible = false;
        DGV.ReadOnly = true;
        DGV.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
        DGV.AllowUserToAddRows = false;
        DGV.RowHeadersVisible = false;


        f.ShowDialog();
    }

    public bool IdentifySubSets(bool[] Child, bool[] Parent)
    {
        bool res = true;

        for (int i = 0; i < Child.Length; i++)
        {
            if (Child[i] == true && Parent[i] != true)
                return (false);
        }

        return (res);
    }

    public void EliminateSubSets()
    {
        for (int i = 0; i < CutSets.Count; i++)
            CutSets[0].IsMinimal = true;

        for (int i = 0; i < CutSets.Count; i++)
        {
            var child = CutSets[i].ItemStates;

            for (int j = 0; j < CutSets.Count; j++)
            {
                var parent = CutSets[j].ItemStates;

                if (i != j)
                {
                    bool res = IdentifySubSets(child, parent);
                    if (res == true)
                    {
                        CutSets[j].IsMinimal = false;
                        CutSets[j].result = "Superset of:" + CutSets[i].Name;
                    }



                }

            }
        }

        for (int i = 0; i < UniqueTags.Count; i++)
        {
            MCSresultsTable.Columns.Add(UniqueTags[i].Tag);
        }
        MCSresultsTable.Columns.Add("Name");
        MCSresultsTable.Columns.Add("Result");
        MCSresultsTable.Columns.Add("Type");
        MCSresultsTable.Columns.Add("Frequency");

        for (int i = 0; i < CutSets.Count; i++)
        {
            DataRow r = MCSresultsTable.NewRow();
            MCSresultsTable.Rows.Add(r);

            for (int j = 0; j < CutSets[i].ItemStates.Length; j++)
                r[j] = CutSets[i].ItemStates[j];


            r[UniqueTags.Count + 3] = CutSets[i].Freq.ToString();
            r[UniqueTags.Count + 2] = CutSets[i].IsMinimal ? "Minimal cut set" : "Cut set";
            r[UniqueTags.Count + 1] = CutSets[i].result;

            r[UniqueTags.Count] = CutSets[i].Name;
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

    public void ComputeTree()
    {
        var TopEvent = EngineLogic.GetItem(EngineLogic.TopEventGuid);
        GetChildrenLogic(TopEvent);
        return;
    }

    public void GetChildrenLogic(FTAitem parent)
    {
        for (int i = 0; i < parent.Children.Count; i++)
        {
            GetChildrenLogic(EngineLogic.GetItem(parent.Children[i]));

            int gate = parent.GateType;
            bool f = EngineLogic.GetItem(parent.Children[i]).ItemState;

            if (i == 0)
                parent.ItemState = f;
            else
            {
                //AND
                if (gate == 1)
                    parent.ItemState = parent.ItemState | f;
                //OR
                if (gate == 2)
                    parent.ItemState = parent.ItemState & f;
            }
        }
    }

    public void GenerateCombinationMatrix()
    {
        int n = NumberOftags;
        double count = Math.Pow(2, n);
        for (int i = 0; i < count; i++)
        {
            string str = Convert.ToString(i, 2).PadLeft(n, '0');
            bool[] boolArr = str.Select((x) => x == '1').ToArray();
            CombinationMatrix.Add(boolArr);
        }
    }

    public void FillTreeNode(TreeNode t)
    {
        t.Nodes.Clear();
        for (int i = 0; i < CutSets.Count; i++)
        {
            DataRow r = MCSresultsTable.NewRow();
            MCSresultsTable.Rows.Add(r);

            if (CutSets[i].IsMinimal)
            {
                String temp = CutSets[i].Name + " (f=" + CutSets[i].Freq.ToString() + ")";
                TreeNode children = t.Nodes.Add(temp);

                for (int j = 0; j < CutSets[i].items.Count; j++)
                    children.Nodes.Add(CutSets[i].items[j].Name);

            }
        }
    }
}

