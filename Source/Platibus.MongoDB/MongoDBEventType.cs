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
    /// Type of <see cref="DiagnosticEvent"/>s related to MongoDB operations
    /// </summary>
    public class MongoDBEventType
    {
        /// <summary>
        /// Thrown when an index is created
        /// </summary>
        public static readonly DiagnosticEventType IndexCreated = new DiagnosticEventType("IndexCreated");

        /// <summary>
        /// Thrown when an index creation operation fails
        /// </summary>
        public static readonly DiagnosticEventType IndexCreationFailed = new DiagnosticEventType("IndexCreationFailed", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted when message headers or content cannot be read from the database
        /// </summary>
        public static readonly DiagnosticEventType MessageDocumentFormatError = new DiagnosticEventType("MessageDocumentFormatError", DiagnosticEventLevel.Error);
        
        /// <summary>
        /// Emitted when a MongoDB insert operation fails
        /// </summary>
        public static readonly DiagnosticEventType MongoDBInsertFailed =
            new DiagnosticEventType("MongoDBInsertFailed", DiagnosticEventLevel.Error);  

        /// <summary>
        /// Emitted when a MongoDB update operation fails
        /// </summary>
        public static readonly DiagnosticEventType MongoDBUpdateFailed =
            new DiagnosticEventType("MongoDBUpdateFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted when a MongoDB delete operation fails
        /// </summary>
        public static readonly DiagnosticEventType MongoDBDeleteFailed =
            new DiagnosticEventType("MongoDBDeleteFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted when a MongoDB find operation fails
        /// </summary>
        public static readonly DiagnosticEventType MongoDBFindFailed =
            new DiagnosticEventType("MongoDBFindFailed", DiagnosticEventLevel.Error);
    }
}
