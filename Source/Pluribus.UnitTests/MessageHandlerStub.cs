using Common.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus.UnitTests
{
    public class MessageHandlerStub : IMessageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(UnitTestLoggingCategories.UnitTests);

        private static readonly AutoResetEvent MessageReceivedEvent = new AutoResetEvent(false);
        private static readonly ConcurrentQueue<Message> HandledMessageQueue = new ConcurrentQueue<Message>();

        public WaitHandle WaitHandle { get { return MessageReceivedEvent; } }

        public IEnumerable<Message> HandledMessages
        {
            get { return HandledMessageQueue; }
        }

        public string Name
        {
            get { return "TestMessageHandler"; }
        }

        public Task HandleMessage(Message message, IMessageContext messageContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            Log.DebugFormat("Handling message ID {0}...", message.Headers.MessageId);
            HandledMessageQueue.Enqueue(message);
            messageContext.Acknowledge();
            MessageReceivedEvent.Set();
            return Task.FromResult(true);
        }

        public static void Reset()
        {
            Message message;
            while (HandledMessageQueue.TryDequeue(out message))
            {
            }
        }
    }
}
