using System.Collections.Generic;

namespace util
{
    public class VertexModel
    {
        public VertexModel(string id, string label, List<EdgeModel> edges, List<PropertyModel> properties, string partitionKey)
        {
            this.id = id;
            this.label = label;
            this.edges = edges == null ? new List<EdgeModel>() : edges;
            this.properties = properties == null ? new List<PropertyModel>() : properties;
            this.partitionKey = partitionKey;
        }

        public string id { get; set; }
        public string label { get; set; }
        public List<EdgeModel> edges { get; set; }
        public List<PropertyModel> properties { get; set; }
        public string partitionKey { get; set; }
    }
}