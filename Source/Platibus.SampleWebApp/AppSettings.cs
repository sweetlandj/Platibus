using System;
using System.Configuration;

namespace Platibus.SampleWebApp
{
    internal static class AppSettings
    {
        /// <summary>
        /// Whether to explicitly initialize the HTTP module within Global.asax.cs
        /// </summary>
        public const string ExplicitHttpModule = "platibus:RegisterHttpModule";

        /// <summary>
        /// Whether to configure Platibus OWIN middleware in Startup.cs
        /// </summary>
        public const string PlatibusMiddleware = "platibus:ConfigureOwinMiddleware";

        public static bool IsEnabled(this string key, bool enabledByDefault = false)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value)) return enabledByDefault;

            return bool.TryParse(value, out var boolValue) 
                ? boolValue 
                : enabledByDefault;
        }
    }
}