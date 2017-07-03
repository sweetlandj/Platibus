using System;
using System.IO;
using Platibus.Diagnostics;

namespace Platibus.Filesystem
{
    /// <summary>
    /// A diagnostic event related to a filesystem operation
    /// </summary>
    public class FilesystemEvent : DiagnosticEvent
    {
        private readonly FileInfo _file;
        private readonly DirectoryInfo _directory;
        private readonly string _path;

        /// <summary>
        /// The file to which the event pertains
        /// </summary>
        public FileInfo File { get { return _file; } }

        /// <summary>
        /// The directory to which the event pertains
        /// </summary>
        public DirectoryInfo Directory { get { return _directory; } }

        /// <summary>
        /// The path to which the event pertains
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
        /// <param name="endpointName">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="file">The file to which the event pertains</param>
        /// <param name="directory">The directory to which the event pertains</param>
        /// <param name="path">The path to which the event pertains</param>
        public FilesystemEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpointName = null, QueueName queue = null, TopicName topic = null, FileInfo file = null, DirectoryInfo directory = null, string path = null) 
            : base(source, type, detail, exception, message, endpointName, queue, topic)
        {
            _directory = directory;
            _path = path;
            _file = file;
        }
    }
}
