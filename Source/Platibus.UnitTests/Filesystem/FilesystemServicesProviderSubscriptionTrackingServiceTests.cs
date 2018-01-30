using System;
using System.IO;
using System.Threading.Tasks;
using Platibus.Filesystem;
using Xunit;
#if NET452
using Platibus.Config;
#endif
#if NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.UnitTests.Filesystem
{
    [Trait("Category", "UnitTests")]
    [Collection(FilesystemCollection.Name)]
    public class FilesystemServicesProviderSubscriptionTrackingServiceTests
    {
        protected readonly DirectoryInfo Path;

        public TopicName Topic = Guid.NewGuid().ToString();
        public Uri Subscriber = new Uri("http://localhost/" + Guid.NewGuid());
#if NET452
        protected SubscriptionTrackingElement Configuration = new SubscriptionTrackingElement();
#endif
#if NETCOREAPP2_0
        protected IConfiguration Configuration;
#endif

        public FilesystemServicesProviderSubscriptionTrackingServiceTests(FilesystemFixture fixture)
        {
#if NETCOREAPP2_0
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

#endif
            Path = fixture.BaseDirectory;
        }

        [Fact]
        public async Task PathHasDefaultValue()
        {
            var defaultPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platibus", "subscriptions");
            GivenPathOverride(null);
            await WhenAddingASubscription();
            AssertSubscriptionFileUpdated(defaultPath);
        }
        
        [Fact]
        public async Task PathCanBeOverridden()
        {
            var pathOverride = System.IO.Path.Combine(Path.FullName, "override");
            GivenPathOverride(pathOverride);
            await WhenAddingASubscription();
            AssertSubscriptionFileUpdated(pathOverride);
        }
        
        protected void GivenPathOverride(string path)
        {
            ConfigureAttribute("path", path);
        }
        
        protected async Task WhenAddingASubscription()
        {
            var subscriptionTrackingService = await new FilesystemServicesProvider()
                .CreateSubscriptionTrackingService(Configuration);

            await subscriptionTrackingService.AddSubscription(Topic, Subscriber);
        }

        protected void AssertSubscriptionFileUpdated(string path)
        {
            var subscriptionFilename = Topic + ".psub";
            var subscriptionFilePath = System.IO.Path.Combine(path, subscriptionFilename);
            var lines = File.ReadAllLines(subscriptionFilePath);
            Assert.Single(lines);
            Assert.Contains(Subscriber.ToString(), lines[0]);
        }

        protected void ConfigureAttribute(string name, string value)
        {
#if NET452
            Configuration.SetAttribute(name, value);
#endif
#if NETCOREAPP2_0
            Configuration[name] = value;
#endif
        }
    }
}