namespace Platibus.Security
{
    /// <summary>
    /// Endpoint crentials consisting of a basic username and password
    /// </summary>
    public class BasicAuthCredentials : IEndpointCredentials
    {
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// The username used to authenticate
        /// </summary>
        public string Username
        {
            get { return _username; }
        }

        /// <summary>
        /// The password used to authenticate
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Initializes a new <see cref="BasicAuthCredentials"/> with the specified
        /// <paramref name="username"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="username">The username used to authenticate</param>
        /// <param name="password">The password used to authenticate</param>
        public BasicAuthCredentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        void IEndpointCredentials.Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}