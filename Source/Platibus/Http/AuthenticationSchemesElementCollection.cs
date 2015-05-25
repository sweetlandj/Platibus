using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace Platibus.Http
{
    public class AuthenticationSchemesElementCollection : ConfigurationElementCollection,
        IEnumerable<AuthenticationSchemeElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AuthenticationSchemeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AuthenticationSchemeElement) element).Scheme;
        }

        IEnumerator<AuthenticationSchemeElement> IEnumerable<AuthenticationSchemeElement>.GetEnumerator()
        {
            return this.OfType<AuthenticationSchemeElement>().GetEnumerator();
        }

        public AuthenticationSchemes GetFlags()
        {
            if (Count == 0) return AuthenticationSchemes.Anonymous;

            var authSchemes = this.Select(e => e.Scheme);
            var flags = authSchemes.Aggregate(default(AuthenticationSchemes),
                (current, scheme) => current | scheme);

            return flags;
        }
    }
}