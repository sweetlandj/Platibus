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
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Queueing;
using Platibus.Security;

namespace Platibus.Filesystem
{
    /// <summary>
    /// A see <see cref="IMessageQueueingService"/> that queues messages as files on disk
    /// </summary>
    public class FilesystemMessageQueueingService : AbstractMessageQueueingService<FilesystemMessageQueue>
    {
        private readonly DirectoryInfo _baseDirectory;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="FilesystemMessageQueueingService"/> that will create
        /// directories and files relative to the specified <paramref name="baseDirectory"/>
        /// </summary>
        /// <param name="baseDirectory">(Optional) The directory in which queued message files
        /// will be stored</param>
        /// <param name="securityTokenService">(Optional) The message security token
        /// service to use to issue and validate security tokens for persisted messages.</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <remarks>
        /// <para>If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\queues</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created in the
        /// <see cref="Init"/> method.</para>
        /// <para>If a <paramref name="securityTokenService"/> is not specified then a
        /// default implementation based on unsigned JWTs will be used.</para>
        /// </remarks>
        public FilesystemMessageQueueingService(DirectoryInfo baseDirectory = null, ISecurityTokenService securityTokenService = null, IDiagnosticService diagnosticService = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "queues"));
            }
            _baseDirectory = baseDirectory;
            _securityTokenService = securityTokenService ?? new JwtSecurityTokenService();
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }
        
        /// <inheritdoc />
        protected override Task<FilesystemMessageQueue> InternalCreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var queueDirectory = new DirectoryInfo(Path.Combine(_baseDirectory.FullName, queueName));
            var queue = new FilesystemMessageQueue(queueDirectory, _securityTokenService, queueName, listener, options, _diagnosticService);
            return Task.FromResult(queue);
        }
        
        /// <summary>
        /// Initializes the fileystem queueing service
        /// </summary>
        /// <remarks>
        /// Creates directories if they do not exist
        /// </remarks>
        public void Init()
        {
            _baseDirectory.Refresh();
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }
        }
    }
}