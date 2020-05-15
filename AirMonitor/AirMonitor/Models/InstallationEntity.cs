using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class InstallationEntity
    {
        public InstallationEntity()
        {

        }
        [PrimaryKey, AutoIncrement]
        public string Id { get; set; }
        public string Address { get; set; }
        public double Elevation { get; set; }
        [JsonProperty(PropertyName = "airly")]
        public bool IsAirlyInstallation { get; set; }
        public string Sponsor { get; set; }
        public string Measurement { get; set; }
    }
}
