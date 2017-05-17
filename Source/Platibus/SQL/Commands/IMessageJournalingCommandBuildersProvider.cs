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

using System.Configuration;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// An interface describing a class that provides message journal commands that 
    /// conform the the SQL syntax appropriate for a given ADO.NET connection provider
    /// </summary>
    public interface IMessageJournalingCommandBuildersProvider
    {
        /// <summary>
        /// Returns the message journal commands that are most appropriate given the specified 
        /// <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <returns>Returns the message journal commands that are most appropriate for use with 
        /// connections based on the supplied <paramref name="connectionStringSettings"/></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if 
        /// <paramref name="connectionStringSettings"/> is <c>null</c></exception>
        IMessageJournalingCommandBuilders GetMessageJournalingCommandBuilders(ConnectionStringSettings connectionStringSettings);
    }
}
