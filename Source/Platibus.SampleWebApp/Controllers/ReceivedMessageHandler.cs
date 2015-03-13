using Common.Logging;
using Platibus.SampleWebApp.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleWebApp.Controllers
{
    public class ReceivedMessageHandler : IMessageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        private readonly ReceivedMessageRepository _repository;

        public ReceivedMessageHandler(ReceivedMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleMessage(object message, IMessageContext context, CancellationToken cancellationToken)
        {
            var testMessage = (TestMessage)message;
         
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            var headers = context.Headers;
            Log.DebugFormat("Handling {0} ID {1} sent from {2} by {3} at {4:o} and received {5:o}...", 
                headers.MessageName,
                headers.MessageId, 
                headers.Origination,
                context.SenderPrincipal.GetName(),
                headers.Sent,
                headers.Received);

            var receivedMessage = new ReceivedMessage
            {
                SenderPrincipal = context.SenderPrincipal == null ? null : context.SenderPrincipal.Identity.Name,
                MessageId = headers.MessageId,
                MessageName = headers.MessageName,
                Origination = headers.Origination == null ? "" : headers.Origination.ToString(),
                Destination = headers.Destination == null ? "" : headers.Destination.ToString(),
                RelatedTo = headers.RelatedTo,
                Sent = headers.Sent,
                Received = headers.Received,
                Expires = headers.Expires,
                ContentType = headers.ContentType,
                Content = testMessage == null ? "" : testMessage.Text
            };

            await _repository.Add(receivedMessage);
            context.Acknowledge();

            Log.DebugFormat("{0} ID {1} handled successfully",
                headers.MessageName,
                headers.MessageId);
        }
    }
}