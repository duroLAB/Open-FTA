using Open_FTA.Properties;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace Open_FTA.forms
{
    /// <summary>
    /// Read-only view of the minimal cut set tree (tab "Minimal Cut Set Tree"):
    /// pan with the left mouse button, zoom with the mouse wheel, centre, export as image.
    /// The tree is built from FTAlogic.MinimalCutSets by <see cref="McsDisplayTree"/> and drawn
    /// by its own DrawingEngine, so the fault tree itself cannot be changed here.
    /// </summary>
    [DesignerCategory("Code")]
    public class McsTreeView : UserControl
    {
        private readonly ToolStrip toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        private readonly PictureBox canvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };

        private DrawingEngine? engine;
        private McsDisplayTree? tree;
        private bool needsFit;
        private bool dragging;
        private Point lastMouse;

        /// <summary>Raised by the "Run Analysis" button; the main form runs the MCS analysis.</summary>
        public event EventHandler? RunAnalysisRequested;

        public McsTreeView()
        {
            var run = new ToolStripButton("Run Analysis", Resources.RunAnalysis) { ImageTransparentColor = Color.Magenta };
            run.Click += (s, e) => RunAnalysisRequested?.Invoke(this, EventArgs.Empty);

            var center = new ToolStripButton("Center", Resources.AbsolutePosition_24) { ImageTransparentColor = Color.Magenta, ImageScaling = ToolStripItemImageScaling.None };
            center.Click += (s, e) => FitToView();

            var export = new ToolStripButton("Export image", Resources.TakeSnapshot) { ImageTransparentColor = Color.Magenta, ImageScaling = ToolStripItemImageScaling.None };
            export.Click += (s, e) => ExportImage();

            toolStrip.Items.AddRange(new ToolStripItem[] { run, new ToolStripSeparator(), center, export });

            // Fill control first, then the top tool strip (WinForms docks the last added control first)
            Controls.Add(canvas);
            Controls.Add(toolStrip);

            canvas.Paint += Canvas_Paint;
            canvas.Resize += (s, e) => canvas.Invalidate();
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += (s, e) => dragging = false;
            canvas.MouseWheel += Canvas_MouseWheel;
        }

        /// <summary>Builds the tree from the current minimal cut sets and fits it to the view.</summary>
        public void ShowTree(FTAlogic logic)
        {
            if (logic.MinimalCutSets.Count == 0)
            {
                Clear();
                return;
            }

            tree = McsDisplayTree.Build(logic);
            engine = new DrawingEngine(logic, tree.Items);
            needsFit = true;
            canvas.Invalidate();
        }

        /// <summary>Removes the tree (e.g. after a new file was opened).</summary>
        public void Clear()
        {
            tree = null;
            engine = null;
            canvas.Invalidate();
        }

        /// <summary>Redraws the tree, e.g. after a minimal cut set was highlighted.</summary>
        public void RefreshView() => canvas.Invalidate();

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            if (engine == null || tree == null)
            {
                using var font = new Font("Segoe UI", 11);
                TextRenderer.DrawText(e.Graphics,
                    "Run the minimal cut set analysis (Run Analysis) to draw the minimal cut set tree.",
                    font, canvas.ClientRectangle, Color.DimGray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            if (needsFit && canvas.Width > 0 && canvas.Height > 0)
                SetFitTransform();

            engine.SetDimensions(canvas.Width, canvas.Height);
            engine.DrawFTA(e.Graphics, tree.Root);
        }

        private void FitToView()
        {
            if (engine == null)
                return;
            SetFitTransform();
            canvas.Invalidate();
        }

        /// <summary>Zoom and offset so that the whole tree is centred in the canvas.</summary>
        private void SetFitTransform()
        {
            if (engine == null || tree == null || canvas.Width <= 0 || canvas.Height <= 0)
                return;

            var items = tree.Items.Values;
            int minX = items.Min(i => i.X);
            int minY = items.Min(i => i.Y);
            float width = Math.Max(1, items.Max(i => i.X) + Constants.EventWidth - minX);
            float height = Math.Max(1, items.Max(i => i.Y) + Constants.EventHeight - minY);

            double zoom = Math.Clamp(Math.Min(canvas.Width / width, canvas.Height / height) * 0.9, 0.1, 5.0);
            engine.GlobalZoom = zoom;
            engine.offsetX = (int)((canvas.Width - width * zoom) / 2 - minX * zoom);
            engine.offsetY = (int)((canvas.Height - height * zoom) / 2 - minY * zoom);
            needsFit = false;
        }

        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            dragging = true;
            lastMouse = e.Location;
        }

        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!dragging || engine == null)
                return;
            engine.offsetX += e.X - lastMouse.X;
            engine.offsetY += e.Y - lastMouse.Y;
            lastMouse = e.Location;
            canvas.Invalidate();
        }

        /// <summary>Zoom around the mouse cursor.</summary>
        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (engine == null)
                return;
            double oldZoom = engine.GlobalZoom;
            double newZoom = Math.Clamp(oldZoom * (1 + e.Delta * 0.0005), 0.1, 5.0);
            engine.offsetX = (int)(e.X - (e.X - engine.offsetX) * newZoom / oldZoom);
            engine.offsetY = (int)(e.Y - (e.Y - engine.offsetY) * newZoom / oldZoom);
            engine.GlobalZoom = newZoom;
            canvas.Invalidate();
        }

        private void ExportImage()
        {
            if (engine == null)
                return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap|*.bmp|Windows Metafile|*.wmf",
                Title = "Export minimal cut set tree",
                FileName = "MinimalCutSetTree.png"
            };
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                switch (sfd.FilterIndex)
                {
                    case 2: engine.ExportToBMP(sfd.FileName, ImageFormat.Jpeg); break;
                    case 3: engine.ExportToBMP(sfd.FileName, ImageFormat.Bmp); break;
                    case 4: engine.ExportToMeta(sfd.FileName); break;
                    default: engine.ExportToBMP(sfd.FileName, ImageFormat.Png); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creating the image.\n\nDetails: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OpenFTA.MainForm.AskToShowInExplorer(sfd.FileName, "Image has been exported");
        }
    }
}
