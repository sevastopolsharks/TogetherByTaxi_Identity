using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace SevSharks.Identity.WebUI.esiaconnection
{
    /// <summary>
    /// OkAuthenticationOptions
    /// </summary>
    public class EsiaAuthenticationOptions : OAuthOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EsiaAuthenticationOptions()
        {
            ClaimsIssuer = "ЕСИА";

            CallbackPath = new PathString("/signin-esa");
            MainUrl = "https://esia-portal1.test.gosuslugi.ru";
            AuthorizationEndpointPostfix = "aas/oauth2/ac";
            TokenEndpointPostfix = "aas/oauth2/te";
            UserInformationEndpointPostfix = "rs/prns/{0}";
            AuthorizationEndpoint = Flurl.Url.Combine(MainUrl, AuthorizationEndpointPostfix);
            TokenEndpoint = Flurl.Url.Combine(MainUrl, TokenEndpointPostfix);
            UserInformationEndpoint = Flurl.Url.Combine(MainUrl, UserInformationEndpointPostfix);
            // AuthorizationEndpoint = "https://esia-portal1.test.gosuslugi.ru/aas/oauth2/ac";
            // TokenEndpoint = "https://esia-portal1.test.gosuslugi.ru/aas/oauth2/te";
            //UserInformationEndpoint = "https://esia-portal1.test.gosuslugi.ru/rs/prns/{0}";

            //openid fullname usr_org
            Scope.Add("openid");
            Scope.Add("fullname");
            Scope.Add("usr_org");
            ClientSecret = "must be generated from code";
        }

        /// <summary>
        /// InformationSystemThumbprint
        /// </summary>
        public string InformationSystemThumbprint { get; set; }

        /// <summary>
        /// MainUrl
        /// </summary>
        public string MainUrl { get; set; }

        /// <summary>
        /// AuthorizationEndpointPostfix
        /// </summary>
        public string AuthorizationEndpointPostfix { get; set; }

        /// <summary>
        /// TokenEndpointPostfix
        /// </summary>
        public string TokenEndpointPostfix { get; set; }

        /// <summary>
        /// UserInformationEndpointPostfix
        /// </summary>
        public string UserInformationEndpointPostfix { get; set; }
    }
}