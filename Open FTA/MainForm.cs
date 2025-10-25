using Open_FTA.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace OpenFTA
{
    public partial class MainForm : Form
    {
        FTAlogic EngineLogic;
        FTAlogic EngineLogicMCS;
        UIlogic UIEngine;
        DrawingEngine MyDrawingEngine;


        private Stack<List<FTAitem>> undoStack = new Stack<List<FTAitem>>();
        private Stack<List<FTAitem>> redoStack = new Stack<List<FTAitem>>();

        public MainForm()
        {
            InitializeComponent();
            EngineLogic = new FTAlogic();
            EngineLogicMCS = new FTAlogic();
            UIEngine = new UIlogic(EngineLogic);
            MyDrawingEngine = new DrawingEngine(EngineLogic, EngineLogic.FTAStructure);
            /* toolStripMenuItem_ALIGN.SelectedIndexChanged += toolStripMenuItem_SelectedIndexChanged;
             pictureBox1.Dock = DockStyle.Fill;
             pictureBox1.BackColor = Color.White;
             SQLiteConnection sqlite_conn;
             sqlite_conn = CreateConnection();*/
            Width = 1280;
            Height = 800;
            this.Text = "Open FTA";

            /*
                        ToolStripControlHost host = new ToolStripControlHost(tableLayoutPanel2);
                        host.AutoSize = false;
                        host.Size = new Size(50, 50);
                        toolStrip1.Items.Insert(8, host);*/


            MainAppSettings.Instance.Load();

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(20, 5);
            tabControl1.Alignment = TabAlignment.Bottom;   // ⇦ záložky budú dole
            tabControl1.DrawItem += TabControl1_DrawItem;


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
            UIEngine.FillTreeView(treeView1);
            MyDrawingEngine.SetDimensions(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            EngineLogic.AssignLevelsToAllEvents();
        }

        #region picture box events
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Left:
                    MyDrawingEngine.offsetX = MyDrawingEngine.offsetX + 20;
                    break;
                case Keys.Right:
                    MyDrawingEngine.offsetX = MyDrawingEngine.offsetX - 20;
                    break;
                case Keys.Up:
                    MyDrawingEngine.offsetY = MyDrawingEngine.offsetY + 20;
                    break;
                case Keys.Down:
                    MyDrawingEngine.offsetY = MyDrawingEngine.offsetY - 20;
                    break;

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
                    toolStripMenuItem_HIDEUNHIDE.Text = selectedEvent.IsHidden ? "Unhide" : "Hide";

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

                bool EventWasSelected = MyDrawingEngine.Mouse_SelectEvent(coordinates);
                if (EventWasSelected)
                {
                    MyDrawingEngine.SetLastDragPosition(me.Location);
                }
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox1.Invalidate();
            }

            UpdateMainFormControls();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyDrawingEngine.SelectedEventDrag || MyDrawingEngine.IsDraggingTree)
            {
                MyDrawingEngine.Mouse_DragEvent(e.Location);
                pictureBox1.Invalidate();
            }

            int X = 0;
            int Y = 0;
            MyDrawingEngine.PixelToRealPosition(e.Location, ref X, ref Y);
            String str = "X:" + X + ", Y:" + Y + ")";
            toolStripStatusLabelCoordinates.Text = str;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MyDrawingEngine.Mouse_Up();
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.Invalidate();
        }
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            MyDrawingEngine.offsetX += (int)(MyDrawingEngine.GlobalZoom * 0.05 * (0.5 * pictureBox1.Width - e.X));
            MyDrawingEngine.offsetY += (int)(MyDrawingEngine.GlobalZoom * 0.05 * (0.5 * pictureBox1.Height - e.Y));
            MyDrawingEngine.GlobalZoom = MyDrawingEngine.GlobalZoom * (1 + e.Delta * 0.0005);
            pictureBox1.Invalidate();
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            MyDrawingEngine.DrawFTA(e.Graphics);
        }

        #endregion

        #region Controls
        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
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
            }
        }

        private void toolStripButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            ofd.Title = "Open FTAStructure";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<FTAitem>));
                    List<FTAitem> loadedList;
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        loadedList = (List<FTAitem>)serializer.Deserialize(fs);
                    }

                    EngineLogic.FTAStructure.Clear();
                    foreach (FTAitem item in loadedList)
                    {
                        EngineLogic.FTAStructure[item.GuidCode] = item;
                    }

                    FTAitem topEvent = loadedList.FirstOrDefault(x => x.Parent == Guid.Empty);
                    if (topEvent != null)
                    {
                        EngineLogic.TopEventGuid = topEvent.GuidCode;
                    }
                    else
                    {
                        MessageBox.Show("Top event not found in loaded data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    EngineLogic.FindAllChilren();
                    EngineLogic.AssignLevelsToAllEvents();
                    UIEngine.FillTreeView(treeView1);
                    pictureBox1.Invalidate();

                    // MessageBox.Show("FTAStructure loaded successfully.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading FTAStructure: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            EngineLogic.CopySelectedEvents();
            UIEngine.FillTreeView(treeView1);
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            EngineLogic.PasteCopiedEvents();
            if (MainAppSettings.Instance.AutoSortTree)
            {
                MyDrawingEngine.ArrangeMainTreeHierarchically();
            }
            treeView1.ExpandAll();
            pictureBox1.Invalidate();
            UIEngine.FillTreeView(treeView1);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            SaveStateForUndo();
            EngineLogic.DeleteSelectedEvents();
            UIEngine.FillTreeView(treeView1);
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
            MyDrawingEngine.ArrangeMainTreeHierarchically();

            pictureBox1.Invalidate();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Redo();
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

                UIEngine.FillTreeView(treeView1);
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


            newEvent.Name = "New Item";
            // newEvent.Tag = "NewTag";
            newEvent.Gate = Gates.OR;
            newEvent.Frequency = 0;
            newEvent.ValueType = ValueTypes.F;
            newEvent.ValueUnit = 0;
            newEvent.ValueUnit = 0;


            UpdateMainFormControls();
            UIEngine.FillTreeView(treeView1);
            EngineLogic.AssignLevelsToAllEvents();
            pictureBox1.Invalidate();
        }

        private void toolStripButtonAddIntermediateEvent_Click(object sender, EventArgs e)
        {
            CreateNewEvent(EngineLogic.SelectedEvents[0], false);
        }
        private void toolStripButtonEdit_Click(object sender, EventArgs e)
        {
            OpenEditFormForEvent(false);
        }

        #endregion

        /*     public void ArrangeMainTreeHierarchically()
             {
                 FTAitem topEvent = EngineLogic.FTAStructure.Values.FirstOrDefault(e => e.Parent == Guid.Empty);
                 if (topEvent == null)
                     return;

                 // Set top event position
                 topEvent.X = pictureBox1.Width / 2;
                 topEvent.Y = 50;

                 ArrangeChildren(topEvent);

                 pictureBox1.Invalidate();
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
                     if (EngineLogic.FTAStructure.TryGetValue(childGuid, out FTAitem child))
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
                     if (EngineLogic.FTAStructure.TryGetValue(childGuid, out FTAitem child))
                     {
                         totalWidth += ComputeSubtreeWidth(child, gap) + gap;
                     }
                 }
                 totalWidth -= gap;
                 return Math.Max(totalWidth, Constants.EventWidth);
             }*/

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
                UIEngine.FillTreeView(treeView1);
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
                UIEngine.FillTreeView(treeView1);
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
        }

        private double ConvertToYears(double freq, string unit)
        {
            switch (unit)
            {
                case "h⁻¹":
                    return freq * 24 * 365;
                case "s⁻¹":
                    return freq * 60 * 60 * 24 * 365;
                case "y⁻¹":
                default:
                    return freq;
            }
        }

        private void HideSubtree(FTAitem node, bool hide)
        {
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

            MyDrawingEngine.GlobalZoom = newZoom;
            MyDrawingEngine.offsetX = offsetX;
            MyDrawingEngine.offsetY = offsetY;
            pictureBox1.Invalidate();
        }

        private void explicitToolStripMenuItem_Click(object sender, EventArgs e)
        {/*
            var A = new MCSEngine(EngineLogic);
            A.PerformMCS();

            A.FillTreeNode(UIEngine.TreeNodeMinimalCutSet);
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
            /*FormSettings settingsForm = new FormSettings();
            settingsForm.ShowDialog();*/





        }

        private void toolStripButtonExportImage_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new ExportDialogForm())
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        switch (dlg.SelectedOption)
                        {
                            case ExportDialogForm.ExportOption.Bitmap:
                                SaveToBMP();
                                break;
                            case ExportDialogForm.ExportOption.Metafile:
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
                MessageBoxIcon.Error
            );
            }
        }

        private void SaveToVector()
        {
            double BackupGlobalZoom = MyDrawingEngine.GlobalZoom;
            int BackupoffsetX = MyDrawingEngine.offsetX;
            int BackupoffsetY = MyDrawingEngine.offsetY;

            MyDrawingEngine.GlobalZoom = 1;
            MyDrawingEngine.offsetX = 0;
            MyDrawingEngine.offsetY = 0;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "wmf Image|*.wmf";
            sfd.Title = "Save Screenshot";
            sfd.FileName = "FaultTreeimg.wmf";


            pictureBox1.Invalidate();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MyDrawingEngine.ExportToMeta(sfd.FileName);
            }




            MyDrawingEngine.GlobalZoom = BackupGlobalZoom;
            MyDrawingEngine.offsetX = BackupoffsetX;
            MyDrawingEngine.offsetY = BackupoffsetY;
            pictureBox1.Invalidate();
        }
        private void SaveToBMP()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
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

                bmp.Save(sfd.FileName, format);
                // MessageBox.Show("Screenshot saved successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            EngineLogic = new FTAlogic();
            UIEngine = new UIlogic(EngineLogic);
            MyDrawingEngine = new DrawingEngine(EngineLogic, EngineLogic.FTAStructure);


            UIEngine.FillTreeView(treeView1);
            MyDrawingEngine.SetDimensions(pictureBox1.Width, pictureBox1.Height);

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


            if (EngineLogic.SelectedEvents.Count > 0)
                UpdateMainFormControls_EventsAreSelected();


            toolStripMenuItem_PASTE.Enabled = toolStripButtonPaste.Enabled;
            toolStripMenuItem_COPY.Enabled = toolStripButtonCopy.Enabled;
            toolStripMenuItem_DELETE.Enabled = toolStripButtonDelete.Enabled;
            toolStripMenuItem_EDIT.Enabled = toolStripButtonEdit.Enabled;


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
            pictureBox1.Invalidate();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
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

            pictureBox1.Invalidate();
        }

        private void toolStripMenuItem_HIDEUNHIDE_Click(object sender, EventArgs e)
        {
            if (EngineLogic.SelectedEvents.Count == 1)
            {
                var selectedEvent = EngineLogic.SelectedEvents.First();
                selectedEvent.IsHidden = !selectedEvent.IsHidden;
                HideSubtree(selectedEvent, selectedEvent.IsHidden);
                ((ToolStripMenuItem)sender).Text = selectedEvent.IsHidden ? "Unhide" : "Hide";

                pictureBox1.Invalidate();
            }
        }

        private void minimalCutSetToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            foreach (FTAitem item in l)
            {
                dataGridView1.Columns.Add(item.Tag, item.Tag);
            }
            dataGridView1.Columns.Add("Frequency", "Frequency");
            int rowIndex = 0;
            int MaxColums = 0;

            foreach (var item in EngineLogic.MCSStructure)
            {

                if (item.Value.Children.Count > 0 && item.Value.Parent != Guid.Empty)
                {
                    dataGridView1.Rows.Add(item.Value.Name);
                    dataGridView1.Rows[rowIndex].Cells[1].Value = item.Value.Value;

                    for (int i = 0; i < item.Value.Children.Count; i++)
                    {
                        FTAitem fTAitem = EngineLogic.GetItem(item.Value.Children[i], EngineLogic.MCSStructure);
                        dataGridView1.Rows[rowIndex].Cells[i + 2].Value = fTAitem.Name;


                        if (i > MaxColums)
                        {
                            MaxColums = i;
                        }
                    }
                    rowIndex += 1;
                }
            }
            MaxColums += 3; // plus frequency and name columns
            for (int i = dataGridView1.Columns.Count - 1; i >= MaxColums; i--)
            {
                dataGridView1.Columns.RemoveAt(i);

            }
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;



            UIEngine.SetupModernGrid(dataGridView1);

            var column = dataGridView1.Columns[1];
            dataGridView1.Sort(column, System.ComponentModel.ListSortDirection.Descending);
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
            SaveGridToCsv(dataGridView1);
        }
    }
}
