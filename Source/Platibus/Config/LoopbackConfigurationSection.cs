using System;
using System.Configuration;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration section for hosting a loopback Platibus instance
    /// </summary>
    public class LoopbackConfigurationSection : ConfigurationSection
    {
        private const string JournalingPropertyName = "journaling";
        private const string ReplyTimeoutPropertyName = "replyTimeout";
        private const string TopicsPropertyName = "topics";
        private const string QueueingPropertyName = "queueing";

        /// <summary>
        /// Initializes a new <see cref="PlatibusConfigurationSection"/> with defaults
        /// </summary>
        public LoopbackConfigurationSection()
        {
            Topics = new TopicElementCollection();
        }

        /// <summary>
        /// Configuration related to message journaling
        /// </summary>
        [ConfigurationProperty(JournalingPropertyName, IsRequired = false, DefaultValue = null)]
        public JournalingElement Journaling
        {
            get { return (JournalingElement)base[JournalingPropertyName]; }
            set { base[JournalingPropertyName] = value; }
        }

        /// <summary>
        /// The maximum amount of time to wait for a reply to a sent message
        /// before clearing references and freeing held resources
        /// </summary>
        [ConfigurationProperty(ReplyTimeoutPropertyName)]
        public TimeSpan ReplyTimeout
        {
            get { return (TimeSpan)base[ReplyTimeoutPropertyName]; }
            set { base[ReplyTimeoutPropertyName] = value; }
        }
        
        /// <summary>
        /// Configured topics to which messages can be published
        /// </summary>
        [ConfigurationProperty(TopicsPropertyName)]
        [ConfigurationCollection(typeof(TopicElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public TopicElementCollection Topics
        {
            get { return (TopicElementCollection)base[TopicsPropertyName]; }
            set { base[TopicsPropertyName] = value; }
        }

        /// <summary>
        /// Configuration related to message queueing
        /// </summary>
        [ConfigurationProperty(QueueingPropertyName)]
        public QueueingElement Queueing
        {
            get { return (QueueingElement)base[QueueingPropertyName]; }
            set { base[QueueingPropertyName] = value; }
        }
    }
}
