using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class MeasurementItemEntity
    {
        public MeasurementItemEntity()
        {

        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FromDateTime { get; set; }
        public string TillDateTime { get; set; }
        public string Values { get; set; }
        public string Indexes { get; set; }
        public string Standards { get; set; }
    }
}
