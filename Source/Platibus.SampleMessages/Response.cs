using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Platibus.SampleMessages
{
    public class Response
    {
        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public IList<Error> Errors { get; set; }

        public static Response<TData> Containing<TData>(TData data)
        {
            return new Response<TData>(data);
        }
    }

    [DataContract]
    public class Response<TData>
    {
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public TData Data { get; set; }

        public Response(TData data)
        {
            Data = data;
        }

        public Response()
        {
        }
    }
}
