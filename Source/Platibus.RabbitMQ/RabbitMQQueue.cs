using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    class RabbitMQQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly QueueName _queueName;
        private readonly IQueueListener _listener;
        private readonly IConnection _connection;
        private readonly Encoding _encoding;
        private readonly bool _autoAcknowledge;
        private readonly int _concurrencyLimit;
        private readonly Task[] _consumerTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        
        public RabbitMQQueue(QueueName queueName, IQueueListener listener, IConnection connection, Encoding encoding = null, QueueOptions options = default(QueueOptions))
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            if (listener == null) throw new ArgumentNullException("listener");
            if (connection == null) throw new ArgumentNullException("connection");

            _queueName = queueName;
            _listener = listener;
            _connection = connection;
            _encoding = encoding ?? Encoding.UTF8;
            _autoAcknowledge = options.AutoAcknowledge;
            _concurrencyLimit = Math.Max(options.ConcurrencyLimit, 1);
            _consumerTasks = new Task[_concurrencyLimit];
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Init()
        {
            for (var i = 0; i < _concurrencyLimit; i++)
            {
                _consumerTasks[i] = Task.Run(() => ConsumeMessages(_cancellationTokenSource.Token));
            }
        }

        public async Task Enqueue(Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(_queueName, true, false, false, null);
                using (var stringWriter = new StringWriter())
                using (var messageWriter = new MessageWriter(stringWriter))
                {
                    await messageWriter.WritePrincipal(senderPrincipal);
                    await messageWriter.WriteMessage(message);
                    var messageBody = stringWriter.ToString();
                    channel.BasicPublish("", _queueName, null, _encoding.GetBytes(messageBody));
                }
            }
        }

        private async Task ConsumeMessages(CancellationToken cancellationToken)
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(_queueName, true, false, false, null);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(_queueName, _autoAcknowledge, consumer);
                while (!cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var delivery = consumer.Queue.Dequeue();

                    Log.DebugFormat("Received message from RabbitMQ queue \"{0}\" with delivery tag {1}...", _queueName, delivery.DeliveryTag);
                    
                    try
                    {
                        var messageBody = _encoding.GetString(delivery.Body);
                        using (var reader = new StringReader(messageBody))
                        using (var messageReader = new MessageReader(reader))
                        {
                            var principal = await messageReader.ReadPrincipal();
                            var message = await messageReader.ReadMessage();
                            var context = new RabbitMQQueuedMessageContext(message.Headers, principal);
                            await _listener.MessageReceived(message, context, cancellationToken);

                            if (context.Acknowledged && !_autoAcknowledge)
                            {
                                Log.DebugFormat("Acknowledging message from RabbitMQ queue \"{0}\" with delivery tag {1}...", _queueName, delivery.DeliveryTag);
                                channel.BasicAck(delivery.DeliveryTag, false);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Error consuming message from RabbitMQ queue \"{0}\" with delivery tag {1}", e, _queueName, delivery.DeliveryTag);
                    }
                }    
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~RabbitMQQueue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
