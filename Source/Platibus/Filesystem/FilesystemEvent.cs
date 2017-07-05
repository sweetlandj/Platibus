using System;
using Platibus.Diagnostics;

namespace Platibus.Filesystem
{
    /// <summary>
    /// A diagnostic event related to a filesystem operation
    /// </summary>
    public class FilesystemEvent : DiagnosticEvent
    {
        private readonly string _path;

        /// <summary>
        /// The path of the file or directory to which the event pertains
        /// </summary>
        public string Path { get { return _path; } }

        /// <summary>
        /// Initializes a new <see cref="DiagnosticEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="path">The path of the file or directory to which the event pertains</param>
        public FilesystemEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, string endpoint = null, string queue = null, string topic = null, string path = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            _path = path;
        }
    }
}
