using System;
using System.Collections.Generic;
using System.Text;

namespace ImageRecog.Classes
{
    public class Word
    {
        public List<double> boundingBox { get; set; }
        public string text { get; set; }
        public int confidence { get; set; }
    }
}
