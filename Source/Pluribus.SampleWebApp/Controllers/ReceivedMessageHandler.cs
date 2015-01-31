using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Pluribus.SampleWebApp.Models;

namespace Pluribus.SampleWebApp.Controllers
{
    public class ReceivedMessageHandler : IMessageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        public ReceivedMessageHandler()
        {
            Log.DebugFormat("Initializing ReceivedMessageHandler [Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);
        }

        public Task HandleMessage(Message message, IMessageContext context, CancellationToken cancellationToken)
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            Log.DebugFormat("Handling {0} ID {1} sent from {2} by {3} at {4:o} and received {5:o}...", 
                message.Headers.MessageName,
                message.Headers.MessageId, 
                message.Headers.Origination,
                context.SenderPrincipal.GetName(),
                message.Headers.Sent,
                message.Headers.Received);

            var receivedMessage = new ReceivedMessage
            {
                SenderPrincipal = context.SenderPrincipal == null ? null : context.SenderPrincipal.Identity.Name,
                MessageId = message.Headers.MessageId,
                MessageName = message.Headers.MessageName,
                Origination = message.Headers.Origination == null ? "" : message.Headers.Origination.ToString(),
                RelatedTo = message.Headers.RelatedTo,
                Sent = message.Headers.Sent,
                Received = message.Headers.Received,
                Expires = message.Headers.Expires,
                ContentType = message.Headers.ContentType,
                Content = message.Content
            };

            var repository = ReceivedMessageRepository.SingletonInstance;
            repository.Add(receivedMessage);
            context.Acknowledge();

            Log.DebugFormat("{0} ID {1} handled successfully",
                message.Headers.MessageName,
                message.Headers.MessageId);

            return Task.FromResult(true);
        }
    }
}