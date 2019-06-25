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
                new CustomApiResources.SignalR(),
                new CustomApiResources.BiddingSign(),
                new CustomApiResources.EmployeeApiResource()
            };
        }

        /// <summary>
        ///     clients want to access resources (aka scopes)
        /// </summary>
        public static IEnumerable<Client> GetClients()
        {
            const int AccessTokenLifeTime = 7200;
            const int IdentityTokenLifeTime = 7200;
            //TODO Сделать нормальное время жизни токенов после демо
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
                        CustomApiResources.CustomScopes.BiddingSignScopeName
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
                        CustomApiResources.CustomScopes.BiddingSignScopeName
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
                // AddPermissionsAndRoles(context);
                // var createUserService = scope.ServiceProvider.GetRequiredService<CreateUserService>();
                // var busManager = scope.ServiceProvider.GetRequiredService<IBusManager>();
                // AddAllUsers(context, createUserService, busManager);
            }
        }
        /*
        private static void AddAllUsers(Context context, CreateUserService createUserService, IBusManager busManager)
        {
            CreateSeller(context, createUserService, busManager, "seller1@email.com");
            CreateSeller(context, createUserService, busManager, "seller2@email.com");
            CreateBuyer(context, createUserService, busManager, "buyer1@email.com");
            CreateBuyer(context, createUserService, busManager, "buyer2@email.com");
            CreateBuyer(context, createUserService, busManager, "buyer3@email.com");
            context.SaveChanges();
        }

        private static void CreateSeller(Context context, CreateUserService createUserService, IBusManager busManager, string sellerName)
        {
            CreateUser(context, createUserService, busManager, sellerName, true);
        }

        private static void CreateBuyer(Context context, CreateUserService createUserService, IBusManager busManager, string buyerName)
        {
            CreateUser(context, createUserService, busManager, buyerName, false);
        }

        private static void CreateUser(Context context, CreateUserService createUserService, IBusManager busManager, string userName, bool isSeller)
        {
            var fakeBonusAccountInfo = GenerateFakeAccountInfo("BA_");
            var userDto = new CreateUserDto
            {
                UserName = userName,
                EmailConfirmed = true,
                PhoneNumber = "+79780256699",
                PhoneNumberConfirmed = true,
                Password = "Pass123$",
                Email = userName,
                BonusAccountNumber = fakeBonusAccountInfo.number,
                BonusAccountBalance = fakeBonusAccountInfo.balance
            };

            var user = createUserService.CreateUser(userDto).Result;
            if (isSeller && !context.UserExternalLogins.Any(l => l.ExternalUserName == "1000299681"))
            {
                user.ExternalLogins = new List<UserExternalLogin>
                {
                    new UserExternalLogin
                    {
                        Id = Guid.NewGuid(),
                        ExternalSystemName = "ЕСИА",
                        ExternalUserName = "1000299681",
                        User = user,
                        UserId = user.Id
                    }
                };
            }

            CreateOrganizationAndEmployee(busManager, isSeller, user.Id);
        }

        private static void CreateOrganizationAndEmployee(IBusManager busManager, bool isSeller, string userId)
        {
            var request = GetRequest(isSeller, userId);
            var response = busManager.Request<CreateDataForUserRequest, CreateDataForUserResponse>(request).Result;
            if (response == null || !response.IsSuccess)
            {
                throw new Exception($"Error during create organization and employee for user {userId}");
            }
        }

        private static CreateDataForUserRequest GetRequest(bool isSeller, string userId)
        {
            var createDataForUserRequest = new CreateDataForUserRequest
            {
                CreateDataForUser = new CreateDataForUser
                {
                    IsSeller = isSeller,
                    UserId = userId,
                    OrganizationDto = GetOrganizationDto(),
                    EmployeeDto = GetEmployeeDto(),
                    Certificates = new List<CreateCertificateDto>()
                }
            };
           
            return createDataForUserRequest;

            CreateOrganizationDto GetOrganizationDto()
            {
                var fakeVirtualAccountInfo = GenerateFakeAccountInfo("VA_");

                var result = new CreateOrganizationDto
                {
                    Inn = "6449013711",
                    It1OrganizationType = It1OrganizationType.Juridical,
                    Name = GetFakeOrganizationName(),
                    VirtualAccountNumber = fakeVirtualAccountInfo.number,
                    VirtualAccountBalance = fakeVirtualAccountInfo.balance,
                    Email = "my_org5@email.com",
                    Kpp = "644901001",
                    Phone = "+79187854582",
                    Ogrn = "1595239559191",
                    FullAddress = "444333, Российская Федерация, г. Москва, Арбат, дом 1333, 1233, ОКАТО: 45000000000",
                    ExternalId = string.Empty
                };
                return result;
            }

            CreateEmployeeDto GetEmployeeDto()
            {
                var fakeName = GetFakeName();

                var result = new CreateEmployeeDto
                {
                    FirstName = fakeName[0],
                    MiddleName = fakeName[1],
                    LastName = fakeName[2],
                    Position = GetFakePosition(),
                    Snils = "194-456-789 10"
                };
                return result;
            }
        }

        private static string[] GetFakeName()
        {
            string[] firstNames = { "Федор", "Вячеслав", "Михаил", "Андрей", "Степан", "Владимир", "Петр", "Алексей" };
            string[] middleNames = { "Федорович", "Вячеславович", "Михаилович", "Андреевич", "Степанович", "Владимирович", "Петрович", "Алексеевич" };
            string[] lastNames = { "Федоров", "Арбузов", "Михайлов", "Андреев", "Степанов", "Владимиров", "Петров", "Алексеев" };

            var random = new Random();
            return new [] {
                firstNames[random.Next(firstNames.Length)],
                middleNames[random.Next(middleNames.Length)],
                lastNames[random.Next(lastNames.Length)],
            };
        }

        private static string GetFakePosition()
        {
            string[] positions =
            {
                "Главный бухгалтер",
                "Системный администратор",
                "Специалист по рекламе",
                "Специалист по закупкам",
                "Начальник отдела реализации продукции",
                "Начальник отдела производства",
                "Заместитель директора по финансовой части"
            };

            var random = new Random();
            return positions[random.Next(positions.Length)];
        }

        private static string GetFakeOrganizationName()
        {
            string[] organizationNames =
            {
                "ООО \"РосСевСтрой\"",
                "ООО \"Мебель Омска\"",
                "ООО \"СтройГранд\"",
                "ООО \"Avis\"",
                "ООО \"StreamHouse\"",
                "ООО \"МебельСтиль\"",
                "ООО \"Русское море\"",
                "ООО \"Пролив\""
            };

            var random = new Random();
            return organizationNames[random.Next(organizationNames.Length)];
        }
        private static (string number, decimal balance) GenerateFakeAccountInfo(string prefix)
        {
            var bonusAccountNumber = Guid.NewGuid().ToString() + Guid.NewGuid();
            bonusAccountNumber = bonusAccountNumber.Replace("-", "");
            if (bonusAccountNumber.Length > 37)
            {
                bonusAccountNumber = bonusAccountNumber.Substring(0, 37);
            }
            decimal bonusAccountBalance = new Random().Next(1, 100000);
            return (number: string.Concat(prefix, bonusAccountNumber), balance: bonusAccountBalance);
        }
        */
    }
}