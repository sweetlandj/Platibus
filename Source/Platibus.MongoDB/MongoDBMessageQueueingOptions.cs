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
using MongoDB.Driver;
using Platibus.Diagnostics;
using Platibus.Security;

namespace Platibus.MongoDB
{
    /// <summary>
    /// Settings governing the behavior of a <see cref="MongoDBMessageQueueingService"/>
    /// </summary>
    public class MongoDBMessageQueueingOptions
    {
        /// <summary>
        /// The diagnostic service through which events related to MongoDB queueing will
        /// be raised and processed
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }
        
        /// <summary>
        /// The MongoDB database in which queued messages should be persisted 
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        /// A factory method that identifies the name of the collection to use for
        /// a given queue name
        /// </summary>
        public QueueCollectionNameFactory CollectionNameFactory { get; set; }

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
        /// Initializes new <see cref="MongoDBMessageQueueingOptions"/>
        /// </summary>
        /// <param name="database">The MongoDB database in which queued messages should be persisted</param>
        public MongoDBMessageQueueingOptions(IMongoDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }
    }
}
