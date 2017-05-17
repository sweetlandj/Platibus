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
    public class SelectJournaledMessagesCommandBuilder
    {
        private IList<string> _categories;
        private IList<string> _topics;

        /// <summary>
        /// Specifies the categories to which results should be constrained
        /// </summary>
        public IList<string> Categories
        {
            get { return _categories ?? (_categories = new List<string>()); }
            set { _categories = value; }
        }

        /// <summary>
        /// Specifies the topics to which results should be constrained
        /// </summary>
        public IList<string> Topics
        {
            get { return _topics ?? (_topics = new List<string>()); }
            set { _topics = value; }
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
        /// Initializes and returns a new query <see cref="DbCommand"/> with the configured
        /// parameters
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a new query <see cref="DbCommand"/> with the configured
        /// parameters</returns>
        public virtual DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@Count", Count);
            command.SetParameter("@Start", Start);

            AppendTopicFilterConditions(command, Topics);
            AppendCategoryFilterConditions(command, Categories);
            AppendSort(command);

            return command;
        }

        /// <summary>
        /// Reads and parses the raw values from a <see cref="IDataRecord"/> into a 
        /// <see cref="JournaledMessageRecord"/>
        /// </summary>
        /// <param name="dataRecord">A data record read from the query command produced by the
        /// <see cref="BuildDbCommand"/> method.</param>
        /// <returns>Returns a new <see cref="JournaledMessageRecord"/> whose properties are 
        /// initialized according to the data in the supplied <paramref name="dataRecord"/></returns>
        public virtual JournaledMessageRecord BuildJournaledMessageRecord(IDataRecord dataRecord)
        {
            return new JournaledMessageRecord
            {
                Id = dataRecord.GetInt("Id").GetValueOrDefault(),
                Category = dataRecord.GetString("Category"),
                Headers = dataRecord.GetString("Headers"),
                Content = dataRecord.GetString("MessageContent")
            };
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText
        {
            get { return @"
SELECT TOP (@Count)
    [Id],
    [Category],
    [Headers], 
    [MessageContent]
FROM [PB_MessageJournal]
WHERE [Id] >= @Start"; }
        }

        /// <summary>
        /// Appends topic filter criteria to the command 
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="topics">The topics to which the results should be constrained</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        public virtual void AppendTopicFilterConditions(DbCommand command, IList<string> topics)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (topics == null) return;

            if (topics.Any())
            {
                var topicParameters = topics
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
        /// <param name="categories">The categories to which the results should be constrained</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>command</c> is <c>null</c></exception>
        public virtual void AppendCategoryFilterConditions(DbCommand command, IList<string> categories)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (categories == null) return;
            
            if (categories.Any())
            {
                var categoryParameters = categories
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
        public virtual void AppendSort(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            command.CommandText += " ORDER BY [Id]";
        }
    }
}
