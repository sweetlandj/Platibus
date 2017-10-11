using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    [Trait("Category", "UnitTests")]
    public class DiagnosticEventLevelSpecificationTests
    {
        protected DiagnosticEvent Event;
        protected DiagnosticEventLevel MinLevel;
        protected DiagnosticEventLevel MaxLevel;
        protected bool IsSatisfied;

        [Fact]
        protected void NullEventsDoNotSatisfySpec()
        {
            Event = null;
            GivenMinLevel(DiagnosticEventLevel.Trace);
            GivenMaxLevel(DiagnosticEventLevel.Error);
            WhenEvaluating();
            AssertSpecificationNotSatisified();
        }

        [Fact]
        protected void EventsWithLevelsBelowMinLevelDoNotSatisfySpec()
        {
            GivenEventWithLevel(DiagnosticEventLevel.Debug);
            GivenMinLevel(DiagnosticEventLevel.Warn);
            GivenMaxLevel(DiagnosticEventLevel.Error);
            WhenEvaluating();
            AssertSpecificationNotSatisified();
        }

        [Fact]
        protected void EventsWithLevelsAboveMaxLevelDoNotSatisfySpec()
        {
            GivenEventWithLevel(DiagnosticEventLevel.Error);
            GivenMinLevel(DiagnosticEventLevel.Trace);
            GivenMaxLevel(DiagnosticEventLevel.Warn);
            WhenEvaluating();
            AssertSpecificationNotSatisified();
        }

        [Fact]
        protected void EventsWithMinLevelSatisfySpec()
        {
            GivenEventWithLevel(DiagnosticEventLevel.Info);
            GivenMinLevel(DiagnosticEventLevel.Info);
            GivenMaxLevel(DiagnosticEventLevel.Warn);
            WhenEvaluating();
            AssertSpecificationIsSatisified();
        }

        [Fact]
        protected void EventsWithMaxLevelSatisfySpec()
        {
            GivenEventWithLevel(DiagnosticEventLevel.Warn);
            GivenMinLevel(DiagnosticEventLevel.Info);
            GivenMaxLevel(DiagnosticEventLevel.Warn);
            WhenEvaluating();
            AssertSpecificationIsSatisified();
        }

        [Fact]
        protected void EventsWithLevelsBetweenMinAndMaxLevelsSatisfySpec()
        {
            GivenEventWithLevel(DiagnosticEventLevel.Info);
            GivenMinLevel(DiagnosticEventLevel.Debug);
            GivenMaxLevel(DiagnosticEventLevel.Warn);
            WhenEvaluating();
            AssertSpecificationIsSatisified();
        }

        protected void GivenEventWithLevel(DiagnosticEventLevel level)
        {
            var diagnosticEventType = new DiagnosticEventType("TestDebug", level);
            Event = new DiagnosticEventBuilder(this, diagnosticEventType).Build();
        }
        
        protected void GivenMinLevel(DiagnosticEventLevel level)
        {
            MinLevel = level;
        }

        protected void GivenMaxLevel(DiagnosticEventLevel level)
        {
            MaxLevel = level;
        }

        protected void WhenEvaluating()
        {
            var spec = new DiagnosticEventLevelSpecification(MinLevel, MaxLevel);
            IsSatisfied = spec.IsSatisfiedBy(Event);
        }

        protected void AssertSpecificationIsSatisified()
        {
            Assert.True(IsSatisfied);
        }

        protected void AssertSpecificationNotSatisified()
        {
            Assert.False(IsSatisfied);
        }
    }
}
