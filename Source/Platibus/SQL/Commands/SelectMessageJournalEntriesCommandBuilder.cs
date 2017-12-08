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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Default command builder for creating commands to select journaled messages from the 
    /// database 
    /// </summary>
    public class SelectMessageJournalEntriesCommandBuilder
    {
        private IList<string> _categories;
        private IList<string> _topics;

        /// <summary>
        /// Specifies the categories to which results should be constrained
        /// </summary>
        public IList<string> Categories
        {
            get => _categories ?? (_categories = new List<string>());
            set => _categories = value;
        }

        /// <summary>
        /// Specifies the topics to which results should be constrained
        /// </summary>
        public IList<string> Topics
        {
            get => _topics ?? (_topics = new List<string>());
            set => _topics = value;
        }

        /// <summary>
        /// The ID of the first result to return
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// The number of results to return
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constrains results to entries whose timestamp is greater than or equal to the
        /// specified date/time.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Constrains results to entries whose timestamp is less than the specified date/time.
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Constrains results to entries originating from the specified base URI.
        /// </summary>
        public Uri Origination { get; set; }

        /// <summary>
        /// Constrains results to entries addressed to the specified base URI.
        /// </summary>
        public Uri Destination { get; set; }

        /// <summary>
        /// Constrains results to entries with message names containing the specified substring.
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Constrains results to entries that are related to to the specified message ID.
        /// </summary>
        public Guid? RelatedTo { get; set; }
        
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

            command.SetParameter("@Count", Count);
            command.SetParameter("@Start", Start);

            AppendTimestampFilterConditions(command);
            AppendTopicFilterConditions(command);
            AppendCategoryFilterConditions(command);
            AppendRoutingFilterConditions(command);
            AppendMessageNameFilterConditions(command);
            AppendSort(command);

            return command;
        }

        /// <summary>
        /// Reads and parses the raw values from a <see cref="IDataRecord"/> into a 
        /// <see cref="MessageJournalEntryRecord"/>
        /// </summary>
        /// <param name="dataRecord">A data record read from the query command produced by the
        /// <see cref="BuildDbCommand"/> method.</param>
        /// <returns>Returns a new <see cref="MessageJournalEntryRecord"/> whose properties are 
        /// initialized according to the data in the supplied <paramref name="dataRecord"/></returns>
        public virtual MessageJournalEntryRecord BuildJournaledMessageRecord(IDataRecord dataRecord)
        {
            return new MessageJournalEntryRecord
            {
                Id = dataRecord.GetInt("Id").GetValueOrDefault(),
                Category = dataRecord.GetString("Category"),
                Timestamp = dataRecord.GetDateTime("Timestamp").GetValueOrDefault(),
                Headers = dataRecord.GetString("Headers"),
                Content = dataRecord.GetString("MessageContent")
            };
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
SELECT TOP (@Count)
    [Id],
    [Category],
    CAST([Timestamp] AS [DateTime]) AS [Timestamp],
    [Headers], 
    [MessageContent]
FROM [PB_MessageJournal]
WHERE [Id] >= @Start";

        /// <summary>
        /// Appends timestamp based filter criteria to a command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendTimestampFilterConditions(DbCommand command)
        {
            if (From != null)
            {
                command.CommandText += " AND [Timestamp] >= @From";
                command.SetParameter("@From", From);
            }

            if (To != null)
            {
                command.CommandText += " AND [Timestamp] < @To";
                command.SetParameter("@To", To);
            }
        }

        /// <summary>
        /// Appends origination and destination filter criteria to a command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendRoutingFilterConditions(DbCommand command)
        {
            if (Origination != null)
            {
                command.CommandText += " AND [Origination] = @Origination";
                command.SetParameter("@Origination", Origination.WithTrailingSlash().ToString());
            }

            if (Destination != null)
            {
                command.CommandText += " AND [Destination] = @Destination";
                command.SetParameter("@Destination", Destination.WithTrailingSlash().ToString());
            }

            if (RelatedTo != null)
            {
                command.CommandText += " AND [RelatedTo] = @RelatedTo";
                command.SetParameter("@RelatedTo", RelatedTo);
            }
        }

        /// <summary>
        /// Appends message name filter criteria to a command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendMessageNameFilterConditions(DbCommand command)
        {
            if (!string.IsNullOrWhiteSpace(MessageName))
            {
                command.CommandText += " AND [MessageName] LIKE @MessageName";
                command.SetParameter("@MessageName", "%" + MessageName + "%");
            }
        }

        /// <summary>
        /// Appends topic filter criteria to a command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendTopicFilterConditions(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (Topics == null) return;

            if (Topics.Any())
            {
                var topicParameters = Topics
                    .Select((topic, i) => new {Name = "@TopicName" + i, Value = topic})
                    .ToList();

                var topicParameterList = string.Join(", ", topicParameters.Select(p => p.Name));
                command.CommandText += " AND [TopicName] IN (" + topicParameterList + ")";
                foreach (var parameter in topicParameters)
                {
                    command.SetParameter(parameter.Name, parameter.Value);
                }
            }
        }

        /// <summary>
        /// Appends category filter criteria to the command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendCategoryFilterConditions(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (Categories == null) return;
            
            if (Categories.Any())
            {
                var categoryParameters = Categories
                    .Select((topic, i) => new { Name = "@Category" + i, Value = topic })
                    .ToList();

                var categoryParameterList = string.Join(", ", categoryParameters.Select(p => p.Name));
                command.CommandText += " AND [Category] IN (" + categoryParameterList + ")";
                foreach (var parameter in categoryParameters)
                {
                    command.SetParameter(parameter.Name, parameter.Value);
                }
            }
        }

        /// <summary>
        /// Appends sort criteria to the command
        /// </summary>
        /// <param name="command">The command</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void AppendSort(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            command.CommandText += " ORDER BY [Id]";
        }
    }
}
