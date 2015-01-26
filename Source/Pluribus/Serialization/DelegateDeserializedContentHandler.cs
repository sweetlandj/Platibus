// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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

namespace Pluribus.Serialization
{
    public class DelegateDeserializedContentHandler<TContent> : DeserializedContentHandler<TContent>
    {
        private readonly Func<TContent, IMessageContext, CancellationToken, Task> _handleMessageContent;

        public DelegateDeserializedContentHandler(Func<TContent, IMessageContext, Task> handleMessageContent, ISerializationService serializationService = null)
            : base(serializationService)
        {
            if (handleMessageContent == null) throw new ArgumentNullException("handleMessageContent");
            _handleMessageContent = (cont, ctx, tok) => handleMessageContent(cont, ctx);
        }

        public DelegateDeserializedContentHandler(
            Func<TContent, IMessageContext, CancellationToken, Task> handleMessageContent,
            ISerializationService serializationService = null)
            : base(serializationService)
        {
            if (handleMessageContent == null) throw new ArgumentNullException("handleMessageContent");
            _handleMessageContent = handleMessageContent;
        }

        public DelegateDeserializedContentHandler(Action<TContent, IMessageContext> handleMessageContent,
            ISerializationService serializationService = null)
            : base(serializationService)
        {
            if (handleMessageContent == null) throw new ArgumentNullException("handleMessageContent");
            _handleMessageContent = (cont, ctx, tok) => Task.Run(() => handleMessageContent(cont, ctx), tok);
        }

        protected override async Task HandleMessageContent(TContent content, IMessageContext context, CancellationToken cancellationToken)
        {
            await _handleMessageContent(content, context, cancellationToken).ConfigureAwait(false);
        }
    }
}