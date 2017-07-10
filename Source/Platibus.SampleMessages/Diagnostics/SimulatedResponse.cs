using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Diagnostics
{
    [DataContract]
    public class SimulatedResponse
    {
        /// <summary>
        /// Correlation ID returned in the response
        /// </summary>
        [DataMember(Name = "cid")]
        public string CorrelationId { get; set; }
    }
}
