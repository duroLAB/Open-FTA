using Open_FTA.engine;
using Open_FTA.Properties;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class DrawingEngine(FTAlogic f, Dictionary<Guid, FTAitem> structure)
{
    public FTAlogic EngineLogic = f;
    public int offsetX = 0;
    public int offsetY = 0;
    public double GlobalZoom = 1;
    private int FTAWidth;
    private int FTAHeight;
    public bool SelectedEventDrag;
    public bool IsDraggingTree = false;
    private Point LastMousePosition;
    private Point _lastDragPosition;
    Dictionary<Guid, FTAitem> DrawingStructure = structure;
    public FTAitem TopEvent;

    private FTAitem HoveredItem = null;

    // Hover and selection are not drawn into exported images.
    private bool _exporting;

    // Accent colour of hover and selection (Windows accent blue); the MCS highlight stays green.
    private static readonly Color SelectionAccent = Color.FromArgb(0, 120, 215);

    double minX;
    double maxX;
    double minY;
    double maxY;

    

    public void SetDimensions(int fTAWidth, int fTAHeight)
    {
        FTAWidth = fTAWidth;
        FTAHeight = fTAHeight;
    }

    public void DrawFTA(Graphics e)
    {
        DrawFTA(e, TopEvent);
    }

    public void DrawFTA(Graphics e, FTAitem top)
    {
        minX = double.MaxValue;
        maxX = double.MinValue;
        minY = double.MaxValue;
        maxY = double.MinValue;

        TopEvent = top;

        if (TopEvent == null)
            TopEvent = GetRoot();

        _metrics = ComputeMetrics();
        DrawBackGround(e);
        //DrawEvents(e,EngineLogic.GetItem(EngineLogic.TopEventGuid));
        DrawEvents(e, TopEvent,true);
        

        DrawLinesAndGates(e);
    }

    public void SetStructure(Dictionary<Guid, FTAitem> structure) //Switch between Minimalcutset drawing and Treedrawing
    {
        DrawingStructure = structure;
        TopEvent = GetRoot();
    }

    private static readonly Color CanvasColor = Color.FromArgb(245, 247, 250);
    private static readonly Color CanvasDotColor = Color.FromArgb(214, 220, 228);
    private const int GridStep = 20;   // distance of the grid dots in model units

    /// <summary>Light grey canvas with a dot grid that moves with pan and zoom; exports stay white.</summary>
    private void DrawBackGround(Graphics g)
    {
        if (_exporting)
        {
            g.FillRectangle(Brushes.White, 0, 0, FTAWidth, FTAHeight);
            return;
        }

        using (var brush = new SolidBrush(CanvasColor))
            g.FillRectangle(brush, 0, 0, FTAWidth, FTAHeight);

        float step = (float)(GridStep * GlobalZoom);
        if (step < 8)
            return;   // too dense when zoomed out

        float dot = (float)Math.Clamp(1.5 * GlobalZoom, 1.2, 2.5);
        float startX = offsetX % step;
        float startY = offsetY % step;
        if (startX < 0) startX += step;
        if (startY < 0) startY += step;

        using var dotBrush = new SolidBrush(CanvasDotColor);
        for (float x = startX; x < FTAWidth; x += step)
            for (float y = startY; y < FTAHeight; y += step)
                g.FillRectangle(dotBrush, x - dot / 2, y - dot / 2, dot, dot);
    }


    private void DrawEvents(Graphics g, FTAitem top, bool isTopEvent)
    {
        for (int i = 0; i < top.Children.Count; i++)
        {
            FTAitem? child = GetDrawingItem(top.Children[i]);
            if (child != null)
                DrawEvents(g, child, false);
        }

        DrawEvent(g, top, isTopEvent);
    }

    // The engine draws only its own DrawingStructure (the fault tree or the MCS tree),
    // so events, children and collapsed state are always looked up there.
    private FTAitem? GetDrawingItem(Guid guid) =>
        DrawingStructure.TryGetValue(guid, out FTAitem? item) ? item : null;

    /// <summary>True if the children of the event are collapsed (hidden) in the drawn structure.</summary>
    private bool HasHiddenChildren(FTAitem evt) =>
        !evt.IsHidden && evt.Children.Count > 0 && GetDrawingItem(evt.Children[0]) is { IsHidden: true };

    /// <summary>Root of the drawn structure: the top event of the fault tree, otherwise the item without a parent.</summary>
    private FTAitem GetRoot() =>
        GetDrawingItem(EngineLogic.TopEventGuid)
        ?? DrawingStructure.Values.FirstOrDefault(i => !DrawingStructure.ContainsKey(i.Parent))
        ?? DrawingStructure.Values.First();

    private void DrawEvent(Graphics g, FTAitem evt, bool isTopEvent)
    {
        if (evt.IsHidden)
            return;

        Rectangle r = RealPositionToPixel(new Rectangle(evt.X, evt.Y, Constants.EventWidth, Constants.EventHeight));

        // events of the minimal cut set selected in the tree: green border and the name of the MCS above
        bool inHighlightedMcs = EngineLogic.HighlightedEvents.Any(item => item.Tag == evt.Tag);
        if (inHighlightedMcs)
            DrawMcsLabel(g, r, EngineLogic.HighlightedMCS);

        bool isSelected = evt.IsSelected && !_exporting;
        bool isHover = HoveredItem == evt && !_exporting;
        FrequencyDisplayMode mode = MainAppSettings.Instance.FrequencyDisplayMode;
        bool hideValue = mode == FrequencyDisplayMode.alwaysHidden
                         || (mode == FrequencyDisplayMode.topEventsOnly && !isTopEvent);

        using Pen? mcsPen = inHighlightedMcs ? new Pen(EventPalette.Mcs, 2f) : null;
        MetricInfo? metric = _metrics.TryGetValue(evt, out MetricInfo m) ? m : null;
        DrawCard(g, evt, r, MainAppSettings.Instance.EventStyle, isTopEvent, mcsPen, isSelected, isHover, hideValue, metric);
        DrawTypeIcon(g, evt, r);
    }

    /// <summary>Name of the highlighted minimal cut set as a green label above the top right corner of the event.</summary>
    private void DrawMcsLabel(Graphics g, Rectangle r, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        float k = r.Height / 90f;
        int h = Math.Max(4, (int)Math.Round(15 * k));
        int gap = (int)Math.Round(4 * k);
        using var font = new Font("Segoe UI Semibold", Math.Max(1f, 8.5f * k), FontStyle.Regular, GraphicsUnit.Pixel);
        int w = Math.Max(h, (int)Math.Ceiling(g.MeasureString(text, font).Width + 10 * k));
        var pill = new Rectangle(r.Right - w - gap, r.Y - h - gap, w, h);

        System.Drawing.Text.TextRenderingHint oldHint = g.TextRenderingHint;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        using (GraphicsPath path = GetRoundedRectangle(pill, Math.Max(1, h / 2)))
        using (var fill = new SolidBrush(EventPalette.Mcs))
            g.FillPath(fill, path);
        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            g.DrawString(text, font, Brushes.White, pill, sf);
        g.TextRenderingHint = oldHint;
    }

    /// <summary>Symbol below the event: basic, house, undeveloped, or transfer for collapsed children.</summary>
    private void DrawTypeIcon(Graphics g, FTAitem evt, Rectangle r)
    {
        switch (evt.ItemType)
        {
            case 1:
                if (evt.Children.Count > 0 && HasHiddenChildren(evt))
                    BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Transfer", EventPalette.Intermediate);
                break;
            case 2:
                BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Basic", EventPalette.Basic);
                break;
            case 3:
                BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "House", EventPalette.House);
                break;
            case 4:
                BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Undeveloped", EventPalette.Undeveloped);
                break;
        }
    }

    // ---- Event card styles (Settings: Event style) ----

    private static readonly Color CardText = Color.FromArgb(31, 41, 51);
    private static readonly Color CardValueText = Color.FromArgb(72, 82, 94);
    private static readonly Color CleanAccent = Color.FromArgb(40, 70, 110);

    /// <summary>
    /// Event card: Header (tag band coloured by event type, grey value row),
    /// Minimal (colour stripe, tag and value as labels), Clean (tinted tag and value rows).
    /// Sizes are given for a 90 px high card and scale with the zoom. Names have the same font size
    /// in all events; it is reduced only when the text does not fit.
    /// </summary>
    private void DrawCard(Graphics g, FTAitem evt, Rectangle r, EventStyle style, bool isTopEvent,
                                Pen? mcsPen, bool isSelected, bool isHover, bool hideValue, MetricInfo? metric = null)
    {
        float k = r.Height / 90f;
        int Px(float v) => Math.Max(1, (int)Math.Round(v * k));
        int radius = Px(style switch { EventStyle.Header => 7, EventStyle.Minimal => 6, _ => 9 });
        Color typeColor = EventPalette.ForEvent(evt, isTopEvent);

        System.Drawing.Text.TextRenderingHint oldHint = g.TextRenderingHint;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        try
        {
            DrawShadowRounded(g, r, radius, lifted: isHover || isSelected);

            using GraphicsPath card = GetRoundedRectangle(r, radius);
            using (var white = new SolidBrush(Color.White))
                g.FillPath(white, card);

            // importance measure in the bottom row (CardRow) or as a badge on the corner (Badge)
            bool metricInRow = metric != null && MainAppSettings.Instance.MetricDisplay == MetricDisplayStyle.CardRow;
            bool metricBadge = metric != null && MainAppSettings.Instance.MetricDisplay == MetricDisplayStyle.Badge;
            bool showRow = !hideValue || metricInRow;

            Rectangle tagRect, nameRect, valueRect = Rectangle.Empty;
            Color tagColor, valueColor, borderColor;
            string nameFamily = "Segoe UI Semibold";
            StringAlignment tagAlign = StringAlignment.Center;

            switch (style)
            {
                case EventStyle.Header:
                {
                    int band = Px(18);
                    tagRect = new Rectangle(r.X, r.Y, r.Width, band);
                    FillClipped(g, card, tagRect, typeColor);
                    if (showRow)
                    {
                        valueRect = new Rectangle(r.X, r.Bottom - band, r.Width, band);
                        FillClipped(g, card, valueRect, Color.FromArgb(241, 243, 245));
                        using var sep = new Pen(Color.FromArgb(222, 226, 230));
                        g.DrawLine(sep, r.X, valueRect.Y, r.Right, valueRect.Y);
                    }
                    nameRect = Rectangle.FromLTRB(r.X + Px(5), tagRect.Bottom, r.Right - Px(5), showRow ? valueRect.Top : r.Bottom);
                    (tagColor, valueColor, borderColor) = (Color.White, CardValueText, Color.FromArgb(150, 160, 172));
                    break;
                }
                case EventStyle.Minimal:
                {
                    int stripe = Px(5);
                    FillClipped(g, card, new Rectangle(r.X, r.Y, stripe, r.Height), typeColor);
                    tagRect = new Rectangle(r.X + Px(11), r.Y + Px(3), r.Width - Px(16), Px(16));
                    tagAlign = StringAlignment.Near;
                    if (showRow)
                        valueRect = new Rectangle(r.X + stripe, r.Bottom - Px(20), r.Width - stripe, Px(14));
                    nameRect = Rectangle.FromLTRB(r.X + stripe + Px(4), r.Y + Px(18), r.Right - Px(4), showRow ? r.Bottom - Px(22) : r.Bottom - Px(4));
                    (tagColor, valueColor, borderColor) = (typeColor, CleanAccent, Color.FromArgb(203, 210, 217));
                    break;
                }
                default: // Clean
                {
                    int band = Px(16);
                    Color tint = Color.FromArgb(234, 241, 250);
                    tagRect = new Rectangle(r.X, r.Y, r.Width, band);
                    FillClipped(g, card, tagRect, tint);
                    using var sep = new Pen(Color.FromArgb(196, 212, 232));
                    g.DrawLine(sep, r.X, tagRect.Bottom, r.Right, tagRect.Bottom);
                    if (showRow)
                    {
                        valueRect = new Rectangle(r.X, r.Bottom - band, r.Width, band);
                        FillClipped(g, card, valueRect, tint);
                        g.DrawLine(sep, r.X, valueRect.Y, r.Right, valueRect.Y);
                    }
                    nameRect = Rectangle.FromLTRB(r.X + Px(5), tagRect.Bottom, r.Right - Px(5), showRow ? valueRect.Top : r.Bottom);
                    nameFamily = "Segoe UI";
                    (tagColor, valueColor, borderColor) = (CleanAccent, CleanAccent, Color.FromArgb(74, 111, 165));
                    break;
                }
            }

            // thin bar along the bottom edge: position of the measure between min and max
            if (metricInRow)
            {
                int barHeight = Px(3);
                FillClipped(g, card, new Rectangle(r.X, r.Bottom - barHeight, r.Width, barHeight), Color.FromArgb(226, 230, 236));
                int fill = Math.Max(barHeight, (int)(r.Width * Math.Max(metric!.Value.Position, 0.02f)));
                FillClipped(g, card, new Rectangle(r.X, r.Bottom - barHeight, fill, barHeight), MetricColor(metric.Value.Position));
            }

            // ---- state and border (green border of a highlighted minimal cut set stays) ----
            if (isSelected)
            {
                DrawSelection(g, r, radius, style == EventStyle.Clean ? tagRect : Rectangle.Empty, isHover);
            }
            else
            {
                if (isHover)
                    DrawHoverTint(g, r, radius);
                using var hoverPen = new Pen(SelectionAccent, 1.5f);
                using var stylePen = new Pen(borderColor, 1f);
                g.DrawPath(mcsPen ?? (isHover ? hoverPen : stylePen), card);
            }

            // ---- texts ----
            using var oneLine = new StringFormat(StringFormatFlags.NoWrap)
            { Alignment = tagAlign, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            using var centred = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisWord };

            string tag = evt.Tag ?? "";
            using (Font f = FitFont(g, tag, tagRect, "Segoe UI", FontStyle.Bold, 9.5f * k, singleLine: true))
            using (var b = new SolidBrush(tagColor))
                g.DrawString(tag, f, b, tagRect, oneLine);

            // "|" in a name is a manual line break
            string name = (evt.Name ?? "").Contains('|')
                ? string.Join("\n", evt.Name!.Split('|').Select(t => t.Trim()))
                : evt.Name ?? "";
            using (Font f = FitFont(g, name, nameRect, nameFamily, FontStyle.Regular, 10.5f * k))
            using (var b = new SolidBrush(CardText))
                g.DrawString(name, f, b, nameRect, centred);

            if (showRow)
            {
                // text stays above the metric bar along the bottom edge
                Rectangle row = metricInRow && style != EventStyle.Minimal
                    ? Rectangle.FromLTRB(valueRect.X, valueRect.Y, valueRect.Right, valueRect.Bottom - Px(2))
                    : valueRect;
                // with the measure in the row: value of the event on the left, measure on the right
                Rectangle valueArea = metricInRow
                    ? Rectangle.FromLTRB(row.X + Px(6), row.Y, row.X + (int)(row.Width * 0.56), row.Bottom)
                    : row;

                if (!hideValue)
                {
                    string value = GetFrequencyText(evt);
                    oneLine.Alignment = metricInRow ? StringAlignment.Near : StringAlignment.Center;
                    using Font f = FitFont(g, value, valueArea, "Segoe UI", FontStyle.Regular, 9f * k, singleLine: true);
                    if (style == EventStyle.Minimal)
                    {
                        // value as a pill
                        float w = Math.Min(valueArea.Width - Px(4), g.MeasureString(value, f).Width + Px(10));
                        float x = metricInRow ? valueArea.X - Px(4) : valueArea.X + (valueArea.Width - w) / 2;
                        var pill = new Rectangle((int)x, row.Y, (int)w, row.Height);
                        using GraphicsPath pillPath = GetRoundedRectangle(pill, Math.Max(1, pill.Height / 2));
                        using (var pillFill = new SolidBrush(Color.FromArgb(232, 240, 250)))
                            g.FillPath(pillFill, pillPath);
                    }
                    using var b = new SolidBrush(valueColor);
                    g.DrawString(value, f, b, valueArea, oneLine);
                }

                if (metricInRow)
                    DrawMetricText(g, Rectangle.FromLTRB(row.X + Px(6), row.Y, row.Right - Px(6), row.Bottom), metric!.Value, k, centred: hideValue);
            }

            if (metricBadge)
                DrawMetricBadge(g, r, metric!.Value, k);
        }
        finally
        {
            g.TextRenderingHint = oldHint;
        }
    }

    /// <summary>Fills a rectangle clipped to a path (bands inside a rounded card).</summary>
    private static void FillClipped(Graphics g, GraphicsPath clip, Rectangle rect, Color color)
    {
        GraphicsState state = g.Save();
        g.SetClip(clip, CombineMode.Intersect);
        using (var brush = new SolidBrush(color))
            g.FillRectangle(brush, rect);
        g.Restore(state);
    }

    /// <summary>
    /// Font of the given pixel size, made smaller (down to half) only if the text does not fit
    /// into the rectangle; text that still does not fit is trimmed with an ellipsis when drawn.
    /// </summary>
    private static Font FitFont(Graphics g, string text, Rectangle rect, string family, FontStyle style,
                                float maxPx, bool singleLine = false)
    {
        float size = Math.Max(1f, maxPx);
        float minPx = Math.Max(1f, maxPx * 0.5f);
        if (rect.Width <= 0 || rect.Height <= 0 || string.IsNullOrEmpty(text))
            return new Font(family, size, style, GraphicsUnit.Pixel);

        using var sf = new StringFormat(singleLine ? StringFormatFlags.NoWrap : 0);
        while (true)
        {
            var font = new Font(family, size, style, GraphicsUnit.Pixel);
            SizeF needed = singleLine
                ? g.MeasureString(text, font, PointF.Empty, sf)
                : g.MeasureString(text, font, rect.Width, sf);
            if ((needed.Width <= rect.Width && needed.Height <= rect.Height) || size <= minPx)
                return font;
            font.Dispose();
            size = Math.Max(minPx, size * 0.92f);
        }
    }

    /// <summary>
    /// Selected event: light accent fill, tinted tag row, 2 px accent border and a soft focus ring
    /// around the card (stronger when the mouse is over it).
    /// </summary>
    private void DrawSelection(Graphics g, Rectangle r, int radius, Rectangle tagRow, bool hover)
    {
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        {
            using (var fill = new SolidBrush(Color.FromArgb(hover ? 34 : 24, SelectionAccent)))
                g.FillPath(fill, path);

            GraphicsState state = g.Save();   // tag row tinted, clipped to the rounded card
            g.SetClip(path, CombineMode.Intersect);
            using (var band = new SolidBrush(Color.FromArgb(55, SelectionAccent)))
                g.FillRectangle(band, tagRow);
            g.Restore(state);

            using (var border = new Pen(SelectionAccent, 2f))
                g.DrawPath(border, path);
        }

        using (GraphicsPath ring = GetRoundedRectangle(Rectangle.Inflate(r, 4, 4), radius + 4))
        using (var ringPen = new Pen(Color.FromArgb(hover ? 110 : 75, SelectionAccent), 3f))
            g.DrawPath(ringPen, ring);
    }

    /// <summary>Event under the mouse: very light accent fill (the border is drawn by the caller).</summary>
    private void DrawHoverTint(Graphics g, Rectangle r, int radius)
    {
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (var fill = new SolidBrush(Color.FromArgb(14, SelectionAccent)))
            g.FillPath(fill, path);
    }

    internal string GetFrequencyText(FTAitem evt)
    {
        // Ak nemá item definovaný typ hodnoty, nič nevykreslíme
        if (evt.ItemType < 0)
            return "";

        // Urči prefix podľa typu hodnoty
        string prefix = evt.ValueType switch
        {
            ValueTypes.F => "f=",
            ValueTypes.P => "P=",
            ValueTypes.R => "R=",
            ValueTypes.Lambda => "λ=",
            _ => ""
        };

        // Formát hodnoty (E-notation pre malé čísla)
        string formattedValue = (evt.Value < 0.001)
            ? evt.Value.ToString("0.0000E+0")
            : evt.Value.ToString("F6");

        // Jednotky iba pre f a λ
        string unit = "";
        if (evt.ValueType == ValueTypes.F || evt.ValueType == ValueTypes.Lambda)
        {
            if (evt.ValueUnit >= 0 && evt.ValueUnit < EngineLogic.MetricUnitsList.Count)
                unit = " " + EngineLogic.MetricUnitsList[evt.ValueUnit];
        }

        return prefix + formattedValue + unit;
    }

    /// <summary>Soft shadow; a lifted card (hover, selection) has a larger and darker shadow.</summary>
    private void DrawShadowRounded(Graphics g, Rectangle r, int radius, bool lifted = false)
    {
        int offset = lifted ? 6 : 4;
        Rectangle shadow = new Rectangle(r.X + offset, r.Y + offset, r.Width, r.Height);

        using (GraphicsPath path = GetRoundedRectangle(shadow, radius))
        using (PathGradientBrush brush = new PathGradientBrush(path))
        {
            brush.CenterColor = Color.FromArgb(lifted ? 75 : 50, 0, 0, 0);
            brush.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
            g.FillPath(brush, path);
        }
    }

    private GraphicsPath GetRoundedRectangle(Rectangle r, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        int d = radius * 2;

        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);

        path.CloseFigure();
        return path;
    }
    /// <summary>Displayed importance measure of a basic event: name, value and position between min and max (0..1).</summary>
    private readonly record struct MetricInfo(string Label, string Value, float Position);

    private Dictionary<FTAitem, MetricInfo> _metrics = new();

    /// <summary>
    /// Values of the measure chosen in Settings (Displayed Metric) for the visible basic events, with their
    /// position between the smallest and the largest value. Empty when no measure is shown, when there are no
    /// values (measures not computed, n/a) or when all values are equal.
    /// </summary>
    private Dictionary<FTAitem, MetricInfo> ComputeMetrics()
    {
        var result = new Dictionary<FTAitem, MetricInfo>();
        DisplayMetricType metric = MainAppSettings.Instance.DisplayedMetric;
        if (metric == DisplayMetricType.None)
            return result;

        Func<FTAitem, double> selector = metric switch
        {
            DisplayMetricType.BIM => (i) => i.BIM,
            DisplayMetricType.CIM => (i) => i.CIM,
            DisplayMetricType.RAW => (i) => i.RAW,
            DisplayMetricType.RRW => (i) => i.RRW,
            DisplayMetricType.FV => (i) => i.FV,
            _ => (i) => 0
        };

        var values = DrawingStructure.Values
            .Select(selector)
            .Where(v => v != 0 && double.IsFinite(v))   // NaN = n/a, infinity cannot be scaled
            .ToList();
        if (values.Count == 0)
            return result;
        double min = values.Min();
        double max = values.Max();
        if (max == 0 || min == max)
            return result;

        foreach (FTAitem evt in DrawingStructure.Values)
        {
            if (evt.ItemType < 2 || evt.IsHidden)
                continue;
            double value = selector(evt);
            if (value == 0 || !double.IsFinite(value))
                continue;
            result[evt] = new MetricInfo(metric.ToString(), value.ToString("G4"), (float)((value - min) / (max - min)));
        }
        return result;
    }

    /// <summary>Light to deep orange with the position of the value between min and max.</summary>
    private static Color MetricColor(float position)
    {
        float p = Math.Clamp(position, 0f, 1f);
        Color lo = Color.FromArgb(250, 205, 160), hi = Color.FromArgb(217, 72, 15);
        return Color.FromArgb((int)(lo.R + (hi.R - lo.R) * p), (int)(lo.G + (hi.G - lo.G) * p), (int)(lo.B + (hi.B - lo.B) * p));
    }

    /// <summary>"BIM 0.999": name in grey and value in bold, right-aligned in the area or centred.</summary>
    private static void DrawMetricText(Graphics g, Rectangle area, MetricInfo metric, float k, bool centred)
    {
        float px = Math.Max(1f, 8.5f * k);
        using var labelFont = new Font("Segoe UI Semibold", px, FontStyle.Regular, GraphicsUnit.Pixel);
        using var valueFont = new Font("Segoe UI", px, FontStyle.Bold, GraphicsUnit.Pixel);
        using var sf = new StringFormat(StringFormat.GenericTypographic)
        { LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };

        float labelWidth = g.MeasureString(metric.Label, labelFont, PointF.Empty, sf).Width;
        float valueWidth = g.MeasureString(metric.Value, valueFont, PointF.Empty, sf).Width;
        float gap = 3 * k;
        float x = centred
            ? area.X + (area.Width - labelWidth - gap - valueWidth) / 2
            : area.Right - valueWidth - gap - labelWidth;

        using var labelBrush = new SolidBrush(Color.FromArgb(100, 110, 122));
        using var valueBrush = new SolidBrush(Color.FromArgb(31, 41, 51));
        g.DrawString(metric.Label, labelFont, labelBrush, new RectangleF(x, area.Y, labelWidth + 1, area.Height), sf);
        g.DrawString(metric.Value, valueFont, valueBrush, new RectangleF(x + labelWidth + gap, area.Y, valueWidth + 2, area.Height), sf);
    }

    /// <summary>Badge "BIM 0.999" on the bottom right corner of the event, coloured by the position of the value.</summary>
    private void DrawMetricBadge(Graphics g, Rectangle r, MetricInfo metric, float k)
    {
        string text = metric.Label + " " + metric.Value;
        using var font = new Font("Segoe UI Semibold", Math.Max(1f, 8.5f * k), FontStyle.Regular, GraphicsUnit.Pixel);
        int h = Math.Max(4, (int)Math.Round(15 * k));
        int w = Math.Max(h, (int)Math.Ceiling(g.MeasureString(text, font).Width + 8 * k));
        var badge = new Rectangle(r.Right - w + (int)Math.Round(6 * k), r.Bottom - h / 2 + (int)Math.Round(2 * k), w, h);

        using (GraphicsPath path = GetRoundedRectangle(badge, Math.Max(1, h / 2)))
        {
            using (var fill = new SolidBrush(MetricColor(metric.Position)))
                g.FillPath(fill, path);
            using (var outline = new Pen(Color.White, Math.Max(1f, 1.2f * k)))
                g.DrawPath(outline, path);
        }
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
        using var textBrush = new SolidBrush(metric.Position > 0.45f ? Color.White : Color.FromArgb(90, 50, 20));
        g.DrawString(text, font, textBrush, badge, sf);
    }

    private void DrawLinesAndGates(Graphics g)
    {

        using (Pen linePen = new Pen(EventPalette.Line, 1.5f))
        {
            foreach (var parentEvent in DrawingStructure)
            {
                var parent = parentEvent.Value;

                if (parent.IsHidden || (DrawingStructure.ContainsKey(parent.Parent) && DrawingStructure[parent.Parent].IsHidden))
                    continue;

                if (HasHiddenChildren(parent))
                { continue; }


                if (parent.Children.Count > 0)
                {
                    Rectangle parentRect = RealPositionToPixel(new Rectangle
                    {
                        X = parent.X,
                        Y = parent.Y,
                        Width = Constants.EventWidth,
                        Height = Constants.EventHeight
                    });
                    Point parentCenter = new Point(
                        parentRect.X + parentRect.Width / 2,
                        parentRect.Y + parentRect.Height
                    );

                    // Gate position
                    int gateY = parentRect.Y + parentRect.Height + 20;

                    // Gate-Parent line
                    Point gateTop = new Point(parentCenter.X, gateY - 10);
                    Point gateBottom = new Point(parentCenter.X, gateY + 20);
                    g.DrawLine(linePen, gateTop, parentCenter);

                    // Child-Gate line
                    foreach (var childGuid in parent.Children)
                    {
                        var child = GetDrawingItem(childGuid);
                        if (child == null || child.IsHidden) continue;

                        Rectangle childRect = RealPositionToPixel(new Rectangle
                        {
                            X = child.X,
                            Y = child.Y,
                            Width = Constants.EventWidth,
                            Height = Constants.EventHeight
                        });

                        Point childTop = new Point(childRect.X + childRect.Width / 2, childRect.Y);


                        int verticalDistance = Math.Abs(childTop.Y - gateTop.Y);
                        int horizontalDistance = Math.Abs(childTop.X - gateTop.X);

                        Point verticalMidpoint = new Point(gateTop.X, gateTop.Y + verticalDistance / 2);
                        Point horizontalMidpoint = new Point(childTop.X, gateTop.Y + verticalDistance / 2);

                        g.DrawLine(linePen, gateTop, verticalMidpoint);
                        g.DrawLine(linePen, verticalMidpoint, horizontalMidpoint);
                        g.DrawLine(linePen, horizontalMidpoint, childTop);
                    }

                    if (MainAppSettings.Instance.Technicalgates)
                    {

                        if (parent.Gate == Gates.OR)
                        {
                            // OR Gate 
                            g.DrawArc(linePen, parentCenter.X - 20, gateY - 10, 40, 60, 0, -180);
                            g.DrawArc(linePen, parentCenter.X - 20, gateY + 10, 40, 20, 0, -180);
                        }
                        else if (parent.Gate == Gates.AND)
                        {
                            // AND Gate 
                            Point orLeft = new Point(parentCenter.X - 20, gateY + 20);
                            Point orRight = new Point(parentCenter.X + 20, gateY + 20);
                            Point orTop = new Point(parentCenter.X, gateY - 20);

                            g.DrawArc(linePen, parentCenter.X - 20, gateY - 10, 40, 60, 0, -180);
                            g.DrawLine(linePen, orLeft, orRight);
                        }
                    }

                    else
                    {
                        if (!HasHiddenChildren(parent))
                            switch (parent.Gate)
                            {
                                case Gates.OR: // OR Gate
                                    BitmapDrawingEngine.Instance.DrawOrGate(g, parentRect, EventPalette.ForEvent(parent, parent == TopEvent));
                                    break;
                                case Gates.AND: // AND Gate
                                    BitmapDrawingEngine.Instance.DrawANDGate(g, parentRect, EventPalette.ForEvent(parent, parent == TopEvent));
                                    break;
                                /*  case 3: // Not Gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gateNot.png", verticalOffset: 10);
                                      break;
                                  case 4: // Nand Gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gateNand.png", verticalOffset: 10);
                                      break;
                                  case 5: // Nor gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gateNor.png", verticalOffset: 10);
                                      break;
                                  case 6: // Xor gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gateXor.png", verticalOffset: 10);
                                      break;
                                  case 7: // Inhibit gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gateInhibit.png", verticalOffset: 10);
                                      break;
                                  case 8: // Priority and gate
                                      DrawGateBitmap(g, parentRect, "pic\\gates\\gatePriorityand.png", verticalOffset: 10);
                                      break;*/
                                default:
                                    break;

                            }
                    }

                }


            }
        }
    }

    private bool IsTopHiddenParent(FTAitem evt)
    {
        if (!evt.IsHidden)
            return false;

        if (evt.Parent != null && DrawingStructure.TryGetValue(evt.Parent, out FTAitem? parent) && parent != null && parent.IsHidden)
            return false;

        foreach (var childGuid in evt.Children)
        {
            if (DrawingStructure.TryGetValue(childGuid, out FTAitem? child) && child != null && !child.IsHidden)
                return false;
        }

        return true;
    }

    public Rectangle RealPositionToPixel(Rectangle r)
    {
        Rectangle outR = new Rectangle();
        outR.X = (int)(r.X * GlobalZoom + offsetX);
        outR.Y = (int)(r.Y * GlobalZoom + offsetY);

        outR.Width = (int)(r.Width * GlobalZoom);
        outR.Height = (int)(r.Height * GlobalZoom);

        return outR;
    }


    public void PixelToRealPosition(Point p, ref int xreal, ref int yreal)
    {
        xreal = (int)((p.X - offsetX) / GlobalZoom);
        yreal = (int)((p.Y - offsetY) / GlobalZoom);
    }

   
    public void SetLastDragPosition(Point p)
    {
        _lastDragPosition = p;
    }

    private List<FTAitem> GetSubtreeItems(FTAitem root)
    {
        List<FTAitem> subtree = new List<FTAitem> { root };

        foreach (var childGuid in root.Children)
        {
            if (DrawingStructure.TryGetValue(childGuid, out FTAitem? child) && child is not null)
            {
                subtree.AddRange(GetSubtreeItems(child));
            }
        }

        return subtree;
    }

    /// <summary>Height of the symbols below the events (model units).</summary>
    private static int SymbolHeight => (int)(0.2 * Constants.EventWidth);

    public void Mouse_DragEvent(Point mouseCoordinates)
    {
        if (SelectedEventDrag && EngineLogic.SelectedEvents.Count > 0)
        {
            int deltaX = mouseCoordinates.X - _lastDragPosition.X;
            int deltaY = mouseCoordinates.Y - _lastDragPosition.Y;

            if (deltaX * deltaX + deltaY * deltaY < 50 * GlobalZoom)
                return;

            foreach (var evt in EngineLogic.SelectedEvents)
            {
                if (IsTopHiddenParent(evt))
                {
                    List<FTAitem> subtreeItems = GetSubtreeItems(evt);

                    double temp1 = deltaX * 1000 / GlobalZoom;
                    double temp2 = deltaY * 1000 / GlobalZoom;

                    foreach (var item in subtreeItems)
                    {
                        item.X += (int)Math.Round(temp1 / 1000);
                        item.Y += (int)Math.Round(temp2 / 1000);
                    }
                }
                else
                {
                    double temp1 = deltaX * 1000 / GlobalZoom;
                    double temp2 = deltaY * 1000 / GlobalZoom;

                    evt.X += (int)Math.Round(temp1 / 1000);
                    evt.Y += (int)Math.Round(temp2 / 1000);

                }
            }

            _lastDragPosition = mouseCoordinates;
        }
        else if (IsDraggingTree)
        {
            // Panning 
            int deltaX = mouseCoordinates.X - LastMousePosition.X;
            int deltaY = mouseCoordinates.Y - LastMousePosition.Y;

            offsetX += deltaX;
            offsetY += deltaY;

            LastMousePosition = mouseCoordinates;
        }
    }
    public void Mouse_Up()
    {
        SelectedEventDrag = false;
        IsDraggingTree = false;
    }
    public bool Mouse_SelectEvent(Point mouseCoordinates)
    {
        int X = 0;
        int Y = 0;
        PixelToRealPosition(mouseCoordinates, ref X, ref Y);

        // Check if the SHIFT key is pressed.
        bool shiftDown = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;


        // Find the event that was clicked.
        FTAitem? clickedEvent = null;
        foreach (var evt in DrawingStructure.Values)
        {

            if (evt.IsHidden && !IsTopHiddenParent(evt))
                continue;
            if ((X > evt.X) && (X < evt.X + Constants.EventWidth) &&
                (Y > evt.Y) && (Y < evt.Y + Constants.EventHeight))
            {
                clickedEvent = evt;
                break;
            }
        }

        if (clickedEvent != null)
        {
            // If SHIFT is pressed, toggle the selection mode.
            if (shiftDown)
            {
                if (clickedEvent.IsSelected)
                {
                    clickedEvent.IsSelected = false;
                    EngineLogic.SelectedEvents.Remove(clickedEvent);
                }
                else
                {
                    clickedEvent.IsSelected = true;
                    EngineLogic.SelectedEvents.Add(clickedEvent);
                }
            }
            else
            {
                // If the clicked event is already selected, then do nothing (keep multi-select).
                if (!clickedEvent.IsSelected)
                {
                    foreach (var evt in DrawingStructure.Values)
                    {
                        evt.IsSelected = false;
                    }
                    if (EngineLogic.SelectedEvents != null)
                    {
                        EngineLogic.SelectedEvents.Clear();
                    }
                    clickedEvent.IsSelected = true;
                    if (EngineLogic.SelectedEvents != null)
                        EngineLogic.SelectedEvents.Add(clickedEvent);
                }
            }

            SelectedEventDrag = true;
            IsDraggingTree = false;
            SetLastDragPosition(mouseCoordinates);
            return true;
        }
        else
        {
            // Clicked on empty area: clear selection if SHIFT is not pressed.
            if (!shiftDown)
            {
                foreach (var evt in DrawingStructure.Values)
                {
                    evt.IsSelected = false;
                }
                if (EngineLogic.SelectedEvents != null)
                {
                    EngineLogic.SelectedEvents.Clear();
                }
            }
            // Enable panning.
            SelectedEventDrag = false;
            IsDraggingTree = true;
            LastMousePosition = mouseCoordinates;
            return false;
        }
    }

    /// <summary>Clears the hover state (mouse left the canvas); true if a repaint is needed.</summary>
    public bool ClearHover()
    {
        bool changed = HoveredItem != null;
        HoveredItem = null;
        return changed;
    }

    public bool Mouse_OnEvevt(Point mouseCoordinates,out bool Inv)
    {
        Inv = false;
        int X = 0;
        int Y = 0;
        PixelToRealPosition(mouseCoordinates, ref X, ref Y);
        FTAitem old = HoveredItem;
        HoveredItem = null;

        FTAitem? clickedEvent = null;
        foreach (var evt in DrawingStructure.Values)
        {

            if (evt.IsHidden && !IsTopHiddenParent(evt))
                continue;
            if ((X > evt.X) && (X < evt.X + Constants.EventWidth) &&
                (Y > evt.Y) && (Y < evt.Y + Constants.EventHeight))
            {
                HoveredItem = evt;
                if (old != HoveredItem)
                    Inv = true;
               return (true);
            }
        }

        HoveredItem = null;
        Inv = old != null;   // repaint only when the hover ends, not on every move over empty space
        return (false);

        
    }

    private const float ExportMargin = 20f;

    /// <summary>Vector export (WMF) of the whole tree at zoom 1.</summary>
    public void ExportToMeta(string Filename)
    {
        RectangleF bounds = GetExportBounds();
        float width = bounds.Width + 2 * ExportMargin;
        float height = bounds.Height + 2 * ExportMargin;

        using (var stream = new FileStream(Filename, FileMode.Create))
        using (var refGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            IntPtr hdc = refGraphics.GetHdc();

            RectangleF rect = new RectangleF(0, 0, width, height);
            using (var metafile = new Metafile(stream, hdc, rect, MetafileFrameUnit.Pixel))
            {
                refGraphics.ReleaseHdc(hdc);

                using (Graphics gMeta = Graphics.FromImage(metafile))
                    DrawForExport(gMeta, bounds);
            }
        }
    }

    /// <summary>
    /// Zoom and offset so that the whole visible tree (events, symbols, metric bars) fills the view
    /// of the given size with a small margin; the zoom is limited to 2 so that small trees are not blown up.
    /// </summary>
    public void FitToView(int viewWidth, int viewHeight, int margin = 20)
    {
        if (viewWidth <= 2 * margin || viewHeight <= 2 * margin || DrawingStructure.Count == 0)
            return;

        RectangleF bounds = GetExportBounds();
        double zoom = Math.Min((viewWidth - 2.0 * margin) / Math.Max(1, bounds.Width),
                               (viewHeight - 2.0 * margin) / Math.Max(1, bounds.Height));
        zoom = Math.Clamp(zoom, 0.05, 2.0);

        GlobalZoom = zoom;
        offsetX = (int)Math.Round((viewWidth - bounds.Width * zoom) / 2 - bounds.Left * zoom);
        offsetY = (int)Math.Round((viewHeight - bounds.Height * zoom) / 2 - bounds.Top * zoom);
    }

    /// <summary>Bitmap export (PNG, JPEG, BMP) of the whole tree at zoom 1.</summary>
    public void ExportToBMP(string Filename, ImageFormat format)
    {
        RectangleF bounds = GetExportBounds();
        int width = (int)Math.Ceiling(bounds.Width + 2 * ExportMargin);
        int height = (int)Math.Ceiling(bounds.Height + 2 * ExportMargin);

        using (var bitmap = new Bitmap(width, height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                DrawForExport(g, bounds);
            }
            bitmap.Save(Filename, format);
        }
    }

    /// <summary>
    /// Draws the whole tree from the real top event at zoom 1 without offset, shifted so that
    /// the export area starts at (ExportMargin, ExportMargin). The view (zoom, offset, TopEvent)
    /// is restored afterwards.
    /// </summary>
    private void DrawForExport(Graphics g, RectangleF bounds)
    {
        double zoom = GlobalZoom;
        int ox = offsetX, oy = offsetY;
        var previousTop = TopEvent;
        try
        {
            _exporting = true;
            GlobalZoom = 1;
            offsetX = 0;
            offsetY = 0;
            g.TranslateTransform(ExportMargin - bounds.Left, ExportMargin - bounds.Top);
            DrawFTA(g, GetRoot());
        }
        finally
        {
            _exporting = false;
            GlobalZoom = zoom;
            offsetX = ox;
            offsetY = oy;
            TopEvent = previousTop;
        }
    }

    /// <summary>
    /// Area covered by the visible events in model coordinates: whole event rectangles
    /// (FTAitem.X/Y is the top-left corner) plus the symbols and metric bars below the events.
    /// </summary>
    private RectangleF GetExportBounds()
    {
        var visible = DrawingStructure.Values.Where(i => !i.IsHidden).ToList();
        if (visible.Count == 0)
            return new RectangleF(0, 0, Constants.EventWidth, Constants.EventHeight);

        float left = visible.Min(i => i.X);
        float top = visible.Min(i => i.Y);
        float right = visible.Max(i => i.X) + Constants.EventWidth;
        float bottom = visible.Max(i => i.Y) + Constants.EventHeight;

        // symbols below the events (basic, house, ...); the metric is drawn at the same height
        bottom += SymbolHeight + 2;

        return RectangleF.FromLTRB(left, top, right, bottom);
    }

    

   





}


