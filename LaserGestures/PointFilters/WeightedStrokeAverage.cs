using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LaserGestures {
    class WeightedAverage : GestureOps, IFilter {

        private double minPercentage;

        public WeightedAverage(double d) {
            minPercentage = d;
        }


        public List<Point> Process(List<Point> input) {
            List<Point> points = new List<Point>(input);

            int[] distances = getDistances(points);
            int i = 0;
            while (i < distances.Length) {
                int totalLength = calculateTotalLength(points);
                string s = calculateGesture(points);
                //Debug.WriteLine("totalLength: {0}  numDirections: {1}  averageDistance: {2:0.00}  thisDistance: {3}  proximity: {4:0.00}",
                //totalLength, s.Length, (double)totalLength / s.Length, distances[i], (double)distances[i] / totalLength);
                if (distances[i] < (double)totalLength / s.Length * minPercentage) {
                    int aveX = (int)(points[i].X + points[i + 1].X) / 2;
                    int aveY = (int)(points[i].Y + points[i + 1].Y) / 2;
                    points[i] = new Point(aveX, aveY);
                    points.RemoveAt(i + 1);
                    distances = getDistances(points);

                    continue;
                }
                i++;
            }
            //Debug.WriteLine("");
            return points;
        } 
    }
}
