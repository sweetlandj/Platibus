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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Platibus.UnitTests
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> implementation that writes log messages to an
    /// XUnit <see cref="ITestOutputHelper"/>
    /// </summary>
    public class TestOutputHelperSink : IDiagnosticEventSink
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Initializes a new <see cref="TestOutputHelperSink"/> that targets the supplied
        /// text <paramref name="outputHelper"/>
        /// </summary>
        /// <param name="outputHelper">The text writer to target</param>
        public TestOutputHelperSink(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper ?? throw new ArgumentNullException("outputHelper");
        }

        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            _outputHelper.WriteLine(FormatLogMessage(@event));
            // Allow exceptions to propagate to the IDiagnosticService where they will be caught
            // and handled by registered DiagnosticSinkExceptionHandlers 
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            _outputHelper.WriteLine(FormatLogMessage(@event));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Formats the log message
        /// </summary>
        /// <param name="event">The diagnostic event</param>
        /// <returns>Returns a formatted log message</returns>
        protected virtual string FormatLogMessage(DiagnosticEvent @event)
        {
            var source = @event.Source;
            var timestamp = @event.Timestamp.ToString("s");
            var level = @event.Type.Level.ToString("G");
            var category = source == null ? "Platibus" : source.GetType().FullName;
            var message = timestamp + " " + level + " " + category + " " + @event.Type.Name;
            if (@event.Detail != null)
            {
                message += ": " + @event.Detail;
            }

            var fields = new OrderedDictionary();
            if (@event.Message != null)
            {
                var headers = @event.Message.Headers;
                fields["MessageId"] = headers.MessageId;
                fields["Origination"] = headers.Origination;
                fields["Destination"] = headers.Destination;
            }
            fields["Queue"] = @event.Queue;
            fields["Topic"] = @event.Topic;

            var fieldsStr = string.Join("; ", fields
                .OfType<DictionaryEntry>()
                .Where(f => f.Value != null)
                .Select(f => f.Key + "=" + f.Value));

            if (!string.IsNullOrWhiteSpace(fieldsStr))
            {
                message += " (" + fieldsStr + ")";
            }

            if (@event.Exception != null)
            {
                message += ", Exception: " + @event.Exception;
            }

            return message;
        }
    }
}