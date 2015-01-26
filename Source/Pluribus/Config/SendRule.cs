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
using System.Collections.Generic;
using System.Linq;

namespace Pluribus.Config
{
    internal class SendRule : ISendRule
    {
        private readonly IEnumerable<EndpointName> _endpoints;
        private readonly IMessageSpecification _specification;

        public SendRule(IMessageSpecification specification, IEnumerable<EndpointName> endpoints)
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (endpoints == null) throw new ArgumentNullException("endpoint");
            if (!endpoints.Any(x => x != null)) throw new EndpointRequiredException();
            _specification = specification;
            _endpoints = endpoints.Where(x => x != null).ToList();
        }

        public SendRule(IMessageSpecification specification, params EndpointName[] endpoints)
            : this(specification, (IEnumerable<EndpointName>) endpoints)
        {
        }

        public IMessageSpecification Specification
        {
            get { return _specification; }
        }

        public IEnumerable<EndpointName> Endpoints
        {
            get { return _endpoints; }
        }
    }
}