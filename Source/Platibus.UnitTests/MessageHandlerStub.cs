using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.UnitTests
{
    public class MessageHandlerStub : IMessageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(UnitTestLoggingCategories.UnitTests);

        private static readonly AutoResetEvent MessageReceivedEvent = new AutoResetEvent(false);
        private static readonly ConcurrentQueue<object> HandledMessageQueue = new ConcurrentQueue<object>();

        public WaitHandle WaitHandle
        {
            get { return MessageReceivedEvent; }
        }

        public IEnumerable<object> HandledMessages
        {
            get { return HandledMessageQueue; }
        }

        public string Name
        {
            get { return "TestMessageHandler"; }
        }

        public Task HandleMessage(object content, IMessageContext messageContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Log.DebugFormat("Handling message ID {0}...", messageContext.Headers.MessageId);
            HandledMessageQueue.Enqueue(content);
            messageContext.Acknowledge();
            MessageReceivedEvent.Set();
            return Task.FromResult(true);
        }

        public static void Reset()
        {
            object message;
            while (HandledMessageQueue.TryDequeue(out message))
            {
            }
        }
    }
}