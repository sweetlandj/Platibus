using Microsoft.Owin;

namespace Platibus.Owin
{
    /// <summary>
    /// Extension methods for working with Platibus components within an HTTP context
    /// </summary>
    public static class OwinContextExtensions
    {
        /// <summary>
        /// Returns the Platibus bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Returns the Platibus bus instance for the current HTTP context</returns>
        public static IBus GetBus(this IOwinContext context)
        {
            if (context == null) return null;
            return context.Get<IBus>(OwinContextItemKeys.Bus);
        }

        /// <summary>
        /// Used internally to intialize the bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="bus">The bus instance in scope for the HTTP context</param>
        internal static void SetBus(this IOwinContext context, Bus bus)
        {
            context.Set(OwinContextItemKeys.Bus, bus);
        }
    }
}
