using System.Web;

namespace Platibus.IIS
{
    /// <summary>
    /// Extension methods for working with Platibus components within an HTTP context
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns the Platibus bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Returns the Platibus bus instance for the current HTTP context</returns>
        public static IBus GetBus(this HttpContext context)
        {
            if (context == null) return null;
            return context.Items[HttpContextItemKeys.Bus] as IBus;
        }

        /// <summary>
        /// Returns the Platibus bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Returns the Platibus bus instance for the current HTTP context</returns>
        public static IBus GetBus(this HttpContextBase context)
        {
            if (context == null) return null;
            return context.Items[HttpContextItemKeys.Bus] as IBus;
        }

        /// <summary>
        /// Used internally to intialize the bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="bus">The bus instance in scope for the HTTP context</param>
        internal static void SetBus(this HttpContext context, Bus bus)
        {
            context.Items[HttpContextItemKeys.Bus] = bus;
        }
    }
}
