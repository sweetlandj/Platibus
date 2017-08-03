namespace Platibus.Diagnostics
{
    /// <summary>
    /// Criteria used to identify subsets of <see cref="DiagnosticEvent"/>s
    /// </summary>
    public interface IDiagnosticEventSpecification
    {
        /// <summary>
        /// Indicates whether the specified <paramref name="event"/> satisfies the specification
        /// </summary>
        /// <param name="event">The diagnostic event to consider</param>
        /// <returns>Returns <c>true</c> if the specified <paramref name="event"/> satisfies this
        /// specification; <c>false</c> otherwise</returns>
        bool IsSatisfiedBy(DiagnosticEvent @event);
    }
}
