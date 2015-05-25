namespace Platibus
{
    public interface IEndpointCredentials
    {
        void Accept(IEndpointCredentialsVisitor visitor);
    }
}