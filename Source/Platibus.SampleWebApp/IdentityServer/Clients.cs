using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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