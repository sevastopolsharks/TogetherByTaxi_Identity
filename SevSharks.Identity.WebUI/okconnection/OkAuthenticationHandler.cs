using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace SevSharks.Identity.WebUI.okconnection
{
    /// <summary>
    /// OkAuthenticationHandler
    /// </summary>
    public class OkAuthenticationHandler : OAuthHandler<OkAuthenticationOptions>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OkAuthenticationHandler(IOptionsMonitor<OkAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        /// <summary>
        /// CreateTicketAsync
        /// </summary>
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            Dictionary<string, string> parametersToAdd = new Dictionary<string, string>
            {
                {"application_key", Options.PublicApplicationKey}
            };
            if (Options.Fields.Count != 0)
            {
                parametersToAdd.Add("fields", string.Join(",", Options.Fields));
            }
            parametersToAdd.Add("format", "json");
            parametersToAdd.Add("method", "users.getCurrentUser");
            parametersToAdd.Add("access_token", tokens.AccessToken);

            var address = string.Empty;
            var allParameters = string.Empty;
            foreach (var kvp in parametersToAdd)
            {
                address = QueryHelpers.AddQueryString(string.IsNullOrEmpty(address) ? Options.UserInformationEndpoint : address, kvp.Key, kvp.Value);
                if (kvp.Key != "access_token")
                {
                    allParameters = allParameters + kvp.Key + "=" + kvp.Value;
                }
            }

            var sessionSecretKey = CreateMd5(tokens.AccessToken + Options.ClientSecret);
            allParameters += sessionSecretKey;
            var sig = CreateMd5(allParameters);
            address = QueryHelpers.AddQueryString(address, "sig", sig);

            var response = await Backchannel.GetAsync(address, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("An error occurred while retrieving the user profile: the remote server " +
                                "returned a {Status} response with the following payload: {Headers} {Body}.",
                                /* Status: */ response.StatusCode,
                                /* Headers: */ response.Headers.ToString(),
                                /* Body: */ await response.Content.ReadAsStringAsync());

                throw new HttpRequestException("An error occurred while retrieving the user profile.");
            }

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

            AddOptionalClaim(identity, ClaimTypes.NameIdentifier, payload.Value<string>("uid"), Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.GivenName, payload.Value<string>("first_name"), Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.Surname, payload.Value<string>("last_name"), Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.Email, payload.Value<string>("email"), Options.ClaimsIssuer);

            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload);

            context.RunClaimActions();

            await Options.Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }

        private void AddOptionalClaim(ClaimsIdentity identity, string type, string value, string issuer)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            // Don't update the identity if the claim cannot be safely added.
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
            {
                return;
            }

            identity.AddClaim(new Claim(type, value, ClaimValueTypes.String, issuer ?? ClaimsIdentity.DefaultIssuer));
        }

        private string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}