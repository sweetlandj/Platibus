using System.Collections.Generic;
using Newtonsoft.Json;

namespace Platibus.Http
{
    /// <summary>
    /// A journaled message
    /// </summary>
    public class JournaledMessageResource
    {
        /// <summary>
        /// The journal offset at which this message is located
        /// </summary>
        [JsonProperty("offset")]
        public string Offset { get; set; }

        /// <summary>
        /// The journal category (sent, received, published) for this entry
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// The message headers
        /// </summary>
        [JsonProperty("headers")]
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The message content
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}