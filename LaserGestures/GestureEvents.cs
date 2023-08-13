using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LaserGestures {


    public delegate void DynamicGestureHandler(object sender, DynamicGestureArgs eventArgs);
    public delegate void StaticGestureHandler(object sender, StaticGestureArgs eventArgs);

    public class DynamicGestureArgs : EventArgs {
        private Point point;
        private string gestureString;
        private DynamicAction action;

        public DynamicGestureArgs(Point point, string gestureString, DynamicAction action) {
            this.point = point;
            this.gestureString = gestureString;
            this.action = action;
        }

        public System.Drawing.Point Point {
            get { return point; }
        }
        public string String{
            get { return gestureString; }
        }
        public DynamicAction Action {
            get { return action; }
        }

    }

    public class StaticGestureArgs : EventArgs {
        private string gestureString;

        public StaticGestureArgs(string gestureString) {
            this.gestureString = gestureString;
        }

        public string Gesture {
            get { return gestureString; }
        }
    }



}
