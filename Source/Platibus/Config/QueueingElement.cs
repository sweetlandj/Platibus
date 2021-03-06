﻿#if NET452 || NET461
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

using System.Configuration;

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Configuration element for message queueing
    /// </summary>
    public class QueueingElement : ExtensibleConfigurationElement
    {
        private const string ProviderPropertyName = "provider";
        private const string SecurityTokensPropertyName = "securityTokens";
        private const string EncryptionPropertyName = "encryption";

        /// <summary>
        /// The name of the message journaling service provider
        /// </summary>
        /// <seealso cref="Platibus.Config.Extensibility.IMessageQueueingServiceProvider"/>
        /// <seealso cref="Platibus.Config.Extensibility.ProviderAttribute"/>
        [ConfigurationProperty(ProviderPropertyName, DefaultValue = "Filesystem")]
        public string Provider
        {
            get => (string) base[ProviderPropertyName];
            set => base[ProviderPropertyName] = value;
        }

        /// <summary>
        /// Security token configuration
        /// </summary>
        [ConfigurationProperty(SecurityTokensPropertyName)]
        public SecurityTokensElement SecurityTokens
        {
            get => (SecurityTokensElement)base[SecurityTokensPropertyName];
            set => base[SecurityTokensPropertyName] = value;
        }

        /// <summary>
        /// Encryption configuration
        /// </summary>
        [ConfigurationProperty(EncryptionPropertyName)]
        public EncryptionElement Encryption
        {
            get => (EncryptionElement)base[EncryptionPropertyName];
            set => base[EncryptionPropertyName] = value;
        }
    }
}
#endif