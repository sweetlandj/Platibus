using System;
using System.Configuration;
using MongoDB.Driver;

namespace Platibus.MongoDB
{
    /// <summary>
    /// Helper class for working with MongoDB 
    /// </summary>
    public static class MongoDBHelper
    {
        /// <summary>
        /// Connects to the MongoDB database specified by the supplied
        /// <paramref name="connectionStringSettings"/> and <paramref name="databaseName"/>
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        /// <param name="databaseName">(Optional) If omitted, the default database specified
        /// in the <see cref="ConnectionStringSettings"/> will be used</param>
        /// <returns>Returns the MongoDB database object</returns>
        public static IMongoDatabase Connect(ConnectionStringSettings connectionStringSettings,
            string databaseName = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var mongoUrl = new MongoUrl(connectionStringSettings.ConnectionString);
            var myDatabaseName = string.IsNullOrWhiteSpace(databaseName)
                ? mongoUrl.DatabaseName
                : databaseName;

            var client = new MongoClient(mongoUrl);
            return client.GetDatabase(myDatabaseName);
        }
    }
}
