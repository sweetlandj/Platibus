using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Platibus.Owin;
using Platibus.SampleWebApp;
using Platibus.SampleWebApp.IdentityServer;
using AuthenticationOptions = IdentityServer3.Core.Configuration.AuthenticationOptions;

[assembly: OwinStartup(typeof(Startup))]

namespace Platibus.SampleWebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureIdentityServer3(app);
            app.UsePlatibusMiddleware();
        }

        public void ConfigureIdentityServer3(IAppBuilder app)
        {
            app.Map("/identity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SiteName = "Embedded IdentityServer",
                    SigningCertificate = LoadCertificate(),
                    CspOptions = new CspOptions
                    {
                        Enabled = false
                    },
                    Factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get()),

                    AuthenticationOptions = new AuthenticationOptions
                    {
                        EnablePostSignOutAutoRedirect = true
                    }
                });
            });
            
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                CookieSecure = CookieSecureOption.Never
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44338/identity",
                ClientId = "sample",
                Scope = "openid profile roles api",
                ResponseType = "id_token token",
                RedirectUri = "https://localhost:44338/",
                SignInAsAuthenticationType = "Cookies",
                
                UseTokenLifetime = false,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = OnSecurityTokenValidated,
                    RedirectToIdentityProvider = OnRedirectToIdentityProvider
                }
            });
        }

        private static Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            SetIdTokenHintForLogoutRequest(notification);
            return Task.FromResult(0);
        }

        private static void SetIdTokenHintForLogoutRequest(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            {
                var idTokenHint = notification.OwinContext.Authentication.User.FindFirst("id_token");
                if (idTokenHint != null)
                {
                    notification.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                }
            }
        }

        private static async Task OnSecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            var protocolMessage = notification.ProtocolMessage;
            var idToken = protocolMessage.IdToken;
            var accessToken = protocolMessage.AccessToken;
            var identity = CreateIdentity(notification.AuthenticationTicket);

            await AddUserInfoClaims(identity, notification.Options, accessToken);
            
            // Keep the id_token for logout
            identity.AddClaim(new Claim("id_token", idToken));

            // Add access token for API calls.  The value of this claim will be used as the
            // argument to the BearerCredentials constructor when specifying credentials as part
            // of the SendOptions passed to Bus.Send methods.
            identity.AddClaim(new Claim("access_token", accessToken));

            // Keep track of access token expiration
            int expiresInSeconds;
            if (int.TryParse(protocolMessage.ExpiresIn, out expiresInSeconds))
            {
                var expiresIn = TimeSpan.FromSeconds(expiresInSeconds);
                var expries = DateTimeOffset.UtcNow.Add(expiresIn);
                identity.AddClaim(new Claim("expires", expries.ToString("O")));
            }
            
            notification.AuthenticationTicket = new AuthenticationTicket(identity, notification.AuthenticationTicket.Properties);
        }

        private static ClaimsIdentity CreateIdentity(AuthenticationTicket authenticationTicket)
        {
            var authenticationType = authenticationTicket.Identity.AuthenticationType;
            return new ClaimsIdentity(authenticationType);
        }

        public static async Task AddUserInfoClaims(ClaimsIdentity identity, OpenIdConnectAuthenticationOptions options, string accessToken)
        {
            var userInfoClient = new UserInfoClient(options.Authority + "/connect/userinfo");
            var userInfo = await userInfoClient.GetAsync(accessToken);
            foreach (var claim in userInfo.Claims)
            {
                // JWT specifies claim types like "sub", "iss", "aud", etc. whereas the .NET
                // platform has claim types that are more verbose
                // ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" for example).  To 
                // ensure good interop with Windows/.NET claims and security primitives the
                // JwtSecurityTokenHandler.InboundClaimsMap can be leveraged to map the JWT claims
                // onto their .NET equivalents.
                var inboundClaimType = claim.Type;
                string mappedClaimType;
                if (!JwtSecurityTokenHandler.InboundClaimTypeMap.TryGetValue(inboundClaimType, out mappedClaimType))
                {
                    mappedClaimType = inboundClaimType;
                }
                identity.AddClaim(new Claim(mappedClaimType, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer, claim.Subject));
            }

            // The "sub" claim is (rightly) mapped to the .NET
            // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" claim.
            // However, in order for the Identity.Name property to return a value, the default
            // name type claim, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", must
            // also be specified.
            if (string.IsNullOrWhiteSpace(identity.Name))
            {
                var nameIdentifierClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (nameIdentifierClaim != null)
                {
                    var name = nameIdentifierClaim.Value;
                    var nameClaim = new Claim(identity.NameClaimType, name);
                    identity.AddClaim(nameClaim);
                }
            }
        }

        private static X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\identityServer\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
