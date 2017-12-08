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
using System.Data;
using System.Data.Common;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Default command builder for creating commands to delete topic subscription information
    /// from the database
    /// </summary>
    public class DeleteSubscriptionCommandBuilder
    {
        /// <summary>
        /// The name of the topic to which the <see cref="Subscriber"/> is subscribed
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// The URI of the instance subscribed to the <see cref="TopicName"/>
        /// </summary>
        public string Subscriber { get; set; }

        /// <summary>
        /// Initializes and returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@TopicName", TopicName);
            command.SetParameter("@Subscriber", Subscriber);

            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
DELETE FROM [PB_Subscriptions]
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber";
    }
}