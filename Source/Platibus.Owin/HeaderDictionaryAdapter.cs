using Microsoft.Owin;

namespace Platibus.Owin
{
    internal sealed class HeaderDictionaryAdapter : System.Collections.Specialized.NameValueCollection
    {
        public HeaderDictionaryAdapter(IHeaderDictionary headerDictionary)
        {
            
            if (headerDictionary != null)
            {
                foreach (var entry in headerDictionary)
                {
                    foreach (var value in entry.Value)
                    {
                        Add(entry.Key, value);
                    }
                }
            }
        }
    }
}
