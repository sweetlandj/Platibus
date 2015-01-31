using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.Logging;
using Pluribus.SampleWebApp.Models;

namespace Pluribus.SampleWebApp.Controllers
{
    public class ReceivedMessageRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        public static readonly ReceivedMessageRepository SingletonInstance;

        static ReceivedMessageRepository()
        {
            SingletonInstance = new ReceivedMessageRepository();
        }

        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly ConcurrentDictionary<MessageId, ReceivedMessage> _messages = new ConcurrentDictionary<MessageId, ReceivedMessage>();

        public ReceivedMessageRepository()
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                   Process.GetCurrentProcess().Id,
                   Thread.CurrentThread.ManagedThreadId,
                   AppDomain.CurrentDomain.Id);

            Log.DebugFormat("Creating new ReceivedMessageRepository instance {0}...", _instanceId);
            
        }

        public IEnumerable<ReceivedMessage> GetMessages()
        {
            Log.DebugFormat("Retrieving messages ({0})...", _instanceId);
            return _messages.Values.ToList();
        }

        public ReceivedMessage Get(MessageId messageId)
        {
            ReceivedMessage message;
            _messages.TryGetValue(messageId, out message);
            return message;
        }

        public void Add(ReceivedMessage message)
        {
            Log.DebugFormat("Adding message ID {0} to received message repository ({1})...", message.MessageId, _instanceId);
            _messages[message.MessageId] = message;
            Log.DebugFormat("Message ID {0} addded successfully (count: {1})", message.MessageId, _messages.Count);
        }

        public void Remove(MessageId messageId)
        {
            Log.DebugFormat("Removing message ID {0} from received message repository...", messageId);
            ReceivedMessage removed;
            if (_messages.TryRemove(messageId, out removed))
            {
                Log.DebugFormat("Successfuly removed message ID {0} from received message repository", messageId);
            }
            else
            {
                Log.InfoFormat("Message ID {0} not found in message repository", messageId);
            }
        }

        public void RemoveAll()
        {
            Log.Debug("Removing all messages from received message repository...");
            _messages.Clear();
        }
    }
}