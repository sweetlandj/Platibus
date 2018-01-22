using Platibus.Diagnostics;
using Platibus.Security;
using System.IO;

namespace Platibus.Filesystem
{
    /// <summary>
    /// Options for configuring a <see cref="FilesystemMessageQueueingService"/>
    /// </summary>
    public class FilesystemMessageQueueingOptions
    {
        /// <summary>
        /// The <see cref="IDiagnosticService"/> through which diagnostic events
        /// related to filesystem queueing will be emitted.
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }

        /// <summary>
        /// The base directory under which queue directories will be created
        /// </summary>
        public DirectoryInfo BaseDirectory { get; set; }

        /// <summary>
        /// The message security token  service to use to issue and validate 
        /// security tokens for persisted messages.
        /// </summary>
        public ISecurityTokenService SecurityTokenService { get; set; }

        /// <summary>
        /// The encryption service used to encrypted persisted message files at rest
        /// </summary>
        public IMessageEncryptionService MessageEncryptionService { get; set; }
    }
}
