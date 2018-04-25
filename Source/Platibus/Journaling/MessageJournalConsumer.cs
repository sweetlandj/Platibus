// The MIT License (MIT)
// 
// Copyright (c) 2018 Jesse Sweetland
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
#if NET452 || NET461
using System.Configuration;
#endif
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.Queueing;
using Platibus.Utils;

namespace Platibus.Journaling
{
    /// <inheritdoc />
    /// <summary>
    /// Consumes messages from a message journal according to configured message handling rules
    /// </summary>
    public class MessageJournalConsumer : IDisposable
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly IMessageJournal _messageJournal;
        private readonly Task<IBus> _bus;
        private readonly MessageJournalFilter _filter;
        private readonly int _batchSize;
        private readonly bool _rethrowExceptions;
        private readonly bool _haltAtEndOfJournal;
        private readonly TimeSpan _pollingInterval;
        private readonly IProgress<MessageJournalConsumerProgress> _progress;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Journaling.MessageJournalConsumer" /> with the specified configuration
        /// and services
        /// </summary>
        /// <param name="configuration">The bus configuration</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if any of the parameters are <c>null</c></exception>
        public MessageJournalConsumer(IPlatibusConfiguration configuration) : this(configuration, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Journaling.MessageJournalConsumer" /> with the specified configuration
        /// and services
        /// </summary>
        /// <param name="bus">A configured and initialized bus instance</param>
        /// <param name="messageJournal">The message journal from which entries will be consumed</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if any of the parameters are <c>null</c></exception>
        public MessageJournalConsumer(IBus bus, IMessageJournal messageJournal) : this(bus, messageJournal, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MessageJournalConsumer"/> with the specified configuration
        /// and services
        /// </summary>
        /// <remarks>
        /// Message journal consumers created with this constructor use an internal <see cref="Bus"/>
        /// instance based on the <see cref="LoopbackTransportService"/> and <see cref="VirtualMessageQueueingService"/>
        /// to minimize overhead of message processing.
        /// </remarks>
        /// <param name="configuration">The bus configuration</param>
        /// <param name="options">(Optional) Options to customize the behavior of the message journal consumer</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are <c>null</c></exception>
        public MessageJournalConsumer(IPlatibusConfiguration configuration, MessageJournalConsumerOptions options)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _diagnosticService = configuration.DiagnosticService;
            _messageJournal = configuration.MessageJournal ?? throw new ConfigurationErrorsException("Message journal is required");
            _bus = InitBus(configuration);
            
            var myOptions = options ?? new MessageJournalConsumerOptions();

            _filter = myOptions.Filter;
            _batchSize = myOptions.BatchSize > 0
                ? myOptions.BatchSize
                : MessageJournalConsumerOptions.DefaultBatchSize;

            _rethrowExceptions = myOptions.RethrowExceptions;
            _haltAtEndOfJournal = myOptions.HaltAtEndOfJournal;
            _pollingInterval = myOptions.PollingInterval > TimeSpan.Zero 
                ? myOptions.PollingInterval 
                : MessageJournalConsumerOptions.DefaultPollingInterval;

            _progress = myOptions.Progress;
        }

        /// <summary>
        /// Initializes a new <see cref="MessageJournalConsumer"/> with the specified configuration
        /// and services
        /// </summary>
        /// <param name="bus">A configured and initialized bus instance that will be used to handle
        /// incoming messages</param>
        /// <param name="messageJournal">The message journal from which entries will be consumed</param>
        /// <param name="options">(Optional) Options to customize the behavior of the message journal consumer</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are <c>null</c></exception>
        public MessageJournalConsumer(IBus bus, IMessageJournal messageJournal, MessageJournalConsumerOptions options)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _diagnosticService = DiagnosticService.DefaultInstance;
            _messageJournal = messageJournal ?? throw new ConfigurationErrorsException("Message journal is required");
            _bus = Task.FromResult(bus);
            
            var myOptions = options ?? new MessageJournalConsumerOptions();

            _filter = myOptions.Filter;
            _batchSize = myOptions.BatchSize > 0
                ? myOptions.BatchSize
                : MessageJournalConsumerOptions.DefaultBatchSize;

            _rethrowExceptions = myOptions.RethrowExceptions;
            _haltAtEndOfJournal = myOptions.HaltAtEndOfJournal;
            _pollingInterval = myOptions.PollingInterval > TimeSpan.Zero 
                ? myOptions.PollingInterval 
                : MessageJournalConsumerOptions.DefaultPollingInterval;

            _progress = myOptions.Progress;
        }

