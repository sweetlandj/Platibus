namespace Platibus.Security
{
    /// <summary>
    /// Specifies that the current user's default network credentials should be used
    /// </summary>
    /// <remarks>
    /// This type of endpoint credentials implies integrated Windows (NTLM/Kerberos) 
    /// authentication
    /// </remarks>
    public sealed class DefaultCredentials : IEndpointCredentials
    {
        void IEndpointCredentials.Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}