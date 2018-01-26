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

namespace Platibus.MongoDB
{
    /// <summary>
    /// Options that influence the behavior of <see cref="MongoDBSubscriptionTrackingService"/>
    /// </summary>
    public class MongoDBSubscriptionTrackingOptions
    {
        /// <summary>
        /// The diagnostic service through which events related to MongoDB subscription
        /// tracking will be raised and processed
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }
        
        /// <summary>
        /// The MongoDB database in which subscriptions should be persisted 
        /// </summary>
        public IMongoDatabase Database { get; } 
        
        /// <summary>
        /// The name of the collection in which subscriptions should be persisted
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Initializes a new <see cref="MongoDBSubscriptionTrackingOptions"/>
        /// </summary>
        /// <param name="database">The MongoDB database in which subscriptions should be persisted</param>
        public MongoDBSubscriptionTrackingOptions(IMongoDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }
    }
}
