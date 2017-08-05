using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Platibus.Http.Models;
using Platibus.Journaling;

namespace Platibus.Http.Clients
{
    /// <summary>
    /// A client for consuming messages from a local or remote journal via HTTP endpoint
    /// </summary>
    public class HttpMessageJournalClient : IDisposable
    {
        private readonly Task<HttpClient> _httpClient;
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="HttpMessageJournalClient"/> for the instance at the specified
        /// <paramref name="baseUri"/>
        /// </summary>
        /// <param name="baseUri">The base URI of the instance from which journaled messages
        /// will be consumed</param>
        /// <param name="credentials">(Optional) The credentials needed to connect to the
        /// instance hosting the journal</param>
        public HttpMessageJournalClient(Uri baseUri, IEndpointCredentials credentials = null)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            _httpClient = new BasicHttpClientFactory().GetClient(baseUri, credentials);
        }
        
        /// <summary>
        /// Reads messages from the journal
        /// </summary>
        /// <param name="start">The first position to read or <c>null</c> to begin reading from
        /// the beginning of the journal</param>
        /// <param name="count">The maximum number of messages to read</param>
        /// <param name="filter">(Optional) Constraints on which messages should be read</param>
        /// <param name="cancellationToken">(Optional) A token that the caller can use to
        /// request cancelation of the read operation</param>
        /// <returns>Returns a task whose result is the journal messages and meta data returned
        /// by the server</returns>
        public async Task<MessageJournalReadResult> Read(string start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpClient = await _httpClient;
            var relativeUri = new Uri("journal?" + BuildQuery(start, count, filter), UriKind.Relative);
            using (var responseMessage = await httpClient.GetAsync(relativeUri, cancellationToken))
            {
                responseMessage.EnsureSuccessStatusCode();
                return await ParseResponseContent(responseMessage);
            }
        }

        private static string BuildQuery(string start, int count, MessageJournalFilter filter)
        {
            var queryParameters = new Dictionary<string, string>();
            if (start != null)
            {
                queryParameters["start"] = start;
            }
            queryParameters["count"] = count.ToString();

            if (filter != null)
            {
                if (filter.Topics.Count > 0)
                {
                    queryParameters["topic"] = string.Join(",", filter.Topics);
                }
                if (filter.Categories.Count > 0)
                {
                    queryParameters["category"] = string.Join(",", filter.Categories);
                }
            }
            return string.Join("&", queryParameters
                .Select(p => p.Key + "=" + HttpUtility.UrlEncode(p.Value)));
        }

        private static async Task<MessageJournalReadResult> ParseResponseContent(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<JournalGetResponseModel>(content);
            var start = new VerbatimMessageJournalPosition(model.Start);
            var next = new VerbatimMessageJournalPosition(model.Next);
            var endOfJournal = model.EndOfJournal;
            var entries = new List<MessageJournalEntry>();
            foreach (var entryModel in model.Entries)
            {
                var category = entryModel.Category;
                var position = new VerbatimMessageJournalPosition(entryModel.Position);
                var timestamp = entryModel.Timestamp;
                var headers = new MessageHeaders(entryModel.Data.Headers);
                var messageContent = entryModel.Data.Content;
                var message = new Message(headers, messageContent);
                var entry = new MessageJournalEntry(category, position, timestamp, message);
                entries.Add(entry);
            }
            return new MessageJournalReadResult(start, next, endOfJournal, entries);
        }

        /// <summary>
        /// Dispose of resources used by this object
        /// </summary>
        /// <param name="disposing">Whether this method is called from the <see cref="Dispose()"/>
        /// method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null) _httpClient.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~HttpMessageJournalClient()
        {
            Dispose(false);
        }

        private class VerbatimMessageJournalPosition : MessageJournalPosition, IEquatable<VerbatimMessageJournalPosition>
        {
            private readonly string _responseModelValue;

            public VerbatimMessageJournalPosition(string responseModelValue)
            {
                _responseModelValue = responseModelValue;
            }

            public override string ToString()
            {
                return _responseModelValue;
            }

            public bool Equals(VerbatimMessageJournalPosition other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(_responseModelValue, other._responseModelValue);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((VerbatimMessageJournalPosition) obj);
            }

            public override int GetHashCode()
            {
                return _responseModelValue != null ? _responseModelValue.GetHashCode() : 0;
            }
        }
    }
}
