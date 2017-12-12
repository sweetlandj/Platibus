using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Xunit;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.UnitTests.Config.Extensibility
{
    public class DiagnosticEventSinkFactoryTests
    {
#if NET452
        protected DiagnosticEventSinkElement Configuration = new DiagnosticEventSinkElement();
#endif
#if NETCOREAPP2_0
        protected IConfiguration Configuration;
#endif
        protected IDiagnosticEventSink Sink;

#if NETCOREAPP2_0
        public DiagnosticEventSinkFactoryTests()
        {
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
        }
#endif

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
#if NET452
            Configuration.SetAttribute("host", "localhost");
            Configuration.SetAttribute("port", "12201");
#endif
#if NETCOREAPP2_0
            Configuration["host"] = "localhost";
            Configuration["port"] = "12201";
#endif
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
#if NET452
            Configuration.SetAttribute("host", "localhost");
            Configuration.SetAttribute("port", "12201");
#endif
#if NETCOREAPP2_0
            Configuration["host"] = "localhost";
            Configuration["port"] = "12201";
#endif
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
#if NET452
            Configuration.SetAttribute("uri", "http://localhost:12201");
#endif
#if NETCOREAPP2_0
            Configuration["uri"] = "http://localhost:12201";
#endif
            await WhenInitializingSink();
            AssertSink<GelfHttpLoggingSink>();
        }

        [Theory]
        [InlineData("InfluxDB")]
        [InlineData("influxdb")]
        public async Task InfluxDBLoggingSinkInitializedForInfluxDBProvider(string providerName)
        {
            GivenProvider(providerName);
#if NET452
            Configuration.SetAttribute("uri", "http://localhost:8086");
            Configuration.SetAttribute("database", "test");
#endif
#if NETCOREAPP2_0
            Configuration["uri"] = "http://localhost:8086";
            Configuration["database"] = "test";
#endif
            await WhenInitializingSink();
            AssertSink<InfluxDBSink>();
        }

        protected void GivenProvider(string providerName)
        {
#if NET452
            Configuration.Provider = providerName;
#endif
#if NETCOREAPP2_0
            Configuration["provider"] = providerName;
#endif
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
