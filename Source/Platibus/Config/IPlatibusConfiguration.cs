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

using System.Collections.Generic;
using Platibus.Serialization;

namespace Platibus.Config
{
    /// <summary>
    /// An interface describing the core configuration for Platibus instances
    /// regardless of how they are hosted.
    /// </summary>
    public interface IPlatibusConfiguration
    {
        /// <summary>
        /// The service used to serialize and deserialize message content
        /// </summary>
        ISerializationService SerializationService { get; }

        /// <summary>
        /// The service used to map content object types onto canonical names
        /// to facilitate deserialization
        /// </summary>
        IMessageNamingService MessageNamingService { get; }

        /// <summary>
        /// A service used to track and record the sending, receipt, and 
        /// publication of messages.
        /// </summary>
        IMessageJournalingService MessageJournalingService { get; }

        /// <summary>
        /// The topics to which messages can be published
        /// </summary>
        IEnumerable<TopicName> Topics { get; }

        /// <summary>
        /// The set of known endpoints and their addresses
        /// </summary>
        IEndpointCollection Endpoints { get; }

        /// <summary>
        /// Rules that specify the endpoints to which messages should be sent
        /// </summary>
        IEnumerable<ISendRule> SendRules { get; }

        /// <summary>
        /// Rules that specify the handlers to which inbound messages should be
        /// routed
        /// </summary>
        IEnumerable<IHandlingRule> HandlingRules { get; }

        /// <summary>
        /// Subscriptions to topics hosted in local or remote bus instances
        /// </summary>
        IEnumerable<ISubscription> Subscriptions { get; }
    }
}