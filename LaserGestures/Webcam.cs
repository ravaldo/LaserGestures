using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace LaserGestures {

    public class Webcam : ISource {

        private VideoCaptureDevice camera;
        private BlockingCollection<Bitmap> queue;
        private Bitmap bitmap = new Bitmap(320, 240); //needs to be instantiated the first lock
        private string source;

        public event NewFrameEventHandler NewFrame;

        public Webcam(String deviceMoniker, Size size) {
            source = deviceMoniker;
            camera = new VideoCaptureDevice(deviceMoniker);

            int maxFPS = 30;
            foreach (VideoCapabilities c in camera.VideoCapabilities) {
                if (c.FrameRate > maxFPS)
                    maxFPS = c.FrameRate;
            }

            camera.DesiredFrameRate = maxFPS;
            camera.DesiredFrameSize = size;
            camera.NewFrame += new NewFrameEventHandler(video_NewFrame);
            queue = new BlockingCollection<Bitmap>(100);
            //camera.DisplayPropertyPage(IntPtr.Zero);
        }


        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs) {
            //Debug.WriteLine("Webcam | new frame event {0}   Thread: {1} ; {2}",
            //        DateTime.Now.ToLongTimeString(), Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
            lock (bitmap) {
                bitmap = (Bitmap)eventArgs.Frame.Clone();
                queue.Add((Bitmap)bitmap.Clone());
                if (NewFrame != null)
                    NewFrame(this, new NewFrameEventArgs((Bitmap)bitmap.Clone()));    //invoke the event
            }
        }



        #region Properties

        public Bitmap Bitmap {
            get {
                Object o;
                lock (bitmap)
                    o = bitmap.Clone();
                return (Bitmap)o;
            }
        }

        public IEnumerable<Bitmap> Queue {
            get { return queue.GetConsumingEnumerable(); }
        }

        public int QueueCount {
            get { return queue.Count; }
        }

        public Size FrameSize {
            get { return camera.DesiredFrameSize; }
        }

        public double FrameRate {
            get { return camera.DesiredFrameRate; }
        }

        public bool IsRunning {
            get {
                for (int i = 0; i < 10; i++) {
                    if (camera.IsRunning && camera.FramesReceived > 0)
                        return true;
                    else
                        Thread.Sleep(100);
                }
                return false; // a second has passed and it's still not running
            }
        }

        public string Source {
            get { return source; }
        }

        #endregion Properties






        #region Methods

        public void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("Webcam | Dispose got called!");
                camera.SignalToStop();
                camera.WaitForStop();
                camera.Stop();
                queue.Dispose();
            }
        }

        public void Start() {
            camera.Start();
        }

        #endregion Methods

    }
}
