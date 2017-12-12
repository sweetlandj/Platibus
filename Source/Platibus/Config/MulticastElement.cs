#if NET452
using System.ComponentModel;
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


        /// <summary>
        /// Whether to broadcast subscription changes to members of a multicast group
        /// </summary>
        [ConfigurationProperty(EnabledPropertyName, DefaultValue = false)]
        public bool Enabled
        {
            get => (bool)base[EnabledPropertyName];
            set => base[EnabledPropertyName] = value;
        }

        /// <summary>
        /// The IP address of the multicast group to join
        /// </summary>
        [ConfigurationProperty(AddressPropertyName, DefaultValue = MulticastDefaults.Address)]
        [TypeConverter(typeof(IPAddressConverter))]
        public IPAddress Address
        {
            get => (IPAddress)base[AddressPropertyName];
            set => base[AddressPropertyName] = value;
        }

        /// <summary>
        /// The port of the multicast group to join
        /// </summary>
        [ConfigurationProperty(PortPropertyName, DefaultValue = MulticastDefaults.Port)]
        public int Port
        {
            get => (int)base[PortPropertyName];
            set => base[PortPropertyName] = value;
        }
    }
}

#endif