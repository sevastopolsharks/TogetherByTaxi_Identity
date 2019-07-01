using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using SevSharks.Identity.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SevSharks.Identity.WebUI
{
    /// <summary>
    /// </summary>
    public class SeedData
    {
        /// <summary>
        ///     scopes define the resources in your system
        /// </summary>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new CustomIdentityResources.Roles(),
                new CustomIdentityResources.Permissions()
            };
        }

        /// <summary>
        ///     GetApiResources
        /// </summary>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new CustomApiResources.Gateway(),
                new CustomApiResources.SignalR()
            };
        }

        /// <summary>
        ///     clients want to access resources (aka scopes)
        /// </summary>
        public static IEnumerable<Client> GetClients()
        {
            const int AccessTokenLifeTime = 7200;
            const int IdentityTokenLifeTime = 7200;

            // client credentials client
            return new List<Client>
            {
                // JavaScript Client
                new Client
                {
                    ClientId = Constants.WebSpaClientId,
                    ClientName = Constants.WebSpaClientName,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,

                    RedirectUris = {"http://localhost:4200/login/callback", "http://localhost:4200/assets/silent-renew.html", "http://webspa/login/callback", "http://webspa/assets/silent-renew.html"},
                    PostLogoutRedirectUris = {"http://localhost:4200", "http://webspa"},
                    AllowedCorsOrigins = {"http://localhost:4200", "http://webspa"},

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        CustomIdentityResources.CustomScopes.Roles,
                        CustomIdentityResources.CustomScopes.Permissions,
                        CustomApiResources.CustomScopes.GatewayScopeName,
                        CustomApiResources.CustomScopes.SignalrScopeName,
                    },

                    AccessTokenLifetime = AccessTokenLifeTime,
                    IdentityTokenLifetime = IdentityTokenLifeTime
                },
                // Client for test
                new Client
                {
                    ClientId = Constants.ClientTestId,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    Enabled = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        CustomIdentityResources.CustomScopes.Roles,
                        CustomIdentityResources.CustomScopes.Permissions,
                        CustomApiResources.CustomScopes.GatewayScopeName,
                        CustomApiResources.CustomScopes.SignalrScopeName,
                    },

                    AccessTokenLifetime = AccessTokenLifeTime,
                    IdentityTokenLifetime = IdentityTokenLifeTime
                }
            };
        }

        /// <summary>
        ///     EnsureSeedData
        /// </summary>
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configurationDbContext.Database.Migrate();
                if (!configurationDbContext.Clients.Any())
                {
                    foreach (var client in GetClients())
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());
                    }

                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var resource in GetIdentityResources())
                    {
                        configurationDbContext.IdentityResources.Add(resource.ToEntity());
                    }

                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.ApiResources.Any())
                {
                    foreach (var resource in GetApiResources())
                    {
                        configurationDbContext.ApiResources.Add(resource.ToEntity());
                    }

                    configurationDbContext.SaveChanges();
                }

                var context = scope.ServiceProvider.GetService<Context>();
                context.Database.Migrate();
            }
        }
    }
}