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
    /// Default command builder for creating commands to update existing queued messages in the
    /// database.
    /// </summary>
    public class UpdateQueuedMessageCommandBuilder
    {
        /// <summary>
        /// The message ID
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// The name of the message queue
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The date/time the message was acknowledged
        /// </summary>
        public DateTime? Acknowledged { get; set; }

        /// <summary>
        /// The date/time the message was abandoned (maximum attempts exhausted)
        /// </summary>
        public DateTime? Abandoned { get; set; }

        /// <summary>
        /// The total number of attempts that have been made to process the message
        /// </summary>
        public int Attempts { get; set; }

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
            if (connection == null) throw new ArgumentNullException("connection");

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@MessageId", MessageId);
            command.SetParameter("@QueueName", QueueName);
            command.SetParameter("@Acknowledged", Acknowledged);
            command.SetParameter("@Abandoned", Abandoned);
            command.SetParameter("@Attempts", Attempts);

            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText
        {
            get
            {
                return @"
UPDATE [PB_QueuedMessages] SET 
    [Acknowledged]=@Acknowledged,
    [Abandoned]=@Abandoned,
    [Attempts]=@Attempts
WHERE [MessageId]=@MessageId 
AND [QueueName]=@QueueName";
            }
        }
    }
}
