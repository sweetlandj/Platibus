// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using Platibus.Diagnostics;
using Platibus.Security;
using Platibus.SQL.Commands;

namespace Platibus.SQL
{
    /// <summary>
    /// Options that influence the behavior of the <see cref="SQLMessageQueueingService"/>
    /// </summary>
    public class SQLMessageQueueingOptions
    {
        /// <summary>
        /// The diagnostic service through which diagnostic events related to SQL
        /// queueing will be raised
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }

        /// <summary>
        /// A factory object that provides connections to the database
        /// </summary>
        public IDbConnectionProvider ConnectionProvider { get; }

        /// <summary>
        /// Dialect-specific factories for creating database commands
        /// </summary>
        public IMessageQueueingCommandBuilders CommandBuilders { get; }

        /// <summary>
        /// The message security token  service to use to issue and validate 
        /// security tokens for persisted messages.
        /// </summary>
        public ISecurityTokenService SecurityTokenService { get; set; }

        /// <summary>
        /// The encryption service used to encrypted persisted message files at rest
        /// </summary>
        public IMessageEncryptionService MessageEncryptionService { get; set; }

        /// <summary>
        /// Initializes <see cref="SQLMessageQueueingOptions"/>
        /// </summary>
        /// <param name="connectionProvider">A factory object that provides 
        /// connections to the database</param>
        /// <param name="commandBuilders">Dialect-specific factories for creating
        /// database commands</param>
        public SQLMessageQueueingOptions(IDbConnectionProvider connectionProvider, IMessageQueueingCommandBuilders commandBuilders)
        {
            ConnectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            CommandBuilders = commandBuilders ?? throw new ArgumentNullException(nameof(commandBuilders));
        }
    }
}
