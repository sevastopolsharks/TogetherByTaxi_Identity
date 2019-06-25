using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace SevSharks.Identity.WebUI.vkconnection
{
    /// <summary>
    /// VkAuthenticationHandler
    /// </summary>
    public class VkAuthenticationHandler : OAuthHandler<VkAuthenticationOptions>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VkAuthenticationHandler(IOptionsMonitor<VkAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        /// <summary>
        /// CreateTicketAsync
        /// </summary>
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var address = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken);

            address = QueryHelpers.AddQueryString(address, "v", Options.ApiVersion);

            if (Options.Fields.Count != 0)
            {
                address = QueryHelpers.AddQueryString(address, "fields", string.Join(",", Options.Fields));
            }

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
            var user = (JObject)payload["response"][0];

            foreach (var scope in Options.Scope)
            {
                var scopeValue = tokens.Response.Value<string>(scope);
                if (!string.IsNullOrEmpty(scopeValue))
                {
                    user.Add(scope, scopeValue);
                }
            }

            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, user);

            context.RunClaimActions();

            await Options.Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
    }
}