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
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <summary>
    /// An interface describing the available bus operations
    /// </summary>
    public interface IBus
    {
        /// <summary>
        ///     Sends <paramref name="content" /> to default configured endpoints.
        /// </summary>
        /// <param name="content">The content to send.</param>
        /// <param name="options">Optional settings that influence how the message is sent.</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        Task<ISentMessage> Send(object content, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Sends <paramref name="content" /> to a single caller-specified
        ///     <paramref name="endpointName" />.
        /// </summary>
        /// <param name="content">The message to send.</param>
        /// <param name="endpointName">The endpoint to which the message should be sent.</param>
        /// <param name="options">Optional settings that influence how the message is sent.</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        Task<ISentMessage> Send(object content, EndpointName endpointName, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Sends <paramref name="content" /> to a single caller-specified
        ///     endpoint <paramref name="endpointAddress" />.
        /// </summary>
        /// <param name="content">The message to send.</param>
        /// <param name="endpointAddress">The URI of the endpoint to which the message should be sent.</param>
        /// <param name="credentials">Optional credentials for authenticating with the endpoint
        /// at the specified URI.</param>
        /// <param name="options">Optional settings that influence how the message is sent.</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        Task<ISentMessage> Send(object content, Uri endpointAddress, IEndpointCredentials credentials = null,
            SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Publishes <paramref name="content" /> to the specified
        ///     <paramref name="topic" />.
        /// </summary>
        /// <param name="content">The message to publish.</param>
        /// <param name="topic">The topic to which the message should be published.</param>
        /// <param name="cancellationToken">An optional cancellation token</param>
        Task Publish(object content, TopicName topic, CancellationToken cancellationToken = default(CancellationToken));
    }
}