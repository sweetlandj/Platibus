using System;

namespace Platibus.Http
{
    /// <summary>
    /// Metadata for maintaining topic subscriptions over HTTP
    /// </summary>
    public class HttpSubscriptionMetadata
    {
        /// <summary>
        /// The maximum amount of time that can elapse between renewal requests irrespective of
        /// the TTL on the subscription
        /// </summary>
        public static readonly TimeSpan MaxRenewalInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The minimum amount of time that can elapse before retrying a failed subscription request
        /// </summary>
        public static readonly TimeSpan MinRetryInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The maximum amount of time that can elapse before retrying a failed subscription request
        /// </summary>
        public static readonly TimeSpan MaxRetryInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The minimum Time To Live (TTL)
        /// </summary>
        public static readonly TimeSpan MinTTL = TimeSpan.FromSeconds(10);

        private readonly IEndpoint _endpoint;
        private readonly TopicName _topic;
        private readonly TimeSpan _ttl;
        private readonly bool _expires;
        private readonly TimeSpan _renewalInterval;
        private readonly TimeSpan _retryInterval;

        /// <summary>
        /// The endpoint to which subscription requests will be sent
        /// </summary>
        public IEndpoint Endpoint { get { return _endpoint; } }

        /// <summary>
        /// The topic to which the subscriber is subscribing
        /// </summary>
        public TopicName Topic { get { return _topic; } }

        /// <summary>
        /// The (adjusted) time-to-live to send in each subscription request
        /// </summary>
        public TimeSpan TTL
        {
            get { return _ttl; }
        }

        /// <summary>
        /// Whether the subscription is set to expire (nonzero TTL)
        /// </summary>
        public bool Expires
        {
            get { return _expires; }
        }

        /// <summary>
        /// The amount of time to wait between renewal requests
        /// </summary>
        public TimeSpan RenewalInterval
        {
            get { return _renewalInterval; }
        }

        /// <summary>
        /// The amount of time to wait before retrying a failed subscription request
        /// </summary>
        public TimeSpan RetryInterval
        {
            get { return _retryInterval; }
        }

        /// <summary>
        /// Initializes a new set of <see cref="HttpSubscriptionMetadata"/> for a subscription
        /// with the specified <paramref name="ttl"/>
        /// </summary>
        /// <param name="endpoint">The endpoint to which subscription requests will be sent</param>
        /// <param name="topic">The topic to which the subscriber is subscribing</param>
        /// <param name="ttl">The non-negative time-to-live for the subscription</param>
        public HttpSubscriptionMetadata(IEndpoint endpoint, TopicName topic, TimeSpan ttl)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (topic == null) throw new ArgumentNullException("topic");
            if (ttl < TimeSpan.Zero) throw new ArgumentOutOfRangeException("ttl");

            _endpoint = endpoint;
            _topic = topic;
            
            if (ttl > TimeSpan.Zero)
            {
                _ttl = ttl > MinTTL ? ttl : MinTTL;
                _expires = true;    
            }

            // Attempt to renew after half of the TTL or 5 minutes (whichever is less)
            // to allow for issues that may occur when attempting to renew the
            // subscription.
            _renewalInterval = new TimeSpan(ttl.Ticks / 2);
            if (_renewalInterval > MaxRenewalInterval)
            {
                _renewalInterval = MaxRenewalInterval;
            }

            // Wait for the standard renewal interval or 30 seconds (whichever is less) before
            // retrying in the event of an unsuccessful subscription request 
            _retryInterval = _renewalInterval;
            if (_retryInterval > MaxRetryInterval)
            {
                _retryInterval = MaxRetryInterval;
            }

            // Ensure a minimum amount of time elapses between retries to avoid overloading
            // both the subscriber and publisher with requests
            if (_retryInterval < MinRetryInterval)
            {
                _retryInterval = MinRetryInterval;
            }
        }
    }
}
