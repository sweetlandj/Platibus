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
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Platibus.Journaling;

namespace Platibus.MongoDB
{
    /// <summary>
    /// A <see cref="IMessageJournal"/> implementation that uses a MongoDB database to store
    /// journaled messages
    /// </summary>
    public class MongoDBMessageJournal : IMessageJournal
    {
        /// <summary>
        /// The default name of the collection that will be used to store message journal entries
        /// </summary>
        public const string DefaultCollectionName = "platibus.messageJournal";

        private readonly IMongoCollection<MessageJournalEntryDocument> _messageJournalEntries;

        /// <summary>
        /// Initializes a new <see cref="MongoDBMessageJournal"/> with the specified
        /// <paramref name="connectionStringSettings"/> and <paramref name="databaseName"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string to use to connect to the
        /// MongoDB database</param>
        /// <param name="databaseName">(Optional) The name of the database to use.  If omitted,
        /// the default database identified in the <paramref name="connectionStringSettings"/>
        /// will be used</param>
        /// <param name="collectionName">(Optional) The name of the collection in which 
        /// subscription documents will be stored.  If omitted, the
        /// <see cref="DefaultCollectionName"/> will be used</param>
        public MongoDBMessageJournal(ConnectionStringSettings connectionStringSettings, string databaseName = null, string collectionName = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var myCollectionName = string.IsNullOrWhiteSpace(collectionName)
                ? DefaultCollectionName
                : collectionName;

            var database = MongoDBHelper.Connect(connectionStringSettings, databaseName);
            _messageJournalEntries = database.GetCollection<MessageJournalEntryDocument>(
                myCollectionName,
                new MongoCollectionSettings
                {
                    AssignIdOnInsert = false
                });
        }

        /// <inheritdoc />
        public Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = new CancellationToken())
        {
            var id = new ObjectId(0, 0, 0, 0);
            var pos = new MongoDBMessageJournalPosition(id);
            return Task.FromResult<MessageJournalPosition>(pos);
        }

        /// <inheritdoc />
        public Task Append(Message message, MessageJournalCategory category,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var entry = new MessageJournalEntryDocument
            {
                Timestamp = message.GetJournalTimestamp(category),
                Category = category,
                Topic = message.Headers.Topic,
                MessageId = message.Headers.MessageId,
                MessageName = message.Headers.MessageName,
                Headers = message.Headers.ToDictionary(h => (string) h.Key, h => h.Value),
                Content = message.Content
            };

            return _messageJournalEntries.InsertOneAsync(entry, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var fb = Builders<MessageJournalEntryDocument>.Filter;
            var filterDef = fb.Gte(mje => mje.Id, ((MongoDBMessageJournalPosition) start).Id);
            if (filter != null && filter.Topics.Any())
            {
                filterDef = filterDef & fb.In(e => e.Topic, filter.Topics.Select(t => (string) t));
            }

            if (filter != null && filter.Categories.Any())
            {
                filterDef = filterDef & fb.In(e => e.Category, filter.Categories.Select(c => (string)c));
            }

            var entryDocuments = await _messageJournalEntries.Find(filterDef)
                .Limit(count + 1)
                .ToListAsync(cancellationToken);

            var endOfJournal = entryDocuments.Count <= count;
            var nextId = entryDocuments.Select(e => e.Id).LastOrDefault();
            if (endOfJournal)
            {
                nextId = new ObjectId(nextId.Timestamp, nextId.Machine, nextId.Pid, nextId.Increment + 1);
            }

            var nextPosition = new MongoDBMessageJournalPosition(nextId);
            var entries = new List<MessageJournalEntry>();
            foreach (var entryDocument in entryDocuments.Take(count))
            {
                var position = new MongoDBMessageJournalPosition(entryDocument.Id);
                var timestamp = entryDocument.Timestamp;
                var category = entryDocument.Category;
                var headers = new MessageHeaders(entryDocument.Headers);
                var message = new Message(headers, entryDocument.Content);
                entries.Add(new MessageJournalEntry(category, position, timestamp, message));
            }

            return new MessageJournalReadResult(start, nextPosition, endOfJournal, entries);
        }

        /// <inheritdoc />
        public MessageJournalPosition ParsePosition(string str)
        {
            return MongoDBMessageJournalPosition.Parse(str);
        }
    }
}
