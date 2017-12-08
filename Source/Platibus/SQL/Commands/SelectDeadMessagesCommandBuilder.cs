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
    /// Default command builder for creating commands to select abandoned messages for a queue
    /// from the database 
    /// </summary>
    public class SelectDeadMessagesCommandBuilder
    {
        /// <summary>
        /// The name of the queue
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The beginning of the date range to select from
        /// </summary>
        /// <remarks>
        /// This date is compared with the date as of which the message was marked as abandoned.
        /// </remarks>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end of the date range to select from
        /// </summary>
        /// <remarks>
        /// This date is compared with the date as of which the message was marked as abandoned.
        /// </remarks>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Initializes and returns a new query <see cref="DbCommand"/> with the configured
        /// parameters
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a new query <see cref="DbCommand"/> with the configured
        /// parameters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@QueueName", QueueName);
            command.SetParameter("@StartDate", StartDate);
            command.SetParameter("@EndDate", EndDate);
            return command;
        }

        /// <summary>
        /// Reads and parses the raw values from a <see cref="IDataRecord"/> into a 
        /// <see cref="QueuedMessageRecord"/>
        /// </summary>
        /// <param name="dataRecord">A data record read from the query command produced by the
        /// <see cref="BuildDbCommand"/> method.</param>
        /// <returns>Returns a new <see cref="QueuedMessageRecord"/> whose properties are 
        /// initialized according to the data in the supplied <paramref name="dataRecord"/></returns>
        public virtual QueuedMessageRecord BuildQueuedMessageRecord(IDataRecord dataRecord)
        {
            return new QueuedMessageRecord
            {
                Content = dataRecord.GetString("MessageContent"),
                Headers = dataRecord.GetString("Headers"),
                Attempts = dataRecord.GetInt("Attempts").GetValueOrDefault(),
#pragma warning disable 612
                SenderPrincipal = dataRecord.GetString("SenderPrincipal")
#pragma warning restore 612
            };
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
SELECT 
    [SenderPrincipal], 
    [Headers], 
    [MessageContent], 
    [Attempts]
FROM [PB_QueuedMessages]
WHERE [QueueName]=@QueueName 
AND [Acknowledged] IS NULL
AND [Abandoned] >= @StartDate
AND [Abandoned] < @EndDate";
    }
}
