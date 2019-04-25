using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace telemetryApi.Config
{
    public class CosmosDBConfig
    {
        public string EndpointUri { get; set; }
        public string Key { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
