using Platibus.Diagnostics;

namespace Platibus.Filesystem
{
    /// <summary>
    /// Types of <see cref="DiagnosticEvent"/>s specific to filesystem based services
    /// </summary>
    public static class FilesystemEventType
    {
        /// <summary>
        /// Emitted when a message file is created and written to disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileCreated = new DiagnosticEventType("MessageFileCreated", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a message file is deleted from disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileDeleted = new DiagnosticEventType("MessageFileDeleted", DiagnosticEventLevel.Trace);

        /// <summary>
        /// Emitted when a message file cannot be read from disk
        /// </summary>
        public static readonly DiagnosticEventType MessageFileFormatError = new DiagnosticEventType("MessageFileFormatError", DiagnosticEventLevel.Error);

    }
}
