using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LaserGestures {
    class ProximityTrim : GestureOps, IFilter {

        private double minProximity;


        public ProximityTrim(double d) {
            minProximity = d;
        }


        public List<Point> Process(List<Point> input) {
            List<Point> points = new List<Point>(input);

            int[] distances = getDistances(points);
            int i = 0;
            while (i < distances.Length) {
                int totalLength = calculateTotalLength(points);
                string s = calculateGesture(points);

                double proximity = (double)distances[i] / totalLength;
                //Debug.WriteLine("points: {0}   delta: {1} \t proximity: {2}", points.Count, distances[i], proximity);

                if (proximity <= minProximity || distances[i] < 5) {
                    //the two points are within the proximity variable.
                    if (i > 0 && i < distances.Length - 2) {
                        if (s[i - 1] == s[i + 1]) {
                            //heading of the preceeding&following unit are the same
                            //assumption: this unit should ideally be parallel too. try removing it.
                            points.RemoveAt(i);
                            distances = getDistances(points);
                            continue;
                        }
                    }
                    if (i < distances.Length - 1) {
                        if (distance(points[i], points[i + 2]) < distances[i] + distances[i + 1]) {
                            //the line would be shorter if it travelled in a straight line between here and the 2nd next point
                            //assumption: line has jitter, removing the next point would smooth it out
                            points.RemoveAt(i + 1);
                            distances = getDistances(points);
                            continue;
                        }
                    }
                    if (i < distances.Length - 1) {
                        //assume we would benefit by replacing the two points by a new average
                        int aveX = (int)(points[i].X + points[i + 1].X) / 2;
                        int aveY = (int)(points[i].Y + points[i + 1].Y) / 2;
                        points[i] = new Point(aveX, aveY);
                        points.RemoveAt(i + 1);
                        distances = getDistances(points);
                        continue;
                    }
                }
                i++;
            }
            //Debug.WriteLine("");
            return points;
        }


    }
}
