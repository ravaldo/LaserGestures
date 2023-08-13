using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace LaserGestures {
    public class OutputPanel : PictureBox {

        public OutputPanel() {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);
            BackColor = System.Drawing.Color.White;
            Name = "outputPanel";
            SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs pe) {
            //only line needed for anti-aliasing to be turned on
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //set the anti-aliasing quality
            pe.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //draw the contents
            base.OnPaint(pe);
        }

    }
}
