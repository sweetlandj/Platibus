// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Collections.Concurrent;
using System.Linq;
using Platibus.Diagnostics;
using Platibus.Utils;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// A basic implementation of <see cref="T:Platibus.IMessageNamingService" /> that used
    /// the full name of the message type as the message name
    /// </summary>
    public class DefaultMessageNamingService : IMessageNamingService
    {
        private readonly IReflectionService _reflectionSevice;
        private readonly ConcurrentDictionary<MessageName, Type> _messageTypesByName = new ConcurrentDictionary<MessageName, Type>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.DefaultMessageNamingService" />
        /// </summary>
        public DefaultMessageNamingService() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DefaultMessageNamingService"/>
        /// </summary>
        /// <param name="diagnosticService">The diagnostic service through which diagnostic
        /// events related to message name resolution should be reported</param>
        public DefaultMessageNamingService(IDiagnosticService diagnosticService)
        {
            var diagnosticService1 = diagnosticService ?? DiagnosticService.DefaultInstance;
            _reflectionSevice = new DefaultReflectionService(diagnosticService1);
        }

        /// <inheritdoc />
        public MessageName GetNameForType(Type messageType)
        {
            return messageType.FullName;
        }

        /// <inheritdoc />
        public Type GetTypeForName(MessageName messageName)
        {
            return messageName == null 
                ? typeof(object) 
                : _messageTypesByName.GetOrAdd(messageName, name => FindTypeByName(name));
        }

        private Type FindTypeByName(string typeName)
        {
            return _reflectionSevice
                .FindTypesByName(typeName)
                .FirstOrDefault(t => !t.IsGenericTypeDefinition);
        }
    }
}