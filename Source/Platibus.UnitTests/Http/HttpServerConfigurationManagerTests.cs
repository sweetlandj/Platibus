using Platibus.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Filesystem;
using Platibus.Security;
using Xunit;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
    public class HttpServerConfigurationManagerTests
    {
        protected HttpServerConfiguration Configuration;

        [Fact]
        public async Task ConfigurationCanBeInitializedFromExeConfig()
        {
            GivenEmptyConfiguration();
            var configurationManager = new HttpServerConfigurationManager();
            await configurationManager.Initialize(Configuration, "platibus.httpserver");
            AssertExeConfigurationInitialized();
        }
        
        protected void GivenEmptyConfiguration()
        {
            Configuration = new HttpServerConfiguration();
        }
        
        protected void AssertExeConfigurationInitialized()
        {
            Assert.Equal(new Uri("http://localhost:8081/"), Configuration.BaseUri);
            Assert.Equal("application/json", Configuration.DefaultContentType);
            Assert.Equal(TimeSpan.FromMinutes(10), Configuration.ReplyTimeout);
            Assert.IsType<FilesystemMessageQueueingService>(Configuration.MessageQueueingService);
            Assert.IsType<FilesystemSubscriptionTrackingService>(Configuration.SubscriptionTrackingService);

            Assert.Contains(Configuration.Endpoints, 
                e => e.Key == "example" &&
                e.Value.Address == new Uri("http://example.com/platibus/") &&
                new BasicAuthCredentials("user", "pass").Equals(e.Value.Credentials));

            Assert.Contains(new TopicName("foo"), Configuration.Topics);

            Assert.Contains(Configuration.SendRules, e =>
                new MessageNamePatternSpecification(".*example.*").Equals(e.Specification) &&
                e.Endpoints.Contains(new EndpointName("example")));

            Assert.Contains(Configuration.Subscriptions, s =>
                s.Topic == "bar" &&
                s.Endpoint == "example" &&
                s.TTL == TimeSpan.FromHours(1));

            Assert.NotNull(Configuration.DefaultSendOptions);
            Assert.Equal("text/plain", Configuration.DefaultSendOptions.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(5), Configuration.DefaultSendOptions.TTL);
            Assert.True(Configuration.DefaultSendOptions.Synchronous);
            Assert.IsType<DefaultCredentials>(Configuration.DefaultSendOptions.Credentials);
        }
    }
}