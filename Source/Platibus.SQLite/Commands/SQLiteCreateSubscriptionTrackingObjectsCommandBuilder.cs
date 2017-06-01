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
using Platibus.SQL.Commands;

namespace Platibus.SQLite.Commands
{
    /// <summary>
    /// A subclass of <see cref="CreateMessageJournalObjectsCommandBuilder"/> that produces
    /// commands for creating message journal objects in a SQLite database using SQLite syntax.
    /// </summary>
    public class SQLiteCreateSubscriptionTrackingObjectsCommandBuilder : CreateSubscriptionTrackingObjectsCommandBuilder
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;
            return command;
        }

        /// <summary>
        /// The default command text (SQLite syntax)
        /// </summary>
        public virtual string CommandText
        {
            get
            {
                return @"
CREATE TABLE IF NOT EXISTS [PB_Subscriptions]
(
    [TopicName] TEXT NOT NULL,
    [Subscriber] TEXT NOT NULL,
    [Expires] TEXT NULL,

    CONSTRAINT [PB_Subscriptions_PK] 
        PRIMARY KEY ([TopicName], [Subscriber])
);

CREATE INDEX IF NOT EXISTS [PB_Subscriptions_IX_TopicName] 
    ON [PB_Subscriptions]([TopicName]);";
            }
        }
    }
}