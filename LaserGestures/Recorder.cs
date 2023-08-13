using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;
using AForge.Video.FFMPEG;
using AForge.Video;
using System.Diagnostics;


namespace LaserGestures {
    class Recorder {

        private Bitmap bitmap = new Bitmap(320, 240);
        private string filePath;
        private VideoFileWriter writer;
        private ISource source;


        public Recorder(ISource source, String file) {
            try {
                this.source = source;
                filePath = file;
                writer = new VideoFileWriter();
                writer.Open(file, source.FrameSize.Width, source.FrameSize.Height, (int)source.FrameRate, VideoCodec.Default);
                source.NewFrame += new NewFrameEventHandler(video_NewFrame);
            } catch (Exception e) {
                Debug.WriteLine("Recorder  |  failed in the contructor. \n {0}", e.Message);
            }
        }


        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs) {
            lock (writer) {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, 320, 240), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bitmap.UnlockBits(bmpData);
                writer.WriteVideoFrame(bitmap);
            }
        }


        public string Source {
            get { return filePath; }
        }


        public void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("Recorder | Dispose got called!");
                source.NewFrame -= new NewFrameEventHandler(video_NewFrame);
                writer.Close();
            }
        }


        public bool IsRunning {
            get { return (writer.IsOpen)? true : false; }
        }

    }//end of class
}
