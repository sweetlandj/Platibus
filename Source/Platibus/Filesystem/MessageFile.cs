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
using Platibus.IO;

namespace Platibus.Filesystem
{
    /// <inheritdoc />
    /// <summary>
    /// An abstraction that represents a message stored in a file on disk
    /// </summary>
    public class MessageFile : IDisposable
    {
        private bool _disposed;
        private readonly SemaphoreSlim _fileAccess = new SemaphoreSlim(1);

        private volatile Message _message;

        /// <summary>
        /// The path and filename in which the message is stored
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// Initializes a new <see cref="MessageFile"/> for an message file stored in disk
        /// </summary>
        /// <param name="file">The path and filename in which the message file is stored</param>
        public MessageFile(FileInfo file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        /// <summary>
        /// A factory method used to initialize and store a message file on disk
        /// </summary>
        /// <param name="directory">The directory in which the message file should be created</param>
        /// <param name="message">The message to persist</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        ///     the caller to cancel creation of the message file</param>
        /// <returns>Returns a task whose result is a <see cref="MessageFile"/> representing the 
        /// stored message</returns>
        public static async Task<MessageFile> Create(DirectoryInfo directory, Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            FileInfo file;
            var counter = 0;
            do
            {
                var filename = counter == 0
                    ? $"{message.Headers.MessageId}.pmsg"
                    : $"{message.Headers.MessageId}_{counter}.pmsg";

                var filePath = Path.Combine(directory.FullName, filename);
                file = new FileInfo(filePath);
                counter++;
            } while (file.Exists);

            cancellationToken.ThrowIfCancellationRequested();
            return await Create(message, file, cancellationToken);
        }

        private static async Task<MessageFile> Create(Message message, FileInfo file, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string messageFileContent;
            using (var stringWriter = new StringWriter())
            using (var messageFileWriter = new MessageWriter(stringWriter))
            {
                await messageFileWriter.WriteMessage(message);
                messageFileContent = stringWriter.ToString();
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var fileStream = file.Open(FileMode.CreateNew, FileAccess.Write))
            using (var fileWriter = new StreamWriter(fileStream))
            {
                await fileWriter.WriteAsync(messageFileContent);
            }
            return new MessageFile(file);
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

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check to see if another thread already read the file.  The principal
                // *could* be null, the message is the only thing we can go by.
                if (_message != null) return;

                string messageFileContent;
                using (var fileStream = File.OpenRead())
                using (var fileReader = new StreamReader(fileStream))
                {
                    messageFileContent = await fileReader.ReadToEndAsync();
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (var stringReader = new StringReader(messageFileContent))
                using (var messageFileReader = new MessageReader(stringReader))
                {
                    _message = await messageFileReader.ReadMessage();
                }
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MessageFileFormatException(File.FullName, "Error reading message file", ex);
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
            var newPath = Path.Combine(destinationDirectory.FullName, File.Name);
            await _fileAccess.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.MoveTo(newPath);
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

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                File.Refresh();
                if (File.Exists)
                {
                    File.Delete();
                }
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileAccess.Dispose();
            }
        }
    }
}