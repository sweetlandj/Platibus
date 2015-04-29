using Platibus.Security;

namespace Platibus
{
    public interface IEndpointCredentialsVisitor
    {
        void Visit(BasicAuthCredentials credentials);
        void Visit(DefaultCredentials credentials);
    }
}
