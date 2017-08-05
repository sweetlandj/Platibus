// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.HttpServer
{
    public class HttpServerFixture : IDisposable
    {      
        private readonly Task<Http.HttpServer> _sendingHttpServer;
        private readonly Task<Http.HttpServer> _receivingHttpServer;

        private bool _disposed;
        
        public Task<IBus> Sender
        {
            get { return _sendingHttpServer.ContinueWith(serverTask => serverTask.Result.Bus); }
        }

        public Task<IBus> Receiver
        {
            get { return _receivingHttpServer.ContinueWith(serverTask => serverTask.Result.Bus); }
        }

        public HttpServerFixture() : this("platibus.http0", "platibus.http1")
        {
        }

        public HttpServerFixture(string senderConfigSectionName, string receiverConfigSectionName)
        {
            _sendingHttpServer = StartHttpServer(senderConfigSectionName);
            _receivingHttpServer = StartHttpServer(receiverConfigSectionName);
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
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Task.WhenAll(
                    _sendingHttpServer.ContinueWith(t => t.Result.Dispose()),
                    _receivingHttpServer.ContinueWith(t => t.Result.Dispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
