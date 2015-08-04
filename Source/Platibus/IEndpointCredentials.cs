namespace Platibus
{
    /// <summary>
    /// An interface describing a set of credentials required to connect to an endpoint
    /// </summary>
    public interface IEndpointCredentials
    {
        /// <summary>
        /// Accepts a visitor that transforms the type-specific credential details into
        /// data that can be used by the transport service
        /// </summary>
        /// <param name="visitor">The visitor to accept</param>
        void Accept(IEndpointCredentialsVisitor visitor);
    }
}