
namespace util
{
    public class EdgeModel
    {
        public EdgeModel(string id, string label, string inV, string outV)
        {
            this.id = id;
            this.label = label;
            this.inV = inV;
            this.outV = outV;
        }

        public string id { get; set; }
        public string label { get; set; }
        public string inV { get; set; }
        public string outV { get; set; }
    }
}