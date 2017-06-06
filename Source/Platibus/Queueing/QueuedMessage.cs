// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Security.Principal;

namespace Platibus.Queueing
{
    /// <summary>
    /// An immutable representation of a queued message
    /// </summary>
    public class QueuedMessage
    {
        private readonly Message _message;
        private readonly IPrincipal _principal;
        private readonly int _attempts;

        /// <summary>
        /// The queued message
        /// </summary>
        public Message Message
        {
            get { return _message; }
        }

        /// <summary>
        /// The principal represending the message sender
        /// </summary>
        public IPrincipal Principal
        {
            get { return _principal; }
        }

        /// <summary>
        /// The number of attempts that have been made so far to handle the message
        /// </summary>
        public int Attempts { get { return _attempts; } }

        /// <summary>
        /// Initializes a new <see cref="QueuedMessage"/>
        /// </summary>
        /// <param name="message">The queued message</param>
        /// <param name="principal">(Optional) The principal representing the message sender</param>
        public QueuedMessage(Message message, IPrincipal principal)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _principal = principal;
        }

        /// <summary>
        /// Initializes a new <see cref="QueuedMessage"/>
        /// </summary>
        /// <param name="message">The queued message</param>
        /// <param name="principal">(Optional) The principal representing the message sender</param>
        /// <param name="attempts">The number of attempts that have been made to process the
        ///     queued message</param>
        public QueuedMessage(Message message, IPrincipal principal, int attempts)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _principal = principal;
            _attempts = attempts;
        }

        /// <summary>
        /// Returns a copy of this <see cref="QueuedMessage"/> object with an incremented
        /// <see cref="Attempts"/> value
        /// </summary>
        /// <returns>Returns a copy of this <see cref="QueuedMessage"/> object with an incremented
        /// <see cref="Attempts"/> value</returns>
        public QueuedMessage NextAttempt()
        {
            return new QueuedMessage(_message, _principal, _attempts + 1);
        }
    }
}
