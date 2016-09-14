// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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

namespace Platibus
{
    /// <summary>
    /// Discrete categories to which Platibus log messages are written
    /// </summary>
    public static class LoggingCategories
    {
        /// <summary>
        /// Category for log events related to core functionality
        /// </summary>
        public const string Core = "Platibus";

        /// <summary>
        /// Category for log events related to configuration
        /// </summary>
        public const string Config = "Platibus.Config";

        /// <summary>
        /// Category for log events related to filesystem-based services
        /// </summary>
        public const string Filesystem = "Platibus.Filesystem";

        /// <summary>
        /// Category for log events related to serialization
        /// </summary>
        public const string Serialization = "Platibus.Serialization";

        /// <summary>
        /// Category for log events related to HTTP hosting or transport
        /// </summary>
        public const string Http = "Platibus.Http";

        /// <summary>
        /// Category for log events related to IIS hosting
        /// </summary>
        public const string IIS = "Platibus.IIS";

        /// <summary>
        /// Category for log events related to OWIN hosting
        /// </summary>
        public const string Owin = "Platibus.Owin";

        /// <summary>
        /// Category for log events related to SQL-based services
        /// </summary>
        public const string SQL = "Platibus.SQL";
    }
}