using System;

namespace Platibus.Config
{
    /// <summary>
    /// A loopback configuration
    /// </summary>
    public class LoopbackConfiguration : PlatibusConfiguration, ILoopbackConfiguration
    {
        private readonly EndpointName _loopbackEndpoint;
        private readonly Uri _baseUri;

        /// <summary>
        /// The base URI of the loopback bus instance
        /// </summary>
        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        /// <summary>
        /// The message queueing service to use
        /// </summary>
        public IMessageQueueingService MessageQueueingService { get; set; }

        /// <summary>
        /// Initializes a new <see cref="LoopbackConfiguration"/>
        /// </summary>
        public LoopbackConfiguration()
        {
            _loopbackEndpoint = new EndpointName("looback");
            _baseUri = new Uri("urn:localhost/loopback");
            base.AddEndpoint(_loopbackEndpoint, new Endpoint(_baseUri));
            var allMessages = new MessageNamePatternSpecification(".*");
            base.AddSendRule(new SendRule(allMessages, _loopbackEndpoint));
        }

        /// <summary>
        /// Adds a topic to the configuration
        /// </summary>
        /// <param name="topic">The name of the topic</param>
        /// <remarks>
        /// Topics must be explicitly added in order to publish messages to them
        /// </remarks>
        public override void AddTopic(TopicName topic)
        {
            base.AddTopic(topic);
            base.AddSubscription(new Subscription(_loopbackEndpoint, topic));
        }

        /// <summary>
        /// Adds a named endpoint to the configuration
        /// </summary>
        /// <param name="name">The name of the endpoint</param>
        /// <param name="endpoint">The endpoint</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddEndpoint(EndpointName name, IEndpoint endpoint)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds a rule governing to which endpoints messages will be sent
        /// </summary>
        /// <param name="sendRule">The send rule</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddSendRule(ISendRule sendRule)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds a subscription to a local or remote topic
        /// </summary>
        /// <param name="subscription">The subscription</param>
        /// <remarks>
        /// Not supported in loopback configurations
        /// </remarks>
        /// <exception cref="NotSupportedException">Always thrown</exception>
        public override void AddSubscription(ISubscription subscription)
        {
            throw new NotSupportedException();
        }
    }
}
