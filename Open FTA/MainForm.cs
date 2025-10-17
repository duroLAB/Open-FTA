using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OpenFTA
{
    public partial class MainForm : Form
    {
        FTAlogic EngineLogic;
        UIlogic UIEngine;
        DrawingEngine TreeEngine;
        // Settingsform APPSet;

        private Stack<List<FTAitem>> undoStack = new Stack<List<FTAitem>>();
        private Stack<List<FTAitem>> redoStack = new Stack<List<FTAitem>>();

        public MainForm()
        {
            InitializeComponent();
            EngineLogic = new FTAlogic();
            UIEngine = new UIlogic(EngineLogic);
            TreeEngine = new DrawingEngine(EngineLogic);
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
        }

       
        private void MainForm_Load(object sender, EventArgs e)
        {
            UIEngine.FillTreeView(treeView1);
            TreeEngine.SetDimensions(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            EngineLogic.AssignLevelsToAllEvents();
        }

        #region picture box events
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Left:
                    TreeEngine.offsetX = TreeEngine.offsetX + 20;
                    break;
                case Keys.Right:
                    TreeEngine.offsetX = TreeEngine.offsetX - 20;
                    break;
                case Keys.Up:
                    TreeEngine.offsetY = TreeEngine.offsetY + 20;
                    break;
                case Keys.Down:
                    TreeEngine.offsetY = TreeEngine.offsetY - 20;
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

                bool EventWasSelected = TreeEngine.Mouse_SelectEvent(coordinates);
                if (EventWasSelected)
                {
                    TreeEngine.SetLastDragPosition(me.Location);
                }
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox1.Invalidate();
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TreeEngine.SelectedEventDrag || TreeEngine.IsDraggingTree)
            {
                TreeEngine.Mouse_DragEvent(e.Location);
                pictureBox1.Invalidate();
            }

            int X = 0;
            int Y = 0;
            TreeEngine.PixelToRealPosition(e.Location, ref X, ref Y);
            String str = "X:"+X+", Y:"+Y+")";
            toolStripStatusLabelCoordinates.Text = str;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            TreeEngine.Mouse_Up();
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.Invalidate();
        }
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            TreeEngine.offsetX += (int)(TreeEngine.GlobalZoom*0.05 *(0.5*pictureBox1.Width - e.X));
            TreeEngine.offsetY += (int)(TreeEngine.GlobalZoom*0.05 *(0.5*pictureBox1.Height - e.Y));
            TreeEngine.GlobalZoom = TreeEngine.GlobalZoom * (1 + e.Delta * 0.0005);
            pictureBox1.Invalidate();
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            TreeEngine.DrawFTA(e.Graphics);
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

                    MessageBox.Show("FTAStructure loaded successfully.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (MainAppSettings.Current.AutoSortTree)
            {
               ArrangeMainTreeHierarchically();
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
            ArrangeMainTreeHierarchically();

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
            int selectedCount = EngineLogic.SelectedEvents.Count;

            if (selectedCount == 0)
            {
                MessageBox.Show("No parent selected. Please select one parent.", "Error");
                return;
            }
            else if (selectedCount > 1)
            {
                MessageBox.Show("You can choose only one parent!", "Error");
                return;
            }

            var parent = EngineLogic.SelectedEvents[0];
            if (parent.ItemType == 2)
            {
                MessageBox.Show("Please select a non-basic event as parent.", "Error");
                return;
            }


            var Parent = EngineLogic.SelectedEvents[0];

            FormEditEvent edit = new FormEditEvent(EngineLogic);
            edit.EngineLogic = EngineLogic;

            edit.textBoxName.Text = "New Item";
            edit.comboBoxGates.SelectedValue = 1;
            edit.comboBoxEventType.SelectedValue = 1;
            edit.textBoxFrequency.Text = "0";
            edit.textBoxTag.Text = "NewTag";


            SaveStateForUndo();

            if (DialogResult.OK == edit.ShowDialog())
            {
                Guid A = EngineLogic.AddNewEvent(Parent.GuidCode, "", 0, 0, 0);
                var newEvent = EngineLogic.GetItem(A);

                ReadInfoFromEditForm(edit, newEvent);

                UIEngine.FillTreeView(treeView1);
            }

            EngineLogic.AssignLevelsToAllEvents();
            pictureBox1.Invalidate();
        }

        private void toolStripMenuItem_EDIT_Click(object sender, EventArgs e)
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

            edit.textBoxName.Text = selectedEvent.Name;
            edit.comboBoxGates.SelectedValue = selectedEvent.GateType;
            edit.comboBoxEventType.SelectedValue = selectedEvent.ItemType;
           // edit.textBoxFrequency.Text = selectedEvent.Frequency.ToString();
            edit.textBoxTag.Text = selectedEvent.Tag;

            edit.textBoxFrequency.Text = selectedEvent.UserMetricValue.ToString();
            edit.comboBoxMetricType.SelectedIndex = selectedEvent.UserMetricType;
            edit.comboBoxUnits.SelectedIndex = selectedEvent.UserMetricUnit;

            SaveStateForUndo();

            if (DialogResult.OK == edit.ShowDialog())
            {

                ReadInfoFromEditForm(edit, selectedEvent);
                UIEngine.FillTreeView(treeView1);
                EngineLogic.AssignLevelsToAllEvents();
                pictureBox1.Invalidate();
            }

        
        }
        #endregion

        public void ArrangeMainTreeHierarchically()
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
        }

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
             
        private void ReadInfoFromEditForm(FormEditEvent edit,FTAitem item)
        {
            item.Name = edit.textBoxName.Text;
            item.Tag = edit.textBoxTag.Text;
            item.GateType = Convert.ToInt32(edit.comboBoxGates.SelectedValue);
            item.ItemType = Convert.ToInt32(edit.comboBoxEventType.SelectedValue);

            /* double inputFreq = Convert.ToDouble(edit.textBoxFrequency.Text);
             string selectedUnit = edit.comboBoxUnits.SelectedItem.ToString();*/

            //TODO: add comp
            //selectedEvent.Frequency = ConvertToYears(inputFreq, selectedUnit);

            item.Description = edit.textBoxDescription.Text;

            if (item.ItemType > 1)
            {
                item.UserMetricValue = Convert.ToDouble(edit.textBoxFrequency.Text.ToString());
                item.UserMetricType = edit.comboBoxMetricType.SelectedIndex;
                item.UserMetricUnit = edit.comboBoxUnits.SelectedIndex;
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

            TreeEngine.GlobalZoom = newZoom;
            TreeEngine.offsetX = offsetX;
            TreeEngine.offsetY = offsetY;
            pictureBox1.Invalidate();
        }

       
    }
}
