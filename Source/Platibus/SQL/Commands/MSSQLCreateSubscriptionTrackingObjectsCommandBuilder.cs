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
    /// A subclass of <see cref="CreateMessageQueueingObjectsCommandBuilder"/> that produces
    /// commands for creating message queueing objects in a SQL Server database using Transact-SQL
    /// syntax.
    /// </summary>
    public class MSSQLCreateSubscriptionTrackingObjectsCommandBuilder : CreateSubscriptionTrackingObjectsCommandBuilder
    {
        /// <inheritdoc />
        /// <summary>
        /// Produces a non-query command that can be used to create database objects necessary
        /// to store subscription information
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a non-query database command to create required database objects</returns>
        public override DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

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
IF OBJECT_ID('[PB_Subscriptions]') IS NULL
BEGIN
    CREATE TABLE [PB_Subscriptions]
    (
        [TopicName] VARCHAR(255) NOT NULL,
        [Subscriber] VARCHAR(500) NOT NULL,
        [Expires] DATETIME NULL,

        CONSTRAINT [PB_Subscriptions_PK] 
            PRIMARY KEY CLUSTERED ([TopicName], [Subscriber])
    )

    CREATE INDEX [PB_Subscriptions_IX_TopicName] 
        ON [PB_Subscriptions]([TopicName])
END";
            }
        }
    }
}