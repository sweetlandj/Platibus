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

using Platibus.Security;

namespace Platibus
{
    /// <summary>
    /// An interface describing an object that can visit specific types of
    /// endpoint credentials and map their type-specific details into a
    /// form that is usable by the transport
    /// </summary>
    public interface IEndpointCredentialsVisitor
    {
        /// <summary>
        /// Visits a set of basic authentication credentials
        /// </summary>
        /// <param name="credentials">The set of basic authentication credentials to visit</param>
        void Visit(BasicAuthCredentials credentials);

        /// <summary>
        /// Visits a set of default credentials
        /// </summary>
        /// <param name="credentials">The set of default credentials to visit</param>
        void Visit(DefaultCredentials credentials);
    }
}