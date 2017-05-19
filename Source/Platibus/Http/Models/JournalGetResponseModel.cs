using System.Collections.Generic;
using Newtonsoft.Json;

namespace Platibus.Http.Models
{
    /// <summary>
    /// A model representing the response content for a GET responses for the journal resource
    /// </summary>
    /// <remarks>
    /// Used for serializing and deserializing HTTP content related to message journal queries.
    /// </remarks>
    public class JournalGetResponseModel
    {
        private IList<ErrorModel> _errors;

        /// <summary>
        /// Error messages that occurred while servicing the request (if applicable)
        /// </summary>
        [JsonProperty("errors")]
        public IList<ErrorModel> Errors
        {
            get { return _errors ?? (_errors = new List<ErrorModel>()); }
            set { _errors = value; }
        }

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
        /// The journal entries
        /// </summary>
        [JsonProperty("entries")]
        public IList<MessageJournalEntryModel> Entries { get; set; }
    }
}
