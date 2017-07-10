using System.Runtime.Serialization;

namespace Platibus.SampleMessages.Diagnostics
{
    [DataContract]
    public class SimulatedRequest
    {
        /// <summary>
        /// Correlation ID returned in the response
        /// </summary>
        [DataMember(Name = "cid")]
        public string CorrelationId { get; set; }

        /// <summary>
        /// The time it should take to process the simulated request
        /// </summary>
        [DataMember(Name = "time")]
        public int Time { get; set; }

        /// <summary>
        /// Whether the simulated request should produce an error in the form of an unhandled 
        /// exception
        /// </summary>
        [DataMember(Name = "error")]
        public bool Error { get; set; }

        /// <summary>
        /// Whether the simulated request should be acknowledged
        /// </summary>
        [DataMember(Name = "ack")]
        public bool Acknowledge { get; set; }

        /// <summary>
        /// Whether a reply should be sent in response to the simulated request
        /// </summary>
        [DataMember(Name = "reply")]
        public bool Reply { get; set; }


    }
}
