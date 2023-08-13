using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;


namespace LaserGestures {
    public class InputPanel : Panel {

        ISource source;
        Bitmap image = null;

        Pen pen = new Pen(Color.Red);
        Font drawFont = new Font("Arial", 12);
        SolidBrush drawBrush = new SolidBrush(Color.White);
        Point[] polygon;


        #region FPS calculator

        DateTime lastFrame = DateTime.Now;
        double fps;
        const int SAMPLES = 128;
        int tickindex = 0;
        int ticksum = 0;
        int[] ticklist = new int[SAMPLES];

        double CalcAverageTick(int newtick) {// uses a circular buffer to maintain 100 samples
            ticksum -= ticklist[tickindex];  // subtract value falling off
            ticksum += newtick;              // add new value
            ticklist[tickindex] = newtick;   // save new value so it can be subtracted later
            if (++tickindex == SAMPLES)      // inc buffer index
                tickindex = 0;
            return ((double)ticksum / SAMPLES);  // return average
        }

        #endregion FPS calculator


        public InputPanel(ISource source) {
            this.source = source;
            Size = source.FrameSize;
            source.NewFrame += new NewFrameEventHandler(newFrame);

            SetVisibleCore(true);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);
        }


        protected override void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("InputPanel | Dispose got called!");
                source.NewFrame -= new NewFrameEventHandler(newFrame);
            }
            base.Dispose(disposing);
        }


        protected override void OnPaint(PaintEventArgs e) {
            //Debug.WriteLine("InputPanel | I'm being painted!");
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle rc = this.ClientRectangle;

            lock (this) {
                g.DrawRectangle(pen, rc.X, rc.Y, rc.Width - 1, rc.Height - 1);
                if (image != null)
                    g.DrawImage(image, rc.X + 1, rc.Y + 1, rc.Width - 2, rc.Height - 2);
                if (polygon != null)
                    g.DrawLines(pen, polygon);
                String s = String.Format("{0:00.0} fps       queue: {1}", fps, source.QueueCount);
                g.DrawString(s, drawFont, drawBrush, new PointF(5, 5));
            }
        }


        private void newFrame(object sender, NewFrameEventArgs eventArgs) {
            lock (this) {
                image = (Bitmap)eventArgs.Frame.Clone();
            }

            DateTime thisFrame = DateTime.Now;
            int delta = thisFrame.Subtract(lastFrame).Milliseconds;
            if (delta > 0)
                fps = 1000 / CalcAverageTick(delta);
            lastFrame = thisFrame;
            this.Invalidate();
        }


        public List<AForge.IntPoint> Corners {
            set {
                lock (this) {
                    if (value == null)
                        polygon = null;
                    else {
                        polygon = new Point[5];
                        for (int i = 0; i < value.Count; i++)
                            polygon[i] = new Point(value[i].X, value[i].Y);
                        polygon[4] = polygon[0];// new Point(value[0].X, value[0].Y);
                    }
                }
            }
        }


    }

}

