using System;
using System.Collections.Generic;
using System.Drawing;
using AForge.Video;

namespace LaserGestures {

    public interface ISource {

        Bitmap Bitmap { get; }
        IEnumerable<Bitmap> Queue { get; }
        int QueueCount { get; }
        Size FrameSize { get; }
        double FrameRate { get; }
        bool IsRunning { get; }
        string Source { get; }

        // check the events available in AForge.Video.FFMPEG & AForge.Video.DirectShow
        event NewFrameEventHandler NewFrame;
        // event PlayingFinishedEventHandler PlayingFinished;
        // event VideoSourceErrorEventHandler VideoSourceError;

        void Dispose(Boolean b);
        void Start();
    }
}