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
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    public class RabbitMQHostFixture : IDisposable
    {
        private readonly Task<RabbitMQ.RabbitMQHost> _sendingHost;
        private readonly Task<RabbitMQ.RabbitMQHost> _receivingHost;

        private bool _disposed;

        public Task<IBus> Sender
        {
            get { return _sendingHost.ContinueWith(hostTask => (IBus) hostTask.Result.Bus); }
        }

        public Task<IBus> Receiver
        {
            get { return _receivingHost.ContinueWith(hostTask => (IBus) hostTask.Result.Bus); }
        }

        public RabbitMQHostFixture()
        {
            // docker run --rm--name rabbitmq -p 5672:5672 - p 15672:15672 rabbitmq: 3 - management

            CreateVHosts();
            
            _sendingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq0");
            _receivingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq1");
        }

        private static void CreateVHosts()
        {
            var baseAddress = new Uri("http://localhost:15672/api/");
            var basicAuthCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes("guest:guest"));
            var adminPerms = "{\"configure\":\".*\",\"write\":\".*\",\"read\":\".*\"}";
            using (var httpClient = new HttpClient{BaseAddress = baseAddress})
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthCreds);
                var response1 = httpClient.PutAsync("vhosts/platibus0", new StringContent("")).Result;
                var response2 = httpClient.PutAsync("permissions/platibus0/guest", new StringContent(adminPerms)).Result;
                var response3 = httpClient.PutAsync("vhosts/platibus1", new StringContent("")).Result;
                var response4 = httpClient.PutAsync("permissions/platibus1/guest", new StringContent(adminPerms)).Result;
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
            Task.WhenAll(
                    _sendingHost.ContinueWith(t => t.Result.Dispose()),
                    _receivingHost.ContinueWith(t => t.Result.Dispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }
    }
}
