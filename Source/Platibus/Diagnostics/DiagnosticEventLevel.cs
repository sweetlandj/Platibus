namespace Platibus.Diagnostics
{
    /// <summary>
    /// Discrete levels of diagnostic events that can assist with handling logic in 
    /// <see cref="IDiagnosticService"/> implementations
    /// </summary>
    public enum DiagnosticEventLevel
    {
        /// <summary>
        /// Routine low-level events used for monitoring or metrics gathering
        /// </summary>
        Trace = -1,

        /// <summary>
        /// Routine low-level events exposing intermediate state, branch logic, or other 
        /// information useful for troubleshooting unexpected conditions or outcomes
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Routine high-level events used to verify correct operation
        /// </summary>
        Info = 1,

        /// <summary>
        /// Unexpected but recoverable conditions that may warrant attention
        /// </summary>
        Warn = 2,

        /// <summary>
        /// Unexpected and unrecoverable conditions that may require review and intervention
        /// </summary>
        Error = 3
    }
}
