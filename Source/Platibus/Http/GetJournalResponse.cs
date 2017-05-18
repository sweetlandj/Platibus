using System.Collections.Generic;
using Newtonsoft.Json;

namespace Platibus.Http
{
    /// <summary>
    /// A response to a GET request for the "journal" resource
    /// </summary>
    public class GetJournalResponse
    {
        /// <summary>
        /// The journal offset at which the query started
        /// </summary>
        [JsonProperty("start")]
        public string Start { get; set; }

        /// <summary>
        /// The next journal offset to read from
        /// </summary>
        [JsonProperty("next")]
        public string Next { get; set; }

        /// <summary>
        /// Whether the end of the journal was reached
        /// </summary>
        [JsonProperty("endOfJournal")]
        public bool EndOfJournal { get; set; }

        /// <summary>
        /// The journaled messages
        /// </summary>
        [JsonProperty("journaledMessages")]
        public IList<JournaledMessageResource> Data { get; set; }
    }
}
