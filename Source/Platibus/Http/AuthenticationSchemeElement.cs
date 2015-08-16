using System.Configuration;
using System.Net;

namespace Platibus.Http
{
    /// <summary>
    /// A configuration element used to indicate that a particular HTTP authentication
    /// scheme should be supported or enabled on the HTTP server
    /// </summary>
    public class AuthenticationSchemeElement : ConfigurationElement
    {
        private const string SchemePropertyName = "scheme";

        /// <summary>
        /// The supported authentication scheme
        /// </summary>
        /// <seealso cref="AuthenticationSchemes"/>
        [ConfigurationProperty(SchemePropertyName, IsKey = true, IsRequired = true)]
        public AuthenticationSchemes Scheme
        {
            get { return (AuthenticationSchemes) base[SchemePropertyName]; }
            set { base[SchemePropertyName] = value; }
        }
    }
}