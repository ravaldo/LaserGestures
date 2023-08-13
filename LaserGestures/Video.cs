using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Concurrent;
using System.Threading;
using AForge.Video.FFMPEG;


namespace LaserGestures {
    public class Video : ISource {

        private VideoFileSource fileSource;
        private BlockingCollection<Bitmap> queue;
        private Bitmap bitmap = new Bitmap(320, 240); //needs to be instantiated the first lock
        private string filePath;

        public event NewFrameEventHandler NewFrame;

        public Video(string file) {
            try {
                filePath = file;
                fileSource = new VideoFileSource(filePath);
                fileSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                queue = new BlockingCollection<Bitmap>();
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs) {
            //Debug.WriteLine("Video | new frame event {0}   Thread: {1} ; {2}",
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
                lock(bitmap)
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
            get {
                Size size;
                if (!fileSource.IsRunning) {
                    fileSource.Start();
                    size = queue.Take().Size;
                    fileSource.Stop();
                    return size;
                } else {
                    lock (bitmap)
                        size = bitmap.Size;
                }
                return size;
            }
        }

        public double FrameRate {
            get { return 1000.0 / fileSource.FrameInterval; }
        }

        public bool IsRunning {
            get {
                for (int i = 0; i < 10; i++) {
                    if (fileSource.IsRunning && fileSource.FramesReceived > 0)
                        return true;
                    else
                        Thread.Sleep(100);
                }
                return false; // a second has passed and it's still not running
            }
        }

        public string Source {
            get { return filePath; }
        }

        #endregion Properties






        #region Methods

        public void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("Video | Dispose got called!");
                fileSource.SignalToStop();
                fileSource.WaitForStop();
                queue.Dispose();
            }
        }

        public void Start() {
            fileSource.Start();
        }

        #endregion Methods

    }//end of class
}
