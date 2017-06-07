using System;
using Platibus.Http;
using Xunit;

namespace Platibus.UnitTests.Http
{
    public class HttpSubscriptionMetadataTests
    {
        protected IEndpoint Endpoint;
        protected TopicName Topic;
        protected TimeSpan TTL;
        protected HttpSubscriptionMetadata Metadata;

        [Fact]
        public void EndpointIsRequired()
        {
            GivenTopic();
            GivenTTL();
            var nre = Assert.Throws<ArgumentNullException>(() => WhenInitializingMetadata());
            Assert.Equal("endpoint", nre.ParamName);
        }

        [Fact]
        public void TopicIsRequired()
        {
            GivenEndpoint();
            GivenTTL();
            var nre = Assert.Throws<ArgumentNullException>(() => WhenInitializingMetadata());
            Assert.Equal("topic", nre.ParamName);
        }

        [Fact]
        public void TtlMustBeNonNegative()
        {
            GivenEndpoint();
            GivenTopic();
            GivenNegativeTTL();
            Assert.Throws<ArgumentOutOfRangeException>(() => WhenInitializingMetadata());
        }

        [Fact]
        public void SubscriptionDoesNotExpireIfNoTTLGiven()
        {
            GivenEndpoint();
            GivenTopic();
            GivenNoTTL();
            WhenInitializingMetadata();
            Assert.Equal(false, Metadata.Expires);
        }

        [Fact]
        public void SubscriptionExpiresIfTTLGiven()
        {
            GivenEndpoint();
            GivenTopic();
            GivenTTL();
            WhenInitializingMetadata();
            Assert.Equal(true, Metadata.Expires);
        }

        [Fact]
        public void TTLAlwaysBeGreaterThanOrEqualToMinTTL()
        {
            GivenEndpoint();
            GivenTopic();
            GivenNominalTTL();
            WhenInitializingMetadata();

            Assert.Equal(HttpSubscriptionMetadata.MinTTL, Metadata.TTL);
        }

        [Fact]
        public void RenewalIntervalAlwaysLessThanMaxRenewalInterval()
        {
            GivenEndpoint();
            GivenTopic();
            GivenLongTTL();
            WhenInitializingMetadata();

            Assert.Equal(HttpSubscriptionMetadata.MaxRenewalInterval, Metadata.RenewalInterval);
        }

        [Fact]
        public void RetrIntervalAlwaysLessThanMaxRetryInterval()
        {
            GivenEndpoint();
            GivenTopic();
            GivenLongTTL();
            WhenInitializingMetadata();

            Assert.Equal(HttpSubscriptionMetadata.MaxRetryInterval, Metadata.RetryInterval);
        }

        protected void GivenEndpoint()
        {
            Endpoint = new Endpoint(new Uri("http://localhost/platibus"));
        }

        protected void GivenTopic()
        {
            Topic = "TestTopic";
        }

        protected void GivenTTL()
        {
            TTL = TimeSpan.FromHours(1);
        }

        protected void GivenNegativeTTL()
        {
            TTL = TimeSpan.FromSeconds(-1);
        }

        protected void GivenNoTTL()
        {
            TTL = TimeSpan.Zero;
        }

        protected void GivenNominalTTL()
        {
            TTL = TimeSpan.FromSeconds(1);
        }

        protected void GivenLongTTL()
        {
            TTL = TimeSpan.FromDays(365);
        }

        protected void WhenInitializingMetadata()
        {
            Metadata = new HttpSubscriptionMetadata(Endpoint, Topic, TTL);
        }
    }
}
