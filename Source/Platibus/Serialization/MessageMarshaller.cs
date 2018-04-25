using System;

namespace Platibus.Serialization
{
    /// <summary>
    /// Service that marshals object content into a <see cref="Message"/> suitable for
    /// transport that contains metadata necessary to reconstruct the content on the
    /// receiving bus instance.
    /// </summary>
    public class MessageMarshaller
    {
        private readonly IMessageNamingService _messageNamingService;
        private readonly ISerializationService _serializationService;
        private readonly string _defaultContentType;
        
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Serialization.MessageMarshaller" />
        /// </summary>
        public MessageMarshaller()
            : this(null, null, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Serialization.MessageMarshaller" />
        /// </summary>
        /// <param name="messageNamingService">A service used to map <see cref="T:Platibus.MessageName" />s
        /// onto runtime <see cref="T:System.Type" />s when deserializing content</param>
        /// <param name="serializationService">A service used to parse string content onto the
        /// <see cref="T:System.Type" />s identified by the <paramref name="messageNamingService" /></param>
        public MessageMarshaller(IMessageNamingService messageNamingService, ISerializationService serializationService)
            : this(messageNamingService, serializationService, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MessageMarshaller"/>
        /// </summary>
        /// <param name="messageNamingService">A service used to map <see cref="MessageName"/>s
        /// onto runtime <see cref="System.Type"/>s when deserializing content</param>
        /// <param name="serializationService">A service used to parse string content onto the
        /// <see cref="System.Type"/>s identified by the <paramref name="messageNamingService"/></param>
        /// <param name="defaultContentType">The default content type to assume if the
        /// <see cref="IMessageHeaders.ContentType"/> header is not specified</param>
        public MessageMarshaller(IMessageNamingService messageNamingService, ISerializationService serializationService, string defaultContentType)
        {
            _messageNamingService = messageNamingService ?? new DefaultMessageNamingService();
            _serializationService = serializationService ?? new DefaultSerializationService();
            _defaultContentType = string.IsNullOrWhiteSpace(defaultContentType) ? "text/plain" : defaultContentType;
        }

        /// <summary>
        /// Deserializes the <see cref="Message.Content"/> of the supplied <paramref name="message"/>
        /// into an instance of the appropriate runtime <see cref="System.Type"/> based on the
        /// <see cref="IMessageHeaders.MessageName"/> and <see cref="IMessageHeaders.ContentType"/>
        /// specified in the <see cref="Message.Headers"/>
        /// </summary>
        /// <param name="message">The message whose content is to be deserialized</param>
        /// <returns>The deserialized content of the specified <paramref name="message"/></returns>
        public object Unmarshal(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var messageName = message.Headers.MessageName;
            var content = message.Content;
            var contentType = string.IsNullOrWhiteSpace(message.Headers.ContentType)
                ? _defaultContentType
                : message.Headers.ContentType;

            var messageType = _messageNamingService.GetTypeForName(messageName);
            var serializer = _serializationService.GetSerializer(contentType);
            return serializer.Deserialize(content, messageType);
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> by serializing the specified <paramref name="content"/>
        /// and appending the negotiated <see cref="IMessageHeaders.MessageName"/> and
        /// <see cref="IMessageHeaders.ContentType"/> to the resulting <see cref="Message.Headers"/>
        /// </summary>
        /// <param name="content">The message content</param>
        /// <param name="headers">The initial headers for the message</param>
        /// <returns>A message consisting of the serialized <see cref="content"/> and the
        /// supplied <paramref name="headers"/> augmented with the <see cref="IMessageHeaders.MessageName"/>
        /// and <see cref="IMessageHeaders.ContentType"/> used to serialize the content</returns>
        public Message Marshal(object content, IMessageHeaders headers)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            var messageName = _messageNamingService.GetNameForType(content.GetType());
            var contentType = headers.ContentType;
            if (string.IsNullOrWhiteSpace(headers.ContentType))
            {
                contentType = _defaultContentType;
            }

            var newHeaders = new MessageHeaders(headers)
            {
                MessageName = messageName,
                ContentType = contentType
            };

            var serializer = _serializationService.GetSerializer(contentType);
            var serializedContent = serializer.Serialize(content);

            return new Message(newHeaders, serializedContent);
        }
    }
}
