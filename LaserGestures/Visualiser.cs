using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace LaserGestures {
    class Visualiser : GestureOps {

        public Visualiser() { }


        private Bitmap drawCoordinates(List<Point> points, Color col) {
            Bitmap temp = new Bitmap(320, 240, PixelFormat.Format32bppArgb);
            temp.MakeTransparent(Color.White);

            Graphics g = Graphics.FromImage(temp);
            Pen pen = new Pen(Color.Pink, 1);
            SolidBrush drawBrush = new SolidBrush(col);

            String s = "";
            foreach (Point p in points)
                s += System.String.Format("{0:0##}, {1:0##} \n", p.X, p.Y);

            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            drawBrush = new SolidBrush(Color.FromArgb(128, col));
            Font drawFont = new Font("Arial", 12);
            g.DrawString(s, drawFont, drawBrush, new Point(230, 15));
            return temp;
        }


        public Bitmap drawPoints(List<Point> points, Point offset, Color col) {
            Bitmap temp = new Bitmap(320, 240, PixelFormat.Format32bppArgb);
            temp.MakeTransparent(Color.White);

            Graphics g = Graphics.FromImage(temp);
            Pen pen = new Pen(col, 1);
            SolidBrush drawBrush = new SolidBrush(col);

            points[0].Offset(offset);
            g.FillEllipse(drawBrush, points[0].X - 5, points[0].Y - 5, 10, 10);

            for (int i = 1; i < points.Count; i++) {
                points[i].Offset(offset);
                g.FillEllipse(drawBrush, points[i].X - 3, points[i].Y - 3, 6, 6);
                g.DrawLine(pen, points[i - 1], points[i]);
            }

            return temp;
        }


        public Bitmap drawGesture(List<List<Point>> strokes) {

            Pen pen = new Pen(Color.FromArgb(90, Color.Gray), 6);
            pen.StartCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            List<Bitmap> pics = new List<Bitmap>();

            foreach (List<Point> stroke in strokes) {

                Point topLeft = new Point(450, 450);
                Point bottomRight = new Point(550, 550);
                int x = 500;
                int y = 500;
                int unit = 50;

                String gesture = calculateGesture(stroke);
                Bitmap bmp = new Bitmap(1000, 1000, PixelFormat.Format32bppArgb);
                bmp.MakeTransparent(Color.White);
                Graphics g = Graphics.FromImage(bmp);

                Point[] drawingPoints = new Point[gesture.Length + 1];
                drawingPoints[0] = new Point(x, y);

                for (int i = 0; i < gesture.Length; i++) {
                    Char c = gesture[i];

                    //remember 0,0 is top left, not bottom left, hence the "y -= ..."
                    switch (c) {
                        case 'U': y -= +unit;
                            break;
                        case 'D': y -= -unit;
                            break;
                        case 'L': x += -unit;
                            break;
                        case 'R': x += +unit;
                            break;
                        case '1': x += -unit;
                            y -= -unit;
                            break;
                        case '3': x += +unit;
                            y -= -unit;
                            break;
                        case '7': x += -unit;
                            y -= +unit;
                            break;
                        case '9': x += +unit;
                            y -= +unit;
                            break;
                    }
                    drawingPoints[i + 1] = new Point(x, y);
                    if (drawingPoints[i + 1].X <= topLeft.X)
                        topLeft.X -= 50;
                    if (drawingPoints[i + 1].Y <= topLeft.Y)
                        topLeft.Y -= 50;
                    if (drawingPoints[i + 1].X >= bottomRight.X)
                        bottomRight.X += 50;
                    if (drawingPoints[i + 1].Y >= bottomRight.Y)
                        bottomRight.Y += 50;
                }

                if (drawingPoints.Length > 1)
                    g.DrawLines(pen, drawingPoints);

                //bring our cropping margins in closer
                topLeft.Offset(25, 25);
                bottomRight.Offset(-25, -25);
                Rectangle cropArea = new Rectangle(topLeft, new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
                Bitmap cropped = bmp.Clone(cropArea, PixelFormat.Format32bppArgb);
                pics.Add(cropped);
            }//end of foreach


            //pics now holds individual bitmaps for each stroke.
            //we iteratively squeeze them onto a single bitmap
            Bitmap img = new Bitmap(320, 240, PixelFormat.Format32bppArgb);
            img.MakeTransparent(Color.White);
            Graphics g1 = Graphics.FromImage(img);

            int division = 320 / pics.Count;
            for (int i = 0; i < pics.Count; i++)
                g1.DrawImage(pics[i], new Rectangle(i * division, 0, i+1 * division, 240)); //resize

            return img;

        }//end of drawGesture



    }//end of Visualiser
}
