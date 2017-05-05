namespace Platibus.Security
{
    /// <summary>
    /// Credentials for bearer token authentication schemes
    /// </summary>
    public class BearerCredentials : IEndpointCredentials
    {
        private readonly string _credentials;

        public string Credentials { get { return _credentials; } }

        public BearerCredentials(string credentials)
        {
            _credentials = credentials;
        }

        public void Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
