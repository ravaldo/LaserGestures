using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace LaserGestures {
    public abstract class GestureOps {


        protected int distance(Point a, Point b) {
            int dX = a.X - b.X;
            int dY = a.Y - b.Y;
            return (int)Math.Sqrt(dX * dX + dY * dY);
        }


        protected int[] getDistances(List<Point> points) {
            int[] distances = new int[points.Count - 1];
            for (int i = 0; i < points.Count - 1; i++)
                distances[i] = distance(points[i + 1], points[i]);
            return distances;
        }


        protected String calculateGesture(List<Point> points) {
            String s = "";
            for (int i = 1; i < points.Count; i++)
                s += calculateHeading(points[i - 1], points[i]);
            return s;
        }


        protected string calculateGesture(List<List<Point>> strokes) {
            String s = "";
            foreach (List<Point> stroke in strokes)
                s += calculateGesture(stroke) + '-';
            return s.Trim('-');
        }


        protected int calculateTotalLength(List<Point> points) {
            int total = 0;
            for (int i = 1; i < points.Count; i++)
                total += distance(points[i], points[i - 1]);
            return total;
        }


        protected double calculateDegrees(Point p1, Point p2) {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double rad = Math.Atan2(dy, dx);
            double degree = rad * (180 / Math.PI);  // 0 deg = 3 o'clock, -90 deg = 12 o'clock
            degree = (degree + 450) % 360;          // map 0 degrees to 12 o'clock and normalise (-180 to 180) to (0 to 360)
            return degree;
        }


        protected Char calculateHeading(Point p1, Point p2) {
            double degree = calculateDegrees(p1, p2);
            double boundary = 22.5;

            if (degree >= boundary * 1 && degree < boundary * 3)
                return '9';
            if (degree >= boundary * 3 && degree < boundary * 5)
                return 'R';
            if (degree >= boundary * 5 && degree < boundary * 7)
                return '3';
            if (degree >= boundary * 7 && degree < boundary * 9)
                return 'D';
            if (degree >= boundary * 9 && degree < boundary * 11)
                return '1';
            if (degree >= boundary * 11 && degree < boundary * 13)
                return 'L';
            if (degree >= boundary * 13 && degree < boundary * 15)
                return '7';
            if (degree >= boundary * 15 || degree < boundary * 1)
                return 'U';
            return '?';
        }
    }
}
