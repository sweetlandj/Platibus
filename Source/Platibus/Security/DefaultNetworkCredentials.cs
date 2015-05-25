namespace Platibus.Security
{
    public class DefaultCredentials : IEndpointCredentials
    {
        public void Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}