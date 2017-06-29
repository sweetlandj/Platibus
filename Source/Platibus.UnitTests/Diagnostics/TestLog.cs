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
using System.Collections.Generic;
using Common.Logging.NLog;
using NLog;
using LogLevel = Common.Logging.LogLevel;

namespace Platibus.UnitTests.Diagnostics
{
    public class TestLog : NLogLogger
    {
        private readonly IList<LoggedMessage> _loggedMessages = new List<LoggedMessage>();

        public IEnumerable<LoggedMessage> LoggedMessages { get { return _loggedMessages; } }

        public TestLog(Logger logger) : base(logger)
        {
        }

        protected override void WriteInternal(LogLevel logLevel, object message, Exception exception)
        {
            _loggedMessages.Add(new LoggedMessage(logLevel, message, exception));
            base.WriteInternal(logLevel, message, exception);
        }

        public class LoggedMessage
        {
            private readonly LogLevel _logLevel;
            private readonly object _message;
            private readonly Exception _exception;

            public LogLevel LogLevel1 { get { return _logLevel; } }
            public object Message { get { return _message; } }
            public Exception Exception { get { return _exception; } }

            public LoggedMessage(LogLevel logLevel, object message, Exception exception)
            {
                _logLevel = logLevel;
                _message = message;
                _exception = exception;
            }
        }

    }
}
