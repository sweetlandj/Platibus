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

namespace Platibus.Config
{
    /// <summary>
    /// An implementation of <see cref="IMessageSpecification"/> that uses a
    /// specified function delegate to determine whether a message satisfies
    /// the conditions for which the specification is being used (e.g. whether
    /// a <see cref="ISendRule"/> applies).
    /// </summary>
    public class DelegateMessageSpecification : IMessageSpecification
    {
        private readonly Func<Message, bool> _delegate;

        /// <summary>
        /// Initializes a new <see cref="DelegateMessageSpecification"/> with
        /// the specified function <paramref name="delegate"/>.
        /// </summary>
        /// <param name="delegate">The function used to determine whether
        /// messages satisfy the specification</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="delegate"/>
        /// is <c>null</c></exception>
        public DelegateMessageSpecification(Func<Message, bool> @delegate)
        {
            if (@delegate == null) throw new ArgumentNullException("delegate");
            _delegate = @delegate;
        }

        /// <summary>
        /// Indicates whether the specification is satisfied by the 
        /// supplied <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message in question</param>
        /// <returns>Returns <c>true</c> if the specification is
        /// satisfied by the message; <c>false</c> otherwise.</returns>
        public bool IsSatisfiedBy(Message message)
        {
            return _delegate(message);
        }

        /// <summary>
        /// Implicitly casts a function delegate to a 
        /// <see cref="DelegateMessageSpecification"/>.
        /// </summary>
        /// <param name="delegate">The function delegate</param>
        /// <returns>Returns <c>null</c> if <paramref name="delegate"/> is
        /// <c>null</c> or a new <see cref="DelegateMessageSpecification"/> wrapping
        /// the <paramref name="delegate"/> otherwise.</returns>
        public static implicit operator DelegateMessageSpecification(Func<Message, bool> @delegate)
        {
            return @delegate == null ? null : new DelegateMessageSpecification(@delegate);
        }
    }
}