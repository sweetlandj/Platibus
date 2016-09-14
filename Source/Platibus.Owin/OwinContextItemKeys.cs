namespace Platibus.Owin
{
    /// <summary>
    /// Lists the keys used to store Platibus components within the OWIN context
    /// </summary>
    public static class OwinContextItemKeys
    {
        /// <summary>
        /// The key under which the bus instance is stored
        /// </summary>
        public const string Bus = "Platibus.Bus";
    }
}
