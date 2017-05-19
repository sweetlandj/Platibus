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

using System.Data.Common;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// An abstract base class that can be extended to produce commands capable of creating
    /// database objects needed to store journaled messages according to commands, syntax, and
    /// features of the underlying database.
    /// </summary>
    public abstract class CreateMessageJournalObjectsCommandBuilder
    {
        /// <summary>
        /// Produces a non-query command that can be used to create database objects necessary
        /// to store journaled messages
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a non-query database command to create required database objects</returns>
        public abstract DbCommand BuildDbCommand(DbConnection connection);
    }
}