using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

/// <summary>Icons of the tool bars and menus (see <see cref="AppIcons"/>).</summary>
internal enum AppIcon
{
    New, Open, Save, Undo, Redo, Evaluate, SaveImage, Report, Database, Settings, Help,
    AddBasicEvent, AddEvent, Delete, Edit, Copy, Paste, Center, Sort, Run,
    Export, Word, Print, FolderOpen, NewFolder, OpenInExplorer, Archive, Add,
}

/// <summary>
/// Icons in one style: outline glyphs of the Windows icon font (Segoe Fluent Icons on Windows 11,
/// Segoe MDL2 Assets on Windows 10) in one dark colour, and the fault tree icons (add event, add basic
/// event, database, run) drawn in code in the colours of the event types. Bitmaps are made for the DPI of
/// the window, so they stay sharp at 125 % or 150 %, and are cached. Without an icon font the original
/// images of the buttons are kept.
/// </summary>
internal static class AppIcons
{
    private static readonly Color IconColor = Color.FromArgb(51, 65, 85);
    private static readonly Color RunColor = Color.FromArgb(16, 124, 65);

    private static readonly Dictionary<AppIcon, string> Glyphs = new()
    {
        [AppIcon.New] = "",            // Page (+ badge)
        [AppIcon.Open] = "",           // FolderOpen
        [AppIcon.Save] = "",
        [AppIcon.Undo] = "",
        [AppIcon.Redo] = "",
        [AppIcon.Evaluate] = "",       // Calculator
        [AppIcon.SaveImage] = "",      // Picture
        [AppIcon.Report] = "",         // ReportDocument
        [AppIcon.Settings] = "",
        [AppIcon.Help] = "",
        [AppIcon.Delete] = "",
        [AppIcon.Edit] = "",
        [AppIcon.Copy] = "",
        [AppIcon.Paste] = "",
        [AppIcon.Center] = "",         // FitPage
        [AppIcon.Sort] = "",
        [AppIcon.Export] = "",
        [AppIcon.Word] = "",           // Document
        [AppIcon.Print] = "",
        [AppIcon.FolderOpen] = "",
        [AppIcon.NewFolder] = "",
        [AppIcon.OpenInExplorer] = "", // NewWindow
        [AppIcon.Archive] = "",        // Package
        [AppIcon.Add] = "",
    };

    private static readonly Dictionary<(AppIcon, int), Bitmap> Cache = new();
    private static string? _fontFamily;
    private static bool _fontSearched;

    /// <summary>Segoe Fluent Icons, otherwise Segoe MDL2 Assets, otherwise null.</summary>
    private static string? IconFontFamily
    {
        get
        {
            if (!_fontSearched)
            {
                _fontSearched = true;
                using var fonts = new InstalledFontCollection();
                var names = fonts.Families.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                _fontFamily = names.Contains("Segoe Fluent Icons") ? "Segoe Fluent Icons"
                            : names.Contains("Segoe MDL2 Assets") ? "Segoe MDL2 Assets"
                            : null;
            }
            return _fontFamily;
        }
    }

    /// <summary>Icon of the given logical size (pixels at 96 DPI) for the DPI of the control; null without an icon font.</summary>
    public static Bitmap? Get(AppIcon icon, int logicalSize, Control dpiSource)
    {
        int px = Math.Max(8, (int)Math.Round(logicalSize * dpiSource.DeviceDpi / 96f));
        if (Cache.TryGetValue((icon, px), out Bitmap? cached))
            return cached;
        if (IconFontFamily == null)
            return null;

        Bitmap bmp = icon switch
        {
            AppIcon.AddBasicEvent => DrawAddBasicEvent(px),
            AppIcon.AddEvent => DrawAddEvent(px),
            AppIcon.Database => DrawDatabase(px),
            AppIcon.Run => DrawRun(px),
            _ => DrawGlyph(Glyphs[icon], px, IconColor),
        };
        if (icon == AppIcon.New)
            DrawPlusBadge(bmp);
        Cache[(icon, px)] = bmp;
        return bmp;
    }

    /// <summary>Sets the icon of a tool strip button or menu item (keeps its image when no icon font is available).</summary>
    public static void Apply(ToolStripItem item, AppIcon icon, Control dpiSource, int logicalSize = 20)
    {
        Bitmap? bmp = Get(icon, logicalSize, dpiSource);
        if (bmp == null)
            return;
        item.Image = bmp;
        item.ImageScaling = ToolStripItemImageScaling.None;
        item.ImageTransparentColor = Color.Empty;
    }

    private static Bitmap NewBitmap(int px, out Graphics g)
    {
        var bmp = new Bitmap(px, px, PixelFormat.Format32bppArgb);
        g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        return bmp;
    }

