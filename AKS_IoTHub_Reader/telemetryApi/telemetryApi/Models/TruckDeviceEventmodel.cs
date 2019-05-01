using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using telemetryModel;
using Microsoft.Azure.Documents.Spatial;

namespace telemetryApi.Models
{
    public class TruckDeviceEventModel : TruckDeviceEvent
    {
        public string id { get; set; }
        public string deviceid { get; set; }
        public DateTime enqueuedUtcTime { get; set; }                        

        public Point Location { get; set; }
    }
}
