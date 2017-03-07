using System.Configuration;
using System.Net;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for configuring multicast groups
    /// </summary>
    public class MulticastElement : ConfigurationElement
    {
        private const string EnabledPropertyName = "enabled";
        private const string AddressPropertyName = "address";
        private const string PortPropertyName = "port";

        private const string DefaultAddress = "239.255.21.80";
        private const int DefaultPort = 52181;

        /// <summary>
        /// Whether to broadcast subscription changes to members of a multicast group
        /// </summary>
        [ConfigurationProperty(EnabledPropertyName, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)base[EnabledPropertyName]; }
            set { base[EnabledPropertyName] = value; }
        }

        /// <summary>
        /// The IP address of the multicast group to join
        /// </summary>
        [ConfigurationProperty(AddressPropertyName, DefaultValue = DefaultAddress)]
        public IPAddress Address
        {
            get { return (IPAddress)base[AddressPropertyName]; }
            set { base[AddressPropertyName] = value; }
        }

        /// <summary>
        /// The port of the multicast group to join
        /// </summary>
        [ConfigurationProperty(PortPropertyName, DefaultValue = DefaultPort)]
        public int Port
        {
            get { return (int)base[PortPropertyName]; }
            set { base[PortPropertyName] = value; }
        }
    }
}
