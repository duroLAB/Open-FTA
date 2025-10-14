using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

class DrawingEngine
{
    FTAlogic EngineLogic;
    public int offsetX = 0;
    public int offsetY = 0;
    public double GlobalZoom = 1;
    private int FTAWidth;
    private int FTAHeight;
    public bool SelectedEventDrag;

    public static bool UseTechnicalGates { get; set; } = MainAppSettings.Current.Technicalgates;


    public DrawingEngine(FTAlogic f)
    {
        EngineLogic = f;
    }

    public void SetDimensions(int fTAWidth, int fTAHeight)
    {
        FTAWidth = fTAWidth;
        FTAHeight = fTAHeight;
    }

    public void DrawFTA(Graphics e)
    {
        DrawBackGround(e);
        DrawEvents(e);
        DrawLinesAndGates(e);
    }

    public void SetStructure(Dictionary<Guid, FTAitem> structure) //Switch between Minimalcutset drawing and Treedrawing
    {
        EngineLogic.FTAStructure = structure;
    }

    private void DrawBackGround(Graphics g)
    {
        using (Brush brush = new SolidBrush(Color.White))
        {
            g.FillRectangle(brush, 0, 0, FTAWidth, FTAHeight);
        }
    }

    private void DrawEvents(Graphics g)
    {
        using (Pen selPen = new Pen(Color.Blue))
        {
            // Prejdeme všetky udalosti v hlavnej štruktúre
            foreach (var evtPair in EngineLogic.FTAStructure)
            {

                FTAitem evt = evtPair.Value;

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
                        selPen.Color = Color.Red;
                    else
                        selPen.Color = Color.Blue;

                    // Ak je aktívny režim ťahania a udalosť je vybraná, použijeme čierne perá so šrafovaním.
                    if (SelectedEventDrag && evt.IsSelected)
                    {
                        selPen.Color = Color.Black;
                        selPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    }

                    if (evt.IsHidden)
                    {
                        // Zobrazíme trojuholník len pre najvyššiu skrytú udalosť
                        if (evt.Parent == Guid.Empty || !EngineLogic.FTAStructure[evt.Parent].IsHidden)
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
                        }
                    }

                    else
                    {
                        // Vykreslíme udalosť podľa jej typu – pomocou bitmapy, ak je to definované.
                        switch (evt.ItemType)
                        {
                            case 1: // Intermediate Event 
                                    // Môžete pridať vlastné vykreslenie pre Intermediate Event
                                break;
                            case 2: // Basic Event
                                    // Pre Basic Event použijeme bitmapu s miernym scalingom
                                DrawEventBitmap(g, r, "pic\\events\\eventBasic.png", verticalOffset: 0, scaleFactorWidth: 0.4f, scaleFactorHeight: 0.45f);
                                break;
                            case 3: // House Event
                                DrawEventBitmap(g, r, "pic\\events\\eventHouse.png");
                                break;
                            case 4: // Undeveloped Event 
                                DrawEventBitmap(g, r, "pic\\events\\eventUndeveloped.png");
                                break;
                            case 5: // Conditioning Event 
                                    // Pre Conditioning Event použijeme odlišný scaling, aby vznikol oválny tvar
                                DrawEventBitmap(g, r, "pic\\events\\eventConditioning.png", verticalOffset: 0, scaleFactorWidth: 0.4f, scaleFactorHeight: 0.35f);
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
                        string nameText = evt.Name;
                        Font nameFont = FindFittingFont(g, nameText, middleRect);
                        SizeF nameSize = g.MeasureString(nameText, nameFont);

                        // Overíme, či sa text zmestí do obdlžníka
                        if (nameSize.Width <= middleRect.Width && nameSize.Height <= middleRect.Height)
                        {
                            PointF namePos = new PointF(
                                middleRect.X + (middleRect.Width - nameSize.Width) / 2,
                                middleRect.Y + (middleRect.Height - nameSize.Height) / 2
                            );
                            g.DrawString(nameText, nameFont, Brushes.Black, namePos);
                        }

                        // Vykreslenie frekvencie v spodnej časti
                        string freqText = (evt.Frequency == 0) ? "" :
                                  (evt.Frequency < 0.001) ? evt.Frequency.ToString("0.0000E+0") :
                                  evt.Frequency.ToString("F6");

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
        }
    }

    private Font FindFittingFont(Graphics g, string text, Rectangle rect)// Methode for fitting text into to event visual borders
    {
        float maxFontSize = 10f;
        float minFontSize = 6f;
        Font font = null;

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
            foreach (var parentEvent in EngineLogic.FTAStructure)
            {
                var parent = parentEvent.Value;

                if (parent.IsHidden || (EngineLogic.FTAStructure.ContainsKey(parent.Parent) && EngineLogic.FTAStructure[parent.Parent].IsHidden))
                    continue;

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
                        var child = EngineLogic.GetItem(childGuid);

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

                    if (MainAppSettings.Current.Technicalgates)
                    {
                        if (parent.GateType == 1)
                        {
                            // OR Gate 
                            g.DrawArc(linePen, parentCenter.X - 20, gateY - 10, 40, 60, 0, -180);
                            g.DrawArc(linePen, parentCenter.X - 20, gateY + 10, 40, 20, 0, -180);
                        }
                        else if (parent.GateType == 2)
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
                        switch (parent.GateType)
                        {
                            case 1: // OR Gate
                                DrawGateBitmap(g, parentRect, "pic\\gates\\gateOr.png", verticalOffset: 10);
                                break;
                            case 2: // AND Gate
                                DrawGateBitmap(g, parentRect, "pic\\gates\\gateAnd.png", verticalOffset: 10);
                                break;
                            case 3: // Not Gate
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
                                break;
                            default:
                                break;

                        }
                    }



                }
            }
        }
    }

