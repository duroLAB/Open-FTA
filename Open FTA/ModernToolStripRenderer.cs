using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_FTA
{
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
