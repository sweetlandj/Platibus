// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Threading.Tasks;
using Platibus.Serialization;

namespace Platibus.Http
{
    public class TopicController : IHttpResourceController
    {
        private readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();
        private readonly IList<TopicName> _topics;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;

        public TopicController(ISubscriptionTrackingService subscriptionTrackingService, IEnumerable<TopicName> topics)
        {
            if (subscriptionTrackingService == null) throw new ArgumentNullException("subscriptionTrackingService");
            _subscriptionTrackingService = subscriptionTrackingService;
            _topics = (topics ?? Enumerable.Empty<TopicName>())
                .Where(t => t != null)
                .ToList();
        }

        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response,
            IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            var topicSegments = subPath.ToList();
            if (!topicSegments.Any() && "get".Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                await GetTopics(request, response);
                return;
            }
            if (topicSegments.Any())
            {
                var topic = topicSegments.First();
                var nestedResource = topicSegments.Skip(1).FirstOrDefault();
                if ("subscriber".Equals(nestedResource, StringComparison.OrdinalIgnoreCase))
                {
                    var subscriberSubPath = topicSegments.Skip(2);
                    await PostOrDeleteSubscriber(request, response, topic, subscriberSubPath);
                    return;
                }
            }

            response.StatusCode = 400;
        }

        public async Task GetTopics(IHttpResourceRequest request, IHttpResourceResponse response)
        {
            var topicList = _topics.Select(t => t.ToString()).ToArray();
            var responseContent = _serializer.Serialize(topicList);
            var encoding = response.ContentEncoding;
            var encodedContent = encoding.GetBytes(responseContent);
            await response.OutputStream.WriteAsync(encodedContent, 0, encodedContent.Length);
            response.StatusCode = 200;
        }

        public async Task PostOrDeleteSubscriber(IHttpResourceRequest request, IHttpResourceResponse response,
            TopicName topic, IEnumerable<string> subPath)
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

            if ("delete".Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
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