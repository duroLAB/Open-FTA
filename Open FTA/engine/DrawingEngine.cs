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
            TopEvent = DrawingStructure.First().Value;

        DrawBackGround(e);
        //DrawEvents(e,EngineLogic.GetItem(EngineLogic.TopEventGuid));
        DrawEvents(e, TopEvent);

        DrawLinesAndGates(e);
        DrawProgressBars(e);

        if (EngineLogic.IsAnyItemOverlapping())
        {
            Rectangle headerRect = new Rectangle(0, 30, 300, 25);
            string t = "Warning: Some items are overlapping!";
            DrawMCSHeader(e, headerRect, t, 25, 10);
        }
    }

    public void SetStructure(Dictionary<Guid, FTAitem> structure) //Switch between Minimalcutset drawing and Treedrawing
    {
        DrawingStructure = structure;
        TopEvent = structure.First().Value;
    }

    private void DrawBackGround(Graphics g)
    {
        using (Brush brush = new SolidBrush(Color.White))
        {
            g.FillRectangle(brush, 0, 0, FTAWidth, FTAHeight);
        }
    }

    private void DrawMCSHeader(Graphics g, Rectangle rect, string text, int Height, int FontSize)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Výška hlavičky
        int headerHeight = Height;

        // Vytvorenie obdĺžnika v hornej časti
        Rectangle headerRect = new Rectangle(rect.X, rect.Y, rect.Width, headerHeight);

        // Vyplnenie zelenou farbou
        using (Brush headerBrush = new SolidBrush(Color.LightGreen))
        {
            g.FillRectangle(headerBrush, headerRect);
        }

        // Ohraničenie hlavičky
        using (Pen borderPen = new Pen(Color.DarkGreen, 2))
        {
            g.DrawRectangle(borderPen, headerRect);
        }

        // Text v strede
        using (Font font = new Font("Segoe UI", FontSize, FontStyle.Bold))
        using (Brush textBrush = new SolidBrush(Color.DarkGreen))
        {
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            g.DrawString(text, font, textBrush, headerRect, sf);
        }
    }

    private void DrawEvents(Graphics g, FTAitem top)
    {
        for (int i = 0; i < top.Children.Count; i++)
        {
            DrawEvents(g, EngineLogic.GetItem(top.Children[i]));
        }
        DrawEvent(g, top);
    }
    private void DrawEventOLD(Graphics g, FTAitem evt)
    {
        // using (Pen selPen = new Pen(Color.Blue))
        using (Pen selPen = new Pen(MainAppSettings.Instance.ItemPen.Color, MainAppSettings.Instance.ItemPen.Width))
        {
            // Prejdeme všetky udalosti v hlavnej štruktúre
            /* foreach (var evtPair in DrawingStructure)
             {*/

            // FTAitem evt = evtPair.Value;

            {
                // Určíme polohu a rozmery udalosti
                Rectangle r = new Rectangle
                {
                    X = evt.X,
                    Y = evt.Y,
                    Width = Constants.EventWidth,
                    Height = Constants.EventHeight
                };
                r = RealPositionToPixel(r);

                // Nastavíme farbu pera podľa toho, či je udalosť vybraná (multiselect)
                if (evt.IsSelected)
                {
                    selPen.Color = Color.Red;
                    selPen.Width = MainAppSettings.Instance.ItemPen.Width + 1;
                }
                else
                {
                    selPen.Color = MainAppSettings.Instance.ItemPen.Color;
                    selPen.Width = MainAppSettings.Instance.ItemPen.Width;
                }


                string searchGuid = evt.Tag;
                bool exists = EngineLogic.HighlightedEvents.Any(item => item.Tag == searchGuid);
                if (exists)
                {
                    selPen.Color = Color.Green;

                    selPen.Width = MainAppSettings.Instance.ItemPen.Width + 1;
                    Rectangle headerRect = new Rectangle(0, 0, 220, 25);
                    string t = "Minimal Cut Set:" + EngineLogic.HighlightedMCS;
                    DrawMCSHeader(g, headerRect, t, 25, 10);

                    headerRect = r;
                    headerRect.X = r.X + r.Width / 2 + 2;
                    headerRect.Y = r.Y - 18;
                    headerRect.Width = r.Width / 2 - 5;

                    DrawMCSHeader(g, headerRect, EngineLogic.HighlightedMCS, 16, 8);
                }


                // Ak je aktívny režim ťahania a udalosť je vybraná, použijeme čierne perá so šrafovaním.
                if (SelectedEventDrag && evt.IsSelected)
                {
                    selPen.Color = Color.Black;
                    selPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }

                if (evt.IsHidden)
                {
                    /*
                    // Zobrazíme trojuholník len pre najvyššiu skrytú udalosť
                    if (evt.Parent == Guid.Empty || !DrawingStructure[evt.Parent].IsHidden)
                    {
                        // Vykreslíme obrázok trojuholníka pomocou bitmapy
                        DrawEventBitmap(g, r, "pic\\events\\Transferin.png", verticalOffset: -r.Height);

                        // Ak je udalosť vybraná, zvýrazníme obrys trojuholníka
                        if (evt.IsSelected)
                        {
                            // Vypočítame rozmery obrázku trojuholníka (používame predvolené scale faktory z DrawEventBitmap)
                            int imageWidth = (int)(r.Width * 0.5f);
                            int imageHeight = (int)(r.Height * 0.5f);
                            int imageX = r.X + (r.Width - imageWidth) / 2;
                            int imageY = r.Y;

                            // Definujeme body trojuholníka
                            Point topVertex = new Point(imageX + imageWidth / 2, imageY);
                            Point bottomLeft = new Point(imageX, imageY + imageHeight);
                            Point bottomRight = new Point(imageX + imageWidth, imageY + imageHeight);
                            Point[] trianglePoints = new Point[] { topVertex, bottomLeft, bottomRight };

                            using (Pen redDashPen = new Pen(Color.Red, 1))
                            {
                                redDashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                                g.DrawPolygon(redDashPen, trianglePoints);
                            }
                        }
                    }*/
                }

                bool temp = false;
                /* if (evt.Parent == Guid.Empty || !DrawingStructure[evt.Parent].IsHidden)
                     temp = true;*/
                if (!evt.IsHidden)
                    temp = true;


                if (temp)
                {
                    if (evt.X < minX) minX = evt.X;
                    if (evt.X > maxX) maxX = evt.X;
                    if (evt.Y < minY) minY = evt.Y;
                    if (evt.Y > maxY) maxY = evt.Y;

                    // Vykreslíme udalosť podľa jej typu – pomocou bitmapy, ak je to definované.
                    switch (evt.ItemType)
                    {
                        case 1:
                            if (evt.Children.Count > 0)
                                if (EngineLogic.GetItem(evt.Children[0]).IsHidden)
                                    BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Transfer");
                            break;
                        case 2: // Basic Event                                
                            BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Basic");
                            break;
                        case 3: // House Event
                                // DrawEventBitmap(g, r, "pic\\events\\eventHouse.png", verticalOffset: 0, scaleFactorWidth: 0.3f, scaleFactorHeight: 0.4f);
                            BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "House");
                            break;
                        case 4: // Undeveloped Event 
                            //DrawEventBitmap(g, r, "pic\\events\\eventUndeveloped.png", verticalOffset: 0, scaleFactorWidth: 0.3f, scaleFactorHeight: 0.4f);
                            BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Undeveloped");
                            break;
                        case 5: // Conditioning Event 
                                // Pre Conditioning Event použijeme odlišný scaling, aby vznikol oválny tvar
                           
                            break;
                        case 6: // Basic Repeat Event 
                                // Prípadne vykreslite obdĺžnik alebo inú bitmapu pre Basic Repeat Event.
                            break;
                        default: // Not Set 
                            break;
                    }



                    // Definujeme rozloženie textu v udalosti: horná časť pre TAG, stred pre názov, spodná pre frekvenciu.
                    int topRowHeight = r.Height / 6;
                    int bottomRowHeight = r.Height / 6;
                    int middleHeight = r.Height - topRowHeight - bottomRowHeight;

                    Rectangle topRect = new Rectangle(r.X, r.Y, r.Width, topRowHeight);
                    Rectangle middleRect = new Rectangle(r.X, r.Y + topRowHeight, r.Width, middleHeight);
                    Rectangle bottomRect = new Rectangle(r.X, r.Y + topRowHeight + middleHeight, r.Width, bottomRowHeight);

                    // Nakreslíme oddelenie jednotlivých častí pomocou čiar.
                    g.DrawLine(selPen, r.X, r.Y + topRowHeight, r.X + r.Width, r.Y + topRowHeight);
                    g.DrawLine(selPen, r.X, r.Y + topRowHeight + middleHeight, r.X + r.Width, r.Y + topRowHeight + middleHeight);

                    // Vykreslenie TAG textu v hornej časti
                    string tagText = evt.Tag;
                    Font tagFont = FindFittingFont(g, tagText, topRect);
                    SizeF tagSize = g.MeasureString(tagText, tagFont);
                    PointF tagPos = new PointF(
                        topRect.X + (topRect.Width - tagSize.Width) / 2,
                        topRect.Y + (topRect.Height - tagSize.Height) / 2
                    );
                    g.DrawString(tagText, tagFont, Brushes.Black, tagPos);

                    // Vykreslenie názvu udalosti v strednej časti
                    //   string nameText = evt.Name;
                    string nameText = SplitStringToLines(evt.Name);

                    Font nameFont = FindFittingFont(g, nameText, middleRect);
                    SizeF nameSize = g.MeasureString(nameText, nameFont);

                    // Overíme, či sa text zmestí do obdlžníka
                    if (nameSize.Width <= middleRect.Width && nameSize.Height <= middleRect.Height)
                    {
                        PointF namePos = new PointF(
                            middleRect.X + (middleRect.Width - nameSize.Width) / 2,
                            middleRect.Y + (middleRect.Height - nameSize.Height) / 2
                        );

                        // Nastavenie zarovnania
                        StringFormat format = new StringFormat();
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;

                        //g.DrawString(nameText, nameFont, Brushes.Black, namePos);
                        g.DrawString(nameText, nameFont, Brushes.Black, middleRect, format);

                    }

                    // Vykreslenie frekvencie v spodnej časti
                    string freqText = (evt.Frequency == 0) ? "" :
                              (evt.Frequency < 0.001) ? evt.Frequency.ToString("0.0000E+0") :
                              evt.Frequency.ToString("F6");

                    if (evt.ItemType > -1)
                    {

                        freqText = evt.ValueType switch
                        {
                            ValueTypes.F => "f=",
                            ValueTypes.P => "P=",
                            ValueTypes.R => "R=",
                            ValueTypes.Lambda => "λ=",
                            _ => ""
                        };
                        freqText += (evt.Value < 0.001) ? evt.Value.ToString("0.0000E+0") : evt.Value.ToString("F6");
                        if (evt.ValueType == ValueTypes.F || evt.ValueType == ValueTypes.Lambda) { freqText += " " + EngineLogic.MetricUnitsList[evt.ValueUnit]; }

                    }

                    Font freqFont = FindFittingFont(g, freqText, bottomRect);
                    SizeF freqSize = g.MeasureString(freqText, freqFont);
                    PointF freqPos = new PointF(
                        bottomRect.X + (bottomRect.Width - freqSize.Width) / 2,
                        bottomRect.Y + (bottomRect.Height - freqSize.Height) / 2
                    );
                    g.DrawString(freqText, freqFont, Brushes.Black, freqPos);

                    // Nakreslíme vonkajšie zaoblené okraje udalosti.
                    using (GraphicsPath roundedRect = GetRoundedRectangle(r, 5))
                    {
                        g.DrawPath(selPen, roundedRect);
                    }


                }
            }
        }
        //}
    }

    private void DrawEvent(Graphics g, FTAitem evt)
    {

        if (evt.IsHidden)
            return;

         
            using (Pen selPen = new Pen(MainAppSettings.Instance.ItemPen.Color, MainAppSettings.Instance.ItemPen.Width))
        {
            Rectangle r = new Rectangle
            {
                X = evt.X,
                Y = evt.Y,
                Width = Constants.EventWidth,
                Height = Constants.EventHeight
            };
            r = RealPositionToPixel(r);

           

            // ---- SELECTION COLORS ----
         /*   if (evt.IsSelected)
            {
                selPen.Color = Color.Red;
                selPen.Width = MainAppSettings.Instance.ItemPen.Width + 1;
            }*/

            

            // Highlight logic preserved
            string searchGuid = evt.Tag;
            bool exists = EngineLogic.HighlightedEvents.Any(item => item.Tag == searchGuid);
            if (exists)
            {
                selPen.Color = Color.Green;
                selPen.Width = MainAppSettings.Instance.ItemPen.Width + 1;

                Rectangle headerRect = new Rectangle(r.X + r.Width / 2 + 2, r.Y - 18, r.Width / 2 - 5, 20);
                DrawMCSHeader(g, headerRect, EngineLogic.HighlightedMCS, 16, 8);
            }

            // ---- DRAW MODERN SHADOW ----
            if (!evt.IsSelected)
                DrawShadowRounded(g, r, 12);
                        
            if (!evt.IsSelected)
                Draw3DGradientBackground(g, r, 12);

            // ---- BORDER (rounded) ----
            using (GraphicsPath border = GetRoundedRectangle(r, 12))
                g.DrawPath(selPen, border);

            // ---- ORIGINAL ICON DRAWING ----
            switch (evt.ItemType)
            {
                case 1:
                    if (evt.Children.Count > 0)
                        if (EngineLogic.GetItem(evt.Children[0]).IsHidden)
                            BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Transfer");
                    break;
                case 2:
                    BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Basic");
                    break;
                case 3:
                    BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "House");
                    break;
                case 4:
                    BitmapDrawingEngine.Instance.DrawEventIcon(g, r, "Undeveloped");
                    break;
            }


            bool isHover = (HoveredItem == evt);

            if (isHover)
            {
                DrawHoverEffect(g, r, 12);
            }


            if (evt.IsSelected)
            {
                //  DrawSelectedEffect(g, r, 12);
                //DrawNeonSelection(g, r, 12);
                //DrawPressedSelection(g, r, 12);
                DrawPressedSelectionv2(g, r, 12);
            }


            // ---- TEXT LAYOUT (unchanged) ----
            int topRowHeight = r.Height / 6;
            int bottomRowHeight = r.Height / 6;
            int middleHeight = r.Height - topRowHeight - bottomRowHeight;

            Rectangle topRect = new Rectangle(r.X, r.Y, r.Width, topRowHeight);
            Rectangle middleRect = new Rectangle(r.X, r.Y + topRowHeight, r.Width, middleHeight);
            Rectangle bottomRect = new Rectangle(r.X, r.Y + topRowHeight + middleHeight, r.Width, bottomRowHeight);

            // Separator lines — softened color for modern UI
            using (Pen softPen = new Pen(Color.FromArgb(120, selPen.Color), 1))
            {
                g.DrawLine(softPen, r.X, r.Y + topRowHeight, r.X + r.Width, r.Y + topRowHeight);
                g.DrawLine(softPen, r.X, r.Y + topRowHeight + middleHeight, r.X + r.Width, r.Y + topRowHeight + middleHeight);
            }

            // ----- TAG -----
            string tagText = evt.Tag;
            Font tagFont = FindFittingFont(g, tagText, topRect);
            TextRenderer.DrawText(g, tagText, tagFont, topRect,
                                  Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // ----- NAME -----
            string nameText = SplitStringToLines(evt.Name);
            Font nameFont = FindFittingFont(g, nameText, middleRect);
            TextRenderer.DrawText(g, nameText, nameFont, middleRect,
                                  Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // ----- FREQUENCY -----
            string freqText = GetFrequencyText(evt);
            Font freqFont = FindFittingFont(g, freqText, bottomRect);
            TextRenderer.DrawText(g, freqText, freqFont, bottomRect,
                                  Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            
        }
    }

    private void DrawPressedSelection(Graphics g, Rectangle r, int radius)
    {
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (LinearGradientBrush brush = new LinearGradientBrush(
            r,
            Color.FromArgb(255, 240, 160, 160),
            Color.FromArgb(255, 200, 60, 60),
            LinearGradientMode.Vertical
        ))
        {
            g.FillPath(brush, path);
        }

        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen p = new Pen(Color.DarkRed, 2))
        {
            g.DrawPath(p, path);
        }
    }

    private void DrawPressedSelectionv2(Graphics g, Rectangle r, int radius)
    {
        // Jemné zosvetlené pozadie
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (LinearGradientBrush brush = new LinearGradientBrush(
            r,
            Color.FromArgb(40, 255, 120, 120),   // svetlá transparentná červená
            Color.FromArgb(10, 200, 60, 60),     // tmavšia šeď s červeným tónom
            LinearGradientMode.Vertical))
        {
            g.FillPath(brush, path);
        }

        // Jemný vnútorný biely highlight (moderné UI)
        Rectangle inner = Rectangle.Inflate(r, -1, -1);
        using (GraphicsPath path = GetRoundedRectangle(inner, radius - 1))
        using (Pen lightPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
        {
            g.DrawPath(lightPen, path);
        }

        // Jemný vonkajší červený rám
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen border = new Pen(Color.FromArgb(180, 220, 50, 50), 2))
        {
            g.DrawPath(border, path);
        }
    }

    private void DrawNeonSelection(Graphics g, Rectangle r, int radius)
    {
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen p = new Pen(Color.FromArgb(255, 255, 50, 50), 3))
        {
            p.LineJoin = LineJoin.Round;
            g.DrawPath(p, path);
        }

        // jemná neónová žiara
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen glow = new Pen(Color.FromArgb(70, 255, 70, 70), 7))
        {
            glow.LineJoin = LineJoin.Round;
            g.DrawPath(glow, path);
        }
    }


    private void DrawSelectedEffect(Graphics g, Rectangle r, int radius)
    {
        // Outer glow
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (PathGradientBrush glow = new PathGradientBrush(path))
        {
            glow.CenterColor = Color.FromArgb(120, 255, 80, 80);          // červený glow
            glow.SurroundColors = new[] { Color.FromArgb(0, 255, 80, 80) };

            g.FillPath(glow, path);
        }

        // Inner white highlight (jemný)
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen inner = new Pen(Color.FromArgb(140, 255, 255, 255), 1))
        {
            g.DrawPath(inner, path);
        }

        // Strong outer border
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen outer = new Pen(Color.FromArgb(200, 255, 40, 40), 2))
        {
            g.DrawPath(outer, path);
        }
    }

    private string GetFrequencyText(FTAitem evt)
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

    private void DrawHoverEffect(Graphics g, Rectangle r, int radius)
    {

        // glow
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (PathGradientBrush glow = new PathGradientBrush(path))
        {
            glow.CenterColor = Color.FromArgb(90, 80, 140, 255);      // modrastý glow
            glow.SurroundColors = new[] { Color.FromArgb(0, 80, 140, 255) };
            g.FillPath(glow, path);
        }

        // jemné zosvetlenie pozadia
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, Color.White)))
        {
            g.FillPath(brush, path);
        }

        // zvýraznený vonkajší rám
        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (Pen p = new Pen(Color.FromArgb(130, 80, 140, 255), 2))
        {
            g.DrawPath(p, path);
        }
    }


    private void DrawShadowRounded(Graphics g, Rectangle r, int radius)
    {
        

        Rectangle shadow = new Rectangle(r.X + 4, r.Y + 4, r.Width, r.Height);

        using (GraphicsPath path = GetRoundedRectangle(shadow, radius))
        using (PathGradientBrush brush = new PathGradientBrush(path))
        {
            brush.CenterColor = Color.FromArgb(50, 0, 0, 0);
            brush.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
            g.FillPath(brush, path);
        }
    }

    private void Draw3DGradientBackground(Graphics g, Rectangle r, int radius)
    {
 

        using (GraphicsPath path = GetRoundedRectangle(r, radius))
        using (LinearGradientBrush b = new LinearGradientBrush(
            r,
            Color.White,
            Color.FromArgb(225, 225, 225),
            LinearGradientMode.Vertical))
        {
            g.FillPath(b, path);
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




    private void DrawProgressBars(Graphics g)
    {
        if (MainAppSettings.Instance.DisplayedMetric == DisplayMetricType.None)
            return;


        Func<FTAitem, double> selector = MainAppSettings.Instance.DisplayedMetric switch
        {
            DisplayMetricType.BIM => (i) => i.BIM,
            DisplayMetricType.CIM => (i) => i.CIM,
            DisplayMetricType.RAW => (i) => i.RAW,
            DisplayMetricType.RRW => (i) => i.RRW,
            DisplayMetricType.FV => (i) => i.FV,
            _ => (i) => 0
        };

        // 2️⃣ Získame zoznam hodnôt metriky
        var metricValues = DrawingStructure.Values
            .Select(selector)
            .Where(v => v != 0)
            .ToList();

        if (!metricValues.Any())
            return;

        double min = metricValues.Min();
        double max = metricValues.Max();

        if (max == 0 || min == max)
            return;

        // 3️⃣ Prejdeme udalosti
        foreach (var evtPair in DrawingStructure)
        {
            FTAitem evt = evtPair.Value;

            if (evt.ItemType >= 2 && !evt.IsHidden)
            {
                double value = selector(evt); // použitie funkcie

                if (value == 0)
                    continue;

                double percent = 100 * (value - min) / (max - min);

                Rectangle r = new Rectangle
                {
                    X = evt.X,
                    Y = evt.Y + (int)(Constants.EventHeight + 0.3 * Constants.EventHeight),
                    Width = Constants.EventWidth,
                    Height = (int)(Constants.EventHeight / 6)
                };
                r = RealPositionToPixel(r);

                string txt = $"{MainAppSettings.Instance.DisplayedMetric}: {percent:F4}";
                DrawTurboProgressBar(g, r, (float)percent, txt);
            }
        }
    }



    static string SplitStringToLines(string text)
    {
        if (text.Contains('|'))
        {
            var parts = text.Split('|', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("\n", parts.Select(p => p.Trim()));
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int totalLength = text.Length;


        int lineCount;
        if (totalLength < 15)
            lineCount = 1;
        else if (totalLength < 30)
            lineCount = 2;
        else
            lineCount = 3;

        if (lineCount == 1)
            return text;


        double[] thresholds;
        if (lineCount == 2)
            thresholds = new double[] { 0.6 };
        else
            thresholds = new double[] { 0.5, 0.8 }; // 50% + 30% + 20%

        var sb = new StringBuilder();
        int currentLength = 0;
        int newlineCount = 0;

        foreach (var word in words)
        {
            if (newlineCount < thresholds.Length &&
                currentLength + word.Length + 1 > totalLength * thresholds[newlineCount])
            {
                sb.Append('\n');
                newlineCount++;
            }
            else if (sb.Length > 0 && sb[^1] != '\n')
            {
                sb.Append(' ');
            }

            sb.Append(word);
            currentLength += word.Length + 1;
        }

        return sb.ToString();
    }

    private Font FindFittingFont(Graphics g, string text, Rectangle rect)// Methode for fitting text into to event visual borders
    {
        float maxFontSize = 10f;
        float minFontSize = 6f;
        Font font;

        for (float fontSize = maxFontSize; fontSize >= minFontSize; fontSize -= 0.5f)
        {
            font = new Font("Arial", fontSize);
            SizeF textSize = g.MeasureString(text, font);

            if (textSize.Width <= rect.Width && textSize.Height <= rect.Height)
            {

                return font;
            }

            font.Dispose();
        }
        return new Font("Arial", minFontSize);
    }

    private void DrawLinesAndGates(Graphics g)
    {

        using (Pen linePen = new Pen(Color.Black, 2))
        using (Brush gateBrush = new SolidBrush(Color.Black))
        {
            foreach (var parentEvent in DrawingStructure)
            {
                var parent = parentEvent.Value;

                if (parent.IsHidden || (DrawingStructure.ContainsKey(parent.Parent) && DrawingStructure[parent.Parent].IsHidden))
                    continue;

                if (EngineLogic.HasEventHiddenChildren(parent))
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
                        var child = EngineLogic.GetItem(childGuid, DrawingStructure);
                        if (child.IsHidden) continue;

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
                        if (!EngineLogic.HasEventHiddenChildren(parent))
                            switch (parent.Gate)
                            {
                                case Gates.OR: // OR Gate
                                    BitmapDrawingEngine.Instance.DrawOrGate(g, parentRect);
                                    break;
                                case Gates.AND: // AND Gate
                                    BitmapDrawingEngine.Instance.DrawANDGate(g, parentRect);
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

    private GraphicsPath GetRoundedRectangleOLD(Rectangle bounds, int radius)//Methode for rounding corners of events
    {
        GraphicsPath path = new GraphicsPath();
        int diameter = radius * 2;

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom - radius);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddLine(bounds.Right - radius, bounds.Bottom, bounds.X + radius, bounds.Bottom);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Y + radius);
        path.CloseFigure();

        return path;
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

    public static void DrawTurboProgressBar(Graphics g, Rectangle rect, float value, string text = null)
    {
        value = Math.Clamp(value, 0, 100);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // pozadie
        using (var backBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
            g.FillRectangle(backBrush, rect);

        // výplň
        int fillWidth = (int)(rect.Width * (value / 100f));
        if (fillWidth > 0)
        {
            Rectangle fillRect = new Rectangle(rect.X, rect.Y, fillWidth, rect.Height);

            // vypočítaj "horný limit" palety (koľko % z nej sa použije)
            float stopLimit = value / 100f;

            // definícia Turbo colormap
            ColorBlend blend = new ColorBlend
            {
                Colors = new[]
                {
                    Color.FromArgb(  48,  18,  59), // dark blue
                    Color.FromArgb(  50,  72, 174), // blue
                    Color.FromArgb(  31, 167, 206), // cyan
                    Color.FromArgb(  87, 216, 122), // greenish
                    Color.FromArgb( 194, 223,  35), // yellow
                    Color.FromArgb( 253, 161,  34), // orange
                    Color.FromArgb( 241,  47,  47)  // red
                },
                Positions = new[]
                {
                    0.0f, 0.15f, 0.35f, 0.55f, 0.7f, 0.85f, 1.0f
                }
            };

            // vytvor nový ColorBlend "orezaný" podľa hodnoty
            ColorBlend partial = CropColorBlend(blend, stopLimit);

            using (var brush = new LinearGradientBrush(
                fillRect,
                Color.Black,
                Color.Black,
                LinearGradientMode.Horizontal))
            {
                brush.InterpolationColors = partial;
                g.FillRectangle(brush, fillRect);
            }
        }

        // rámik
        using (var borderPen = new Pen(Color.Gray, 1))
            g.DrawRectangle(borderPen, rect);

        // text
        if (!string.IsNullOrEmpty(text))
        {
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                Color textColor = value < 50 ? Color.Black : Color.White;
                using (var textBrush = new SolidBrush(textColor))
                    g.DrawString(text, font, textBrush, rect, sf);
            }
        }
    }

    /// <summary>
    /// Oreže farebný prechod podľa "percenta" palety (0–1)
    /// </summary>
    private static ColorBlend CropColorBlend(ColorBlend original, float limit)
    {
        limit = Math.Clamp(limit, 0f, 1f);

        // ak je veľmi nízke percento, zjednodušene použijeme len 1 farbu
        if (limit <= 0.001f)
        {
            var single = new ColorBlend(2)
            {
                Colors = new[] { original.Colors[0], original.Colors[0] },
                Positions = new[] { 0f, 1f }
            };
            return single;
        }

        // interpolácia farby pri danej pozícii limitu
        Color endColor = InterpolateColor(original, limit);

        // všetky farby a pozície do limitu
        var colors = new System.Collections.Generic.List<Color>();
        var positions = new System.Collections.Generic.List<float>();

        for (int i = 0; i < original.Positions.Length; i++)
        {
            float pos = original.Positions[i];
            if (pos <= limit)
            {
                colors.Add(original.Colors[i]);
                positions.Add(pos / limit); // preškáluj do 0–1
            }
            else
                break;
        }

        // pridaj "koncovú" interpolovanú farbu
        colors.Add(endColor);
        positions.Add(1f);

        return new ColorBlend
        {
            Colors = colors.ToArray(),
            Positions = positions.ToArray()
        };
    }

    /// <summary>
    /// Nájde interpolovanú farbu v rámci palety pre dané t (0–1)
    /// </summary>
    private static Color InterpolateColor(ColorBlend blend, float t)
    {
        for (int i = 1; i < blend.Positions.Length; i++)
        {
            float p0 = blend.Positions[i - 1];
            float p1 = blend.Positions[i];
            if (t >= p0 && t <= p1)
            {
                float localT = (t - p0) / (p1 - p0);
                return Lerp(blend.Colors[i - 1], blend.Colors[i], localT);
            }
        }
        return blend.Colors[^1];
    }

    private static Color Lerp(Color a, Color b, float t)
    {
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }


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
        Inv = true;
        return (false);

        
    }

    public void ExportToMeta(string Filename)
    {
        var bounds = GetBounds(EngineLogic.FTAStructure);
        float margin = 10f;
        float width = bounds.Width + 200 + 2 * margin;
        float height = bounds.Height + 200 + 2 * margin;

        using (var stream = new FileStream(Filename, FileMode.Create))
        using (var refGraphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            IntPtr hdc = refGraphics.GetHdc();

            RectangleF rect = new RectangleF(0, 0, width, height);
            using (var metafile = new Metafile(stream, hdc, rect, MetafileFrameUnit.Pixel))
            {
                refGraphics.ReleaseHdc(hdc);

                using (Graphics gMeta = Graphics.FromImage(metafile))
                {
                    // Posun, aby sa záporné hodnoty dostali do viditeľnej oblasti
                    gMeta.TranslateTransform(margin - bounds.Left, margin - bounds.Top);

                    DrawFTA(gMeta, DrawingStructure.First().Value);

                    // Kreslenie všetkých položiek
                    /*  foreach (var fta in items.Values)
                      {
                          gMeta.FillEllipse(Brushes.Red, (float)fta.X - 2, (float)fta.Y - 2, 4, 4);
                      }*/

                    // Ak chceš kresliť polygony:
                    // var points = items.Values.Select(i => new PointF((float)i.X, (float)i.Y)).ToArray();
                    // gMeta.DrawPolygon(Pens.Blue, points);
                }
            }
        }

    }

    public void ExportToBMP(string Filename, ImageFormat format)
    {
        var bounds = GetBounds(EngineLogic.FTAStructure);
        float margin = 10f;
        int width = (int)(bounds.Width + 200 + 2 * margin);
        int height = (int)(bounds.Height + 200 + 2 * margin);


        using (var bitmap = new Bitmap(width, height))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White);

            // Posun, aby sa záporné hodnoty dostali do viditeľnej oblasti
            g.TranslateTransform(margin - bounds.Left, margin - bounds.Top);

            // Kreslenie prvej položky
            DrawFTA(g, DrawingStructure.First().Value);



            // Uloženie do súboru, napr. PNG
            bitmap.Save(Filename, format);


        }

    }
    RectangleF GetBounds(Dictionary<Guid, FTAitem> items)
    {
        /*  if (items == null || items.Count == 0)
              return RectangleF.Empty;

          double minX = double.MaxValue;
          double maxX = double.MinValue;
          double minY = double.MaxValue;
          double maxY = double.MinValue;

          foreach (var fta in items.Values)
          {
              if (fta.X < minX) minX = fta.X;
              if (fta.X > maxX) maxX = fta.X;
              if (fta.Y < minY) minY = fta.Y;
              if (fta.Y > maxY) maxY = fta.Y;
          }
  */
        return new RectangleF(
            (float)minX,
            (float)minY,
            (float)(maxX - minX),
            (float)(maxY - minY)
        );
    }

    

   





}


