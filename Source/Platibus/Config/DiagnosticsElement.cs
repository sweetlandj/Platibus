using System.Configuration;

namespace Platibus.Config
{
    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    public class DiagnosticsElement : ConfigurationElement
    {
        private const string SinksPropertName = "sinks";

        /// <summary>
        /// The data sinks in which diagnostic events will be consumed and handled
        /// </summary>
        [ConfigurationProperty(SinksPropertName)]
        [ConfigurationCollection(typeof(EndpointElement),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public DiagnosticEventSinkElementCollection Sinks
        {
            get { return (DiagnosticEventSinkElementCollection)base[SinksPropertName]; }
            set { base[SinksPropertName] = value; }
        }
    }
}
