using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Platibus.SampleMessages
{
    [DataContract]
    public class ResponseDocument
    {
        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public IList<Error> Errors { get; set; }

        public static ResponseDocument<TData> Containing<TData>(TData data)
        {
            return new ResponseDocument<TData>(data);
        }
    }

    [DataContract]
    public class ResponseDocument<TData>
    {
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public TData Data { get; set; }

        public ResponseDocument(TData data)
        {
            Data = data;
        }

        public ResponseDocument()
        {
        }
    }
}
