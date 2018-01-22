using System.IO;
using Platibus.Diagnostics;
using Platibus.Security;

namespace Platibus.SQLite
{
    /// <summary>
    /// Options that influence the behavior of the <see cref="SQLiteMessageQueueingService"/>
    /// </summary>
    public class SQLiteMessageQueueingOptions
    {
        /// <summary>
        /// The diagnostic service through which diagnostic events related to SQLite
        /// queueing will be raised
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }

        /// <summary>
        /// The directory underneath which the SQLite database files will be created
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
