using System;
using Newtonsoft.Json;

namespace Platibus.Http.Models
{
    /// <summary>
    /// A model representing a message journal entry
    /// </summary>
    /// <remarks>
    /// Used for serializing and deserializing HTTP content related to message journal queries
    /// </remarks>
    public class MessageJournalEntryModel
    {
        /// <summary>
        /// The journal position at which this entry is located
        /// </summary>
        [JsonProperty("pos")]
        public string Position { get; set; }

        /// <summary>
        /// The timestamp associated with this entry
        /// </summary>
        [JsonProperty("ts")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The journal category (sent, received, published) for this entry
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// The message data
        /// </summary>
        [JsonProperty("data")]
        public MessageJournalEntryDataModel Data { get; set; }
    }
}