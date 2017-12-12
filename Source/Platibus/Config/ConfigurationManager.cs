#if NETSTANDARD2_0

// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Platibus.Config
{
    /// <summary>
    /// Shim to simulate .NET Framework configuration behavior using the 
    /// .NET Standard 2.0 APIs
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly IConfigurationRoot ConfigurationRoot;

        /// <summary>
        /// Static initializer that loads configuration from the <c>appsettings.json</c> file
        /// </summary>
        static ConfigurationManager()
        {
            ConfigurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();
        }

        /// <summary>
        /// The collection of configured connection strings
        /// </summary>
        public static ConnectionStringSettingsCollection ConnectionStrings => 
            new ConnectionStringSettingsCollection(ConfigurationRoot);

        /// <summary>
        /// Returns the configuration with the specified name
        /// </summary>
        /// <param name="sectionName">The name of the configuration section</param>
        /// <returns>Return the requested configuration section or <c>null</c> if a
        /// section with the specified name could not be found</returns>
        public static IConfigurationSection GetSection(string sectionName)
        {
            return ConfigurationRoot.GetSection(sectionName);
        }

        /// <summary>
        /// A collection of connection strings indexed by name
        /// </summary>
        public class ConnectionStringSettingsCollection
        {
            private static readonly object SyncRoot = new object();
            private readonly IDictionary<string, ConnectionStringSettings> _connectionStringSettings = new Dictionary<string, ConnectionStringSettings>();

            /// <summary>
            /// Returns the connection string settings for the specified named
            /// connection
            /// </summary>
            /// <param name="connectionName">The name of the connection</param>
            /// <returns>Returns the connection string settings for the specified named
            /// connection or <c>null</c> if no connection with the specified name
            /// exists</returns>
            public ConnectionStringSettings this[string connectionName]
            {
                get
                {
                    var myConnectionName = connectionName?.Trim().ToLower();
                    lock (SyncRoot)
                    {
                        _connectionStringSettings.TryGetValue(myConnectionName, out var connectionStringSettings);
                        return connectionStringSettings;
                    }
                }
                set
                {
                    var myConnectionName = connectionName?.Trim().ToLower();
                    lock (SyncRoot)
                    {
                        _connectionStringSettings[myConnectionName] = value;
                    }
                }
            }

            public ConnectionStringSettingsCollection(IConfiguration configurationRoot)
            {
                if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));
                var connectionStringsSection = configurationRoot.GetSection("ConnectionStrings");
                if (connectionStringsSection == null) return;

                var connectionStringSettingSections = connectionStringsSection.GetChildren();
                foreach (var connectionStringSettingSection in connectionStringSettingSections)
                {
                    var connectionName = connectionStringSettingSection.Key.Trim().ToLower();
                    var connectionStringSettings = new ConnectionStringSettings
                    {
                        Name = connectionName,
                        ProviderName = connectionStringSettingSection["providerName"] ??
                                       connectionStringSettingSection["provider"],
                        ConnectionString = connectionStringSettingSection["connectionString"] ??
                                           connectionStringSettingSection.Value
                    };
                    _connectionStringSettings[connectionName] = connectionStringSettings;
                }
            }
        }
    }
}

#endif