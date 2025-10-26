using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


class UIEngine
{
    FTAlogic EngineLogic;
    public UIEngine(FTAlogic f)
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

            FillMinimalCutSets(treeView1, TreeNodeMinimalCutSet);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        treeView1.ExpandAll();
    }

    public void FillMinimalCutSets(TreeView treeView1,TreeNode MCSnode)
    {
        foreach (var evt in EngineLogic.MCSStructure.Values)
        {
            foreach (var childGuid in EngineLogic.MCSStructure[evt.GuidCode].Children)
            {
                TreeNode parentNode = treeView1.Nodes.Find(evt.GuidCode.ToString(), true).FirstOrDefault();

                if (parentNode == null)
                {
                    TreeNode newParentNode = new TreeNode(EngineLogic.MCSStructure[evt.GuidCode].Name)
                    {
                        Name = evt.GuidCode.ToString()
                    };
                    TreeNode grandParentNode = treeView1.Nodes.Find(EngineLogic.MCSStructure[evt.GuidCode].Parent.ToString(), true).FirstOrDefault();
                    if (grandParentNode != null)
                        grandParentNode.Nodes.Add(newParentNode);
                    else
                        MCSnode.Nodes.Add(newParentNode);

                    parentNode = newParentNode;
                }
               parentNode.Nodes.Add(childGuid.ToString(), EngineLogic.MCSStructure[childGuid].Name);
            }
        }
    }

    public void SetupModernGrid(DataGridView grid)
    {
        // 🟩 Zakázať editovanie a zmenu štruktúry
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.AllowUserToResizeColumns = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;

        // 🟦 Farby a štýly
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Color.LightGray;

        // 🟨 Hlavičky stĺpcov
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        grid.ColumnHeadersHeight = 32;
        grid.EnableHeadersVisualStyles = false; // bez tohto by systém prepísal farby

        // 🟪 Riadky
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.Black;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);

        // 🟧 Alternujúce farby riadkov
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
        grid.GridColor = Color.LightGray; // alebo iná decentná farba

        grid.AllowUserToResizeColumns = true;

        

    }


}
