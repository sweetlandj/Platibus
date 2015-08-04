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

namespace Platibus.Config
{
    /// <summary>
    /// Basic send rule implementation
    /// </summary>
    public class SendRule : ISendRule
    {
        private readonly IEnumerable<EndpointName> _endpoints;
        private readonly IMessageSpecification _specification;

        /// <summary>
        /// The message specification that selects messages to which the
        /// send rule applies.
        /// </summary>
        public IMessageSpecification Specification
        {
            get { return _specification; }
        }

        /// <summary>
        /// The set of endpoints to which messages that match the
        /// <see cref="ISendRule.Specification"/> should be sent.
        /// </summary>
        public IEnumerable<EndpointName> Endpoints
        {
            get { return _endpoints; }
        }

        /// <summary>
        /// Initializes a new <see cref="SendRule"/> with the provided
        /// <paramref name="specification"/> and <paramref name="endpoints"/>
        /// </summary>
        /// <param name="specification">The message specification used to
        /// determine whether this rule applies to a particular message</param>
        /// <param name="endpoints">The endpoints to which messages matching
        /// the specification will be sent</param>
        /// <exception cref="ArgumentNullException">If either argument is <c>null</c></exception>
        /// <exception cref="EndpointRequiredException">If <paramref name="endpoints"/>
        /// is empty or contains only <c>null</c> elements</exception>
        public SendRule(IMessageSpecification specification, IEnumerable<EndpointName> endpoints)
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (endpoints == null) throw new ArgumentNullException("endpoints");

            var endpointList = endpoints as IList<EndpointName> ?? endpoints.ToList();
            if (endpointList.All(x => x == null)) throw new EndpointRequiredException();

            _specification = specification;
            _endpoints = endpointList.Where(x => x != null).ToList();
        }

        /// <summary>
        /// Initializes a new <see cref="SendRule"/> with the provided
        /// <paramref name="specification"/> and <paramref name="endpoints"/>
        /// </summary>
        /// <param name="specification">The message specification used to
        /// determine whether this rule applies to a particular message</param>
        /// <param name="endpoints">The endpoints to which messages matching
        /// the specification will be sent</param>
        /// <exception cref="ArgumentNullException">If either argument is <c>null</c></exception>
        /// <exception cref="EndpointRequiredException">If <paramref name="endpoints"/>
        /// is empty or contains only <c>null</c> elements</exception>
        public SendRule(IMessageSpecification specification, params EndpointName[] endpoints)
            : this(specification, (IEnumerable<EndpointName>) endpoints)
        {
        }
    }
}