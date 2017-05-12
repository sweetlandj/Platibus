using System;
using System.IO;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.HttpServer
{
    public class HttpServerFixture : IDisposable
    {      
        private readonly Task<Http.HttpServer> _sendingHttpServer;
        private readonly Task<Http.HttpServer> _receivingHttpServer;

        public Task<IBus> Sender
        {
            get { return _sendingHttpServer.ContinueWith(serverTask => serverTask.Result.Bus); }
        }

        public Task<IBus> Receiver
        {
            get { return _receivingHttpServer.ContinueWith(serverTask => serverTask.Result.Bus); }
        }

        public HttpServerFixture()
        {
            _sendingHttpServer = StartHttpServer("platibus.http0");
            _receivingHttpServer = StartHttpServer("platibus.http1");
        }
        
        private static Task<Http.HttpServer> StartHttpServer(string configSectionName)
        {
            var serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configSectionName);
            var serverDirectory = new DirectoryInfo(serverPath);
            serverDirectory.Refresh();
            if (serverDirectory.Exists)
            {
                serverDirectory.Delete(true);
            }
            return Http.HttpServer.Start(configSectionName);
        }

        public void Dispose()
        {
            Task.WhenAll(
                    _sendingHttpServer.ContinueWith(t => t.Result.TryDispose()),
                    _receivingHttpServer.ContinueWith(t => t.Result.TryDispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
