using System.Web;

namespace Platibus.IIS
{
    /// <summary>
    /// Lists the keys used to store Platibus components within the
    /// <see cref="HttpContext.Items"/> collection
    /// </summary>
    public static class HttpContextItemKeys
    {
        /// <summary>
        /// The key under which the bus instance is stored
        /// </summary>
        public const string Bus = "Platibus.Bus";
    }
}
