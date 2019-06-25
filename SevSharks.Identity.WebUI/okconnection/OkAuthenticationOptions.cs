using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace SevSharks.Identity.WebUI.okconnection
{
    /// <summary>
    /// OkAuthenticationOptions
    /// </summary>
    public class OkAuthenticationOptions : OAuthOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OkAuthenticationOptions()
        {
            ClaimsIssuer = "OK";

            CallbackPath = new PathString("/signin-odnoklassniki");

            AuthorizationEndpoint = "https://connect.ok.ru/oauth/authorize";
            TokenEndpoint = "https://api.ok.ru/oauth/token.do";
            UserInformationEndpoint = "https://api.ok.ru/fb.do";
        }

        /// <summary>
        /// Fields
        /// </summary>
        public ISet<string> Fields { get; } = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        public string PublicApplicationKey { get; set; }
    }
}