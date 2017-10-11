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
using System.Configuration;

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Configuration element for default send options
    /// </summary>
    public class SendOptionsElement : ConfigurationElement
    {
        private const string ContentTypePropertyName = "contentType";
        private const string TtlPropertyName = "ttl";
        private const string SynchronousPropertyName = "synchronous";
        private const string CredentialTypePropertyName = "credentialType";
        private const string UsernamePropertyName = "username";
        private const string PasswordPropertyName = "password";

        /// <summary>
        /// The default content type
        /// </summary>
        [ConfigurationProperty(ContentTypePropertyName)]
        public string ContentType
        {
            get { return (string)base[ContentTypePropertyName]; }
            set { base[ContentTypePropertyName] = value; }
        }

        /// <summary>
        /// The default Time To Live (TTL) for messages
        /// </summary>
        [ConfigurationProperty(TtlPropertyName)]
        public TimeSpan TTL
        {
            get { return (TimeSpan)base[TtlPropertyName]; }
            set { base[TtlPropertyName] = value; }
        }

        /// <summary>
        /// Whether to override the default queueing and asynchronous processing
        /// behavior and attempt to process messages synchronously.
        /// </summary>
        [ConfigurationProperty(SynchronousPropertyName)]
        public bool Synchronous
        {
            get { return (bool)base[SynchronousPropertyName]; }
            set { base[SynchronousPropertyName] = value; }
        }

        /// <summary>
        /// (Optional) The type of client credentials required to connect to the endpoint.
        /// </summary>
        [ConfigurationProperty(CredentialTypePropertyName, IsRequired = false, DefaultValue = ClientCredentialType.None)]
        public ClientCredentialType CredentialType
        {
            get { return (ClientCredentialType)base[CredentialTypePropertyName]; }
            set { base[CredentialTypePropertyName] = value; }
        }

        /// <summary>
        /// (Optional) The username used to authenticate with the endpoint.  Required
        /// if <see cref="CredentialType"/> is <see cref="ClientCredentialType.Basic"/>.
        /// </summary>
        [ConfigurationProperty(UsernamePropertyName, IsRequired = false)]
        public string Username
        {
            get { return (string)base[UsernamePropertyName]; }
            set { base[UsernamePropertyName] = value; }
        }

        /// <summary>
        /// (Optional) The password used to authenticate with the endpoint.  Required
        /// if <see cref="CredentialType"/> is <see cref="ClientCredentialType.Basic"/>.
        /// </summary>
        [ConfigurationProperty(PasswordPropertyName, IsRequired = false)]
        public string Password
        {
            get { return (string)base[PasswordPropertyName]; }
            set { base[PasswordPropertyName] = value; }
        }
    }
}
