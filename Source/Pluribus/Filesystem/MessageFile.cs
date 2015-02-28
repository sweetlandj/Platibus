// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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

namespace Pluribus.Filesystem
{
    internal class MessageFile : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Filesystem);

        private bool _disposed;
        private readonly FileInfo _file;
        private readonly SemaphoreSlim _fileAccess = new SemaphoreSlim(1);

        private volatile Message _message;
        private volatile IPrincipal _principal;

        public FileInfo File
        {
            get { return _file; }
        }

        public MessageFile(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            _file = file;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static async Task<MessageFile> Create(DirectoryInfo directory, Message message, IPrincipal senderPrincipal)
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
            }
            while (file.Exists);
            return await Create(directory, message, senderPrincipal, file);
        }

        private static async Task<MessageFile> Create(DirectoryInfo directory, Message message, IPrincipal senderPrincipal, FileInfo file)
        {
            Log.DebugFormat("Creating message file {0} for message ID {1}...", file, message.Headers.MessageId);

            using (var fileStream = file.Open(FileMode.CreateNew, FileAccess.Write))
            using (var messageWriter = new MessageFileWriter(fileStream))
            {
                await messageWriter.WritePrincipal(senderPrincipal).ConfigureAwait(false);
                await messageWriter.WriteMessage(message).ConfigureAwait(false);
            }

            Log.DebugFormat("Message file {0} created successfully", file, message.Headers.MessageId);

            return new MessageFile(file);
        }

        public async Task<IPrincipal> ReadSenderPrincipal(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (_message != null) return _principal;

            await ReadFile(cancellationToken).ConfigureAwait(false);
            return _principal;
        }

        public async Task<Message> ReadMessage(CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (_message != null) return _message;

            await ReadFile(cancellationToken).ConfigureAwait(false);
            return _message;
        }

        private async Task ReadFile(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure that the file hasn't already been read
            if (_message != null) return;

            await _fileAccess.WaitAsync(cancellationToken).ConfigureAwait(false);

            Log.DebugFormat("Reading message file {0}...", _file);

            try
            {
                // Check to see if another thread already read the file.  The principal
                // *could* be null, the message is the only thing we can go by.
                if (_message != null) return;

                using (var fileStream = _file.OpenRead())
                using (var messageReader = new MessageFileReader(fileStream))
                {
                    // Read principal and then message (same order they are written)
                    _principal = await messageReader.ReadPrincipal().ConfigureAwait(false);
                    _message = await messageReader.ReadMessage().ConfigureAwait(false);
                }

                Log.DebugFormat("Message file {0} read successfully", _file);
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

        public async Task<MessageFile> MoveTo(DirectoryInfo destinationDirectory, CancellationToken cancellationToken = default(CancellationToken))
        {
            var newPath = Path.Combine(destinationDirectory.FullName, _file.Name);
            await _fileAccess.WaitAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task Delete()
        {
            CheckDisposed();
            await _fileAccess.WaitAsync().ConfigureAwait(false);

            Log.DebugFormat("Deleting message file {0}...", _file);
            try
            {
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

        ~MessageFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _fileAccess.Dispose();
            }
            _disposed = true;
        }
    }
}