    /// <summary>Glyph of the icon font as a filled path, centred in the bitmap.</summary>
    private static Bitmap DrawGlyph(string glyph, int px, Color color)
    {
        Bitmap bmp = NewBitmap(px, out Graphics g);
        using (g)
        using (var family = new FontFamily(IconFontFamily!))
        using (var path = new GraphicsPath())
        {
            path.AddString(glyph, family, (int)FontStyle.Regular, px * 0.9f, PointF.Empty, StringFormat.GenericTypographic);
            RectangleF b = path.GetBounds();
            if (b.Width > 0 && b.Height > 0)
            {
                using var move = new Matrix();
                move.Translate((float)Math.Round(px / 2f - (b.X + b.Width / 2)), (float)Math.Round(px / 2f - (b.Y + b.Height / 2)));
                path.Transform(move);
            }
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
        }
        return bmp;
    }

    /// <summary>Small accent circle with a white plus in the bottom right corner.</summary>
    private static void DrawPlusBadge(Bitmap bmp)
    {
        int px = bmp.Width;
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float r = px * 0.25f;
        float cx = px - r - 0.5f, cy = px - r - 0.5f;
        using (var fill = new SolidBrush(ModernUi.Accent))
            g.FillEllipse(fill, cx - r, cy - r, 2 * r, 2 * r);
        using (var ring = new Pen(Color.White, Math.Max(1f, px / 20f)))
            g.DrawEllipse(ring, cx - r, cy - r, 2 * r, 2 * r);
        using var plus = new Pen(Color.White, Math.Max(1f, px / 13f));
        g.DrawLine(plus, cx - r * 0.55f, cy, cx + r * 0.55f, cy);
        g.DrawLine(plus, cx, cy - r * 0.55f, cx, cy + r * 0.55f);
    }

    /// <summary>Basic event: circle in the colour of basic events, with a plus badge.</summary>
    private static Bitmap DrawAddBasicEvent(int px)
    {
        Bitmap bmp = NewBitmap(px, out Graphics g);
        using (g)
        {
            float m = px * 0.08f, d = px * 0.7f;
            using (var fill = new SolidBrush(EventPalette.Basic))
                g.FillEllipse(fill, m, m, d, d);
            using var outline = new Pen(EventPalette.Darker(EventPalette.Basic), Math.Max(1f, px / 20f));
            g.DrawEllipse(outline, m, m, d, d);
        }
        DrawPlusBadge(bmp);
        return bmp;
    }

    /// <summary>Event card with a header in the colour of intermediate events, with a plus badge.</summary>
    private static Bitmap DrawAddEvent(int px)
    {
        Bitmap bmp = NewBitmap(px, out Graphics g);
        using (g)
        {
            var card = new RectangleF(px * 0.05f, px * 0.16f, px * 0.78f, px * 0.56f);
            float radius = Math.Max(1f, px * 0.1f);
            using var path = RoundedRect(card, radius);
            g.FillPath(Brushes.White, path);
            GraphicsState state = g.Save();
            g.SetClip(path);
            using (var header = new SolidBrush(EventPalette.Intermediate))
                g.FillRectangle(header, card.X, card.Y, card.Width, card.Height * 0.34f);
            g.Restore(state);
            using var outline = new Pen(EventPalette.Intermediate, Math.Max(1f, px / 16f));
            g.DrawPath(outline, path);
        }
        DrawPlusBadge(bmp);
        return bmp;
    }

    /// <summary>Database cylinder in outline style.</summary>
    private static Bitmap DrawDatabase(int px)
    {
        Bitmap bmp = NewBitmap(px, out Graphics g);
        using (g)
        using (var pen = new Pen(IconColor, Math.Max(1f, px / 14f)))
        {
            float x = px * 0.2f, w = px * 0.6f, top = px * 0.12f, bottom = px * 0.88f, e = px * 0.2f;
            g.DrawEllipse(pen, x, top, w, e);
            g.DrawLine(pen, x, top + e / 2, x, bottom - e / 2);
            g.DrawLine(pen, x + w, top + e / 2, x + w, bottom - e / 2);
            g.DrawArc(pen, x, bottom - e, w, e, 0, 180);
            g.DrawArc(pen, x, (top + bottom - e) / 2, w, e, 0, 180);
        }
        return bmp;
    }

    /// <summary>Filled green triangle (main action).</summary>
    private static Bitmap DrawRun(int px)
    {
        Bitmap bmp = NewBitmap(px, out Graphics g);
        using (g)
        using (var fill = new SolidBrush(RunColor))
        {
            using var path = new GraphicsPath();
            path.AddPolygon(new PointF[] { new(px * 0.25f, px * 0.15f), new(px * 0.85f, px * 0.5f), new(px * 0.25f, px * 0.85f) });
            g.FillPath(fill, path);
            using var round = new Pen(RunColor, Math.Max(1f, px / 10f)) { LineJoin = LineJoin.Round };
            g.DrawPath(round, path);
        }
        return bmp;
    }

    private static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
