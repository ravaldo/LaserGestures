using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LaserGestures {
    public class Coalesce : GestureOps, IFilter {

        public Coalesce() { }

        public List<Point> Process(List<Point> input) {
            List<Point> points = new List<Point>(input);
            int i = 2;
            while (i < points.Count) {
                Point a = points[i - 2];
                Point b = points[i - 1];
                Point c = points[i];

                if (calculateHeading(a, b) == calculateHeading(b, c)) {
                    // the start/middle/end points all have the same heading. remove the middle point
                    points.RemoveAt(i - 1);
                    continue;	// avoid increment we test again from the same starting point
                } else {
                    // handle the case where units have different headings but are within 12 degrees of the boundary
                    double diff = Math.Abs(calculateDegrees(a, b) - calculateDegrees(b, c));
                    if (diff < 12) {
                        points.RemoveAt(i - 1);
                        continue;   
                    }
                }
                i++;
            }
            return points;
        }
    }
}
