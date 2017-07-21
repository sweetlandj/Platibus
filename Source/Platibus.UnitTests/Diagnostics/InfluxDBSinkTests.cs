using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "InfluxDB")]
    public class InfluxDBSinkTests
    {
        protected TimeSpan SampleRate = TimeSpan.FromSeconds(1);
        protected InfluxDBOptions Options = new InfluxDBOptions(new Uri("http://localhost:8086"), "platibus");
        protected List<DiagnosticEvent> DiagnosticEvents = new List<DiagnosticEvent>();

        [Fact]
        public async Task AcknowledgementFailuresAreRecorded()
        {
            GivenQueuedMessageFlowWithAcknowledgementFailure();
            await WhenConsumingEvents();
            
        }

        protected void GivenQueuedMessageFlowWithAcknowledgementFailure()
        {
            var fakes = new DiagnosticFakes(this);
            DiagnosticEvents.AddRange(fakes.QueuedMessageFlowWithAcknowledgementFailure());
        }

        protected async Task WhenConsumingEvents()
        {
            var sink = new InfluxDBSink(Options, SampleRate);
            var consumeTasks = DiagnosticEvents.Select(async e => await sink.ConsumeAsync(e));
            await Task.WhenAll(consumeTasks);
            sink.RecordMeasurements();
        }
    }
}
