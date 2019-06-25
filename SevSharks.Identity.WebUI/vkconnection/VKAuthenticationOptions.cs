using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace SevSharks.Identity.WebUI.vkconnection
{
    /// <summary>
    /// VkAuthenticationOptions
    /// </summary>
    public class VkAuthenticationOptions : OAuthOptions
    {
        /// <summary>
        /// VkAuthenticationOptions
        /// </summary>
        public VkAuthenticationOptions()
        {
            ClaimsIssuer = "VK";

            CallbackPath = new PathString("/signin-vkontakte");

            AuthorizationEndpoint = "https://oauth.vk.com/authorize";
            TokenEndpoint = "https://oauth.vk.com/access_token";
            UserInformationEndpoint = "https://api.vk.com/method/users.get.json";
        }

        /// <summary>
        /// Fields
        /// </summary>
        public ISet<string> Fields { get; } = new HashSet<string>();

        /// <summary>
        /// Api Version for VK. 8.57 is default value
        /// </summary>
        public string ApiVersion { get; set; } = "8.57";
    }
}