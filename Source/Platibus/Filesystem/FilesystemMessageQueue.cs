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
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Queueing;
using Platibus.Security;

namespace Platibus.Filesystem
{
    /// <inheritdoc />
    /// <summary>
    /// A message queue that persists messages to disk so that they survive application restarts
    /// </summary>
    public class FilesystemMessageQueue : AbstractMessageQueue
    {
        private readonly DirectoryInfo _directory;
        private readonly DirectoryInfo _deadLetterDirectory;
        private readonly ISecurityTokenService _securityTokenService;

        private int _initialized;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Filesystem.FilesystemMessageQueue" />
        /// </summary>
        /// <param name="directory">The directory into which message files will be persisted</param>
        /// <param name="securityTokenService">The service used to issue and validate security
        /// tokens stored with the persisted messages to preserve the security context in which
        /// the message was received</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="listener">The listener that will consume messages from the queue</param>
        /// <param name="options">(Optional) Queueing options</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public FilesystemMessageQueue(DirectoryInfo directory, ISecurityTokenService securityTokenService, 
            QueueName queueName, IQueueListener listener, QueueOptions options = null, 
            IDiagnosticService diagnosticService = null)
            : base(queueName, listener, options, diagnosticService)
        {
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _deadLetterDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "dead"));
            _securityTokenService = securityTokenService ?? throw new ArgumentNullException(nameof(securityTokenService));

            MessageEnqueued += OnMessageEnqueued;
            MessageAcknowledged += OnMessageAcknowledged;
            MaximumAttemptsExceeded += OnMaximumAttemptsExceeded;
        }

        private Task OnMessageEnqueued(object source, MessageQueueEventArgs args)
        {
            return CreateMessageFile(args.QueuedMessage);
        }

        private Task OnMessageAcknowledged(object source, MessageQueueEventArgs args)
        {
            return DeleteMessageFile(args.QueuedMessage);
        }
        
        private Task OnMaximumAttemptsExceeded(object source, MessageQueueEventArgs args)
        {
            return MoveToDeadLetterDirectory(args.QueuedMessage);
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<QueuedMessage>> GetPendingMessages(CancellationToken cancellationToken = new CancellationToken())
        {
            var pendingMessages = new List<QueuedMessage>();
            var files = _directory.EnumerateFiles("*.pmsg");
            foreach (var file in files)
            {
                try
                {
                    var messageFile = new MessageFile(file);
                    var message = await messageFile.ReadMessage(cancellationToken);
                    var principal = await _securityTokenService.NullSafeValidate(message.Headers.SecurityToken);
                    var queuedMessage = new QueuedMessage(message, principal);
                    pendingMessages.Add(queuedMessage);
                }
                catch (Exception ex)
                {
                    DiagnosticService.Emit(new FilesystemEventBuilder(this, FilesystemEventType.MessageFileFormatError)
                    {
                        Detail = "Error reading previously queued message file; skipping",
                        Path = file.FullName,
                        Exception = ex
                    }.Build());
                }
            }
            return pendingMessages;
        }

        private async Task CreateMessageFile(QueuedMessage queuedMessage)
        {
            var message = queuedMessage.Message;
            var principal = queuedMessage.Principal;
            var securityToken = await _securityTokenService.NullSafeIssue(principal, message.Headers.Expires);
            var messageWithSecurityToken = message.WithSecurityToken(securityToken);
            var messageFile = await MessageFile.Create(_directory, messageWithSecurityToken);

            await DiagnosticService.EmitAsync(
                new FilesystemEventBuilder(this, FilesystemEventType.MessageFileCreated)
                {
                    Detail = "Message file created",
                    Message = message,
                    Queue = QueueName,
                    Path = messageFile.File.FullName
                }.Build());
        }

        private async Task DeleteMessageFile(QueuedMessage queuedMessage)
        {
            var message = queuedMessage.Message;
            var headers = message.Headers;
            var pattern = headers.MessageId + "*.pmsg";
            var matchingFiles = _directory.EnumerateFiles(pattern);
            foreach (var matchingFile in matchingFiles)
            {
                matchingFile.Delete();
                await DiagnosticService.EmitAsync(
                    new FilesystemEventBuilder(this, FilesystemEventType.MessageFileDeleted)
                    {
                        Detail = "Message file deleted",
                        Message = message,
                        Queue = QueueName,
                        Path = matchingFile.FullName
                    }.Build());
            }
        }

        private async Task MoveToDeadLetterDirectory(QueuedMessage queuedMessage)
        {
            var message = queuedMessage.Message;
            var headers = message.Headers;
            var pattern = headers.MessageId + "*.pmsg";
            var matchingFiles = _directory.EnumerateFiles(pattern);
            foreach (var matchingFile in matchingFiles)
            {
                var messageFile = new MessageFile(matchingFile);
                var deadLetter = await messageFile.MoveTo(_deadLetterDirectory);
                await DiagnosticService.EmitAsync(
                    new FilesystemEventBuilder(this, DiagnosticEventType.DeadLetter)
                    {
                        Detail = "Message file deleted",
                        Message = message,
                        Queue = QueueName,
                        Path = deadLetter.File.FullName
                    }.Build());
            }
        }

        /// <inheritdoc />
        public override async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                _directory.Refresh();
                var directoryAlreadyExisted = _directory.Exists;
                if (!directoryAlreadyExisted)
                {
                    _directory.Create();
                    _directory.Refresh();
                }

                _deadLetterDirectory.Refresh();
                if (!_deadLetterDirectory.Exists)
                {
                    _deadLetterDirectory.Create();
                    _deadLetterDirectory.Refresh();
                }
                
                cancellationToken.ThrowIfCancellationRequested();

                await DiagnosticService.EmitAsync(
                    new FilesystemEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                    {
                        Detail = "Filesystem queue initialized",
                        Queue = QueueName,
                        Path = _directory.FullName
                    }.Build(), cancellationToken);
            }

            await base.Init(cancellationToken);
        }
    }
}