using System.Collections.Generic;
using Newtonsoft.Json;

namespace Platibus.Http.Models
{
    /// <summary>
    /// A model representing a message data stored in a journal entry
    /// </summary>
    /// <remarks>
    /// Used for serializing and deserializing HTTP content related to message journal queries
    /// </remarks>
    public class MessageJournalEntryDataModel
    {
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