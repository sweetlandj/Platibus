using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Platibus.Http.Models;
using Platibus.Journaling;
using System.Threading.Tasks;
#if NET452
using System.Web;
#endif

namespace Platibus.Http.Clients
{
    /// <summary>
    /// A client for consuming messages from a local or remote journal via HTTP endpoint
    /// </summary>
    public class HttpMessageJournalClient : IDisposable
    {
        private static readonly UrlEncoder UrlEncoder = new UrlEncoder();
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
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
            _httpClient = new BasicHttpClientFactory().GetClient(baseUri, credentials);
        }

        /// <summary>
        /// Initializes a new <see cref="HttpMessageJournalClient"/> for the instance using a
        /// previously configured <paramref name="httpClient"/>
        /// </summary>
        /// <param name="httpClient">An HTTP client configured with a base URI and 
        /// necessary default headers</param>
        public HttpMessageJournalClient(HttpClient httpClient)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            _httpClient = Task.FromResult(httpClient);
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
                if (filter.From != null)
                {
                    queryParameters["from"] = FormatDate(filter.From);
                }
                if (filter.To != null)
                {
                    queryParameters["to"] = FormatDate(filter.To);
                }
                if (filter.Origination != null)
                {
                    queryParameters["origination"] = filter.Origination.ToString();
                }
                if (filter.Destination != null)
                {
                    queryParameters["destination"] = filter.Destination.ToString();
                }
                if (!string.IsNullOrWhiteSpace(filter.MessageName))
                {
                    queryParameters["messageName"] = filter.MessageName;
                }
                if (filter.RelatedTo != null)
                {
                    queryParameters["relatedTo"] = filter.RelatedTo;
                }
            }
            return string.Join("&", queryParameters
                .Select(p => p.Key + "=" + UrlEncoder.Encode(p.Value)));
        }

        private static string FormatDate(DateTime? date)
        {
            return date.GetValueOrDefault().ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
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
                _httpClient?.Dispose();
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
