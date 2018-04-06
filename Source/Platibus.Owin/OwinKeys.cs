namespace Platibus.Owin
{
    /// <summary>
    /// Lists the keys used to store Platibus components within the OWIN app builder
    /// properties and/or OWIN context
    /// </summary>
    public static class OwinKeys
    {
        /// <summary>
        /// The key under which the bus instance is stored
        /// </summary>
        public const string Bus = "platibus.Bus";
    }
}