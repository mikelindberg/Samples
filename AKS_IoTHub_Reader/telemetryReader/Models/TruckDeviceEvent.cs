using Microsoft.Azure.Documents.Spatial;
using System;

namespace telemetryReader.Models
{
    public class TruckDeviceEvent
    {
        public double latitude { get; set; }
        public double longitude {get;set;}
        public double temperature {get;set;}
    }
}
