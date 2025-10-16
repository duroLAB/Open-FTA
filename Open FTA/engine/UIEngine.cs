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


   
   
}
