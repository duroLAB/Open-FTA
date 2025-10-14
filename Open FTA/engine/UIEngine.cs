using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


class UIlogic
{
    FTAlogic EngineLogic;
    public UIlogic(FTAlogic f)
    {
        EngineLogic = f;
    }
    public TreeNode TreeNodeMinimalCutSet;

    public void FillTreeView(TreeView treeView1)
    {
        treeView1.Nodes.Clear();

        TreeNode MainTree = treeView1.Nodes.Add("Basic Structure");
        TreeNodeMinimalCutSet = treeView1.Nodes.Add("Minimal cut sets");
        FTAitem topItem = EngineLogic.GetItem(EngineLogic.TopEventGuid);
        if (topItem == null)
        {
            MessageBox.Show("Top event not found.");
            return;
        }
        TreeNode TopEvent = MainTree.Nodes.Add(topItem.GuidCode.ToString(), topItem.Name);
        EngineLogic.FindAllChilren();

        try
        {
            foreach (var evt in EngineLogic.FTAStructure.Values)
            {
                foreach (var childGuid in EngineLogic.FTAStructure[evt.GuidCode].Children)
                {
                    TreeNode parentNode = treeView1.Nodes.Find(evt.GuidCode.ToString(), true).FirstOrDefault();

                    if (parentNode == null)
                    {
                        TreeNode newParentNode = new TreeNode(EngineLogic.FTAStructure[evt.GuidCode].Name)
                        {
                            Name = evt.GuidCode.ToString()
                        };
                        TreeNode grandParentNode = treeView1.Nodes.Find(EngineLogic.FTAStructure[evt.GuidCode].Parent.ToString(), true).FirstOrDefault();
                        if (grandParentNode != null)
                            grandParentNode.Nodes.Add(newParentNode);
                        else
                            MainTree.Nodes.Add(newParentNode);

                        parentNode = newParentNode;
                    }
                    parentNode.Nodes.Add(childGuid.ToString(), EngineLogic.FTAStructure[childGuid].Name);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        treeView1.ExpandAll();
    }


    //(ParentGuid,Event type,Gatetype,freqvency,xPosition,yPosition,Tag)
    public void GenerateSampleTree2()
    {
        Guid A = EngineLogic.AddNewEvent(EngineLogic.TopEventGuid, "Intermediate event 1", 1, 2, 0, 100, 270, "IE1");
        Guid B = EngineLogic.AddNewEvent(EngineLogic.TopEventGuid, "Intermediate event 2", 1, 1, 0.039213, 400, 270, "IE2");

        Guid A_A = EngineLogic.AddNewEvent(A, "Base event 1", 2, 1, 0.005, 200, 450, "BE1");
        Guid A_B = EngineLogic.AddNewEvent(A, "Base event 2", 2, 1, 0.007472, 0, 450, "BE2");


        Guid B_A = EngineLogic.AddNewEvent(B, "Base event 3", 2, 2, 0.039211, 400, 450, "BE1");
        Guid B_B = EngineLogic.AddNewEvent(B, "Intermediate event 3", 1, 2, 2.9955e-6, 600, 450, "IE3");

        Guid B_B_A = EngineLogic.AddNewEvent(B_B, "Base event 4", 2, 1, 0.002996, 400, 650, "BE3");
        Guid B_B_B = EngineLogic.AddNewEvent(B_B, "Base event 5", 2, 2, 0.001, 600, 650, "BE4");
        Guid B_B_C = EngineLogic.AddNewEvent(B_B, "Intermediate event 4", 1, 2, 2.9955e-6, 800, 650, "IE3");

        Guid B_B_C_A = EngineLogic.AddNewEvent(B_B_C, "Base event 6", 2, 1, 0.005, 700, 850, "BE4");
        Guid B_B_C_B = EngineLogic.AddNewEvent(B_B_C, "Base event 7", 2, 1, 0.007472, 900, 850, "BE2");
    }

    public void GenerateSampleTree1()
    {
        Guid F = EngineLogic.AddNewEvent(EngineLogic.TopEventGuid, "Intermediate event 0", 1, 2, 0, 250, 270, "IE0");
        Guid G = EngineLogic.AddNewEvent(EngineLogic.TopEventGuid, "X", 2, 1, 0.005, 50, 270, "X");

        Guid A = EngineLogic.AddNewEvent(F, "Intermediate event 1", 1, 1, 0, 250, 570, "IE1");
        Guid B = EngineLogic.AddNewEvent(F, "Intermediate event 2", 1, 1, 0, 550, 570, "IE2");
        Guid C = EngineLogic.AddNewEvent(F, "B", 2, 1, 0.005, 50, 570, "B");

        Guid A_A = EngineLogic.AddNewEvent(A, "A", 2, 1, 0.005, 150, 750, "A");
        Guid A_B = EngineLogic.AddNewEvent(B, "D", 2, 1, 0.007472, 550, 750, "D");
        Guid B_A = EngineLogic.AddNewEvent(A, "Intermediate event 3", 1, 1, 0, 350, 750, "IE3");
        Guid B_B = EngineLogic.AddNewEvent(B, "Intermediate event 4", 1, 2, 0, 750, 750, "IE4");

        Guid B_B_A = EngineLogic.AddNewEvent(B_A, "B", 2, 1, 0.002996, 250, 950, "B");
        Guid B_B_B = EngineLogic.AddNewEvent(B_A, "C", 2, 2, 0.001, 450, 950, "C");
        Guid B_B_C = EngineLogic.AddNewEvent(B_B, "A", 2, 1, 0.005, 650, 950, "A");
        Guid B_B_D = EngineLogic.AddNewEvent(B_B, "C", 2, 1, 0.007472, 850, 950, "C");
    }
}
