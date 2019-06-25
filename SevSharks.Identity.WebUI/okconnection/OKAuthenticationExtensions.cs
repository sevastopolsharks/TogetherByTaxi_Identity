using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace SevSharks.Identity.WebUI.okconnection
{
    /// <summary>
    /// Examples on:
    /// https://github.com/DubZero/AspNet.Security.OAuth.Providers
    /// </summary>
    public static class OkAuthenticationExtensions
    {
        public static AuthenticationBuilder AddOK(this AuthenticationBuilder builder)
            => builder.AddOK("OK", _ => { });

        public static AuthenticationBuilder AddOK(this AuthenticationBuilder builder, Action<OkAuthenticationOptions> configureOptions)
            => builder.AddOK("OK", configureOptions);

        public static AuthenticationBuilder AddOK(this AuthenticationBuilder builder, string authenticationScheme, Action<OkAuthenticationOptions> configureOptions)
            => builder.AddOK(authenticationScheme, "OK", configureOptions);

        public static AuthenticationBuilder AddOK(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<OkAuthenticationOptions> configureOptions)
            => builder.AddOAuth<OkAuthenticationOptions, OkAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}