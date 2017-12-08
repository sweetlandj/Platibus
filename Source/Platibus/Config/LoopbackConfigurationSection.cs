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

namespace Platibus.Config
{
    /// <inheritdoc />
    /// <summary>
    /// Configuration section for hosting a loopback Platibus instance
    /// </summary>
    public class LoopbackConfigurationSection : ConfigurationSection
    {
        private const string JournalingPropertyName = "journaling";
        private const string ReplyTimeoutPropertyName = "replyTimeout";
        private const string TopicsPropertyName = "topics";
        private const string QueueingPropertyName = "queueing";
        private const string DefaultContentTypePropertyName = "defaultContentType";
        private const string DiagnosticsPropertName = "diagnostics";
        private const string DefaultSendOptionsPropertyName = "defaultSendOptions";

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Config.PlatibusConfigurationSection" /> with defaults
        /// </summary>
        public LoopbackConfigurationSection()
        {
            Topics = new TopicElementCollection();
        }

        /// <summary>
        /// Configuration related to diagnostics
        /// </summary>
        [ConfigurationProperty(DiagnosticsPropertName, IsRequired = false, DefaultValue = null)]
        public DiagnosticsElement Diagnostics
        {
            get => (DiagnosticsElement)base[DiagnosticsPropertName];
            set => base[DiagnosticsPropertName] = value;
        }

        /// <summary>
        /// Configuration related to message journaling
        /// </summary>
        [ConfigurationProperty(JournalingPropertyName, IsRequired = false, DefaultValue = null)]
        public JournalingElement Journaling
        {
            get => (JournalingElement)base[JournalingPropertyName];
            set => base[JournalingPropertyName] = value;
        }

        /// <summary>
        /// The maximum amount of time to wait for a reply to a sent message
        /// before clearing references and freeing held resources
        /// </summary>
        [ConfigurationProperty(ReplyTimeoutPropertyName)]
        public TimeSpan ReplyTimeout
        {
            get => (TimeSpan)base[ReplyTimeoutPropertyName];
            set => base[ReplyTimeoutPropertyName] = value;
        }
        
        /// <summary>
        /// Configured topics to which messages can be published
        /// </summary>
        [ConfigurationProperty(TopicsPropertyName)]
        [ConfigurationCollection(typeof(TopicElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public TopicElementCollection Topics
        {
            get => (TopicElementCollection)base[TopicsPropertyName];
            set => base[TopicsPropertyName] = value;
        }

        /// <summary>
        /// Configuration related to message queueing
        /// </summary>
        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get => (QueueingElement)base[QueueingPropertyName];
            set => base[QueueingPropertyName] = value;
        }

        /// <summary>
        /// The default content type to use for messages sent from this instance
        /// </summary>
        [ConfigurationProperty(DefaultContentTypePropertyName, IsRequired = false, DefaultValue = null)]
        public string DefaultContentType
        {
            get => (string)base[DefaultContentTypePropertyName];
            set => base[DefaultContentTypePropertyName] = value;
        }

        /// <summary>
        /// Default send options
        /// </summary>
        [ConfigurationProperty(DefaultSendOptionsPropertyName)]
        public SendOptionsElement DefaultSendOptions
        {
            get => (SendOptionsElement)base[DefaultSendOptionsPropertyName];
            set => base[DefaultSendOptionsPropertyName] = value;
        }
    }
}

#endif