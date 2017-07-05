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
using Platibus.Diagnostics;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platibus.RabbitMQ
{
    internal class DurableConsumer : IDisposable
    {
        private readonly string _queueName;
        private readonly string _consumerTag;
        private readonly ushort _concurrencyLimit;
        private readonly Action<IModel, BasicDeliverEventArgs, CancellationToken> _consume;
        private readonly bool _autoAcknowledge;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IDiagnosticEventSink _diagnosticEventSink;
        
        private volatile IConnection _connection;
        private volatile IModel _channel;
        
        private bool _disposed;

        public DurableConsumer(IConnection connection, string queueName, 
            Action<IModel, BasicDeliverEventArgs, CancellationToken> consume, 
            string consumerTag = null, int concurrencyLimit = 0,
            bool autoAcknowledge = false, IDiagnosticEventSink diagnosticEventSink = null)
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
            _diagnosticEventSink = diagnosticEventSink ?? NoopDiagnosticEventSink.Instance;
        }

        public void Init()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            _channel = CreateChannel(cancellationToken);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) =>
            {
                try
                {
                    _consume(_channel, args, cancellationToken);
                }
                catch (Exception ex)
                {
                    _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                    {
                        Detail = "Unhandled exception in consumer callback",
                        Exception = ex,
                        ChannelNumber = _channel.ChannelNumber,
                        ConsumerTag = _consumerTag,
                        DeliveryTag = args.DeliveryTag,
                    }.Build());

                    _channel.BasicNack(args.DeliveryTag, true, false);
                }
            };

            _channel.BasicConsume(_queueName, _autoAcknowledge, _consumerTag, consumer);
            _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConsumerAdded)
            {
                ChannelNumber = _channel.ChannelNumber,
                ConsumerTag = _consumerTag
            }.Build());
        }

        private IModel CreateChannel(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var channel = _connection.CreateModel();
                    channel.BasicQos(0, _concurrencyLimit, false);

                    _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQChannelCreated)
                    {
                        ChannelNumber = channel.ChannelNumber,
                        ConsumerTag = _consumerTag
                    }.Build());

                    return channel;
                }
                catch (Exception ex)
                {
                    var delay = TimeSpan.FromSeconds(5);
                    _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQChannelCreationFailed)
                    {
                        Detail = "Error creating RabbitMQ channel.  Retrying in " + delay,
                        Exception = ex,
                        ConsumerTag = _consumerTag
                    }.Build());
                    Task.Delay(delay, cancellationToken).Wait(cancellationToken);
                }
            } 
            throw new OperationCanceledException();
        }

        private void TryCancelConsumer()
        {
            var myChannel = _channel;
            if (myChannel == null) return;
            if (!myChannel.IsOpen) return;
            
            try
            {
                myChannel.BasicCancel(_consumerTag);
                _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConsumerCanceled)
                {
                    ChannelNumber = myChannel.ChannelNumber,
                    ConsumerTag = _consumerTag
                }.Build());
            }
            catch (Exception ex)
            {
                _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQConsumerCancelError)
                {
                    ChannelNumber = myChannel.ChannelNumber,
                    ConsumerTag = _consumerTag,
                    Exception = ex
                }.Build());
            }
        }

        private void TryCloseChannel()
        {
            var myChannel = _channel;
            if (myChannel == null) return;
            if (!myChannel.IsOpen) return;

            try
            {
                myChannel.Close();
                _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQChannelClosed)
                {
                    ChannelNumber = myChannel.ChannelNumber,
                    ConsumerTag = _consumerTag
                }.Build());
            }
            catch (Exception ex)
            {
                _diagnosticEventSink.Receive(new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQChannelCloseError)
                {
                    ChannelNumber = myChannel.ChannelNumber,
                    ConsumerTag = _consumerTag,
                    Exception = ex
                }.Build());
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

        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();

            TryCancelConsumer();
            TryCloseChannel();

            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }

    }
}
