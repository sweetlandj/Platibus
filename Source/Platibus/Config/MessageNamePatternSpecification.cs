// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Text.RegularExpressions;

namespace Platibus.Config
{
    /// <summary>
    /// An <see cref="IMessageSpecification"/> implementation that matches
    /// messages based on the value of the <see cref="HeaderName.MessageName"/> 
    /// header.
    /// </summary>
    public class MessageNamePatternSpecification : IMessageSpecification
    {
        private readonly Regex _nameRegex;

        /// <summary>
        /// The regular expression used to match message names
        /// </summary>
        public Regex NameRegex
        {
            get { return _nameRegex; }
        }

        /// <summary>
        /// Initializes a new <see cref="MessageNamePatternSpecification"/>
        /// that will match message names using the specified regular
        /// expression
        /// </summary>
        /// <param name="namePattern">A regular expression used to match
        /// message names</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="namePattern"/>
        /// is <c>null</c> or whitespace</exception>
        public MessageNamePatternSpecification(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern)) throw new ArgumentNullException("namePattern");
            _nameRegex = new Regex(namePattern, RegexOptions.Compiled);
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
            var messageName = (string)message.Headers.MessageName ?? "";
            return (_nameRegex == null || _nameRegex.IsMatch(messageName));
        }
    }
}