    public bool IsDraggingTree = false;
    private Point LastMousePosition;
    public bool Mouse_SelectEvent(Point mouseCoordinates)
    {
        int X = 0;
        int Y = 0;
        PixelToRealPosition(mouseCoordinates, ref X, ref Y);

        // Check if the SHIFT key is pressed.
        bool shiftDown = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;


        // Find the event that was clicked.
        FTAitem clickedEvent = null;
        foreach (var evt in EngineLogic.FTAStructure.Values)
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
                    foreach (var evt in EngineLogic.FTAStructure.Values)
                    {
                        evt.IsSelected = false;
                    }
                    if (EngineLogic.SelectedEvents != null)
                    {
                        EngineLogic.SelectedEvents.Clear();
                    }
                    clickedEvent.IsSelected = true;
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
                foreach (var evt in EngineLogic.FTAStructure.Values)
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
    private bool IsTopHiddenParent(FTAitem evt)
    {
        if (!evt.IsHidden)
            return false;

        if (evt.Parent != null && EngineLogic.FTAStructure.TryGetValue(evt.Parent, out FTAitem parent) && parent.IsHidden)
            return false;

        foreach (var childGuid in evt.Children)
        {
            if (EngineLogic.FTAStructure.TryGetValue(childGuid, out FTAitem child) && !child.IsHidden)
                return false;
        }

        return true;
    }



    private GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)//Methode for rounding corners of events
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
    private Point _lastDragPosition;

    public void Mouse_DragEvent(Point mouseCoordinates)
    {
        if (SelectedEventDrag && EngineLogic.SelectedEvents.Count > 0)
        {
            int deltaX = mouseCoordinates.X - _lastDragPosition.X;
            int deltaY = mouseCoordinates.Y - _lastDragPosition.Y;

            foreach (var evt in EngineLogic.SelectedEvents)
            {
                if (IsTopHiddenParent(evt))
                {
                    List<FTAitem> subtreeItems = GetSubtreeItems(evt);

                    foreach (var item in subtreeItems)
                    {
                        item.X += deltaX;
                        item.Y += deltaY;
                    }
                }
                else
                {

                    evt.X += deltaX;
                    evt.Y += deltaY;
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

    public Rectangle RealPositionToPixel(Rectangle r)
    {
        Rectangle outR = new Rectangle();
        outR.X = (int)(r.X * GlobalZoom + offsetX);
        outR.Y = (int)(r.Y * GlobalZoom + offsetY);

        outR.Width = (int)(r.Width * GlobalZoom);
        outR.Height = (int)(r.Height * GlobalZoom);

        return outR;
    }


    void PixelToRealPosition(Point p, ref int xreal, ref int yreal)
    {
        xreal = (int)((p.X - offsetX) / GlobalZoom);
        yreal = (int)((p.Y - offsetY) / GlobalZoom);
    }

    private void DrawEventBitmap(Graphics g, Rectangle r, string imageFilename, int verticalOffset = 0, float scaleFactorWidth = 0.5f, float scaleFactorHeight = 0.5f, Pen fallbackPen = null)
    {
        // Calculate the image dimensions based on the rectangle dimensions using separate scale factors.
        int imageWidth = (int)(r.Width * scaleFactorWidth);
        int imageHeight = (int)(r.Height * scaleFactorHeight);

        // Center the image horizontally and position it below the rectangle.
        int imageX = r.X + (r.Width - imageWidth) / 2;
        int imageY = r.Y + r.Height + verticalOffset;
        Rectangle imageRect = new Rectangle(imageX, imageY, imageWidth, imageHeight);

        // Build the absolute path to the image.
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string picPath = System.IO.Path.GetDirectoryName(exePath);
        string imagePath = System.IO.Path.Combine(picPath, imageFilename);

        if (System.IO.File.Exists(imagePath))
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                g.DrawImage(bitmap, imageRect);
            }
        }
        else
        {
            // If the image does not exist, draw a fallback red ellipse.
            using (Brush redBrush = new SolidBrush(Color.Red))
            {
                g.FillEllipse(redBrush, imageRect);
            }
            g.DrawEllipse(fallbackPen ?? Pens.Black, imageRect);
        }
    }

    private void DrawGateBitmap(Graphics g, Rectangle r, string imageFilename, int verticalOffset = 0, float scaleFactorWidth = 0.6f, float scaleFactorHeight = 0.5f, Pen fallbackPen = null)
    {
        // Vypočítame rozmery bitmapy podľa veľkosti brány
        int imageWidth = (int)(r.Width) / 3;
        int imageHeight = (int)(r.Height) / 3;

        // Umiestnenie bitmapy - centrovanie horizontálne, vertikálne pod bránou
        int imageX = r.X + (r.Width - imageWidth) / 2;
        int imageY = r.Y + r.Height + verticalOffset;
        Rectangle imageRect = new Rectangle(imageX, imageY, imageWidth, imageHeight);

        // Získanie cesty k obrázku
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string picPath = System.IO.Path.GetDirectoryName(exePath);
        string imagePath = System.IO.Path.Combine(picPath, imageFilename);

        if (System.IO.File.Exists(imagePath))
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                g.DrawImage(bitmap, imageRect);
            }
        }
        else
        {
            // Ak sa obrázok nenájde, nakreslíme červený kríž ako fallback
            using (Brush redBrush = new SolidBrush(Color.Red))
            {
                g.FillRectangle(redBrush, imageRect);
            }
            g.DrawRectangle(fallbackPen ?? Pens.Black, imageRect);
        }
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
            if (EngineLogic.FTAStructure.TryGetValue(childGuid, out FTAitem child))
            {
                subtree.AddRange(GetSubtreeItems(child));
            }
        }

        return subtree;
    }
}
