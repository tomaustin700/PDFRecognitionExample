using System;
using System.Collections.Generic;
using System.Text;

namespace ImageRecog.Classes
{
    public class Region
    {
        public string boundingBox { get; set; }
        public List<Line> lines { get; set; }
    }
}
