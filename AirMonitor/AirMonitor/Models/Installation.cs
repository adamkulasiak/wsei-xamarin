using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class Installation
    {
        public Installation()
        {

        }
        public int Id { get; set; }
        public Address Address { get; set; }
        public Loc Location { get; set; }
        public double Elevation { get; set; }
        [JsonProperty(PropertyName = "airly")]
        public bool IsAirlyInstallation { get; set; }
        public Sponsor Sponsor { get; set; }
        public Measurement Measurement { get; set; }
    }
}
