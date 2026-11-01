using System.Drawing.Drawing2D;
using System.IO.Compression;

class UIEngine
{
    FTAlogic EngineLogic;
    public UIEngine(FTAlogic f)
    {
        EngineLogic = f;
    }
    public TreeNode TreeNodeMinimalCutSet;

    /// <summary>Section nodes at the top level of the tree view.</summary>
    public const string MainTreeNodeText = "Main Tree Structure";
    public const string McsNodeText = "Minimal Cut Sets";

    public void FillTreeView(TreeView treeView1)
    {
        treeView1.Nodes.Clear();

        TreeNode MainTree = treeView1.Nodes.Add(MainTreeNodeText);
        TreeNodeMinimalCutSet = treeView1.Nodes.Add(McsNodeText);
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
        StyleSectionNodes(treeView1);
    }

    private static readonly Color SectionNodeColor = Color.FromArgb(33, 52, 77);
    private static Font? _regularNodeFont;

    /// <summary>
    /// Section nodes (Main Tree Structure, Minimal Cut Sets) in bold and dark blue, all other nodes regular.
    /// The tree view itself gets the bold font, so that the native control measures the bold section
    /// names correctly (a bold NodeFont in a regular tree would be cut off); the other nodes get a
    /// regular NodeFont.
    /// </summary>
    private static void StyleSectionNodes(TreeView tree)
    {
        if (!tree.Font.Bold)
            tree.Font = new Font(tree.Font, FontStyle.Bold);
        _regularNodeFont ??= new Font(tree.Font, FontStyle.Regular);

        foreach (TreeNode section in tree.Nodes)
        {
            section.ForeColor = SectionNodeColor;
            SetRegularFont(section.Nodes, _regularNodeFont);
        }
    }

    private static void SetRegularFont(TreeNodeCollection nodes, Font font)
    {
        foreach (TreeNode node in nodes)
        {
            node.NodeFont = font;
            SetRegularFont(node.Nodes, font);
        }
    }

    /// <summary>
    /// Minimal cut sets in the tree view: top event -> MCS1..n -> events.
    /// MCS nodes are keyed by the cut set name (stable after regenerating MCS),
    /// event nodes by the GUID of the original event (selecting it selects the event in the diagram).
    /// </summary>
    public void FillMinimalCutSets(TreeView treeView1, TreeNode MCSnode)
    {
        if (EngineLogic.MinimalCutSets.Count == 0)
            return;

        FTAitem topItem = EngineLogic.GetItem(EngineLogic.TopEventGuid);
        TreeNode topNode = MCSnode.Nodes.Add(topItem?.Name ?? "Top Event");

        foreach (var mcs in EngineLogic.MinimalCutSets)
        {
            if (mcs.Events.Count == 0)
                continue;

            TreeNode mcsNode = topNode.Nodes.Add(mcs.Name, mcs.Name);
            foreach (var evt in mcs.Events)
                mcsNode.Nodes.Add(evt.GuidCode.ToString(), evt.Name);
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

    private readonly Font headerFont = new Font("Segoe UI Semibold", 8.25f);

    /// <summary>
    /// Title of a side panel in the style of current tool windows: light background, small
    /// upper-case grey text and a thin line below (instead of the former blue gradient band).
    /// </summary>
    public void CreateHeaderPanel(Panel p, String title)
    {
        string text = title.ToUpperInvariant();
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = TextRenderer.MeasureText("Ag", headerFont).Height + 12,
            BackColor = Color.FromArgb(245, 247, 250),
        };
        p.Controls.Add(headerPanel);
        headerPanel.SendToBack();

        headerPanel.Resize += (s, e) => headerPanel.Invalidate();
        headerPanel.Paint += (s, e) =>
        {
            Rectangle r = headerPanel.ClientRectangle;
            TextRenderer.DrawText(e.Graphics, text, headerFont, new Rectangle(r.X + 10, r.Y, r.Width - 12, r.Height - 1),
                Color.FromArgb(82, 92, 105), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            using var line = new Pen(Color.FromArgb(222, 226, 230));
            e.Graphics.DrawLine(line, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
        };
    }

    public string CreateArchive(string sourceDirectory, string extension, string additionalText = "")
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException("Adresár neexistuje.");

        if (!extension.StartsWith("."))
            extension = "." + extension;

        string folderName = new DirectoryInfo(sourceDirectory).Name;
        string date = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string zipFileName = $"{folderName}_autoarchive_{date}";

        if (!string.IsNullOrWhiteSpace(additionalText))
            zipFileName += $"_{additionalText}";

        string zipPath = Path.Combine(sourceDirectory, zipFileName + ".zip");

        var files = Directory
            .GetFiles(sourceDirectory, "*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f).Equals(extension, StringComparison.OrdinalIgnoreCase));

        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var file in files)
            {
                string entryName = Path.GetRelativePath(sourceDirectory, file);
                archive.CreateEntryFromFile(file, entryName);
            }
        }

        return zipPath;
    }
}
