using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using AForge.Math.Geometry;


namespace LaserGestures {
    class Deduce : GestureOps {


        public Deduce() { }

        public string Gesture(List<Point> stroke) {
            return calculateGesture(stroke);
        }

        public string Gesture(List<List<Point>> strokes) {
            return calculateGesture(strokes);
        }

        public double Distance(Point a, Point b) {
            return distance(a, b);
        }

        public bool isDot(List<Point> points) {
            int range = 20;
            if (getDistances(points).Sum() / points.Count < range
                && distance(points[0], points[points.Count - 1]) < range)
                return true;
            else
                return false;
        }


        public int LeftRight(Point start, Point end) {
            if (end.X == start.X)
                return 0;
            return (end.X - start.X > 0) ? 1 : -1;
        }


        /* calculate whether the second of two consecutive vectors
         * heads to the left or the right of the first.
         */
        public int Turn(Point a, Point b, Point c) {
            double initialHeading = calculateDegrees(a, b);
            double finalHeading = calculateDegrees(b, c);
            double subtractedHeading = (finalHeading-initialHeading + 180) % 360;
            if (subtractedHeading < 170)
                return -1;
            if (subtractedHeading > 190)
                return 1;
            return 0;
        }



    }
}
