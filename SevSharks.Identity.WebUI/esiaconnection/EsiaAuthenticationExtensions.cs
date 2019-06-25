using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace SevSharks.Identity.WebUI.esiaconnection
{
    /// <summary>
    /// Examples on:
    /// https://github.com/DubZero/AspNet.Security.OAuth.Providers
    /// </summary>
    public static class EsiaAuthenticationExtensions
    {
        public static AuthenticationBuilder AddEsia(this AuthenticationBuilder builder)
            => builder.AddEsia("ЕСИА", _ => { });

        public static AuthenticationBuilder AddEsia(this AuthenticationBuilder builder, Action<EsiaAuthenticationOptions> configureOptions)
            => builder.AddEsia("ЕСИА", configureOptions);

        public static AuthenticationBuilder AddEsia(this AuthenticationBuilder builder, string authenticationScheme, Action<EsiaAuthenticationOptions> configureOptions)
            => builder.AddEsia(authenticationScheme, "ЕСИА", configureOptions);

        public static AuthenticationBuilder AddEsia(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<EsiaAuthenticationOptions> configureOptions)
            => builder.AddOAuth<EsiaAuthenticationOptions, EsiaAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}