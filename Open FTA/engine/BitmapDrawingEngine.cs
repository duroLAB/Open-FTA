using System.Drawing.Drawing2D;

namespace Open_FTA.engine
{
    /// <summary>
    /// Gates and event symbols below the events (also used for the previews in the event editor).
    /// Flat fill in the colour of the event type, darker outline. The geometry is relative to the
    /// rectangle of the event the gate or symbol belongs to.
    /// </summary>
    internal class BitmapDrawingEngine
    {
        private static readonly Lazy<BitmapDrawingEngine> _instance = new(() => new BitmapDrawingEngine());

        private BitmapDrawingEngine() { }

        public static BitmapDrawingEngine Instance => _instance.Value;

        public void DrawOrGate(Graphics g, Rectangle rect, Color? fill = null)
        {
            int x = (int)(rect.X + 0.35 * rect.Width);
            int y = (int)(rect.Y + rect.Height + 0.05 * rect.Height);
            int w = (int)(0.3 * rect.Width);
            int h1 = (int)(1.5 * w);
            int h2 = (int)(0.4 * w);
            int t = (int)(0.5 * (h1 - h2));
            if (w <= 0)
                return;

            using var path = new GraphicsPath();
            path.AddArc(x, y, w, h1, 0, -180);          // outer curve, right -> top -> left
            path.AddArc(x, y + t, w, h2, 180, 180);     // concave base, left -> right
            path.CloseFigure();

            float textWidth = w * 0.6f;
            float textHeight = (h1 + h2 + t) * 0.45f;
            var centre = new PointF(x + w / 2f, y + h2 / 2f + t / 4f);
            DrawGate(g, path, "OR", centre, textWidth, textHeight, fill ?? EventPalette.Intermediate);
        }

        public void DrawANDGate(Graphics g, Rectangle rect, Color? fill = null)
        {
            int x = (int)(rect.X + 0.35 * rect.Width);
            int y = (int)(rect.Y + rect.Height + 0.05 * rect.Height);
            int w = (int)(0.3 * rect.Width);
            int h1 = w;
            int h2 = (int)(0.4 * w);
            int t = (int)(0.5 * (h1 - h2));
            if (w <= 0)
                return;

            int bottom = y + (int)(h1 * 0.7);
            using var path = new GraphicsPath();
            path.AddArc(x, y, w, h1, 0, -180);          // dome, right -> top -> left
            path.AddLine(x, y + h1 / 2f, x, bottom);
            path.AddLine(x, bottom, x + w, bottom);
            path.CloseFigure();

            float textWidth = w * 0.8f;
            float textHeight = (h1 + h2 + t) * 0.65f;
            var centre = new PointF(x + w / 2f, y + h2 / 2f + t / 1.5f);
            DrawGate(g, path, "AND", centre, textWidth, textHeight, fill ?? EventPalette.Intermediate);
        }

        private static void DrawGate(Graphics g, GraphicsPath path, string text, PointF centre,
                                     float textWidth, float textHeight, Color fill)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);
            using (var outline = new Pen(EventPalette.Darker(fill), 1f))
                g.DrawPath(outline, path);

            System.Drawing.Text.TextRenderingHint oldHint = g.TextRenderingHint;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using Font font = FitFont(g, text, "Segoe UI Semibold", textWidth, textHeight);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, Brushes.White, centre, sf);
            g.TextRenderingHint = oldHint;
        }

        /// <summary>
        /// Symbol below an event: "Basic" (circle), "House", "Undeveloped" (diamond) or
        /// "Transfer" (triangle, collapsed children).
        /// </summary>
        public void DrawEventIcon(Graphics g, Rectangle rect, string iconType, Color? fill = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = (int)(rect.X + 0.4 * rect.Width);
            int y = (int)(rect.Y + rect.Height);
            int w = (int)(0.2 * rect.Width);
            int t = (int)(0.5 * w);
            if (w <= 0)
                return;

            using var path = new GraphicsPath();
            Color colour;
            if (iconType.Contains("Basic"))
            {
                path.AddEllipse(x, y, w, w);
                colour = fill ?? EventPalette.Basic;
            }
            else if (iconType.Contains("Undeveloped"))
            {
                path.AddPolygon(new PointF[] { new(x + t, y), new(x + 2 * t, y + t), new(x + t, y + 2 * t), new(x, y + t) });
                colour = fill ?? EventPalette.Undeveloped;
            }
            else if (iconType.Contains("House"))
            {
                path.AddPolygon(new PointF[] { new(x + t, y), new(x, y + t), new(x, y + 2 * t), new(x + 2 * t, y + 2 * t), new(x + 2 * t, y + t) });
                colour = fill ?? EventPalette.House;
            }
            else if (iconType.Contains("Transfer"))
            {
                path.AddPolygon(new PointF[] { new(x + t, y), new(x, y + 2 * t), new(x + 2 * t, y + 2 * t) });
                colour = fill ?? EventPalette.Intermediate;
            }
            else
                return;

            using (var brush = new SolidBrush(colour))
                g.FillPath(brush, path);
            using (var outline = new Pen(EventPalette.Darker(colour), 1f))
                g.DrawPath(outline, path);
        }

        /// <summary>Largest font (from 30 pt down) of the family whose text fits into the given size.</summary>
        private static Font FitFont(Graphics g, string text, string family, float maxWidth, float maxHeight)
        {
            for (float size = 30f; size > 2f; size -= 0.5f)
            {
                var font = new Font(family, size, FontStyle.Regular);
                SizeF textSize = g.MeasureString(text, font);
                if (textSize.Width <= maxWidth && textSize.Height <= maxHeight)
                    return font;
                font.Dispose();
            }
            return new Font(family, 2f, FontStyle.Regular);
        }
    }
}
