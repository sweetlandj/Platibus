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
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    internal class DurableConsumer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly string _queueName;
        private readonly string _consumerTag;
        private readonly ushort _concurrencyLimit;
        private readonly Action<IModel, BasicDeliverEventArgs, CancellationToken> _consume;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        private Task _consumerTask;
        private volatile IConnection _connection;
        private volatile IModel _channel;
        
        private bool _disposed;

        public DurableConsumer(IConnection connection, string queueName, 
            Action<IModel, BasicDeliverEventArgs, CancellationToken> consume, 
            string consumerTag = null, int concurrencyLimit = 0,
            bool autoAcknowledge = false)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException("queueName");
            if (connection == null) throw new ArgumentNullException("connection");
            if (consume == null) throw new ArgumentNullException("consume");
            _connection = connection;
            _queueName = queueName;
            _consume = consume;
            _consumerTag = consumerTag;
            _concurrencyLimit = concurrencyLimit > 0 
                ? (ushort)concurrencyLimit
                : (ushort)QueueOptions.DefaultConcurrencyLimit;

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
            EventingBasicConsumer consumer = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_channel == null || !_channel.IsOpen)
                    {
                        _channel = CreateChannel(cancellationToken);
                        consumer = null;
                    }

                    var interruption = new TaskCompletionSource<bool>();
                    cancellationToken.Register(() => interruption.TrySetResult(true));
                    _channel.CallbackException += (sender, args) => interruption.TrySetResult(true);
                    _channel.ModelShutdown += (sender, args) => interruption.TrySetResult(true);

                    if (consumer == null)
                    {
                        Log.DebugFormat("Initializing consumer '{0}' on channel number '{1}' for queue '{2}'...",
                            _consumerTag, _channel.ChannelNumber, _queueName);
                        consumer = new EventingBasicConsumer(_channel);
                        consumer.Received += (sender, args) =>
                        {
                            Log.DebugFormat("Consumer '{0}' received delivery '{1}' from queue '{2}'", _consumerTag,
                                args.DeliveryTag, _queueName);
                            try
                            {
                                _consume(_channel, args, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                Log.ErrorFormat(
                                    "Unhandled exception in callback (consumer '{0}', channel '{1}', queue '{2}'", ex,
                                    _consumerTag, _channel, _queueName);
                                _channel.BasicNack(args.DeliveryTag, true, false);
                            }
                        };

                        _channel.BasicConsume(_queueName, _autoAcknowledge, _consumerTag, consumer);
                    }

                    interruption.Task.Wait(cancellationToken);
                    TryCancelConsumer();
                    TryCloseChannel();
                }
                catch (OperationCanceledException ex)
                {
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unhandled exception consuming messages from queue \"{0}\"", ex, _queueName);
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
                    Log.DebugFormat("Attempting to create RabbitMQ channel for consumer '{0}' of queue '{1}'...", _consumerTag, _queueName);
                    channel = _connection.CreateModel();
                    Log.DebugFormat("RabbitMQ channel number \"{0}\" successfully created for consumer '{1}' of queue '{2}'", channel.ChannelNumber, _consumerTag, _queueName);
                    channel.BasicQos(0, _concurrencyLimit, false);
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

        private void TryCancelConsumer()
        {
            if (_channel == null) return;
            if (!_channel.IsOpen) return;
            
            try
            {
                Log.DebugFormat("Canceling consumer '{0}' on channel number '{1}' for queue '{2}'...", _consumerTag, _channel.ChannelNumber, _queueName);
                _channel.BasicCancel(_consumerTag);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Error canceling RabbitMQ consumer {0}", ex, _consumerTag);
            }
        }

        private void TryCloseChannel()
        {
            if (_channel == null) return;
            if (_channel.IsClosed) return;

            try
            {
                Log.DebugFormat("Closing channel number '{0}' for queue '{1}'...", _channel.ChannelNumber, _queueName);
                _channel.Close();
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Error closing RabbitMQ channel number '{0}' for queue '{1}'", ex, _channel.ChannelNumber, _queueName);
            }
            _channel = null;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            _consumerTask.Wait(TimeSpan.FromSeconds(30));
            if (disposing)
            {
                _cancellationTokenSource.TryDispose();
            }
        }

    }
}
