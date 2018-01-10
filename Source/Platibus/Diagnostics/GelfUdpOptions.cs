using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Options for configuring the <see cref="GelfUdpLoggingSink"/>
    /// </summary>
    public class GelfUdpOptions
    {
        /// <summary>
        /// The maximum chunk size (excluding chunk header)
        /// </summary>
        public const int MaxChunkSize = 8180;

        /// <summary>
        /// Conservative default chunk size
        /// </summary>
        public const int DefaultChunkSize = 1200;

        /// <summary>
        /// The default port for GELF UDP inputs
        /// </summary>
        public const int DefaultPort = 12201;

        /// <summary>
        /// The hostname of the Graylog UDP endpoint
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// (Optional) The port of the Gralog UDP endpoint (12201)
        /// </summary>
        public int Port { get; set; } = DefaultPort;

        /// <summary>
        /// (Optional) Whether to enable compression (true)
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// (Optional) The size (in bytes) at which UDP messages are chunked
        /// </summary>
        public int ChunkSize { get; set; } = DefaultChunkSize;

        /// <summary>
        /// Initializes new <see cref="GelfUdpOptions"/>
        /// </summary>
        /// <param name="host">The hostname of the Graylog UDP endpoint</param>
        public GelfUdpOptions(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            Host = host;
        }
    }
}
