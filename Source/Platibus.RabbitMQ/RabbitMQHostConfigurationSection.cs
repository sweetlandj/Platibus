#if NET452
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
using System.Configuration;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Extends the <see cref="PlatibusConfigurationSection"/> with configuration specific
    /// to RabbitMQ
    /// </summary>
    public class RabbitMQHostConfigurationSection : PlatibusConfigurationSection
    {
        /// <summary>
        /// The default base URI
        /// </summary>
        public const string DefaultBaseUri = "amqp://localhost:5672";

        private const string BaseUriPropertyName = "baseUri";
        private const string EncodingPropertyName = "encoding";
        private const string AutoAcknowledgePropertyName = "autoAcknowledge";
        private const string ConcurrencyLimitPropertyName = "concurrencyLimit";
        private const string MaxAttemptsPropertyName = "maxAttempts";
        private const string RetryDelayPropertyName = "retryDelay";
        private const string DurablePropertyName = "durable";

        private const string SecurityTokensPropertyName = "securityTokens";

        /// <summary>
        /// The base URI for the RabbitMQ hosted bus instance
        /// </summary>
        /// <remarks>
        /// This is the server URI.  The use of virtual hosts is recommended.
        /// </remarks>
        [ConfigurationProperty(BaseUriPropertyName, DefaultValue = DefaultBaseUri)]
        public Uri BaseUri
        {
            get => (Uri)base[BaseUriPropertyName];
            set => base[BaseUriPropertyName] = value;
        }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        [ConfigurationProperty(EncodingPropertyName, DefaultValue = "UTF-8")]
        public string Encoding
        {
            get => (string)base[EncodingPropertyName];
            set => base[EncodingPropertyName] = value;
        }

        /// <summary>
        /// Whether queues should be configured to automatically acknowledge
        /// messages when read by a consumer
        /// </summary>
        [ConfigurationProperty(AutoAcknowledgePropertyName, DefaultValue = false)]
        public bool AutoAcknowledge
        {
            get => (bool)base[AutoAcknowledgePropertyName];
            set => base[AutoAcknowledgePropertyName] = value;
        }

        /// <summary>
        /// The maxiumum number of concurrent consumers that will be started
        /// for each queue
        /// </summary>
        [ConfigurationProperty(ConcurrencyLimitPropertyName, DefaultValue = 1)]
        public int ConcurrencyLimit
        {
            get => (int)base[ConcurrencyLimitPropertyName];
            set => base[ConcurrencyLimitPropertyName] = value;
        }

        /// <summary>
        /// The maximum number of attempts to process a message before moving it to
        /// a dead letter queue
        /// </summary>
        [ConfigurationProperty(MaxAttemptsPropertyName, DefaultValue = 10)]
        public int MaxAttempts
        {
            get => (int)base[MaxAttemptsPropertyName];
            set => base[MaxAttemptsPropertyName] = value;
        }

        /// <summary>
        /// The amount of time to wait between redelivery attempts
        /// </summary>
        [ConfigurationProperty(RetryDelayPropertyName, DefaultValue = "00:00:05")]
        public TimeSpan RetryDelay
        {
            get => (TimeSpan)base[RetryDelayPropertyName];
            set => base[RetryDelayPropertyName] = value;
        }

        /// <summary>
        /// Indicates whether queues should be durable by default
        /// </summary>
        /// <remarks>
        /// The default value of this property is <c>true</c>.
        /// </remarks>
        [ConfigurationProperty(DurablePropertyName, DefaultValue = true)]
        public bool IsDurable
        {
            get => (bool)base[DurablePropertyName];
            set => base[DurablePropertyName] = value;
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
    }
}

#endif