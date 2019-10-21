using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace util
{
    public class SensorValue
    {
        public string SensorId { get; set; }
        public double Value { get; set; }

        // Returns the text of the enum instead of number
        [JsonConverter(typeof(StringEnumConverter))]
        public SensorType Type { get; set; }
    }
}