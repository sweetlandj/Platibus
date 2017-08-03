namespace Platibus.Diagnostics
{
    /// <summary>
    /// A <see cref="IDiagnosticEventSpecification"/> implementation that selects
    /// <see cref="DiagnosticEvent"/>s based on the <see cref="DiagnosticEventLevel"/> associated
    /// with their <see cref="DiagnosticEventType"/>
    /// </summary>
    /// <seealso cref="DiagnosticEvent.Type"/>
    /// <seealso cref="DiagnosticEventType.Level"/>
    public class DiagnosticEventLevelSpecification : IDiagnosticEventSpecification
    {
        private readonly DiagnosticEventLevel _minLevel;
        private readonly DiagnosticEventLevel _maxLevel;

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEventLevelSpecification"/> with the specified
        /// range of <see cref="DiagnosticEventLevel"/>s
        /// </summary>
        /// <param name="minLevel">The minimum level required to satisfy the specification</param>
        /// <param name="maxLevel">The maximum level allowed to satisfy the specification</param>
        public DiagnosticEventLevelSpecification(DiagnosticEventLevel minLevel, DiagnosticEventLevel maxLevel)
        {
            _minLevel = minLevel;
            _maxLevel = maxLevel;
        }

        /// <inheritdoc />
        public bool IsSatisfiedBy(DiagnosticEvent @event)
        {
            if (@event == null) return false;
            var level = @event.Type.Level;
            return level >= _minLevel && level <= _maxLevel;
        }
    }
}
