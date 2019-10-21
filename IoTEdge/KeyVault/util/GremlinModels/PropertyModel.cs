
namespace util
{
    public class PropertyModel
    {
        public PropertyModel(string id, string label, dynamic value)
        {
            this.id = id;
            this.label = label;
            this.value = value;
        }
        
        public string id { get; set; }
        public string label { get; set; }
        public dynamic value { get; set; }
    }
}