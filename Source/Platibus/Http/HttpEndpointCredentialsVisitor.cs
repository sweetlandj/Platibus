using Platibus.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.Http
{
    class HttpEndpointCredentialsVisitor : IEndpointCredentialsVisitor
    {
        private readonly HttpClientHandler _clientHandler;

        public HttpEndpointCredentialsVisitor(HttpClientHandler clientHandler)
        {
            if (clientHandler == null) throw new ArgumentNullException("clientHandler");
            _clientHandler = clientHandler;
        }

        public void Visit(BasicAuthCredentials credentials)
        {
            _clientHandler.Credentials = new NetworkCredential(credentials.Username, credentials.Password);
        }

        public void Visit(DefaultCredentials credentials)
        {
            _clientHandler.UseDefaultCredentials = true;
        }
    }
}