        private static async Task<IBus> InitBus(IPlatibusConfiguration configuration)
        {
            var transportService = new LoopbackTransportService();
            var messageQueuingService = new VirtualMessageQueueingService();

            var bus = new Bus(configuration, new Uri("urn:loopback"), transportService, messageQueuingService);
            await bus.Init();
            return bus;
        }

        /// <summary>
        /// Consumes entries from the message journal starting at the specified <paramref name="start"/>
        /// position
        /// </summary>
        /// <param name="start">The position of the first journal entry to consume</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to interrupt the message consumption process</param>
        /// <returns>Returns a task that completes when the end of the journal is reached, when an
        /// exception is thrown, or when cancellation is requested, depending on the options
        /// specified in the constructor</returns>
        public Task Consume(MessageJournalPosition start,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ConsumeAsync(start, cancellationToken)
                .GetCompletionSource(cancellationToken)
                .Task;
        }

        /// <summary>
        /// Consumes entries from the message journal starting at the specified <paramref name="start"/>
        /// position
        /// </summary>
        /// <param name="start">The position of the first journal entry to consume</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to interrupt the message consumption process</param>
        /// <returns>Returns a task that completes when the end of the journal is reached, when an
        /// exception is thrown, or when cancellation is requested, depending on the options
        /// specified in the constructor</returns>
        private async Task ConsumeAsync(MessageJournalPosition start,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var count = 0L;
            var bus = await _bus;
            var current = start ?? await _messageJournal.GetBeginningOfJournal(cancellationToken);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var readResult = await ReadNext(cancellationToken, current);
                    if (readResult != null)
                    {
                        count = await HandleMessageJournalEntries(count, readResult, bus, cancellationToken);

                        current = readResult.Next;
                        if (readResult.EndOfJournal && _haltAtEndOfJournal)
                        {
                            return;
                        }
                    }
                    await Task.Delay(_pollingInterval, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task<MessageJournalReadResult> ReadNext(CancellationToken cancellationToken, MessageJournalPosition current)
        {
            MessageJournalReadResult readResult = null;
            try
            {
                readResult = await _messageJournal.Read(current, _batchSize, _filter, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.UnhandledException)
                {
                    Detail = "Error reading message journal",
                    Exception = ex
                }.Build());

                if (_rethrowExceptions) throw;
            }

            return readResult;
        }

        private async Task<long> HandleMessageJournalEntries(long count, MessageJournalReadResult result, IBus bus, CancellationToken cancellationToken)
        {
            var entries = result.Entries;
            using (var entryEnumerator = entries.GetEnumerator())
            {
                if (!entryEnumerator.MoveNext())
                {
                    return count;
                }
                
                // Handle messages [0, ..., n-2]
                var current = entryEnumerator.Current;
                count++;
                while (entryEnumerator.MoveNext())
                {
                    var next = entryEnumerator.Current;
                    await HandleMessageJournalEntry(bus, cancellationToken, current, next.Position, count);
                    count++;
                }

                // Handle message [n-1]
                await HandleMessageJournalEntry(bus, cancellationToken, current, result.Next, count);
                count++;
            }

            return count;
        }

        private async Task HandleMessageJournalEntry(IBus bus, CancellationToken cancellationToken, MessageJournalEntry current, MessageJournalPosition next, long count)
        {
            try
            {
                // Throws MessageNotAcknowledgedException if there are handling rules that apply to
                // the message but the message is not acknowledged (automatically or otherwise)
                await bus.HandleMessage(current.Data, Thread.CurrentPrincipal, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (MessageNotAcknowledgedException ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                {
                    Message = current.Data,
                    Detail = "Message was not acknowledged by any handlers",
                    Exception = ex
                }.Build());

                if (_rethrowExceptions) throw;
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.UnhandledException)
                {
                    Message = current.Data,
                    Detail = "Unhandled exception thrown by one or more message handlers",
                    Exception = ex
                }.Build());

                if (_rethrowExceptions) throw;
            }
            finally
            {
                if (_progress != null)
                {
                    var progressReport = new MessageJournalConsumerProgress(
                        count, current.Timestamp, current.Position, next, false);

                    _progress.Report(progressReport);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_bus is IDisposable disposableBus)
            {
                disposableBus.Dispose();
            }
        }
    }
}
