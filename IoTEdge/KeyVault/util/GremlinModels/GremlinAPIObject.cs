using System.Collections.Generic;

namespace util
{
    public class GremlinAPIObject
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string inV { get; set; }
        public string outV { get; set; }
        public Dictionary<string, dynamic> properties { get; set; }
    }
}