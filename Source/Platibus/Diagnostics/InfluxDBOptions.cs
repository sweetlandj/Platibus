using System;
using System.Collections.Generic;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Parameters for configuring the <see cref="InfluxDBSink"/>
    /// </summary>
    public class InfluxDBOptions
    {
        /// <summary>
        /// The default measurement name ("pb_stats")
        /// </summary>
        public const string DefaultMeasurement = "pb_stats";

        private readonly Uri _uri;
        private readonly string _database;
        
        private IDictionary<string, string> _tags;

        /// <summary>
        /// The URI (scheme, host, and port) of the InfluxDB server
        /// </summary>
        public Uri Uri { get { return _uri; } }

        /// <summary>
        /// The name of the database to target
        /// </summary>
        public string Database { get { return _database; } }

        /// <summary>
        /// (Optional) The measurement name to use
        /// </summary>
        /// <remarks>
        /// The default measurement name is "pb_stats"
        /// </remarks>
        public string Measurement { get; set; }

        /// <summary>
        /// (Optional) The precision for timestamp measurements
        /// </summary>
        /// <remarks>
        /// The default is <see cref="InfluxDBPrecision.Nanosecond"/>
        /// </remarks>
        public InfluxDBPrecision Precision { get; set; }

        /// <summary>
        /// (Optional) The username for authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// (Optional) The password for authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// (Optional) Custom tags for points emitted by the sink
        /// </summary>
        /// <remarks>
        /// <para>If tags are not specified, the following default tags will be included with each
        /// measurement:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Tag Name</term>
        /// <description>Tag Value</description>
        /// </listheader>
        /// <item>
        /// <term>host</term>
        /// <description>The host on which the process is executing 
        /// (<see cref="System.Environment.MachineName"/>)</description>
        /// </item>
        /// <item>
        /// <term>app</term>
        /// <description>The full name of the entry assembly 
        /// (<see cref="System.Reflection.Assembly.GetEntryAssembly()"/>; 
        /// <see cref="System.Reflection.Assembly.GetName()"/>)</description>
        /// </item>
        /// <item>
        /// <term>app_ver</term>
        /// <description>The version number of the entry assembly
        /// (<see cref="System.Reflection.Assembly.GetEntryAssembly()"/>; 
        /// <see cref="System.Reflection.AssemblyName.Version"/>)</description>
        /// </item>
        /// </list>
        /// </remarks>
        public IDictionary<string, string> Tags
        {
            get { return _tags ?? (_tags = new Dictionary<string, string>()); }
            set { _tags = value; }
        }

        /// <summary>
        /// Initializes a new set of <see cref="InfluxDBOptions"/>
        /// </summary>
        /// <param name="uri">The URI of the InfluxDB server</param>
        /// <param name="database">The database to which points will be written</param>
        public InfluxDBOptions(Uri uri, string database)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (string.IsNullOrWhiteSpace(database)) throw new ArgumentNullException("database");
            _uri = uri;
            _database = database.Trim();
        }
    }
}
