using System;
using System.Collections.Generic;
using System.Text;

namespace ImageRecog.Classes
{
    public class ReadResult
    {
        public int page { get; set; }
        public int angle { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public string unit { get; set; }
        public List<Line> lines { get; set; }
    }
}
