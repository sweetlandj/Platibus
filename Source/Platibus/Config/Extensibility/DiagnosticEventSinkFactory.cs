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
#if NET452
using System.Configuration;
#elif NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
using System.Threading.Tasks;
using Platibus.Diagnostics;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Factory class for initializing diagnostic event sinks
    /// </summary>
    public class DiagnosticEventSinkFactory
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly IProviderService _providerService;

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventSinkFactory"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public DiagnosticEventSinkFactory(IDiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            _providerService = new ReflectionBasedProviderService(_diagnosticService);
        }

#if NET452
        /// <summary>
        /// Initializes a subscription tracking service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration</param>
        /// <returns>Returns a task whose result is the initialized subscription tracking service</returns>
        public async Task<IDiagnosticEventSink> InitDiagnosticEventSink(DiagnosticEventSinkElement configuration)
        {
            var myConfig = configuration ?? new DiagnosticEventSinkElement();
            if (string.IsNullOrWhiteSpace(myConfig.Provider))
            {
                var message = "Provider not specified for diagnostic event sink '" + myConfig.Name + "'";
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = message
                    }.Build());
                throw new ConfigurationErrorsException(message);
            }

            var provider = GetProvider(myConfig.Provider);
            if (provider == null)
            {
                var message = "Unknown provider '" + myConfig.Provider + "' specified for diagnostic event sink '" + myConfig.Name + "'";
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = message
                    }.Build());
                throw new ConfigurationErrorsException(message);
            }

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Initializing diagnostic event sink '" + myConfig.Name + "'"
                }.Build());

            var sink = await provider.CreateDiagnosticEventSink(myConfig);
            var filterSpec = new DiagnosticEventLevelSpecification(myConfig.MinLevel, myConfig.MaxLevel);
            var filterSink = new FilteringSink(sink, filterSpec);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Diagnostic event sink '" + myConfig.Name + "' initialized"
                }.Build());

            return filterSink;
        }
#elif NETSTANDARD2_0
        /// <summary>
        /// Initializes a subscription tracking service based on the supplied
        /// <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The subscription tracking configuration</param>
        /// <returns>Returns a task whose result is the initialized subscription tracking service</returns>
        public async Task<IDiagnosticEventSink> InitDiagnosticEventSink(IConfiguration configuration)
        {
            var name = configuration?["name"];
            var providerName = configuration?["provider"];
            if (string.IsNullOrWhiteSpace(providerName))
            {
                var message = "Provider not specified for diagnostic event sink '" + name + "'";
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = message
                    }.Build());
                throw new ConfigurationErrorsException(message);
            }

            var provider = GetProvider(providerName);
            if (provider == null)
            {
                var message = "Unknown provider '" + providerName + "' specified for diagnostic event sink '" + name + "'";
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationError)
                    {
                        Detail = message
                    }.Build());
                throw new ConfigurationErrorsException(message);
            }

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Initializing diagnostic event sink '" + name + "'"
                }.Build());

            var minLevel = configuration?.GetValue<DiagnosticEventLevel>("minLevel") ?? DiagnosticEventLevel.Debug;
            var maxLevel = configuration?.GetValue<DiagnosticEventLevel>("minLevel") ?? DiagnosticEventLevel.Error;
            var sink = await provider.CreateDiagnosticEventSink(configuration);
            var filterSpec = new DiagnosticEventLevelSpecification(minLevel, maxLevel);
            var filterSink = new FilteringSink(sink, filterSpec);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                {
                    Detail = "Diagnostic event sink '" + name + "' initialized"
                }.Build());

            return filterSink;
        }
#endif

        private IDiagnosticEventSinkProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));
            return _providerService.GetProvider<IDiagnosticEventSinkProvider>(providerName);
        }
    }
}