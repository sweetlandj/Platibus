// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Platibus.Queueing;
using Platibus.Security;

namespace Platibus.MongoDB
{
    /// <summary>
    /// A message queue based on a SQL database
    /// </summary>
    public class MongoDBMessageQueue : AbstractMessageQueue
    {
        private readonly IMongoCollection<QueuedMessageDocument> _queuedMessages;
        private readonly ISecurityTokenService _securityTokenService;

        /// <summary>
        /// Initializes a new <see cref="MongoDBMessageQueue"/> with the specified values
        /// </summary>
        /// <param name="database">The MongoDB database</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The object that will be notified when messages are
        /// added to the queue</param>
        /// <param name="securityTokenService"></param>
        /// <param name="options">(Optional) Settings that influence how the queue behaves</param>
        /// <param name="collectionName">(Optional) The name of the collection in which the
        /// queued messages should be stored.  If omitted, the queue name will be used.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="database"/>, <paramref name="queueName"/>, or 
        /// <paramref name="listener"/> are <c>null</c>
        /// </exception>
        public MongoDBMessageQueue(IMongoDatabase database, QueueName queueName, IQueueListener listener, ISecurityTokenService securityTokenService, QueueOptions options = null, string collectionName = null)
            : base(queueName, listener, options)
        {
            if (database == null) throw new ArgumentNullException("database");
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");
            if (securityTokenService == null) throw new ArgumentNullException("securityTokenService");
            
            _securityTokenService = securityTokenService;

            var myCollectionName = string.IsNullOrWhiteSpace(collectionName)
                ? MapToCollectionName(queueName)
                : collectionName;

            _queuedMessages = database.GetCollection<QueuedMessageDocument>(myCollectionName);
            MessageEnqueued += OnMessageEnqueued;
            MessageAcknowledged += OnMessageAcknowledged;
            AcknowledgementFailure += OnAcknowledgementFailure;
            MaximumAttemptsExceeded += OnMaximumAttemptsExceeded;
        }

        private Task OnMessageEnqueued(object source, MessageQueueEventArgs args)
        {
            return InsertQueuedMessage(args.QueuedMessage);
        }

        private Task OnMessageAcknowledged(object source, MessageQueueEventArgs args)
        {
            return DeleteQueuedMessage(args.QueuedMessage);
        }

        private Task OnAcknowledgementFailure(object source, MessageQueueEventArgs args)
        {
            return UpdateQueuedMessage(args.QueuedMessage, null);
        }

        private Task OnMaximumAttemptsExceeded(object source, MessageQueueEventArgs args)
        {
            return UpdateQueuedMessage(args.QueuedMessage, DateTime.UtcNow);
        }

        /// <inheritdoc />
        public override async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            var ikb = Builders<QueuedMessageDocument>.IndexKeys;

            var queueMessageId = ikb.Ascending(qm => qm.Queue).Ascending(qm => qm.MessageId);
            var unique = new CreateIndexOptions {Unique = true};
            await _queuedMessages.Indexes.CreateOneAsync(queueMessageId, unique, cancellationToken);

            var state = ikb.Ascending(qm => qm.State);
            await _queuedMessages.Indexes.CreateOneAsync(state, cancellationToken: cancellationToken);

            await base.Init(cancellationToken);
        }

        private static string MapToCollectionName(QueueName queueName)
        {
            var collectionName = queueName.ToString()
                .Replace(" ", "_")
                .Replace("$", "_");

            const string reservedNamespace = "system.";
            if (collectionName.StartsWith(reservedNamespace, StringComparison.OrdinalIgnoreCase))
            {
                collectionName = collectionName.Substring(reservedNamespace.Length);
            }
            return collectionName;
        }

