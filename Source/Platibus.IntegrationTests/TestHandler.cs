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

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests
{
    public class TestHandler
    {
        public static async Task HandleMessage(TestMessage message, IMessageContext messageContext,
            CancellationToken cancellationToken)
        {
            if (message.SimulateAuthorizationFailure)
            {
                throw new UnauthorizedAccessException();
            }

            if (message.PublishHandledPublication)
            {
                await messageContext.Bus.Publish(new TestPublication
                {
                    GuidData = message.GuidData,
                    IntData = message.IntData,
                    StringData = message.StringData,
                    DateData = message.DateData
                }, "Topic0", cancellationToken);
            }

            if (message.PublishUnhandledPublication)
            {
                await messageContext.Bus.Publish(new UnhandledTestPublication
                {
                    GuidData = message.GuidData,
                    IntData = message.IntData,
                    StringData = message.StringData,
                    DateData = message.DateData
                }, "Topic0", cancellationToken);
            }

            await messageContext.SendReply(new TestReply
            {
                GuidData = message.GuidData,
                IntData = message.IntData,
                StringData = message.StringData,
                DateData = message.DateData,
				ContentType = messageContext.Headers.ContentType
            }, cancellationToken: cancellationToken);

            if (message.SimulateAcknowledgementFailure)
            {
                return;
            }

            messageContext.Acknowledge();
        }
    }
}