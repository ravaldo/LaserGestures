using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

namespace LaserGestures {

    class Analyser {

        private ISource source;
        private OutputPanel outputPanel;
        private List<Point> points = new List<Point>();
        private List<List<Point>> strokes = new List<List<Point>>();
        private List<AForge.IntPoint> keystoneCorners = null;
        private Bitmap bmp;

        private QuadrilateralTransformation quadTransform;
        private FiltersSequence imageFilter;
        private PointFilterSequence pointFilter;
        private Deduce deduce;
        private Visualiser visualiser;

        private TimeSpan blankFrameTime;
        private DateTime lastFrameTime;

        private int threshold;
        private bool running = true;
        private bool transform = false;
        private Point origin = new Point(0, 0);

        private GestureSet gestureSet;
        private Gesture dynamicGesture = null;
        public event DynamicGestureHandler DynamicGesturePoint;
        public event DynamicGestureHandler DynamicGestureStart;
        public event EventHandler DynamicGestureStop;
        public event StaticGestureHandler StaticGesture;

        public Analyser(ISource source, int threshold, OutputPanel outputPanel, GestureSet gestureSet) {
            this.source = source;
            this.threshold = threshold;
            this.outputPanel = outputPanel;
            this.gestureSet = gestureSet;

            imageFilter = new FiltersSequence();
            imageFilter.Add(new ExtractChannel(AForge.Imaging.RGB.R));
            imageFilter.Add(new Threshold(threshold));

            Coalesce coalesce = new Coalesce();

            pointFilter = new PointFilterSequence();
            pointFilter.Add(coalesce);
            pointFilter.Add(new ProximityTrim(0.05));
            pointFilter.Add(coalesce);
            pointFilter.Add(new WeightedLengthAverage(0.33));
            pointFilter.Add(coalesce);

            deduce = new Deduce();
            visualiser = new Visualiser();
        }


        public Analyser(ISource source, int threshold, OutputPanel outputPanel, GestureSet gestureSet, List<AForge.IntPoint> keystoneCorners)
            : this(source, threshold, outputPanel, gestureSet) {
            this.keystoneCorners = keystoneCorners;
            Size size = source.FrameSize;
            if (keystoneCorners != null) {
                quadTransform = new QuadrilateralTransformation(keystoneCorners);
                quadTransform.AutomaticSizeCalculaton = false;
                quadTransform.NewWidth = size.Width;
                quadTransform.NewHeight = size.Height;
                transform = true;
            }
        }


        public void Run() {
            foreach (Bitmap img in source.Queue) {
                //Debug.WriteLine("Analyser | Image pulled from queue   Thread: {0} ", Thread.CurrentThread.ManagedThreadId);

                bool laserPointFound = false;
                int xPos = 0, yPos = 0;

                bmp = transform ? quadTransform.Apply(img) : img;

                //filter the image and get coordinates of laser point if found
                Bitmap redPeaks = imageFilter.Apply(bmp);
                bmp.Dispose();
                img.Dispose();
                BlobCounter bc = new BlobCounter(redPeaks);
                Blob[] blobs = bc.GetObjectsInformation();
                if (blobs.Length > 0) {
                    xPos = blobs[0].CenterOfGravity.X;
                    yPos = blobs[0].CenterOfGravity.Y;
                    laserPointFound = true;
                }

                //adjust timestamps and if point found either add to static stroke or fire dynamic event
                DateTime timestamp = DateTime.Now;
                if (laserPointFound) {
                    if (dynamicGesture != null)
                        DynamicGesturePoint(this, new DynamicGestureArgs(new Point(xPos, yPos), dynamicGesture.String, dynamicGesture.Action));
                    else
                        points.Add(new Point(xPos, yPos));
                    blankFrameTime = TimeSpan.Zero;
                } else {
                    blankFrameTime += timestamp.Subtract(lastFrameTime);
                }
                lastFrameTime = timestamp;

                //handle the case when enough blank time has elapsed to signal either
                //the start of a new static stroke or the start of a dynamic gesture
                if (blankFrameTime.Milliseconds >= 100 && blankFrameTime.Milliseconds < 500) {
                    if (points.Count >= 2) {
                        strokes.Add(new List<Point>(points));
                        //check if this first stroke is part of a dynamic gesture and enter dynamic mode if so
                        if (deduce.isDot(strokes[0]) && strokes.Count >= 1) {
                            var p = pointFilter.Process(points);
                            dynamicGesture = gestureSet.Contains(deduce.Gesture(p), Genre.DYNAMIC);
                            if (dynamicGesture != null)
                                DynamicGestureStart(this, new DynamicGestureArgs(points[0], dynamicGesture.String, dynamicGesture.Action));
                        }
                        points.Clear();
                    }
                }

                //handle the case when enough blank time has elapsed to signal the end of a gesture
                if (blankFrameTime.Milliseconds >= 500) {
                    if (dynamicGesture != null) {
                        DynamicGestureStop(this, new EventArgs());
                        dynamicGesture = null;
                        strokes.Clear();
                    }
                    if (strokes.Count > 0) {
                        Bitmap temp = new Bitmap(320, 240, PixelFormat.Format32bppArgb);
                        Graphics g = Graphics.FromImage(temp);

                        for (int i = 0; i < strokes.Count; i++) {
                            //draw the raw points of this stroke in light green
                            g.DrawImage(visualiser.drawPoints(strokes[i], new Point(0, 0), Color.LightGreen), origin);

                            //apply the point filter then redraw the stroke
                            strokes[i] = pointFilter.Process(strokes[i]);
                            //Color col = (i % 2 == 0) ? Color.Red : Color.Blue;
                            g.DrawImage(visualiser.drawPoints(strokes[i], new Point(0, 0), Color.Red), origin);
                        }

                        //draw a pretty version of our gesture
                        var gestureImage = visualiser.drawGesture(strokes);
                        g.DrawImage(gestureImage, origin);

                        //draw the gesture string
                        g.DrawString(deduce.Gesture(strokes), new Font("Arial", 14), new SolidBrush(Color.Red), origin);

                        var matchedGesture = gestureSet.Contains(deduce.Gesture(strokes), Genre.STATIC);
                        if (matchedGesture != null) {
                            StaticGesture(this, new StaticGestureArgs(matchedGesture.String));
                            //add a hint to our output saying if we matched by levenshtein
                            if (deduce.Gesture(strokes) != matchedGesture.String)
                                g.DrawString(matchedGesture.String + " fired", new Font("Arial", 14), new SolidBrush(Color.Blue), new Point(0, 20));
                        }

                        outputPanel.Image = temp;
                    }
                    strokes.Clear();
                    points.Clear();
                    blankFrameTime = TimeSpan.Zero;
                }

                //free up resources
                //bmp.Dispose();
                //img.Dispose();
                if (!running)
                    break;
            }//end of Queue loop
        }//end of Run()


        public void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("Analyser | Dispose got called!");
                running = false;
            }
        }

        public GestureSet GestureSet {
            set { gestureSet = value; }
        }

    }//end of Analyser
}