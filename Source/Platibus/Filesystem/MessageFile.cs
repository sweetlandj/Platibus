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
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Security;

namespace Platibus.Filesystem
{
    /// <summary>
    /// An abstraction that represents a message stored in a file on disk
    /// </summary>
    public class MessageFile : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private bool _disposed;
        private readonly FileInfo _file;
        private readonly SemaphoreSlim _fileAccess = new SemaphoreSlim(1);

        private volatile Message _message;
        private volatile IPrincipal _principal;

        /// <summary>
        /// The path and filename in which the message is stored
        /// </summary>
        public FileInfo File
        {
            get { return _file; }
        }

        /// <summary>
        /// Initializes a new <see cref="MessageFile"/> for an message file stored in disk
        /// </summary>
        /// <param name="file">The path and filename in which the message file is stored</param>
        public MessageFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            _file = file;
        }

        /// <summary>
        /// A factory method used to initialize and store a message file on disk
        /// </summary>
        /// <param name="directory">The directory in which the message file should be created</param>
        /// <param name="message">The message to persist</param>
        /// <param name="principal">(Optional) The principal from which the message was originally
        /// received</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel creation of the message file</param>
        /// <returns>Returns a task whose result is a <see cref="MessageFile"/> representing the 
        /// stored message</returns>
        public static async Task<MessageFile> Create(DirectoryInfo directory, Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            FileInfo file;
            var counter = 0;
            do
            {
                var filename = counter == 0
                    ? string.Format("{0}.pmsg", message.Headers.MessageId)
                    : string.Format("{0}_{1}.pmsg", message.Headers.MessageId, counter);

                var filePath = Path.Combine(directory.FullName, filename);
                file = new FileInfo(filePath);
                counter++;
            } while (file.Exists);

            cancellationToken.ThrowIfCancellationRequested();
            return await Create(message, principal, file, cancellationToken);
        }

        private static async Task<MessageFile> Create(Message message, IPrincipal principal, FileInfo file, CancellationToken cancellationToken = default(CancellationToken))
        {
            Log.DebugFormat("Creating message file {0} for message ID {1}...", file, message.Headers.MessageId);

            cancellationToken.ThrowIfCancellationRequested();

            string messageFileContent;
            using (var stringWriter = new StringWriter())
            using (var messageFileWriter = new MessageFileWriter(stringWriter))
            {
                // Add or update the SecurityToken header in the message and write the message 
                // with the updated headers
                var messageWithSecurityToken = WithSecurityToken(message, principal);
                await messageFileWriter.WriteMessage(messageWithSecurityToken);
                messageFileContent = stringWriter.ToString();
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var fileStream = file.Open(FileMode.CreateNew, FileAccess.Write))
            using (var fileWriter = new StreamWriter(fileStream))
            {
                await fileWriter.WriteAsync(messageFileContent);
            }

            Log.DebugFormat("Message file {0} created successfully", file, message.Headers.MessageId);

            return new MessageFile(file);
        }

        private static Message WithSecurityToken(Message message, IPrincipal principal)
        {
            var securityToken = principal == null ? null : MessageSecurityToken.Create(principal);
            if (message.Headers.SecurityToken == securityToken) return message;

            var updatedHeaders = new MessageHeaders(message.Headers)
            {
                SecurityToken = securityToken
            };
            return new Message(updatedHeaders, message.Content);
        }

        public async Task<IPrincipal> ReadPrincipal(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (_message != null) return _principal;

            await ReadFile(cancellationToken);
            return _principal;
        }

        /// <summary>
        /// Reads the message from the file
        /// </summary>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the read operation</param>
        /// <returns>Returns a task whose result is the <see cref="Message"/> stored in the file</returns>
        public async Task<Message> ReadMessage(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (_message != null) return _message;

            await ReadFile(cancellationToken);
            return _message;
        }

        private async Task ReadFile(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure that the file hasn't already been read
            if (_message != null) return;

            await _fileAccess.WaitAsync(cancellationToken);

            Log.DebugFormat("Reading message file {0}...", _file);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check to see if another thread already read the file.  The principal
                // *could* be null, the message is the only thing we can go by.
                if (_message != null) return;

                string messageFileContent;
                using (var fileStream = _file.OpenRead())
                using (var fileReader = new StreamReader(fileStream))
                {
                    messageFileContent = await fileReader.ReadToEndAsync();
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (var stringReader = new StringReader(messageFileContent))
                using (var messageFileReader = new MessageFileReader(stringReader))
                {
                    _message = await messageFileReader.ReadMessage();
                }

                var securityToken = _message.Headers.SecurityToken;
                if (!string.IsNullOrWhiteSpace(securityToken))
                {
                    _principal = MessageSecurityToken.Validate(securityToken);
                }

                Log.DebugFormat("Message file {0} read successfully", _file);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MessageFileFormatException(_file.FullName, "Error reading message file", ex);
            }
            finally
            {
                _fileAccess.Release();
            }
        }

        /// <summary>
        /// Moves the message file another directory
        /// </summary>
        /// <param name="destinationDirectory">The destination directory</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the move operation</param>
        /// <returns>Returns a task whose result is a new <see cref="MessageFile"/> representing
        /// the file in the new directory</returns>
        public async Task<MessageFile> MoveTo(DirectoryInfo destinationDirectory,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var newPath = Path.Combine(destinationDirectory.FullName, _file.Name);
            await _fileAccess.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _file.MoveTo(newPath);
                return new MessageFile(new FileInfo(newPath));
            }
            finally
            {
                _fileAccess.Release();
            }
        }

        /// <summary>
        /// Deletes a message file from disk
        /// </summary>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the delete operation</param>
        /// <returns>Returns a task that will complete when the delete operation has completed</returns>
        public async Task Delete(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            await _fileAccess.WaitAsync(cancellationToken);

            Log.DebugFormat("Deleting message file {0}...", _file);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _file.Refresh();
                if (_file.Exists)
                {
                    _file.Delete();
                    Log.DebugFormat("Message file {0} deleted successfully", _file);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error deleting message file {0}", ex, _file);
            }
            finally
            {
                _fileAccess.Release();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <inheritdoc />
        ~MessageFile()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources held by this object
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_fileAccess")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileAccess.TryDispose();
            }
        }
    }
}