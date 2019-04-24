using Microsoft.Azure.Documents.Spatial;
using System;

namespace telemetryReader.Models
{
    public class TruckDeviceEvent
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double speed { get; set; }
        public string speed_unit { get; set; } = "mph";
        public double temperature { get; set; }
        public string temperature_unit { get; set; } = "f";
    }
}
