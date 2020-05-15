using System;
using System.Collections.Generic;
using System.Text;

namespace AirMonitor.Models
{
    public class Address
    {
        public Address()
        {

        }
        public string Street { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int? Number { get; set; }
        public string DisplayAddress1 { get; set; }
        public string DisplayAddress2 { get; set; }

        public string Description => $"{Street} {DisplayAddress1} {DisplayAddress2}";
    }
}
