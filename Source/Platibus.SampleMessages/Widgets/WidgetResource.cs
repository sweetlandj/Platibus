using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetResource
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "type")]
        public string Type
        {
            get; private set;
        }

        [DataMember(Name = "attributes")]
        public WidgetAttributes Attributes { get; set; }

        public WidgetResource()
        {
            Type = "widgets";
        }
    }
}
