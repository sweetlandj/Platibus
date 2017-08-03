using System.Threading;
using System.Threading.Tasks;
using Moq;
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    public class FilteringSinkTests
    {
        protected Mock<IDiagnosticEventSink> Inner;
        protected DiagnosticEvent Event;
        protected IDiagnosticEventSpecification Specification;

        public FilteringSinkTests()
        {
            Inner = MockEventSink();
        }

        [Fact]
        protected void EventsThatMatchSpecificationAreConsumed()
        {
            GivenEventMatchingSpecification();
            WhenFiltering();
            AssertEventIsConsumed();
        }

        [Fact]
        protected async Task EventsThatMatchSpecificationAreConsumedAsync()
        {
            GivenEventMatchingSpecification();
            await WhenFilteringAsync();
            AssertEventIsConsumedAsync();
        }

        [Fact]
        protected void EventsThatDoNotMatchSpecificationAreNotConsumed()
        {
            GivenEventThatDoesNotMatchSpecification();
            WhenFiltering();
            AssertEventNotConsumed();
        }

        [Fact]
        protected async Task EventsThatDoNotMatchSpecificationAreNotConsumedAsync()
        {
            GivenEventThatDoesNotMatchSpecification();
            await WhenFilteringAsync();
            AssertEventNotConsumed();
        }

        protected void GivenEventMatchingSpecification()
        {
            GivenSomeEvent();
            var mockSpec = new Mock<IDiagnosticEventSpecification>();
            mockSpec.Setup(x => x.IsSatisfiedBy(Event)).Returns(true);
            Specification = mockSpec.Object;
        }

        protected void GivenEventThatDoesNotMatchSpecification()
        {
            GivenSomeEvent();
            var mockSpec = new Mock<IDiagnosticEventSpecification>();
            mockSpec.Setup(x => x.IsSatisfiedBy(Event)).Returns(false);
            Specification = mockSpec.Object;
        }

        protected void GivenSomeEvent()
        {
            var type = new DiagnosticEventType("Test");
            Event = new DiagnosticEventBuilder(this, type).Build();
        }

        protected void WhenFiltering()
        {
            var filter = new FilteringSink(Inner.Object, Specification);
            filter.Consume(Event);
        }

        protected async Task WhenFilteringAsync()
        {
            var filter = new FilteringSink(Inner.Object, Specification);
            await filter.ConsumeAsync(Event);
        }

        protected void AssertEventIsConsumed()
        {
            Inner.Verify(x => x.Consume(Event), Times.Once);
        }

        protected void AssertEventIsConsumedAsync()
        {
            Inner.Verify(x => x.ConsumeAsync(Event, It.IsAny<CancellationToken>()), Times.Once);
        }

        protected void AssertEventNotConsumed()
        {
            Inner.Verify(x => x.Consume(Event), Times.Never);
            Inner.Verify(x => x.ConsumeAsync(Event, It.IsAny<CancellationToken>()), Times.Never);
        }

        protected static Mock<IDiagnosticEventSink> MockEventSink()
        {
            var mock = new Mock<IDiagnosticEventSink>();
            mock.Setup(x => x.ConsumeAsync(It.IsAny<DiagnosticEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }
    }
}
