using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace LaserGestures {
    class WeightedLengthAverage : GestureOps, IFilter {

        private double minPercentage;

        public WeightedLengthAverage(double d) {
            minPercentage = d;
        }

        public List<Point> Process(List<Point> input) {
            List<Point> points = new List<Point>(input);

            int[] distances = getDistances(points);

            //take our biggest edge's length and consider it to be a standard unit for this gesture
            int biggest = distances.Max();

            //test if we have any edges that fall below our threshold
            while (distances.Min() < (biggest * minPercentage) && points.Count > 2) {
                int i = Array.IndexOf(distances, distances.Min());
                //Debug.WriteLine("units: {0}  biggest: {1}  i: {2}", distances.Length, biggest, i);

                //if we're on the very first edge, then chop it off by removing the first point
                if (i == 0) {
                    points.RemoveAt(i);
                    distances = getDistances(points);
                    continue;
                }

                //if we're on the very last edge, then chop it off by removing the last point
                if (i == distances.Length - 1) {
                    points.RemoveAt(i + 1);
                    distances = getDistances(points);
                    continue;
                }

                //else, we're somewhere in between and we're going to remove a point to merge this edge
                //with its neighbour, so it's best to merge it with whichever neighbour is shorter
                int nextShortest = int.MaxValue;
                int? pointToRemove = null;

                //check preceeding edge if available
                if (i >= 1) {
                    if (distances[i - 1] < nextShortest) {
                        nextShortest = distances[i - 1];
                        pointToRemove = i;
                    }
                }

                //check succeeding edge if available
                if (i < distances.Length - 1) {
                    if (distances[i + 1] < nextShortest) {
                        nextShortest = distances[i + 1];
                        pointToRemove = i + 1;
                    }
                }

                if (pointToRemove != null) {
                    points.RemoveAt((int)pointToRemove);
                    distances = getDistances(points);
                    continue;
                }
            }//end of while
            return points;
        }
    }
}
