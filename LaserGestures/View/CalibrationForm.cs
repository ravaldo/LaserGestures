using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;

enum Mode { THRESHOLD, KEYSTONE }

namespace LaserGestures {
    public partial class CalibrationForm : Form {

        private List<AForge.IntPoint> corners;
        private ISource source;
        private Bitmap canvas;
        private Mode mode;
        private bool finished = false;
        private bool autoAdjustThreshold = true;
        private FiltersSequence imageFilter;
        private QuadrilateralFinder qf = new QuadrilateralFinder();
        private Add addFilter;
        private int threshold = 150;

        //delegate for cross-thread calls
        private delegate void SetValue(int value);

        public CalibrationForm(ISource source) {
            InitializeComponent();
            this.source = source;

            keystoneRadioButton_CheckedChanged(this, null);
            canvas = new Bitmap(320, 240, PixelFormat.Format8bppIndexed);
            addFilter = new Add();
            imageFilter = new FiltersSequence();
            imageFilter.Add(new ExtractChannel(RGB.R));
            imageFilter.Add(new Threshold(threshold));

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);
        }

        private void resetCanvas() {
            lock (canvas)
                canvas = new Bitmap(320, 240, PixelFormat.Format8bppIndexed);
        }


        public void Run() {
            foreach (Bitmap bmp in source.Queue) {
                Bitmap redPeaks = imageFilter.Apply(bmp);

                if (mode == Mode.KEYSTONE) {
                    //iteratively paint our motion onto a binarized canvas
                    addFilter.OverlayImage = redPeaks;
                    addFilter.ApplyInPlace(canvas);
                    try {
                        corners = qf.ProcessImage(canvas);
                        //if (corners.Count == 4)
                        //    Debug.WriteLine("({0})  ({1})  ({2})  ({3})", corners[0], corners[1], corners[2], corners[3]);

                        //lock bitmap for drawing
                        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                        //draw bounding box
                        Drawing.Polygon(bmpData, corners, Color.Blue);
                        //draw a shape indicating which corner is first
                        Drawing.FillRectangle(bmpData, new Rectangle(corners[0].X, corners[0].Y, 5, 5), Color.Purple);

                        //extract a grayscale bitmap with the full range of red
                        ExtractChannel extractChannel = new ExtractChannel(RGB.R);
                        Bitmap temp = extractChannel.Apply(bmpData);

                        //add our canvas to this red channel, intensifying the areas of motion
                        addFilter.OverlayImage = canvas;
                        addFilter.ApplyInPlace(temp);

                        //replace this intensified red channel back into the current frame
                        ReplaceChannel replaceChannel = new ReplaceChannel(RGB.R, temp);
                        replaceChannel.ApplyInPlace(bmpData);
                        bmp.UnlockBits(bmpData);
                    } catch (ArgumentOutOfRangeException e) {
                        Debug.Write(e.Message);
                        Debug.WriteLine("QuadrilateralFinder failed to ProcessImage().", e.Message);
                    } catch (ArgumentException e) {
                        Debug.Write(e.Message);
                        Debug.WriteLine("Canvas is blank, cannot find corners.");
                    }
                    pictureBox1.Image = bmp;
                }

                if (mode == Mode.THRESHOLD) {
                    pictureBox1.Image = (Bitmap)redPeaks.Clone();
                    if (autoAdjustThreshold) {
                        //count the blobs in our filtered image and keep increasing the threshold
                        //until we get zero blobs. our background is now filtered

                        BlobCounter bc = new BlobCounter(redPeaks);
                        if (bc.GetObjectsInformation().Length > 0) {
                            if (threshold < 255)
                                adjustThreshold(threshold + 1);
                            else {
                                autoAdjustThreshold = false;
                                resetCanvas();
                                corners = null;
                            }
                        } else {
                            //reached a point where no more blobs found but increase by 10 for good measure
                            var i = threshold + 10;
                            if (i > 255)
                                i = 255;
                            adjustThreshold(threshold + 10);
                            autoAdjustThreshold = false;
                            resetCanvas();
                            corners = null;
                        }
                    }
                }

                if (this.InvokeRequired)
                    this.BeginInvoke(new MethodInvoker(Update));
                else
                    Update();
                /* The constructor for this Control was invoked inside the MainWindow class/thread.
                 * This Run() method however is operating in it's own unique thread meaning the Update() call is
                 * made from a thread that didn't create the object; which throws the .NET cross threading exception.
                 * Hence, the InvokeRequired check and call to the delegate which ensures the creating thread makes the call.
                 */

                redPeaks.Dispose();

                if (finished)
                    break;
            }
        }


        public List<AForge.IntPoint> Corners {
            get {
                if (corners == null || corners.Count != 4)
                    return null;
                else {
                    /*
                     * QuadrilateralFinder seems to designate the leftmost point as the starting point.
                     * In the case of the SW point being further left than the NW point then bottomLeft becomes
                     * the starting location and the eventual call to QuadrilateralTransformation produces an image
                     * rotated clockwise by 90 degrees. Need to account for this to ensure our keystoning is the right way up.
                     * */
                    if (corners[0].Y > corners[1].Y && corners[0].Y > corners[2].Y) {//first point lower than the next two
                        AForge.IntPoint p = corners[0];
                        corners.RemoveAt(0);
                        corners.Add(p);
                    }
                    return new List<AForge.IntPoint>(corners);
                }
            }
        }


        public int Threshold {
            get { return threshold; }
        }

        private void clearButton_Click(object sender, EventArgs e) {
            resetCanvas();
        }

        private void CalibrationForm_FormClosed(object sender, FormClosedEventArgs e) {
            finished = true;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            adjustThreshold(trackBar1.Value);
        }

        private void keystoneRadioButton_CheckedChanged(object sender, EventArgs e) {
            mode = keystoneRadioButton.Checked ? Mode.KEYSTONE : Mode.THRESHOLD;
            switch (mode) {
                case Mode.KEYSTONE:
                    clearButton.Visible = true;
                    trackBar1.Visible = false;
                    autoButton.Visible = false;
                    break;
                case Mode.THRESHOLD:
                    clearButton.Visible = false;
                    trackBar1.Visible = true;
                    autoButton.Visible = true;
                    break;
            }
        }

        private void autoThreshold_Click(object sender, EventArgs e) {
            adjustThreshold(trackBar1.Minimum);
            autoAdjustThreshold = true;
        }

        private void adjustThreshold(int value) {
            if (trackBar1.InvokeRequired) {
                SetValue callback = new SetValue(trackbar_callback);
                trackBar1.Invoke(callback, new object[] { value });
            } else
                trackBar1.Value = value;
            threshold = value;
            lock (imageFilter) {
                imageFilter.RemoveAt(1);
                imageFilter.Add(new Threshold(value));
            }
        }

        //required solely for when the Run() method needs to modify the trackbar control
        private void trackbar_callback(int value) {
            trackBar1.Value = value;
        }

        private void CalibrationForm_Load(object sender, EventArgs e) {

        }


    }
}
