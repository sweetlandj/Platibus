using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Journaling;
using Platibus.Security;
using Platibus.Serialization;

namespace Platibus.Http
{
    /// <summary>
    /// An HTTP resource controller for querying the message journal
    /// </summary>
    public class JournalController : IHttpResourceController
    {
        private readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageJournal _messageJournal;
        
        /// <summary>
        /// Initializes a <see cref="JournalController"/> with the specified 
        /// <paramref name="messageJournal"/>
        /// </summary>
        /// <param name="messageJournal">The message journal</param>
        /// <param name="authorizationService">(Optional) Used to determine whether a requestor is 
        /// authorized to query the message journal</param>
        public JournalController(IMessageJournal messageJournal, IAuthorizationService authorizationService = null)
        {
            if (messageJournal == null) throw new ArgumentNullException("messageJournal");
            _messageJournal = messageJournal;
            _authorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response, IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            if (!request.IsGet())
            {
                response.StatusCode = 405;
                response.AddHeader("Allow", "GET");
                return;
            }

            await Get(request, response);
        }

        private async Task Get(IHttpResourceRequest request, IHttpResourceResponse response)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            var authorized = _authorizationService == null ||
                             await _authorizationService.IsAuthorizedToQueryJournal(request.Principal);

            if (!authorized)
            {
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
                return;
            }
            
            var startStr = request.QueryString["start"];
            var start = string.IsNullOrWhiteSpace(startStr)
                ? await _messageJournal.GetBeginningOfJournal()
                : _messageJournal.ParseOffset(startStr);

            var countStr = request.QueryString["count"];
            if (string.IsNullOrWhiteSpace(countStr))
            {
                response.StatusCode = 400;
                response.StatusDescription = "Invalid Request - Query parameter 'count' is required";
                response.ContentType = "text/plain";
                return;
            }

            int count;
            if (!int.TryParse(countStr, out count) || count <= 0)
            {
                response.StatusCode = 400;
                response.StatusDescription = "Invalid Request - Query parameter 'count' must be a positive integer value";
                response.ContentType = "text/plain";
                return;
            }

            var filter = new MessageJournalFilter();
            var topic = request.QueryString["topic"];
            if (!string.IsNullOrWhiteSpace(topic))
            {
                filter.Topics = topic.Split(',')
                    .Select(t => (TopicName)t)
                    .ToList();
            }

            var category = request.QueryString["category"];
            if (!string.IsNullOrWhiteSpace(category))
            {
                filter.Categories = category.Split(',')
                    .Select(t => (JournaledMessageCategory)t.Trim())
                    .ToList();
            }

            var result = await _messageJournal.Read(start, count, filter);
            var responseContent = new GetJournalResponse
            {
                Start = start.ToString(),
                Next = result.Next.ToString(),
                EndOfJournal = result.EndOfJournal,
                Data = result.JournaledMessages.Select(jm => new JournaledMessageResource
                {
                    Offset = jm.Offset.ToString(),
                    Category = jm.Category,
                    Headers = jm.Message.Headers.ToDictionary(h => (string) h.Key, h => h.Value),
                    Content = jm.Message.Content
                }).ToList()
            };

            response.ContentType = "application/json";
            var serializedContent = _serializer.Serialize(responseContent);
            var encoding = response.ContentEncoding;
            var encodedContent = encoding.GetBytes(serializedContent);
            await response.OutputStream.WriteAsync(encodedContent, 0, encodedContent.Length);
            response.StatusCode = 200;
        }
        
    }
}
