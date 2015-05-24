using System.Configuration;
using System.Net;

namespace Platibus.Http
{
    public class AuthenticationSchemeElement : ConfigurationElement
    {
        private const string SchemePropertyName = "scheme";

        [ConfigurationProperty(SchemePropertyName, IsKey = true, IsRequired = true)]
        public AuthenticationSchemes Scheme
        {
            get { return (AuthenticationSchemes) base[SchemePropertyName]; }
            set { base[SchemePropertyName] = value; }
        }
    }
}
