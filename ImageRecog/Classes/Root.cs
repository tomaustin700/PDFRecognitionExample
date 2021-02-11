using System;
using System.Collections.Generic;
using System.Text;

namespace ImageRecog.Classes
{
    public class Root
    {
        public string status { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastUpdatedDateTime { get; set; }
        public AnalyzeResult analyzeResult { get; set; }
    }
}
