using System.Runtime.Serialization;

namespace Platibus.SampleMessages
{
    [DataContract]
    public class Request<TData>
    {
        [DataMember(Name = "data")]
        public TData Data { get; set; }
    }
}
