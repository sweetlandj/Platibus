using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace Platibus.Http
{
    public class AuthenticationSchemesElementCollection : ConfigurationElementCollection, IEnumerable<AuthenticationSchemes>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AuthenticationSchemeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AuthenticationSchemeElement) element).Scheme;
        }

        IEnumerator<AuthenticationSchemes> IEnumerable<AuthenticationSchemes>.GetEnumerator()
        {
            return this.OfType<AuthenticationSchemes>().GetEnumerator();
        }

        public AuthenticationSchemes GetFlags()
        {
            if (Count == 0) return AuthenticationSchemes.Anonymous;
            
            var authSchemes = this as IEnumerable<AuthenticationSchemes>;
            return authSchemes.Aggregate(default(AuthenticationSchemes), 
                (current, scheme) => current | scheme);
        }
    }
}
