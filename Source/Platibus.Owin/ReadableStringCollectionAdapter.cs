using Microsoft.Owin;

namespace Platibus.Owin
{
    internal sealed class ReadableStringCollectionAdapter : System.Collections.Specialized.NameValueCollection
    {
        public ReadableStringCollectionAdapter(IReadableStringCollection query)
        {
            foreach (var entry in query)
            {
                foreach (var value in entry.Value)
                {
                    Add(entry.Key, value);
                }
            }
        }
    }
}