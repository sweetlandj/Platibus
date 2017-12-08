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
using Platibus.Diagnostics;

namespace Platibus.Config
{
    /// <summary>
    /// A loopback configuration
    /// </summary>
    public class LoopbackConfiguration : PlatibusConfiguration, ILoopbackConfiguration
    {
        private readonly EndpointName _loopbackEndpoint;

        /// <summary>
        /// The base URI of the loopback bus instance
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// The message queueing service to use
        /// </summary>
        public IMessageQueueingService MessageQueueingService { get; set; }


        /// <summary>
        /// Initializes a new <see cref="LoopbackConfiguration"/> with a preconfigured
        /// <paramref name="diagnosticService"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public LoopbackConfiguration(IDiagnosticService diagnosticService = null) : base(diagnosticService)
        {
            _loopbackEndpoint = new EndpointName("looback");
            BaseUri = new Uri("urn:localhost/loopback");
            base.AddEndpoint(_loopbackEndpoint, new Endpoint(BaseUri));
            var allMessages = new MessageNamePatternSpecification(".*");
            base.AddSendRule(new SendRule(allMessages, _loopbackEndpoint));
        }

        /// <summary>
        /// Adds a topic to the configuration
        /// </summary>
        /// <param name="topic">The name of the topic</param>
        /// <remarks>
        /// Topics must be explicitly added in order to publish messages to them
        /// </remarks>
        public override void AddTopic(TopicName topic)
        {
            base.AddTopic(topic);
            base.AddSubscription(new Subscription(_loopbackEndpoint, topic));
        }

        /// <summary>
        /// Adds a named endpoint to the configuration
        /// </summary>
        /// <param name="name">The name of the endpoint</param>
        /// <param name="endpoint">The endpoint</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddEndpoint(EndpointName name, IEndpoint endpoint)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds a rule governing to which endpoints messages will be sent
        /// </summary>
        /// <param name="sendRule">The send rule</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddSendRule(ISendRule sendRule)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds a subscription to a local or remote topic
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddSubscription(ISubscription subscription)
        {
            throw new NotSupportedException();
        }
    }
}
