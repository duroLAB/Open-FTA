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

    public void FillMinimalCutSets(TreeView treeView1, TreeNode MCSnode)
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
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
        grid.GridColor = Color.LightGray; // alebo iná decentná farba

        grid.AllowUserToResizeColumns = true;


    }


    public void MakeTabControlModern(TabControl tabControl)
    {
        // Ak je vnorený TabControl (napr. v inej TabPage), nechaj ho klasický
        if (tabControl.Parent is TabPage)
        {
            tabControl.Appearance = TabAppearance.Normal;
            tabControl.DrawMode = TabDrawMode.Normal;
            tabControl.BackColor = SystemColors.Control;
            return;
        }

        // --- Moderný vzhľad ---
        tabControl.Appearance = TabAppearance.Normal;
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.SizeMode = TabSizeMode.Fixed;
        tabControl.ItemSize = new Size(150, 28);
        tabControl.Padding = new Point(8, 3);
        tabControl.Multiline = false;
        tabControl.BackColor = Color.White;

        // odstránenie rámov a tieňovania TabPages
        foreach (TabPage page in tabControl.TabPages)
        {
            page.BackColor = tabControl.Parent.BackColor;
            page.BorderStyle = BorderStyle.None;
        }

        // Vlastné kreslenie tabov a pozadia
        tabControl.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Vyčisti pozadie - odstráni 3D rám a tieň
            g.Clear(tabControl.Parent.BackColor);

            // Spodná čiara pod tabmi (oddelenie)
            if (tabControl.TabCount > 0)
            {
                using (Pen line = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    g.DrawLine(line, 0, tabControl.GetTabRect(0).Bottom, tabControl.Width, tabControl.GetTabRect(0).Bottom);
                }
            }
        };

        // Vlastné kreslenie jednotlivých tabov
        tabControl.DrawItem -= ModernTabControl_DrawItem;
        tabControl.DrawItem += ModernTabControl_DrawItem;
    }

    public void ModernTabControl_DrawItem(object sender, DrawItemEventArgs e)
    {
        var tabControl = sender as TabControl;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        Rectangle rect = e.Bounds;
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

        // moderné farby tabov
        Color back = selected ? Color.FromArgb(235, 243, 255) : Color.FromArgb(250, 250, 250);
        Color border = selected ? Color.FromArgb(66, 133, 244) : Color.FromArgb(230, 230, 230);
        Color text = selected ? Color.FromArgb(25, 70, 150) : Color.FromArgb(100, 100, 100);

        using (Brush b = new SolidBrush(back))
            g.FillRectangle(b, rect);

        // spodná linka pre vybraný tab
        if (selected)
        {
            using (Pen p = new Pen(border, 2))
                g.DrawLine(p, rect.Left + 3, rect.Bottom - 2, rect.Right - 3, rect.Bottom - 2);
        }

        TextRenderer.DrawText(
            g,
            tabControl.TabPages[e.Index].Text,
            new Font("Segoe UI", 9f, FontStyle.Regular),
            rect,
            text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        );
    }

    public void MakeGroupBoxModern(GroupBox groupBox)
    {


        groupBox.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            string text = groupBox.Text;
            Font font = new Font("Segoe UI", 9F, FontStyle.Bold);
            SizeF textSize = g.MeasureString(text, font);

            int textPadding = 6;
            int lineY = (int)(textSize.Height / 2);

            // draw top line (split by text)
            using (Pen pen = new Pen(Color.FromArgb(180, 180, 180)))
            {
                g.DrawLine(pen, 0, lineY, textPadding, lineY);
                g.DrawLine(pen, (int)(textPadding + textSize.Width), lineY, groupBox.Width, lineY);

                // draw left, right and bottom lines
                g.DrawLine(pen, 0, lineY, 0, groupBox.Height); // left
                g.DrawLine(pen, groupBox.Width - 1, lineY, groupBox.Width - 1, groupBox.Height); // right
                g.DrawLine(pen, 0, groupBox.Height - 1, groupBox.Width, groupBox.Height - 1); // bottom
            }

            // fill only the background behind the text
            using (Brush backText = new SolidBrush(Color.FromArgb(180, 210, 250)))
            {
                g.FillRectangle(backText, textPadding, 0, textSize.Width, textSize.Height);
            }

            // draw text
            using (Brush textBrush = new SolidBrush(Color.FromArgb(25, 70, 150)))
            {
                g.DrawString(text, font, textBrush, textPadding, 0);
            }
        };
    }




}
