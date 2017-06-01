using System.Runtime.Serialization;

namespace Platibus.SampleMessages
{
    [DataContract]
    public class RequestDocument
    {
        public static RequestDocument<TData> Containing<TData>(TData data)
        {
            return new RequestDocument<TData>(data);
        }
    }

    [DataContract]
    public class RequestDocument<TData>
    {
        [DataMember(Name = "data")]
        public TData Data { get; set; }

        public RequestDocument(TData data)
        {
            Data = data;
        }

        public RequestDocument()
        {
        }
    }
}
