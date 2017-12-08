using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSink"/> implementation that wraps another
    /// <see cref="IDiagnosticEventSink"/> and prevents consumption of events that do not satisfy
    /// filter criteria
    /// </summary>
    public class FilteringSink : IDiagnosticEventSink
    {
        private readonly IDiagnosticEventSpecification _specification;

        /// <summary>
        /// The wrapped sink
        /// </summary>
        public IDiagnosticEventSink Inner { get; }

        /// <summary>
        /// Initializes a new <see cref="FilteringSink"/> that wraps the specified 
        /// <paramref name="inner"/> sink and only forwards events that match the supplied
        /// <paramref name="specification"/>
        /// </summary>
        /// <param name="inner">The event sink to wrap</param>
        /// <param name="specification">The specification that identifies the events that should
        /// be forwarded to the wrapped sink</param>
        public FilteringSink(IDiagnosticEventSink inner, IDiagnosticEventSpecification specification)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        /// <inheritdoc />
        public void Consume(DiagnosticEvent @event)
        {
            if (_specification.IsSatisfiedBy(@event))
            {
                Inner.Consume(@event);
            }
        }

        /// <inheritdoc />
        public Task ConsumeAsync(DiagnosticEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            return _specification.IsSatisfiedBy(@event) 
                ? Inner.ConsumeAsync(@event, cancellationToken) 
                : Task.FromResult(0);
        }
    }
}
