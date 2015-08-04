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

namespace Platibus.Config
{
    /// <summary>
    /// An interface that describes a set of conditions that must be satisfied
    /// pertaining to a message.
    /// </summary>
    /// <remarks>
    /// Message specifications are used, for example, in <see cref="ISendRule"/>s
    /// to determine whether a rule applies to a particular message.
    /// </remarks>
    public interface IMessageSpecification
    {
        /// <summary>
        /// Indicates whether the specification is satisfied by the 
        /// supplied <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message in question</param>
        /// <returns>Returns <c>true</c> if the specification is
        /// satisfied by the message; <c>false</c> otherwise.</returns>
        bool IsSatisfiedBy(Message message);
    }
}