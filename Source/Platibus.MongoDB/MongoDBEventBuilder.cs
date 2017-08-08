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

using Platibus.Diagnostics;

namespace Platibus.MongoDB
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events
    /// </summary>
    public class MongoDBEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The name of the MongoDB database
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The name of the MongoDB collection
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// The name of the MongoDB index
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Initializes a new <see cref="MongoDBEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public MongoDBEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new MongoDBEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, DatabaseName, CollectionName, IndexName);
        }
    }
}
