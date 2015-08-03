namespace Platibus.Config
{
    /// <summary>
    /// Enumerates the ways in which client credentials can be supplied
    /// </summary>
    public enum ClientCredentialType
    {
        /// <summary>
        /// Client authentication is disabled
        /// </summary>
        None,

        /// <summary>
        /// Basic username and password based authentication
        /// </summary>
        Basic,

        /// <summary>
        /// Built-in Windows client authentication
        /// </summary>
        Windows,

        /// <summary>
        /// NTLM authentication
        /// </summary>
        NTLM
    }
}