using System.Runtime.Serialization;

namespace Platibus.SampleMessages
{
    [DataContract]
    public class Error
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "detail")]
        public string Detail { get; set; }
    }
}
