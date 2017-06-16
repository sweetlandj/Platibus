// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platibus.Security;
using Platibus.Serialization;

namespace Platibus.Http.Controllers
{
    /// <summary>
    /// An HTTP resource controller for topics and related resources
    /// </summary>
    public class TopicController : IHttpResourceController
    {
        private readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();
        private readonly IList<TopicName> _topics;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new <see cref="TopicController"/>
        /// </summary>
        /// <param name="subscriptionTrackingService">The subscription tracking service</param>
        /// <param name="topics">The list of defined topics</param>
        /// <param name="authorizationService">(Optional) Used to determine whether
        /// a requestor is authorized to subscribe to a topic</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscriptionTrackingService"/>
        /// is <c>null</c></exception>
        public TopicController(ISubscriptionTrackingService subscriptionTrackingService, IEnumerable<TopicName> topics, IAuthorizationService authorizationService = null)
        {
            if (subscriptionTrackingService == null) throw new ArgumentNullException("subscriptionTrackingService");
            _subscriptionTrackingService = subscriptionTrackingService;
            _authorizationService = authorizationService;
            _topics = (topics ?? Enumerable.Empty<TopicName>())
                .Where(t => t != null)
                .ToList();
        }

        /// <summary>
        /// Processes the specified <paramref name="request"/> and updates the supplied
        /// <paramref name="response"/>
        /// </summary>
        /// <param name="request">The HTTP resource request to process</param>
        /// <param name="response">The HTTP response to update</param>
        /// <param name="subPath">The portion of the request path that remains after the
        /// request was routed to this controller</param>
        /// <returns>Returns a task that completes when the request has been processed and the
        /// response has been updated</returns>
        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response,
            IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            var topicSegments = subPath.ToList();
            if (!topicSegments.Any() && request.IsGet())
            {
                await GetTopics(response);
                return;
            }

            if (topicSegments.Any())
            {
                var topic = topicSegments.First();
                var nestedResource = topicSegments.Skip(1).FirstOrDefault();
                if ("subscriber".Equals(nestedResource, StringComparison.OrdinalIgnoreCase))
                {
                    await PostOrDeleteSubscriber(request, response, topic);
                    return;
                }
            }

            response.StatusCode = 400;
        }

        private async Task GetTopics(IHttpResourceResponse response)
        {
            response.ContentType = "application/json";
            var encoding = response.ContentEncoding;
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
                response.ContentEncoding = encoding;
            }

            var topicList = _topics.Select(t => t.ToString()).ToArray();
            var responseContent = _serializer.Serialize(topicList);
            var encodedContent = encoding.GetBytes(responseContent);
            await response.OutputStream.WriteAsync(encodedContent, 0, encodedContent.Length);
            response.StatusCode = 200;
        }

        private async Task PostOrDeleteSubscriber(IHttpResourceRequest request, IHttpResourceResponse response,
            TopicName topic)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            var uri = request.QueryString["uri"];
            if (uri == null)
            {
                response.StatusCode = 400;
                response.StatusDescription = "Subscriber URI is required";
                return;
            }

            var subscriber = new Uri(uri);
            if (!_topics.Contains(topic))
            {
                response.StatusCode = 404;
                response.StatusDescription = "Topic not found";
                return;
            }

            var authorized = _authorizationService == null
                             || await _authorizationService.IsAuthorizedToSubscribe(request.Principal, topic);

            if (!authorized)
            {
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
                return;
            }

            if ("delete".Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Verify that the subscriber is the one deleting the subscription
                // Ideas: Issue another HTTP request back to the subscriber to verify?
                //        Require the subscriber to specify a subscription ID (GUID?)
                //        when subscribing and then supply the same ID when unsubscribing?
                await _subscriptionTrackingService.RemoveSubscription(topic, subscriber);
            }
            else
            {   
                var ttlStr = request.QueryString["ttl"];
                var ttl = string.IsNullOrWhiteSpace(ttlStr)
                    ? default(TimeSpan)
                    : TimeSpan.FromSeconds(int.Parse(ttlStr));

                await _subscriptionTrackingService.AddSubscription(topic, subscriber, ttl);
            }
            response.StatusCode = 202;
        }
    }
}