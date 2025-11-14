using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_FTA.engine
{
    internal class BitmapDrawingEngine
    {
         
        private static BitmapDrawingEngine? _instance;

        
        private static readonly object _lock = new object();

         
        private BitmapDrawingEngine()
        {
             
        }

         
        public static BitmapDrawingEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BitmapDrawingEngine();
                        }
                    }
                }
                return _instance;
            }
        }

        public void DrawOrGate(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = (int)(rect.X + 0.35 * rect.Width);
            int y = (int)(rect.Y + rect.Height + 0.05 * rect.Height);

            int w = (int)(0.3 * rect.Width);
            int h1 = (int)(1.5 * w);
            int h2 = (int)(0.4 * w);
            int t = (int)(0.5 * (h1 - h2));

            Rectangle adjustedRect = new Rectangle(x, y, w, h1 - h2);

            using (LinearGradientBrush brush = new LinearGradientBrush(
              adjustedRect, Color.LightBlue, Color.DarkBlue, LinearGradientMode.ForwardDiagonal))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(x, y, w, h1, 0, -180);
                path.AddArc(x, y + t, w, h2, 0, -180);
                path.CloseFigure(); // uzavrie cestu
                g.FillPath(brush, path);
            }


            using (Pen linePen = new Pen(Color.Black, 1))
            {
                g.DrawArc(linePen, x, y, w, h1, 0, -180);
                g.DrawArc(linePen, x, y + t, w, h2, 0, -180);

            }

            float centerX = x + w / 2f;
            float centerY = y + (h2 / 2f) + (t / 4f);


            float textWidth = w * 0.6f;
            float textHeight = (h1 + h2 + t) * 0.45f;


            Font scalableFont = FitFont(g, "OR", new Font("Arial", 30, FontStyle.Bold),
                                        textWidth, textHeight);


            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (Brush textBrush = new SolidBrush(Color.WhiteSmoke))
            {
                g.DrawString("OR", scalableFont, textBrush,
                             new PointF(centerX, centerY), sf);
            }
        }

        public void DrawANDGate(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = (int)(rect.X + 0.35 * rect.Width);
            int y = (int)(rect.Y + rect.Height + 0.05 * rect.Height);

            int w = (int)(0.3 * rect.Width);
            int h1 = w;
            int h2 = (int)(0.4 * w);
            int t = (int)(0.5 * (h1 - h2));

            Rectangle adjustedRect = new Rectangle(x, y, w, h1);

            using (LinearGradientBrush brush = new LinearGradientBrush(
              adjustedRect, Color.LightBlue, Color.DarkBlue, LinearGradientMode.ForwardDiagonal))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(x, y, w, h1, 0, -180);

                Point p1 = new Point(x, y + (int)(h1 * 0.5));
                Point p2 = new Point(x, y + (int)(h1 * 0.7));
                path.AddLine(p1, p2);

                p1 = new Point(x, y + (int)(h1 * 0.7));
                p2 = new Point(x + w, y + (int)(h1 * 0.7));
                path.AddLine(p1, p2);

                p1 = new Point(x + w, y + (int)(h1 * 0.5));
                p2 = new Point(x + w, y + (int)(h1 * 0.7));
                path.AddLine(p1, p2);





                //path.CloseFigure(); // uzavrie cestu
                g.FillPath(brush, path);
            }



            using (Pen linePen = new Pen(Color.Black, 1))
            {
                g.DrawArc(linePen, x, y, w, h1, 0, -180);

                Point orLeft = new Point(x, y + (int)(h1 * 0.7));
                Point orRight = new Point(x + w, y + (int)(h1 * 0.7));
                g.DrawLine(linePen, orLeft, orRight);

                orLeft = new Point(x, y + (int)(h1 * 0.5));
                orRight = new Point(x, y + (int)(h1 * 0.7));
                g.DrawLine(linePen, orLeft, orRight);

                orLeft = new Point(x + w, y + (int)(h1 * 0.5));
                orRight = new Point(x + w, y + (int)(h1 * 0.7));
                g.DrawLine(linePen, orLeft, orRight);
            }

            float centerX = x + w / 2f;
            float centerY = y + (h2 / 2f) + (t / 1.5f);


            float textWidth = w * 0.8f;
            float textHeight = (h1 + h2 + t) * 0.65f;


            Font scalableFont = FitFont(g, "AND", new Font("Arial", 30, FontStyle.Bold),
                                        textWidth, textHeight);


            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            using (Brush textBrush = new SolidBrush(Color.WhiteSmoke))
            {
                g.DrawString("AND", scalableFont, textBrush,
                             new PointF(centerX, centerY), sf);
            }
        }

        public void DrawEventIcon(Graphics g, Rectangle rect, string IconType)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = (int)(rect.X + 0.4 * rect.Width);
            int y = (int)(rect.Y + rect.Height);

            int w = (int)(0.2 * rect.Width);
            int h1 = w;
            int h2 = (int)(0.4 * w);
            int t = (int)(0.5 * w);

            using (Pen linePen = new Pen(Color.Black, 1))
            {
                if (IconType.Contains("Basic"))
                    g.DrawEllipse(linePen, x, y, w, w);

                if (IconType.Contains("Undeveloped"))
                {

                    PointF[] pts = new PointF[]
                        {
                           new PointF(x+t, y ), // horný
                           new PointF(x + t+t, y+t), // pravý
                           new PointF(x+t, y + t+t), // dolný
                           new PointF(x , y+t)  // ľavý
                            };
                    g.DrawPolygon(Pens.Black, pts);

                }

                if (IconType.Contains("House"))
                {

                    PointF[] pts = new PointF[]
                        {
                           new PointF(x+t, y ), // horný                          
                           new PointF(x , y+t),  // ľavý
                           new PointF(x , y+t+t),  // ľavýdolný
                           new PointF(x+t+t , y+t+t),  // pravýdolný
                           new PointF(x + t+t, y+t)
                            };
                    g.DrawPolygon(Pens.Black, pts);

                }

                Rectangle adjustedRect = new Rectangle(x, y, w + 10, w + 10);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                 adjustedRect, Color.Red, Color.Black, LinearGradientMode.ForwardDiagonal))
                {
                    if (IconType.Contains("Basic"))
                        g.FillEllipse(brush, x, y, w, w);

                    if (IconType.Contains("Undeveloped"))
                    {
                        PointF[] pts = new PointF[]
                            {
                           new PointF(x+t, y ), // horný
                           new PointF(x + t+t, y+t), // pravý
                           new PointF(x+t, y + t+t), // dolný
                           new PointF(x , y+t)  // ľavý
                                };
                        g.FillPolygon(brush, pts);
                    }

                    if (IconType.Contains("House"))
                    {
                        PointF[] pts = new PointF[]
                            {
                              new PointF(x+t, y ), // horný                          
                           new PointF(x , y+t),  // ľavý
                           new PointF(x , y+t+t),  // ľavýdolný
                           new PointF(x+t+t , y+t+t),  // pravýdolný
                           new PointF(x + t+t, y+t)
                                };
                        g.FillPolygon(brush, pts);
                    }

                    if (IconType.Contains("Transfer"))
                    {
                        PointF[] pts = new PointF[]
                            {
                              new PointF(x+t, y ), // horný                                                   
                              new PointF(x , y+t+t),  // ľavýdolný
                              new PointF(x+t+t , y+t+t),  // pravýdolný
                         
                                };
                        g.FillPolygon(brush, pts);
                    }

                }

                if (IconType.Contains("Transfer"))
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                adjustedRect, Color.Green, Color.DarkGreen, LinearGradientMode.ForwardDiagonal))
                    {
                        PointF[] pts = new PointF[]
                        {
                              new PointF(x+t, y ), // horný                                                   
                              new PointF(x , y+t+t),  // ľavýdolný
                              new PointF(x+t+t , y+t+t),  // pravýdolný

                            };
                        g.FillPolygon(brush, pts);
                    }
                }

            }

        }

        private Font FitFont(Graphics g, string text, Font baseFont, float maxWidth, float maxHeight)
        {
            float size = baseFont.Size;

            while (true)
            {
                using (Font testFont = new Font(baseFont.FontFamily, size, baseFont.Style))
                {
                    SizeF textSize = g.MeasureString(text, testFont);

                    if (textSize.Width <= maxWidth && textSize.Height <= maxHeight)
                        return new Font(baseFont.FontFamily, size, baseFont.Style);

                    size -= 0.5f;
                    if (size < 2f)
                        return new Font(baseFont.FontFamily, 2f, baseFont.Style);
                }
            }
        }
    }

}
