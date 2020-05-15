using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class MeasurementEntity
    {
        public MeasurementEntity()
        {

        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CurrentId { get; set; }
    }
}
