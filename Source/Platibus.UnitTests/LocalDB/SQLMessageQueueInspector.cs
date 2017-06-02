﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQL;

namespace Platibus.UnitTests.LocalDB
{
    internal class SQLMessageQueueInspector : SQLMessageQueue
    {
        public SQLMessageQueueInspector(SQLMessageQueueingService messageQueueingService, QueueName queueName, ISecurityTokenService securityTokenService)
            : base(
                messageQueueingService.ConnectionProvider, messageQueueingService.CommandBuilders, queueName,
                new NoopQueueListener(), securityTokenService)
        {
        }
        
        public Task<QueuedMessage> InsertMessage(Message testMessage, IPrincipal principal)
        {
            return InsertQueuedMessage(testMessage, principal);
        }

        public Task<IEnumerable<QueuedMessage>> EnumerateMessages()
        {
            return SelectPendingMessages();
        }

        public Task<IEnumerable<QueuedMessage>> EnumerateAbandonedMessages(DateTime startDate, DateTime endDate)
        {
            return SelectDeadMessages(startDate, endDate);
        }

        private class NoopQueueListener : IQueueListener
        {
            public Task MessageReceived(Message message, IQueuedMessageContext context,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(false);
            }
        }
    }
}