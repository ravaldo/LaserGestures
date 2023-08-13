using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;


namespace LaserGestures {
    class PointFilterSequence : CollectionBase, IFilter {


        public PointFilterSequence() { }


        public PointFilterSequence(params IFilter[] filters) {
            InnerList.AddRange(filters);
        }


        public void Add(IFilter filter) {
            InnerList.Add(filter);
        }


        public List<Point> Process(List<Point> points) {
            int n = InnerList.Count;
            if (n == 0)
                throw new ApplicationException("No filters in the sequence.");

            List<Point> dst = points;
            List<Point> tmp;

            for (int i = 0; i < n; i++) {
                tmp = ((IFilter)InnerList[i]).Process(dst);
                dst = tmp;
            }
            return dst;
        }


        public List<List<Point>> Process(List<List<Point>> strokes) {
            List<List<Point>> filteredStrokes = new List<List<Point>>();
            foreach (List<Point> stroke in strokes)
                filteredStrokes.Add(Process(stroke));
            return filteredStrokes;
        }




    }
}
