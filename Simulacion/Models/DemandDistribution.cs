using System;
using System.Collections.Generic;
using System.Text;

namespace Simulacion.Models
{
    public class DemandDistribution
    {
        public int Demand { get; set; }
        public double Probability { get; set; }
        public double LowerLimit { get; set; }
        public double UpperLimit { get; set; }
    }
}
