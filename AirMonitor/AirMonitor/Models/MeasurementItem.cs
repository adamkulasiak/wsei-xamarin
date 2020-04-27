using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AirMonitor.Models
{
    public class MeasurementItem
    {
        public string FromDateTime { get; set; }
        public string TillDateTime { get; set; }
        [JsonProperty(PropertyName = "values")]
        public List<Values> Values { get; set; }
        public List<Index> Indexes { get; set; }
        public List<Standard> Standards { get; set; }
    }
}