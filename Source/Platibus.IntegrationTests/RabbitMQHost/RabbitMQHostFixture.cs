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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.RabbitMQ;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    public class RabbitMQHostFixture : IDisposable
    {
        private readonly RabbitMQ.RabbitMQHost _sendingHost;
        private readonly RabbitMQ.RabbitMQHost _receivingHost;

        private bool _disposed;

        public IBus Sender => _sendingHost.Bus;

        public IBus Receiver => _receivingHost.Bus;

        public RabbitMQHostFixture()
        {
            // docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
            WaitForRabbitMQ(new Uri("amqp://guest:guest@localhost:5682"));
            CreateVHosts();
            
            _sendingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq0");
            _receivingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq1", configuration =>
            {
                configuration.AddHandlingRule<TestMessage>(".*TestMessage", TestHandler.HandleMessage, "TestHandler");
                configuration.AddHandlingRule(".*TestPublication", new TestPublicationHandler(), "TestPublicationHandler");
            });
        }
        
        private static void WaitForRabbitMQ(Uri uri)
        {
            using (var connectionManager = new ConnectionManager())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var connection = connectionManager.GetConnection(uri);
                        if (connection.IsOpen) return;
                        
                        Task.Delay(TimeSpan.FromSeconds(1)).Wait(cts.Token);
                    }
                    catch (Exception)
                    {
                    }

                }
            }

            throw new TimeoutException("RabbitMQ not available");
        }

        private static void CreateVHosts()
        {

            var baseAddress = new Uri("http://localhost:15672/api/");
            var basicAuthCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes("guest:guest"));
            var adminPerms = "{\"configure\":\".*\",\"write\":\".*\",\"read\":\".*\"}";
            using (var httpClient = new HttpClient{BaseAddress = baseAddress})
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthCreds);

                // net452
                var response1 = httpClient.PutAsync("vhosts/platibus0", new StringContent("")).Result;
                var response2 = httpClient.PutAsync("permissions/platibus0/guest", new StringContent(adminPerms)).Result;
                var response3 = httpClient.PutAsync("vhosts/platibus1", new StringContent("")).Result;
                var response4 = httpClient.PutAsync("permissions/platibus1/guest", new StringContent(adminPerms)).Result;

                // netcoreapp2.0
                var response5 = httpClient.PutAsync("vhosts/platibus2", new StringContent("")).Result;
                var response6 = httpClient.PutAsync("permissions/platibus2/guest", new StringContent(adminPerms)).Result;
                var response7 = httpClient.PutAsync("vhosts/platibus3", new StringContent("")).Result;
                var response8 = httpClient.PutAsync("permissions/platibus3/guest", new StringContent(adminPerms)).Result;
            }
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
            _sendingHost?.Dispose();
            _receivingHost?.Dispose();
        }
    }
}
