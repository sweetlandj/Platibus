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
using Platibus.Config.Extensibility;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for message journaling
    /// </summary>
    public class JournalingElement : ExtensibleConfigurationElement
    {
        private const string EnabledPropertyName = "enabled";
        private const string ProviderPropertyName = "provider";
        private const string SentPropertyName = "sent";
        private const string ReceivedPropertyName = "received";
        private const string PublishedPropertyName = "published";

        /// <summary>
        /// Indicates whether message journaling is enabled
        /// </summary>
        [ConfigurationProperty(EnabledPropertyName, DefaultValue = true)]
        public bool IsEnabled
        {
            get { return (bool) base[EnabledPropertyName]; }
            set { base[EnabledPropertyName] = value; }
        }

        /// <summary>
        /// The name of the message journaling service provider
        /// </summary>
        /// <seealso cref="IMessageJournalProvider"/>
        /// <seealso cref="Platibus.Config.Extensibility.ProviderAttribute"/>
        [ConfigurationProperty(ProviderPropertyName)]
        public string Provider
        {
            get { return (string) base[ProviderPropertyName]; }
            set { base[ProviderPropertyName] = value; }
        }

        /// <summary>
        /// Whether to journal sent messages
        /// </summary>
        [ConfigurationProperty(SentPropertyName, DefaultValue = true)]
        public bool JournalSentMessages
        {
            get { return (bool)base[SentPropertyName]; }
            set { base[SentPropertyName] = value; }
        }

        /// <summary>
        /// Whether to journal received messages
        /// </summary>
        [ConfigurationProperty(ReceivedPropertyName, DefaultValue = true)]
        public bool JournalReceivedMessages
        {
            get { return (bool)base[ReceivedPropertyName]; }
            set { base[ReceivedPropertyName] = value; }
        }

        /// <summary>
        /// Whether to journal published messages
        /// </summary>
        [ConfigurationProperty(PublishedPropertyName, DefaultValue = true)]
        public bool JournalPublishedMessages
        {
            get { return (bool)base[PublishedPropertyName]; }
            set { base[PublishedPropertyName] = value; }
        }
    }
}