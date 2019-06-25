using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace SevSharks.Identity.WebUI.vkconnection
{
    /// <summary>
    /// 
    /// </summary>
    public static class VkAuthenticationExtensions
    {
        /// <summary>
        /// AddVk
        /// </summary>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder)
            => builder.AddVk("VK", _ => { });

        /// <summary>
        /// AddVk
        /// </summary>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, Action<VkAuthenticationOptions> configureOptions)
            => builder.AddVk("VK", configureOptions);

        /// <summary>
        /// AddVk
        /// </summary>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, string authenticationScheme, Action<VkAuthenticationOptions> configureOptions)
            => builder.AddVk(authenticationScheme, "VK", configureOptions);

        /// <summary>
        /// AddVk
        /// </summary>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<VkAuthenticationOptions> configureOptions)
            => builder.AddOAuth<VkAuthenticationOptions, VkAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}