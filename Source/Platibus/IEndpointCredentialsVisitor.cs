using Platibus.Security;

namespace Platibus
{
    /// <summary>
    /// An interface describing an object that can visit specific types of
    /// endpoint credentials and map their type-specific details into a
    /// form that is usable by the transport
    /// </summary>
    public interface IEndpointCredentialsVisitor
    {
        /// <summary>
        /// Visits a set of basic authentication credentials
        /// </summary>
        /// <param name="credentials">The set of basic authentication credentials to visit</param>
        void Visit(BasicAuthCredentials credentials);

        /// <summary>
        /// Visits a set of default credentials
        /// </summary>
        /// <param name="credentials">The set of default credentials to visit</param>
        void Visit(DefaultCredentials credentials);
    }
}