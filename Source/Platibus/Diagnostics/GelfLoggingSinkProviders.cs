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

#if NET452
using System.Configuration;    
#endif
using System;
using System.Threading.Tasks;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Security;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A suite of <see cref="IDiagnosticEventSinkProvider"/> implementations that produce data
    /// sinks for consuming GELF formatted messages via UDP, TCP, or HTTP.
    /// </summary>
    public static class GelfLoggingSinkProviders
    {
        /// <summary>
        /// A <see cref="IDiagnosticEventSinkProvider"/> implementation that sends GELF formatted
        /// messages via UDP datagrams
        /// </summary>
        [Provider("GelfUdp")]
        public class Udp : IDiagnosticEventSinkProvider
        {
#if NET452
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(DiagnosticEventSinkElement configuration)
            {
                var host = configuration.GetString("host");
                var port = configuration.GetInt("port");
                var enableCompression = configuration.GetBool("compress");

                if (string.IsNullOrWhiteSpace(host)) throw new ConfigurationErrorsException("'host' attribute is required");
                if (port == 0) throw new ConfigurationErrorsException("'port' attribute is required");
                if (port < 0 || port > 65535) throw new ConfigurationErrorsException("Invalid port");
                return Task.FromResult<IDiagnosticEventSink>(new GelfUdpLoggingSink(host, port, enableCompression));
            }
#else
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(IConfiguration configuration)
            {
                var host = configuration?["host"];
                var port = configuration?.GetValue<int>("port") ?? 0;
                var enableCompression = configuration?.GetValue<bool>("compress") ?? false;

                if (string.IsNullOrWhiteSpace(host)) throw new ConfigurationErrorsException("'host' attribute is required");
                if (port == 0) throw new ConfigurationErrorsException("'port' attribute is required");
                if (port < 0 || port > 65535) throw new ConfigurationErrorsException("Invalid port");
                return Task.FromResult<IDiagnosticEventSink>(new GelfUdpLoggingSink(host, port, enableCompression));
            }
#endif
        }

        /// <summary>
        /// A <see cref="IDiagnosticEventSinkProvider"/> implementation that sends GELF formatted
        /// messages over a persistent TCP connection
        /// </summary>
        [Provider("GelfTcp")]
        public class Tcp : IDiagnosticEventSinkProvider
        {
#if NET452
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(DiagnosticEventSinkElement configuration)
            {
                var host = configuration.GetString("host");
                var port = configuration.GetInt("port");

                if (string.IsNullOrWhiteSpace(host)) throw new ConfigurationErrorsException("'host' attribute is required");
                if (port == 0) throw new ConfigurationErrorsException("'port' attribute is required");
                if (port < 0 || port > 65535) throw new ConfigurationErrorsException("Invalid port");
                return Task.FromResult<IDiagnosticEventSink>(new GelfTcpLoggingSink(host, port));
            }
#else
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(IConfiguration configuration)
            {
                var host = configuration?["host"];
                var port = configuration?.GetValue<int>("port") ?? 0;

                if (string.IsNullOrWhiteSpace(host)) throw new ConfigurationErrorsException("'host' attribute is required");
                if (port == 0) throw new ConfigurationErrorsException("'port' attribute is required");
                if (port < 0 || port > 65535) throw new ConfigurationErrorsException("Invalid port");
                return Task.FromResult<IDiagnosticEventSink>(new GelfTcpLoggingSink(host, port));
            }
#endif
        }

        /// <summary>
        /// A <see cref="IDiagnosticEventSinkProvider"/> implementation that POSTs GELF formatted
        /// messages to an HTTP endpoint
        /// </summary>
        [Provider("GelfHttp")]
        public class Http : IDiagnosticEventSinkProvider
        {
#if NET452
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(DiagnosticEventSinkElement configuration)
            {
                var uri = configuration.GetUri("uri");
                var username = configuration.GetString("username");
                var password = configuration.GetString("password");

                var credentials = string.IsNullOrWhiteSpace(username) 
                    ? null 
                    : new BasicAuthCredentials(username, password);

                if (uri == null) throw new ConfigurationErrorsException("'uri' attribute is required");
                return Task.FromResult<IDiagnosticEventSink>(new GelfHttpLoggingSink(uri, credentials));
            }
#else
            /// <inheritdoc />
            public Task<IDiagnosticEventSink> CreateDiagnosticEventSink(IConfiguration configuration)
            {
                var uri = configuration?.GetValue<Uri>("uri");
                var username = configuration?["username"];
                var password = configuration?["password"];

                var credentials = string.IsNullOrWhiteSpace(username)
                    ? null
                    : new BasicAuthCredentials(username, password);

                if (uri == null) throw new ConfigurationErrorsException("'uri' attribute is required");
                return Task.FromResult<IDiagnosticEventSink>(new GelfHttpLoggingSink(uri, credentials));
            }
#endif
        }
    }
}
