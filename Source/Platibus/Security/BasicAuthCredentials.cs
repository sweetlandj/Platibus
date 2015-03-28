using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.Security
{
    public class BasicAuthCredentials : IEndpointCredentials
    {
        private readonly string _username;
        private readonly string _password;

        public string Username
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _password; }
        }

        public BasicAuthCredentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public void Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
