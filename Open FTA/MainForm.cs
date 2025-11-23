using Open_FTA;
using Open_FTA.forms;
using Open_FTA.Properties;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace OpenFTA
{
  

    public enum ExportType
    {
        Bitmap,
        Vector
    }

    public partial class MainForm : Form
    {
        FTAlogic EngineLogic;
        UIEngine MyUIEngine;
        // DrawingEngine EngineLogic.MyDrawingEngine;
        String WorkingDirectory = "";
        String WorkingFileName = "";

        CustomTreeView treeView1;

        private Stack<List<FTAitem>> undoStack = new Stack<List<FTAitem>>();
        private Stack<List<FTAitem>> redoStack = new Stack<List<FTAitem>>();



        public MainForm()
        {
            this.Icon = Resources.Main;

            InitializeComponent();
            EngineLogic = new FTAlogic();
            MyUIEngine = new UIEngine(EngineLogic);
            // EngineLogic.MyDrawingEngine = new DrawingEngine(EngineLogic, EngineLogic.FTAStructure);
            /* toolStripMenuItem_ALIGN.SelectedIndexChanged += toolStripMenuItem_SelectedIndexChanged;
             pictureBox1.Dock = DockStyle.Fill;
             pictureBox1.BackColor = Color.White;
             SQLiteConnection sqlite_conn;
             sqlite_conn = CreateConnection();*/
            Width = 1280;
            Height = 800;
            this.Text = "Open FTA";

            KeyPreview = true;

            treeView1 = new CustomTreeView();
            //treeView1.Parent = panelLeft;
            treeView1.Parent = panelLeft_Top;
            treeView1.Dock = DockStyle.Fill;
            // panelLeft.Controls.Add(treeView1);
            panelLeft_Top.Controls.Add(treeView1);
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.BeforeCollapse += TreeView1_BeforeCollapse;
            treeView1.BeforeExpand += TreeView1_BeforeExpand;

            MyUIEngine.CreateHeaderPanel(panelLeft_Top, "Tree Structure");

            MyUIEngine.CreateHeaderPanel(panelLeft_Bottom, "Working Directory");






            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(20, 5);
            tabControl1.Alignment = TabAlignment.Bottom;   // ⇦ záložky budú dole
            tabControl1.DrawItem += TabControl1_DrawItem;






            // Nastavenie ListView
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.HideSelection = false;
            // listView1.OwnerDraw = true;

            // Jeden stĺpec = názov súboru
            listView1.Columns.Clear();
            listView1.Columns.Add("Name", listView1.Width - 100); // automaticky roztiahne
            listView1.Columns.Add("Created", 100);



            listView1.Resize += (s, e) =>
            {
                int columnWidth = listView1.ClientSize.Width / listView1.Columns.Count;
                foreach (ColumnHeader column in listView1.Columns)
                    column.Width = columnWidth;
            };

            if (WorkingDirectory.Length < 1)
            {
                string exePath = Application.StartupPath;
                string examplesPath = Path.Combine(exePath, "Examples");

                if (Directory.Exists(examplesPath))
                {
                    WorkingDirectory = examplesPath;
                    LoadFiles(examplesPath);
                }
                else
                {
                    WorkingDirectory = exePath;
                }
            }


            UpdateMainFormControls();

        }








        private void TreeView1_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            var tree = (CustomTreeView)sender;

            if (HasAncestorWithName(e.Node, "Minimal cut sets")) return;


            var selectedItem = EngineLogic.GetItem(e.Node.Name);
            if (tree.IsUserAction)
            {
                HideSubtree(selectedItem, false);
                if (MainAppSettings.Instance.AutoSortTreeCollapseExpand)
                    EngineLogic.ArrangeTree();
                pictureBox1.Invalidate();
            }


        }

        private void TreeView1_BeforeCollapse(object? sender, TreeViewCancelEventArgs e)
        {

            var tree = (CustomTreeView)sender;

            if (HasAncestorWithName(e.Node, "Minimal cut sets")) return;

            var selectedItem = EngineLogic.GetItem(e.Node.Name);
            if (tree.IsUserAction)
            {
                HideSubtree(selectedItem, true);
                if (MainAppSettings.Instance.AutoSortTreeCollapseExpand)
                    EngineLogic.ArrangeTree();
                pictureBox1.Invalidate();
            }

        }

        bool HasAncestorWithName(TreeNode node, string name)
        {
            if (node == null) return (false);
            TreeNode current = node.Parent;

            while (current != null)
            {
                if (current.Text == name)
                    return true;

                current = current.Parent;
            }

            return false;
        }

        public void LoadAppSettings()
        {
            MainAppSettings.Instance.Load();
        }

        public void GetAppSetings()
        {
            Constants.EventVerticalSpacing = MainAppSettings.Instance.EventVerticalSpacing;
            Constants.EventHorizontalSpacing = MainAppSettings.Instance.EventHorizontalSpacing;
            Constants.EventWidth = MainAppSettings.Instance.EventWidth;
            Constants.EventHeight = MainAppSettings.Instance.EventHeight;
        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = (TabControl)sender;
            var g = e.Graphics;
            var rect = e.Bounds;
            bool selected = (e.Index == tab.SelectedIndex);

            // zisti, či sú tabs dole
            bool bottomTabs = (tab.Alignment == TabAlignment.Bottom);

            // farby
            Color backColor = selected ? Color.FromArgb(45, 120, 230) : Color.FromArgb(240, 240, 240);
            Color textColor = selected ? Color.White : Color.Black;

            // vyplnenie pozadia tab-u
            using (var brush = new SolidBrush(backColor))
                g.FillRectangle(brush, rect);

            // text v strede
            string text = tab.TabPages[e.Index].Text;
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var brush = new SolidBrush(textColor))
                g.DrawString(text, tab.Font, brush, rect, sf);

            // oddelovacia / zvýrazňovacia linka
            if (bottomTabs)
            {
                // ak je dole, zvýrazni hornú hranu
                using (var pen = new Pen(selected ? Color.FromArgb(45, 120, 230) : Color.LightGray, selected ? 3 : 1))
                    g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
            }
            else
            {
                // ak je hore, zvýrazni spodnú hranu
                using (var pen = new Pen(selected ? Color.FromArgb(45, 120, 230) : Color.LightGray, selected ? 3 : 1))
                    g.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MyUIEngine.FillTreeView(treeView1);
            EngineLogic.MyDrawingEngine.SetDimensions(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            EngineLogic.AssignLevelsToAllEvents();
        }

        #region picture box events
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Left:
                    EngineLogic.MyDrawingEngine.offsetX = EngineLogic.MyDrawingEngine.offsetX + 20;
                    break;
                case Keys.Right:
                    EngineLogic.MyDrawingEngine.offsetX = EngineLogic.MyDrawingEngine.offsetX - 20;
                    break;
                case Keys.Up:
                    EngineLogic.MyDrawingEngine.offsetY = EngineLogic.MyDrawingEngine.offsetY + 20;
                    break;
                case Keys.Down:
                    EngineLogic.MyDrawingEngine.offsetY = EngineLogic.MyDrawingEngine.offsetY - 20;
                    break;

            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                // iba ak je focus na PictureBox

                MessageBox.Show("Ctrl+C detected on PictureBox - Copying selected events.");


            }
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {

        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // If right mouse button click, show context menu
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip_Treeview.Show(pictureBox1, e.Location);
                var selectedEvent = EngineLogic.SelectedEvents.Count == 1 ? EngineLogic.SelectedEvents.First() : null;
                if (selectedEvent != null)
                {
                    if (selectedEvent.Children.Count > 0)

                        toolStripMenuItem_HIDEUNHIDE.Text = EngineLogic.GetItem(selectedEvent.Children[0]).IsHidden ? "Unhide" : "Hide";

                }
                contextMenuStrip_Treeview.Show(pictureBox1, e.Location);
                return;

            }

            // If left click add to selected event list
            if (e.Button == MouseButtons.Left)
            {
                MouseEventArgs me = (MouseEventArgs)e;
                Point coordinates = me.Location;
                toolStripMenuItem_ALIGN.Text = "Align";

                bool EventWasSelected = EngineLogic.MyDrawingEngine.Mouse_SelectEvent(coordinates);
                if (EventWasSelected)
                {
                    EngineLogic.MyDrawingEngine.SetLastDragPosition(me.Location);
                    pictureBox1.Cursor = Cursors.Cross;
                }
                else
                    pictureBox1.Cursor = Cursors.NoMove2D;
                pictureBox1.Invalidate();
            }

            UpdateMainFormControls();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            bool InvEVN = false;

            if (EngineLogic.MyDrawingEngine.SelectedEventDrag || EngineLogic.MyDrawingEngine.IsDraggingTree)
            {
                EngineLogic.MyDrawingEngine.Mouse_DragEvent(e.Location);
                pictureBox1.Invalidate();
            }
            else
            if (EngineLogic.MyDrawingEngine.Mouse_OnEvevt(e.Location, out InvEVN))
            {
                if (InvEVN)
                    pictureBox1.Invalidate();
                Cursor = Cursors.Hand;

            }
            else
            {
                if (InvEVN)
                    pictureBox1.Invalidate();
                Cursor = Cursors.Default;
            }

            /* int X = 0;
             int Y = 0;
             EngineLogic.MyDrawingEngine.PixelToRealPosition(e.Location, ref X, ref Y);
             String str = "X:" + X + ", Y:" + Y + ")";
             toolStripStatusLabelCoordinates.Text = str;*/
            toolStripStatusLabelCoordinates.Text = "";

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            EngineLogic.MyDrawingEngine.Mouse_Up();
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.Invalidate();
        }
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            EngineLogic.MyDrawingEngine.offsetX += (int)(EngineLogic.MyDrawingEngine.GlobalZoom * 0.05 * (0.5 * pictureBox1.Width - e.X));
            EngineLogic.MyDrawingEngine.offsetY += (int)(EngineLogic.MyDrawingEngine.GlobalZoom * 0.05 * (0.5 * pictureBox1.Height - e.Y));
            EngineLogic.MyDrawingEngine.GlobalZoom = EngineLogic.MyDrawingEngine.GlobalZoom * (1 + e.Delta * 0.0005);
            pictureBox1.Invalidate();
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            EngineLogic.MyDrawingEngine.DrawFTA(e.Graphics);
        }

        #endregion

        #region Controls
        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            /*SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            sfd.Title = "Save FTAStructure";
            sfd.FileName = "ftastructure.xml";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<FTAitem> list = EngineLogic.ConvertStructureToList();
                    XmlSerializer serializer = new XmlSerializer(typeof(List<FTAitem>));

                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        serializer.Serialize(fs, list);
                    }

                    MessageBox.Show("Structure saved successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving structure: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }*/

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "fta files (*.fta)|*.fta|All files (*.*)|*.*";
            sfd.Title = "Save FTAStructure";
            sfd.FileName = "ftastructure.fta";
            if (WorkingDirectory.Length > 0)
                sfd.InitialDirectory = WorkingDirectory;
            if (WorkingFileName.Length > 0)
                sfd.FileName = Path.GetFileName(WorkingFileName);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveTreeClass saveData = new SaveTreeClass
                    {
                        BuildVersion = "1.0.1.0",
                        Zoom = EngineLogic.MyDrawingEngine.GlobalZoom,
                        OffsetX = EngineLogic.MyDrawingEngine.offsetX,
                        OffsetY = EngineLogic.MyDrawingEngine.offsetY,
                        FtaStructure = EngineLogic.FTAStructure
                    };

                    saveData.SaveToFile(sfd.FileName);

                    MessageBox.Show("Structure saved successfully.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving structure: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "fta files (*.fta)|*.fta|All files (*.*)|*.*";
            ofd.Title = "Open FTAStructure";

            ofd.InitialDirectory = WorkingDirectory;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadFile(ofd.FileName);
            }
        }

        private bool LoadFile(String fullpath)
        {
            try
            {
                SaveTreeClass loadedData = SaveTreeClass.LoadFromFile(fullpath);

                EngineLogic.MyDrawingEngine.GlobalZoom = loadedData.Zoom;
                EngineLogic.MyDrawingEngine.offsetX = (int)loadedData.OffsetX;
                EngineLogic.MyDrawingEngine.offsetY = (int)loadedData.OffsetY;
                EngineLogic.FTAStructure.Clear();
                EngineLogic.FTAStructure = loadedData.FtaStructure;



                FTAitem root = EngineLogic.FTAStructure.Values.FirstOrDefault(item => item.Parent == Guid.Empty);
                if (root == null)
                {
                    root = EngineLogic.FTAStructure.Values.First();
                    root.Parent = Guid.Empty;
                }

                EngineLogic.TopEventGuid = root.GuidCode;
                EngineLogic.FindAllChilren();
                EngineLogic.AssignLevelsToAllEvents();
                MyUIEngine.FillTreeView(treeView1);
                EngineLogic.MyDrawingEngine.SetStructure(EngineLogic.FTAStructure);
                pictureBox1.Invalidate();

                WorkingFileName = fullpath;
                WorkingDirectory = Path.GetDirectoryName(fullpath);


                UpdateMainFormControls();
                return (true);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading FTAStructure: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (false);
            }
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            /*ExportDialogForm d = new ExportDialogForm();
            d.Text = "Copy Options";*/



            EngineLogic.CopySelectedEvents();
            // MyUIEngine.FillTreeView(treeView1);
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            SaveStateForUndo();
            EngineLogic.PasteCopiedEvents();
            if (MainAppSettings.Instance.AutoSortTreeCopyPaste)
            {
                EngineLogic.ArrangeTree();
            }
            // treeView1.ExpandAll();
            pictureBox1.Invalidate();
            MyUIEngine.FillTreeView(treeView1);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            SaveStateForUndo();
            EngineLogic.DeleteSelectedEvents();
            MyUIEngine.FillTreeView(treeView1);

            if (MainAppSettings.Instance.AutoSortTreeAddRemove)
                EngineLogic.ArrangeTree();

            pictureBox1.Invalidate();
        }

        private void SaveStateForUndo()
        {
            List<FTAitem> currentState = EngineLogic.ConvertStructureToList();

            undoStack.Push(currentState);
            redoStack.Clear();
        }

        private void toolStripButtonCenter_Click(object sender, EventArgs e)
        {
            Centertree();

        }

        private void toolStripButtonSort_Click(object sender, EventArgs e)
        {
            //         EngineLogic.MyDrawingEngine.ArrangeMainTreeHierarchically();


            //EngineLogic.ArrangeEventsAlgo1();         

            /* EngineLogic.FTAStructure = new Dictionary<Guid, FTAitem>(EngineLogic.MCSStructure);
             EngineLogic.FindAllChilren(EngineLogic.FTAStructure);*/

            if (EngineLogic.SelectedEvents.Count == 1)
                EngineLogic.MyDrawingEngine.TopEvent = EngineLogic.SelectedEvents[0];
            else
                EngineLogic.MyDrawingEngine.TopEvent = EngineLogic.GetItem(EngineLogic.TopEventGuid);




            EngineLogic.ArrangeTree();
            // EngineLogic.MyDrawingEngine.SetStructure(EngineLogic.FTAStructure); 
            pictureBox1.Invalidate();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            Undo();
            UpdateMainFormControls();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Redo();
            UpdateMainFormControls();
        }
        private void toolStripMenuItem_NEW_Click(object sender, EventArgs e)
        {
            OpenEditFormForEvent(true);
        }

        private void toolStripMenuItem_EDIT_Click(object sender, EventArgs e)
        {
            OpenEditFormForEvent(false);
        }

        private void toolStripButtonAddBasicEvent_Click(object sender, EventArgs e)
        {
            if (EngineLogic.SelectedEvents.Count == 1)
                CreateNewEvent(EngineLogic.SelectedEvents[0], true);
        }

        private void OpenEditFormForEvent(bool isNew)
        {

            int selectedCount = EngineLogic.SelectedEvents.Count;
            if (selectedCount == 0)
            {
                MessageBox.Show("No event selected!", "Error");
                return;
            }
            else if (selectedCount > 1)
            {
                MessageBox.Show("You can only edit one event at a time!", "Error");
                return;
            }
            var selectedEvent = EngineLogic.SelectedEvents[0];
            FormEditEvent edit = new FormEditEvent(EngineLogic);
            edit.EngineLogic = EngineLogic;

            if (isNew)
            {

                edit.textBoxName.Text = "New Item";
                edit.comboBoxGates.SelectedItem = Gates.OR;
                edit.comboBoxEventType.SelectedValue = 1;
                edit.textBoxFrequency.Text = "0";
                edit.textBoxTag.Text = "NewTag";
                edit.textBoxReference.Text = "";

            }
            else
            {

                edit.comboBoxGates.SelectedItem = selectedEvent.Gate;
                edit.comboBoxEventType.SelectedValue = selectedEvent.ItemType;
                edit.textBoxFrequency.Text = selectedEvent.Frequency.ToString();
                edit.textBoxDescription.Text = selectedEvent.Description;
                edit.textBoxName.Text = selectedEvent.Name;
                edit.comboBoxEventType.SelectedValue = selectedEvent.ItemType;
                edit.textBoxTag.Text = selectedEvent.Tag;
                edit.textBoxFrequency.Text = selectedEvent.Value.ToString();
                edit.comboBoxUnits.SelectedIndex = selectedEvent.ValueUnit;

                edit.comboBoxMetricType.SelectedIndex = (int)selectedEvent.ValueType;
                edit.Text = "Event:" + selectedEvent.Name + " (" + selectedEvent.Tag + ")";

                if (selectedEvent.Children.Count > 0)
                    edit.comboBoxEventType.Enabled = false;
                edit.textBoxReference.Text = selectedEvent.Reference;

            }




            if (DialogResult.OK == edit.ShowDialog())
            {
                if (isNew)
                {
                    Guid A = EngineLogic.AddNewEvent(selectedEvent.GuidCode, "", 0, 0, 0);
                    var newEvent = EngineLogic.GetItem(A);
                    ReadInfoFromEditForm(edit, newEvent);
                }
                else
                {
                    ReadInfoFromEditForm(edit, selectedEvent);
                }

                MyUIEngine.FillTreeView(treeView1);
                EngineLogic.AssignLevelsToAllEvents();
                pictureBox1.Invalidate();
            }
            UpdateMainFormControls();
            SaveStateForUndo();
        }

        void CreateNewEvent(FTAitem parent, bool Basis)
        {

            int selectedCount = EngineLogic.SelectedEvents.Count;
            if (selectedCount != 1)
                return;

            if (parent.ItemType > 1)
            {
                MessageBox.Show("Cannot add event to a basic event!", "Error");
                return;
            }

            SaveStateForUndo();


            int temp = Basis ? 2 : 1;
            Guid A = EngineLogic.AddNewEvent(parent.GuidCode, "", temp, 0, 0);
            var newEvent = EngineLogic.GetItem(A);


            //   newEvent.Name = "New Item";
            // newEvent.Tag = "NewTag";
            newEvent.Gate = Gates.OR;
            newEvent.Frequency = 0;
            newEvent.ValueType = ValueTypes.F;
            newEvent.ValueUnit = 0;
            newEvent.ValueUnit = 0;


            UpdateMainFormControls();
            MyUIEngine.FillTreeView(treeView1);

            if (MainAppSettings.Instance.AutoSortTreeAddRemove)
                EngineLogic.ArrangeTree();

            EngineLogic.AssignLevelsToAllEvents();
            pictureBox1.Invalidate();
        }

        private void toolStripButtonAddIntermediateEvent_Click(object sender, EventArgs e)
        {
            if (EngineLogic.SelectedEvents.Count == 1)
                CreateNewEvent(EngineLogic.SelectedEvents[0], false);
        }
        private void toolStripButtonEdit_Click(object sender, EventArgs e)
        {
            OpenEditFormForEvent(false);
        }

        #endregion

        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                List<FTAitem> currentState = EngineLogic.ConvertStructureToList();
                redoStack.Push(currentState);
                List<FTAitem> prevState = undoStack.Pop();

                EngineLogic.FTAStructure.Clear();
                foreach (FTAitem item in prevState)
                {
                    EngineLogic.FTAStructure[item.GuidCode] = item;
                }

                EngineLogic.FindAllChilren();
                EngineLogic.AssignLevelsToAllEvents();
                MyUIEngine.FillTreeView(treeView1);
                pictureBox1.Invalidate();
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                List<FTAitem> currentState = EngineLogic.ConvertStructureToList();
                undoStack.Push(currentState);
                List<FTAitem> nextState = redoStack.Pop();
                EngineLogic.FTAStructure.Clear();
                foreach (FTAitem item in nextState)
                {
                    EngineLogic.FTAStructure[item.GuidCode] = item;
                }

                EngineLogic.FindAllChilren();
                EngineLogic.AssignLevelsToAllEvents();
                MyUIEngine.FillTreeView(treeView1);
                pictureBox1.Invalidate();
            }
        }

        private void ReadInfoFromEditForm(FormEditEvent edit, FTAitem item)
        {

            item.Name = edit.textBoxName.Text;
            item.Tag = edit.textBoxTag.Text;
            //     item.Gate = Convert.ToInt32(edit.comboBoxGates.SelectedValue);

            item.Gate = (Gates)edit.comboBoxGates.SelectedItem;
            item.ItemType = Convert.ToInt32(edit.comboBoxEventType.SelectedValue);

            /* double inputFreq = Convert.ToDouble(edit.textBoxFrequency.Text);
             string selectedUnit = edit.comboBoxUnits.SelectedItem.ToString();*/

            //TODO: add comp
            //selectedEvent.Frequency = ConvertToYears(inputFreq, selectedUnit);

            item.Description = edit.textBoxDescription.Text;

            if (item.ItemType > 1)
            {
                item.Value = Convert.ToDouble(edit.textBoxFrequency.Text.ToString());
                //   item.UserMetricType = edit.comboBoxMetricType.SelectedIndex;
                item.ValueType = (ValueTypes)edit.comboBoxMetricType.SelectedIndex;



                item.ValueUnit = edit.comboBoxUnits.SelectedIndex;
            }
            item.Reference = edit.textBoxReference.Text;
        }

        private void HideSubtree(FTAitem node, bool hide)
        {
            if (node == null) return;
            foreach (var childGuid in node.Children)
            {
                if (EngineLogic.FTAStructure.TryGetValue(childGuid, out FTAitem child))
                {
                    child.IsHidden = hide;
                    HideSubtree(child, hide);
                }
            }
        }
        private void Centertree()
        {
            if (EngineLogic.FTAStructure.Count == 0)
                return;

            SaveStateForUndo();

            var topEvent = EngineLogic.FTAStructure.Values.FirstOrDefault(evt => evt.Parent == Guid.Empty);
            if (topEvent == null)
                return;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var evt in EngineLogic.FTAStructure.Values)
            {
                minX = Math.Min(minX, evt.X);
                minY = Math.Min(minY, evt.Y);
                maxX = Math.Max(maxX, evt.X + Constants.EventWidth);
                maxY = Math.Max(maxY, evt.Y + Constants.EventHeight);
            }

            int treeWidth = maxX - minX;
            int treeHeight = maxY - minY;

            float scaleX = (float)pictureBox1.Width / treeWidth;
            float scaleY = (float)pictureBox1.Height / treeHeight;
            float newZoom = Math.Min(scaleX, scaleY) * 0.8f;

            const float minZoom = 0.15f;
            const float maxZoom = 5.00f;
            newZoom = Math.Max(minZoom, Math.Min(maxZoom, newZoom));

            int offsetX = (int)((pictureBox1.Width - treeWidth * newZoom) / 2 - minX * newZoom);
            int offsetY = (int)((pictureBox1.Height - treeHeight * newZoom) / 2 - minY * newZoom);

            EngineLogic.MyDrawingEngine.GlobalZoom = newZoom;
            EngineLogic.MyDrawingEngine.offsetX = offsetX;
            EngineLogic.MyDrawingEngine.offsetY = offsetY;
            pictureBox1.Invalidate();
        }

        private void explicitToolStripMenuItem_Click(object sender, EventArgs e)
        {/*
            var A = new MCSEngine(EngineLogic);
            A.PerformMCS();

            A.FillTreeNode(MyUIEngine.TreeNodeMinimalCutSet);
            */



        }

        private void toolStripButtonrReport_Click(object sender, EventArgs e)
        {


            EngineLogic.GenerateHTMLreport();

            ReportForm r = new ReportForm();
            r.html = EngineLogic.html.ToString();
            r.Show();

        }

        private void freqvencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EngineLogic.PerformFullTest())
                return;

            String vysledok;
            var TopEvent = EngineLogic.GetItem(EngineLogic.TopEventGuid);
            EngineLogic.ComputeTree();
            pictureBox1.Invalidate();
            vysledok = "Top Event Failure Probability is= " + TopEvent.Frequency.ToString("0.000E0") + "\n" + " [1/h]";
            MessageBox.Show(vysledok, "The calculation completed succesfully");

            pictureBox1.Invalidate();
        }

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {

            FormSettings settingsForm = new FormSettings();

            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                GetAppSetings();
            }
        }

        private void toolStripButtonExportImage_Click(object sender, EventArgs e)
        {


            /*  var dialog = new OptionsDialog<ExportType>(
              "Export Options",
              new List<OptionItem<ExportType>>
              {
          new OptionItem<ExportType>
          {

              Title = "Bitmap",
              Description = "Raster image (.bmp/.png). Best for pixel graphics.",
              Icon = Resources.Copy_24,
              Value = ExportType.Bitmap
          },
          new OptionItem<ExportType>
          {
              Title = "Vector",
              Description = "Vector image (.emf/.wmf). Great for scaling.",
              Icon = Resources.Add_BE_24,
              Value = ExportType.Vector
          }
              }
          );


              dialog.ShowDialog();*/

            try
            {
                using (
                       var dialog = new OptionsDialog<ExportType>(
            "Export Options",
            new List<OptionItem<ExportType>>
            {
        new OptionItem<ExportType>
        {

            Title = "Bitmap",
            Description = "Raster image (.bmp/.png). Best for pixel graphics.",
            Icon = Resources.Copy_24,
            Value = ExportType.Bitmap
        },
        new OptionItem<ExportType>
        {
            Title = "Vector",
            Description = "Vector image (.emf/.wmf). Great for scaling.",
            Icon = Resources.Add_BE_24,
            Value = ExportType.Vector
        }
            }
        )

                    )
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        switch (dialog.SelectedValue)
                        {
                            case ExportType.Bitmap:
                                SaveToBMP();
                                break;
                            case ExportType.Vector:
                                SaveToVector();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                "An unknown error occurred while creating the image.\n\nDetails: " + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void SaveToVector()
        {
            double BackupGlobalZoom = EngineLogic.MyDrawingEngine.GlobalZoom;
            int BackupoffsetX = EngineLogic.MyDrawingEngine.offsetX;
            int BackupoffsetY = EngineLogic.MyDrawingEngine.offsetY;

            EngineLogic.MyDrawingEngine.GlobalZoom = 1;
            EngineLogic.MyDrawingEngine.offsetX = 0;
            EngineLogic.MyDrawingEngine.offsetY = 0;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "wmf Image|*.wmf";
            sfd.Title = "Save Screenshot";
            sfd.FileName = "FaultTreeimg.wmf";


            pictureBox1.Invalidate();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                EngineLogic.MyDrawingEngine.ExportToMeta(sfd.FileName);
            }




            EngineLogic.MyDrawingEngine.GlobalZoom = BackupGlobalZoom;
            EngineLogic.MyDrawingEngine.offsetX = BackupoffsetX;
            EngineLogic.MyDrawingEngine.offsetY = BackupoffsetY;
            pictureBox1.Invalidate();
        }
        private void SaveToBMP()
        {
            double BackupGlobalZoom = EngineLogic.MyDrawingEngine.GlobalZoom;
            int BackupoffsetX = EngineLogic.MyDrawingEngine.offsetX;
            int BackupoffsetY = EngineLogic.MyDrawingEngine.offsetY;

            EngineLogic.MyDrawingEngine.GlobalZoom = 1;
            EngineLogic.MyDrawingEngine.offsetX = 0;
            EngineLogic.MyDrawingEngine.offsetY = 0;


            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Choice1 Image|*.bmp";
            sfd.Title = "Save Screenshot";
            sfd.FileName = "FaultTreeimg.png";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format = ImageFormat.Png;
                switch (sfd.FilterIndex)
                {
                    case 2:
                        format = ImageFormat.Jpeg;
                        break;
                    case 3:
                        format = ImageFormat.Bmp;
                        break;
                    default:
                        format = ImageFormat.Png;
                        break;
                }

                //  bmp.Save(sfd.FileName, format);
                // MessageBox.Show("Screenshot saved successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                EngineLogic.MyDrawingEngine.ExportToBMP(sfd.FileName, format);

                EngineLogic.MyDrawingEngine.GlobalZoom = BackupGlobalZoom;
                EngineLogic.MyDrawingEngine.offsetX = BackupoffsetX;
                EngineLogic.MyDrawingEngine.offsetY = BackupoffsetY;
                pictureBox1.Invalidate();
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            EngineLogic = new FTAlogic();
            MyUIEngine = new UIEngine(EngineLogic);
            EngineLogic.MyDrawingEngine = new DrawingEngine(EngineLogic, EngineLogic.FTAStructure);


            MyUIEngine.FillTreeView(treeView1);
            EngineLogic.MyDrawingEngine.SetDimensions(pictureBox1.Width, pictureBox1.Height);

            EngineLogic.AssignLevelsToAllEvents();

            Invalidate();

        }

        private void UpdateMainFormControls()
        {
            toolStripButtonPaste.Enabled = false;
            toolStripButtonAddBasicEvent.Enabled = false;
            toolStripButtonAddIntermediateEvent.Enabled = false;
            toolStripButtonEdit.Enabled = false;
            toolStripButtonCopy.Enabled = false;
            toolStripButtonDelete.Enabled = false;
            toolStripMenuItemSELECTCHILDREN.Enabled = false;
            toolStripMenuItem_SELECTALLCHILDREN.Enabled = false;

            if (EngineLogic.SelectedEvents.Count > 0)
                UpdateMainFormControls_EventsAreSelected();


            toolStripMenuItem_PASTE.Enabled = toolStripButtonPaste.Enabled;
            toolStripMenuItem_COPY.Enabled = toolStripButtonCopy.Enabled;
            toolStripMenuItem_DELETE.Enabled = toolStripButtonDelete.Enabled;
            toolStripMenuItem_EDIT.Enabled = toolStripButtonEdit.Enabled;


            if (undoStack.Count > 0)
                toolStripButtonUndo.Enabled = true;
            else
                toolStripButtonUndo.Enabled = false;

            if (redoStack.Count > 0)
                toolStripButtonRedo.Enabled = true;
            else
                toolStripButtonRedo.Enabled = false;


            toolStripStatusLabelFileName.Text = " |  File name: " + Path.GetFileName(WorkingFileName);
            toolStripStatusLabelWorkingDir.Text = " Working directory: " + WorkingDirectory;


            if (WorkingFileName.Length > 0)
            {
                Text = "Open FTA - " + Path.GetFileName(WorkingFileName);
            }
            else
                Text = "Open FTA - (unsaved)";
        }

        private void UpdateMainFormControls_EventsAreSelected()
        {
            bool OneEventIsSelected = EngineLogic.SelectedEvents.Count == 1;

            toolStripButtonCopy.Enabled = true;
            toolStripButtonDelete.Enabled = true;


            if (OneEventIsSelected)
            {
                toolStripButtonPaste.Enabled = true;
                toolStripButtonAddBasicEvent.Enabled = true;
                toolStripButtonAddIntermediateEvent.Enabled = true;
                toolStripButtonEdit.Enabled = true;

                toolStripMenuItem_DELETE.Enabled = true;
                toolStripMenuItem_EDIT.Enabled = true;

                toolStripMenuItemSELECTCHILDREN.Enabled = true;
                toolStripMenuItem_SELECTALLCHILDREN.Enabled = true;
            }
            /*else  ///MultiSelection
            {
                toolStripButtonCopy.Enabled = true;
                toolStripButtonDelete.Enabled = true;                
            }*/
        }

        private void toolStripMenuItemSELECTCHILDREN_Click(object sender, EventArgs e)
        {
            if (EngineLogic.SelectedEvents.Count == 1)
            {
                EngineLogic.SelectChildren(EngineLogic.SelectedEvents[0], false);
            }
            pictureBox1.Invalidate();
        }

        private void toolStripMenuItem_SELECTALLCHILDREN_Click(object sender, EventArgs e)
        {
            if (EngineLogic.SelectedEvents.Count == 1)
            {
                EngineLogic.SelectChildren(EngineLogic.SelectedEvents[0], true);
            }

            /* for(int i=0;i< EngineLogic.SelectedEvents.Count; i++)
             {
                 EngineLogic.DisplayStructure.Add(EngineLogic.SelectedEvents[i].GuidCode,EngineLogic.GetItem(EngineLogic.SelectedEvents[i].GuidCode));
             }
             EngineLogic.MyDrawingEngine.SetStructure(EngineLogic.DisplayStructure);*/


            pictureBox1.Invalidate();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;

            //  ShiftDown for multiselect
            bool shiftDown = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            // Get Selected Item
            var selectedItem = EngineLogic.GetItem(treeView1.SelectedNode.Name);
            if (selectedItem != null)
            {
                if (shiftDown)
                {
                    if (selectedItem.IsSelected)
                    {
                        selectedItem.IsSelected = false;
                        EngineLogic.SelectedEvents.Remove(selectedItem);
                    }
                    else
                    {
                        selectedItem.IsSelected = true;
                        EngineLogic.SelectedEvents.Add(selectedItem);
                    }
                }
                else
                {
                    foreach (var evt in EngineLogic.FTAStructure.Values)
                        evt.IsSelected = false;
                    EngineLogic.SelectedEvents.Clear();

                    selectedItem.IsSelected = true;
                    EngineLogic.SelectedEvents.Add(selectedItem);
                }
            }

            EngineLogic.HighlightedMCS = "";
            ////Minimal cutset test
            EngineLogic.HighlightedEvents.Clear();
            var selectedMSCItem = EngineLogic.GetItem(treeView1.SelectedNode.Name, EngineLogic.MCSStructure);
            if (selectedMSCItem != null)
            {
                if (selectedMSCItem.ItemType == 1)
                {
                    EngineLogic.HighlightedMCS = selectedMSCItem.Name;
                    for (int i = 0; i < selectedMSCItem.Children.Count; i++)
                    {
                        var child = EngineLogic.GetItem(selectedMSCItem.Children[i], EngineLogic.MCSStructure);

                        if (child != null)
                            EngineLogic.HighlightedEvents.Add(child);

                    }
                }
            }






            pictureBox1.Invalidate();
        }

        private void toolStripMenuItem_HIDEUNHIDE_Click(object sender, EventArgs e)
        {
            /* if (EngineLogic.SelectedEvents.Count == 1)
             {
                 var selectedEvent = EngineLogic.SelectedEvents.First();
                 // selectedEvent.IsHidden = !selectedEvent.IsHidden;
                 //HideSubtree(selectedEvent, selectedEvent.IsHidden);
                 HideSubtree(selectedEvent, true);
                 ((ToolStripMenuItem)sender).Text = selectedEvent.IsHidden ? "Unhide" : "Hide";

                 pictureBox1.Invalidate();
             }*/

            if (EngineLogic.SelectedEvents.Count == 1)
            {
                var selectedEvent = EngineLogic.SelectedEvents.First();
                if (selectedEvent.Children.Count > 0)
                {
                    HideSubtree(selectedEvent, !EngineLogic.GetItem(selectedEvent.Children[0]).IsHidden);
                    ((ToolStripMenuItem)sender).Text = EngineLogic.GetItem(selectedEvent.Children[0]).IsHidden ? "Unhide" : "Hide";

                    if (EngineLogic.GetItem(selectedEvent.Children[0]).IsHidden)
                    {
                        HideSubtree(selectedEvent, true);
                        ((ToolStripMenuItem)sender).Text = "Hide";

                        treeView1.CollapseNodeByText(selectedEvent.GuidCode.ToString());
                    }
                    else
                    {
                        HideSubtree(selectedEvent, false);
                        ((ToolStripMenuItem)sender).Text = "Unhide";
                        treeView1.ExpandNodeByText(selectedEvent.GuidCode.ToString());
                    }

                    if (MainAppSettings.Instance.AutoSortTreeCollapseExpand)
                        EngineLogic.ArrangeTree();

                }



                pictureBox1.Invalidate();
            }
        }

        private void minimalCutSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EngineLogic.PerformFullTest())
                return;


            string equation = "";
            string simplifiedEquation = "";

            EngineLogic.GenerateMCS(out equation, out simplifiedEquation);
            if (EngineLogic.MCSStructure != null && EngineLogic.MCSStructure.Count > 0)
            {
                string message = "Generated Tree Expression:\n" + equation + Environment.NewLine +
                        "\nSimplified Expression:\n" + simplifiedEquation;

                textBoxMCSExpr.Text = message;
            }

            FTAitem TopEvent = null;
            foreach (var item in EngineLogic.MCSStructure)
            {
                if (item.Value.Parent == Guid.Empty) TopEvent = item.Value;
            }

            EngineLogic.SumChildren(TopEvent, EngineLogic.MCSStructure);
            List<FTAitem> l = EngineLogic.GenerateListOfBasicEvents();

            dataGridViewMCSResults.Rows.Clear();
            dataGridViewMCSResults.Columns.Clear();
            foreach (FTAitem item in l)
            {
                dataGridViewMCSResults.Columns.Add(item.Tag, item.Tag);
            }
            dataGridViewMCSResults.Columns.Add("Frequency", "Frequency");

            int rowIndex = 0;
            int MaxColums = 0;

            foreach (var item in EngineLogic.MCSStructure)
            {

                if (item.Value.Children.Count > 0 && item.Value.Parent != Guid.Empty)
                {
                    dataGridViewMCSResults.Rows.Add(item.Value.Name);
                    dataGridViewMCSResults.Rows[rowIndex].Cells[1].Value = item.Value.Value;

                    for (int i = 0; i < item.Value.Children.Count; i++)
                    {
                        if (i + 3 > dataGridViewMCSResults.ColumnCount)
                            dataGridViewMCSResults.Columns.Add("", "");

                        FTAitem fTAitem = EngineLogic.GetItem(item.Value.Children[i], EngineLogic.MCSStructure);
                        dataGridViewMCSResults.Rows[rowIndex].Cells[i + 2].Value = fTAitem.Name;



                        if (i > MaxColums)
                        {
                            MaxColums = i;
                        }
                    }
                    rowIndex += 1;
                }
            }
            MaxColums += 3; // plus frequency and name columns
            for (int i = dataGridViewMCSResults.Columns.Count - 1; i >= MaxColums; i--)
            {
                dataGridViewMCSResults.Columns.RemoveAt(i);

            }
            dataGridViewMCSResults.ColumnHeadersVisible = false;
            dataGridViewMCSResults.RowHeadersVisible = false;



            MyUIEngine.SetupModernGrid(dataGridViewMCSResults);
            dataGridViewMCSResults.Columns[0].Width = 80;  // prvý stĺpec
            dataGridViewMCSResults.Columns[1].Width = 120; // druhý stĺpec

            var column = dataGridViewMCSResults.Columns[1];
            dataGridViewMCSResults.Sort(column, System.ComponentModel.ListSortDirection.Descending);

            MyUIEngine.FillTreeView(treeView1);

            tabControl1.SelectedTab = tabPage2;


        }

        private void SaveGridToCsv(DataGridView grid)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export DataGridView";
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.FileName = "export.csv";
                sfd.OverwritePrompt = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportToCsv(grid, sfd.FileName, ';');

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Pri ukladaní došlo k chybe:\n" + ex.Message,
                            "Chyba exportu",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        public static void ExportToCsv(DataGridView grid, string filePath, char separator = ';')
        {
            var sb = new StringBuilder();

            // 🟦 Hlavičky
            var headers = grid.Columns
                .Cast<DataGridViewColumn>()
                .Select(c => Quote(c.HeaderText, separator));
            sb.AppendLine(string.Join(separator, headers));

            // 🟩 Riadky
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue; // preskočí prázdny riadok na konci

                var cells = row.Cells
                    .Cast<DataGridViewCell>()
                    .Select(c => Quote(c.Value?.ToString() ?? "", separator));

                sb.AppendLine(string.Join(separator, cells));
            }

            // 🟨 Uloženie do súboru (UTF-8)
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Quote(string value, char separator)
        {
            if (value.Contains(separator) || value.Contains('"') || value.Contains('\n'))
            {
                value = value.Replace("\"", "\"\""); // zdvojíme úvodzovky
                return $"\"{value}\"";
            }
            return value;
        }

        private void toolStripButtonExportToCSV_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2)
            {
                SaveGridToCsv(dataGridViewMCSResults);

            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                SaveGridToCsv(dataGridViewImportanceMeasureResults);
            }

        }

        private void importanceMeasureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EngineLogic.PerformFullTest())
                return;

            DataTable dt = new DataTable();
            dt.Columns.Add("Event Name", typeof(string));
            dt.Columns.Add("Event GUID", typeof(string));
            dt.Columns.Add("Birnbaum importance measure (BIM)", typeof(double));
            dt.Columns.Add("Critical importance measure (CIM)", typeof(double));
            dt.Columns.Add("Reliability achievement worth (RAW)", typeof(double));
            dt.Columns.Add("Reliability reduction worth (RRW)", typeof(double));
            dt.Columns.Add("Fussell-Vessely importance (FV)", typeof(double));

            foreach (FTAitem evt in EngineLogic.FTAStructure.Values)
            {
                if (evt.ItemType >= 2)
                {
                    double bimValue = Math.Round(EngineLogic.ComputeBIM(evt), 8);
                    double cimValue = Math.Round(EngineLogic.ComputeCIM(evt), 8);
                    double rawValue = Math.Round(EngineLogic.ComputeRAW(evt), 8);
                    double rrwValue = Math.Round(EngineLogic.ComputeRRW(evt), 8);
                    double fvValue = Math.Round(EngineLogic.ComputeFV(evt), 8);

                    DataRow row = dt.NewRow();
                    row["Event Name"] = evt.Name;
                    row["Event GUID"] = evt.GuidCode.ToString();
                    row["Birnbaum importance measure (BIM)"] = bimValue;
                    row["Critical importance measure (CIM)"] = cimValue;
                    row["Reliability achievement worth (RAW)"] = rawValue;
                    row["Reliability reduction worth (RRW)"] = rrwValue;
                    row["Fussell-Vessely importance (FV)"] = fvValue;
                    dt.Rows.Add(row);
                }
            }

            DataView dv = dt.DefaultView;
            dv.Sort = "Birnbaum importance measure (BIM) DESC";
            DataTable sortedDt = dv.ToTable();

            Form tableForm = new Form();
            tableForm.Text = "Importance Measures Overview";
            tableForm.Size = new Size(700, 400);


            dataGridViewImportanceMeasureResults.Dock = DockStyle.Fill;
            dataGridViewImportanceMeasureResults.DataSource = sortedDt;

            MyUIEngine.SetupModernGrid(dataGridViewImportanceMeasureResults);

            tabControl1.SelectedTab = tabPage3;

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2)
            {
                toolStrip3.Parent = tabPage2;
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                toolStrip3.Parent = tabPage3;
            }
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            newFile();
        }

        private void newFile()
        {
            undoStack.Clear();
            redoStack.Clear();

            EngineLogic = new FTAlogic();

            EngineLogic.FTAStructure.Clear();
            EngineLogic.SelectedEvents.Clear();
            EngineLogic.CopiedEvents.Clear();
            EngineLogic.CreateNewTopEvent();
            EngineLogic.MyDrawingEngine.GlobalZoom = 1;
            EngineLogic.MyDrawingEngine.offsetX = 0;
            EngineLogic.MyDrawingEngine.offsetY = 0;
            pictureBox1.Invalidate();
            MyUIEngine = new UIEngine(EngineLogic);
            MyUIEngine.FillTreeView(treeView1);

            WorkingFileName = "";

            UpdateMainFormControls();

        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            FormDbViewer dbViewer = new FormDbViewer(EngineLogic);
            dbViewer.panelButtons.Visible = false;
            dbViewer.ShowDialog();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            Point mousePos = pictureBox1.PointToClient(MousePosition);
            if (pictureBox1.ClientRectangle.Contains(mousePos))
            {
                if (e.KeyCode == Keys.Delete)
                {
                    toolStripButtonDelete_Click(this, null);
                    // e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    toolStripButtonCopy_Click(this, e);
                    //e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.V)
                {
                    toolStripButtonPaste_Click(this, e);
                    // e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.A)
                {
                    toolStripMenuItem_SELECTALLCHILDREN_Click(this, e);
                    // e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.Z)
                {
                    Undo();
                }
                else if (e.Control && e.KeyCode == Keys.Y)
                {
                    Redo();
                }
            }
        }

        private void toolStripButtonSelectWorkingDirectory_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select project directory";
                dialog.ShowNewFolderButton = true; // povoliť vytváranie nového adresára

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    WorkingDirectory = dialog.SelectedPath;
                    LoadFiles(WorkingDirectory);
                }
            }
        }
        private void LoadFiles(string folder)
        {
            listView1.Items.Clear();
            foreach (string file in Directory.GetFiles(folder))
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Extension != ".fta")
                    continue;

                var item = new ListViewItem(fi.Name);
                //item.SubItems.Add((fi.Length / 1024) + " KB");
                item.SubItems.Add((fi.CreationTime.ToShortDateString()));
                listView1.Items.Add(item);
            }
        }

        private void toolStripButtonCreateWorkingDirectory_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select parent folder";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string parentPath = dialog.SelectedPath;

                    // Požiadať používateľa o názov nového adresára
                    string newFolderName = Microsoft.VisualBasic.Interaction.InputBox(
                        "Enter name of the new folder:", "New Folder", "MyNewFolder");

                    if (!string.IsNullOrWhiteSpace(newFolderName))
                    {
                        string newFolderPath = Path.Combine(parentPath, newFolderName);

                        if (!Directory.Exists(newFolderPath))
                        {
                            Directory.CreateDirectory(newFolderPath);
                            WorkingDirectory = newFolderPath;
                            LoadFiles(WorkingDirectory);
                            MessageBox.Show("Created project folder: " + newFolderPath);
                        }
                        else
                        {
                            MessageBox.Show("Folder already exists");
                        }
                    }
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            ListViewItem item = listView1.SelectedItems[0];
            string fileName = item.Text; // názov súboru

            string fullPath = Path.Combine(WorkingDirectory, fileName);
            if (File.Exists(fullPath))
            {
                LoadFile(fullPath);
            }
        }

        private void toolStripButtonRunAnalysis_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2)
            {
                
                minimalCutSetToolStripMenuItem_Click(this, e);

            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                importanceMeasureToolStripMenuItem_Click(this, e);
            }
        }
    }
}
