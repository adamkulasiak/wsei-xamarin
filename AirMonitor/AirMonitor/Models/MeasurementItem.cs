using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AirMonitor.Models
{
    public class MeasurementItem
    {
        public MeasurementItem()
        {

        }
        public string FromDateTime { get; set; }
        public string TillDateTime { get; set; }
        public List<MeasurementValue> Values { get; set; }
        public List<AirQualityIndex> Indexes { get; set; }
        public List<AirQualityStandard> Standards { get; set; }
    }
}