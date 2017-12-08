using System.Configuration;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Config.Extensibility
{
    public class DiagnosticEventSinkFactoryTests
    {
        protected DiagnosticEventSinkElement Configuration = new DiagnosticEventSinkElement();
        protected IDiagnosticEventSink Sink;

        [Fact]
        public async Task ProviderNameIsRequired()
        {
            GivenProvider(null);
            await Assert.ThrowsAsync<ConfigurationErrorsException>(async () => await WhenInitializingSink());
        }

        [Fact]
        public async Task UnknownProviderThrowsProviderNotFoundException()
        {
            GivenProvider("DoesNotExist");
            await Assert.ThrowsAsync<ProviderNotFoundException>(async () => await WhenInitializingSink());
        }

        [Theory]
        [InlineData("Console")]
        [InlineData("console")]
        public async Task ConsoleLoggingSinkInitializedForConsoleProvider(string providerName)
        {
            GivenProvider(providerName);
            await WhenInitializingSink();
            AssertSink<ConsoleLoggingSink>();
        }
        
        [Theory]
        [InlineData("GelfUdp")]
        [InlineData("gelfudp")]
        [InlineData("GELFUDP")]
        public async Task GelfUdpLoggingSinkInitializedForGelfUdpProvider(string providerName)
        {
            GivenProvider(providerName);
            Configuration.SetAttribute("host", "localhost");
            Configuration.SetAttribute("port", "12201");
            await WhenInitializingSink();
            AssertSink<GelfUdpLoggingSink>();
        }

        [Theory]
        [InlineData("GelfTcp")]
        [InlineData("gelftcp")]
        [InlineData("GELFTCP")]
        public async Task GelfTcpLoggingSinkInitializedForGelfTcpProvider(string providerName)
        {
            GivenProvider(providerName);
            Configuration.SetAttribute("host", "localhost");
            Configuration.SetAttribute("port", "12201");
            await WhenInitializingSink();
            AssertSink<GelfTcpLoggingSink>();
        }

        [Theory]
        [InlineData("GelfHttp")]
        [InlineData("gelfhttp")]
        [InlineData("GELFHTTP")]
        public async Task GelfHttpLoggingSinkInitializedForGelfHttpProvider(string providerName)
        {
            GivenProvider(providerName);
            Configuration.SetAttribute("uri", "http://localhost:12201");
            await WhenInitializingSink();
            AssertSink<GelfHttpLoggingSink>();
        }

        [Theory]
        [InlineData("InfluxDB")]
        [InlineData("influxdb")]
        public async Task InfluxDBLoggingSinkInitializedForInfluxDBProvider(string providerName)
        {
            GivenProvider(providerName);
            Configuration.SetAttribute("uri", "http://localhost:8086");
            Configuration.SetAttribute("database", "test");
            await WhenInitializingSink();
            AssertSink<InfluxDBSink>();
        }

        protected void GivenProvider(string providerName)
        {
            Configuration.Provider = providerName;
        }
        
        protected async Task WhenInitializingSink()
        {
            var factory = new DiagnosticEventSinkFactory(DiagnosticService.DefaultInstance);
            Sink = await factory.InitDiagnosticEventSink(Configuration);
        }

        protected void AssertSink<TSink>() where TSink : IDiagnosticEventSink
        {
            Assert.IsType<FilteringSink>(Sink);
            var filteringSink = (FilteringSink) Sink;
            Assert.IsType<TSink>(filteringSink.Inner);
        }
        
    }
}
