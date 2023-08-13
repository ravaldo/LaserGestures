using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace LaserGestures {

    delegate void DynamicGestureAction(Point p);

    class Recogniser {
        private List<Point> lastPoints = new List<Point>(3);
        private Point startPoint;
        private Deduce deduce = new Deduce();
        private Analyser analyser;
        private OutputPanel outputPanel;

        private bool listening = true;
        private List<Socket> ahkClients;
        private TcpListener welcomeSocket;
        private int port = 5678;


        public Recogniser(Analyser analyser) {
            this.analyser = analyser;

            analyser.DynamicGesturePoint += new DynamicGestureHandler(dynamicPoint);
            analyser.DynamicGestureStart += new DynamicGestureHandler(dynamicStart);
            analyser.DynamicGestureStop += new EventHandler(dynamicStop);
            analyser.StaticGesture += new StaticGestureHandler(staticGesture);

            welcomeSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            welcomeSocket.Start();
            ahkClients = new List<Socket>();
            Task.Factory.StartNew(AcceptingClients);
        }


        public Recogniser(Analyser analyser, OutputPanel outputpanel)
            : this(analyser) {
            this.outputPanel = outputpanel;
        }

        private void staticGesture(object sender, StaticGestureArgs e) {
            Send(e.Gesture);
        }

        private void dynamicStart(object sender, DynamicGestureArgs e) {
            startPoint = e.Point;
        }

        private void dynamicStop(object sender, EventArgs e) {
            lastPoints.Clear();
        }

        private void dynamicPoint(object sender, DynamicGestureArgs e) {
            Point point = e.Point;
            var action = e.Action;

            if (action == DynamicAction.TURN) {
                //populate lastPoints on the first 3 iterations
                if (lastPoints.Count < 3) {
                    lastPoints.Insert(0, point);
                    return;
                }
                //with the list populated, use it as a queue
                lastPoints.RemoveAt(2);
                lastPoints.Insert(0, point);
                //skip calling Seek if the point hasn't moved by much
                double distance = deduce.Distance(lastPoints[0], lastPoints[1]);
                //Debug.WriteLine(distance);
                if (distance < 9)
                    return;
                //calculate our turning direction
                int i = deduce.Turn(lastPoints[2], lastPoints[1], lastPoints[0]);
                string msg = "";
                if (i < 0) {
                    msg = e.String + "#TURN-";
                }
                if (i > 0) {
                    msg = e.String + "#TURN+";
                }
                Send(msg);
                Display(msg);
            }

            if (action == DynamicAction.SEEK) {
                int distance = Math.Abs(startPoint.X - point.X);
                int direction = deduce.LeftRight(startPoint, point);
                char c = (direction < 0) ? '-' : '+';
                string msg = e.String + "#SEEK" + c + distance;
                if (Math.Abs(distance) > 30) {
                    Send(msg);
                    Display(msg);
                }
                //Debug.WriteLine(msg);
            }
        }


        public void Dispose(bool disposing) {
            if (disposing) {
                Debug.WriteLine("Recogniser | Dispose got called!");
                analyser.DynamicGestureStart -= new DynamicGestureHandler(dynamicStart);
                analyser.DynamicGesturePoint -= new DynamicGestureHandler(dynamicPoint);
                analyser.DynamicGestureStop -= new EventHandler(dynamicStop);
                analyser.StaticGesture -= new StaticGestureHandler(staticGesture);
                foreach (Socket s in ahkClients)
                    s.Close();
                ahkClients.Clear();
                welcomeSocket.Stop();
                listening = false;
            }
        }


        public void AcceptingClients() {
            while (listening) {
                try {
                    Socket newClient = welcomeSocket.AcceptSocket();
                    ahkClients.Add(newClient);                    
                } catch (SocketException) {
                    Debug.WriteLine("Recogniser | Error accepting new tcp clients.");
                }
            }
        }


        private void Send(string s) {
            try {
                for (int i = 0; i < ahkClients.Count; i++) {
                    if (!ahkClients[i].Connected) {
                        ahkClients.RemoveAt(i);
                        continue;
                    } else
                        ahkClients[i].Send(Encoding.UTF8.GetBytes(s), 0, s.Length, SocketFlags.None);
                }
            } catch (SocketException) {
                Debug.WriteLine("Recogniser | Error sending gesture to tcp clients.");
            }
        }


        private void Display(string msg) {
            Bitmap temp = new Bitmap(320, 240, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(temp);

            g.DrawString(msg, new Font("Arial", 14), new SolidBrush(Color.Red), new Point(0,0));
            outputPanel.Image = temp;
        }


    }
}
