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

namespace Platibus.IntegrationTests
{
    internal class MessageHandledExpectation : Expectation
    {
        private readonly Func<object, IMessageContext, bool> _isSatisfied;

        public MessageHandledExpectation(Func<object, IMessageContext, bool> isSatisfied)
        {
            _isSatisfied = isSatisfied;
        }

        public void MessageHandled(object content, IMessageContext context)
        {
            if (_isSatisfied(content, context))
            {
                TaskCompletionSource.TrySetResult(true);
            }
        }
    }

    internal class MessageHandledExpectation<TContent> : MessageHandledExpectation
    {
        public MessageHandledExpectation(Func<TContent, IMessageContext, bool> isSatisfied)
            : base((obj, ctx) => obj is TContent && isSatisfied((TContent)obj, ctx))
        {
        }
    }
}