using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Widgets
{
    [DataContract]
    public class WidgetAttributes
    {
        [DataMember(Name = "part", EmitDefaultValue = false)]
        public string PartNumber { get; set; }

        [DataMember(Name = "descr", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "length", EmitDefaultValue = false)]
        public float? Length { get; set; }

        [DataMember(Name = "width", EmitDefaultValue = false)]
        public float? Width { get; set; }

        [DataMember(Name = "height", EmitDefaultValue = false)]
        public float? Height { get; set; }
    }
}
