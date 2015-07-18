
using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    public class DurableConsumer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly string _queueName;
        private readonly string _consumerTag;
        private readonly Action<IModel, BasicDeliverEventArgs, CancellationToken> _consume;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        private Task _consumerTask;
        private volatile IConnection _connection;
        private volatile IModel _channel;
        
        private bool _disposed;

        public DurableConsumer(IConnection connection, string queueName, Action<IModel, BasicDeliverEventArgs, CancellationToken> consume, string consumerTag = null, bool autoAcknowledge = false)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException("queueName");
            if (connection == null) throw new ArgumentNullException("connection");
            if (consume == null) throw new ArgumentNullException("consume");
            _connection = connection;
            _queueName = queueName;
            _consume = consume;
            _consumerTag = consumerTag;
            _autoAcknowledge = autoAcknowledge;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Init()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            _consumerTask = Task.Run(() => Consume(cancellationToken), cancellationToken);
        }

        public void Reconnect(IConnection newConnection)
        {
            var currentChannel = _channel;
            _connection = newConnection;
            _channel = null;

            try
            {
                currentChannel.Close();
            }
            catch (Exception ex)
            {
                Log.Error("Error closing channel", ex);
            }
        }

        private void Consume(CancellationToken cancellationToken)
        {
            const int dequeueTimeout = 1000;
            QueueingBasicConsumer consumer = null;
            while (!cancellationToken.IsCancellationRequested)
            {   
                try
                {   
                    if (_channel == null || !_channel.IsOpen)
                    {
                        _channel = CreateChannel(cancellationToken);                            
                        consumer = null;
                    }

                    if (consumer == null)
                    {
                        consumer = new QueueingBasicConsumer(_channel);
                        _channel.BasicConsume(_queueName, _autoAcknowledge, _consumerTag, consumer);
                    }

                    BasicDeliverEventArgs delivery;
                    if (consumer.Queue.Dequeue(dequeueTimeout, out delivery))
                    {
                        _consume(_channel, delivery, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error consuming messages from queue \"{0}\"", ex, _queueName);
                }
            }
        }

        private IModel CreateChannel(CancellationToken cancellationToken = default(CancellationToken))
        {
            IModel channel = null;
            while (channel == null || !channel.IsOpen)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    Log.Debug("Attempting to create RabbitMQ channel...");
                    channel = _connection.CreateModel();
                    Log.DebugFormat("RabbitMQ channel number \"{0}\" created successfully", channel.ChannelNumber);
                    channel.BasicQos(0, 1, false);
                    channel.ModelShutdown += (sender, args) =>
                    {
                        _channel = null;
                    };
                }
                catch (Exception ex)
                {
                    var delay = TimeSpan.FromSeconds(5);
                    Log.ErrorFormat("Error creating RabbitMQ channel.  Retrying in {0}...", ex, delay);
                    Task.Delay(delay, cancellationToken).Wait(cancellationToken);
                }
            } 
            return channel;
        }

        ~DurableConsumer()
        {
            if (_disposed) return;
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
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _consumerTask.Wait(TimeSpan.FromSeconds(30));

                if (_channel != null)
                {
                    _channel.Close();
                    _channel = null;
                }

                _cancellationTokenSource.TryDispose();
            }
        }

    }
}
