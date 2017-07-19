using System;
using System.Collections.Generic;
using System.Text;

namespace CustomVisionAccess
{
    public class PredictionResult
    {
        public string Target { get; set; }
        public string TagId { get; set; }
        public string Tag { get; set; }
        public double Probability { get; set; }
    }
}
