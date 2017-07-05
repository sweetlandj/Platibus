using System.IO;
using Platibus.Diagnostics;

namespace Platibus.Filesystem
{
    /// <summary>
    /// Mutable factory object that produces immutable diagnostic events for filesystem operations
    /// </summary>
    public class FilesystemEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The file to which the event pertains
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// The directory to which the event pertains
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Initializes a new <see cref="FilesystemEventBuilder"/>
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public FilesystemEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new FilesystemEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, File, Directory);
        }
    }
}
