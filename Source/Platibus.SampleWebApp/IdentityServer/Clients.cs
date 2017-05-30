using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace Platibus.SampleWebApp.IdentityServer
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    ClientName = "Platibus.SampleWebApp",
                    ClientId = "sample",
                    Flow = Flows.Implicit,
                    AccessTokenLifetime = 86400,
                    RequireConsent = false,
                    RedirectUris = new List<string>
                    {
                        "https://localhost:44359/"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://localhost:44359/"
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "roles",
                        "api"
                    }
                },
                new Client
                {
                    ClientName = "Platibus",
                    ClientId = "platibus",
                    Flow = Flows.ClientCredentials,
                    AccessTokenLifetime = 86400,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        "api"
                    }
                }
            };
        }
    }
}