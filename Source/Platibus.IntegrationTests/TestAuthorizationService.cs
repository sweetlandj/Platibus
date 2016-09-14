using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IntegrationTests
{
    internal class TestAuthorizationService : BasicAuthorizationService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly bool _canSend;
        private readonly bool _canSubscribe;

        public TestAuthorizationService(string username, string password, bool canSend, bool canSubscribe)
        {
            _username = username;
            _password = password;
            _canSend = canSend;
            _canSubscribe = canSubscribe;
        }

        protected override Task<bool> Authenticate(string username, string password)
        {
            var authenticated = string.Equals(username, _username, StringComparison.OrdinalIgnoreCase)
                   && Equals(password, _password);

            return Task.FromResult(authenticated);
        }

        public override async Task<bool> IsAuthorizedToSendMessages(IPrincipal principal)
        {
            return await base.IsAuthorizedToSendMessages(principal) && _canSend;
        }

        public override async Task<bool> IsAuthorizedToSubscribe(IPrincipal principal, TopicName topic)
        {
            return await base.IsAuthorizedToSubscribe(principal, topic) && _canSubscribe;
        }
    }
}
