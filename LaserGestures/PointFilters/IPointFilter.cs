using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LaserGestures {
    public interface IFilter {
        List<Point> Process(List<Point> points);
    }
}

