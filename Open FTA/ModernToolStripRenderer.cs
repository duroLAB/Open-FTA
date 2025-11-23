using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_FTA
{

    public static class TreeRenderer
    {
        public static void DrawExpandCollapseIcon(Graphics g, TreeNode node, Rectangle bounds)
        {
            int size = 12;
            int x = bounds.X - size - 4;
            int y = bounds.Y + (bounds.Height - size) / 2;

            Rectangle r = new Rectangle(x, y, size, size);

            // štvorec pozadia
            g.FillRectangle(Brushes.Gray, r);
            g.DrawRectangle(Pens.White, r);

            // horizontálna čiara
            g.DrawLine(Pens.White, r.X + 3, r.Y + size / 2, r.X + size - 3, r.Y + size / 2);

            // vertikálna čiara len ak je collapsed
            if (!node.IsExpanded)
                g.DrawLine(Pens.White, r.X + size / 2, r.Y + 3, r.X + size / 2, r.Y + size - 3);
        }
    }


    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }

    public class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernToolStripRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 245, 245)))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (e.Item.Pressed)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(210, 210, 210)))
                    e.Graphics.FillRectangle(b, bounds);
            }
            else if (e.Item.Selected)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    e.Graphics.FillRectangle(b, bounds);
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color ToolStripBorder => Color.FromArgb(200, 200, 200);
        public override Color ToolStripGradientBegin => Color.FromArgb(245, 245, 245);
        public override Color ToolStripGradientMiddle => Color.FromArgb(245, 245, 245);
        public override Color ToolStripGradientEnd => Color.FromArgb(245, 245, 245);
    }


}
