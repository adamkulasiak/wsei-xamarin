using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class Standard
    {
        public string Name { get; set; }
        public string Pollutant { get; set; }
        public double Limit { get; set; }
        public double Percent { get; set; }
    }
}