        /// <summary>
        /// Inserts a new queued message into the database
        /// </summary>
        /// <param name="queuedMessage">The queued message to insert</param>
        /// <param name="cancellationToken">(Optional) A cancellation token through which the
        /// caller can request cancellation of the insert operation</param>
        /// <returns>Returns a task that completes when the insert operation completes</returns>
        protected virtual async Task InsertQueuedMessage(QueuedMessage queuedMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = queuedMessage.Message;
            var principal = queuedMessage.Principal;
            var messageId = message.Headers.MessageId;
            var expires = message.Headers.Expires;
            var securityToken = await _securityTokenService.NullSafeIssue(principal, expires);
            var messageWithSecurityToken = message.WithSecurityToken(securityToken);
            var queuedMessageDocument = new QueuedMessageDocument
            {
                Queue = QueueName,
                MessageId = messageId,
                Headers = messageWithSecurityToken.Headers.ToDictionary(h => (string)h.Key, h => h.Value),
                Content = messageWithSecurityToken.Content
            };

            await _queuedMessages.InsertOneAsync(queuedMessageDocument, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes a queued message from the database
        /// </summary>
        /// <param name="queuedMessage">The queued message to delete</param>
        /// <param name="cancellationToken">(Optional) A cancellation token through which the
        /// caller can request cancellation of the delete operation</param>
        /// <returns>Returns a task that completes when the delete operation completes</returns>
        protected virtual async Task DeleteQueuedMessage(QueuedMessage queuedMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = queuedMessage.Message;
            var messageHeaders = message.Headers;
            var messageId = messageHeaders.MessageId;
            var fb = Builders<QueuedMessageDocument>.Filter;
            var filter = fb.Eq(qm => qm.Queue, (string)QueueName) &
                         fb.Eq(qm => qm.MessageId, (string)messageId);
            await _queuedMessages.DeleteOneAsync(filter, cancellationToken);
        }

        /// <summary>
        /// Updates a queued message in the database i.e. in response to an acknowledgement failure
        /// </summary>
        /// <param name="queuedMessage">The queued message to delete</param>
        /// <param name="abandoned">The date/time the message was abandoned (if applicable)</param>
        /// <param name="cancellationToken">(Optional) A cancellation token through which the
        ///     caller can request cancellation of the update operation</param>
        /// <returns>Returns a task that completes when the update operation completes</returns>
        protected virtual Task UpdateQueuedMessage(QueuedMessage queuedMessage, DateTime? abandoned, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = queuedMessage.Message;
            var messageHeaders = message.Headers;
            var messageId = messageHeaders.MessageId;

            var state = QueuedMessageState.Pending;
            if (abandoned != null)
            {
                state = QueuedMessageState.Dead;
            }
            
            var fb = Builders<QueuedMessageDocument>.Filter;
            var filter = fb.Eq(qm => qm.Queue, (string) QueueName) &
                         fb.Eq(qm => qm.MessageId, (string) messageId);

            var update = Builders<QueuedMessageDocument>.Update
                .Set(qm => qm.Attempts, queuedMessage.Attempts)
                .Set(qm => qm.State, state)
                .Set(qm => qm.Abandoned, abandoned);

            return _queuedMessages.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Returns messages in the queue that are pending
        /// </summary>
        /// <param name="cancellationToken">(Optional) A token provided by the caller that can
        /// be used by the caller to request cancellation of the fetch operation</param>
        /// <returns>Returns the set of pending messages in the queue</returns>
        protected override async Task<IEnumerable<QueuedMessage>> GetPendingMessages(CancellationToken cancellationToken = default(CancellationToken))
        {
            var fb = Builders<QueuedMessageDocument>.Filter;
            var filter = fb.Eq(qm => qm.Queue, (string)QueueName) &
                         fb.Eq(qm => qm.State, QueuedMessageState.Pending);

            var existingMessages = await _queuedMessages.Find(filter).ToListAsync(cancellationToken);
            var queuedMessages = new List<QueuedMessage>();
            foreach (var queuedMessage in existingMessages)
            {
                var messageHeaders = new MessageHeaders(queuedMessage.Headers);
                var principal = await _securityTokenService.NullSafeValidate(messageHeaders.SecurityToken);
                var message = new Message(messageHeaders, queuedMessage.Content);
                queuedMessages.Add(new QueuedMessage(message, principal, queuedMessage.Attempts));
            }
            return queuedMessages;
        }

        /// <summary>
        /// Returns messages in the queue that are dead
        /// </summary>
        /// <param name="startDate">The start date for the target date range</param>
        /// <param name="endDate">The end date for the target date range</param>
        /// <param name="cancellationToken">(Optional) A token provided by the caller that can
        /// be used by the caller to request cancellation of the fetch operation</param>
        /// <returns>Returns the set of pending messages in the queue</returns>
        protected async Task<IEnumerable<QueuedMessage>> GetDeadMessages(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fb = Builders<QueuedMessageDocument>.Filter;
            var filter = fb.Eq(qm => qm.Queue, (string)QueueName) &
                         fb.Eq(qm => qm.State, QueuedMessageState.Dead);

            var existingMessages = await _queuedMessages.Find(filter).ToListAsync(cancellationToken);
            var queuedMessages = new List<QueuedMessage>();
            foreach (var queuedMessage in existingMessages)
            {
                var messageHeaders = new MessageHeaders(queuedMessage.Headers);
                var principal = await _securityTokenService.NullSafeValidate(messageHeaders.SecurityToken);
                var message = new Message(messageHeaders, queuedMessage.Content);
                queuedMessages.Add(new QueuedMessage(message, principal, queuedMessage.Attempts));
            }
            return queuedMessages;
        }
    }
}