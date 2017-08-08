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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using Platibus.Diagnostics;
using Platibus.Journaling;

namespace Platibus.MongoDB
{
    /// <summary>
    /// A <see cref="IMessageJournal"/> implementation that uses a MongoDB database to store
    /// journaled messages
    /// </summary>
    public class MongoDBMessageJournal : IMessageJournal
    {
        private static readonly Collation Collation = new Collation("simple", strength: CollationStrength.Secondary);

        /// <summary>
        /// The default name of the collection that will be used to store message journal entries
        /// </summary>
        public const string DefaultCollectionName = "platibus.messageJournal";

        private readonly IDiagnosticService _diagnosticService;
        private readonly IMongoCollection<MessageJournalEntryDocument> _messageJournalEntries;
        private readonly bool _collationSupported;

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
        /// <param name="diagnosticService">(Optional) The diagnostic service through which
        /// diagnostic events will be emitted</param>
        public MongoDBMessageJournal(ConnectionStringSettings connectionStringSettings, string databaseName = null, string collectionName = null, IDiagnosticService diagnosticService = null)
        {
            _diagnosticService = diagnosticService;
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

            var minServerVersion = database.Client.Cluster.Description.Servers.Min(s => s.Version);
            _collationSupported = Feature.Collation.IsSupported(minServerVersion);

            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var ikb = Builders<MessageJournalEntryDocument>.IndexKeys;
            var options = new CreateIndexOptions();
            if (_collationSupported)
            {
                options.Collation = Collation;
            }

            TryCreateIndex(ikb.Ascending(c => c.Category), "category_1");
            TryCreateIndex(ikb.Ascending(c => c.Topic), "topic_1");
            TryCreateIndex(ikb.Ascending(c => c.MessageName), "messageName_1");
            TryCreateIndex(ikb.Descending(c => c.Timestamp), "timestamp_1");
            TryCreateIndex(ikb.Ascending(c => c.Origination), "origination_1");
            TryCreateIndex(ikb.Ascending(c => c.Destination), "destination_1");
            TryCreateIndex(ikb.Ascending(c => c.RelatedTo), "relatedTo_1");
        }

        private void TryCreateIndex(IndexKeysDefinition<MessageJournalEntryDocument> indexKeys, string name = null)
        {
            var options = new CreateIndexOptions
            {
                Name = name
            };

            if (_collationSupported)
            {
                options.Collation = Collation;
            }

            try
            {
                _messageJournalEntries.Indexes.CreateOne(indexKeys, options);
                _diagnosticService.Emit(new MongoDBEventBuilder(this, MongoDBEventType.IndexCreated)
                {
                    DatabaseName = _messageJournalEntries.Database.DatabaseNamespace.DatabaseName,
                    CollectionName = _messageJournalEntries.CollectionNamespace.CollectionName,
                    IndexName = name ?? indexKeys.ToString()
                }.Build());
            }
            catch (Exception e)
            {
                _diagnosticService.Emit(new MongoDBEventBuilder(this, MongoDBEventType.IndexCreationFailed)
                {
                    DatabaseName = _messageJournalEntries.Database.DatabaseNamespace.DatabaseName,
                    CollectionName = _messageJournalEntries.CollectionNamespace.CollectionName,
                    IndexName = name ?? indexKeys.ToString(),
                    Exception = e
                }.Build());
            }
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
                Category = Normalize(category),
                Topic = Normalize(message.Headers.Topic),
                MessageId = Normalize(message.Headers.MessageId),
                MessageName = message.Headers.MessageName,
                Headers = message.Headers.ToDictionary(h => (string) h.Key, h => h.Value),
                Content = message.Content,
                Origination = Normalize(message.Headers.Origination),
                Destination = Normalize(message.Headers.Destination),
                RelatedTo = Normalize(message.Headers.RelatedTo)
            };

            return _messageJournalEntries.InsertOneAsync(entry, cancellationToken: cancellationToken);
        }

        
        /// <inheritdoc />
        public async Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var fb = Builders<MessageJournalEntryDocument>.Filter;
            var filterDef = fb.Gte(mje => mje.Id, ((MongoDBMessageJournalPosition) start).Id);

            filterDef = BuildFilter(filter, filterDef, fb);

            var options = new FindOptions();
            if (_collationSupported)
            {
                options.Collation = Collation;
            }

            var entryDocuments = await _messageJournalEntries.Find(filterDef, options)
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

        private FilterDefinition<MessageJournalEntryDocument> BuildFilter(MessageJournalFilter filter, FilterDefinition<MessageJournalEntryDocument> filterDef,
            FilterDefinitionBuilder<MessageJournalEntryDocument> fb)
        {
            if (filter == null) return filterDef;
            
            if (filter.Topics.Any())
            {
                var topics = filter.Topics.Select(Normalize);
                filterDef = filterDef & fb.In(e => e.Topic, topics);
            }

            if (filter.Categories.Any())
            {
                var categories = filter.Categories.Select(Normalize);
                filterDef = filterDef & fb.In(e => e.Category, categories);
            }

            if (filter.From != null)
            {
                filterDef = filterDef & fb.Gte(e => e.Timestamp, filter.From);
            }

            if (filter.To != null)
            {
                filterDef = filterDef & fb.Lte(e => e.Timestamp, filter.To);
            }

            if (filter.Origination != null)
            {
                var origination = Normalize(filter.Origination);
                filterDef = filterDef & fb.Eq(e => e.Origination, origination);
            }

            if (filter.Destination != null)
            {
                var destination = Normalize(filter.Destination);
                filterDef = filterDef & fb.Eq(e => e.Destination, destination);
            }

            if (!string.IsNullOrWhiteSpace(filter.MessageName))
            {
                var partial = Normalize(filter.MessageName);
                var pattern = ".*" + Regex.Escape(partial) + ".*";
                var regex = new BsonRegularExpression(pattern, "i");
                filterDef = filterDef & fb.Regex(e => e.MessageName, regex);
            }

            if (filter.RelatedTo != null)
            {
                var relatedTo = Normalize(filter.RelatedTo);
                filterDef = filterDef & fb.Eq(e => e.RelatedTo, relatedTo);
            }

            return filterDef;
        }

        private string Normalize(Uri uri)
        {
            if (uri == null) return null;
            if (_collationSupported) return uri.WithTrailingSlash().ToString();

            return new UriBuilder(uri)
            {
                Scheme = uri.Scheme.ToLower(),
                Host = uri.Host.ToLower()
            }.Uri.WithTrailingSlash().ToString().Trim();
        }

        private string Normalize<T>(T obj)
        {
            if (obj == null) return null;

            var str = obj as string ?? obj.ToString();

            if (string.IsNullOrWhiteSpace(str)) return null;

            return _collationSupported
                ? str.Trim()
                : str.Trim().ToLower();
        }
    }
}
