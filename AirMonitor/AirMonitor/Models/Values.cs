using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class Values
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }
    }
}
