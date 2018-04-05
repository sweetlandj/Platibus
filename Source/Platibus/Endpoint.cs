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
using System.Diagnostics;
using Platibus.Utils;

namespace Platibus
{
    /// <summary>
    /// A basic implementation of <see cref="IEndpoint"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(Address) + ",nq}")]
    public class Endpoint : IEndpoint
    {
        /// <summary>
        /// Initializes a new <see cref="Endpoint"/> with the specified address
        /// and credentials
        /// </summary>
        /// <param name="address">The endoint address</param>
        /// <param name="credentials">(Optional) The credentials required to
        /// connect to the endpoint</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="address"/>
        /// is <c>null</c></exception>
        public Endpoint(Uri address, IEndpointCredentials credentials = null)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            Address = address.WithTrailingSlash();
            Credentials = credentials;
        }

        /// <summary>
        /// The base URI used to connect to the endpoint
        /// </summary>
        public Uri Address { get; }

        /// <summary>
        /// The credentials required to connect to the endpoint
        /// </summary>
        public IEndpointCredentials Credentials { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Address.ToString();
        }
    }
}