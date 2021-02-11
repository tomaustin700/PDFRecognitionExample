using System;
using System.Collections.Generic;
using System.Text;

namespace ImageRecog.Classes
{
    public class Line
    {
        public List<double> boundingBox { get; set; }
        public string text { get; set; }
        public List<Word> words { get; set; }
    }
}
